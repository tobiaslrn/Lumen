using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Lumen.Desktop.ViewModels;
using Lumen.Desktop.Views;

namespace Lumen.Desktop;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            DataContext = new MainWindowViewModel();
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Open(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow() { DataContext = DataContext };
            desktop.MainWindow.Show();
            desktop.MainWindow.Topmost = true;
            desktop.MainWindow.Deactivated += MainWindowOnDeactivated;
        }
    }

    private void MainWindowOnDeactivated(object? o, EventArgs eventArgs)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow?.Close();
            desktop.MainWindow = null;
        }

        GC.Collect();
    }

    public void Exit(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow?.Close();
            desktop.MainWindow = null;
            desktop.Shutdown();
        }
    }
}