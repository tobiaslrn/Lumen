using System.Text.Json.Serialization;

namespace Lumen.Desktop.Models;

public class LumenSettings(StripSettings stripSettings, ApplicationSettings applicationSettings)
{
    [JsonRequired]
    [JsonPropertyName("Strip")]
    public StripSettings StripSettings { get; set; } = stripSettings;

    [JsonRequired]
    [JsonPropertyName("Application")]
    public ApplicationSettings ApplicationSettings { get; set; } = applicationSettings;
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(LumenSettings))]
internal partial class LumenSettingsContext : JsonSerializerContext;