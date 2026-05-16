using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftSeeded.Noise;

internal class MapClassifier
{
    private readonly SeededNoiseGrid _cur;
    private readonly SeededNoiseGrid _cur2;
    private readonly SeededNoiseGrid _cur3;
    private readonly SeededNoiseGrid _cur4;

    internal int GridSx { get; }
    internal int GridSy { get; }
    internal int TileA { get; }
    internal int TileB { get; }
    internal int TileC { get; }
    internal int TileD { get; }
    internal int TileE { get; }
    internal bool GenerateHole { get; }

    internal MapClassifier(
        SeededNoiseGrid cur,
        SeededNoiseGrid cur2,
        SeededNoiseGrid cur3,
        SeededNoiseGrid cur4,
        int gridSx,
        int gridSy,
        int a, int b, int c, int d, int e,
        bool generateHole = false)
    {
        _cur = cur ?? throw new ArgumentNullException(nameof(cur));
        _cur2 = cur2 ?? throw new ArgumentNullException(nameof(cur2));
        _cur3 = cur3 ?? throw new ArgumentNullException(nameof(cur3));
        _cur4 = cur4 ?? throw new ArgumentNullException(nameof(cur4));
        GridSx = gridSx;
        GridSy = gridSy;
        TileA = a;
        TileB = b;
        TileC = c;
        TileD = d;
        TileE = e;
        GenerateHole = generateHole;
    }

    internal virtual int ClassifyTile(int i, int j)
    {
        if (FixedTileAt(i, j) is { } f) return f;
        (double coast, double v2, double v3) = ComputeIntermediate(i, j);
        return ClassifyFromValues(coast, v2, v3);
    }

    /// <summary>
    /// Returns a fixed tile id for the ladder structure at the grid centre,
    /// or <see langword="null"/> for all other cells.
    /// The 3×3 area around (GridSx/2, GridSy/2) is always Rock (3); the
    /// centre cell itself is always Hole (11).
    /// </summary>
    protected int? FixedTileAt(int i, int j)
    {
        if (!GenerateHole) return null;
        int cx = GridSx / 2;
        int cy = GridSy / 2;
        return Math.Abs(i - cx) > 1 || Math.Abs(j - cy) > 1
            ? null
            : (i == cx && j == cy)
                ? (int)TileId.Hole
                : (int)TileId.Rock;
    }

    /// <summary>
    /// Returns true if the cell is occupied by a fixed ladder tile that cannot be biased.
    /// </summary>
    internal bool IsFixedTile(int i, int j)
    {
        return FixedTileAt(i, j).HasValue;
    }

    /// <summary>
    /// Exposes <see cref="ComputeIntermediate"/> to non-subclass internal callers.
    /// Returns the raw (coast, v2, v3) triple before any bias is applied.
    /// </summary>
    internal (double coast, double v2, double v3) GetIntermediate(int i, int j)
    {
        return ComputeIntermediate(i, j);
    }

    protected (double coast, double v2, double v3) ComputeIntermediate(int i, int j)
    {
        double vCur = _cur.GetValue(i, j);
        double vCur2 = _cur2.GetValue(i, j);
        double vCur3 = _cur3.GetValue(i, j);
        double vCur4 = _cur4.GetValue(i, j);

        double v = Math.Abs(vCur - vCur2);
        double v2 = Math.Abs(vCur - vCur3);
        double v3 = Math.Abs(vCur - vCur4);

        double di = Math.Abs(((double)i / GridSx) - 0.5) * 2.0;
        double dj = Math.Abs(((double)j / GridSy) - 0.5) * 2.0;
        double dist = Math.Pow(Math.Max(di, dj), 4.0);

        double coast = (v * 4.0) - (dist * 4.0);

        return (coast, v2, v3);
    }

    protected int ClassifyFromValues(double coast, double v2, double v3)
    {
        int id = TileA;

        if (coast > 0.3)
            id = TileB;

        if (coast > 0.6)
            id = TileC;

        if (coast > 0.3 && v2 > 0.5)
            id = TileD;

        if (id == TileC && v3 > 0.5)
            id = TileE;

        return id;
    }
}
