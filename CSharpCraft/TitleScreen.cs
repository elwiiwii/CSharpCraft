using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft
{
    public class TitleScreen(Pico8Functions p8, Texture2D logo, SpriteBatch batch, GraphicsDevice graphicsDevice, List<IGameMode> gameModes) : IGameMode
    {

        public string GameModeName { get => "TitleScreen"; }

        private readonly SpriteBatch batch = batch;
        private readonly GraphicsDevice graphicsDevice = graphicsDevice;
        private readonly Texture2D logo = logo;
        private readonly Pico8Functions p8 = p8;
        private readonly List<IGameMode> gameModes = gameModes;

        private int menuSelected;
        public int currentGameMode;

        private int Loop<T>(int sel, List<T> l)
        {
            var lp = l.Count;
            return ((sel % lp) + lp) % lp;
        }

        public void Init()
        {
            menuSelected = 0;
            currentGameMode = -1;
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();

            if (currentGameMode == -1) // titlescreen
            {
                if (p8.Btnp(2)) { menuSelected -= 1; }
                if (p8.Btnp(3)) { menuSelected += 1; }

                menuSelected = Loop(menuSelected, gameModes);

                if (state.IsKeyDown(Keys.Enter))
                {
                    gameModes[menuSelected].Init();
                    currentGameMode = menuSelected;
                }
            }
        }

        public void Draw()
        {
            batch.GraphicsDevice.Clear(Color.Black);

            // Get the size of the viewport
            int viewportWidth = batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = batch.GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(1 * cellWidth, 1 * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);

            batch.Draw(logo, position, null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            p8.Print("c# craft 0.0.1", 0, 18, 6);
            p8.Print("by nusan-2016 and ellie-2024", 0, 24, 6);

            //p8.Print("musicNote", 3, 36, 14);
            //p8.Print("musicNote", 11, 38, 14);
            //p8.Print("musicNote", 19, 36, 14);
            //p8.Print("musicNote", 27, 34, 14);

            p8.Print("choose a game mode", 0, 50, 6);
            p8.Print(">", 0, 62 + (menuSelected * 6), 7);

            int i = 0;
            foreach (var gameMode in gameModes)
            {
                p8.Print(gameMode.GameModeName, 8, 62 + i, 7);
                i += 6;
            }
        }


    }
}
