using Ara3D.Models;

namespace Ara3D.Studio.API;

public static class AssetExtensions
{
    public static RenderableAsset ToRenderableAsset(this IModel3D model)
    {
        var r = new RenderableAsset();
        r.Update(model);
        return r;
    }
}   