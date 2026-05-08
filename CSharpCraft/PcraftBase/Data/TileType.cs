namespace CSharpCraft.PcraftBase.Data;

internal class TileType(BlendGroup blendGroup, int[]? spritePal = null)
{
    internal BlendGroup BlendGroup { get; } = blendGroup;
    internal int[]? SpritePal { get; } = spritePal;
}

internal sealed class WallTileType(
    BlendGroup blendGroup, int[]? spritePal,
    ItemDef mat, TileType underlyingType, int life)
    : TileType(blendGroup, spritePal)
{
    internal ItemDef Mat { get; } = mat;
    internal TileType UnderlyingType { get; } = underlyingType;
    internal int Life { get; } = life;
}
