namespace Ara3D.Studio.Samples.Modifiers;

public class Merge : IModifier
{
    public IModel3D Eval(IModel3D m)
    {
        var mesh = m.ToMesh(); 
        var mat = m.FirstOrDefaultMaterial();
        return Model3D.Create(mesh, mat);
    }
}