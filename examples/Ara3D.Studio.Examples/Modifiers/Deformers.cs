namespace Ara3D.Studio.Samples.Modifiers;

[Category(nameof(Categories.Deformers))]
public class TwistDeformer : IModifier
{
    [Range(-10f, 10f)] public float Revolutions { get; set; }
    [Range(0, 2)] public int Axis = 2;

    public Vector3 AxisVector => Vector3.Zero.WithComponent(Axis, 1);

    public Point3D Deform(Point3D p, Bounds3D bounds)
    {
        var v = p.InverseLerp(bounds);
        var amount = v[Axis];
        var axisAngle = new AxisAngle(AxisVector, amount.Turns * Revolutions);
        return p.Transform(axisAngle);
    }

    public TriangleMesh3D Eval(TriangleMesh3D mesh)
    {
        var bounds = mesh.Bounds;
        return mesh.Deform(p => Deform(p, bounds));
    }
}

[Category(nameof(Categories.Deformers))]
public class SkewDeformer : IModifier
{
    [Range(-5f, 5f)] public float X { get; set; }
    [Range(-5f, 5f)] public float Y { get; set; }
    [Range(-5f, 5f)] public float Z { get; set; }

    [Range(0, 2)] public int Axis = 2;
    public bool Flip;

    public Vector3 MaxTranslation => (X, Y, Z);

    public Point3D Deform(Point3D p, Bounds3D bounds)
    {
        var v = p.InverseLerp(bounds);
        var amount = v[Axis];
        if (Flip) amount = 1f - amount;
        var translation = Vector3.Zero.Lerp(MaxTranslation, amount);
        return p.Translate(translation);
    }

    public TriangleMesh3D Eval(TriangleMesh3D mesh)
    {
        var bounds = mesh.Bounds;
        return mesh.Deform(p => Deform(p, bounds));
    }
}

[Category(nameof(Categories.Deformers))]
public class Spherify : IModifier
{
    [Range(0f, 10f)] public float Radius { get; set; }
    [Range(0f, 1f)] public float Strength { get; set; }

    public Point3D Deform(Point3D p, Bounds3D bounds)
    {
        var center = bounds.Center;
        var v = p - center;
        var dir = v.LengthSquared >= 0.001 ? v.Normalize : Vector3.UnitZ;
        var target = p + dir * Radius;
        return p.Lerp(target, Strength);
    }

    public TriangleMesh3D Eval(TriangleMesh3D mesh)
    {
        var bounds = mesh.Bounds;
        return mesh.Deform(p => Deform(p, bounds));
    }
}

// IDEA:
// Boxify, Clamping, 