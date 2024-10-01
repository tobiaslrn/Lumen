using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Lumen.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (Screens.Primary is { } primaryScreen)
        {
            var workingAreaSize = primaryScreen.WorkingArea.Size;
            var windowSize = PixelSize.FromSize(ClientSize, primaryScreen.Scaling);
            var x = workingAreaSize.Width - windowSize.Width - 10;
            var y = workingAreaSize.Height - windowSize.Height - 10;
            Position = new PixelPoint(x, y);
        }
    }

    private void OpenInGithubButton_OnClick(object? sender, RoutedEventArgs e)
    {
        OpenUrl("https://github.com/sprunq/Lumen");
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}