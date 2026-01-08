namespace Ara3D.Studio.Samples;

public class TestCube : IModelGenerator
{
    public Action PressMe = () => MessageBox.Show("Thank you!");

    public IModel3D Eval(EvalContext eval)
    {
        var mesh = PlatonicSolids.TriangulatedCube;
        var instance = new InstanceStruct(0, Matrix4x4.Identity, 0, Material.Default);
        return new Model3D([mesh], [instance]);
    }
}