namespace Ara3D.Geometry;

public class QuadMesh3DBuilder
{
    public List<Point3D> Points { get; } = new();
    public List<Integer4> Faces { get; } = new();
    public QuadMesh3D ToQuadMesh3D() => (Points, Faces);
}