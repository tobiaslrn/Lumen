using System.Text.Json.Serialization;

namespace Lumen.Desktop.Models;

public class StripSettings
{
    public StripSettings(string? activeEffectTypeName, StripLayoutSettings stripLayoutSettings,
        ConnectionSettings connectionSettings, EffectSettingsCache cachedEffectSettings)
    {
        ActiveEffectTypeName = activeEffectTypeName;
        StripLayoutSettings = stripLayoutSettings;
        ConnectionSettings = connectionSettings;
        CachedEffectSettings = cachedEffectSettings;
    }

    [JsonRequired]
    [JsonPropertyName("ActiveEffectTypeName")]
    public string? ActiveEffectTypeName { get; set; }

    [JsonRequired]
    [JsonPropertyName("StripLayout")]
    public StripLayoutSettings StripLayoutSettings { get; set; }

    [JsonRequired]
    [JsonPropertyName("Connection")]
    public ConnectionSettings ConnectionSettings { get; set; }

    [JsonRequired]
    [JsonPropertyName("CachedEffectSettings")]
    public EffectSettingsCache CachedEffectSettings { get; set; }

    public EffectSettings? GetCurrentEffectSettings()
    {
        if (ActiveEffectTypeName != null && CachedEffectSettings.TryGetEffect(ActiveEffectTypeName, out var effect))
        {
            return effect;
        }

        return null;
    }

    public void DeleteCurrentEffectSettings()
    {
        if (ActiveEffectTypeName != null)
        {
            CachedEffectSettings.DeleteEffect(ActiveEffectTypeName);
        }
    }
}