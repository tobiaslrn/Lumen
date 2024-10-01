namespace Lumen.Service.Effect;

public interface IEffect : IDisposable
{
    TimeSpan RequestedRefreshRate();
    StripFrame? GetEffectValue();
}