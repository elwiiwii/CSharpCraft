namespace CSharpCraft.Pcraft.Data;

internal sealed class ItemDef(string name, int spr, int[]? pal = null, bool beCraft = false)
{
    internal string Name { get; } = name;
    internal int Spr { get; } = spr;
    internal int[]? Pal { get; } = pal;
    internal bool BeCraft { get; } = beCraft;
    internal int GiveLife { get; set; }
    internal int BigSpr { get; set; }
    internal bool Drop { get; set; }
}
