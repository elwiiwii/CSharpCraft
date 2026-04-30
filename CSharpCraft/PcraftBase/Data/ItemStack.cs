namespace CSharpCraft.PcraftBase.Data;

internal sealed class ItemStack(ItemDef type, int? count = null)
{
    public ItemDef Type { get; } = type;
    internal int? Count { get; set; } = count;
    internal int? Power { get; set; }
}
