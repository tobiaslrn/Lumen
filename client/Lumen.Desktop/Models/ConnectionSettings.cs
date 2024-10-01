using System.Text.Json.Serialization;

namespace Lumen.Desktop.Models;

public class ConnectionSettings(string controllerAddress, ushort controllerPort)
{
    [JsonPropertyName("ControllerAddress")]
    public string ControllerAddress { get; set; } = controllerAddress;

    [JsonPropertyName("ControllerPort")] public ushort ControllerPort { get; set; } = controllerPort;
}