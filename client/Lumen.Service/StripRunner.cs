using Lumen.Service.Connection;
using Lumen.Service.ControllerMessages;
using Lumen.Service.Effect;

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
        await Task.WhenAll(postEffect, postKeepalive);
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
                await _connection.SendMessage(msg, cts);
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
            await _connection.SendMessage(msg, cts);
            await periodicTimer.WaitForNextTickAsync(cts);
        }
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}