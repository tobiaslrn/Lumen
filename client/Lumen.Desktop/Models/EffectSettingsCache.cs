using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace Lumen.Desktop.Models;

public class EffectSettingsCache
{
    private Dictionary<string, EffectSettings> _effectSettingsMap = new();

    [JsonRequired]
    [JsonPropertyName("CacheItems")]
    // ReSharper disable once UnusedMember.Global : Used for serialization
    public List<EffectSettings> Items
    {
        get => _effectSettingsMap.Values.ToList();
        // ReSharper disable once UnusedMember.Global : Used for serialization
        set => _effectSettingsMap = value.ToDictionary(e => e.GetType().Name);
    }

    public void WriteEffect(EffectSettings effectSetting)
    {
        var typename = effectSetting.GetType().Name;
        _effectSettingsMap[typename] = effectSetting;
    }

    public bool TryGetEffect(string name, [NotNullWhen(returnValue: true)] out EffectSettings? effectSetting)
    {
        return _effectSettingsMap.TryGetValue(name, out effectSetting);
    }
}