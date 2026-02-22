using CSharpCraft.Pico8;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Xna.Framework;
using SixLabors.ImageSharp.Metadata;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Competitive;

public static class Shared
{
    public static Icon? UpdateIcon(Pico8Functions p8, Icon[] icons, float x, float y)
    {
        foreach (Icon icon in icons)
        {
            if (x > icon.StartPos.x * p8.Cell.Width && x < (icon.EndPos.x + 1) * p8.Cell.Width && y > icon.StartPos.y * p8.Cell.Height && y < (icon.EndPos.y + 1) * p8.Cell.Height) { return icon; }
        }
        return null;
    }

    public static void DrawIcons(Pico8Functions p8, Icon[] icons, float x, float y)
    {
        foreach (Icon icon in icons)
        {
            bool sel = UpdateIcon(p8, [icon], x, y) is not null;
            if (icon.ShadowTexture is not null) { GameRendering.Current.Draw(icon.ShadowTexture, new Vector2 (icon.StartPos.x * p8.Cell.Width, icon.StartPos.y * p8.Cell.Height), Color.White, p8.Cell.Width, p8.Cell.Height); }
            if (icon.IconTexture is not null) { GameRendering.Current.Draw(icon.IconTexture, new Vector2 ((icon.StartPos.x + (sel ? icon.Offset.x : 0)) * p8.Cell.Width, (icon.StartPos.y + (sel ? icon.Offset.y : 0)) * p8.Cell.Height), Color.White, p8.Cell.Width, p8.Cell.Height); }
        }
    }

    public static void DrawCursor(Pico8Functions p8, float x, float y)
    {
        GameRendering.Current.Draw("Cursor", new(x - 15 * (p8.Cell.Width / 2.0f), y - 15 * (p8.Cell.Height / 2.0f)), Color.White, p8.Cell.Width / 2.0f, p8.Cell.Height / 2.0f);
    }

    public static void DrawNameBubble(Pico8Functions p8, string s, int x, int y)
    {
        GameRendering.Current.Draw("10pxHighlightEdge", new Vector2 ((x - s.Length * 2 - 5) * p8.Cell.Width, y * p8.Cell.Height), Color.White, p8.Cell.Width, p8.Cell.Height);
        GameRendering.Current.Draw("10pxHighlightCenter", new((x - s.Length * 2) * p8.Cell.Width, y * p8.Cell.Height), Color.White, (s.Length * 4 + 1) * p8.Cell.Width, p8.Cell.Height);
        GameRendering.Current.Draw("10pxHighlightEdge", new((x + s.Length * 2 + 1) * p8.Cell.Width, y * p8.Cell.Height), Color.White, p8.Cell.Width, p8.Cell.Height, flipX: true);
        Printc(p8, "rooms", x + 1, y + 2, 15);
    }

    public static void Printc(Pico8Functions p8, string t, int x, int y, int c)
    {
        p8.Print(t, x - t.Length * 2, y, c);
    }

    public static void Printc(string t, int x, int y, int c)
    {
        Pico8.Pico8.Print(t, x - t.Length * 2, y, c);
    }

    public static void PrintcBig(Pico8Functions p8, string t, int x, int y, Color c)
    {
        p8.PrintBig(t, x - t.Length * 4, y, c);
    }

    public static void PrintcBig(string t, int x, int y, Color c)
    {
        Pico8.Pico8.PrintBig(t, x - t.Length * 4, y, c);
    }

    public static void Printr(Pico8Functions p8, string t, int x, int y, int c)
    {
        p8.Print(t, x - t.Length * 4 + 1, y, c);
    }

    public static void Printcb(Pico8Functions p8, string t, int x, int y, int c1, int c2)
    {
        p8.Print(t, x + 1 - t.Length * 2 + 1, y, c2);
        p8.Print(t, x + 1 - t.Length * 2 - 1, y, c2);
        p8.Print(t, x + 1 - t.Length * 2, y + 1, c2);
        p8.Print(t, x + 1 - t.Length * 2, y - 1, c2);
        p8.Print(t, x + 1 - t.Length * 2, y, c1);
    }

}
