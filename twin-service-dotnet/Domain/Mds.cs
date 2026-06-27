namespace Spine.Twin.Domain;

// The genome + contracts. Pure records; no I/O. Mirrors the proven oracle (domain.py).

public enum PortKind { MedGas, Electrical, Data, Plumbing }
public enum PortRole { Provides, Requires }

public record Port(string Name, PortKind Kind, PortRole Role);

public record Mds(
    string TypeId,
    string Name,
    string Version,
    int FootprintW,
    int FootprintD,
    int[] Clearance,                 // [left, top, right, bottom] mm, local frame
    Port[] Ports,
    string GeometryRef)
{
    public bool Provides(PortKind k) => Ports.Any(p => p.Kind == k && p.Role == PortRole.Provides);
    public bool Requires(PortKind k) => Ports.Any(p => p.Kind == k && p.Role == PortRole.Requires);
}

public static class Registry
{
    public const int MedGasReachMm = 2500;

    public static readonly IReadOnlyDictionary<string, Mds> All = new Dictionary<string, Mds>
    {
        ["headwall-hw204"] = new("headwall-hw204", "Headwall HW-204", "2.3.0",
            1800, 300, new[] { 0, 0, 0, 900 },           // access clearance in front
            new[] { new Port("mg-out", PortKind.MedGas, PortRole.Provides),
                    new Port("pwr-out", PortKind.Electrical, PortRole.Provides) },
            "gltf://sha256-hw204"),

        ["bed-icu"] = new("bed-icu", "ICU Bed", "1.4.0",
            1000, 2200, new[] { 600, 0, 600, 0 },         // egress clearance on sides
            new[] { new Port("mg-in", PortKind.MedGas, PortRole.Requires) },
            "gltf://sha256-bedicu"),

        ["sink-clinical"] = new("sink-clinical", "Clinical Sink", "1.0.1",
            600, 600, new[] { 300, 0, 300, 450 },
            new[] { new Port("h2o", PortKind.Plumbing, PortRole.Requires) },
            "gltf://sha256-sink"),
    };

    public static Mds Def(string typeId) => All[typeId];
}
