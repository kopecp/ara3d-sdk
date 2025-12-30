namespace Ara3D.Studio.API;

public class EvalContext
{
    public IHostApplication Application { get; }
    public double AnimationTime { get; }
    public EvalObject EvalObject { get; } = new();
    public object? Input => EvalObject.Value;

    public EvalContext(IHostApplication application, double animationTime)
    {
        Application = application;
        AnimationTime = animationTime;
    }
}