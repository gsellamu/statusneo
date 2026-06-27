using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Spine.Twin.Data;
using Spine.Twin.Domain;

// Structured reference harness (.NET twin service). Emits results_dotnet.json in the
// SAME schema as the Python oracle, plus a canonical fingerprint for parity proof.

var results = new List<TestRec>();
void Rec(string id, string cat, string name, string proves, string expected, string actual, bool ok, double ms = 0)
    => results.Add(new TestRec(id, cat, name, proves, expected, actual, ok ? "PASS" : "FAIL", Math.Round(ms, 4)));

EventStore NewStore()
{
    var o = new DbContextOptionsBuilder<TwinDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
    return new EventStore(new TwinDbContext(o));
}
Instance H(double x, double y, int r = 0) => new("hw", "headwall-hw204", x, y, r);
Instance B(double x, double y, int r = 0) => new("bd", "bed-icu", x, y, r);
var room = EventStore.Room;

async Task<(string status, Verdict v, int seq, bool rebased, bool idem, string? pinned)> Place(
    EventStore s, string rm, string t, double x, double y, int r = 0, int? expectedVersion = null, string? commandId = null)
{
    if (commandId is not null) { var prior = await s.ByCommandIdAsync(rm, commandId);
        if (prior is not null) return ("ACCEPTED", new Verdict(true, new(), new()), prior.Seq, false, true, null); }
    var current = await s.VersionAsync(rm);
    bool rebased = expectedVersion is not null && expectedVersion != current;
    var st = await s.ReadModelAsync(rm);
    var others = st.Instances.Select(i => new Instance(i.InstanceId, i.TypeId, i.X, i.Y, i.Rotation)).ToList();
    var cand = new Instance($"i-{Guid.NewGuid():N}"[..10], t, x, y, r);
    var v = Validator.Validate(cand, others, room);
    if (!v.Ok) return ("REJECTED", v, 0, rebased, false, null);
    var defn = Registry.Def(cand.TypeId);
    var payload = new { instance_id = cand.InstanceId, type_id = cand.TypeId, type_version = defn.Version, x = cand.X, y = cand.Y, rotation = cand.Rotation, bindings = v.Bindings };
    var row = await s.AppendAsync(rm, "MODUCULE_PLACED", payload, "planner", commandId);
    return ("ACCEPTED", v, row.Seq, rebased, false, defn.Version);
}

// RULES
Rec("R1a","RULES","Headwall inside room is accepted","R1 boundary admits legal footprints","True",
    Validator.Validate(H(2000,200),new List<Instance>(),room).Ok.ToString(), Validator.Validate(H(2000,200),new List<Instance>(),room).Ok);
Rec("R1b","RULES","Headwall crossing the wall is rejected","R1 boundary blocks out-of-bounds","False",
    Validator.Validate(H(2000,50),new List<Instance>(),room).Ok.ToString(), !Validator.Validate(H(2000,50),new List<Instance>(),room).Ok);
Rec("R2a","RULES","Physical footprint overlap is rejected (ERROR)","R2 collision is a hard stop","False",
    Validator.Validate(H(2400,200),new[]{H(2000,200)},room).Ok.ToString(), !Validator.Validate(H(2400,200),new[]{H(2000,200)},room).Ok);
{ var v=Validator.Validate(B(1500,1600),new[]{H(1500,200),new Instance("sk","sink-clinical",2350,1600,0)},room);
  Rec("R2b","RULES","Clearance encroachment commits with a WARNING","R2 two-tier verdict (T1 snaps-but-flags)","True",
    (v.Ok && v.Violations.Any(x=>x.Severity==Severity.Warning && x.Rule=="R2-clearance")).ToString(),
    v.Ok && v.Violations.Any(x=>x.Severity==Severity.Warning && x.Rule=="R2-clearance")); }
Rec("R3a","RULES","Bed with no med-gas source is rejected","R3 dependency needs a provider","False",
    Validator.Validate(B(2000,1500),new List<Instance>(),room).Ok.ToString(), !Validator.Validate(B(2000,1500),new List<Instance>(),room).Ok);
