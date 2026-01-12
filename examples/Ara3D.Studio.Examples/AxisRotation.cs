namespace Ara3D.Studio.Samples;

public class AxisRotation: IModelModifier
{
    [Range(0, 8)] public int RotationAmount;

    public int Degrees => RotationAmount * 45;

    public List<string> AxisNames() => ["X", "Y", "Z"];

    [Options(nameof(AxisNames))] public int Axis;

    public IModel3D Eval(IModel3D model, EvalContext eval)
    {
        var axis = Axis == 0 ? Vector3.UnitX : Axis == 1 ? Vector3.UnitY : Vector3.UnitZ;
        var mat = Matrix4x4.CreateFromAxisAngle(axis, Degrees.Degrees());
        return model.Transform(mat);
    }
}