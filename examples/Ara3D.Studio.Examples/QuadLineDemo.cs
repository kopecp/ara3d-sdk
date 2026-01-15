namespace Ara3D.Studio.Samples;

public class QuadLineDemo : ILineMeshGenerator
{
    [Range(0f, 20f)] public float Scale = 1.0f;
    
    public LineMesh3D Eval(EvalContext context)
    {
        var points = new[]
        {
            new Point3D(0, 0, 0),
            new Point3D(1, 0, 0),
            new Point3D(1, 1, 0),
            new Point3D(0, 1, 0)
        };
        var lineIndices = new[]
        {
            new Integer2(0, 1),
            new Integer2(1, 2),
            new Integer2(2, 3),
            new Integer2(3, 0),
        };
        return new LineMesh3D(points, lineIndices).Scale(Scale);
    }
}