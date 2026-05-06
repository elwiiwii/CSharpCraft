using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Map;

internal static class MapOps
{
    internal static (int i, int j) GetMCoord(F32 x, F32 y)
        => (F32.FloorToInt(x / F32.FromInt(16)),
            F32.FloorToInt(y / F32.FromInt(16)));

    internal static Tile GetDirectTile(int i, int j, Level level)
    {
        if (OutOfBounds(i, j, level))
            return PcraftData.TilePrototypes[(int)TileId.Water];
        var tile = level.Map[i, j];
        return tile.Floor is null ? PcraftData.TilePrototypes[(int)TileId.Water] : tile;
    }

    internal static Tile GetTile(F32 x, F32 y, Level level)
    {
        var (i, j) = GetMCoord(x, y);
        return GetDirectTile(i, j, level);
    }

    internal static void SetTile(F32 x, F32 y, Tile tile, Level level)
    {
        var (i, j) = GetMCoord(x, y);
        if (OutOfBounds(i, j, level)) return;
        level.SetTile(i, j, tile);
    }

    internal static bool OutOfBounds(int i, int j, Level level)
        => i < 0 || j < 0 || i >= level.Sx || j >= level.Sy;

    internal static bool IsFree(F32 x, F32 y, Level level)
    {
        var tile = GetTile(x, y, level);
        return tile.Surface is null;
    }

    internal static bool IsFreeEnem(F32 x, F32 y, Level level)
    {
        var tile = GetTile(x, y, level);
        return tile.Surface is null && tile.Floor != PcraftData.FtWater;
    }

    internal static bool IsCool(F32 x, F32 y, Level level)
        => !IsFree(x, y, level);
}
