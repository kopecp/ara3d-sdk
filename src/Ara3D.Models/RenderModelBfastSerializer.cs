using System.Diagnostics;
using Ara3D.Geometry;
using Ara3D.IO.BFAST;
using Ara3D.Memory;
using Ara3D.MemoryMappedFiles;
using Ara3D.Utils;

namespace Ara3D.Models;

public static class RenderModelBfastSerializer
{
    public static string[] BufferNames = new[]
    {
        nameof(RenderModelData.VertexBuffer),
        nameof(RenderModelData.IndexBuffer),
        nameof(RenderModelData.MeshBuffer),
        nameof(RenderModelData.InstanceBuffer),
    };

    public static unsafe void Save(RenderModelData renderModelData, FilePath filePath)
    {
        var sizes = new[]
        {
            renderModelData.VertexBuffer.Bytes.Count,
            renderModelData.IndexBuffer.Bytes.Count,
            renderModelData.MeshBuffer.Bytes.Count,
            renderModelData.InstanceBuffer.Bytes.Count,
        };

        Debug.Assert(sizes[0] % sizeof(Point3D) == 0);
        Debug.Assert(sizes[1] % 4 == 0);
        Debug.Assert(sizes[2] % sizeof(MeshSliceStruct) == 0);
        Debug.Assert(sizes[3] % InstanceStruct.Size == 0);

        var ptrs = new[]             
        {
            renderModelData.VertexBuffer.Bytes.Ptr,
            renderModelData.IndexBuffer.Bytes.Ptr,
            renderModelData.MeshBuffer.Bytes.Ptr,
            renderModelData.InstanceBuffer.Bytes.Ptr,
        };

        long OnBuffer(Stream stream, int index, string name, long bytesToWrite)
        {
            var ptr = ptrs[index];
            var size = sizes[index];
            Debug.Assert(bytesToWrite == size);
            while (true)
            {
                var tmp = Math.Min(size, int.MaxValue);
                var span = new ReadOnlySpan<byte>(ptr, (int)tmp);
                stream.Write(span);
                size -= tmp;
                if (size <= 0)
                    break;
            }
            stream.Flush();
            return bytesToWrite;
        }

        BFast.Write((string)filePath, BufferNames, sizes.Select(sz => (long)sz), OnBuffer);
    }

    public static unsafe void AddRange<T>(this UnmanagedList<T> self, byte* ptr, long count)
        where T: unmanaged
    {
        var byteSlice = new ByteSlice(ptr, count);
        self.AddRange(byteSlice.AsReadOnlySpan<T>());
    }

    public static unsafe RenderModelData Load(FilePath fp)
    {
        var r = new RenderModelData(3);

        void OnView(string name, MemoryMappedView view, int index)
        {
            byte* srcPointer = null;
            view.Accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref srcPointer);
            try
            {
                srcPointer += view.Accessor.PointerOffset;

                switch (index)
                {
                    case 0: 
                        r.VertexBuffer.AddRange(srcPointer, view.Size); 
                        break;
                    case 1: 
                        r.IndexBuffer.AddRange(srcPointer, view.Size); 
                        break;
                    case 2: 
                        r.MeshBuffer.AddRange(srcPointer, view.Size); 
                        break;
                    case 3: 
                        r.InstanceBuffer.AddRange(srcPointer, view.Size); 
                        break;
                    default: 
                        throw new Exception($"Unrecognized memory buffer: {name} at position {index}");
                }
            }
            finally
            {
                view.Accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }

        BFastReader.Read(fp, OnView);
        return r;
    }
}