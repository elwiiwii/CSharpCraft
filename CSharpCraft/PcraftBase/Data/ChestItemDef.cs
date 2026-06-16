namespace CSharpCraft.PcraftBase.Data;

internal sealed class ChestItemDef(string name, int spr, int bigSpr, int[]? pal = null) : PlaceableItemDef(name, spr, bigSpr, pal);
