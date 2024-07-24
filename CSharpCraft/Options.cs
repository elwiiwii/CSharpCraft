using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft
{
    public class Options(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, KeyboardOptionsFile keyboardOptionsFile, List<IGameMode> optionsModes) : IGameMode
    {

        public string GameModeName { get => "options"; }

        private int menuX;
        private int menuY;
        private int menuWidth;
        private int menuLength;

        private int Loop(int sel, int size)
        {
            return ((sel % size) + size) % size;
        }

        public void Init()
        {
            menuX = 0;
            menuY = 0;
            menuWidth = 2;
            menuLength = 2;
            
        }

        public void Update()
        {
            if (menuY == 3)
            {
                optionsModes[1].Init();
                optionsModes[1].Update();
            }

            if (p8.Btnp(0)) { menuX -= 1; }
            if (p8.Btnp(1)) { menuX += 1; }
            if (p8.Btnp(2)) { menuY -= 1; }
            if (p8.Btnp(3)) { menuY += 1; }

        }

        public void Draw()
        {
            graphicsDevice.Clear(Color.Black);
            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);

            batch.Draw(textureDictionary["OptionsMenuBackground"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["Arrow"], new Vector2(0 * cellW, 1 * cellH), null, p8.colors[menuX == 0 ? 7 : 5], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Print("back", 5, 1, menuX == 0 ? 7 : 5);

            for (int i = 0; i <= 1; i++)
            {
                var position2 = new Vector2((15 + (58 * i)) * cellW, 16 * cellH);
                var position3 = new Vector2((16 + (58 * i)) * cellW, 17 * cellH);
                batch.Draw(textureDictionary["SelectorBorder"], position2, null, p8.colors[7], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["SelectorCenter"], position3, null, p8.colors[menuX == 1 ? 2 : 0], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }

            var position = new Vector2((15 + (58 * menuX)) * cellW, 22 * cellH);
            batch.Draw(textureDictionary["OptionsMenuTab"], position, null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            p8.Rectfill(15, 22, 53, 27, 7);
            p8.Rectfill(16, 22, 52, 27, 0);

            p8.Print("controls", 19, 19, 7);
            p8.Print("graphics", 19 + 58, 18, 7);

            optionsModes[1].Draw();
        }


    }
}
