using Lumen.Service.Utilities;

namespace Lumen.Service.ControllerMessages;

public readonly record struct Rgb8(byte R, byte G, byte B) : IByteSerializable
{
    public void SerializeAsBytes(ref Span<byte> span)
    {
        BinarySerializer.WriteByte(ref span, R);
        BinarySerializer.WriteByte(ref span, G);
        BinarySerializer.WriteByte(ref span, B);
    }
}