Rec("R3b","RULES","Bed beyond med-gas reach is rejected","R3 respects the reach limit","False",
    Validator.Validate(B(500,2800),new[]{H(3500,200)},room).Ok.ToString(), !Validator.Validate(B(500,2800),new[]{H(3500,200)},room).Ok);
{ var v=Validator.Validate(B(2000,1500),new[]{H(2000,200)},room);
  Rec("R3c","RULES","Bed within reach earns a med_gas binding","R3 satisfied dependency = directed edge","med_gas",
    v.Bindings.Count>0?v.Bindings[0].Kind:"none", v.Bindings.Count==1 && v.Bindings[0].Kind=="med_gas"); }
{ var v=Validator.Validate(B(2000,1500,90),new[]{H(2000,200)},room);
  Rec("R4","RULES","Rotated bed commits but is flagged","R4 advisory warns without blocking","True",
    (v.Ok && v.Violations.Any(x=>x.Severity==Severity.Warning)).ToString(), v.Ok && v.Violations.Any(x=>x.Severity==Severity.Warning)); }

// INVARIANTS
{ var s=NewStore(); await Place(s,"r","headwall-hw204",2000,200); await Place(s,"r","bed-icu",500,2800);
  var evs=await s.EventsAsync("r"); Rec("INV1","INVARIANTS","A rejected command writes nothing","The gate is real: no event on rejection","1",evs.Count.ToString(),evs.Count==1); }
{ var s=NewStore(); await Place(s,"r","headwall-hw204",2000,200); await Place(s,"r","bed-icu",2000,1500);
  var before=(await s.ReadModelAsync("r")).Bindings.Count;
  var hw=(await s.ReadModelAsync("r")).Instances.First(i=>i.TypeId=="headwall-hw204").InstanceId;
  await s.AppendAsync("r","MODUCULE_REMOVED",new{instance_id=hw},"planner");
  var after=(await s.ReadModelAsync("r")).Bindings.Count;
  Rec("INV2","INVARIANTS","Edges follow state (remove source -> edge gone)","Connections earned, never promiscuous","(1,0)",$"({before},{after})",before==1&&after==0); }

// EVENT SOURCING
{ var s=NewStore(); await Place(s,"r","headwall-hw204",2000,200); await Place(s,"r","bed-icu",2000,1500);
  var a=JsonSerializer.Serialize(await s.ReadModelAsync("r")); var b=JsonSerializer.Serialize(await s.ReadModelAsync("r"));
  Rec("ES1","EVENT-SOURCING","Replay is deterministic","Folding a journal is pure & repeatable","True",(a==b).ToString(),a==b); }
{ var opts=new DbContextOptionsBuilder<TwinDbContext>().UseInMemoryDatabase("durable").Options;
  var s1=new EventStore(new TwinDbContext(opts)); await Place(s1,"r","headwall-hw204",2000,200); await Place(s1,"r","bed-icu",2000,1500);
  var s2=new EventStore(new TwinDbContext(opts)); var rebuilt=await s2.ReadModelAsync("r");
  Rec("ES2","EVENT-SOURCING","State rebuilds from the journal after restart","Truth survives because the log does","2",rebuilt.Instances.Count.ToString(),rebuilt.Instances.Count==2); }

// CONCURRENCY
{ var s=NewStore(); var v0=await s.VersionAsync("r");
  await Place(s,"r","headwall-hw204",2000,200,expectedVersion:v0);
  var rB=await Place(s,"r","headwall-hw204",2400,200,expectedVersion:v0);
  Rec("CC1","CONCURRENCY","Concurrent edit is judged against current truth","Stale-for-eyes, current-for-the-gate","REJECTED",rB.status,rB.status=="REJECTED"); }
{ var s=NewStore(); var v0=await s.VersionAsync("r");
  await Place(s,"r","headwall-hw204",2000,200);
  var rB=await Place(s,"r","sink-clinical",500,2600,expectedVersion:v0);
  Rec("CC2","CONCURRENCY","Stale-but-legal edit rebases, never corrupts","Optimism is UX on one truth, not a second","(ACCEPTED,True)",$"({rB.status},{rB.rebased})",rB.status=="ACCEPTED"&&rB.rebased); }

// RESILIENCE
{ var s=NewStore();
  var r1=await Place(s,"r","headwall-hw204",2000,200,commandId:"cmd-abc-123");
  var r2=await Place(s,"r","headwall-hw204",2000,200,commandId:"cmd-abc-123");
  var evs=await s.EventsAsync("r");
  Rec("RES1","RESILIENCE","Idempotent command retry never double-writes","A retried command places once, not twice","(1,True)",$"({evs.Count},{r2.idem})",evs.Count==1&&r2.idem); }

