using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Lumen.Desktop.Models;
using Lumen.Service.Effect;
using Lumen.Service.Utilities;

namespace Lumen.Desktop.ViewModels.TabPages.EffectSettings;

public partial class EffectAmbientViewModel : EffectSettingViewModel
{
    [ObservableProperty] private uint _fps;
    [ObservableProperty] private uint _temporalSmoothing;
    [ObservableProperty] private uint _downscaleLevel;
    [ObservableProperty] private Range<int> _downscaleLevelRange;
    [ObservableProperty] private List<string> _monitors;
    [ObservableProperty] private string _selectedMonitor;

    private readonly EffectAmbientSettings _ambientSettings;

    public EffectAmbientViewModel(EffectAmbientSettings effectAmbientSettings)
    {
        _ambientSettings = effectAmbientSettings;
        _fps = effectAmbientSettings.RefreshRate;
        _temporalSmoothing = effectAmbientSettings.TemporalSmoothing;

        _monitors = AmbientColorEffect.MonitorNames().ToList();
        if (_monitors.Count == 0)
            _monitors.Add("None");

        if (_ambientSettings.Monitor is { } monitor && _monitors.Contains(monitor))
            _selectedMonitor = monitor;
        else
            _selectedMonitor = _monitors[0];

        _downscaleLevelRange = AmbientColorEffect.DetailLevelRange(_selectedMonitor);
        _downscaleLevel = effectAmbientSettings.DetailLevel;
    }

    partial void OnSelectedMonitorChanging(string value)
    {
        DownscaleLevelRange = AmbientColorEffect.DetailLevelRange(value);
    }

    public override string EffectName => "Ambient";

    public override Models.EffectSettings GetEffectSettings()
    {
        return _ambientSettings;
    }

    protected override void UpdateStorageValues()
    {
        _ambientSettings.Monitor = SelectedMonitor;
        _ambientSettings.RefreshRate = Fps;
        _ambientSettings.DetailLevel = DownscaleLevel;
        _ambientSettings.TemporalSmoothing = TemporalSmoothing;
    }
}