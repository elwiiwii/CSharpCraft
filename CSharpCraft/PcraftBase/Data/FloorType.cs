namespace CSharpCraft.PcraftBase.Data;

internal sealed class FloorType(TileId id, int gr)
{
    internal TileId Id { get; } = id;
    internal int    Gr { get; } = gr;
}
