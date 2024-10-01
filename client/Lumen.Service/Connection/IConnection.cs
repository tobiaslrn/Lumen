using Lumen.Service.ControllerMessages;

namespace Lumen.Service.Connection;

public interface IConnection : IDisposable
{
    Task SendMessage(ControllerMessage controllerMessage, CancellationToken cts);
}