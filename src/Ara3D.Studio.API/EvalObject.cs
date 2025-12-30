using Ara3D.Memory;

namespace Ara3D.Studio.API;

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
    public object? Value { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
    public Dictionary<string, IBuffer> Buffers { get; set; } = new();
}
