using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Map;

internal static class MapOps
{
    internal static (int i, int j) GetMCoord(F32 x, F32 y)
    {
        return (F32.FloorToInt(x / F32.FromInt(16)),
                F32.FloorToInt(y / F32.FromInt(16)));
    }

    internal static Tile GetDirectTile(int i, int j, Level level)
    {
        if (OutOfBounds(i, j, level))
            return new Tile(PcraftData.TileWater);
        Tile tile = level.Map[i, j];
        return tile.Type is null ? new Tile(PcraftData.TileWater) : tile;
    }

    internal static Tile GetTile(F32 x, F32 y, Level level)
    {
        (int i, int j) = GetMCoord(x, y);
        return GetDirectTile(i, j, level);
    }

    internal static void SetTile(F32 x, F32 y, Tile tile, Level level)
    {
        (int i, int j) = GetMCoord(x, y);
        if (OutOfBounds(i, j, level)) return;
        level.SetTile(i, j, tile);
    }

    internal static bool OutOfBounds(int i, int j, Level level)
    {
        return i < 0 || j < 0 || i >= level.Sx || j >= level.Sy;
    }

    internal static bool IsFree(F32 x, F32 y, Level level)
    {
        Tile tile = GetTile(x, y, level);
        return tile.Type is not WallTileType;
    }

    internal static bool IsFreeEnem(F32 x, F32 y, Level level)
    {
        Tile tile = GetTile(x, y, level);
        return tile.Type is not WallTileType && tile.Type.BlendGroup != BlendGroup.Water;
    }

    internal static bool IsCool(F32 x, F32 y, Level level)
    {
        return !IsFree(x, y, level);
    }
}
