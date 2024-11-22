using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Lumen.Desktop.ViewModels.TabPages.EffectSettings;
using Lumen.Service;
using Lumen.Service.ControllerMessages;
using Lumen.Service.Effect;

namespace Lumen.Desktop.Models;

[JsonDerivedType(typeof(EffectOffSettings), nameof(EffectOffSettings))]
[JsonDerivedType(typeof(EffectSolidSettings), nameof(EffectSolidSettings))]
[JsonDerivedType(typeof(EffectAmbientSettings), nameof(EffectAmbientSettings))]
public abstract class EffectSettings
{
    public abstract EffectSettingViewModel ToEffectSettingViewModel();
    public abstract bool TryToEffect(StripLayout layout, [NotNullWhen(true)] out IEffect? effect);

    public abstract bool IsConstructable();

    public override bool Equals(object? obj)
    {
        if (obj is EffectSettings other)
        {
            return GetType() == other.GetType();
        }

        return false;
    }

    public override int GetHashCode()
    {
        return GetType().GetHashCode();
    }
}

public class EffectOffSettings : EffectSettings
{
    public override EffectSettingViewModel ToEffectSettingViewModel()
    {
        return new EffectOffViewModel();
    }

    public override bool TryToEffect(StripLayout layout, [NotNullWhen(true)] out IEffect? effect)
    {
        effect = new SolidColorEffect(layout, new Rgb8(0, 0, 0));
        return true;
    }

    public override bool IsConstructable()
    {
        return true;
    }
}

public class EffectAmbientSettings : EffectSettings
{
    [JsonPropertyName("MonitorName")] public string? Monitor { get; set; }
    [JsonPropertyName("RefreshRate")] public uint RefreshRate { get; set; } = 60;
    [JsonPropertyName("DetailLevel")] public uint DetailLevel { get; set; } = 6;

    [JsonPropertyName("TemporalSmoothing")]
    public uint TemporalSmoothing { get; set; } = 4;

    public override EffectSettingViewModel ToEffectSettingViewModel()
    {
        return new EffectAmbientViewModel(this);
    }

    public override bool TryToEffect(StripLayout layout, [NotNullWhen(true)] out IEffect? effect)
    {
        if (AmbientColorEffect.TryBuild(layout, Monitor ?? "", (int)DetailLevel,
                (int)TemporalSmoothing, RefreshRate, out var e))
        {
            effect = e;
            return true;
        }
        else
        {
            effect = null;
            return false;
        }
    }

    public override bool IsConstructable()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}

public class EffectSolidSettings : EffectSettings
{
    [JsonPropertyName("R")] public byte R { get; set; } = 255;
    [JsonPropertyName("G")] public byte G { get; set; } = 255;
    [JsonPropertyName("B")] public byte B { get; set; } = 255;

    public override EffectSettingViewModel ToEffectSettingViewModel()
    {
        return new EffectSolidViewModel(this);
    }

    public override bool TryToEffect(StripLayout layout, [NotNullWhen(true)] out IEffect? effect)
    {
        effect = new SolidColorEffect(layout, new Rgb8(R, G, B));
        return true;
    }

    public override bool IsConstructable()
    {
        return true;
    }
}