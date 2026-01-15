namespace Ara3D.Studio.Samples;

[Category("Path")]
public class ExtrudePath : IModifier
{
    [Range(0f, 10f)] public float Height { get; set; } = 1;
    [Range(1, 100)] public int Count { get; set; } = 1;
    
    public QuadMesh3D Eval(LineMesh3D lineMesh, EvalContext context)
        => lineMesh.Extrude(Vector3.UnitZ, Count);
}