using Ara3D.Geometry;
using Ara3D.Models;

namespace Ara3D.Studio.API;

public enum AttributeDomain
{
    Object,
    Instance,
    Primitive,
    Point,
}

public enum AttributeSemantics
{
    Normal,
    Color,
    Selection,
    Distance,
    Material,
    Id,
    Custom,
}

public record FlowAttribute
{
    public AttributeDomain Domain { get; }
    public AttributeSemantics Semantics { get; }
    public int Index { get; }
    public Type Type { get; }
    public int Count { get; }
    public int Arity { get; }
}

public record FlowAttribute<T> : FlowAttribute
{
    public IReadOnlyList<T> Values { get; }
}

/// <summary>
/// This is the primary type of object that flows through the modifier stack and interflow graphs
/// in Ara 3D Studio. Interflow is a 3D geometric graph system inspired by Houdini, Grasshopper, Dynamo, and MCG.
/// A FlowObject is transient. I
/// </summary>
public sealed class FlowObject : ITransformable3D<FlowObject>
{
    public Type? Type { get; }
    public object? Value { get; }
    public bool IsNull => Value == null;
    public RenderSettings? RenderSettings { get; }
    public Material Material { get; }

    // Attachments are workflow specific
    public IReadOnlyList<object> Attachments { get; }

    // NOTE: selection, UVs, Normals, VertexColors, and more are stored as attributes. 
    public IReadOnlyList<FlowAttribute> Attributes { get; }

    public FlowObject(object? value, RenderSettings? renderSettings, Material material, IReadOnlyList<FlowAttribute> attributes, IReadOnlyList<object> attachments)
    {
        Type = value?.GetType();
        Value = value;
        RenderSettings = renderSettings;
        Attributes = attributes ?? [];
        Material = material;
        Attachments = attachments ?? [];
    }

    public FlowObject WithNewValue(object value)
        => new(value, RenderSettings, Material, Attributes, Attachments);

    public FlowObject WithNewRenderSettings(RenderSettings renderSettings)
        => new(Value, renderSettings, Material, Attributes, Attachments);

    public FlowObject WithNewAttributes(IReadOnlyList<FlowAttribute> attributes)
        => new(Value, RenderSettings, Material, attributes, Attachments);

    public FlowObject WithMaterial(Material material)
        => new(Value, RenderSettings, material, Attributes, Attachments);

    public FlowObject WithNewAttachments(IReadOnlyList<object> attachments)
        => new(Value, RenderSettings, Material, Attributes, attachments);

    public bool HasObject
        => Value != null;

    public FlowObject Transform(Transform3D t)
    {
        throw new NotImplementedException("Work in progress");
    }

    public IEnumerable<T> GetAttachments<T>()
        => Attachments.OfType<T>();

    public T? GetAttachment<T>()
        => GetAttachments<T>().FirstOrDefault();
}