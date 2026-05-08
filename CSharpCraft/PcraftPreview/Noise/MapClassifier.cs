namespace CSharpCraft.PcraftPreview.Noise;

internal class MapClassifier
{
    private readonly SeededNoiseGrid _cur;
    private readonly SeededNoiseGrid _cur2;
    private readonly SeededNoiseGrid _cur3;
    private readonly SeededNoiseGrid _cur4;
    private readonly int _gridSx;
    private readonly int _gridSy;
    private readonly int _a;
    private readonly int _b;
    private readonly int _c;
    private readonly int _d;
    private readonly int _e;

    internal MapClassifier(
        SeededNoiseGrid cur,
        SeededNoiseGrid cur2,
        SeededNoiseGrid cur3,
        SeededNoiseGrid cur4,
        int gridSx,
        int gridSy,
        int a, int b, int c, int d, int e)
    {
        _cur   = cur   ?? throw new ArgumentNullException(nameof(cur));
        _cur2  = cur2  ?? throw new ArgumentNullException(nameof(cur2));
        _cur3  = cur3  ?? throw new ArgumentNullException(nameof(cur3));
        _cur4  = cur4  ?? throw new ArgumentNullException(nameof(cur4));
        _gridSx = gridSx;
        _gridSy = gridSy;
        _a = a;
        _b = b;
        _c = c;
        _d = d;
        _e = e;
    }

    internal virtual int ClassifyTile(int i, int j)
    {
        var (coast, v2, v3) = ComputeIntermediate(i, j);
        return ClassifyFromValues(coast, v2, v3);
    }

    protected (double coast, double v2, double v3) ComputeIntermediate(int i, int j)
    {
        double vCur  = _cur.GetValue(i, j);
        double vCur2 = _cur2.GetValue(i, j);
        double vCur3 = _cur3.GetValue(i, j);
        double vCur4 = _cur4.GetValue(i, j);

        double v  = Math.Abs(vCur - vCur2);
        double v2 = Math.Abs(vCur - vCur3);
        double v3 = Math.Abs(vCur - vCur4);

        double di   = Math.Abs((double)i / _gridSx - 0.5) * 2.0;
        double dj   = Math.Abs((double)j / _gridSy - 0.5) * 2.0;
        double dist = Math.Pow(Math.Max(di, dj), 4.0);

        double coast = v * 4.0 - dist * 4.0;

        return (coast, v2, v3);
    }

    protected int ClassifyFromValues(double coast, double v2, double v3)
    {
        int id = _a;

        if (coast > 0.3)
            id = _b;

        if (coast > 0.6)
            id = _c;

        if (coast > 0.3 && v2 > 0.5)
            id = _d;

        if (id == _c && v3 > 0.5)
            id = _e;

        return id;
    }
}
