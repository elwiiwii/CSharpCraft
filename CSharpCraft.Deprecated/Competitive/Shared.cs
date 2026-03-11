using CSharpCraft.Pico8;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        foreach (Icon icon in icons)
        {
            bool sel = UpdateIcon(p8, [icon], x, y) is not null;
            if (icon.ShadowTexture is not null) { p8.Batch.Draw(p8.TextureDictionary[icon.ShadowTexture], new Vector2 (icon.StartPos.x * p8.Cell.Width, icon.StartPos.y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0); }
            if (icon.IconTexture is not null) { p8.Batch.Draw(p8.TextureDictionary[icon.IconTexture], new Vector2 ((icon.StartPos.x + (sel ? icon.Offset.x : 0)) * p8.Cell.Width, (icon.StartPos.y + (sel ? icon.Offset.y : 0)) * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0); }
        }
    }

    public static void DrawCursor(Pico8Functions p8, float x, float y)
    {
        p8.Batch.Draw(p8.TextureDictionary["Cursor"], new(x - 15 * (p8.Cell.Width / 2.0f), y - 15 * (p8.Cell.Height / 2.0f)), null, Color.White, 0, Vector2.Zero, new Vector2 (p8.Cell.Width / 2.0f, p8.Cell.Height / 2.0f), SpriteEffects.None, 0);
    }

    public static void DrawNameBubble(Pico8Functions p8, string s, int x, int y)
    {
        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        p8.Batch.Draw(p8.TextureDictionary["10pxHighlightEdge"], new Vector2 ((x - s.Length * 2 - 5) * p8.Cell.Width, y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary["10pxHighlightCenter"], new((x - s.Length * 2) * p8.Cell.Width, y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, new Vector2((s.Length * 4 + 1) * p8.Cell.Width, p8.Cell.Height), SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary["10pxHighlightEdge"], new((x + s.Length * 2 + 1) * p8.Cell.Width, y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
        Printc(p8, "rooms", x + 1, y + 2, 15);
    }

    public static void Printc(Pico8Functions p8, string t, int x, int y, int c)
    {
        p8.Print(t, x - t.Length * 2, y, c);
    }

    public static void PrintcBig(Pico8Functions p8, string t, int x, int y, Color c)
    {
        p8.PrintBig(t, x - t.Length * 4, y, c);
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