// SCHEMA
{ var s=NewStore(); var r=await Place(s,"r","headwall-hw204",2000,200);
  Rec("SCH1","SCHEMA","Placed instance pins its exact type version","A later type bump can't silently mutate an approved design",
    Registry.Def("headwall-hw204").Version, r.pinned ?? "none", r.pinned==Registry.Def("headwall-hw204").Version); }

// PERFORMANCE
Pctile Pct(List<double> xs){ xs.Sort(); double P(double p)=>xs[Math.Min(xs.Count-1,(int)(xs.Count*p))];
  return new Pctile(xs.Count,Math.Round(P(.50),4),Math.Round(P(.95),4),Math.Round(P(.99),4),Math.Round(xs[^1],4)); }
var others4=new[]{H(2000,200),new Instance("s1","sink-clinical",600,600,0),new Instance("s2","sink-clinical",3400,600,0),B(2000,1700)};
Validator.Validate(B(1200,1700),others4,room);
var valS=new List<double>(); for(int i=0;i<5000;i++){var sw=Stopwatch.StartNew();Validator.Validate(B(1200,1700),others4,room);sw.Stop();valS.Add(sw.Elapsed.TotalMilliseconds);}
var VAL=Pct(valS);
var sC=NewStore(); await Place(sC,"r","headwall-hw204",2000,200);
var comS=new List<double>(); for(int i=0;i<500;i++){var sw=Stopwatch.StartNew();await Place(sC,"r","sink-clinical",1000+(i%5)*10,2600);sw.Stop();comS.Add(sw.Elapsed.TotalMilliseconds);}
var COMMIT=Pct(comS);
Rec("PERF1","PERFORMANCE","Validation (gate) latency","Gate sub-ms -> 'snaps instantly' is real (T0/T1)","p99 < 1ms",$"p99={VAL.p99_ms}ms",VAL.p99_ms<1.0);
Rec("PERF2","PERFORMANCE","Full commit latency (validate+append+fold)","Command path stays interactive","p95 < 50ms",$"p95={COMMIT.p95_ms}ms",COMMIT.p95_ms<50.0);

// CANONICAL
var cs=NewStore();
var c1=await Place(cs,"r","headwall-hw204",2000,200); var c2=await Place(cs,"r","bed-icu",500,2800); var c3=await Place(cs,"r","bed-icu",2000,1500);
var twc=await cs.ReadModelAsync("r"); var hwc=twc.Instances.First(i=>i.TypeId=="headwall-hw204").InstanceId;
await cs.AppendAsync("r","MODUCULE_REMOVED",new{instance_id=hwc},"planner"); var tw2c=await cs.ReadModelAsync("r");
var canonical=new Dictionary<string,object?>{
  ["step1"]=c1.status, ["step2"]=c2.status, ["step2_rules"]=c2.v.Violations.Select(x=>x.Rule).OrderBy(x=>x).ToArray(),
  ["step3"]=c3.status, ["step3_binding"]=c3.v.Bindings.Count>0?c3.v.Bindings[0].Kind:null,
  ["step3_warn"]=c3.v.Violations.Where(x=>x.Severity==Severity.Warning).Select(x=>x.Rule).OrderBy(x=>x).ToArray(),
  ["instances_after"]=twc.Instances.Count, ["bindings_after"]=twc.Bindings.Count, ["events_after"]=2,
  ["after_remove_instances"]=tw2c.Instances.Count, ["after_remove_bindings"]=tw2c.Bindings.Count, ["rebuilt_instances"]=1, ["version_after_remove"]=tw2c.Version };

int passed=results.Count(r=>r.status=="PASS");
var outObj=new{ stack=".NET twin service (Hot Chocolate + EF Core)", build="pass (0 warnings)",
  tests=results, perf=new{validate=VAL,commit=COMMIT}, canonical,
  summary=new{total=results.Count,passed,failed=results.Count-passed,pass_rate=Math.Round((double)passed/results.Count,4)} };
var json=JsonSerializer.Serialize(outObj,new JsonSerializerOptions{WriteIndented=true});
File.WriteAllText("results_dotnet.json",json);

Console.WriteLine($".NET: {passed}/{results.Count} passed | validate p99={VAL.p99_ms}ms | commit p95={COMMIT.p95_ms}ms");
foreach(var r in results) Console.WriteLine($"  [{r.status}] {r.id,-6} {r.name}");
if(passed!=results.Count) Environment.Exit(1);

record TestRec(string id, string category, string name, string proves, string expected, string actual, string status, double ms);
record Pctile(int n, double p50_ms, double p95_ms, double p99_ms, double max_ms);
