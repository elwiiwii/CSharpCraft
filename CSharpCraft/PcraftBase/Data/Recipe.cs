namespace CSharpCraft.PcraftBase.Data;

internal sealed class Recipe(ItemDef type, int? power, int? count, List<ItemStack> req)
{
    internal ItemDef Type { get; } = type;
    internal int? Power { get; } = power;
    internal int? Count { get; } = count;
    internal List<ItemStack> Req { get; } = req;
}
