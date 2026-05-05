namespace CSharpCraft.PcraftBase.Data;

internal sealed class ToolItem(ItemDef type, int power) : InventorySlot
{
    internal override ItemDef Type  { get; } = type;
    internal int               Power { get; } = power;
}
