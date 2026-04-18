namespace CSharpCraft.PcraftBase.Draw;

internal static class DrawHelpers
{
    internal static void SetPal(int[] l)
    {
        for (int i = 0; i < l.Length; i++)
            Pico8.Pal(i + 1, l[i]);
    }

    internal static void PrintB(string t, double x, double y, double c)
    {
        Pico8.Print(t, x + 1, y, 1);
        Pico8.Print(t, x - 1, y, 1);
        Pico8.Print(t, x, y + 1, 1);
        Pico8.Print(t, x, y - 1, 1);
        Pico8.Print(t, x, y, c);
    }

    internal static void PrintC(string t, int x, int y, int c)
        => Pico8.Print(t, x - t.Length * 2, y, c);
}
