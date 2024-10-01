using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Lumen.Service.Utilities;

public static class AutoStartService
{
    public static bool CurrentPlatformIsSupported()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    public static void SetToAutoStart(string applicationName, string executable)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            InsertAutoStartEntryWindows(applicationName, executable);
        }
        else
        {
            throw new ApplicationException("unsupported os");
        }
    }

    public static void RemoveFromAutoStart(string applicationName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DeleteAutoStartEntryWindows(applicationName);
        }
        else
        {
            throw new ApplicationException("unsupported os");
        }
    }

    public static bool IsSetToAutoStart(string applicationName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return IsStartupItem(applicationName);
        }
        else
        {
            throw new ApplicationException("unsupported os");
        }
    }

    [SupportedOSPlatform("windows")]
    private static void InsertAutoStartEntryWindows(string applicationName, string executablePath)
    {
        RegistryKey? rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        if (!IsStartupItem(applicationName))
            rkApp?.SetValue(applicationName, executablePath);
    }

    [SupportedOSPlatform("windows")]
    private static void DeleteAutoStartEntryWindows(string applicationName)
    {
        RegistryKey? rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        if (IsStartupItem(applicationName))
            rkApp?.DeleteValue(applicationName, false);
    }


    [SupportedOSPlatform("windows")]
    private static bool IsStartupItem(string applicationName)
    {
        RegistryKey? rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        if (rkApp?.GetValue(applicationName) == null)
        {
            // The value doesn't exist, the application is not set to run at startup
            return false;
        }
        else
        {
            // The value exists, the application is set to run at startup
            return true;
        }
    }
}