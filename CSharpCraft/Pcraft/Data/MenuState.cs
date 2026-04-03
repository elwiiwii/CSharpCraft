namespace CSharpCraft.Pcraft.Data;

internal sealed class MenuState(ItemDef type, List<IInventorySlot>? list, int spr, string? text, string? text2)
{
    internal ItemDef Type { get; } = type;
    internal List<IInventorySlot>? List { get; } = list;
    internal int Spr { get; } = spr;
    internal string? Text { get; } = text;
    internal string? Text2 { get; } = text2;
    internal int Sel { get; set; } = 1;
    internal int Off { get; set; } = 0;
}
