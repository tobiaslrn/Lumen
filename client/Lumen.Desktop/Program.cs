using Avalonia;
using System;

namespace Lumen.Desktop;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            // Uses high cpu usage but half the memory. Acceptable since the window is only open for short times.
            .With(new Win32PlatformOptions { RenderingMode = [Win32RenderingMode.Software] })
            .LogToTrace();
}