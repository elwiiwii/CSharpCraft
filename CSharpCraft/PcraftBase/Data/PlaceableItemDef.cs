namespace CSharpCraft.PcraftBase.Data;

internal class PlaceableItemDef(string name, int spr, int bigSpr, int[]? pal = null) : ItemDef(name, spr, pal)
{
    internal int BigSpr { get; } = bigSpr;
}
