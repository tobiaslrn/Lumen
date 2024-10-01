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
    public abstract IEffect? ToEffect(StripLayout layout);

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

    public override IEffect ToEffect(StripLayout layout)
    {
        return new SolidColorEffect(layout, new Rgb8(0, 0, 0));
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

    public override IEffect? ToEffect(StripLayout layout)
    {
        if (!string.IsNullOrEmpty(Monitor))
        {
            return new AmbientColorEffect(layout, Monitor, (int)DetailLevel, (int)TemporalSmoothing, RefreshRate);
        }

        return null;
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

    public override IEffect ToEffect(StripLayout layout)
    {
        return new SolidColorEffect(layout, new Rgb8(R, G, B));
    }

    public override bool IsConstructable()
    {
        return true;
    }
}