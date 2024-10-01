using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Lumen.Desktop.Models;

namespace Lumen.Desktop.ViewModels.TabPages.EffectSettings;

public partial class EffectSolidViewModel : EffectSettingViewModel
{
    [ObservableProperty] private Color _selectedColor;


    private readonly EffectSolidSettings _effectSolidSettings;

    public EffectSolidViewModel(EffectSolidSettings effectSolidSettings)
    {
        _effectSolidSettings = effectSolidSettings;
        SelectedColor = Color.FromRgb(_effectSolidSettings.R, _effectSolidSettings.G, _effectSolidSettings.B);
    }

    public override string EffectName => "Solid";

    public override Models.EffectSettings GetEffectSettings()
    {
        return _effectSolidSettings;
    }

    protected override void UpdateStorageValues()
    {
        _effectSolidSettings.R = SelectedColor.R;
        _effectSolidSettings.G = SelectedColor.G;
        _effectSolidSettings.B = SelectedColor.B;
    }
}