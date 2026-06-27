namespace Spine.Twin.Domain;

// The cerebellum (validate) + the twin-as-fold. Pure functions, unit-testable.
// Behavioural oracle: oracle-python/backend/domain.py + test_domain.py (15 passing).

public record Instance(string InstanceId, string TypeId, double X, double Y, int Rotation)
{
    public (int w, int d) Dims()
    {
        var m = Registry.Def(TypeId);
        return (Rotation is 90 or 270) ? (m.FootprintD, m.FootprintW) : (m.FootprintW, m.FootprintD);
    }
}

public enum Severity { Error, Warning }
public record Violation(string Rule, Severity Severity, string Message, string[] Refs);
public record Binding(string Kind, string From, string To);
public record Verdict(bool Ok, List<Violation> Violations, List<Binding> Bindings);

public static class Geometry
{
    public const double Eps = 1.0;   // mm: allow envelopes to touch, not overlap

    public static (double x0, double y0, double x1, double y1) Footprint(Instance i)
    {
        var (w, d) = i.Dims();
        return (i.X - w / 2.0, i.Y - d / 2.0, i.X + w / 2.0, i.Y + d / 2.0);
    }

    public static int[] ClearanceRotated(Instance i)
    {
        var c = Registry.Def(i.TypeId).Clearance;     // [l,t,r,b]
        int l = c[0], t = c[1], r = c[2], b = c[3];
        for (int n = 0; n < (i.Rotation / 90) % 4; n++) (l, t, r, b) = (b, l, t, r);
        return new[] { l, t, r, b };
    }

    public static (double, double, double, double) Envelope(Instance i)
    {
        var (x0, y0, x1, y1) = Footprint(i);
        var c = ClearanceRotated(i);
        return (x0 - c[0], y0 - c[1], x1 + c[2], y1 + c[3]);
    }

    public static bool Overlap((double, double, double, double) a, (double, double, double, double) b) =>
        a.Item1 < b.Item3 - Eps && a.Item3 > b.Item1 + Eps &&
        a.Item2 < b.Item4 - Eps && a.Item4 > b.Item2 + Eps;

    public static bool Inside((double, double, double, double) inner, (double, double, double, double) outer) =>
        inner.Item1 >= outer.Item1 - Eps && inner.Item2 >= outer.Item2 - Eps &&
        inner.Item3 <= outer.Item3 + Eps && inner.Item4 <= outer.Item4 + Eps;

    public static double Dist(Instance a, Instance b) => Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
}

public static class Validator
{
    // propose -> VALIDATE. Returns a verdict; writes nothing.
    //   R1 boundary  [ERROR] | R2 collision [ERROR] / clearance [WARNING]
    //   R3 med-gas   [ERROR] (earns a directed edge) | R4 orientation [WARNING]
    public static Verdict Validate(Instance cand, IReadOnlyList<Instance> others,
                                   (double, double, double, double) room)
    {
        var defn = Registry.Def(cand.TypeId);
        var v = new List<Violation>();
        var bindings = new List<Binding>();

        // R1 boundary
        if (!Geometry.Inside(Geometry.Footprint(cand), room))
            v.Add(new("R1-boundary", Severity.Error,
                "Footprint extends outside the room boundary.", new[] { cand.InstanceId }));

        // R2 clash: hard footprint overlap = ERROR; clearance encroachment = WARNING
        var fp = Geometry.Footprint(cand);
        var env = Geometry.Envelope(cand);
        foreach (var o in others)
        {
            if (Geometry.Overlap(fp, Geometry.Footprint(o)))
                v.Add(new("R2-collision", Severity.Error,
                    $"Footprint physically overlaps {o.TypeId} ({o.InstanceId}).",
                    new[] { cand.InstanceId, o.InstanceId }));
            else if (Geometry.Overlap(env, Geometry.Envelope(o)))
                v.Add(new("R2-clearance", Severity.Warning,
                    $"Keep-clear zone encroaches {o.TypeId} ({o.InstanceId}).",
                    new[] { cand.InstanceId, o.InstanceId }));
        }

        // R3 med-gas reach (dependency edge)
        if (defn.Requires(PortKind.MedGas))
        {
            var sources = others.Where(o => Registry.Def(o.TypeId).Provides(PortKind.MedGas)).ToList();
            var inReach = sources.Where(o => Geometry.Dist(cand, o) <= Registry.MedGasReachMm).ToList();
            if (sources.Count == 0)
                v.Add(new("R3-medgas", Severity.Error,
                    "No med-gas source placed in this room.", new[] { cand.InstanceId }));
            else if (inReach.Count == 0)
            {
                var nearest = sources.Min(o => Geometry.Dist(cand, o));
                v.Add(new("R3-medgas", Severity.Error,
                    $"Nearest med-gas source is {nearest:F0}mm away (limit {Registry.MedGasReachMm}mm).",
                    new[] { cand.InstanceId }));
            }
            else
            {
                var src = inReach.OrderBy(o => Geometry.Dist(cand, o)).First();
                bindings.Add(new("med_gas", cand.InstanceId, src.InstanceId));
            }
        }

        // R4 advisory orientation
        if (cand.TypeId == "bed-icu" && cand.Rotation != 0)
            v.Add(new("R4-orientation", Severity.Warning,
                "Bed not facing entry wall (advisory).", new[] { cand.InstanceId }));

        bool ok = !v.Any(x => x.Severity == Severity.Error);
        return new Verdict(ok, v, bindings);
    }
}
