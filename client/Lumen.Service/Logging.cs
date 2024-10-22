using Microsoft.Extensions.Logging;

namespace Lumen.Service;

/// <summary>
/// This is really ugly but who cares
/// </summary>
public static class Logging
{
    public static ILogger? Logger { get; set; }
}