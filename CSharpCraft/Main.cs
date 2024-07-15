using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        private List<SoundEffect> music = [];
        private Options options;
        private OptionsFile optionsFile;
        private Pico8Functions p8;
        private Pcraft pcraft;
        private Texture2D pixel;
        private List<SoundEffect> soundEffects = [];
        private TitleScreen titleScreen;

        private int currentGameMode;

#nullable disable

        private double elapsedSeconds = 0.0;

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

            optionsFile = OptionsFile.Initialize();

        }

        
        protected override void Initialize()
        {
            base.Initialize();

            //optionsFile = OptionsFile.Initialize();

            UpdateViewport();

            Array.Copy(p8.colors, p8.resetColors, p8.colors.Length);
            Array.Copy(p8.colors, p8.sprColors, p8.colors.Length);
            Array.Copy(p8.colors, p8.resetSprColors, p8.colors.Length);

            p8.Palt();

            gameModes.Clear();

            gameModes.Add(pcraft);
            gameModes.Add(options);

            currentGameMode = -1;

            titleScreen.Init();
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
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);

            if (state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Right)))
            {

            }

            currentGameMode = titleScreen.currentGameMode;

            if (currentGameMode > -1 && currentGameMode < gameModes.Count)
            {
                gameModes[currentGameMode].Update();

                if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.R)) { gameModes[currentGameMode].Init(); }

                if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Q))
                {
                    p8.SoundDispose();
                    currentGameMode = -1;
                    titleScreen.Init();
                }

            }
            else if (currentGameMode == -1)
            {
                if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Q))
                {
                    Environment.Exit(0);
                }

                titleScreen.Update();
            }

            p8.prev0 = state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Left));
            p8.prev1 = state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Right));
            p8.prev2 = state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Up));
            p8.prev3 = state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Down));
            p8.prev4 = state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Menu));
            p8.prev5 = state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Interact));

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            p8.Pal();
            p8.Palt();

            if (currentGameMode > -1 && currentGameMode < gameModes.Count)
            {
                gameModes[currentGameMode].Draw();
            }
            else if (currentGameMode == -1)
            {
                titleScreen.Draw();
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
            
            for (int i = 0; i <= 6; i++)
            {
                string fileName = $"Content/Music/music_{i}.wav";
                using var stream = TitleContainer.OpenStream(fileName);
                {
                    music.Add(SoundEffect.FromStream(stream));
                }
            }

            for (int i = 0; i <= 21; i++)
            {
                string fileName = $"Content/Sfx/sfx_{i}.wav";
                using var stream = TitleContainer.OpenStream(fileName);
                {
                    soundEffects.Add(SoundEffect.FromStream(stream));
                }
            }

            p8 = new Pico8Functions(soundEffects, music, pixel, batch, GraphicsDevice, optionsFile);
            pcraft = new Pcraft(p8);
            options = new Options(p8);
            titleScreen = new TitleScreen(p8, logo, batch, GraphicsDevice, gameModes);
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