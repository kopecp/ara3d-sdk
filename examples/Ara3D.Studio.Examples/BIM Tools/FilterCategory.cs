using Ara3D.BimOpenSchema;

namespace Ara3D.Studio.Samples.BIM_Tools;

public class FilterCategory : IModifier
{
    //[Options(nameof(CategoryNames))] 
    [Range(0, 80)] public int Category;
    public string CategoryName => CategoryNames?.ElementAtOrDefault(Category, "");
    public List<string> CategoryNames { get; private set; } = [];
    
    private List<StringIndex> _categoryIndices;
    private IModel3D _model3D;

    public void RecomputeCategoryNames()
    {
        if (_model3D is not BimModel3D bim)
        {
            CategoryNames = [];
            return;
        }

        CategoryNames = bim
            .ObjectModel
            .Entities
            .Where(e => e.HasGeometry)
            .Select(e => e.Category)
            .Distinct()
            .ToList();
    }

    public static string GetCategory(BimObjectModel bim, InstanceStruct inst)
        => bim.Entities.ElementAtOrDefault(inst.EntityIndex)?.Category ?? "";

    public IModel3D Eval(IModel3D model3D, EvalContext context)
    {
        if (model3D != _model3D)
        {
            _model3D = model3D;
            RecomputeCategoryNames();
        }

        if (Category < 0 || Category >= CategoryNames.Count)
            return model3D;

        if (model3D is not BimModel3D bim)
            return model3D;

        var entities = bim.ObjectModel.Entities;
        return model3D.Where(inst => entities[inst.EntityIndex].Category == CategoryName);

    }
}