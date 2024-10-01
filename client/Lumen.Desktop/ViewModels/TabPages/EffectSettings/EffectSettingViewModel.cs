namespace Lumen.Desktop.ViewModels.TabPages.EffectSettings;

public abstract class EffectSettingViewModel : StorageBasedViewModel
{
    public abstract string EffectName { get; }
    public abstract Models.EffectSettings GetEffectSettings();
}