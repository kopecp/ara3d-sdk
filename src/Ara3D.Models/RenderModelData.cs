using System.Diagnostics;
using Ara3D.Collections;
using Ara3D.F8;
using Ara3D.Geometry;
using Ara3D.Memory;
using SNVector3 = System.Numerics.Vector3;

namespace Ara3D.Models;

/// <summary>
/// This is a long-lived data structure that contains the memory data buffers that contain
/// the data for rendering models. It minimizes reallocation. Memory is unmanaged, and not
/// touched by the garbage collector. This is also the data structure used when storing loaded data.
/// All meshes in this structure must have the same PrimitiveKind (e.g., all Lines or all Triangles)
/// </summary>
public class RenderModelData : IDisposable
{
    public int PrimitiveSize { get; set; }
    public Bounds3D TotalBounds { get; private set; }
    
    public UnmanagedList<float> VertexBuffer { get; private set; }
    public UnmanagedList<uint> IndexBuffer { get; private set; }
    public UnmanagedList<MeshSliceStruct> MeshBuffer { get; private set; }
    public UnmanagedList<InstanceStruct> InstanceBuffer { get; private set; } 
    public UnmanagedList<Bounds3D> MeshBounds { get; private set; }
    
    public long TotalVertexCount { get; private set; }
    public long TotalFaceCount { get; private set; }

    public int VertexCount => VertexBuffer.Count;
    public int IndexCount => IndexBuffer.Count;
    public int FaceCount => IndexCount / PrimitiveSize;
    public int MeshCount => MeshBuffer.Count;
    public int InstanceCount => InstanceBuffer.Count;

    public RenderModelData(int primSize)
    {
        if (primSize < 1 || primSize > 4)
            throw new Exception($"Render model data primitive size ({primSize}) must be from 1 to 4 inclusive");

        PrimitiveSize = primSize;
        VertexBuffer = new();
        IndexBuffer = new();
        MeshBuffer = new();
        InstanceBuffer = new();
        MeshBounds = new();
    }
    
    public bool IsModel3D
        => PrimitiveSize == 3;

    public Model3D ToModel3D()
    {
        if (PrimitiveSize != 3) throw new Exception("Not a triangle mesh");
        var meshes = MeshBuffer.Select(GetMesh);
        return new Model3D(meshes, InstanceBuffer);
    }

    public void Dispose()
    {
        VertexBuffer?.Dispose();
        IndexBuffer?.Dispose();
        MeshBuffer?.Dispose();
        InstanceBuffer?.Dispose();
        VertexBuffer = null;
        IndexBuffer = null;
        MeshBuffer = null;
        InstanceBuffer = null;
    }
    
    public void Update(
        IBuffer<float> vertexBuffer, 
        IBuffer<uint> indexBuffer, 
        IBuffer<MeshSliceStruct> meshSlices,
        IBuffer<InstanceStruct> instances)
    {
        VertexBuffer.CopyFrom(vertexBuffer);
        IndexBuffer.CopyFrom(indexBuffer);
        MeshBuffer.CopyFrom(meshSlices);
        InstanceBuffer.CopyFrom(instances);
    }

    public void UpdateVertexBuffer(IBuffer<float> vertexBuffer) => VertexBuffer.CopyFrom(vertexBuffer);
    public void UpdateIndexBuffer(IBuffer<uint> indexBuffer) => IndexBuffer.CopyFrom(indexBuffer);
    public void UpdateMeshBuffer(IBuffer<MeshSliceStruct> meshBuffer) => MeshBuffer.CopyFrom(meshBuffer);
    public void UpdateInstanceBuffer(IBuffer<InstanceStruct> instanceBuffer) => InstanceBuffer.CopyFrom(instanceBuffer);

    public void UpdateVertexBuffer(IReadOnlyList<float> vertexBuffer) => VertexBuffer.CopyFrom(vertexBuffer);
    public void UpdateIndexBuffer(IReadOnlyList<uint> indexBuffer) => IndexBuffer.CopyFrom(indexBuffer);
    public void UpdateMeshBuffer(IReadOnlyList<MeshSliceStruct> meshBuffer) => MeshBuffer.CopyFrom(meshBuffer);
    public void UpdateInstanceBuffer(IReadOnlyList<InstanceStruct> instanceBuffer) => InstanceBuffer.CopyFrom(instanceBuffer);

    public void Update(RenderModelData data)
    {
        PrimitiveSize = data.PrimitiveSize;

        UpdateVertexBuffer(data.VertexBuffer);
        UpdateIndexBuffer(data.IndexBuffer);
        UpdateMeshBuffer(data.MeshBuffer);
        UpdateInstanceBuffer(data.InstanceBuffer);
        ValidateMeshSlices();
        MeshBounds.Clear();
        MeshBounds.AddRange(data.MeshBounds);
    }

