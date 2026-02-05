using Ara3D.IO.StepParser;

namespace Ara3D.IfcGeometry;    

public enum IfcPropKind
{
    SingleValue,
    ListValue,
    EnumeratedValue,
    BoundedValue,
    ReferenceValue,
    TableValue,
    ComplexProperty,
    Quantity,
    Other,
}

public record struct IfcPropSet(
    int Id,
    string Name,
    IReadOnlyList<int> Ids); 

public record struct IfcPropValue(
    int Id,
    string Name,
    string EntityName,
    IfcPropKind Kind,
    StepValue? Value,
    StepValue? Unit = null,
    StepValue? Extra = null);

public record struct IfcObjectToPropSet(
    int ObjectId,
    int PropSetId);

public sealed class IfcPropData
{
    public readonly List<(int Id, Exception Ex)> Errors = [];
    public readonly Dictionary<int, IfcPropValue> PropValues = [];
    public readonly Dictionary<int, IfcPropSet> PropSets = [];
    public readonly List<IfcObjectToPropSet> ObjectToPropSets = [];

    private readonly Dictionary<string, Action<int, StepValue>> _parsers;

    public IfcPropData(StepDocument doc)
    {
        var res = new StepValueResolver(doc);

        _parsers = new Dictionary<string, Action<int, StepValue>>(StringComparer.Ordinal)
        {
            ["IFCPROPERTYSET"] = ParsePropertySet,
            ["IFCRELDEFINESBYPROPERTIES"] = ParseRelDefinesByProperties,

            // IfcProperty*
            ["IFCPROPERTYSINGLEVALUE"] = ParsePropertySingleValue,
            ["IFCPROPERTYLISTVALUE"] = ParsePropertyListValue,
            ["IFCPROPERTYENUMERATEDVALUE"] = ParsePropertyEnumeratedValue,
            ["IFCPROPERTYBOUNDEDVALUE"] = ParsePropertyBoundedValue,
            ["IFCPROPERTYREFERENCEVALUE"] = ParsePropertyReferenceValue,
            ["IFCPROPERTYTABLEVALUE"] = ParsePropertyTableValue,
            ["IFCPROPERTYCOMPLEXPROPERTY"] = ParsePropertyComplexProperty,

            // Quantities (QTO)
            ["IFCELEMENTQUANTITY"] = ParseElementQuantity,
            ["IFCQUANTITYLENGTH"] = (id, v) => ParseQuantity(id, v, "IFCQUANTITYLENGTH"),
            ["IFCQUANTITYAREA"] = (id, v) => ParseQuantity(id, v, "IFCQUANTITYAREA"),
            ["IFCQUANTITYVOLUME"] = (id, v) => ParseQuantity(id, v, "IFCQUANTITYVOLUME"),
            ["IFCQUANTITYCOUNT"] = (id, v) => ParseQuantity(id, v, "IFCQUANTITYCOUNT"),
            ["IFCQUANTITYWEIGHT"] = (id, v) => ParseQuantity(id, v, "IFCQUANTITYWEIGHT"),
            ["IFCQUANTITYTIME"] = (id, v) => ParseQuantity(id, v, "IFCQUANTITYTIME"),
        };

        foreach (var (defId, defVal) in res.GetDefinitionIdsAndValues())
        {
            try
            {
                var entityName = defVal.GetEntityName();
                if (_parsers.TryGetValue(entityName, out var parse))
                    parse(defId, defVal);
            }
            catch (Exception ex)
            {
                Errors.Add((defId, ex));
            }
        }
    }

    // ----------------------------
    // Parsers
    // ----------------------------

    private void ParsePropertySet(int id, StepValue entity)
    {
        var a = new StepAttrReader(entity);

        // IFC2x3/IFC4 typical: Name at 2, HasProperties at 4
        var name = a.StringOrEmpty(2);
        var props = a.IdArrayOrEmpty(4);

        PropSets[id] = new IfcPropSet(id, name, props);
    }

    private void ParseRelDefinesByProperties(int _, StepValue entity)
    {
        var a = new StepAttrReader(entity);

        // RelatedObjects at 4, RelatingPropertyDefinition at 5
        var objectIds = a.IdArrayOrEmpty(4);
        var propSetId = a.IdOrZero(5);
        if (propSetId == 0 || objectIds.Count == 0)
            return;

        foreach (var objectId in objectIds)
            ObjectToPropSets.Add(new IfcObjectToPropSet(objectId, propSetId));
    }

