using Ara3D.IO.StepParser;

namespace Ara3D.IfcGeometry;

public class IfcClassifier
{
    public List<(int Id, Exception Ex)> Errors = [];

    public static bool IsInstance(string entityName)
        => IfcClassifierHelpers.InstanceEntities.Contains(entityName);

    public static bool IsType(string entityName)
        => IfcClassifierHelpers.TypeEntities.Contains(entityName);

    public readonly Dictionary<int, StepValue> Instances = [];
    public readonly Dictionary<int, StepValue> Types = [];
    public readonly Dictionary<int, int> InstanceToType = [];

    public IfcClassifier(StepDocument doc)
    {
        var res = new StepValueResolver(doc);
        foreach (var (defId, defVal) in res.GetDefinitionIdsAndValues())
        {
            try
            {
                var entityName = defVal.GetEntityName();
                if (IsInstance(entityName))
                    Instances.Add(defId, defVal);
                else if (IsType(entityName))
                    Types.Add(defId, defVal);
                else if (entityName == "IFCRELDEFINESBYTYPE")
                    ParseRelDefinesByType(defVal.GetEntityAttributesValue());
            }
            catch (Exception ex)
            {
                Errors.Add((defId, ex));
            }
        }
    }

    public void ParseRelDefinesByType(StepValue entity)
    {
        // (GlobalId, OwnerHistory, Name, Description, RelatedObjects, RelatedObjects)
        var elements = entity.GetElements().ToList();
        var typeId = elements[5].AsId();
        var instanceIdList = elements[4].AsIdList();
        foreach (var id in instanceIdList)
        {
            InstanceToType[id] = typeId;
        }
    }


}