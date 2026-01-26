namespace Ara3D.Geometry;

public class TriangleMesh3DBuilder
{
    public List<Point3D> Points { get; } = new();
    public List<Integer3> Faces { get; } = new();
    public TriangleMesh3D ToTriangleMesh3D() => (Points, Faces);
}