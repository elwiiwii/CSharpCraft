using CSharpCraft.Pcraft.Crafting;
using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Inventory;
using CSharpCraft.Pcraft.Map;
using PSharp8;

namespace CSharpCraft.Pcraft.Update;

internal static class MenuUpdater
{
    /// <summary>
    /// Handles menu input for one frame.
    /// Returns true when a menu consumed the frame (caller should skip game logic).
    /// </summary>
    internal static bool Update(WorldState state, PcraftGame game, Random rng)
    {
        if (state.CurMenu == null) return false;

        // ── Splash screen (title / intro / death / win) ───────────────────────
        if (state.CurMenu.Spr != 0)
        {
            if (Pico8.Btnp(4) && !state.Lb4)
            {
                if (state.CurMenu == PcraftData.MainMenu)
                {
                    state.CurMenu = PcraftData.IntroMenu;
                }
                else
                {
                    LevelManager.ResetLevel(state, game, rng);
                    state.CurMenu = null;
                    Pico8.Music(1);
                }
            }
            state.Lb4 = Pico8.Btn(4);
            return true;
        }

        // ── Interactive menu ──────────────────────────────────────────────────
        var intMenu = state.CurMenu;
        var othMenu = state.MenuInvent;

        if (state.CurMenu.Type == PcraftData.Chest)
        {
            if (Pico8.Btnp(0)) { state.ToogleMenu -= 1; Pico8.Sfx(18); }
            if (Pico8.Btnp(1)) { state.ToogleMenu += 1; Pico8.Sfx(18); }
            state.ToogleMenu = ((state.ToogleMenu % 2) + 2) % 2;
            if (state.ToogleMenu == 1)
            {
                intMenu = state.MenuInvent;
                othMenu = state.CurMenu;
            }
        }

        // Count of items in the active list (recipe-aware)
        var activeCount = intMenu == null ? 0
            : intMenu.Type.BeCraft
                ? intMenu.RecipeList?.Count ?? 0
                : intMenu.List?.Count ?? 0;

        if (activeCount > 0)
        {
            if (Pico8.Btnp(2)) { intMenu!.Sel -= 1; Pico8.Sfx(18); }
            if (Pico8.Btnp(3)) { intMenu!.Sel += 1; Pico8.Sfx(18); }

            intMenu!.Sel = InventoryOps.Loop(intMenu.Sel, activeCount);

            if (Pico8.Btnp(5) && !state.Lb5)
            {
                if (state.CurMenu.Type == PcraftData.Chest)
                {
                    Pico8.Sfx(16);
                    var itemList  = intMenu.List!;
                    var el = itemList[intMenu.Sel];
                    itemList.Remove(el);
                    InventoryOps.AddItemInList(othMenu!.List!, el, othMenu.Sel);
                    if (itemList.Count > 0 && intMenu.Sel >= itemList.Count)
                        intMenu.Sel -= 1;
                    if (intMenu == state.MenuInvent && state.CurItem == el)
                        state.CurItem = null;
                }
                else if (state.CurMenu.Type.BeCraft)
                {
                    var recipeList = state.CurMenu.RecipeList;
                    if (recipeList != null && state.CurMenu.Sel >= 0 && state.CurMenu.Sel < recipeList.Count)
                    {
                        var rec = recipeList[state.CurMenu.Sel];
                        if (CraftingSystem.CanCraft(state.Invent, rec))
                        {
                            CraftingSystem.Craft(state.Invent, rec);
                            Pico8.Sfx(16);
                        }
                        else
                        {
                            Pico8.Sfx(17);
                        }
                    }
                }
                else
                {
                    // Equip from inventory: move selected item to front, close menu
                    var itemList = intMenu.List!;
                    var curItem  = itemList[intMenu.Sel];
                    itemList.Remove(curItem);
                    InventoryOps.AddItemInList(itemList, curItem, 0);
                    intMenu.Sel   = 0;
                    state.CurItem = curItem;
                    state.CurMenu = null;
                    state.Block5  = true;
                    Pico8.Sfx(16);
                }
            }
        }

        if (Pico8.Btnp(4) && !state.Lb4)
        {
            state.CurMenu = null;
            Pico8.Sfx(17);
        }
        state.Lb4 = Pico8.Btn(4);
        state.Lb5 = Pico8.Btn(5);
        return true;
    }
}
