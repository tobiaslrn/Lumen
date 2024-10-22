using Lumen.Service.Connection;
using Lumen.Service.ControllerMessages;
using Lumen.Service.Effect;
using Microsoft.Extensions.Logging;
using static Lumen.Service.Logging;

namespace Lumen.Service;

public class StripRunner : IDisposable
{
    private readonly IConnection _connection;

    public StripRunner(IConnection connection)
    {
        _connection = connection;
    }

    public async Task RunWithEffect(IEffect effect, CancellationToken cts)
    {
        var postEffect = PostEffectValues(effect, cts);
        var postKeepalive = PostKeepalive(
            new TimeSpan(0, 0, 0, 0, 400),
            new TimeSpan(0, 0, 0, 0, 1000),
            cts
        );
        Logger?.LogInformation("Starting post and keepalive tasks");
        await Task.WhenAll(postEffect, postKeepalive);
        Logger?.LogInformation("Post and keepalive tasks completed");
    }

    private async Task PostEffectValues(IEffect effect, CancellationToken cts)
    {
        var refreshRate = effect.RequestedRefreshRate();
        if ((long)refreshRate.TotalMilliseconds >= uint.MaxValue)
        {
            refreshRate = new TimeSpan(0, 0, 0, 0, int.MaxValue);
        }

        var periodicTimer = new PeriodicTimer(refreshRate);
        while (!cts.IsCancellationRequested)
        {
            var stripFrame = effect.GetEffectValue();
            if (stripFrame != null)
            {
                var msg = new ControllerMessage(DateTimeOffset.Now, new LedStateMessage(stripFrame.Leds));
                _ = _connection.SendMessage(msg, cts);
            }

            await periodicTimer.WaitForNextTickAsync(cts);
        }
    }

    private async Task PostKeepalive(TimeSpan postEvery, TimeSpan keepAliveFor, CancellationToken cts)
    {
        var periodicTimer = new PeriodicTimer(postEvery);
        var duration = (uint)keepAliveFor.TotalMilliseconds;
        while (!cts.IsCancellationRequested)
        {
            var msg = new ControllerMessage(DateTimeOffset.Now, new KeepAliveMessage(duration));
            _ = _connection.SendMessage(msg, cts);
            await periodicTimer.WaitForNextTickAsync(cts);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}