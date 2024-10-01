using System.Text.Json.Serialization;

namespace Lumen.Desktop.Models;

public class ApplicationSettings(bool startAppAtBoot)
{
    [JsonPropertyName("StartAtBoot")] public bool StartAppAtBoot { get; set; } = startAppAtBoot;
}