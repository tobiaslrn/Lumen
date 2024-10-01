using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Lumen.Desktop.Models;
using Lumen.Service.Utilities;

namespace Lumen.Desktop.ViewModels.TabPages;

public partial class ApplicationViewModel : StorageBasedViewModel
{
    [ObservableProperty] private bool _startAppOnBoot;
    [ObservableProperty] private bool _startAppOnBootSupported;

    private readonly ApplicationSettings _applicationSettings;

    public ApplicationViewModel(ApplicationSettings settings)
    {
        _applicationSettings = settings;
        _startAppOnBootSupported = AutoStartService.CurrentPlatformIsSupported();
        if (_startAppOnBootSupported)
        {
            _startAppOnBoot = AutoStartService.IsSetToAutoStart(Resources.APP_NAME);
        }
    }

    public void SetLaunchOnBootToCommand(bool value)
    {
        if (value)
        {
            string executable = Process.GetCurrentProcess().MainModule!.FileName;
            AutoStartService.SetToAutoStart(Resources.APP_NAME, executable);
        }
        else
        {
            AutoStartService.RemoveFromAutoStart(Resources.APP_NAME);
        }
    }

    protected override void UpdateStorageValues()
    {
        _applicationSettings.StartAppAtBoot = StartAppOnBoot;
    }
}