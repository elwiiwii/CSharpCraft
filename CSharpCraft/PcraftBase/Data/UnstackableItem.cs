namespace CSharpCraft.PcraftBase.Data;

internal sealed class UnstackableItem(ItemDef type) : InventorySlot
{
    internal override ItemDef Type { get; } = type;
}
