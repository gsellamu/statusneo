using Spine.Twin.Domain;

namespace Spine.Twin;

// Self-describing platform manifest: the system tells you its own plug-points, live wiring,
// genome, rules, lenses, event types, and the patterns it implements. Assembled from the
// REAL Registry + the live adapter selection — not a hand-maintained doc.
public static class Capabilities
{
    public static object Build(bool inMemory, string groundingAdapter, string brainMode)
    {
        var ports = new object[]
        {
            new { seam = "Journal (truth)", port = "EventStore",
                  active = inMemory ? "InMemory" : "Postgres", adapters = new[] { "Postgres", "InMemory" },
                  note = "append-only event log; read model is a fold" },
            new { seam = "Grounding (knowledge)", port = "IGroundingStore",
                  active = groundingAdapter, adapters = new[] { "Neo4j", "InMemory", "Qdrant/pgvector (planned)" },
                  note = "retrieves rules/clearances/reach for the agent" },
            new { seam = "Brain (reasoning)", port = "IAgentBrain",
                  active = brainMode, adapters = new[] { "Deterministic", "Ollama (single-shot)", "Reflective (planner→reviewer)", "LangGraph (planned)" },
                  note = "proposes only; output is gate-validated" },
            new { seam = "Event bus (propagation)", port = "IEventBus",
                  active = "In-proc + polling", adapters = new[] { "InProc", "Redpanda (wired)" },
                  note = "publishes twin deltas to subscribers" },
            new { seam = "Renderer (lens)", port = "GraphQL contract",
                  active = "Web lenses (2D/SVG/3D/Data)", adapters = new[] { "2D canvas", "Schematic SVG", "3D Three.js", "Data table", "Unity client", "UE5 client" },
                  note = "renderer owns nothing; many lenses, one twin" },
        };

        var registry = Registry.All.Values.Select(m => new
        {
            typeId = m.TypeId, name = m.Name, version = m.Version,
            footprint = $"{m.FootprintW}×{m.FootprintD}mm",
            clearance = m.Clearance,
            ports = m.Ports.Select(p => $"{p.Name} ({p.Kind}/{p.Role})").ToArray(),
            geometry = m.GeometryRef,
        }).ToArray();

        var rules = new object[]
        {
            new { id = "R1-boundary",  severity = "ERROR",   text = "Footprint must lie within the room boundary.", cites = "internal" },
            new { id = "R2-collision", severity = "ERROR",   text = "No physical footprint overlap.", cites = "internal" },
            new { id = "R2-clearance", severity = "WARNING", text = "Keep-clear / egress zones should stay unobstructed.", cites = "FGI 2.1-3.3" },
            new { id = "R3-medgas",    severity = "ERROR",   text = $"A bed must be within {Registry.MedGasReachMm}mm of a med-gas source.", cites = "FGI 2.1-8.4 / NFPA 99" },
            new { id = "R4-orientation", severity = "WARNING", text = "Beds should face the entry wall where practical.", cites = "design-guidance" },
        };

        var patterns = new object[]
        {
            new { name = "Hexagonal / Ports & Adapters", where = "every seam above", why = "domain core is infra-agnostic; adapters are plug-and-play" },
            new { name = "Event Sourcing + CQRS-lite", where = "EventStore + fold", why = "append-only truth; rebuildable read models; full audit trail" },
            new { name = "Optimistic concurrency (version token)", where = "placeModucule.expectedVersion", why = "stale for the eyes, current for the gate" },
            new { name = "Idempotency keys", where = "command commandId", why = "at-least-once safety; retries commit once" },
            new { name = "Contract/schema versioning", where = "type_version pinning", why = "a later type bump can't silently mutate an approved design" },
            new { name = "Agentic loop (ground→generate→gate→HITL)", where = "agentSuggest", why = "grounded LLM proposes; deterministic gate disposes; human commits" },
            new { name = "Reflection / multi-agent", where = "ReflectiveAgentBrain", why = "planner drafts, reviewer critiques, gate is authoritative" },
            new { name = "Graceful degradation", where = "health-probed fallback", why = "real adapter down → deterministic fallback, never a crash" },
            new { name = "Observability", where = "/health, /metrics, OTel", why = "live per-dependency status; Prometheus → Grafana; traced spans" },
        };

        var events = new[] { "MODUCULE_PLACED", "MODUCULE_REMOVED" };

        return new
        {
            service = "modutecture-twin",
            tagline = "Many renderers, one twin. The gate holds the rules. The brain proposes. The edge carries the bytes.",
            ports, registry, rules, patterns, events,
            scaling = new[]
            {
                new { channel = "Geometry", pattern = "Netflix Open Connect", mechanism = "content-addressed glTF/USD at edge; LOD-as-bitrate" },
                new { channel = "State", pattern = "Meta TAO + feed", mechanism = "edge working-set; instance-push, type-invalidate" },
                new { channel = "AI", pattern = "edge inference", mechanism = "warm model + grounding co-located; never on the frame path" },
            },
        };
    }
}
