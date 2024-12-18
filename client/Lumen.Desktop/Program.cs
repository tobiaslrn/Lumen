using Avalonia;
using System;
using System.Threading;

namespace Lumen.Desktop;

internal static class Program
{
    private static readonly Mutex SingleInstanceMutex = new(true, "{AE44F249-962E-46BF-8592-57A5717C85B9}");


    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (SingleInstanceMutex.WaitOne(TimeSpan.Zero, true))
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            SingleInstanceMutex.ReleaseMutex();
        }
        else
        {
            Console.Error.WriteLine("Another instance of Lumen is already running.");
        }
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