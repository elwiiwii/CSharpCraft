﻿using CSharpCraft.OptionsMenu;
using CSharpCraft.Pcraft;
using CSharpCraft.Pico8;
using CSharpCraft.RaceMode;
using CSharpCraft.Credits;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft
{

    class FNAGame : Game
    {
        [STAThread]
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL3");
            ArgumentNullException.ThrowIfNull(args);

            using FNAGame g = new();
            g.Run();
        }

#nullable enable

        private SpriteBatch batch;
        private List<IScene> scenes = [];
        private GraphicsDeviceManager graphics;
        private Pico8Functions p8;

        private OptionsFile optionsFile;

        private Dictionary<string, Texture2D> textureDictionary = new();
        private Dictionary<string, SoundEffect> musicDictionary = new();
        private Dictionary<string, SoundEffect> soundEffectDictionary = new();

        private Texture2D pixel;
        private KeyboardState prevState;

#nullable disable

        private double elapsedSeconds = 0.0;
        string graphicsFolderPath = "Content/Graphics";
        string musicFolderPath = "Content/Music";
        string sfxFolderPath = "Content/Sfx";

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
            this.TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / 30.0));
            graphics.SynchronizeWithVerticalRetrace = true;

            optionsFile = OptionsFile.Initialize();
            prevState = Keyboard.GetState();
        }


        protected override void Initialize()
        {
            base.Initialize();

            //optionsFile = OptionsFile.Initialize();

            UpdateViewport();

            scenes.Add(new PcraftSingleplayer());
            scenes.Add(new MainRace());
            scenes.Add(new ControlsOptions());
            scenes.Add(new CreditsScene());
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

            if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.R) && !prevState.IsKeyDown(Keys.R))
            {
                p8.LoadCart(p8._cart);
            }

            if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Q) && !prevState.IsKeyDown(Keys.Q))
            {
                p8.LoadCart(new TitleScreen());
            }

            //List<string> keybinds = new();
            //foreach (string keybind in keybinds)
            //{
            //    (p8)Enum.Parse(typeof(p8), $"prev{keybind}") = state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), keybind));
            //}

            p8.Update();

            prevState = state;

            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            p8.Pal();
            p8.Palt();
            p8.Draw();

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            // Draw the grid
            /*for (int i = 0; i <= 128; i++)
            {
                // Draw vertical lines
                batch.DrawLine(pixel, new Vector2(i * cellW, 0), new Vector2(i * cellW, viewportHeight), Color.White, 1);
                // Draw horizontal lines
                batch.DrawLine(pixel, new Vector2(0, i * cellH), new Vector2(viewportWidth, i * cellH), Color.White, 1);
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

            string[] graphicsFiles = Directory.GetFiles(graphicsFolderPath, "*.png");
            foreach (string file in graphicsFiles)
            {
                using Stream stream = TitleContainer.OpenStream(file);
                {
                    Texture2D texture = Texture2D.FromStream(GraphicsDevice, stream);
                    string textureName = Path.GetFileNameWithoutExtension(file);
                    textureDictionary.Add(textureName, texture);
                }
            }

            string[] musicFiles = Directory.GetFiles(musicFolderPath, "*.wav");
            foreach (string file in musicFiles)
            {
                using Stream stream = TitleContainer.OpenStream(file);
                {
                    SoundEffect music = SoundEffect.FromStream(stream);
                    string musicName = Path.GetFileNameWithoutExtension(file);
                    musicDictionary.Add(musicName, music);
                }
            }

            string[] sfxFiles = Directory.GetFiles(sfxFolderPath, "*.wav");
            foreach (string file in sfxFiles)
            {
                using Stream stream = TitleContainer.OpenStream(file);
                {
                    SoundEffect soundEffect = SoundEffect.FromStream(stream);
                    string soundEffectName = Path.GetFileNameWithoutExtension(file);
                    soundEffectDictionary.Add(soundEffectName, soundEffect);
                }
            }

            p8 = new Pico8Functions(new TitleScreen(), scenes, textureDictionary, soundEffectDictionary, musicDictionary, pixel, batch, GraphicsDevice, optionsFile);
        }


        protected override void UnloadContent()
        {
            batch.Dispose();
            pixel.Dispose();
            p8.Dispose();

            foreach (Texture2D texture in textureDictionary.Values)
            {
                texture.Dispose();
            }
            foreach (SoundEffect music in musicDictionary.Values)
            {
                music.Dispose();
            }
            foreach (SoundEffect soundEffect in soundEffectDictionary.Values)
            {
                soundEffect.Dispose();
            }

            base.UnloadContent();

        }


        private void UpdateViewport()
        {
            // Calculate the size of the largest square that fits in the client area
            int maxSize = Math.Min(Window.ClientBounds.Width, Window.ClientBounds.Height);

            // Round the size down to the nearest multiple of 128
            int size = (maxSize / 128) * 128;

            // Calculate the exact center of the client area
            double centerX = Window.ClientBounds.Width / 2.0;
            double centerY = Window.ClientBounds.Height / 2.0;

            // Calculate the top left corner of the square so that its center aligns with the client area's center
            int left = (int)Math.Round(centerX - size / 2.0);
            int top = (int)Math.Round(centerY - size / 2.0);

            // Set the viewport to the square area
            GraphicsDevice.Viewport = new Viewport(left, top, size, size);
        }


        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateViewport();
        }

    }
}