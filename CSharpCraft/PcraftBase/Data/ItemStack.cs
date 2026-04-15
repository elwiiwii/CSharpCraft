namespace CSharpCraft.PcraftBase.Data;

internal sealed class ItemStack(ItemDef type, int? count = null, List<Recipe>? list = null)
{
    public ItemDef Type { get; } = type;
    internal int? Count { get; set; } = count;
    internal List<Recipe>? List { get; } = list;
    internal int? Power { get; set; }
}
