using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft
{
    public class TitleScreen(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, List<IGameMode> gameModes) : IGameMode
    {

        public string GameModeName { get => "TitleScreen"; }

        private int menuSelected;
        public int currentGameMode;

        private int Loop<T>(int sel, List<T> l)
        {
            var lp = l.Count - 1;
            return ((sel % lp) + lp) % lp;
        }

        public void Init()
        {
            menuSelected = 0;
            currentGameMode = 0;
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();

            if (currentGameMode == 0) // titlescreen
            {
                if (p8.Btnp(2)){ menuSelected -= 1; }
                if (p8.Btnp(3)) { menuSelected += 1; }

                menuSelected = Loop(menuSelected, gameModes);

                if (state.IsKeyDown(Keys.Enter) || p8.Btnp(4) || p8.Btnp(5))
                {
                    gameModes[menuSelected + 1].Init();
                    currentGameMode = menuSelected + 1;
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

            Texture2D logo = textureDictionary["CSharpCraftLogo"];
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
                if (gameMode == gameModes[0])
                {
                    continue;
                }
                else
                {
                    p8.Print(gameMode.GameModeName, 8, 62 + i, 7);
                    i += 6;
                }
            }
        }


    }
}
