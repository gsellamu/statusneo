using System.Text.Json;
using Confluent.Kafka;

namespace Spine.Twin.Telemetry;

// Real distributed path: room telemetry flows over Redpanda (Kafka API). The producer lets
// /telemetry/ingest also publish to the bus; the consumer reads the bus into the store.
// Resilient by design — connection errors are caught and retried; startup never blocks.
public static class TelemetryTopic { public const string Name = "room.telemetry"; }

public sealed class RedpandaProducer : IDisposable
{
    private readonly IProducer<Null, string>? _p;
    public bool Enabled => _p is not null;
    public RedpandaProducer(string? bootstrap)
    {
        if (string.IsNullOrWhiteSpace(bootstrap)) return;
        try { _p = new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = bootstrap }).Build(); }
        catch { _p = null; }
    }
    public async Task PublishAsync(Reading r)
    {
        if (_p is null) return;
        try { await _p.ProduceAsync(TelemetryTopic.Name, new Message<Null, string> { Value = JsonSerializer.Serialize(r) }); }
        catch { /* bus down — HTTP path still updated the store */ }
    }
    public void Dispose() => _p?.Dispose();
}

public sealed class RedpandaTelemetryConsumer : BackgroundService
{
    private readonly string? _bootstrap;
    private readonly TelemetryStore _store;
    private readonly ILogger<RedpandaTelemetryConsumer> _log;
    public RedpandaTelemetryConsumer(IConfiguration cfg, TelemetryStore store, ILogger<RedpandaTelemetryConsumer> log)
    { _bootstrap = cfg["Telemetry:Bootstrap"]; _store = store; _log = log; }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_bootstrap)) return Task.CompletedTask;   // consumer disabled
        return Task.Run(() =>
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var c = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
                    {
                        BootstrapServers = _bootstrap, GroupId = "modutecture-telemetry",
                        AutoOffsetReset = AutoOffsetReset.Latest, EnableAutoCommit = true,
                    }).Build();
                    c.Subscribe(TelemetryTopic.Name);
                    _log.LogInformation("[telemetry] consuming {topic} from {bus}", TelemetryTopic.Name, _bootstrap);
                    while (!ct.IsCancellationRequested)
                    {
                        var cr = c.Consume(ct);
                        var r = JsonSerializer.Deserialize<Reading>(cr.Message.Value);
                        if (r is not null) _store.Upsert(r);
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex) { _log.LogWarning("[telemetry] consumer retn in 5s: {m}", ex.Message); Thread.Sleep(5000); }
            }
        }, ct);
    }
}
