namespace CSharpCraft.Pcraft.Data;

internal sealed class ItemStack(ItemDef type, int? count = null, List<Recipe>? list = null) : IInventorySlot
{
    public ItemDef Type { get; } = type;
    internal int? Count { get; set; } = count;
    internal List<Recipe>? List { get; } = list;
    internal int? Power { get; set; }
}
