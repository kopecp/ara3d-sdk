namespace Ara3D.Studio.Samples;

public class FacadeMesh : IQuadMeshGenerator
{
    [Range(0f, 10f)] public float Width = 3f;
    [Range(1, 20)] public int WidthSegments = 4;
    [Range(0f, 10f)] public float Height = 4f;
    [Range(1, 20)] public int HeightSegments = 4;
    [Range(0f, 1f)] public float InsetAmount = 0.5f;
    [Range(-10f, 10f)] public float PushDistance = 0.2f;
    public bool DeleteExtrudedFace;
    [Range(0f, 5f)] public float Mullion = 0.2f;

    public float TotalHeight => Height * HeightSegments;
    public float TotalWidth => Width * WidthSegments;

    public static Quad3D InsetQuad(Quad3D q, float amount)
    {
        var mid = q.Center;
        var a = q.A.Lerp(mid, amount);
        var b = q.B.Lerp(mid, amount);
        var c = q.C.Lerp(mid, amount);
        var d = q.D.Lerp(mid, amount);
        return (a, b, c, d);
    }

    public static Quad3D GetQuad(IReadOnlyList<Point3D> points, Integer4 face)
    {
        var a = points[face.A];
        var b = points[face.B];
        var c = points[face.C];
        var d = points[face.D];
        return (a, b, c, d);
    }

    public static Quad3D PushQuad(Quad3D q, float distance)
    {
        return q.Translate(q.Normal * distance);
    }

    public static Integer4 InsertFace(Integer4 f, Quad3D q, List<Point3D> points, List<Integer4> faces)
    {
        var n = points.Count;
        points.AddRange([q.A, q.B, q.C, q.D]);
        var f0 = new Integer4(f.A, f.B, n + 1, n);
        var f1 = new Integer4(f.B, f.C, n + 2, n + 1);
        var f2 = new Integer4(f.C, f.D, n + 3, n + 2);
        var f3 = new Integer4(f.D, f.A, n, n + 3);
        var f4 = new Integer4(n, n + 1, n + 2, n + 3);
        faces.AddRange([f0, f1, f2, f3, f4]);
        return f4;
    }

    public static Integer4 ExtrudeFace(Integer4 f, float amount, List<Point3D> points, List<Integer4> faces)
    {
        var q = new Quad3D(points[f.A], points[f.B], points[f.C], points[f.D]);
        return InsertFace(f, PushQuad(q, amount), points, faces);
    }
    
    public QuadMesh3D Eval(EvalContext context)
    {
        var a = new Point3D(0, 0, 0);
        var b = new Point3D(TotalWidth, 0, 0);

        var points = a.Sample(b, WidthSegments);
        var grid = HeightSegments.MapRange(i => points.Translate(i * Height * Vector3.UnitZ))
            .RowsToArray()
            .ToQuadGrid3D(false, false);

        var newPoints = new List<Point3D>();
        var newFaces = new List<Integer4>();
        newPoints.AddRange(grid.Points);
        foreach (var f in grid.FaceIndices)
        {
            var q = GetQuad(grid.Points, f);
            var q2 = InsetQuad(q, InsetAmount);
            var newFace = InsertFace(f, q2, newPoints, newFaces);
            newFaces.RemoveAt(newFaces.Count - 1);
            ExtrudeFace(newFace, PushDistance, newPoints, newFaces);
            if (DeleteExtrudedFace)
                newFaces.RemoveAt(newFaces.Count - 1);
        }

        return (newPoints, newFaces);
    }
}