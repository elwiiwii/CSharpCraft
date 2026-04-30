namespace CSharpCraft.PcraftBase.Data;

internal sealed class BenchItemDef(string name, int spr, int[]? pal = null) : PlaceableItemDef(name, spr, pal)
{
    internal IReadOnlyList<Recipe> Recipes { get; set; } = [];
}
