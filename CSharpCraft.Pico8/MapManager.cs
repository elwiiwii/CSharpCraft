namespace CSharpCraft.Pico8;

/// <summary>
/// Manages Pico-8 map and flag data.
/// Encapsulates map tile access and flag checking.
/// </summary>
public class MapManager : IMapManager
{
    private readonly int[] _mapData;
    private readonly int[] _flagData;
    private readonly (int x, int y) _mapDimensions;

    public MapManager(int[] mapData, int[] flagData, (int x, int y) mapDimensions)
    {
        _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        _flagData = flagData ?? throw new ArgumentNullException(nameof(flagData));
        _mapDimensions = mapDimensions;
    }

    public int Mget(double celx, double cely)
    {
        int xFlr = Math.Abs((int)Math.Floor(celx));
        int yFlr = Math.Abs((int)Math.Floor(cely));

        if (xFlr < 0 || yFlr < 0 || xFlr >= _mapDimensions.x || yFlr >= _mapDimensions.y)
            return 0;

        return _mapData[xFlr + yFlr * _mapDimensions.x];
    }

    public void Mset(double celx, double cely, double snum = 0)
    {
        int xFlr = (int)Math.Floor(celx);
        int yFlr = (int)Math.Floor(cely);
        int sFlr = (int)Math.Floor(snum);

        if (xFlr < 0 || yFlr < 0 || xFlr >= _mapDimensions.x || yFlr >= _mapDimensions.y)
            return;

        _mapData[xFlr + yFlr * _mapDimensions.x] = sFlr;
    }

    public int Fget(int n)
    {
        if (n < 0 || n >= _flagData.Length) return 0;
        return _flagData[n];
    }

    public int[] GetMapData() => _mapData;
}
