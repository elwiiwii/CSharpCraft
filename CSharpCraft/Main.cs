using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace CSharpCraft
{

    partial class FNAGame : Game
    {
        [STAThread]
        static void Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            using FNAGame g = new();
            g.Run();
        }

#nullable enable

        private SpriteBatch batch;
        private List<IGameMode> gameModes = [];
        private GraphicsDeviceManager graphics;
        private Texture2D logo;
        private List<SoundEffect> music;
        private Pico8Functions p8;
        private PcraftCode pcraft;
        private Texture2D pixel;
        private List<SoundEffect> soundEffects;

#nullable disable

        private int frameCounter = 0;
        private double elapsedSeconds = 0.0;
        private int menuSelected = 0;
        private int playing = -1;

        private FNAGame()
        {
            graphics = new GraphicsDeviceManager(this);

            // Allow the user to resize the window
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            // All content loaded will be in a "Content" folder
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 512;
            graphics.PreferredBackBufferHeight = 512;
            graphics.IsFullScreen = false;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / 30));
            graphics.SynchronizeWithVerticalRetrace = true;

        }


        protected override void Initialize()
        {
            base.Initialize();

            UpdateViewport();

            Array.Copy(p8.colors, p8.resetColors, p8.colors.Length);
            Array.Copy(p8.colors, p8.sprColors, p8.colors.Length);
            Array.Copy(p8.colors, p8.resetSprColors, p8.colors.Length);

            p8.Palt();

            gameModes.Add(pcraft);
        }


        protected override void Update(GameTime gameTime)
        {
            double fps = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;
            elapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;
            if (elapsedSeconds >= 1.0)
            {
                Console.WriteLine($"FPS: {fps}");
                elapsedSeconds = 0.0;
            }

            KeyboardState state = Keyboard.GetState();

            if (playing < 0)
            {
                if (state.IsKeyDown(Keys.Enter))
                {
                    gameModes[menuSelected].Init();
                    playing = menuSelected;
                }
            }
            if (playing > -1 && playing < gameModes.Count)
            {
                if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.R)) gameModes[menuSelected].Init();
                gameModes[menuSelected].Update();
            }

            p8.prev0 = state.IsKeyDown(Keys.A);
            p8.prev1 = state.IsKeyDown(Keys.D);
            p8.prev2 = state.IsKeyDown(Keys.W);
            p8.prev3 = state.IsKeyDown(Keys.S);
            p8.prev4 = state.IsKeyDown(Keys.M);
            p8.prev5 = state.IsKeyDown(Keys.N);

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            frameCounter++;

            GraphicsDevice.Clear(Color.Black);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            p8.Pal();
            p8.Palt();

            if (playing < 0)
            {
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
                p8.Print(">", 0, 62, 7);

                int i = 0;
                foreach (var gameMode in gameModes)
                {
                    p8.Print(gameMode.GameModeName, 8, 62 + i, 7);
                    i += 6;
                }
            }
            if (playing > -1 && playing < gameModes.Count)
            {
                gameModes[menuSelected].Draw();
            }

            // Draw the grid
            /*for (int i = 0; i <= 128; i++)
            {
                // Draw vertical lines
                batch.DrawLine(pixel, new Vector2(i * cellWidth, 0), new Vector2(i * cellWidth, viewportHeight), Color.White, 1);
                // Draw horizontal lines
                batch.DrawLine(pixel, new Vector2(0, i * cellHeight), new Vector2(viewportWidth, i * cellHeight), Color.White, 1);
            }*/

            batch.End();

            base.Draw(gameTime);
        }


        protected override void LoadContent()
        {
            // Create the batch...
            batch = new SpriteBatch(GraphicsDevice);

            // Create a 1x1 white pixel texture for drawing lines
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            logo = Content.Load<Texture2D>("Graphics/CSharpCraftLogo.png");

            List<SoundEffect> music = [];
            for (int i = 0; i <= 6; i++)
            {
                string fileName = $"Content/Music/music_{i}.wav";
                using var stream = TitleContainer.OpenStream(fileName);
                {
                    music.Add(SoundEffect.FromStream(stream));
                }
            }

            List<SoundEffect> soundEffects = [];
            for (int i = 0; i <= 21; i++)
            {
                string fileName = $"Content/Sfx/sfx_{i}.wav";
                using var stream = TitleContainer.OpenStream(fileName);
                {
                    soundEffects.Add(SoundEffect.FromStream(stream));
                }
            }

            p8 = new Pico8Functions(soundEffects, music, pixel, batch, GraphicsDevice);
            pcraft = new PcraftCode(p8);
        }


        protected override void UnloadContent()
        {
            batch.Dispose();
            pixel.Dispose();
            p8.Dispose();
            logo.Dispose();
            if (soundEffects != null)
            {
                foreach (var soundEffect in soundEffects)
                {
                    soundEffect?.Dispose();
                }
            }
            if (music != null)
            {
                foreach (var song in music)
                {
                    song?.Dispose();
                }
            }
        }

        private void UpdateViewport()
        {
            // Calculate the size of the largest square that fits in the client area
            int maxSize = Math.Min(Window.ClientBounds.Width, Window.ClientBounds.Height);

            // Round the size down to the nearest multiple of 128
            int size = (maxSize / 128) * 128;

            // Calculate the exact center of the client area
            double centerX = Window.ClientBounds.Width / 2.0f;
            double centerY = Window.ClientBounds.Height / 2.0f;

            // Calculate the top left corner of the square so that its center aligns with the client area's center
            int left = (int)Math.Round(centerX - size / 2.0f);
            int top = (int)Math.Round(centerY - size / 2.0f);

            // Set the viewport to the square area
            GraphicsDevice.Viewport = new Viewport(left, top, size, size);
        }


        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateViewport();
        }

    }
}