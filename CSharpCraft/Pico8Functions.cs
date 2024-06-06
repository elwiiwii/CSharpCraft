using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft
{
    public class Pico8Functions : IDisposable
    {
        private SpriteBatch batch;
        private readonly GraphicsDevice graphicsDevice;
        private Texture2D pixel;
        private Dictionary<int, Texture2D> spriteTextures = new Dictionary<int, Texture2D>();

        // Add a constructor that takes a SpriteBatch as a parameter
        public Pico8Functions(Texture2D pixel, SpriteBatch batch, GraphicsDevice graphicsDevice)
        {
            this.batch = batch;
            this.graphicsDevice = graphicsDevice;
            this.pixel = pixel;
        }

        private static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return new Color(r, g, b);
        }

        // pico-8 colors
        private Color[] colors = new Color[]
        {
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
        };
        private Dictionary<Color, int> paletteSwap = new Dictionary<Color, int>();

        public Texture2D CreateTextureFromSpriteData(string spriteData, int spriteWidth, int spriteHeight)
        {
            spriteData = new string(spriteData.Where(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')).ToArray());

            int width = spriteWidth * 16; // 16 sprites per row
            int height = spriteData.Length / width;

            Texture2D texture = new Texture2D(graphicsDevice, width, height);

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

        public void circ(int x, int y, double r, int c)
        {
            if (r < 0) return; // If r is negative, the circle is not drawn

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            for (int i = ((int)x - (int)r); i <= (int)x + (int)r; i++)
            {
                for (int j = ((int)y - (int)r); j <= (int)y + (int)r; j++)
                {
                    // Check if the point 0.36 units into the grid space from the center of the circle is within the circle
                    double offsetX = (i < (int)x) ? 0.36D : -0.36D;
                    double offsetY = (j < (int)y) ? 0.36D : -0.36D;
                    double gridCenterX = i + offsetX;
                    double gridCenterY = j + offsetY;

                    bool isCurrentInCircle = Math.Pow(gridCenterX - (int)x, 2) + Math.Pow(gridCenterY - (int)y, 2) <= (int)r * (int)r;

                    // Check all four adjacent grid spaces
                    bool isRightOutsideCircle = Math.Pow((i + 1 + offsetX) - (int)x, 2) + Math.Pow((j + offsetY) - (int)y, 2) > (int)r * (int)r;
                    bool isLeftOutsideCircle = Math.Pow((i - 1 + offsetX) - (int)x, 2) + Math.Pow((j + offsetY) - (int)y, 2) > (int)r * (int)r;
                    bool isUpOutsideCircle = Math.Pow((i + offsetX) - (int)x, 2) + Math.Pow((j + 1 + offsetY) - (int)y, 2) > (int)r * (int)r;
                    bool isDownOutsideCircle = Math.Pow((i + offsetX) - (int)x, 2) + Math.Pow((j - 1 + offsetY) - (int)y, 2) > (int)r * (int)r;

                    if (isCurrentInCircle && (isRightOutsideCircle || isLeftOutsideCircle || isUpOutsideCircle || isDownOutsideCircle))
                    {
                        // Calculate the position and size of the line
                        Vector2 position = new Vector2(i * cellWidth, j * cellHeight);
                        Vector2 size = new Vector2(cellWidth, cellHeight);

                        // Draw the line
                        batch.Draw(pixel, position, null, colors[c], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public void circfill(double x, double y, double r, int c)
        {
            if (r < 0) return; // If r is negative, the circle is not drawn

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            for (int i = ((int)x - (int)r); i <= (int)x + (int)r; i++)
            {
                for (int j = ((int)y - (int)r); j <= (int)y + (int)r; j++)
                {
                    // Check if the point 0.36 units into the grid space from the center of the circle is within the circle
                    double offsetX = (i < (int)x) ? 0.36D : -0.36D;
                    double offsetY = (j < (int)y) ? 0.36D : -0.36D;
                    double gridCenterX = i + offsetX;
                    double gridCenterY = j + offsetY;

                    if (Math.Pow(gridCenterX - (int)x, 2) + Math.Pow(gridCenterY - (int)y, 2) <= (int)r * (int)r)
                    {
                        // Calculate the position and size
                        Vector2 position = new Vector2(i * cellWidth, j * cellHeight);
                        Vector2 size = new Vector2(cellWidth, cellHeight);

                        // Draw
                        batch.Draw(pixel, position, null, colors[c], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public void print(string str, int x, int y, int c)
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

        public void rectfill(int x1, int y1, int x2, int y2, int c)
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

        public void spr(int spriteNumber, double x, double y, double w = 1.0, double h = 1.0, bool flip_x = false, bool flip_y = false)
        {
            var spriteWidth = 8;
            var spriteHeight = 8;

            if (!spriteTextures.TryGetValue(spriteNumber, out var texture))
            {
                texture = CreateTextureFromSpriteData(SpriteSheets.SpriteSheet1, spriteWidth, spriteHeight);
                spriteTextures[spriteNumber] = texture;
            }

            int spriteX = (spriteNumber % 16) * spriteWidth;
            int spriteY = (spriteNumber / 16) * spriteHeight;

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
                        Vector2 position = new Vector2(((int)x + (flip_x ? -j : j)) * cellWidth, ((int)y + (flip_y ? -i : i)) * cellHeight);
                        Vector2 size = new Vector2(cellWidth, cellHeight);

                        // Draw the pixel
                        batch.Draw(pixel, position, null, color, 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var texture in spriteTextures.Values)
            {
                texture.Dispose();
            }
            spriteTextures.Clear();
        }


    }
}