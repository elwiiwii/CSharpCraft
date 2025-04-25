using CSharpCraft.Pico8;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Competitive
{
    public static class Shared
    {
        public static Icon? IconUpdate(Pico8Functions p8, Icon[] icons, float x, float y)
        {
            int viewportWidth = p8.Batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = p8.Batch.GraphicsDevice.Viewport.Height;
            int w = viewportWidth / 128;
            int h = viewportHeight / 128;
            foreach (Icon icon in icons)
            {
                if (x > icon.StartPos.x * w && x < (icon.EndPos.x + 1) * w && y > icon.StartPos.y * h && y < (icon.EndPos.y + 1) * h) { return icon; }
            }
            return null;
        }

        public static void DrawIcons(Pico8Functions p8, Icon[] icons, float x, float y)
        {
            int viewportWidth = p8.Batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = p8.Batch.GraphicsDevice.Viewport.Height;
            int w = viewportWidth / 128;
            int h = viewportHeight / 128;
            Vector2 size = new(w, h);
            foreach (Icon icon in icons)
            {
                bool sel = IconUpdate(p8, [icon], x, y) is not null;
                if (icon.ShadowTexture is not null) { p8.Batch.Draw(p8.TextureDictionary[icon.ShadowTexture], new Vector2 (icon.StartPos.x * w, icon.StartPos.y * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0); }
                if (icon.IconTexture is not null) { p8.Batch.Draw(p8.TextureDictionary[icon.IconTexture], new Vector2 ((icon.StartPos.x + (sel ? icon.Offset.x : 0)) * w, (icon.StartPos.y + (sel ? icon.Offset.y : 0)) * h), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0); }
            }
        }

        public static void DrawCursor(Pico8Functions p8, float x, float y)
        {
            int viewportWidth = p8.Batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = p8.Batch.GraphicsDevice.Viewport.Height;
            int w = viewportWidth / 128;
            int h = viewportHeight / 128;
            p8.Batch.Draw(p8.TextureDictionary["Cursor"], new(x - 15 * (w / 2.0f), y - 15 * (h / 2.0f)), null, Color.White, 0, Vector2.Zero, new Vector2 (w / 2.0f, h / 2.0f), SpriteEffects.None, 0);
        }

        public static void Printcb(Pico8Functions p8, string t, double x, double y, int c1, int c2)
        {
            p8.Print(t, x + 1 - t.Length * 2 + 1, y, c2);
            p8.Print(t, x + 1 - t.Length * 2 - 1, y, c2);
            p8.Print(t, x + 1 - t.Length * 2, y + 1, c2);
            p8.Print(t, x + 1 - t.Length * 2, y - 1, c2);
            p8.Print(t, x + 1 - t.Length * 2, y, c1);
        }

    }
}
