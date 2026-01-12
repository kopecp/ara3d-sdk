namespace Ara3D.Studio.Samples;

public class SegmentedBox : IQuadMeshGenerator
{
    [Range(1, 100)] public int Segments = 10;

    [Range(0f, 100f)] public float X = 10;
    [Range(0f, 100f)] public float Y = 10;
    [Range(0f, 100f)] public float Z = 10;

    public QuadMesh3D Eval(EvalContext context)
        => Quad2D.Unit.ToLineMesh().Scale((X * Segments, Y * Segments, 0)).Subdivide(Segments).Extrude(Vector3.UnitZ * Z, Segments);
}