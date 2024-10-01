namespace Lumen.Service.ControllerMessages;

public interface IByteSerializable
{
    void SerializeAsBytes(ref Span<byte> span);
}