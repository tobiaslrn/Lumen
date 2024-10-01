using System.Numerics;

namespace Lumen.Service.Utilities;

public readonly record struct Range<T>(T Start, T End) where T : ISubtractionOperators<T, T, T>
{
    public T Length => End - Start;
};