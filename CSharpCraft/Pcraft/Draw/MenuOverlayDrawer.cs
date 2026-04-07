using CSharpCraft.Pcraft.Crafting;
using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Inventory;

namespace CSharpCraft.Pcraft.Draw;

internal static class MenuOverlayDrawer
{
    internal static void DrawMenuOverlay(WorldState state, PcraftGame game)
    {
        if (state.CurMenu == null) return;

        Pico8.Camera();
        var menu = state.CurMenu;

        if (menu.Type == PcraftData.Chest)
        {
            var mi = state.MenuInvent ?? menu;
            if (state.ToogleMenu == 0)
            {
                DrawList(mi, 87, 24, 84, 96, 10);
                DrawList(menu, 4, 24, 84, 96, 10);
            }
            else
            {
                DrawList(menu, -44, 24, 84, 96, 10);
                DrawList(mi, 39, 24, 84, 96, 10);
            }
        }
        else if (menu.Type.BeCraft)
        {
            var recipeList = menu.RecipeList;
            if (recipeList != null && menu.Sel >= 1 && menu.Sel < recipeList.Count)
            {
                var curGoal = recipeList[menu.Sel];
                DrawPanel("have", 71, 50, 52, 30);
                int have = InventoryOps.HowMany(state.Invent, new ItemStack(curGoal.Type));
                Pico8.Print(have.ToString(), 91, 65, 7);
                DrawRequireList(curGoal, 4, 79, 104, 50, state);
            }
            DrawListRecipes(menu, 4, 16, 68, 64, 6, state);
        }
        else
        {
            DrawList(menu, 4, 24, 84, 96, 10);
        }
    }

    internal static void ItemName(double x, double y, ItemStack item, double col)
        => DrawItemVisual(x, y, col, item.Power, item.Type);

    private static void DrawItemVisual(double x, double y, double col, int? power, ItemDef type)
    {
        Pico8.Pal();
        double px = x;
        if (power.HasValue)
        {
            int pw = power.Value - 1;
            if (pw >= 0 && pw < PcraftData.PwrNames.Length)
            {
                string pwn = PcraftData.PwrNames[pw];
                Pico8.Print(pwn, x + 10, y, col);
                px += pwn.Length * 4 + 4;
                if (pw < PcraftData.PwrPal.Length)
                    DrawHelpers.SetPal(PcraftData.PwrPal[pw]);
            }
        }
        if (type.Pal != null) DrawHelpers.SetPal(type.Pal);
        Pico8.Spr(type.Spr, x, y - 2);
        Pico8.Pal();
        Pico8.Print(type.Name, px + 10, y, col);
    }

    private static void DrawPanel(string name, double x, double y, double sx, double sy)
    {
        Pico8.Rectfill(x + 8, y + 8, x + sx - 9, y + sy - 9, 1);
        Pico8.Spr(66, x, y);
        Pico8.Spr(67, x + sx - 8, y);
        Pico8.Spr(82, x, y + sy - 8);
        Pico8.Spr(83, x + sx - 8, y + sy - 8);
        Pico8.Sspr(24, 32, 4, 8, x + 8,      y,          sx - 16, 8);
        Pico8.Sspr(24, 40, 4, 8, x + 8,      y + sy - 8, sx - 16, 8);
        Pico8.Sspr(16, 36, 8, 4, x,           y + 8,      8,       sy - 16);
        Pico8.Sspr(24, 36, 8, 4, x + sx - 8, y + 8,      8,       sy - 16);
        double hx = x + (sx - name.Length * 4) / 2.0;
        Pico8.Rectfill(hx, y + 1, hx + name.Length * 4, y + 7, 13);
        Pico8.Print(name, hx + 1, y + 2, 7);
    }

    private static void DrawList(MenuState menu, double x, double y, double sx, double sy, int my)
    {
        DrawPanel(menu.Type.Name, x, y, sx, sy);
        var list = menu.List;
        if (list == null || list.Count < 1) return;
        DrawListCore(menu, x, y, sx, sy, my, list.Count, (i, lx, py) =>
        {
            var    it = list[i - 1];
            ItemName(lx, py, it, 7);
            if (it.Count.HasValue)
            {
                string c = it.Count.Value.ToString();
                Pico8.Print(c, lx + sx - c.Length * 4 - 10, py, 7);
            }
        });
    }

    private static void DrawListRecipes(MenuState menu, double x, double y, double sx, double sy, int my, WorldState state)
    {
        DrawPanel(menu.Type.Name, x, y, sx, sy);
        var list = menu.RecipeList;
        if (list == null || list.Count < 1) return;
        DrawListCore(menu, x, y, sx, sy, my, list.Count, (i, lx, py) =>
        {
            var    it  = list[i - 1];
            double col = CraftingSystem.CanCraft(state.Invent, it) ? 7 : 0;
            DrawItemVisual(lx, py, col, it.Power, it.Type);
            if (it.Count.HasValue)
            {
                string c = it.Count.Value.ToString();
                Pico8.Print(c, lx + sx - c.Length * 4 - 10, py, col);
            }
        });
    }

    private static void DrawListCore(MenuState menu, double x, double y, double sx, double sy, int my, int tlist, Action<int, double, double> renderRow)
    {
        int sel = menu.Sel;
        if (menu.Off > Math.Max(0, sel - 4))           menu.Off = Math.Max(0, sel - 4);
        if (menu.Off < Math.Min(tlist, sel + 3) - my)  menu.Off = Math.Min(tlist, sel + 3) - my;

        int selAdj = sel - menu.Off;
        int debut  = menu.Off + 1;
        int fin    = Math.Min(menu.Off + my, tlist);

        double sely = y + 3 + selAdj * 8;
        Pico8.Rectfill(x + 1, sely, x + sx - 3, sely + 6, 13);

        double lx = x + 5;
        double ly = y + 12;

        for (int i = debut; i <= fin; i++)
        {
            double py = ly + (i - 1 - menu.Off) * 8;
            renderRow(i, lx, py);
        }

        Pico8.Spr(68, x - 3,       sely);
        Pico8.Spr(68, x + sx - 10, sely, 1, 1, true, false);
    }

    private static void DrawRequireList(Recipe recip, double x, double y, double sx, double sy, WorldState state)
    {
        DrawPanel("require", x, y, sx, sy);
        if (recip.Req.Count < 1) return;

        double lx = x + 5;
        double ly = y + 12;

        for (int i = 0; i < recip.Req.Count; i++)
        {
            var    it = recip.Req[i];
            double py = ly + i * 8;
            ItemName(lx, py, it, 7);
            if (it.Count.HasValue)
            {
                int    h = InventoryOps.HowMany(state.Invent, it);
                string c = $"{h}/{it.Count.Value}";
                Pico8.Print(c, lx + sx - c.Length * 4 - 10, py, h < it.Count.Value ? 8 : 7);
            }
        }
    }

}
