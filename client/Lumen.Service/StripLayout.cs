// ReSharper disable RedundantExplicitPositionalPropertyDeclaration
// ReSharper disable RedundantReadonlyModifier
// ReSharper disable UnusedMember.Global

using Lumen.Service.Utilities;

namespace Lumen.Service;

public readonly record struct StripLayout(int Right, int Top, int Left, int Bottom)
{
    public readonly int Right { get; init; } = Right;
    public readonly int Top { get; init; } = Top;
    public readonly int Left { get; init; } = Left;
    public readonly int Bottom { get; init; } = Bottom;

    public readonly int Count = Right + Top + Left + Bottom;
    public readonly int Width = Math.Max(Right, Left);
    public readonly int Height = Math.Max(Bottom, Top);

    public readonly Range<int> RRight = new(0, Right);
    public readonly Range<int> RTop = new(Right, Right + Top);
    public readonly Range<int> RLeft = new(Right + Top, Right + Top + Left);
    public readonly Range<int> RBottom = new(Right + Top + Left, Right + Top + Left + Bottom);
}