using System.Text.Json.Serialization;
using Lumen.Service;

namespace Lumen.Desktop.Models;

public class StripLayoutSettings(uint ledCountRight, uint ledCountTop, uint ledCountLeft, uint ledCountBottom)
{
    [JsonPropertyName("Right")] public uint LedCountRight { get; set; } = ledCountRight;
    [JsonPropertyName("Top")] public uint LedCountTop { get; set; } = ledCountTop;
    [JsonPropertyName("Left")] public uint LedCountLeft { get; set; } = ledCountLeft;
    [JsonPropertyName("Bottom")] public uint LedCountBottom { get; set; } = ledCountBottom;

    public StripLayout AsLayout()
    {
        return new StripLayout((int)LedCountRight, (int)LedCountTop, (int)LedCountLeft, (int)LedCountBottom);
    }
}