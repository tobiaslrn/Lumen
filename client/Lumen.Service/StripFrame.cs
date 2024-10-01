using Lumen.Service.ControllerMessages;

namespace Lumen.Service;

public class StripFrame
{
    public readonly Rgb8[] Leds;
    public readonly StripLayout Layout;

    public StripFrame(StripLayout layout)
    {
        Layout = layout;
        Leds = new Rgb8[layout.Count];
    }

    public Span<Rgb8> Right => Leds.AsSpan(Layout.RRight.Start, Layout.RRight.Length);
    public Span<Rgb8> Top => Leds.AsSpan(Layout.RTop.Start, Layout.RTop.Length);
    public Span<Rgb8> Left => Leds.AsSpan(Layout.RLeft.Start, Layout.RLeft.Length);
    public Span<Rgb8> Bottom => Leds.AsSpan(Layout.RBottom.Start, Layout.RBottom.Length);
}