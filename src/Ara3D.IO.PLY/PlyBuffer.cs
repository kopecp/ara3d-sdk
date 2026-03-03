using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Ara3D.Memory;

namespace Ara3D.IO.PLY;

public interface IPlyBuffer
{
    string Name { get; }
    int Count { get; }
    void LoadValue(string s);
    void LoadValue(BinaryReader br);
    int GetInt(int index);
    double GetDouble(int index);
}

/// <summary>
/// One fast implementation for all scalar numeric PLY buffers.
/// Uses unmanaged storage and typed pointer access for speed.
/// </summary>
public unsafe class PlyBuffer<T> : IPlyBuffer where T : unmanaged
{
    public readonly UnmanagedList<T> Buffer;
    public static readonly int Size = sizeof(T);
    public int Count => Buffer.Count;
    public string Name { get; }

    public PlyBuffer(int count, string name)
    {
        Buffer = new(count);
        Name = name;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetInt(int index)
        => ToInt(Buffer[index]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetDouble(int index)
        => ToDouble(Buffer[index]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LoadValue(string s)
        => Buffer.Add(ParseAscii(s));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LoadValue(System.IO.BinaryReader br)
        => Buffer.Add(ReadBinary(br));
    

    // ---- parsing / conversion ----

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ParseAscii(string s)
    {
        Debug.Assert(s is not null);
        
        // Fast paths without reflection; extend if you add more numeric types.
        if (typeof(T) == typeof(byte)) return (T)(object)byte.Parse(s);
        if (typeof(T) == typeof(sbyte)) return (T)(object)sbyte.Parse(s);
        if (typeof(T) == typeof(short)) return (T)(object)short.Parse(s);
        if (typeof(T) == typeof(ushort)) return (T)(object)ushort.Parse(s);
        if (typeof(T) == typeof(int)) return (T)(object)int.Parse(s);
        if (typeof(T) == typeof(uint)) return (T)(object)uint.Parse(s);
        if (typeof(T) == typeof(float)) return (T)(object)float.Parse(s);
        if (typeof(T) == typeof(double)) return (T)(object)double.Parse(s);

        throw new NotSupportedException($"PLY ASCII parse not supported for {typeof(T).Name}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ToInt(T v)
    {
        if (typeof(T) == typeof(float)) return (int)(float)(object)v;
        if (typeof(T) == typeof(double)) return (int)(double)(object)v;
        if (typeof(T) == typeof(uint)) return unchecked((int)(uint)(object)v);
        return (int)Convert.ChangeType(v, typeof(int));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ToDouble(T v)
    {
        if (typeof(T) == typeof(float)) return (float)(object)v;
        if (typeof(T) == typeof(double)) return (double)(object)v;
        // integral types
        return Convert.ToDouble(v);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadExactly(BinaryReader reader, Span<byte> buffer)
    {
        if (buffer.Length == 0)
            return;

        var stream = reader.BaseStream;
        var totalRead = 0;

        while (totalRead < buffer.Length)
        {
            var read = stream.Read(buffer.Slice(totalRead));
            if (read == 0)
                throw new EndOfStreamException(
                    $"Unexpected end of stream. Needed {buffer.Length} bytes, got {totalRead}.");

            totalRead += read;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ReadBinary(System.IO.BinaryReader br)
    {
        // NOTE: PLY binary endianness depends on header ("binary_little_endian"/"binary_big_endian").
        // This assumes LITTLE endian. If you support both, pass a flag in and branch to ReadBigEndian.
        if (typeof(T) == typeof(byte)) return (T)(object)br.ReadByte();
        if (typeof(T) == typeof(sbyte)) return (T)(object)br.ReadSByte();

        Span<byte> tmp = stackalloc byte[Size];
        ReadExactly(br, tmp);

        if (typeof(T) == typeof(short)) return (T)(object)BinaryPrimitives.ReadInt16LittleEndian(tmp);
        if (typeof(T) == typeof(ushort)) return (T)(object)BinaryPrimitives.ReadUInt16LittleEndian(tmp);
        if (typeof(T) == typeof(int)) return (T)(object)BinaryPrimitives.ReadInt32LittleEndian(tmp);
        if (typeof(T) == typeof(uint)) return (T)(object)BinaryPrimitives.ReadUInt32LittleEndian(tmp);
        if (typeof(T) == typeof(float))
        {
            var u = BinaryPrimitives.ReadUInt32LittleEndian(tmp);
            return (T)(object)BitConverter.UInt32BitsToSingle(u);
        }
        if (typeof(T) == typeof(double))
        {
            var u = BinaryPrimitives.ReadUInt64LittleEndian(tmp);
            return (T)(object)BitConverter.UInt64BitsToDouble(u);
        }

        throw new NotSupportedException($"PLY binary read not supported for {typeof(T).Name}");
    }

    public IEnumerable<int> GetInts()
    {
        for (var i = 0; i < Count; ++i)
            yield return GetInt(i);
    }

    public int RecentInt
    {
        get
        {
            Debug.Assert(Count > 0, "RecentInt requested when buffer is empty");
            return GetInt(Count - 1);
        }
    }
}

// Convenience aliases to preserve your old type names (optional).
public sealed class UInt8Buffer : PlyBuffer<byte> { public UInt8Buffer(int count, string name) : base(count, name) { } }
public sealed class Int8Buffer : PlyBuffer<sbyte> { public Int8Buffer(int count, string name) : base(count, name) { } }
public sealed class Int16Buffer : PlyBuffer<short> { public Int16Buffer(int count, string name) : base(count, name) { } }
public sealed class UInt16Buffer : PlyBuffer<ushort> { public UInt16Buffer(int count, string name) : base(count, name) { } }
public sealed class Int32Buffer : PlyBuffer<int> { public Int32Buffer(int count, string name) : base(count, name) { } }
public sealed class UInt32Buffer : PlyBuffer<uint> { public UInt32Buffer(int count, string name) : base(count, name) { } }
public sealed class SingleBuffer : PlyBuffer<float> { public SingleBuffer(int count, string name) : base(count, name) { } }
public sealed class DoubleBuffer : PlyBuffer<double> { public DoubleBuffer(int count, string name) : base(count, name) { } }