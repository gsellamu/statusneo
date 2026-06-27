using HotChocolate.Subscriptions;
using Spine.Twin.Data;
using Spine.Twin.Domain;

namespace Spine.Twin.GraphQl;

public record ViolationDto(string Rule, string Severity, string Message, string[] Refs);
public record EventDto(int Seq, string Type, string PayloadJson, string Actor, double Ts);
public record CommandResultDto(string Status, List<ViolationDto> Violations, EventDto? Event)
{ public bool Rebased { get; set; } public int Version { get; set; } public bool IdempotentReplay { get; set; } }
public record AgentProposalDto(ProposedPlacement? Proposal, string Rationale, string[] Citations);

public record StampResultDto(string Status, string TemplateId, int Placed, string RoomStatus, List<ViolationDto> Violations)
{ public int Version { get; set; } }
public record ProposedPlacement(string TypeId, double X, double Y, int Rotation);
public record PlaceInput(string TypeId, double X, double Y, int Rotation = 0);

public class Query
{
    public IEnumerable<Mds> Registry() => Domain.Registry.All.Values;
    public int MedGasReachMm() => Domain.Registry.MedGasReachMm;

    // The Room Moducule catalog — reusable composite blocks (Modutecture's "BaseModucules").
    public IEnumerable<RoomTemplateDto> RoomTemplates() =>
        Domain.RoomTemplates.All.Values.Select(t => new RoomTemplateDto(
            t.TemplateId, t.Name, t.Program, t.Components.Length, t.TypeIds));

    public Task<TwinDto> Twin(string room, [Service] EventStore store) => store.ReadModelAsync(room);

    public async Task<List<EventDto>> Events(string room, [Service] EventStore store) =>
        (await store.EventsAsync(room))
        .Select(e => new EventDto(e.Seq, e.Type, e.Payload, e.Actor, e.Ts)).ToList();

    // Building → Floor → Room roll-up, computed live from each room's twin (read-side projection).
    public async Task<BuildingView> Hierarchy([Service] EventStore store)
    {
        var b = HierarchyProgram.Demo;
        // collect type usage across the whole building for the impact view
        var usage = new Dictionary<string, List<string>>();   // typeId -> room names

        var floors = new List<FloorView>();
        foreach (var f in b.Floors)
        {
            var rooms = new List<RoomView>();
            foreach (var r in f.Rooms)
            {
                var twin = await store.ReadModelAsync(r.RoomId);
                var status = HierarchyProgram.RoomStatus(twin);
                var types = twin.Instances.Select(i => i.TypeId).Distinct().ToList();
                foreach (var ty in types) { if (!usage.ContainsKey(ty)) usage[ty] = new(); usage[ty].Add(r.Name); }
                rooms.Add(new RoomView(r.RoomId, r.Name, r.Program, status,
                    twin.Instances.Count, twin.Bindings.Count, twin.Version, types));
            }
            floors.Add(new FloorView(f.Name, HierarchyProgram.RollUp(rooms.Select(x => x.Status)), rooms));
        }

        var impact = Domain.Registry.All.Values.Select(m => new ImpactView(
            m.TypeId, m.Name, m.Version,
            usage.TryGetValue(m.TypeId, out var rs) ? rs : new List<string>())).ToList();

        return new BuildingView(b.Name, HierarchyProgram.RollUp(floors.Select(x => x.Status)), floors, impact);
    }
}

public record RoomView(string RoomId, string Name, string Program, string Status,
                       int Instances, int Bindings, int Version, List<string> Types);
public record FloorView(string Name, string Status, List<RoomView> Rooms);
public record ImpactView(string TypeId, string Name, string Version, List<string> RoomsUsing);
public record BuildingView(string Name, string Status, List<FloorView> Floors, List<ImpactView> Impact);
public record RoomTemplateDto(string TemplateId, string Name, string Program, int ComponentCount, string[] TypeIds);

public class Subscription
{
    // every ACCEPTED command publishes the new twin to subscribers of this room
    [Subscribe]
    [Topic("{room}")]
    public TwinDto OnTwinChanged(string room, [EventMessage] TwinDto twin) => twin;
}
