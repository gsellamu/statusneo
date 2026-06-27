using System.Text;
using System.Text.Json;
using Spine.Twin.Domain;

namespace Spine.Twin.Integration;

// Reflective multi-agent brain (Reflexion-style): a PLANNER drafts a placement, a REVIEWER
// critiques it against the rules and corrects it, then the deterministic gate has the FINAL
// say. Same IAgentBrain seam as the single-shot and deterministic brains — swappable by
// config (Agent:Mode = reflective). LLM self-critique augments, never replaces, the gate.
public sealed class ReflectiveAgentBrain : IAgentBrain
{
    private readonly HttpClient _http;
    private readonly string _model;
    public ReflectiveAgentBrain(HttpClient http, string baseUrl, string model)
    { _http = http; _http.BaseAddress = new Uri(baseUrl); _model = model; }

    private async Task<JsonElement?> AskJsonAsync(string prompt)
    {
        var body = JsonSerializer.Serialize(new { model = _model, prompt, stream = false, format = "json" });
        using var resp = await _http.PostAsync("/api/generate", new StringContent(body, Encoding.UTF8, "application/json"));
        resp.EnsureSuccessStatusCode();
        var raw = await resp.Content.ReadAsStringAsync();
        var inner = JsonDocument.Parse(raw).RootElement.GetProperty("response").GetString() ?? "{}";
        try { return JsonDocument.Parse(inner).RootElement.Clone(); } catch { return null; }
    }

    public async Task<BrainProposal?> ProposeAsync(GroundingFacts facts, IReadOnlyList<Instance> current, string goal)
    {
        var headwalls = current.Where(i => i.TypeId == facts.HeadwallType).Select(i => $"{{\"x\":{i.X},\"y\":{i.Y}}}");
        var others = current.Select(i => $"{{\"type\":\"{i.TypeId}\",\"x\":{i.X},\"y\":{i.Y}}}");
        var rules = string.Join("\n", facts.Rules.Select(r => $"- {r.Id}: {r.Text}"));
        var ctx = $"Room 4000x3000mm, origin top-left, mm.\nRules:\n{rules}\nMed-gas reach: {facts.MedGasReachMm}mm. " +
                  $"Headwalls: [{string.Join(",", headwalls)}]. Other equipment: [{string.Join(",", others)}].";

        // 1) PLANNER agent — draft
        var draft = await AskJsonAsync(
            $"You are the PLANNER agent for healthcare space planning. {ctx}\nGoal: {goal}. " +
            "Propose ONE bed-icu placement. JSON only: {\"x\":<int>,\"y\":<int>,\"rotation\":0,\"rationale\":\"...\"}");
        if (draft is null || !draft.Value.TryGetProperty("x", out _)) return null;
        double x = draft.Value.GetProperty("x").GetDouble(), y = draft.Value.GetProperty("y").GetDouble();
        int rot = draft.Value.TryGetProperty("rotation", out var r0) ? r0.GetInt32() : 0;

        // 2) REVIEWER agent — critique against the rules and correct
        var review = await AskJsonAsync(
            $"You are the REVIEWER agent. A planner proposed bed-icu at x={x}, y={y}, rotation={rot}. {ctx}\n" +
            "Check for: out-of-bounds, collision with equipment, med-gas reach exceeded. If fine, echo it; " +
            "if not, return a CORRECTED placement satisfying all rules. JSON only: " +
            "{\"valid\":<bool>,\"issues\":[\"...\"],\"x\":<int>,\"y\":<int>,\"rotation\":0,\"rationale\":\"...\"}");

        if (review is not null && review.Value.TryGetProperty("x", out var rx))
        {
            x = rx.GetDouble(); y = review.Value.GetProperty("y").GetDouble();
            rot = review.Value.TryGetProperty("rotation", out var r1) ? r1.GetInt32() : rot;
            var issues = review.Value.TryGetProperty("issues", out var iss) && iss.ValueKind == JsonValueKind.Array
                ? string.Join("; ", iss.EnumerateArray().Select(e => e.GetString()).Where(s => !string.IsNullOrWhiteSpace(s)))
                : "";
            var corrected = review.Value.TryGetProperty("valid", out var v) && v.ValueKind == JsonValueKind.False;
            var verdict = corrected ? "Reviewer corrected the plan" : "Reviewer approved the plan";
            return new BrainProposal("bed-icu", x, y, rot,
                $"Planner → Reviewer: {verdict}{(string.IsNullOrEmpty(issues) ? "" : $" ({issues})")}. Deterministic gate validates next.");
        }
        return new BrainProposal("bed-icu", x, y, rot, "Planner proposal (reviewer unavailable). Deterministic gate validates next.");
    }

    public async Task<bool> HealthyAsync()
    { try { using var r = await _http.GetAsync("/api/tags"); return r.IsSuccessStatusCode; } catch { return false; } }
}
