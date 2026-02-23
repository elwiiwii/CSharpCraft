using CSharpCraft.Pico8;
using static CSharpCraft.Pico8.Pico8;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Xna.Framework;
using SixLabors.ImageSharp.Metadata;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Competitive;

public static class Shared
{
    public static Icon? UpdateIcon(Icon[] icons, float x, float y)
    {
        foreach (Icon icon in icons)
        {
            if (x > icon.StartPos.x * CellWidth && x < (icon.EndPos.x + 1) * CellWidth && y > icon.StartPos.y * CellHeight && y < (icon.EndPos.y + 1) * CellHeight) { return icon; }
        }
        return null;
    }

    public static void DrawIcons(Icon[] icons, float x, float y)
    {
        foreach (Icon icon in icons)
        {
            bool sel = UpdateIcon([icon], x, y) is not null;
            if (icon.ShadowTexture is not null) { GameRendering.Current.Draw(icon.ShadowTexture, icon.StartPos.x, icon.StartPos.y, Color.White); }
            if (icon.IconTexture is not null) { GameRendering.Current.Draw(icon.IconTexture, icon.StartPos.x + (sel ? icon.Offset.x : 0), icon.StartPos.y + (sel ? icon.Offset.y : 0), Color.White); }
        }
    }

    public static void DrawCursor(float x, float y)
    {
        GameRendering.Current.Draw("Cursor", x / CellWidth - 7.5, y / CellHeight - 7.5, Color.White, 0.5, 0.5);
    }

    public static void DrawNameBubble(string s, int x, int y)
    {
        GameRendering.Current.Draw("10pxHighlightEdge", x - s.Length * 2 - 5, y, Color.White);
        GameRendering.Current.Draw("10pxHighlightCenter", x - s.Length * 2, y, Color.White, scaleX: s.Length * 4 + 1);
        GameRendering.Current.Draw("10pxHighlightEdge", x + s.Length * 2 + 1, y, Color.White, flipX: true);
        Printc("rooms", x + 1, y + 2, 15);
    }

    public static void Printc(string t, int x, int y, int c)
    {
        Print(t, x - t.Length * 2, y, c);
    }

    public static void PrintcBig(string t, int x, int y, Color c)
    {
        PrintBig(t, x - t.Length * 4, y, c);
    }

    public static void Printr(string t, int x, int y, int c)
    {
        Print(t, x - t.Length * 4 + 1, y, c);
    }

    public static void Printcb(string t, int x, int y, int c1, int c2)
    {
        Print(t, x + 1 - t.Length * 2 + 1, y, c2);
        Print(t, x + 1 - t.Length * 2 - 1, y, c2);
        Print(t, x + 1 - t.Length * 2, y + 1, c2);
        Print(t, x + 1 - t.Length * 2, y - 1, c2);
        Print(t, x + 1 - t.Length * 2, y, c1);
    }

}
