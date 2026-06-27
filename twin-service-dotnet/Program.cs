using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Spine.Twin.Data;
using Spine.Twin.GraphQl;
using Spine.Twin.Integration;
using Spine.Twin.Telemetry;
using Spine.Twin;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// --- bind address: default :5005 (matches the Unity client + studio lens) -----
// Override with --urls, ASPNETCORE_URLS, or the "Urls" config key. Kestrel reads
// the standard "Urls" key automatically; we just set the default if none given.
if (string.IsNullOrWhiteSpace(cfg["urls"]) && string.IsNullOrWhiteSpace(cfg["ASPNETCORE_URLS"]))
    builder.WebHost.UseUrls("http://localhost:5005");

// --- persistence: Postgres (modutecture DB) or --InMemory --------------------
bool inMemory = args.Contains("--InMemory") || cfg.GetValue<bool>("InMemory");
builder.Services.AddDbContext<TwinDbContext>(o =>
{
    if (inMemory) o.UseInMemoryDatabase("spine");
    else o.UseNpgsql(cfg.GetConnectionString("Twin")
        ?? "Host=localhost;Port=5431;Database=modutecture;Username=jeethhypno_user;Password=jeeth2025");
});
builder.Services.AddScoped<EventStore>();

// --- grounding (Neo4j) + brain (Ollama), with deterministic fallback ---------
var neoUri    = cfg["Neo4j:Uri"]    ?? "bolt://localhost:7687";
var neoUser   = cfg["Neo4j:User"]   ?? "neo4j";
var neoPass   = cfg["Neo4j:Pass"]   ?? "jeeth2025";
var ollamaUrl = cfg["Ollama:Url"]   ?? "http://localhost:11434";
var ollamaMdl = cfg["Ollama:Model"] ?? "mistral-nemo";

IGroundingStore grounding = new Neo4jGroundingStore(neoUri, neoUser, neoPass);
if (!await grounding.HealthyAsync())
{
    Console.WriteLine("[startup] Neo4j unreachable - using in-memory grounding fallback.");
    grounding = new InMemoryGroundingStore();
}
builder.Services.AddSingleton(grounding);

builder.Services.AddHttpClient();
var agentMode = cfg["Agent:Mode"] ?? "ollama";          // deterministic | ollama | reflective | langgraph
var langGraphUrl = cfg["Agent:LangGraphUrl"] ?? "http://localhost:8088";
IAgentBrain brain = agentMode.ToLowerInvariant() switch
{
    "deterministic" => new DeterministicAgentBrain(),
    "reflective"    => new ReflectiveAgentBrain(new HttpClient(), ollamaUrl, ollamaMdl),
    "langgraph"     => new LangGraphAgentBrain(new HttpClient(), langGraphUrl),
    _               => new OllamaAgentBrain(new HttpClient(), ollamaUrl, ollamaMdl),
};
if (brain is not DeterministicAgentBrain && !await brain.HealthyAsync())
{
    Console.WriteLine($"[startup] agent brain ({agentMode}) unreachable - using deterministic brain fallback.");
    brain = new DeterministicAgentBrain();
}
var brainModeActive = brain switch
{
    ReflectiveAgentBrain => "Reflective (planner→reviewer)",
    LangGraphAgentBrain  => "LangGraph (4 hospital pods)",
    OllamaAgentBrain     => "Ollama (single-shot)",
    _                    => "Deterministic",
};
builder.Services.AddSingleton(brain);

// --- telemetry (operational twin) — store + optional Redpanda bus -------------
var telemetry = new TelemetryStore();
builder.Services.AddSingleton(telemetry);
var redpandaProducer = new RedpandaProducer(cfg["Telemetry:Bootstrap"]);
builder.Services.AddSingleton(redpandaProducer);
if (!string.IsNullOrWhiteSpace(cfg["Telemetry:Bootstrap"]))
    builder.Services.AddHostedService<RedpandaTelemetryConsumer>();   // bus → store

// --- GraphQL -----------------------------------------------------------------
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>().AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>().AddInMemorySubscriptions();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// --- OpenTelemetry tracing (exports to OTLP if OTEL_EXPORTER_OTLP_ENDPOINT set) ---
builder.Services.AddOpenTelemetry().WithTracing(t => t
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("modutecture-twin"))
    .AddSource("modutecture.spine")
    .AddAspNetCoreInstrumentation()
    .AddOtlpExporter());

var app = builder.Build();

if (!inMemory)                                   // wait for Postgres, then ensure schema
{
    using var s = app.Services.CreateScope();
    var db = s.ServiceProvider.GetRequiredService<TwinDbContext>();
    for (var i = 1; ; i++)
    {
        try { db.Database.EnsureCreated(); break; }
        catch (Exception ex) when (i < 15)
        { Console.WriteLine($"[startup] Postgres not ready ({i}): {ex.Message}"); Thread.Sleep(2000); }
    }
}

app.UseCors();
app.UseRouting();
app.UseHttpMetrics();                            // Prometheus per-request metrics
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();
app.MapGraphQL("/graphql");
app.MapMetrics("/metrics");                      // scraped by your Prometheus -> Grafana

// --- /capabilities: the platform describes its own plug-points & live wiring ---
app.MapGet("/capabilities", () =>
{
    var groundingAdapter = grounding is Neo4jGroundingStore ? "Neo4j" : "InMemory";
    return Results.Json(Capabilities.Build(inMemory, groundingAdapter, brainModeActive));
});

// --- /health: live per-dependency status -------------------------------------
app.MapGet("/health", async (EventStore store) =>
{
    var deps = new Dictionary<string, string>();
    try { await store.VersionAsync("health"); deps["journal"] = inMemory ? "in-memory" : "postgres:up"; }
    catch (Exception e) { deps["journal"] = "down: " + e.Message; }
    deps["grounding"] = grounding is Neo4jGroundingStore && await grounding.HealthyAsync() ? "neo4j:up"
                        : grounding is Neo4jGroundingStore ? "neo4j:degraded" : "in-memory(fallback)";
    deps["brain"] = brain is DeterministicAgentBrain ? "deterministic(fallback)"
                   : await brain.HealthyAsync() ? $"ollama:up [{brainModeActive}]" : "ollama:degraded";
    var ok = deps.Values.All(v => !v.StartsWith("down"));
    return Results.Json(new { status = ok ? "healthy" : "degraded", dependencies = deps },
                        statusCode: ok ? 200 : 503);
});

// --- telemetry endpoints: ingest (HTTP robust path) + read --------------------
app.MapPost("/telemetry/ingest", async (Reading r, TelemetryStore store, RedpandaProducer bus) =>
{
    store.Upsert(r);                 // robust path: store updated immediately
    await bus.PublishAsync(r);       // distributed path: also publish to Redpanda if configured
    return Results.Ok(new { ok = true, room = r.Room, status = r.Status });
});
app.MapGet("/telemetry", (TelemetryStore store) => Results.Json(new
{
    busEnabled = redpandaProducer.Enabled,
    readings = store.All().OrderBy(x => x.Room),
}));

app.Run();
