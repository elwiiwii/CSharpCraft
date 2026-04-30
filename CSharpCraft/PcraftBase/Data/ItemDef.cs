namespace CSharpCraft.PcraftBase.Data;

internal class ItemDef(string name, int spr, int[]? pal = null)
{
    internal string Name { get; } = name;
    internal int Spr { get; } = spr;
    internal int[]? Pal { get; } = pal;
}
