using Microsoft.EntityFrameworkCore;

namespace Spine.Twin.Data;

// The spine's long-term memory: an append-only event journal in Postgres.
// State is never stored directly — it is derived by replaying this table.

public class EventRow
{
    public int Seq { get; set; }              // identity PK = ordered sequence
    public string RoomId { get; set; } = "";
    public string Type { get; set; } = "";    // MODUCULE_PLACED | MODUCULE_REMOVED
    public string Payload { get; set; } = "";  // JSONB
    public string Actor { get; set; } = "";
    public string? CommandId { get; set; }
    public double Ts { get; set; }
}

public class TwinDbContext : DbContext
{
    public TwinDbContext(DbContextOptions<TwinDbContext> opts) : base(opts) { }
    public DbSet<EventRow> Events => Set<EventRow>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        var e = b.Entity<EventRow>();
        e.ToTable("events");
        e.HasKey(x => x.Seq);
        e.Property(x => x.Seq).ValueGeneratedOnAdd();
        e.Property(x => x.RoomId).HasColumnName("room_id");
        e.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb");
        e.Property(x => x.CommandId).HasColumnName("command_id");
        e.HasIndex(x => x.RoomId);
    }
}
