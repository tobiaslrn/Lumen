using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace Lumen.Service.Utilities;

public static class BinarySerializer
{
    public static void ThrowForBigEndian()
    {
        if (!BitConverter.IsLittleEndian)
            throw new NotSupportedException("BigEndian systems are not supported.");
    }


    public static void WriteBool(ref Span<byte> span, bool val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(bool));
    }


    public static void WriteByte(ref Span<byte> span, byte val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(byte));
    }

    public static void WriteSByte(ref Span<byte> span, sbyte val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(sbyte));
    }

    public static void WriteShort(ref Span<byte> span, short val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(short));
    }


    public static void WriteUShort(ref Span<byte> span, ushort val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(ushort));
    }


    public static void WriteInt(ref Span<byte> span, int val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(int));
    }


    public static void WriteUInt(ref Span<byte> span, uint val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(uint));
    }

    public static void WriteLong(ref Span<byte> span, long val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(long));
    }


    public static void WriteULong(ref Span<byte> span, ulong val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(ulong));
    }


    public static void WriteFloat(ref Span<byte> span, float val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(float));
    }


    public static void WriteDouble(ref Span<byte> span, double val)
    {
        MemoryMarshal.Write(span, in val);
        span = span.Slice(sizeof(double));
    }


    public static void WriteBlock(ref Span<byte> span, ReadOnlySpan<byte> val)
    {
        val.CopyTo(span);
        span = span.Slice(val.Length);
    }


    public static bool ReadBool(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<bool>(span);
        span = span.Slice(sizeof(bool));
        return result;
    }


    public static byte ReadByte(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<byte>(span);
        span = span.Slice(sizeof(byte));
        return result;
    }


    public static sbyte ReadSByte(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<sbyte>(span);
        span = span.Slice(sizeof(sbyte));
        return result;
    }


    public static short ReadShort(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<short>(span);
        span = span.Slice(sizeof(short));
        return result;
    }


    public static ushort ReadUShort(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<ushort>(span);
        span = span.Slice(sizeof(ushort));
        return result;
    }


    public static int ReadInt(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<int>(span);
        span = span.Slice(sizeof(int));
        return result;
    }

    public static uint ReadUInt(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<uint>(span);
        span = span.Slice(sizeof(uint));
        return result;
    }


    public static long ReadLong(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<long>(span);
        span = span.Slice(sizeof(long));
        return result;
    }

    public static ulong ReadULong(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<ulong>(span);
        span = span.Slice(sizeof(ulong));
        return result;
    }


    public static float ReadFloat(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<float>(span);
        span = span.Slice(sizeof(float));
        return result;
    }


    public static double ReadDouble(ref ReadOnlySpan<byte> span)
    {
        var result = MemoryMarshal.Read<double>(span);
        span = span.Slice(sizeof(double));
        return result;
    }


    public static byte[] ReadBlock(ref ReadOnlySpan<byte> span, int byteCount)
    {
        var result = new byte[byteCount];
        ReadBlock(ref span, result);
        return result;
    }


    private static void ReadBlock(ref ReadOnlySpan<byte> span, Span<byte> output)
    {
        span.Slice(0, output.Length).CopyTo(output);
        span = span.Slice(output.Length);
    }
}