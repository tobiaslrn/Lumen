using Lumen.Service.ControllerMessages;

namespace Lumen.Service.Effect;

public class LayoutCalibrationEffect(StripLayout layout) : IEffect
{
    public TimeSpan RequestedRefreshRate()
    {
        return TimeSpan.FromSeconds(1);
    }

    public StripFrame GetEffectValue()
    {
        var frame = new StripFrame(layout);
        foreach (ref var v in frame.Left)
            v = Red;
        foreach (ref var v in frame.Right)
            v = Green;
        foreach (ref var v in frame.Top)
            v = Blue;
        foreach (ref var v in frame.Bottom)
            v = Pink;

        return frame;
    }

    public void Dispose()
    {
    }

    private static Rgb8 Red => new(255, 0, 0);
    private static Rgb8 Green => new(0, 255, 0);
    private static Rgb8 Blue => new(0, 0, 255);
    private static Rgb8 Pink => new(255, 0, 255);
}