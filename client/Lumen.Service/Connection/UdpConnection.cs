using System.Net.Sockets;
using Lumen.Service.ControllerMessages;

namespace Lumen.Service.Connection;

public class UdpConnection : IConnection, IDisposable
{
    private readonly UdpClient _connection;

    public UdpConnection(string hostname, int port)
    {
        _connection = new UdpClient(port);
        _connection.Connect(hostname, port);
    }


    public Task SendMessage(ControllerMessage controllerMessage, CancellationToken cts = default)
    {
        var buffer = new byte[1024];
        var span = buffer.AsSpan();
        try
        {
            controllerMessage.SerializeAsBytes(ref span);
        }
        catch (ArgumentOutOfRangeException)
        {
            return Task.CompletedTask;
        }

        var writtenBytes = buffer.Length - span.Length;
        return _connection.SendAsync(buffer, writtenBytes);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}