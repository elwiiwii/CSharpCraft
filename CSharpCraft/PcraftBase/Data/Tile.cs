namespace CSharpCraft.PcraftBase.Data;

internal record struct Tile(
    FloorType    Floor,
    SurfaceType? Surface     = null,
    F32?         HarvestLife = null,
    F32?         GrowthTimer = null
);
