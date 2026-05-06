namespace CSharpCraft.PcraftBase.Data;

internal class SurfaceType(TileId id, int gr, ItemDef mat, FloorType underlyingFloor, int life)
{
    internal TileId    Id              { get; } = id;
    internal int       Gr              { get; } = gr;
    internal ItemDef   Mat             { get; } = mat;
    internal FloorType UnderlyingFloor { get; } = underlyingFloor;
    internal int       Life            { get; } = life;
}

internal sealed class OverlaySurface(TileId id, int gr, ItemDef mat, FloorType underlyingFloor, int life, int[] pal)
    : SurfaceType(id, gr, mat, underlyingFloor, life)
{
    internal int[] Pal { get; } = pal;
}
