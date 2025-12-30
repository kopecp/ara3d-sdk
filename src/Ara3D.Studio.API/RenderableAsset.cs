using Ara3D.Models;

namespace Ara3D.Studio.API;

public class RenderableAsset : IRenderableAsset
{
    public RenderModelData RenderData { get; private set; }

    public RenderableAsset()
        => RenderData = new RenderModelData(3);

    public void Update(IModel3D model)
        => RenderData.Update(model);

    public void Dispose()
    {
        RenderData?.Dispose();
        RenderData = null;
    }

    ~RenderableAsset()
        => Dispose();
}