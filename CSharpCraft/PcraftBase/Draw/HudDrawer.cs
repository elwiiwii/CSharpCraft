namespace CSharpCraft.PcraftBase.Draw;

internal static class HudDrawer
{
    internal static void DrawHud(WorldState state)
    {
        DrawBar(F32.FromInt(4), F32.FromInt(4), state.Plife, state.Llife, F32.FromInt(8), F32.FromInt(2));
        DrawBar(F32.FromInt(4), F32.FromInt(9), F32.Max(F32.Zero, state.Pstam), state.Lstam, F32.FromInt(11), F32.FromInt(3));

        if (state.CurItem != null)
        {
            const int ix = 35;
            const int iy = 3;
            PcraftServices.ItemName(ix + 1, iy + 3, state.CurItem, 7);
            if (state.CurItem.Count.HasValue)
            {
                string cnt = state.CurItem.Count.Value.ToString();
                Pico8.Print(cnt, ix + 88 - 16, iy + 3, 7);
            }
        }
    }

    internal static void DrawBar(F32 px, F32 py, F32 v, F32 m, F32 c, F32 c2)
    {
        Pico8.Pal();
        F32 pe  = px + v * F32.FromDouble(0.3);
        F32 pe2 = px + m * F32.FromDouble(0.3);
        Pico8.Rectfill(px - 1, py - 1, px + 30, py + 4, F32.Zero);
        Pico8.Rectfill(px, py, pe, py + 3, c2);
        Pico8.Rectfill(px, py, F32.Max(px, pe - 1), py + 2, c);
        if (m > v)
            Pico8.Rectfill(pe + 1, py, pe2, py + 3, F32.FromInt(10));
    }
}
