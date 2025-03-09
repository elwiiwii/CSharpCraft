using CSharpCraft.OptionsMenu;
using System.IO.Pipelines;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;


namespace CSharpCraft
{
    public class TitleScreen : IScene, IDisposable
    {
        public string SceneName { get => "TitleScreen"; }
        private Pico8Functions p8;

        private int menuSelected;
        private KeyboardState prevState;
        private int frame;

        public void Init(Pico8Functions pico8)
        {
            p8 = pico8;

            menuSelected = 0;
            prevState = Keyboard.GetState();
            frame = 0;
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Q) && !prevState.IsKeyDown(Keys.Q))
            {
                Environment.Exit(0);
            }

            if (p8.Btnp(2)){ menuSelected -= 1; }
            if (p8.Btnp(3)) { menuSelected += 1; }

            menuSelected = GeneralFunctions.Loop(menuSelected, p8.scenes);

            if ((state.IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter)) || p8.Btnp(4) || p8.Btnp(5))
            {
                p8.LoadCart(p8.scenes[menuSelected]);
            }

            prevState = state;
        }

        public void Draw()
        {
            p8.batch.GraphicsDevice.Clear(Color.Black);

            // Get the size of the viewport
            int viewportWidth = p8.batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = p8.batch.GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(1 * cellWidth, 1 * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);

            Texture2D logo = p8.textureDictionary["CSharpCraftLogo"];
            p8.batch.Draw(logo, position, null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            p8.Print("c# craft 0.0.1", 0, 18, 6);
            p8.Print("by nusan-2016 and ellie-2024", 0, 24, 6);

            //p8.Print("musicNote", 3, 36, 14);
            //p8.Print("musicNote", 11, 38, 14);
            //p8.Print("musicNote", 19, 36, 14);
            //p8.Print("musicNote", 27, 34, 14);

            p8.Print("choose a game mode", 0, 50, 6);
            p8.Print(">", 0, 62 + (menuSelected * 6), 7);

            int i = 0;
            foreach (IScene scene in p8.scenes)
            {
                p8.Print(scene.SceneName, 8, 62 + i, 7);
             
                i += 6;
            }
        }

        public string SpriteData => @"";
        public string FlagData => @"";
        public string MapData => @"";
        public Dictionary<string, List<(List<(string name, bool loop)> tracks, int group)>> Music => new();
        public Dictionary<string, Dictionary<int, string>> Sfx => new();
        public void Dispose()
        {

        }

    }
}
