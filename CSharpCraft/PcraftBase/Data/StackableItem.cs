namespace CSharpCraft.PcraftBase.Data;

internal sealed class StackableItem(ItemDef type, int count) : InventorySlot
{
    internal override ItemDef Type { get; } = type;
    internal int Count { get; set; } = count;
}
