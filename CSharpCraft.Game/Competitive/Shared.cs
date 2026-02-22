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
            if (icon.ShadowTexture is not null) { GameRendering.Current.Draw(icon.ShadowTexture, new Vector2 (icon.StartPos.x * CellWidth, icon.StartPos.y * CellHeight), Color.White, CellWidth, CellHeight); }
            if (icon.IconTexture is not null) { GameRendering.Current.Draw(icon.IconTexture, new Vector2 ((icon.StartPos.x + (sel ? icon.Offset.x : 0)) * CellWidth, (icon.StartPos.y + (sel ? icon.Offset.y : 0)) * CellHeight), Color.White, CellWidth, CellHeight); }
        }
    }

    public static void DrawCursor(float x, float y)
    {
        GameRendering.Current.Draw("Cursor", new Vector2(x - 15 * (CellWidth / 2.0f), y - 15 * (CellHeight / 2.0f)), Color.White, CellWidth / 2.0f, CellHeight / 2.0f);
    }

    public static void DrawNameBubble(string s, int x, int y)
    {
        GameRendering.Current.Draw("10pxHighlightEdge", new Vector2 ((x - s.Length * 2 - 5) * CellWidth, y * CellHeight), Color.White, CellWidth, CellHeight);
        GameRendering.Current.Draw("10pxHighlightCenter", new Vector2((x - s.Length * 2) * CellWidth, y * CellHeight), Color.White, (s.Length * 4 + 1) * CellWidth, CellHeight);
        GameRendering.Current.Draw("10pxHighlightEdge", new Vector2((x + s.Length * 2 + 1) * CellWidth, y * CellHeight), Color.White, CellWidth, CellHeight, flipX: true);
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
