namespace CSharpCraft.PcraftBase.Draw;

internal static class DrawHelpers
{
    internal static void SetPal(int[] l)
    {
        for (int i = 0; i < l.Length; i++)
            Pico8.Pal(i + 1, l[i]);
    }
}