    public void Update(IModel3D model)
    {
        PrimitiveSize = 3;

        if (!IsModel3D)
            throw new Exception("Not a model 3D");

        VertexBuffer.Clear();
        IndexBuffer.Clear();
        MeshBuffer.Clear();
        InstanceBuffer.Clear();

        foreach (var mesh in model.Meshes)
        {
            var faceIndices = mesh.FaceIndices;
            var points = mesh.Points;

            var meshSlice = new MeshSliceStruct()
            {
                FirstIndex = (uint)IndexBuffer.Count,
                IndexCount = (uint)faceIndices.Count * (uint)PrimitiveSize,
                BaseVertex = VertexBuffer.Count / PrimitiveSize,
                VertexCount = points.Count
            };

            MeshBuffer.Add(meshSlice);

            // TODO: optimization opportunity 

            foreach (var point3D in points)
            {
                VertexBuffer.Add(point3D.X);
                VertexBuffer.Add(point3D.Y);
                VertexBuffer.Add(point3D.Z);
            }

            foreach (var index3 in faceIndices)
            {
                IndexBuffer.Add((uint)index3.A.Value);
                IndexBuffer.Add((uint)index3.B.Value);
                IndexBuffer.Add((uint)index3.C.Value);
            }
        }

        InstanceBuffer.AddRange(model.Instances);
        ValidateMeshSlices();
        RecomputeBounds();
    }

    public void Update(IEnumerable<RenderModelData> models)
    {
        PrimitiveSize = 3;

        if (!IsModel3D)
            throw new Exception("Not a model 3D");

        VertexBuffer.Clear();
        IndexBuffer.Clear();
        MeshBuffer.Clear();
        InstanceBuffer.Clear();

        foreach (var model in models)
        {
            if (model.PrimitiveSize != model.PrimitiveSize)
                continue;

            VertexBuffer.AddRange(model.VertexBuffer);
            IndexBuffer.AddRange(model.IndexBuffer);
            var meshOffset = MeshBuffer.Count;
            MeshBuffer.AddRange(model.MeshBuffer);
            foreach (var i in model.InstanceBuffer)
                InstanceBuffer.Add(i.WithMeshIndex(i.MeshIndex + meshOffset));
        }

        RecomputeBounds();
    }

    public void Update(LineMesh3D lines, Matrix4x4 transform, Material material)
    {
        PrimitiveSize = 2;

        VertexBuffer.Clear();
        IndexBuffer.Clear();
        MeshBuffer.Clear();
        InstanceBuffer.Clear();

        var points = lines.Points;
        var faceIndices = lines.FaceIndices;
        
        var meshSlice = new MeshSliceStruct()
        {
            FirstIndex = (uint)0,
            IndexCount = (uint)faceIndices.Count * (uint)PrimitiveSize,
            BaseVertex = 0,
            VertexCount = points.Count
        };

        MeshBuffer.Add(meshSlice);

        foreach (var point3D in points)
        {
            VertexBuffer.Add(point3D.X);
            VertexBuffer.Add(point3D.Y);
            VertexBuffer.Add(point3D.Z);
        }

        foreach (var index2 in faceIndices)
        {
            IndexBuffer.Add((uint)index2.A.Value);
            IndexBuffer.Add((uint)index2.B.Value);
        }

        var inst = new InstanceStruct(-1, transform, 0, material);

        InstanceBuffer.Add(inst);
        ValidateMeshSlices();
        RecomputeBounds();
    }

    public void ValidateMeshSlices()
    {
#if DEBUG
        for (var i = 0; i < MeshBuffer.Count; i++)
        {
            var meshSlice = MeshBuffer[i];
            Debug.Assert(meshSlice.BaseVertex >= 0);
            Debug.Assert(meshSlice.VertexCount + meshSlice.BaseVertex <= VertexBuffer.Count);
            //Debug.Assert(meshSlice.FirstIndex >= 0);
            Debug.Assert(meshSlice.FirstIndex + meshSlice.IndexCount <= IndexBuffer.Count);
        }
#endif
    }

    public Bounds3D ComputeBounds(IBuffer<Point3D> points)
    {
        var span = points.Reinterpret<SNVector3>().AsReadOnlySpan();
        var (min, max) = span.ComputeBounds();
        return new(new Vector3(min), new Vector3(max));
    }

    public void RecomputeBounds()
    {
        MeshBounds.Clear();
        foreach (var meshSlice in MeshBuffer)
        {
            var points = GetPoints(meshSlice);
            var bounds = ComputeBounds(points);
            MeshBounds.Add(bounds);
        }

        TotalBounds = Bounds3D.Empty;
        TotalFaceCount = 0;
        TotalVertexCount = 0;
        foreach (var inst in InstanceBuffer)
        {
            if (inst.MeshIndex < 0) 
                continue;
            var localMeshSlice = MeshBuffer[inst.MeshIndex];
            var localBounds = MeshBounds[inst.MeshIndex];
            TotalFaceCount += localMeshSlice.IndexCount / PrimitiveSize;
            TotalVertexCount += localMeshSlice.VertexCount;
            if (localBounds.Size.X < 0 || localBounds.Size.Y < 0 || localBounds.Size.Z < 0)
                continue;
            var transformedBounds = localBounds.Transform(inst.Matrix4x4);
            TotalBounds = TotalBounds.Include(transformedBounds);
        }
    }

    public TriangleMesh3D GetMesh(MeshSliceStruct meshSlice)
    {
        if (PrimitiveSize != 3)
            throw new Exception($"Primitive size must be 3 to get a triangle mesh instead it is {PrimitiveSize}");

        var faceSlice = IndexBuffer.Slice(meshSlice.FirstIndex, meshSlice.IndexCount).Reinterpret<Integer3>();
        return new(GetPoints(meshSlice), faceSlice);
    }

    public IBuffer<Point3D> GetPoints(MeshSliceStruct meshSlice)
        => BufferExtensions.Slice(VertexBuffer?.Reinterpret<Point3D>(), meshSlice.BaseVertex, meshSlice.VertexCount);
}