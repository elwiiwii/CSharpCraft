namespace CSharpCraft.PcraftBase.Data;

internal sealed class BenchItemDef(string name, int spr, int bigSpr, int[]? pal = null) : PlaceableItemDef(name, spr, bigSpr, pal)
{
    internal IReadOnlyList<Recipe> Recipes { get; set; } = [];
}
