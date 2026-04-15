using CSharpCraft.PcraftBase.Crafting;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Draw;
using CSharpCraft.PcraftBase.Inventory;

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

    public void Update(WorldState state, PcraftGame game)
    {
        if (Recipes.Count > 0)
        {
            if (Pico8.Btnp(3)) { Sel += 1; Pico8.Sfx(18); }
            if (Pico8.Btnp(2)) { Sel -= 1; Pico8.Sfx(18); }
            Sel = InventoryOps.Loop(Sel, Recipes.Count);

            if (Pico8.Btnp(5))
            {
                var recipe = Recipes[Sel];
                if (CraftingSystem.CanCraft(state.Invent, recipe))
                {
                    CraftingSystem.Craft(state.Invent, recipe);
                    Pico8.Sfx(16);
                }
            }
        }

        if (Pico8.Btnp(4) && !state.Lb4)
        {
            state.CurMenu = null;
            Pico8.Sfx(17);
        }
    }

    public void Draw(WorldState state) => MenuOverlayDrawer.DrawCraftingPanels(this, state);
}
