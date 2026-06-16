using CSharpCraft.PcraftBase.Data;
using PSharp8.Graphics;

namespace CSharpCraft.PcraftBase.Draw;

internal static class HudDrawer
{
    internal static void DrawHud(PlayerEntity player)
    {
        PcraftServices.DrawBar(F32.FromInt(4), F32.FromInt(4),
            player.Life, player.Llife, PicoColor._08Red, PicoColor._02DarkPurple);
        PcraftServices.DrawBar(F32.FromInt(4), F32.FromInt(9),
            F32.Max(F32.Zero, player.Stam), player.Lstam, PicoColor._11Green, PicoColor._03DarkGreen);

        if (player.CurItem is not null)
        {
            const int ix = 35;
            const int iy = 3;
            PcraftServices.ItemName(ix + 1, iy + 3, player.CurItem, PicoColor._07White);
            if (player.CurItem is StackableItem curStack)
            {
                string count = curStack.Count.ToString();
                Pico8.Print(count, ix + 88 - 16, iy + 3, PicoColor._07White);
            }
        }
    }

    internal static void DrawBar(F32 px, F32 py, F32 v, F32 m, PicoColor c, PicoColor c2)
    {
        Pico8.Pal();
        F32 pe = px + (v * F32.FromDouble(0.3));
        F32 pe2 = px + (m * F32.FromDouble(0.3));
        Pico8.Rectfill(px - 1, py - 1, px + 30, py + 4, PicoColor._00Black);
        Pico8.Rectfill(px, py, pe, py + 3, c2);
        Pico8.Rectfill(px, py, F32.Max(px, pe - 1), py + 2, c);
        if (m > v)
            Pico8.Rectfill(pe + 1, py, pe2, py + 3, PicoColor._10Yellow);
    }
}
