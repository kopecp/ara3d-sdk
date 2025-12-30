using System.Collections.Generic;
using Ara3D.Collections;
using Ara3D.Geometry;
using Ara3D.Models;

namespace Ara3D.BimOpenSchema;

public class BimModel3D 
{
    public BimModel3D(BimObjectModel model)
    {
        ObjectModel = model;
        RenderModelData = new RenderModelData(3);
        
    }

    public RenderModelData RenderModelData { get; private set; }
    public BimObjectModel ObjectModel { get; }

    public static BimModel3D Create(BimObjectModel model)
        => new(model);

    public static BimModel3D Create(IBimData data)
        => new(new BimObjectModel(data));
    
    public EntityModel GetEntityModel(InstanceStruct inst)
        => ObjectModel.Entities.ElementAtOrDefault(inst.EntityIndex);
}