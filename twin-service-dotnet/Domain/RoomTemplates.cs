namespace Spine.Twin.Domain;

// Room Moducules: a room is itself a Moducule — a reusable composite of content-Moducules
// plus a layout, stamped into many rooms. This is the literal "lego" reuse and maps to
// Modutecture's "BaseModucules (Get/Create)". Components carry stable local ids so that
// intra-template dependency edges (bed --med_gas--> headwall) resolve on stamp.
public record TemplateComponent(string LocalId, string TypeId, double X, double Y, int Rotation);

public record RoomTemplate(string TemplateId, string Name, string Program, TemplateComponent[] Components)
{
    public string[] TypeIds => Components.Select(c => c.TypeId).Distinct().ToArray();
}

public static class RoomTemplates
{
    // "Standard ICU Room": headwall on the head wall, bed within med-gas reach (earns the edge),
    // clinical sink by the entry. Placed in dependency order: headwall first, then bed, then sink.
    public static readonly IReadOnlyDictionary<string, RoomTemplate> All = new Dictionary<string, RoomTemplate>
    {
        ["std-icu-room"] = new("std-icu-room", "Standard ICU Room", "observation room", new[]
        {
            new TemplateComponent("hw",   "headwall-hw204", 2000,  250, 0),   // head wall: y 100-400, med-gas source
            new TemplateComponent("bed",  "bed-icu",        2000, 1550, 0),   // 2200deep -> y 450-2650 (clears hw, fits 3000); 1300mm from hw <= 2500 reach
            new TemplateComponent("sink", "sink-clinical",   700,  600, 0),   // entry-side, clears bed (bed x 1500-2500)
        }),
        ["std-exam-room"] = new("std-exam-room", "Standard Exam Room", "exam room", new[]
        {
            new TemplateComponent("hw",   "headwall-hw204", 2000,  250, 0),
            new TemplateComponent("bed",  "bed-icu",        2000, 1550, 0),
            new TemplateComponent("sink", "sink-clinical",   700,  600, 0),
        }),
    };

    public static RoomTemplate? Get(string id) => All.TryGetValue(id, out var t) ? t : null;
}
