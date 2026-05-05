using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Menu;

internal sealed class CraftingMenu : IMenu
{
    internal BenchItemDef    BenchType { get; }
    internal IReadOnlyList<Recipe> Recipes => BenchType.Recipes;
    internal int Sel { get; set; } = 0;
    internal int Off { get; set; } = 0;

    internal CraftingMenu(BenchItemDef benchType, List<InventorySlot> playerInvent)
    {
        BenchType = benchType    ?? throw new ArgumentNullException(nameof(benchType));
        _         = playerInvent ?? throw new ArgumentNullException(nameof(playerInvent));
    }

    public bool Update(PlayerEntity player)
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
        return false;
    }

    public void Draw(PlayerEntity player, Level level) => PcraftServices.DrawCraftingPanels(this, player);
}
