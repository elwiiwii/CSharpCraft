namespace CSharpCraft.PcraftBase.Data;

internal sealed class HealthItemDef(string name, int spr, int[]? pal = null) : ItemDef(name, spr, pal)
{
    internal int GiveLife { get; init; }
}
