using Ara3D.Models;

namespace Ara3D.Studio.API;

public class ModelAsset : IModelAsset
{
    public IModel3D Model { get; private set; }

    public ModelAsset(IModel3D model)
        => Model = model;

    public void Update(IModel3D model)
        => Model = model;
}