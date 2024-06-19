using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft
{
    public class Pico8Functions(Texture2D pixel, SpriteBatch batch, GraphicsDevice graphicsDevice) : IDisposable
    {
        private readonly SpriteBatch batch = batch;
        private readonly GraphicsDevice graphicsDevice = graphicsDevice;
        private readonly Texture2D pixel = pixel;
        private readonly Dictionary<int, Texture2D> spriteTextures = new();
        private int[] Map1 = new int[64 * 64];
        private int[] Map2 = new int[32 * 32];

        private static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return new Color(r, g, b);
        }

        // pico-8 colors
        private readonly Color[] colors =
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
        ];
        private readonly Dictionary<Color, int> paletteSwap = new();

        public Texture2D CreateTextureFromSpriteData(string spriteData, int spriteWidth, int spriteHeight)
        {
            spriteData = new string(spriteData.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray());

            int width = spriteWidth * 16; // 16 sprites per row
            int height = spriteData.Length / width;

            Texture2D texture = new(graphicsDevice, width, height);

            Color[] colorData = new Color[width * height];

            for (int i = 0; i < spriteData.Length; i++)
            {
                char c = spriteData[i];
                int colorIndex = Convert.ToInt32(c.ToString(), 16); // Convert hex to int
                Color color = colors[colorIndex]; // Convert the PICO-8 color index to a Color
                colorData[i] = color;
            }

            texture.SetData(colorData);

            return texture;
        }

        public void Circ(double x, double y, double r, int c)
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
                        Vector2 position = new(i * cellWidth, j * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw the line
                        batch.Draw(pixel, position, null, colors[c], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public void Circfill(double x, double y, double r, int c)
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
                        Vector2 position = new(i * cellWidth, j * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw
                        batch.Draw(pixel, position, null, colors[c], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public static int MgetOld(double celx, double cely)
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

        public int Mget(double celx, double cely)
        {
            int xFlr = (int)Math.Floor(celx);
            int yFlr = (int)Math.Floor(cely);

            int mval = Map1[xFlr + (yFlr * 64)];

            return mval;
        }

        public void Mset(double celx, double cely, double snum = 0)
        {
            int xFlr = (int)Math.Floor(celx);
            int yFlr = (int)Math.Floor(cely);
            int sFlr = (int)Math.Floor(snum);

            Map1[xFlr + (yFlr * 64)] = sFlr;
        }

        public void Print(string str, int x, int y, int c)
        {
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
                            var charStartX = (s * charWidth + x + j) * cellWidth;
                            var charEndX = charStartX + cellWidth;
                            var charStartY = (int)((y + i + 0.5) * cellHeight);
                            batch.DrawLine(pixel, new Vector2(charStartX, charStartY), new Vector2(charEndX, charStartY), colors[c], cellHeight);
                        }
                    }
                }
            }
        }

        public void Pset(double x, double y, double c)
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
            Vector2 position = new(xFlr * cellWidth, yFlr * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);

            // Draw the line
            batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }

        public void Rectfill(int x1, int y1, int x2, int y2, int c)
        {
            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            var rectStartX = x1 * cellWidth;
            var rectEndX = x2 * cellWidth;
            var rectStartY = (y1 + ((y2 - y1) / 2)) * cellHeight;
            var rectThickness = (y2 - y1) * cellHeight;
            batch.DrawLine(pixel, new Vector2(rectStartX, rectStartY), new Vector2(rectEndX, rectStartY), colors[c], rectThickness);
        }

        public void Spr(int spriteNumber, double x, double y, double w = 1.0, double h = 1.0, bool flip_x = false, bool flip_y = false)
        {
            var spriteWidth = 8;
            var spriteHeight = 8;

            if (!spriteTextures.TryGetValue(spriteNumber, out var texture))
            {
                texture = CreateTextureFromSpriteData(SpriteSheets.SpriteSheet1, spriteWidth, spriteHeight);
                spriteTextures[spriteNumber] = texture;
            }

            int spriteX = spriteNumber % 16 * spriteWidth;
            int spriteY = spriteNumber / 16 * spriteHeight;

            // Get the size of the viewport
            int viewportWidth = batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = batch.GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            // Get all pixel data from the texture
            Color[] allColors = new Color[texture.Width * texture.Height];
            texture.GetData(allColors);

            // Draw each pixel of the sprite
            for (int i = 0; i < spriteHeight * h; i++)
            {
                for (int j = 0; j < spriteWidth * w; j++)
                {
                    // Get the color of the pixel
                    Color color = allColors[(spriteY + i) * texture.Width + spriteX + j];

                    // If the color is transparent or black, don't draw anything
                    if (color.A != 0 && color != colors[0]) // Add this condition
                    {
                        // Calculate the position and size
                        Vector2 position = new(((int)x + (flip_x ? -j : j)) * cellWidth, ((int)y + (flip_y ? -i : i)) * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw the pixel
                        batch.Draw(pixel, position, null, color, 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
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