namespace CSharpCraft.Pico8;

/// <summary>
/// Manages Pico-8 map and flag data.
/// Encapsulates map tile access and flag checking.
/// </summary>
public class MapManager : IMapManager
{
    private readonly int[] mapData;
    private readonly int[] flagData;
    private readonly (int x, int y) mapDimensions;

    public MapManager(
        int[] mapData,
        int[] flagData,
        (int x, int y) mapDimensions)
    {
        this.mapData = mapData;
        this.flagData = flagData;
        this.mapDimensions = mapDimensions;
    }

    public int Mget(double celx, double cely)
    {
        int xFlr = Math.Abs((int)Math.Floor(celx));
        int yFlr = Math.Abs((int)Math.Floor(cely));

        if (xFlr < 0 || yFlr < 0 || xFlr >= mapDimensions.x || yFlr >= mapDimensions.y)
            return 0;

        return mapData[xFlr + yFlr * mapDimensions.x];
    }

    public void Mset(double celx, double cely, double snum = 0)
    {
        int xFlr = (int)Math.Floor(celx);
        int yFlr = (int)Math.Floor(cely);
        int sFlr = (int)Math.Floor(snum);

        if (xFlr < 0 || yFlr < 0 || xFlr >= mapDimensions.x || yFlr >= mapDimensions.y)
            return;

        mapData[xFlr + yFlr * mapDimensions.x] = sFlr;
    }

    public int Fget(int n)
    {
        if (n < 0 || n >= flagData.Length) return 0;
        return flagData[n];
    }
}
