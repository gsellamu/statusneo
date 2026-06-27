using Spine.Twin.Data;

namespace Spine.Twin;

// The composition hierarchy: Building → Floor → Room → Moducule instances.
// A change in a room rolls up to its floor and the building, in real time.
// Pure projection over existing per-room twins — does NOT touch the command path.
public static class HierarchyProgram
{
    public record RoomDef(string RoomId, string Name, string Program);
    public record FloorDef(string Name, RoomDef[] Rooms);
    public record BuildingDef(string Name, FloorDef[] Floors);

    // demo building (override via config later; rooms are just twin ids)
    public static readonly BuildingDef Demo = new("St. Mary's Medical Center", new[]
    {
        new FloorDef("Floor 1 — ICU", new[]
        {
            new RoomDef("icu-101", "ICU Room 101", "observation room"),
            new RoomDef("icu-102", "ICU Room 102", "observation room"),
            new RoomDef("icu-103", "ICU Room 103", "observation room"),
        }),
        new FloorDef("Floor 2 — Exam", new[]
        {
            new RoomDef("exam-201", "Exam Room 201", "exam room"),
            new RoomDef("exam-202", "Exam Room 202", "exam room"),
        }),
    });

    // a room's compliance status, computed from its twin (pragmatic v1 rule-of-thumb)
    public static string RoomStatus(TwinDto t)
    {
        if (t.Instances.Count == 0) return "EMPTY";
        var beds = t.Instances.Where(i => i.TypeId == "bed-icu").ToList();
        if (beds.Count == 0) return "IN_PROGRESS";              // equipment but no patient bed yet
        var allBound = beds.All(b => t.Bindings.Any(x => x.From == b.InstanceId && x.Kind == "med_gas"));
        return allBound ? "COMPLIANT" : "AT_RISK";              // a bed without a med-gas source
    }

    // floor/building roll-up = worst-of (AT_RISK dominates; EMPTY is neutral)
    public static string RollUp(IEnumerable<string> childStatuses)
    {
        var s = childStatuses.ToList();
        if (s.Contains("AT_RISK")) return "AT_RISK";
        if (s.Contains("IN_PROGRESS")) return "IN_PROGRESS";
        if (s.Any(x => x == "COMPLIANT")) return "COMPLIANT";
        return "EMPTY";
    }
}
