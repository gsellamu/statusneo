using System.Text;
using System.Text.Json;
using Spine.Twin.Domain;

namespace Spine.Twin.Integration;

// The brain proposes a placement given ONLY retrieved facts + current instances.
// It never commits, and its output is schema-checked + validated by the gate afterwards.

public record BrainProposal(string TypeId, double X, double Y, int Rotation, string Rationale);

public interface IAgentBrain
{
    Task<BrainProposal?> ProposeAsync(GroundingFacts facts, IReadOnlyList<Instance> current, string goal);
    Task<bool> HealthyAsync();
}

// --- real: Ollama (mistral-nemo), grounded JSON, via plain HttpClient (no SDK) ----
public sealed class OllamaAgentBrain : IAgentBrain
{
    private readonly HttpClient _http;
    private readonly string _model;
    public OllamaAgentBrain(HttpClient http, string baseUrl, string model)
    { _http = http; _http.BaseAddress = new Uri(baseUrl); _model = model; }

    public async Task<BrainProposal?> ProposeAsync(GroundingFacts facts, IReadOnlyList<Instance> current, string goal)
    {
        // ground the prompt: rules + reach + the live headwall(s). No invented facts.
        var headwalls = current.Where(i => i.TypeId == facts.HeadwallType)
            .Select(i => $"{{\"id\":\"{i.InstanceId}\",\"x\":{i.X},\"y\":{i.Y}}}");
        var rules = string.Join("\n", facts.Rules.Select(r => $"- {r.Id}: {r.Text} ({r.Ref})"));
        var prompt =
$@"You are a healthcare space-planning assistant. Room is 4000mm x 3000mm (origin top-left, mm).
Goal: {goal}.
Rules you MUST satisfy:
{rules}
Med-gas reach limit: {facts.MedGasReachMm}mm.
Currently placed med-gas sources (headwalls): [{string.Join(",", headwalls)}].
Propose ONE bed-icu placement that is inside the room, within med-gas reach of a headwall,
and clear of other equipment. Respond with ONLY JSON: {{""typeId"":""bed-icu"",""x"":<int>,""y"":<int>,""rotation"":0,""rationale"":""<short>""}}";

        var body = JsonSerializer.Serialize(new { model = _model, prompt, stream = false, format = "json" });
        using var resp = await _http.PostAsync("/api/generate",
            new StringContent(body, Encoding.UTF8, "application/json"));
        resp.EnsureSuccessStatusCode();
        var raw = await resp.Content.ReadAsStringAsync();
        var inner = JsonDocument.Parse(raw).RootElement.GetProperty("response").GetString() ?? "{}";

        try
        {
            var j = JsonDocument.Parse(inner).RootElement;          // schema-check the LLM output
            return new BrainProposal(
                j.GetProperty("typeId").GetString() ?? "bed-icu",
                j.GetProperty("x").GetDouble(), j.GetProperty("y").GetDouble(),
                j.TryGetProperty("rotation", out var r) ? r.GetInt32() : 0,
                j.TryGetProperty("rationale", out var ra) ? ra.GetString() ?? "" : "");
        }
        catch { return null; }                                     // malformed → caller falls back
    }

    public async Task<bool> HealthyAsync()
    {
        try { using var r = await _http.GetAsync("/api/tags"); return r.IsSuccessStatusCode; }
        catch { return false; }
    }
}

// --- fallback: deterministic (the proven stub) ------------------------------
public sealed class DeterministicAgentBrain : IAgentBrain
{
    public Task<BrainProposal?> ProposeAsync(GroundingFacts facts, IReadOnlyList<Instance> current, string goal)
    {
        var hw = current.FirstOrDefault(i => i.TypeId == facts.HeadwallType);
        if (hw is null) return Task.FromResult<BrainProposal?>(null);
        return Task.FromResult<BrainProposal?>(new BrainProposal(
            "bed-icu", hw.X, hw.Y + 1500, 0,
            $"Bed 1500mm in front of headwall {hw.InstanceId}; within {facts.MedGasReachMm}mm reach, clear of its zone."));
    }
    public Task<bool> HealthyAsync() => Task.FromResult(true);
}
