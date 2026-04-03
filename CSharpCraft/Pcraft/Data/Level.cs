namespace CSharpCraft.Pcraft.Data;

internal sealed class Level(int x, int y, int sx, int sy, bool isUnder)
{
    internal int X { get; } = x;
    internal int Y { get; } = y;
    internal int Sx { get; } = sx;
    internal int Sy { get; } = sy;
    internal bool IsUnder { get; } = isUnder;
    internal List<ItemEntity> Ent { get; } = [];
    internal List<CharacterEntity> Ene { get; } = [];
    internal Dictionary<int, F32> Dat { get; } = [];
    internal F32 Stx { get; set; }
    internal F32 Sty { get; set; }
}
