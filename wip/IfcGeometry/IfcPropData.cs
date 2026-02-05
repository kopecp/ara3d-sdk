using Ara3D.IO.StepParser;

namespace Ara3D.IfcGeometry;

public record struct IfcPropSet(
    int Id, 
    string Name, 
    IReadOnlyList<int> Ids);

public record struct IfcPropValue(
    int Id,
    string Name,
    StepValue Value);

public record struct IfcObjectToPropSet(
    int ObjectId,
    int PropSetId);

public class IfcPropData
{
    public List<(int, Exception)> Errors = [];
    public Dictionary<int, IfcPropValue> PropValues = [];
    public Dictionary<int, IfcPropSet> PropSets = [];
    public List<IfcObjectToPropSet> ObjectToPropSets = [];

    public IfcPropData(StepDocument doc)
    {
        var res = new StepValueResolver(doc);
        
        foreach (var (defId, defVal) in res.GetDefinitionIdsAndValues())
        {
            try
            {
                var name = defVal.GetEntityName();
                if (name is "IFCPROPERTYSET")
                {
                    var attrs = defVal.GetEntityAttributesValue().GetElements().ToList();
                    var propSet = new IfcPropSet(defId, attrs[2].AsString(), attrs[4].AsIdList());
                    PropSets.Add(defId, propSet);
                }
                else if (name is "IFCRELDEFINESBYPROPERTIES")
                {
                    var attrs = defVal.GetEntityAttributesValue().GetElements().ToList();
                    var objectIds = attrs[4].AsIdList();
                    var propSetId = attrs[5].AsId();
                    foreach (var objectId in objectIds)
                    {
                        ObjectToPropSets.Add(new(objectId, propSetId));
                    }
                }
                else if (name is "IFCPROPERTYSINGLEVALUE")
                {
                    var attrs = defVal.GetEntityAttributesValue().GetElements().ToList();
                    var propName = attrs[0].AsString();
                    var propVal = attrs[2];
                    PropValues.Add(defId, new(defId, propName, propVal));
                }
            }
            catch (Exception ex)
            {
                Errors.Add((defId, ex));
            }
        }
    }
}