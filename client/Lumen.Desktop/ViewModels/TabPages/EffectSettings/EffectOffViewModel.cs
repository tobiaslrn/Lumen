using Lumen.Desktop.Models;

namespace Lumen.Desktop.ViewModels.TabPages.EffectSettings;

public class EffectOffViewModel : EffectSettingViewModel
{
    public override string EffectName => "Off";

    public override Models.EffectSettings GetEffectSettings()
    {
        return new EffectOffSettings();
    }

    protected override void UpdateStorageValues()
    {
    }
}