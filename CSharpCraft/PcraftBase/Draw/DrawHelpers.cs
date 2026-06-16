using PSharp8.Graphics;

namespace CSharpCraft.PcraftBase.Draw;

internal static class DrawHelpers
{
    internal static void SetPal(int[] l)
    {
        for (int i = 0; i < l.Length; i++)
            Pico8.Pal((PicoColor)(i + 1), (PicoColor)l[i]);
    }

    internal static void PrintB(string t, double x, double y, PicoColor inCol, PicoColor outCol)
    {
        Pico8.Print(t, x + 1, y, outCol);
        Pico8.Print(t, x - 1, y, outCol);
        Pico8.Print(t, x, y + 1, outCol);
        Pico8.Print(t, x, y - 1, outCol);
        Pico8.Print(t, x, y, inCol);
    }

    internal static void PrintC(string t, int x, int y, PicoColor c)
    {
        Pico8.Print(t, x - (t.Length * 2), y, c);
    }
}