    private void ParsePropertySingleValue(int id, StepValue entity)
    {
        var a = new StepAttrReader(entity);

        // (Name, Description, NominalValue, Unit)
        var name = a.StringOrEmpty(0);
        var value = a.StepOrNull(2);
        var unit = a.StepOrNull(3);

        PropValues[id] = new IfcPropValue(id, name, "IFCPROPERTYSINGLEVALUE", IfcPropKind.SingleValue, value, unit);
    }

    private void ParsePropertyListValue(int id, StepValue entity)
    {
        var a = new StepAttrReader(entity);

        // (Name, Description, ListValues, Unit)
        var name = a.StringOrEmpty(0);
        var list = a.StepOrNull(2); // list StepValue
        var unit = a.StepOrNull(3);

        PropValues[id] = new IfcPropValue(id, name, "IFCPROPERTYLISTVALUE", IfcPropKind.ListValue, list, unit);
    }

    private void ParsePropertyEnumeratedValue(int id, StepValue entity)
    {
        var a = new StepAttrReader(entity);

        // (Name, Description, EnumerationValues, EnumerationReference)
        var name = a.StringOrEmpty(0);
        var values = a.StepOrNull(2); // list StepValue
        var enumRef = a.StepOrNull(3); // often an Id -> IfcPropertyEnumeration

        PropValues[id] = new IfcPropValue(id, name, "IFCPROPERTYENUMERATEDVALUE", IfcPropKind.EnumeratedValue, values, null, enumRef);
    }

    private void ParsePropertyBoundedValue(int id, StepValue entity)
    {
        var a = new StepAttrReader(entity);

        // (Name, Desc, Upper, Lower, Unit, SetPoint)
        var name = a.StringOrEmpty(0);
        var upper = a.StepOrNull(2);
        var lower = a.StepOrNull(3);
        var unit = a.StepOrNull(4);
        var setPt = a.StepOrNull(5);

        // Value=lower, Unit=unit, Extra = (upper or setpoint) is ambiguous; keep both by packing one into Extra
        // Simple: store lower as Value, store unit, store upper as Extra. SetPoint is rare; you can re-read entity if needed.
        _ = setPt;

        PropValues[id] = new IfcPropValue(id, name, "IFCPROPERTYBOUNDEDVALUE", IfcPropKind.BoundedValue, lower, unit, upper);
    }

    private void ParsePropertyReferenceValue(int id, StepValue entity)
    {
        var a = new StepAttrReader(entity);

        // (Name, Desc, UsageName, PropertyReference)
        var name = a.StringOrEmpty(0);
        var reference = a.StepOrNull(3);

        PropValues[id] = new IfcPropValue(id, name, "IFCPROPERTYREFERENCEVALUE", IfcPropKind.ReferenceValue, reference);
    }

    private void ParsePropertyTableValue(int id, StepValue entity)
    {
        var a = new StepAttrReader(entity);

        // (Name, Desc, DefiningValues, DefinedValues, Expression, DefUnit, DefdUnit)
        var name = a.StringOrEmpty(0);
        var defining = a.StepOrNull(2);
        var defined = a.StepOrNull(3);
        var expr = a.StepOrNull(4);
        PropValues[id] = new IfcPropValue(id, name, "IFCPROPERTYTABLEVALUE", IfcPropKind.TableValue, defining, expr, defined);
    }

    private void ParsePropertyComplexProperty(int id, StepValue entity)
    {
        var a = new StepAttrReader(entity);

        // (Name, Desc, UsageName, HasProperties)
        var name = a.StringOrEmpty(0);
        var children = a.StepOrNull(3); // usually list of property ids

        PropValues[id] = new IfcPropValue(id, name, "IFCPROPERTYCOMPLEXPROPERTY", IfcPropKind.ComplexProperty, children);
    }

    private void ParseElementQuantity(int id, StepValue entity)
    {
        var a = new StepAttrReader(entity);

        // (Name, Desc, MethodOfMeasurement, Quantities)
        var name = a.StringOrEmpty(0);
        var qtyIds = a.IdArrayOrEmpty(3);

        // Treat as a property set (very convenient downstream)
        PropSets[id] = new IfcPropSet(id, name, qtyIds);
    }

    private void ParseQuantity(int id, StepValue entity, string entityName)
    {
        var a = new StepAttrReader(entity);

        // Common pattern: (Name, Desc, Unit, Value)
        var name = a.StringOrEmpty(0);
        var value = a.StepOrNull(3);

        PropValues[id] = new IfcPropValue(id, name, entityName, IfcPropKind.Quantity, value);
    }
}