using CSharpCraft.OptionsMenu;
using CSharpCraft.Pcraft;
using CSharpCraft.Pico8;
using CSharpCraft.RaceMode;
using CSharpCraft.Credits;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using System.Threading.Channels;
using System.Numerics;

namespace CSharpCraft
{

    class FNAGame : Game
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
        private List<IScene> scenes = [];
        private GraphicsDeviceManager graphics;
        private Pico8Functions p8;

        private OptionsFile optionsFile;

        private Dictionary<string, Texture2D> textureDictionary = new();
        private Dictionary<string, SoundEffect> musicDictionary = new();
        private Dictionary<string, SoundEffect> soundEffectDictionary = new();

        private Texture2D pixel;
        private KeyboardState prevState;

        private (string text, double frame) popup;

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

            optionsFile = OptionsFile.Initialize().file;

            graphics.PreferredBackBufferWidth = optionsFile.Gen_Window_Width;
            graphics.PreferredBackBufferHeight = optionsFile.Gen_Window_Height;
            graphics.IsFullScreen = optionsFile.Gen_Fullscreen;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / 30.0));
            graphics.SynchronizeWithVerticalRetrace = true;

            prevState = Keyboard.GetState();
        }


        protected override void Initialize()
        {
            base.Initialize();

            //optionsFile = OptionsFile.Initialize();

            UpdateViewport();

            scenes.Add(new PcraftSingleplayer());
            //scenes.Add(new MainRace());
            scenes.Add(new ControlsOptions());
            scenes.Add(new CreditsScene());
            scenes.Add(new ExitScene());

            popup = ("", 0);
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

            bool er;
            (optionsFile, er) = OptionsFile.Initialize();
            if (er) { popup = ("settings file corrupted", 1.5); }

            p8.Update();

            KeyboardState state = Keyboard.GetState();

            int halfDur = 30;
            if (Math.Abs(popup.frame) < 1.5)
            {
                popup.frame = 0;
            }
            else if (!(popup.frame == 0) && popup.frame < halfDur)
            {
                popup.frame += 1.5;
            }
            else if (!(popup.frame == 0) && popup.frame >= halfDur)
            {
                popup.frame *= -1;
            }

            if ((state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl)) && state.IsKeyDown(Keys.Q) && !prevState.IsKeyDown(Keys.Q))
            {
                p8.LoadCart(new TitleScreen(false));
                popup = ("quit (ctrl-q)", 1.5);
            }

            if ((state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl)) && state.IsKeyDown(Keys.R) && !prevState.IsKeyDown(Keys.R))
            {
                p8.LoadCart(p8._cart);
            }

            if ((state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl)) && state.IsKeyDown(Keys.M) && !prevState.IsKeyDown(Keys.M))
            {
                PropertyInfo propertyName = typeof(OptionsFile).GetProperty("Gen_Sound_On");
                propertyName.SetValue(optionsFile, !optionsFile.Gen_Sound_On);
                OptionsFile.JsonWrite(optionsFile);
                foreach (List<(string name, SoundEffectInstance track, bool loop, int group)> song in p8.channelMusic)
                {
                    foreach ((string name, SoundEffectInstance track, bool loop, int group) track in song)
                    {
                        track.track.Volume = 0.0f;
                    }
                }
                foreach (List<SoundEffectInstance> channel in new List<List<SoundEffectInstance>>([p8.channel0, p8.channel1, p8.channel2, p8.channel3]))
                {
                    foreach (SoundEffectInstance sfx in channel)
                    {
                        sfx.Volume = 0.0f;
                    }
                }
                popup = ($"sound {(optionsFile.Gen_Sound_On ? "on" : "off")} (ctrl-m)", 1.5);
            }

            if ((state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl)) && state.IsKeyDown(Keys.F) && !prevState.IsKeyDown(Keys.F))
            {
                PropertyInfo propertyName = typeof(OptionsFile).GetProperty("Gen_Fullscreen");
                propertyName.SetValue(optionsFile, !optionsFile.Gen_Fullscreen);
                OptionsFile.JsonWrite(optionsFile);
                graphics.ToggleFullScreen();
                popup = ($"fullscreen {(optionsFile.Gen_Fullscreen ? "on" : "off")} (ctrl-f)", 1.5);
            }

            prevState = state;

            base.Update(gameTime);
        }


        private void Popup(string s, int x1, int y1, int x2, int y2)
        {
            p8.Rectfill(x1, y1, x2, y2, 8);
            p8.Print(s, 1, y1 + 1, 15);
        }


        protected override void Draw(GameTime gameTime)
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            p8.Draw();

            int clampFrame = Math.Abs((int)Math.Floor(popup.frame)) > 7 ? 7 : Math.Abs((int)Math.Floor(popup.frame));
            Popup(popup.text, 0, 128 - clampFrame, 127, 128 - clampFrame + 7);

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
            batch = new SpriteBatch(GraphicsDevice);

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

            p8 = new Pico8Functions(new TitleScreen(true), scenes, textureDictionary, soundEffectDictionary, musicDictionary, pixel, batch, graphics, GraphicsDevice, optionsFile);
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