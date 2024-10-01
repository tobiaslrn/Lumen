using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Lumen.Desktop.Models;
using Lumen.Desktop.ViewModels.TabPages.EffectSettings;

namespace Lumen.Desktop.ViewModels.TabPages;

public partial class StripViewModel : StorageBasedViewModel
{
    [ObservableProperty] private List<StripEffectModel> _effectItems;
    [ObservableProperty] private StripEffectModel? _selectedEffectItem;

    [ObservableProperty] private uint _layoutCountLeft;
    [ObservableProperty] private uint _layoutCountTop;
    [ObservableProperty] private uint _layoutCountRight;
    [ObservableProperty] private uint _layoutCountBottom;

    [ObservableProperty] private string _controllerAddress;
    [ObservableProperty] private ushort _controllerPort;

    private readonly StripSettings _stripSettings;

    public StripViewModel(StripSettings stripSettings)
    {
        _stripSettings = stripSettings;

        _layoutCountTop = stripSettings.StripLayoutSettings.LedCountTop;
        _layoutCountLeft = stripSettings.StripLayoutSettings.LedCountLeft;
        _layoutCountRight = stripSettings.StripLayoutSettings.LedCountRight;
        _layoutCountBottom = stripSettings.StripLayoutSettings.LedCountBottom;

        _controllerAddress = stripSettings.ConnectionSettings.ControllerAddress;
        _controllerPort = stripSettings.ConnectionSettings.ControllerPort;

        var prevEffectSettings = _stripSettings.CachedEffectSettings.Items;
        var settingsMap = prevEffectSettings.ToDictionary(e => e.GetType());
        foreach (var effectInAssembly in EffectSettingsInAssembly())
        {
            settingsMap.TryAdd(effectInAssembly.GetType(), effectInAssembly);
        }

        // Some effect could be platform dependent. Do not offer them.
        var availableEffects = settingsMap.Values.Where(o => o.IsConstructable());

        // Settings are ordered in alphabetical order except the Off option, which should always be the first element.
        _effectItems = availableEffects
            .Select(o => o.ToEffectSettingViewModel())
            .OrderBy(o => o is not EffectOffViewModel)
            .ThenBy(o => o.EffectName)
            .Select(o => new StripEffectModel(o))
            .ToList();

        // Try to select the previously selected effect which may no longer be available
        if (_stripSettings.ActiveEffectTypeName is { } effectName)
        {
            _selectedEffectItem =
                _effectItems.FirstOrDefault(e => effectName == e.ViewModel.GetEffectSettings().GetType().Name);
        }

        // Otherwise choose first (Off)
        _selectedEffectItem ??= _effectItems.FirstOrDefault();

        // Update storage value. Previously selected effect could not longer be available.
        OnPropertyChanged();
    }


    protected override void UpdateStorageValues()
    {
        _stripSettings.ConnectionSettings.ControllerAddress = ControllerAddress;
        _stripSettings.ConnectionSettings.ControllerPort = ControllerPort;

        _stripSettings.StripLayoutSettings.LedCountLeft = LayoutCountLeft;
        _stripSettings.StripLayoutSettings.LedCountTop = LayoutCountTop;
        _stripSettings.StripLayoutSettings.LedCountRight = LayoutCountRight;
        _stripSettings.StripLayoutSettings.LedCountBottom = LayoutCountBottom;

        if (SelectedEffectItem is not null)
        {
            _stripSettings.ActiveEffectTypeName = SelectedEffectItem.ViewModel.GetEffectSettings().GetType().Name;
            _stripSettings.CachedEffectSettings.WriteEffect(SelectedEffectItem.ViewModel.GetEffectSettings());
        }
    }

    private static IEnumerable<Models.EffectSettings> EffectSettingsInAssembly()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes()
            .Where(t => typeof(Models.EffectSettings).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false });

        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is Models.EffectSettings effectSettingViewModel)
            {
                yield return effectSettingViewModel;
            }
        }
    }
}

public partial class StripEffectModel : ObservableObject
{
    [ObservableProperty] private EffectSettingViewModel _viewModel;

    public StripEffectModel(EffectSettingViewModel viewModel)
    {
        ViewModel = viewModel;
    }
}