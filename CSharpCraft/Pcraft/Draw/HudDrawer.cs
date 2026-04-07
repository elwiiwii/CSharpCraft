namespace CSharpCraft.Pcraft.Draw;

internal static class HudDrawer
{
    internal static void DrawHud(WorldState state)
    {
        DrawBar(4, 4, state.Plife.Double, state.Llife.Double, 8, 2);
        DrawBar(4, 9, Math.Max(0.0, state.Pstam.Double), state.Lstam.Double, 11, 3);

        if (state.CurItem != null)
        {
            const int ix = 35;
            const int iy = 3;
            MenuOverlayDrawer.ItemName(ix + 1, iy + 3, state.CurItem, 7);
            if (state.CurItem.Count.HasValue)
            {
                string cnt = state.CurItem.Count.Value.ToString();
                Pico8.Print(cnt, ix + 88 - 16, iy + 3, 7);
            }
        }
    }

    internal static void DrawBar(double px, double py, double v, double m, double c, double c2)
    {
        Pico8.Pal();
        double pe  = px + v * 0.3;
        double pe2 = px + m * 0.3;
        Pico8.Rectfill(px - 1, py - 1, px + 30, py + 4, 0);
        Pico8.Rectfill(px, py, pe, py + 3, c2);
        Pico8.Rectfill(px, py, Math.Max(px, pe - 1), py + 2, c);
        if (m > v)
            Pico8.Rectfill(pe + 1, py, pe2, py + 3, 10);
    }
}
