using System.Collections.Concurrent;

namespace Spine.Twin.Telemetry;

// Operational-twin layer: live per-room telemetry (occupancy/environment). The SAME twin
// that validates designs can carry operational signals — this is the bridge to occupied-room
// monitoring. POC scope: in-memory; fed via HTTP ingest (robust) or a Redpanda consumer (bus).
public record Reading(string Room, int Occupancy, double TempC, int Co2Ppm, long Ts)
{
    public string Status => Occupancy > 0 ? "OCCUPIED" : "VACANT";
    public string Comfort => (TempC is < 20 or > 24 || Co2Ppm > 1000) ? "ATTENTION" : "OK";
}

public sealed class TelemetryStore
{
    private readonly ConcurrentDictionary<string, Reading> _latest = new();
    public void Upsert(Reading r) => _latest[r.Room] = r;
    public IReadOnlyCollection<Reading> All() => _latest.Values.ToList();
    public Reading? Get(string room) => _latest.TryGetValue(room, out var r) ? r : null;
}
