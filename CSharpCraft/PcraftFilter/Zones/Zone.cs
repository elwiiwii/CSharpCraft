namespace CSharpCraft.PcraftFilter.Zones;

internal abstract record Zone
{
    internal abstract string HashString { get; }
    internal abstract bool Contains(int x, int y);
    internal abstract IEnumerable<(int x, int y)> Cells(int gridSx, int gridSy);
}

internal sealed record RectangleZone(int X, int Y, int Width, int Height) : Zone
{
    internal override string HashString => $"rect.{X}.{Y}.{Width}.{Height}";

    internal override bool Contains(int x, int y)
    {
        return x >= X && x < X + Width && y >= Y && y < Y + Height;
    }

    internal override IEnumerable<(int x, int y)> Cells(int gridSx, int gridSy)
    {
        int x0 = Math.Max(0, X);
        int y0 = Math.Max(0, Y);
        int x1 = Math.Min(gridSx, X + Width);
        int y1 = Math.Min(gridSy, Y + Height);

        for (int x = x0; x < x1; x++)
            for (int y = y0; y < y1; y++)
                yield return (x, y);
    }
}

internal sealed record RadiusZone(int CenterX, int CenterY, int Radius) : Zone
{
    internal override string HashString => $"rad.{CenterX}.{CenterY}.{Radius}";

    internal override bool Contains(int x, int y)
    {
        int dx = x - CenterX;
        int dy = y - CenterY;
        return (dx * dx) + (dy * dy) <= Radius * Radius;
    }

    internal override IEnumerable<(int x, int y)> Cells(int gridSx, int gridSy)
    {
        int x0 = Math.Max(0, CenterX - Radius);
        int y0 = Math.Max(0, CenterY - Radius);
        int x1 = Math.Min(gridSx, CenterX + Radius + 1);
        int y1 = Math.Min(gridSy, CenterY + Radius + 1);

        for (int x = x0; x < x1; x++)
            for (int y = y0; y < y1; y++)
                if (Contains(x, y))
                    yield return (x, y);
    }
}
