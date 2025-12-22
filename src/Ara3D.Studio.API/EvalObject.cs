using Ara3D.Memory;

namespace Ara3D.Studio.API;

public enum EvalObjectTypes
{
    Unknown,
    Model3D,
    TriangleMesh3D,
    LineMesh3D,
    Points3D, 
    Transforms3D,
}

public enum KnownOptions
{
    VertexColors,
    WireFrame,
}

public enum KnownBuffer
{
    Selection
}

public sealed class EvalObject
{
    public object Value { get; set; }
    public EvalObjectTypes ObjectType { get; set; }
    public Dictionary<string, object> Settings { get; set; }
    public Dictionary<string, IBuffer> Buffers { get; set; }
}