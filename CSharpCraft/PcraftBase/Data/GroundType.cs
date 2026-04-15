namespace CSharpCraft.PcraftBase.Data;

internal sealed class GroundType(int id, int gr)
{
    internal int Id { get; } = id;
    internal int Gr { get; } = gr;
    internal ItemDef? Mat { get; init; }
    internal GroundType? Tile { get; init; }
    internal int Life { get; init; }
    internal bool IsTree { get; init; }
    internal int[]? Pal { get; init; }
}
