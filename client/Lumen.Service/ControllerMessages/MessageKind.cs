using Lumen.Service.Utilities;

namespace Lumen.Service.ControllerMessages;

public abstract record MessageKind : IByteSerializable
{
    public abstract MessageDescriminator Descriminator();
    public abstract void SerializeAsBytes(ref Span<byte> span);
}

public enum MessageDescriminator : ushort
{
    Empty = 0,
    KeepAlive = 1,
    LedState = 2,
}

public record KeepAliveMessage(uint Milliseconds) : MessageKind
{
    public override MessageDescriminator Descriminator() => MessageDescriminator.KeepAlive;

    public override void SerializeAsBytes(ref Span<byte> span)
    {
        BinarySerializer.WriteUInt(ref span, Milliseconds);
    }
}

public record LedStateMessage(Rgb8[] LedValues) : MessageKind
{
    public override MessageDescriminator Descriminator() => MessageDescriminator.LedState;

    public override void SerializeAsBytes(ref Span<byte> span)
    {
        BinarySerializer.WriteUShort(ref span, (ushort)LedValues.Length);

        foreach (var ledValue in LedValues)
        {
            ledValue.SerializeAsBytes(ref span);
        }
    }
}