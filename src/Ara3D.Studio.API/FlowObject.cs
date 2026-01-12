using Ara3D.Geometry;
using Ara3D.Models;

namespace Ara3D.Studio.API;

public record RenderSettings(
    bool VertexColors = false, 
    bool WireFrame = false,
    bool Visible = true
);

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
/// </summary>
public sealed class FlowObject
{
    /*
    public static HashSet<Type> SupportedTypes =
    [
        // Classic discrete 2D renderable geometric objects
        typeof(LineMesh2D),
        typeof(TriangleMesh2D),
        typeof(QuadMesh2D),
        typeof(QuadGrid2D),

        // Classic discrete 3D renderable geometric objects
        typeof(LineMesh3D),
        typeof(TriangleMesh3D),
        typeof(QuadMesh3D),
        typeof(QuadGrid3D),

        // Instanced 3D mesh groups 
        typeof(Model3D),

        // BIM Data (instanced meshes with additional BIM data) 
        typeof(BimModel3D), 

        // Implicit or Continuous objects  
        typeof(ParametricSurface),
        typeof(Curve2D),
        typeof(Curve3D),
        typeof(Solid),
        typeof(SignedDistanceField2D),
        typeof(SignedDistanceField3D),
        
        // 2D Geometric Primitives 
        typeof(Point2D),
        typeof(Line2D),
        typeof(Triangle2D),
        typeof(Quad2D),
        typeof(PolyLine2D),
        typeof(Bounds2D),

        // 3D Geometric Primitives 
        typeof(Point3D),
        typeof(Line3D),
        typeof(Triangle3D),
        typeof(Quad3D),
        typeof(PolyLine3D),
        typeof(Bounds3D),
        typeof(Plane),

        // Transform Primitives
        typeof(Pose2D),
        typeof(Transform2D),
        typeof(Rotation2D),
        typeof(Pose3D),
        typeof(Transform3D),
        typeof(Rotation3D),

        // Primitive Arrays 
        typeof(Point2DArray),
        typeof(Line2DArray),
        typeof(Triangle2DArray),
        typeof(Quad2DArray),
        typeof(PolyLine2DArray),
        typeof(Bounds2DDArray),
        typeof(Point3DArray),
        typeof(Line3DArray),
        typeof(Triangle3DArray),
        typeof(Quad3DArray),
        typeof(PolyLine3DArray),
        typeof(Bounds3DArray),
        typeof(PlaneArray),

        // Transform Arrays
        typeof(Pose2DArray),
        typeof(Transform2DArray),
        typeof(Rotation2DArray),
        typeof(Pose3DArray),
        typeof(Transform3DArray),
        typeof(Rotation3DArray),
    ];
    */

    public static bool IsSupported(Type t) => true;
    public bool CanConvertTo(Type t) => true;
    public static bool IsSupported<T>() => IsSupported(typeof(T));
    public bool CanConvertTo<T>() => CanConvertTo(typeof(T));
    public T Convert<T>() => (T)Value;
    public Type Type { get; }
    public object? Value { get; }
    public List<object> CustomData { get; }
    public bool IsNull => Value == null;
    public RenderSettings? RenderSettings  { get; }

    // NOTE: selection, UVs, Normals, VertexColors, and more are stored as attributes. 
    public IReadOnlyList<Attribute> Attributes { get; }

    public FlowObject(object? value, RenderSettings? renderSettings, IReadOnlyList<Attribute> attributes)
    {
        var type = value?.GetType();
        if (type != null && !IsSupported(type))
            throw new Exception($"Not a supported type: {type}");
        Value = value;
        Type = type;
        RenderSettings = renderSettings;
        Attributes = attributes;
    }

    public FlowObject WithNewValue(object value)
        => new(value, RenderSettings, Attributes);

    public FlowObject WithNewRenderSettings(RenderSettings renderSettings)
        => new(Value, renderSettings, Attributes);

    public FlowObject WithNewAttributes(IReadOnlyList<Attribute> attributes)
        => new(Value, RenderSettings, attributes);
}