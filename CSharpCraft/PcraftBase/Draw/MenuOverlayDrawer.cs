using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;

namespace CSharpCraft.PcraftBase.Draw;

internal static class MenuOverlayDrawer
{
    internal static void ItemName(int x, int y, InventorySlot item, int col)
    {
        int? power = item is ToolItem tool ? tool.Power : null;
        DrawItemVisual(x, y, col, power, item.Type);
    }

    internal static void DrawItemVisual(int x, int y, int col, int? power, ItemDef type)
    {
        Pico8.Pal();
        int px = x;
        if (power.HasValue)
        {
            int pw = power.Value - 1;
            if (pw >= 0 && pw < PcraftData.PwrNames.Length)
            {
                string pwn = PcraftData.PwrNames[pw];
                Pico8.Print(pwn, x + 10, y, col);
                px += pwn.Length * 4 + 4;
                if (pw < PcraftData.PwrPal.Length)
                    PcraftServices.SetPal(PcraftData.PwrPal[pw]);
            }
        }
        if (type.Pal is not null) PcraftServices.SetPal(type.Pal);
        Pico8.Spr(type.Spr, x, y - 2);
        Pico8.Pal();
        Pico8.Print(type.Name, px + 10, y, col);
    }

    internal static void DrawPanel(string name, int x, int y, int sx, int sy)
    {
        Pico8.Rrectfill(x, y, sx, sy, 2, 1);
	    Pico8.Rrect(x + 1, y + 1, sx - 2, sy - 2, 2, 13);
        double hx = x + (sx - name.Length * 4) / 2.0;
        Pico8.Rectfill(hx, y + 1, hx + name.Length * 4, y + 7, 13);
        Pico8.Print(name, hx + 1, y + 2, 7);
    }

    internal static void DrawInventoryMenu(InventoryMenu menu)
    {
        Pico8.Camera();
        DrawPanel(PcraftData.Inventary.Name, 4, 24, 84, 96);
        var list = menu.List;
        if (list.Count < 1) return;
        menu.Off = DrawListCore(menu.Sel, menu.Off, 4, 24, 84, 96, 10, list.Count, (i, lx, py) =>
        {
            var it = list[i - 1];
            ItemName(lx, py, it, 7);
            if (it is StackableItem Stackable)
            {
                string c = Stackable.Count.ToString();
                Pico8.Print(c, lx + 84 - c.Length * 4 - 10, py, 7);
            }
        });
    }

    internal static void DrawChestPanels(ChestMenu menu)
    {
        Pico8.Camera();
        int sel = menu.Sel;
        int off = menu.Off;
        if (menu.TabToggle == 0)
        {
            off = DrawItemList(menu.ChestItems,  PcraftData.Chest.Name,      sel, off, 4,   24, 84, 96, 10);
            off = DrawItemList(menu.PlayerItems, PcraftData.Inventary.Name,  sel, off, 87,  24, 84, 96, 10);
        }
        else
        {
            off = DrawItemList(menu.ChestItems,  PcraftData.Chest.Name,      sel, off, -44, 24, 84, 96, 10);
            off = DrawItemList(menu.PlayerItems, PcraftData.Inventary.Name,  sel, off, 39,  24, 84, 96, 10);
        }
        menu.Off = off;
    }

    internal static int DrawItemList(List<InventorySlot> list, string panelName, int sel, int off, int x, int y, int sx, int sy, int my)
    {
        DrawPanel(panelName, x, y, sx, sy);
        if (list.Count < 1) return off;
        return DrawListCore(sel, off, x, y, sx, sy, my, list.Count, (i, lx, py) =>
        {
            var it = list[i - 1];
            ItemName(lx, py, it, 7);
            if (it is StackableItem Stackable)
            {
                string c = Stackable.Count.ToString();
                Pico8.Print(c, lx + sx - c.Length * 4 - 10, py, 7);
            }
        });
    }

    internal static void DrawCraftingPanels(CraftingMenu menu, PlayerEntity player)
    {
        Pico8.Camera();
        var recipeList = menu.Recipes;
        if (recipeList.Count >= 1 && menu.Sel >= 0 && menu.Sel < recipeList.Count)
        {
            var curGoal = recipeList[menu.Sel];
            DrawPanel("have", 71, 50, 52, 30);
            int have = curGoal.Output is StackableItem haveQuery
                ? PcraftServices.HowMany(player.Invent, haveQuery)
                : 0;
            Pico8.Print(have.ToString(), 91, 65, 7);
            DrawRequireList(curGoal, 4, 79, 104, 50, player);
        }

        DrawPanel(menu.BenchType.Name, 4, 16, 68, 64);

        if (recipeList.Count < 1) return;
        menu.Off = DrawListCore(menu.Sel, menu.Off, 4, 16, 68, 64, 6, recipeList.Count, (i, lx, py) =>
        {
            var it  = recipeList[i - 1];
            int col = PcraftServices.CanCraft(player.Invent, it) ? 7 : 0;
            int? power = it.Output is ToolItem toolOut ? toolOut.Power : null;
            DrawItemVisual(lx, py, col, power, it.Output.Type);
            if (it.Output is StackableItem countedOut)
            {
                string c = countedOut.Count.ToString();
                Pico8.Print(c, lx + 68 - c.Length * 4 - 10, py, col);
            }
        });
    }

    internal static int DrawListCore(int sel, int off, int x, int y, int sx, int sy, int my, int tlist, Action<int, int, int> renderRow)
    {
        if (off > Math.Max(0, sel - 4))           off = Math.Max(0, sel - 4);
        if (off < Math.Min(tlist, sel + 3) - my)  off = Math.Min(tlist, sel + 3) - my;

        int selAdj = sel - off;
        int debut  = off + 1;
        int fin    = Math.Min(off + my, tlist);

        int sely = y + 3 + (selAdj + 1) * 8;
        Pico8.Rectfill(x + 1, sely, x + sx - 3, sely + 6, 13);

        int lx = x + 5;
        int ly = y + 12;

        for (int i = debut; i <= fin; i++)
        {
            int py = ly + (i - 1 - off) * 8;
            renderRow(i, lx, py);
        }

        Pico8.Spr(68, lx - 8,       sely);
        Pico8.Spr(68, lx + sx - 10, sely, 1, 1, true, false);

        return off;
    }

    internal static void DrawRequireList(Recipe recip, int x, int y, int sx, int sy, PlayerEntity player)
    {
        DrawPanel("require", x, y, sx, sy);
        if (recip.Req.Count < 1) return;

        int lx = x + 5;
        int ly = y + 12;

        for (int i = 0; i < recip.Req.Count; i++)
        {
            var    it = recip.Req[i];
            int py = ly + i * 8;
            ItemName(lx, py, it, 7);
            {
                int    h = PcraftServices.HowMany(player.Invent, it);
                string c = $"{h}/{it.Count}";
                Pico8.Print(c, lx + sx - c.Length * 4 - 10, py, h < it.Count ? 8 : 7);
            }
        }
    }

}
