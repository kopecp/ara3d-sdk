using Ara3D.BimOpenSchema;

namespace Ara3D.IfcGeometry;

public interface IBosWriter
{
    EntityIndex AddEntity(Entity e);
    void AddRelation(EntityRelation r);
    StringIndex AddString(string s);
}

public interface IIfcModel
{
    IEnumerable<IfcObjectRec> Objects();          // IfcProduct/IfcObject instances
    IEnumerable<IfcTypeRec> Types();              // IfcTypeObject
    IEnumerable<IfcRelRec> Relationships();       // relationships you care about
}

public record struct IfcObjectRec(
    int StepId,
    string ClassName,    // "IfcWall", "IfcDoor"
    string? GlobalId,
    string? Name);

public record struct IfcTypeRec(
    int StepId,
    string ClassName,    // "IfcDoorType"
    string? GlobalId,
    string? Name);

public enum IfcRelKind
{
    Aggregates,
    ContainedInSpatialStructure,
    VoidsElement,
    FillsElement,
    AssociatesMaterial,
    ConnectsPorts,
    RelDefinesByType,
    AssignsToGroup,
    SpaceBoundary,
    // ...
}

public record struct IfcRelRec(
    IfcRelKind Kind,
    int RelId,
    int A,               // “from”
    int B,               // “to”
    int? Extra = null);  // optional (e.g., port id, opening id, etc.)

public sealed class IfcToBosMapper
{
    private readonly IIfcModel _ifc;
    private readonly IBosWriter _bos;
    private readonly DocumentIndex _doc;

    private readonly Dictionary<int, EntityIndex> _entityByIfcId = new();
    private readonly Dictionary<string, EntityIndex> _categoryByClass = new(StringComparer.Ordinal);

    public IfcToBosMapper(IIfcModel ifc, IBosWriter bos, DocumentIndex doc)
    {
        _ifc = ifc;
        _bos = bos;
        _doc = doc;
    }

    public void MapAll()
    {
        CreateCategories();
        CreateTypes();
        CreateObjects();
        CreateRelations();
    }

    private void CreateCategories()
    {
        foreach (var o in _ifc.Objects())
            GetOrCreateCategory(o.ClassName);
        foreach (var t in _ifc.Types())
            GetOrCreateCategory(t.ClassName);
    }

    private EntityIndex GetOrCreateCategory(string className)
    {
        if (_categoryByClass.TryGetValue(className, out var idx))
            return idx;

        // Category entities: local id can be 0 or a stable hash; global id empty.
        var e = new Entity(
            LocalId: 0,
            GlobalId: _bos.AddString(""),
            Document: _doc,
            Name: _bos.AddString(className),
            Category: default,   // category-of-category optional; could be a root category
            Type: default);

        idx = _bos.AddEntity(e);
        _categoryByClass[className] = idx;
        return idx;
    }

    private void CreateTypes()
    {
        foreach (var t in _ifc.Types())
        {
            var cat = GetOrCreateCategory(t.ClassName);
            var e = new Entity(
                LocalId: t.StepId,
                GlobalId: _bos.AddString(t.GlobalId ?? ""),
                Document: _doc,
                Name: _bos.AddString(t.Name ?? t.ClassName),
                Category: cat,
                Type: default);

            _entityByIfcId[t.StepId] = _bos.AddEntity(e);
        }
    }

    private void CreateObjects()
    {
        foreach (var o in _ifc.Objects())
        {
            var cat = GetOrCreateCategory(o.ClassName);
            var e = new Entity(
                LocalId: o.StepId,
                GlobalId: _bos.AddString(o.GlobalId ?? ""),
                Document: _doc,
                Name: _bos.AddString(o.Name ?? o.ClassName),
                Category: cat,
                Type: default); // filled later from RelDefinesByType

            _entityByIfcId[o.StepId] = _bos.AddEntity(e);
        }
    }

    private void CreateRelations()
    {
        foreach (var r in _ifc.Relationships())
        {
            // Resolve IFC ids -> BOS entity indices
            if (!_entityByIfcId.TryGetValue(r.A, out var a)) continue;
            if (!_entityByIfcId.TryGetValue(r.B, out var b)) continue;

            switch (r.Kind)
            {
                case IfcRelKind.RelDefinesByType:
                    // In BOS, store type pointer on the Entity, OR store a relation.
                    // Your schema has Entity.Type, so you’ll likely patch entities rather than store relation.
                    // If you do patching: keep a temporary map and apply it in a second pass.
                    AddTypeLink(a, b);
                    break;

                case IfcRelKind.Aggregates:
                    _bos.AddRelation(new EntityRelation(a, b, RelationType.ChildOf));
                    break;

                case IfcRelKind.ContainedInSpatialStructure:
                    _bos.AddRelation(new EntityRelation(a, b, RelationType.ContainedIn));
                    break;

                case IfcRelKind.VoidsElement:
                    _bos.AddRelation(new EntityRelation(a, b, RelationType.Voids));
                    break;

                case IfcRelKind.FillsElement:
                    _bos.AddRelation(new EntityRelation(a, b, RelationType.Fills));
                    break;

                case IfcRelKind.AssociatesMaterial:
                    _bos.AddRelation(new EntityRelation(a, b, RelationType.HasMaterial));
                    break;

                case IfcRelKind.ConnectsPorts:
                    _bos.AddRelation(new EntityRelation(a, b, RelationType.ConnectsTo));
                    break;

                case IfcRelKind.AssignsToGroup:
                    _bos.AddRelation(new EntityRelation(a, b, RelationType.MemberOf));
                    break;

                case IfcRelKind.SpaceBoundary:
                    _bos.AddRelation(new EntityRelation(a, b, RelationType.BoundedBy));
                    break;
            }
        }

        ApplyTypeLinks();
    }

    private readonly List<(EntityIndex instance, EntityIndex type)> _typeLinks = new();

    private void AddTypeLink(EntityIndex instance, EntityIndex type)
        => _typeLinks.Add((instance, type));

    private void ApplyTypeLinks()
    {
        // depends on your BOS storage: you might need an "UpdateEntity" operation,
        // or you create Entity.Type in the first place by pre-reading RelDefinesByType.
        // This is intentionally abstract.
    }
}
