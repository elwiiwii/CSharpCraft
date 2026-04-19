using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Menu;

internal sealed class CraftingMenu : IMenu
{
    internal ItemDef     BenchType { get; }
    internal List<Recipe> Recipes  { get; }
    internal int Sel { get; set; } = 0;
    internal int Off { get; set; } = 0;

    internal CraftingMenu(ItemDef benchType, List<Recipe> recipes, List<ItemStack> playerInvent)
    {
        BenchType = benchType    ?? throw new ArgumentNullException(nameof(benchType));
        Recipes   = recipes      ?? throw new ArgumentNullException(nameof(recipes));
        _         = playerInvent ?? throw new ArgumentNullException(nameof(playerInvent));
    }

    public void Update(PlayerEntity player, PcraftGame game)
    {
        if (Recipes.Count > 0)
        {
            if (Pico8.Btnp(3)) { Sel += 1; Pico8.Sfx(18); }
            if (Pico8.Btnp(2)) { Sel -= 1; Pico8.Sfx(18); }
            Sel = PcraftServices.Loop(Sel, Recipes.Count);

            if (Pico8.Btnp(5))
            {
                var recipe = Recipes[Sel];
                if (PcraftServices.CanCraft(player.Invent, recipe))
                {
                    PcraftServices.Craft(player.Invent, recipe);
                    Pico8.Sfx(16);
                }
            }
        }

        if (Pico8.Btnp(4) && !player.Lb4)
        {
            player.CurMenu = null;
            Pico8.Sfx(17);
        }
    }

    public void Draw(PlayerEntity player, Level level) => PcraftServices.DrawCraftingPanels(this, player);
}
