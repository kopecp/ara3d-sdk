namespace Ara3D.Studio.Samples;

[Category("Path")]
public class ExtrudePath : IModifier
{
    [Range(0f, 10f)] public float Height { get; set; }
    
    //[Range(1, 100)] public int Segments { get; set; }

    public QuadMesh3D Eval(LineMesh3D lineMesh, EvalContext context)
    {
        if (lineMesh.FaceIndices.Count == 0)
            return new QuadMesh3D([], []);

        var points = lineMesh.Points;
        var groups = new List<List<Point3D>>();
        var group = new List<Point3D>();
        groups.Add(group);

        var firstFace = lineMesh.FaceIndices[0];
        group.Add(points[firstFace.A]);
        group.Add(points[firstFace.B]);
        var currentIndex = firstFace.B;

        for (var i = 1; i < lineMesh.FaceIndices.Count; i++)
        {
            var face = lineMesh.FaceIndices[i];
            if (face.A != currentIndex)
            {
                group = new List<Point3D>();
                groups.Add(group);
                group.Add(points[face.A]);
            }
            group.Add(points[face.B]);
            currentIndex = face.B;
        }

        return ToQuadMesh(groups.Map(ToQuadGrid3D));
    }

    public QuadMesh3D ToQuadMesh(IEnumerable<QuadGrid3D> grids)
    {
        var points = new List<Point3D>();
        var faces = new List<Integer4>();
        var offset = 0;

        foreach (var grid in grids)
        {
            points.AddRange(grid.Points);
            foreach (var face in grid.FaceIndices)
            {
                faces.Add(new Integer4(
                    face.A + offset, 
                    face.B + offset, 
                    face.C + offset, 
                    face.D + offset));
            }
            offset = points.Count;
        }

        return new QuadMesh3D(points, faces);
    }

    public QuadGrid3D ToQuadGrid3D(IReadOnlyList<Point3D> points)
        => points.Extrude(Height);
}