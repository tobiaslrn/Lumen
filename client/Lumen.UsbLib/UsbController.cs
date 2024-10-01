using System.Runtime.InteropServices;

// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable EnumUnderlyingTypeIsInt

namespace UsbLib;

public enum LumenResultCode : int
{
    Success = 0,
    NullPointer = 1,
    DeviceNotFound = 2,
    DeviceOpenError = 3,
    DeviceInterfaceClaimFailed = 4,
    DeviceTransferFailed = 5
}

public partial class UsbController
{
    private const string ModuleName = "lib_usb";

    private readonly IntPtr _conn;

    public UsbController(ushort vendorId, ushort productId)
    {
        _conn = new_connection(vendorId, productId);
        if (_conn == IntPtr.Zero)
        {
            throw new Exception("Failed to create USB connection.");
        }
    }

    public LumenResultCode InitializeDevice()
    {
        return initialize_device(_conn);
    }

    public LumenResultCode SendBytes(byte[] data, uint length, byte interfaceId, byte requestId, ushort valueId,
        ushort index)
    {
        return send_bytes(_conn, data, length, interfaceId, requestId, valueId, index);
    }

    ~UsbController()
    {
        if (_conn != IntPtr.Zero)
        {
            drop_connection(_conn);
        }
    }

    [LibraryImport(ModuleName, EntryPoint = "new_connection")]
    private static partial IntPtr new_connection(ushort vendorId, ushort productId);

    [LibraryImport(ModuleName, EntryPoint = "drop_connection")]
    private static partial LumenResultCode drop_connection(IntPtr conn);

    [LibraryImport(ModuleName, EntryPoint = "initialize_device")]
    private static partial LumenResultCode initialize_device(IntPtr conn);

    [LibraryImport(ModuleName, EntryPoint = "send_bytes")]
    private static partial LumenResultCode send_bytes(
        IntPtr conn,
        byte[] data, // Using byte[] as base for *const u8
        uint length,
        byte interfaceId,
        byte requestId,
        ushort valueId,
        ushort index
    );
}