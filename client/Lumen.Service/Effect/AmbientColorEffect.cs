﻿using HPPH;
using Lumen.Service.ControllerMessages;
using Lumen.Service.Utilities;
using ScreenCapture.NET;

namespace Lumen.Service.Effect;

public class AmbientColorEffect : IEffect
{
    private static IScreenCaptureService? _screenCaptureService;

    private readonly StripLayout _layout;
    private readonly uint _fps;
    private readonly ICaptureZone _fullscreen;
    private readonly IScreenCapture _screenCapture;
    private readonly LinkedList<StripFrame> _previousFrames;

    private static IScreenCaptureService ScreenCaptureService
    {
        get { return _screenCaptureService ??= WeakScreenCaptureService.Instance; }
    }


    public AmbientColorEffect(StripLayout layout, string monitor, int detailLevel, int smoothingFrames, uint fps)
    {
        _layout = layout;
        _fps = fps;
        _previousFrames = [];
        for (int i = 0; i < smoothingFrames; i++)
        {
            _previousFrames.AddLast(new StripFrame(layout));
        }

        var gpus = ScreenCaptureService.GetGraphicsCards();
        var selectedGpu = gpus.First();

        var displays = ScreenCaptureService.GetDisplays(selectedGpu);
        var selectedDisplay = displays.First(d => d.DeviceName == monitor);

        _screenCapture = ScreenCaptureService.GetScreenCapture(selectedDisplay);

        _fullscreen = _screenCapture.RegisterCaptureZone(
            0,
            0,
            _screenCapture.Display.Width,
            _screenCapture.Display.Height,
            detailLevel
        );
    }

    public void Dispose()
    {
        _screenCapture.UnregisterCaptureZone(_fullscreen);
    }

    public TimeSpan RequestedRefreshRate()
    {
        if (_fps == 0)
            return TimeSpan.Zero;

        var secondsPerFrame = 1.0 / _fps;
        return TimeSpan.FromSeconds(secondsPerFrame);
    }

    public StripFrame? GetEffectValue()
    {
        if (!_screenCapture.CaptureScreen())
        {
            return null;
        }

        _previousFrames.RemoveFirst();

        var workingFrame = ReadFrameFromCaptureZone(_fullscreen, _layout);

        _previousFrames.AddLast(workingFrame);

        var savedFrames = _previousFrames.ToArray();

        if (_previousFrames.Count == 1)
            return _previousFrames.First();

        var average = AverageFrameValues(savedFrames);
        return average;
    }

    public static IEnumerable<string> MonitorNames()
    {
        var gpus = ScreenCaptureService.GetGraphicsCards();
        var selectedGpu = gpus.First();
        var displays = ScreenCaptureService.GetDisplays(selectedGpu);
        return displays.Select(d => d.DeviceName);
    }

    public static IEnumerable<string> GpuNames()
    {
        var gpus = ScreenCaptureService.GetGraphicsCards();
        return gpus.Select(d => d.Name);
    }

    public static Range<int> DetailLevelRange(string displayName)
    {
        var gpus = ScreenCaptureService.GetGraphicsCards();
        var selectedGpu = gpus.First();
        var displays = ScreenCaptureService.GetDisplays(selectedGpu);
        var selectedDisplay = displays.First(d => d.DeviceName == displayName);

        var width = selectedDisplay.Width;
        var height = selectedDisplay.Height;
        var downscaleLevel = 0;
        while (true)
        {
            if (width < 1 || height < 1)
            {
                return new Range<int>(0, downscaleLevel);
            }

            downscaleLevel++;
            width /= 2;
            height /= 2;
        }
    }

    private static StripFrame ReadFrameFromCaptureZone(ICaptureZone zone, StripLayout layout)
    {
        var output = new StripFrame(layout);
        using (zone.Lock())
        {
            var image = zone.Image;
            var topRow = image.Rows.First();
            var bottomRow = image.Rows.Last();
            var leftCol = image.Columns.First();
            var rightCol = image.Columns.Last();

            for (var index = 0; index < output.Right.Length; index++)
            {
                var captureIndex = Compress(index, 0, output.Right.Length, 0, rightCol.Length);
                output.Right[index] = rightCol[captureIndex].ToRgb8();
            }

            for (var index = 0; index < output.Left.Length; index++)
            {
                var captureIndex = Compress(index, 0, output.Left.Length, 0, leftCol.Length);
                output.Left[index] = leftCol[captureIndex].ToRgb8();
            }

            for (var index = 0; index < output.Top.Length; index++)
            {
                var captureIndex = Compress(index, 0, output.Top.Length, 0, topRow.Length);
                output.Top[index] = topRow[captureIndex].ToRgb8();
            }

            for (var index = 0; index < output.Bottom.Length; index++)
            {
                var captureIndex = Compress(index, 0, output.Bottom.Length, 0, bottomRow.Length);
                output.Bottom[index] = bottomRow[captureIndex].ToRgb8();
            }
        }

        return output;
    }

    private static StripFrame AverageFrameValues(StripFrame[] savedFrames)
    {
        var output = new StripFrame(savedFrames.First().Layout);
        var totalWeights = savedFrames.Length + TotalWeight(savedFrames.Length + 1);
        for (var i = 0; i < output.Leds.Length; i++)
        {
            float r = 0;
            float g = 0;
            float b = 0;

            int weight = 1;
            foreach (var frame in savedFrames)
            {
                var item = frame.Leds[i];
                r += item.R * weight;
                g += item.G * weight;
                b += item.B * weight;
                weight += 1;
            }

            r /= totalWeights;
            g /= totalWeights;
            b /= totalWeights;

            output.Leds[i] = new Rgb8((byte)r, (byte)g, (byte)b);
        }

        return output;
    }


    private static int Compress(int n, int min, int max, int nMin, int nMax)
    {
        return (n - min) * (nMax - nMin) / (max - min) + nMin;
    }

    private static int TotalWeight(int f)
    {
        var result = 1;
        for (var i = f; i > 0; i--)
        {
            result += i;
        }

        return result;
    }
}

/// <summary>
/// Windows only allows one <see cref="DX11ScreenCaptureService"/> per Process.
/// Therefore we have to reuse the same one, but release it when we don't need it anymore.
/// </summary>
public static class WeakScreenCaptureService
{
    private static WeakReference<IScreenCaptureService>? _ref;
    private static readonly object Lock = new();

    public static IScreenCaptureService Instance
    {
        get
        {
            IScreenCaptureService instance;
            lock (Lock)
            {
                if (_ref == null || !_ref.TryGetTarget(out instance!))
                {
                    instance = new DX11ScreenCaptureService();
                    _ref = new WeakReference<IScreenCaptureService>(instance);
                }
            }

            return instance;
        }
    }
}

public static class ColorExtensions
{
    public static Rgb8 ToRgb8(this IColor color)
    {
        return new Rgb8(color.R, color.G, color.B);
    }
}