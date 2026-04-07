namespace CSharpCraft.Pcraft.Data;

internal sealed class MenuState(ItemDef type, List<ItemStack>? list, int spr, string? text, string? text2)
{
    internal ItemDef Type { get; } = type;
    internal List<ItemStack>? List { get; } = list;
    internal List<Recipe>? RecipeList { get; set; }
    internal int Spr { get; } = spr;
    internal string? Text { get; } = text;
    internal string? Text2 { get; } = text2;
    internal int Sel { get; set; } = 0;
    internal int Off { get; set; } = 0;
}
