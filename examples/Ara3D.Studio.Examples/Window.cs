using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D.Studio.Samples;

public class Window : IModelGenerator
{
    [Range(0f, 10f)] public float Width = 3f;
    [Range(1, 20)] public int XSegments = 4;
    [Range(0f, 10f)] public float Height = 4f;
    [Range(1, 20)] public int YSegments = 4;
    [Range(0f, 1f)] public float MullionWidth = 0.5f;
    [Range(-10f, 10f)] public float PaneInset = 0.2f;
    
    public static (QuadMesh3D Frame, QuadMesh3D Pane) CreateWindow(Quad3D q, int xSegments, int ySegments, float mullionWidth, float paneInset)
    {
        var grid = q.Subdivide(xSegments, ySegments);
        var bldr = new QuadMesh3DBuilder();;
        bldr.Points.AddRange(grid.Points);

        var pane = new List<Quad3D>();

        foreach (var f in grid.FaceIndices)
        {
            var q1 = grid.Points.GetQuad(f);
            var q2 = q1.Inset(mullionWidth);
            var newFace = bldr.InsertFace(f, q2);
            bldr.DeleteLastFace();
            bldr.ExtrudeFace(newFace, -paneInset);
            pane.Add(bldr.GetLastQuad());
            bldr.DeleteLastFace();
        }

        return (bldr.ToQuadMesh3D(), pane.ToQuadMesh3D());
    }

    public IModel3D Eval(EvalContext context)
    {
        var q = new Quad3D((0, 0, 0), (1, 0, 0), (1, 0, 1), (0, 0, 1));
        q = q.Scale((Width, 1, Height));
        var (frame, pane) = CreateWindow(q, XSegments, YSegments, MullionWidth, PaneInset);
        var meshes = new[] { frame.Triangulate(), pane.Triangulate() };
        var colorFrame = new Color(0.9f, 0.9f, 0.9f, 1f);
        var colorPane = new Color(0, 0, 0.5f, 0.3f);
        var materialFrame = new Material(colorFrame, 1f, 0.1f);
        var materialPane = new Material(colorPane, 1f, 0f);
        var instances = new[]
        {
            new InstanceStruct(0, Matrix4x4.Identity, 0, materialFrame),
            new InstanceStruct(0, Matrix4x4.Identity, 1, materialPane)
        };
        return new Model3D(meshes, instances);
    }
}