namespace CSharpCraft.PcraftBase.Data;

internal sealed class Recipe(InventorySlot output, IReadOnlyList<StackableItem> req)
{
    internal InventorySlot Output { get; } = output ?? throw new ArgumentNullException(nameof(output));
    internal IReadOnlyList<StackableItem> Req { get; } = req ?? throw new ArgumentNullException(nameof(req));
}
