using Lumen.Service.Utilities;

namespace Lumen.Service.ControllerMessages;

public record ControllerMessage(DateTimeOffset Ts, MessageKind MessageKind) : IByteSerializable
{
    public void SerializeAsBytes(ref Span<byte> span)
    {
        BinarySerializer.ThrowForBigEndian();

        BinarySerializer.WriteLong(ref span, Ts.ToUnixTimeMilliseconds());
        BinarySerializer.WriteUShort(ref span, (ushort)MessageKind.Descriminator());
        MessageKind.SerializeAsBytes(ref span);
    }
}