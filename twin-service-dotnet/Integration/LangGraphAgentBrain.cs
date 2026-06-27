using System.Text;
using System.Text.Json;
using Spine.Twin.Domain;

namespace Spine.Twin.Integration;

// LangGraph brain — delegates the PROPOSAL step to the external Python agentic graph
// (the four bounded hospital pods + reflective reviewer behind one HTTP endpoint).
// Same IAgentBrain seam as Ollama/Reflective/Deterministic; swappable by config
// (Agent:Mode = langgraph; Agent:LangGraphUrl = http://localhost:8088).
//
// DISCIPLINE PRESERVED: this only PROPOSES. The .NET deterministic gate
// (Validator) still validates the returned placement before anything commits.
// The Python service is a reasoning seam, never a writer of truth.
public sealed class LangGraphAgentBrain : IAgentBrain
{
    private readonly HttpClient _http;
    public LangGraphAgentBrain(HttpClient http, string baseUrl)
    { _http = http; _http.BaseAddress = new Uri(baseUrl); }

    public async Task<BrainProposal?> ProposeAsync(GroundingFacts facts, IReadOnlyList<Instance> current, string goal)
    {
        // Build a grounded request the Python graph can reason over. We send the
        // SAME grounded facts + live instances; the graph returns a placement proposal.
        var req = new
        {
            goal,
            room = new { w = 4000, h = 3000 },
            med_gas_reach_mm = facts.MedGasReachMm,
            headwall_type = facts.HeadwallType,
            rules = facts.Rules.Select(r => new { id = r.Id, text = r.Text, @ref = r.Ref }),
            instances = current.Select(i => new { type_id = i.TypeId, x = i.X, y = i.Y, rotation = i.Rotation }),
        };
        var body = JsonSerializer.Serialize(req);

        JsonElement root;
        try
        {
            using var resp = await _http.PostAsync("/propose",
                new StringContent(body, Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
            var raw = await resp.Content.ReadAsStringAsync();
            root = JsonDocument.Parse(raw).RootElement;
        }
        catch { return null; }   // graph unreachable → caller falls back to deterministic

        // Expected shape: {"proposal":{"typeId":"bed-icu","x":..,"y":..,"rotation":0},"rationale":"..."}
        if (!root.TryGetProperty("proposal", out var p) || p.ValueKind != JsonValueKind.Object)
            return null;
        try
        {
            return new BrainProposal(
                p.TryGetProperty("typeId", out var t) ? t.GetString() ?? "bed-icu" : "bed-icu",
                p.GetProperty("x").GetDouble(),
                p.GetProperty("y").GetDouble(),
                p.TryGetProperty("rotation", out var r) ? r.GetInt32() : 0,
                root.TryGetProperty("rationale", out var ra)
                    ? (ra.GetString() ?? "LangGraph pods proposed; gate validates next.")
                    : "LangGraph pods proposed; gate validates next.");
        }
        catch { return null; }
    }

    public async Task<bool> HealthyAsync()
    {
        try { using var r = await _http.GetAsync("/healthz"); return r.IsSuccessStatusCode; }
        catch { return false; }
    }
}
