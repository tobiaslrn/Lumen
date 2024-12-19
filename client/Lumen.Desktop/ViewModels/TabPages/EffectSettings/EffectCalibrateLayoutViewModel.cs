using Lumen.Desktop.Models;

namespace Lumen.Desktop.ViewModels.TabPages.EffectSettings;

public class EffectCalibrateLayoutViewModel : EffectSettingViewModel
{
    public override string EffectName => "Calibrate Layout";

    public override Models.EffectSettings GetEffectSettings()
    {
        return new EffectCalibrateLayoutSettings();
    }

    protected override void UpdateStorageValues()
    {
    }
}