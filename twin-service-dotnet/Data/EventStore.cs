using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Spine.Twin.Domain;

namespace Spine.Twin.Data;

// CQRS: write side = Append; read side = ReadModel (a fold over events).
public class EventStore
{
    public static readonly (double, double, double, double) Room = (0, 0, 4000, 3000);
    private readonly TwinDbContext _db;
    public EventStore(TwinDbContext db) => _db = db;

    public async Task<EventRow> AppendAsync(string room, string type, object payload, string actor,
                                            string? commandId = null)
    {
        var row = new EventRow
        {
            RoomId = room,
            Type = type,
            Payload = JsonSerializer.Serialize(payload),
            Actor = actor,
            CommandId = commandId,
            Ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0,
        };
        _db.Events.Add(row);
        await _db.SaveChangesAsync();
        return row;
    }

    public Task<List<EventRow>> EventsAsync(string room) =>
        _db.Events.Where(e => e.RoomId == room).OrderBy(e => e.Seq).ToListAsync();

    public async Task<int> VersionAsync(string room) =>
        (await _db.Events.Where(e => e.RoomId == room).Select(e => (int?)e.Seq).MaxAsync()) ?? 0;

    public Task<EventRow?> ByCommandIdAsync(string room, string commandId) =>
        _db.Events.Where(e => e.RoomId == room && e.CommandId == commandId)
                  .OrderBy(e => e.Seq).FirstOrDefaultAsync();

    public async Task<TwinDto> ReadModelAsync(string room)
    {
        var rm = Fold(await EventsAsync(room));
        rm.Version = await VersionAsync(room);          // optimistic-concurrency token
        return rm;
    }

    // ---- the twin is a left-fold over the journal (pure given the rows) ----
    public static TwinDto Fold(List<EventRow> events)
    {
        var instances = new Dictionary<string, Instance>();
        foreach (var e in events)
        {
            var p = JsonDocument.Parse(e.Payload).RootElement;
            if (e.Type == "MODUCULE_PLACED")
            {
                var id = p.GetProperty("instance_id").GetString()!;
                instances[id] = new Instance(id,
                    p.GetProperty("type_id").GetString()!,
                    p.GetProperty("x").GetDouble(),
                    p.GetProperty("y").GetDouble(),
                    p.GetProperty("rotation").GetInt32());
            }
            else if (e.Type == "MODUCULE_REMOVED")
            {
                instances.Remove(p.GetProperty("instance_id").GetString()!);
            }
        }

        // bindings are a FUNCTION of validated state: recompute med-gas edges
        var insts = instances.Values.ToList();
        var bindings = new List<Binding>();
        foreach (var b in insts)
        {
            if (!Registry.Def(b.TypeId).Requires(PortKind.MedGas)) continue;
            var srcs = insts.Where(o => Registry.Def(o.TypeId).Provides(PortKind.MedGas)
                                        && Geometry.Dist(b, o) <= Registry.MedGasReachMm).ToList();
            if (srcs.Count > 0)
            {
                var src = srcs.OrderBy(o => Geometry.Dist(b, o)).First();
                bindings.Add(new Binding("med_gas", b.InstanceId, src.InstanceId));
            }
        }

        var (x0, y0, x1, y1) = Room;
        return new TwinDto(new RoomDto(x0, y0, x1, y1),
            insts.Select(i => new InstanceDto(i.InstanceId, i.TypeId, i.X, i.Y, i.Rotation)).ToList(),
            bindings.Select(b => new BindingDto(b.Kind, b.From, b.To)).ToList());
    }
}

// ---- GraphQL-facing DTOs (the payload contract clients render) ------------
public record RoomDto(double X0, double Y0, double X1, double Y1);
public record InstanceDto(string InstanceId, string TypeId, double X, double Y, int Rotation);
public record BindingDto(string Kind, string From, string To);
public record TwinDto(RoomDto Room, List<InstanceDto> Instances, List<BindingDto> Bindings) { public int Version { get; set; } }
