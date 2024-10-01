using Lumen.Service.ControllerMessages;

namespace Lumen.Service.Effect;

public class SolidColorEffect(StripLayout layout, Rgb8 color) : IEffect
{
    public TimeSpan RequestedRefreshRate()
    {
        return TimeSpan.FromSeconds(1);
    }

    public StripFrame GetEffectValue()
    {
        var frame = new StripFrame(layout);
        for (var index = 0; index < frame.Leds.Length; index++)
        {
            frame.Leds[index] = color;
        }

        return frame;
    }

    public void Dispose()
    {
    }
}