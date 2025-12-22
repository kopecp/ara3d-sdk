
namespace Ara3D.Studio.API;

public class EvalContext
{
    public IHostApplication Application { get; }
    public double AnimationTime { get; }
    public EvalObject InputObject { get; }

    public EvalContext(IHostApplication application, double animationTime, EvalObject inputObject)
    {
        Application = application;
        AnimationTime = animationTime;
        InputObject = inputObject;
    }
}