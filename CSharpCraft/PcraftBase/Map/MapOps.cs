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
        return PcraftServices.OutOfBounds(i, j, level)
            ? new Tile(PcraftData.TileWater)
            : level.Map[i, j];
    }

    internal static Tile GetTile(F32 x, F32 y, Level level)
    {
        (int i, int j) = PcraftServices.GetMCoord(x, y);
        return PcraftServices.GetDirectTile(i, j, level);
    }

    internal static void SetTile(F32 x, F32 y, Tile tile, Level level)
    {
        (int i, int j) = PcraftServices.GetMCoord(x, y);
        if (PcraftServices.OutOfBounds(i, j, level)) return;
        level.SetTile(i, j, tile);
    }

    internal static bool OutOfBounds(int i, int j, Level level)
    {
        return i < 0 || j < 0 || i >= level.Sx || j >= level.Sy;
    }

    internal static bool IsFree(F32 x, F32 y, Level level)
    {
        Tile tile = PcraftServices.GetTile(x, y, level);
        return tile.Type is not null and not WallTileType;
    }

    internal static bool IsFreeEnem(F32 x, F32 y, Level level)
    {
        Tile tile = PcraftServices.GetTile(x, y, level);
        return tile.Type is not null and not WallTileType and not { BlendGroup: BlendGroup.Water };
    }
}
