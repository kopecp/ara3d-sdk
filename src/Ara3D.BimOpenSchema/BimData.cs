using System.Collections.Generic;

namespace Ara3D.BimOpenSchema;

public class BimData : IBimData
{
    public Manifest Manifest { get; set; } = new();
    public IReadOnlyList<Diagnostic> Diagnostics { get; set; } = [];
    public IReadOnlyList<ParameterDescriptor> Descriptors { get; set; } = [];
    public IReadOnlyList<ParameterInt> IntegerParameters { get; set; } = [];
    public IReadOnlyList<ParameterSingle> SingleParameters { get; set; } = [];
    public IReadOnlyList<ParameterString> StringParameters { get; set; } = [];
    public IReadOnlyList<ParameterEntity> EntityParameters { get; set; } = [];
    public IReadOnlyList<ParameterPoint> PointParameters { get; set; } = [];
    public IReadOnlyList<Document> Documents { get; set; } = [];
    public IReadOnlyList<Entity> Entities { get; set; } = [];
    public IReadOnlyList<string> Strings { get; set; } = [];
    public IReadOnlyList<Point> Points { get; set; } = [];
    public IReadOnlyList<EntityRelation> Relations { get; set; } = [];
    public BimGeometry Geometry { get; set; }
}