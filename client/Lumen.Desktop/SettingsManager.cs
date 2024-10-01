using System.IO;
using System.Text.Json;
using Lumen.Desktop.Models;

namespace Lumen.Desktop;

public class SettingsManager(string filePath)
{
    public LumenSettings? LoadSettings() =>
        File.Exists(filePath)
            ? JsonSerializer.Deserialize(File.ReadAllText(filePath), LumenSettingsContext.Default.LumenSettings)
            : null;

    public void SaveSettings(LumenSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, LumenSettingsContext.Default.LumenSettings);
        new FileInfo(filePath).Directory?.Create();
        File.WriteAllText(filePath, json);
    }
}