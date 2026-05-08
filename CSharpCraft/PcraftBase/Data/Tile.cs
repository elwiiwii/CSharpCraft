namespace CSharpCraft.PcraftBase.Data;

internal record struct Tile(
    TileType Type,
    F32? HarvestLife = null,
    F32? GrowthTimer = null
);
