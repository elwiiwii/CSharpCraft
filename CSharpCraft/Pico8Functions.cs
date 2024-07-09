using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft
{
    public class Pico8Functions(Texture2D pixel, SpriteBatch batch, GraphicsDevice graphicsDevice) : IDisposable
    {
        private readonly SpriteBatch batch = batch;
        private readonly GraphicsDevice graphicsDevice = graphicsDevice;
        private readonly Texture2D pixel = pixel;
        private readonly Dictionary<int, Texture2D> spriteTextures = new();
        private int[] Map1 = new int[128 * 64];
        private int[] Map2 = new int[32 * 32];
        private (int, int) CameraOffset = (0, 0);

        public bool prev0 = false;
        public bool prev1 = false;
        public bool prev2 = false;
        public bool prev3 = false;
        public bool prev4 = false;
        public bool prev5 = false;


        private static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return new Color(r, g, b);
        }


        // pico-8 colors https://pico-8.fandom.com/wiki/Palette
        public Color[] colors =
        [
                HexToColor("000000"), // 00 black
                HexToColor("1D2B53"), // 01 dark-blue
                HexToColor("7E2553"), // 02 dark-purple
                HexToColor("008751"), // 03 dark-green
                HexToColor("AB5236"), // 04 brown
                HexToColor("5F574F"), // 05 dark-grey
                HexToColor("C2C3C7"), // 06 light-grey
                HexToColor("FFF1E8"), // 07 white
                HexToColor("FF004D"), // 08 red
                HexToColor("FFA300"), // 09 orange
                HexToColor("FFEC27"), // 10 yellow
                HexToColor("00E436"), // 11 green
                HexToColor("29ADFF"), // 12 blue
                HexToColor("83769C"), // 13 lavender
                HexToColor("FF77A8"), // 14 pink
                HexToColor("FFCCAA"), // 15 light-peach
                
                /*
                HexToColor("291814"), // 16 brownish-black
                HexToColor("111D35"), // 17 darker-blue
                HexToColor("422136"), // 18 darker-purple
                HexToColor("125359"), // 19 blue-green
                HexToColor("742F29"), // 20 dark-brown
                HexToColor("49333B"), // 21 darker-grey
                HexToColor("A28879"), // 22 medium-grey
                HexToColor("F3EF7D"), // 23 light-yellow
                HexToColor("BE1250"), // 24 dark-red
                HexToColor("FF6C24"), // 25 dark-orange
                HexToColor("A8E72E"), // 26 lime-green
                HexToColor("00B543"), // 27 medium-green
                HexToColor("065AB5"), // 28 true-blue
                HexToColor("754665"), // 29 mauve
                HexToColor("FF6E59"), // 30 dark-peach
                HexToColor("FF9D81"), // 31 peach
                */
        ];
        public Dictionary<Color, int> paletteSwap = new();
        public Color[] resetColors = new Color[16];
        public Color[] resetSprColors = new Color[16];
        public Color[] sprColors = new Color[16];


        private Texture2D CreateTextureFromSpriteData(string spriteData, int spriteX, int spriteY, int spriteWidth, int spriteHeight)
        {
            spriteData = new string(spriteData.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray());

            Texture2D texture = new(graphicsDevice, spriteWidth, spriteHeight);

            Color[] colorData = new Color[spriteWidth * spriteHeight];

            //int j = 0;

            for (int i = spriteX + (spriteY * 128), j = 0; j < (spriteWidth * spriteHeight); i++, j++)
            {
                char c = spriteData[i];
                int colorIndex = Convert.ToInt32(c.ToString(), 16); // Convert hex to int
                Color color = sprColors[colorIndex]; // Convert the PICO-8 color index to a Color
                colorData[j] = color;

                if (i % spriteWidth == spriteWidth - 1) { i += 128 - spriteWidth; }
                //j++;
            }

            texture.SetData(colorData);

            return texture;
        }


        public Entity Add(List<Entity> table, Entity value, int index = -1) // https://pico-8.fandom.com/wiki/Add
        {
            if (index == -1) { table.Add(value); return value; }
            table.Insert(index, value);
            return value;
        }


        public bool Btn(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
        {
            KeyboardState state = Keyboard.GetState();

            if (i == 0) { return state.IsKeyDown(Keys.A); }
            if (i == 1) { return state.IsKeyDown(Keys.D); }
            if (i == 2) { return state.IsKeyDown(Keys.W); }
            if (i == 3) { return state.IsKeyDown(Keys.S); }
            if (i == 4) { return state.IsKeyDown(Keys.M); }
            if (i == 5) { return state.IsKeyDown(Keys.N); }
            else { return false; }
        }


        public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
        {
            if (i == 0 && Btn(i) && !prev0) { return true; }
            if (i == 1 && Btn(i) && !prev1) { return true; }
            if (i == 2 && Btn(i) && !prev2) { return true; }
            if (i == 3 && Btn(i) && !prev3) { return true; }
            if (i == 4 && Btn(i) && !prev4) { return true; }
            if (i == 5 && Btn(i) && !prev5) { return true; }
            else { return false; }
        }


        public void Camera(double x = 0, double y = 0) // https://pico-8.fandom.com/wiki/Camera
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);

            CameraOffset = (xFlr, yFlr);
        }


        public void Circ(double x, double y, double r, int c) // https://pico-8.fandom.com/wiki/Circ
        {
            if (r < 0) return; // If r is negative, the circle is not drawn

            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int rFlr = (int)Math.Floor(r);

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            for (int i = xFlr - rFlr; i <= xFlr + rFlr; i++)
            {
                for (int j = yFlr - rFlr; j <= yFlr + rFlr; j++)
                {
                    // Check if the point 0.36 units into the grid space from the center of the circle is within the circle
                    double offsetX = (i < xFlr) ? 0.35D : -0.35D;
                    double offsetY = (j < yFlr) ? 0.35D : -0.35D;
                    double gridCenterX = i + offsetX;
                    double gridCenterY = j + offsetY;

                    bool isCurrentInCircle = Math.Pow(gridCenterX - xFlr, 2) + Math.Pow(gridCenterY - yFlr, 2) <= rFlr * rFlr;

                    // Check all four adjacent grid spaces
                    bool isRightOutsideCircle = Math.Pow(i + 1 + offsetX - xFlr, 2) + Math.Pow(j + offsetY - yFlr, 2) > rFlr * rFlr;
                    bool isLeftOutsideCircle = Math.Pow(i - 1 + offsetX - xFlr, 2) + Math.Pow(j + offsetY - yFlr, 2) > rFlr * rFlr;
                    bool isUpOutsideCircle = Math.Pow(i + offsetX - xFlr, 2) + Math.Pow(j + 1 + offsetY - yFlr, 2) > rFlr * rFlr;
                    bool isDownOutsideCircle = Math.Pow(i + offsetX - xFlr, 2) + Math.Pow(j - 1 + offsetY - yFlr, 2) > rFlr * rFlr;

                    if (isCurrentInCircle && (isRightOutsideCircle || isLeftOutsideCircle || isUpOutsideCircle || isDownOutsideCircle))
                    {
                        // Calculate the position and size of the line
                        Vector2 position = new((i - CameraOffset.Item1) * cellWidth, (j - CameraOffset.Item2) * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw the line
                        batch.Draw(pixel, position, null, colors[c], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }


        public void Circfill(double x, double y, double r, int c) // https://pico-8.fandom.com/wiki/Circfill
        {
            if (r < 0) return; // If r is negative, the circle is not drawn

            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int rFlr = (int)Math.Floor(r);

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            for (int i = (xFlr - rFlr); i <= xFlr + rFlr; i++)
            {
                for (int j = (yFlr - rFlr); j <= yFlr + rFlr; j++)
                {
                    // Check if the point 0.36 units into the grid space from the center of the circle is within the circle
                    double offsetX = (i < xFlr) ? 0.35D : -0.35D;
                    double offsetY = (j < yFlr) ? 0.35D : -0.35D;
                    double gridCenterX = i + offsetX;
                    double gridCenterY = j + offsetY;

                    if (Math.Pow(gridCenterX - xFlr, 2) + Math.Pow(gridCenterY - yFlr, 2) <= rFlr * rFlr)
                    {
                        // Calculate the position and size
                        Vector2 position = new((i - CameraOffset.Item1) * cellWidth, (j - CameraOffset.Item2) * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw
                        batch.Draw(pixel, position, null, colors[c], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }


        public double Cos(double angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Cos
        {
            return Math.Cos(-angle * 2 * Math.PI);
        }


        public void Del<T>(List<T> table, T value) // https://pico-8.fandom.com/wiki/Del
        {
            table.Remove(value);
        }


        public void Map(double celx, double cely, double sx, double sy, double celw, double celh) // https://pico-8.fandom.com/wiki/Map
        {
            int cxFlr = (int)Math.Floor(celx);
            int cyFlr = (int)Math.Floor(cely);
            int sxFlr = (int)Math.Floor(sx);
            int syFlr = (int)Math.Floor(sy);
            int cwFlr = (int)Math.Floor(celw);
            int chFlr = (int)Math.Floor(celh);

            for (int i = 0; i <= cwFlr; i++)
            {
                for (int j = 0; j <= chFlr; j++)
                {
                    Spr(Mget(i + cxFlr, j + cyFlr), sxFlr + i * 8, syFlr + j * 8);
                }
            }
        }


        public int MgetOld(double celx, double cely)
        {
            int xFlr = (int)Math.Floor(celx);
            int yFlr = (int)Math.Floor(cely);

            string MapData = new(MapFile.Map1.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray());

            char c = MapData[xFlr + (yFlr * 128)];

            int IntC = 0;

            if (c >= 48 && c <= 57)
            {
                IntC = c - '0';
            }
            else if (c >= 97 && c <= 102)
            {
                IntC = 10 + c - 'a';
            }

            return IntC;

        }


        public int Mget(double celx, double cely) // https://pico-8.fandom.com/wiki/Mget
        {
            int xFlr = (int)Math.Floor(celx);
            int yFlr = (int)Math.Floor(cely);

            int mval = Map1[xFlr + (yFlr * 128)];

            return mval;
        }


        public void Mset(double celx, double cely, double snum = 0) // https://pico-8.fandom.com/wiki/Mset
        {
            int xFlr = (int)Math.Floor(celx);
            int yFlr = (int)Math.Floor(cely);
            int sFlr = (int)Math.Floor(snum);

            Map1[xFlr + (yFlr * 128)] = sFlr;
        }


        public void Pal() // https://pico-8.fandom.com/wiki/Pal
        {
            Array.Copy(resetSprColors, sprColors, resetSprColors.Length);
            Array.Copy(resetColors, colors, resetColors.Length);
        }


        public void Pal(double c0 = 0, double c1 = 0) // https://pico-8.fandom.com/wiki/Pal
        {
            int c0Flr = (int)Math.Floor(c0);
            int c1Flr = (int)Math.Floor(c1);

            sprColors[c0Flr] = resetColors[c1Flr]; // idk resetSpr or not
            colors[c0Flr] = resetColors[c1Flr];
        }


        public void Palt() // https://pico-8.fandom.com/wiki/Palt
        {
            sprColors[0].A = 0;
            resetSprColors[0].A = 0;
            for (int i = 1; i <= 15; i++)
            {
                sprColors[i].A = 255;
                resetSprColors[i].A = 255;
            }
        }


        public void Palt(double col, bool t) // https://pico-8.fandom.com/wiki/Palt
        {
            int colFlr = (int)Math.Floor(col);
            
            if (!t)
            {
                sprColors[colFlr].A = 255;
                resetSprColors[colFlr].A = 255;
            }
            else
            {
                sprColors[colFlr].A = 0;
                resetSprColors[colFlr].A = 0;
            }
        }


        public void Print(string str, double x, double y, double c) // https://pico-8.fandom.com/wiki/Print
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int cFlr = (int)Math.Floor(c);

            int charWidth = 4;
            //int charHeight = 5;

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            for (int s = 0; s < str.Length; s++)
            {
                char letter = str[s];

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (Font.chars[letter][i, j] == 1)
                        {
                            var charStartX = (s * charWidth + xFlr + j - CameraOffset.Item1) * cellWidth;
                            //var charEndX = charStartX + cellWidth - CameraOffset.Item1;
                            var charStartY = (yFlr + i - CameraOffset.Item2) * cellHeight;

                            Vector2 position = new Vector2(charStartX, charStartY);
                            Vector2 size = new(cellWidth, cellHeight);

                            batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                        }
                    }
                }
            }
        }


        public void Pset(double x, double y, double c) // https://pico-8.fandom.com/wiki/Pset
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            //float yFlr = (float)(Math.Floor(y) - 0.5);
            int cFlr = (int)Math.Floor(c);

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            // Calculate the position and size of the line
            Vector2 position = new((xFlr - CameraOffset.Item1) * cellWidth, (yFlr - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);
            
            // Draw the line
            batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }


        public void Rectfill(double x1, double y1, double x2, double y2, double c) // https://pico-8.fandom.com/wiki/Rectfill
        {
            int x1Flr = (int)Math.Floor(x1);
            int y1Flr = (int)Math.Floor(y1);
            int x2Flr = (int)Math.Floor(x2);
            int y2Flr = (int)Math.Floor(y2);
            int cFlr = (int)Math.Floor(c);

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            var rectStartX = (x1Flr - CameraOffset.Item1) * cellWidth;
            var rectStartY = (y1Flr - CameraOffset.Item2) * cellHeight;

            var rectSizeX = (x2Flr - x1Flr + 1) * cellWidth;
            var rectSizeY = (y2Flr - y1Flr + 1) * cellHeight;

            //var rectEndX = (x2Flr - CameraOffset.Item1) * cellWidth;
            //var rectThickness = (y2Flr - y1Flr) * cellHeight;
            //batch.DrawLine(pixel, new Vector2(rectStartX, rectStartY), new Vector2(rectEndX, rectStartY), colors[cFlr], rectThickness);

            Vector2 position = new(rectStartX, rectStartY);
            Vector2 size = new(rectSizeX, rectSizeY);

            batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }


        public double Sin(double angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Sin
        {
            return Math.Sin(-angle * 2 * Math.PI);
        }


        public void Spr(double spriteNumber, double x, double y, double w = 1.0, double h = 1.0, bool flip_x = false, bool flip_y = false) // https://pico-8.fandom.com/wiki/Spr
        {
            int spriteNumberFlr = (int)Math.Floor(spriteNumber);
            int xFlr = (int)Math.Floor(x) - 8;
            int yFlr = (int)Math.Floor(y) - 8;
            int wFlr = (int)Math.Floor(w);
            int hFlr = (int)Math.Floor(h);

            var spriteWidth = 8;
            var spriteHeight = 8;

            int spriteX = spriteNumberFlr % 16 * spriteWidth;
            int spriteY = spriteNumberFlr / 16 * spriteHeight;

            int colorCache = 0;

            for (int i = 0; i < resetSprColors.Length; i++)
            {
                if (sprColors[i] != resetSprColors[i])
                {
                    for (int j = 0; j < resetSprColors.Length; j++)
                    {
                        if (sprColors[i] == resetSprColors[j])
                        {
                            colorCache += (i * 100 + j) * 1000;
                            break;
                        }
                    }
                }
            }

            if (!spriteTextures.TryGetValue(spriteNumberFlr + colorCache, out var texture))
            {
                texture = CreateTextureFromSpriteData(SpriteSheets.SpriteSheet1, spriteX, spriteY, spriteWidth * wFlr, spriteHeight * hFlr);
                spriteTextures[spriteNumberFlr + colorCache] = texture;
            }

            // Get the size of the viewport
            int viewportWidth = batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = batch.GraphicsDevice.Viewport.Height;

            //Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(((flip_x ? xFlr + (2 * spriteWidth * wFlr) - spriteWidth : xFlr + spriteWidth) - CameraOffset.Item1) * cellWidth, ((flip_y ? yFlr + (2 * spriteHeight * hFlr) - spriteHeight : yFlr + spriteHeight) - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);
            SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flip_y ? SpriteEffects.FlipVertically : SpriteEffects.None);

            batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
        }


        public void Sspr(double sx, double sy, double sw, double sh, double dx, double dy, double dw = -1, double dh = -1, bool flip_x = false, bool flip_y = false) // https://pico-8.fandom.com/wiki/Sspr
        {
            int sxFlr = (int)Math.Floor(sx);
            int syFlr = (int)Math.Floor(sy);
            int swFlr = (int)Math.Floor(sw);
            int shFlr = (int)Math.Floor(sh);
            int dxFlr = (int)Math.Floor(dw) > 8 ? (int)Math.Floor(dx) - 4 : (int)Math.Floor(dx) - 8;
            int dyFlr = (int)Math.Floor(dh) > 8 ? (int)Math.Floor(dy) - 4 : (int)Math.Floor(dy) - 8;
            int dwFlr = dw == -1 ? swFlr : (int)Math.Floor(dw) > 8 ? (int)Math.Floor(dw / 4) + 1 : (int)Math.Floor(dw / 8);
            int dhFlr = dh == -1 ? shFlr : (int)Math.Floor(dh) > 8 ? (int)Math.Floor(dh / 4) + 1 : (int)Math.Floor(dh / 8);

            var spriteWidth = swFlr;
            var spriteHeight = shFlr;

            //int spriteX = spriteNumberFlr % 16 * spriteWidth;
            //int spriteY = spriteNumberFlr / 16 * spriteHeight;

            int colorCache = 0;

            for (int i = 0; i < resetSprColors.Length; i++)
            {
                if (sprColors[i] != resetSprColors[i])
                {
                    for (int j = 0; j < resetSprColors.Length; j++)
                    {
                        if (sprColors[i] == resetSprColors[j])
                        {
                            colorCache += (i * 100 + j) * 1000;
                            break;
                        }
                    }
                }
            }

            var spriteNumberFlr = (sxFlr * 100) + (syFlr * 100) + (swFlr * 100) + (shFlr * 100);

            if (!spriteTextures.TryGetValue(spriteNumberFlr + colorCache, out var texture))
            {
                texture = CreateTextureFromSpriteData(SpriteSheets.SpriteSheet1, sxFlr, syFlr, swFlr, shFlr);
                spriteTextures[spriteNumberFlr + colorCache] = texture;
            }

            // Get the size of the viewport
            int viewportWidth = batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = batch.GraphicsDevice.Viewport.Height;

            //Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(((flip_x ? dxFlr + (2 * spriteWidth * swFlr) - spriteWidth : dxFlr + spriteWidth) - CameraOffset.Item1) * cellWidth, ((flip_y ? dyFlr + (2 * spriteHeight * shFlr) - spriteHeight : dyFlr + spriteHeight) - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(dwFlr * cellWidth, dhFlr * cellHeight);
            SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flip_y ? SpriteEffects.FlipVertically : SpriteEffects.None);

            batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            foreach (var texture in spriteTextures.Values)
            {
                texture.Dispose();
            }
            spriteTextures.Clear();
        }

    }
}