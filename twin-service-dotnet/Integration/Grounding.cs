using Neo4j.Driver;

namespace Spine.Twin.Integration;

// The Context Spine's grounding layer: retrieves DESIGN KNOWLEDGE (rules, clearances,
// med-gas reach, the headwall type to use) for a room program. NOT live instances —
// those come from the twin (Postgres). Real graph grounds the rules; the twin holds state.

public record GroundingFacts(
    string RoomProgram,
    string HeadwallType,
    int MedGasReachMm,
    IReadOnlyList<RuleFact> Rules);

public record RuleFact(string Id, string Text, string Ref);

public interface IGroundingStore
{
    Task<GroundingFacts> RetrieveAsync(string roomProgram);
    Task<bool> HealthyAsync();
}

// --- real: Neo4j over bolt --------------------------------------------------
public sealed class Neo4jGroundingStore : IGroundingStore, IAsyncDisposable
{
    private readonly IDriver _driver;
    public Neo4jGroundingStore(string uri, string user, string pw)
        => _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, pw));

    public async Task<GroundingFacts> RetrieveAsync(string roomProgram)
    {
        await using var s = _driver.AsyncSession();
        // one query returns the program, its headwall type + med-gas reach, and its rules
        var cursor = await s.RunAsync(@"
            MATCH (p:RoomProgram {id:$id})
            OPTIONAL MATCH (p)-[:USES]->(h:ModuculeType {role:'med_gas_source'})
            OPTIONAL MATCH (p)-[:GOVERNED_BY]->(r:Rule)
            RETURN p.id AS program,
                   coalesce(h.id,'headwall-hw204') AS headwall,
                   coalesce(p.medGasReachMm, 2500) AS reach,
                   collect(DISTINCT {id:r.id, text:r.text, ref:r.ref}) AS rules",
            new { id = roomProgram });

        var rec = await cursor.SingleAsync();
        var rules = rec["rules"].As<List<IDictionary<string, object>>>()
            .Where(d => d["id"] != null)
            .Select(d => new RuleFact((string)d["id"], (string)d["text"], (string)d["ref"]))
            .ToList();
        return new GroundingFacts(rec["program"].As<string>(), rec["headwall"].As<string>(),
                                  rec["reach"].As<int>(), rules);
    }

    public async Task<bool> HealthyAsync()
    {
        try { await using var s = _driver.AsyncSession();
              await (await s.RunAsync("RETURN 1")).ConsumeAsync(); return true; }
        catch { return false; }
    }

    public async ValueTask DisposeAsync() => await _driver.DisposeAsync();
}

// --- fallback: in-memory (mirrors the seed, so the demo runs if Neo4j is down) ---
public sealed class InMemoryGroundingStore : IGroundingStore
{
    public Task<GroundingFacts> RetrieveAsync(string roomProgram) =>
        Task.FromResult(new GroundingFacts(
            roomProgram, "headwall-hw204", 2500, new[]
            {
                new RuleFact("R3-medgas", "Each patient bed must be within 2500mm of a med-gas source.", "FGI 2.1-8.4"),
                new RuleFact("R2-clearance", "Maintain 600mm egress clearance on both long sides of a bed.", "FGI 2.1-3.3"),
                new RuleFact("R1-boundary", "All equipment footprints must lie within the room boundary.", "internal"),
            }));
    public Task<bool> HealthyAsync() => Task.FromResult(true);
}
