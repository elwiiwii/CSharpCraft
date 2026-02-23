using CSharpCraft.OptionsMenu;
using CSharpCraft.Pcraft;
using CSharpCraft.Pico8;
using CSharpCraft.Credits;
using CSharpCraft.Competitive;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static CSharpCraft.Pico8.Pico8;

namespace CSharpCraft;

class FNAGame : Game
{
    [STAThread]
    static void Main(string[] args)
    {
        Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL3");
        Environment.SetEnvironmentVariable("FNA_NO_OPENGL_INTERCEPTION", "1");
        ArgumentNullException.ThrowIfNull(args);

        using FNAGame g = new();
        g.Run();
    }


    private SpriteBatch batch;
    private readonly List<IScene> scenes = [];
    private readonly GraphicsDeviceManager graphics;
    private GameOrchestrator orchestrator = null!;

    private readonly OptionsFile optionsFile;

    private readonly Dictionary<string, Texture2D> textureDictionary = new();
    private readonly Dictionary<string, SoundEffect> musicDictionary = new();
    private readonly Dictionary<string, SoundEffect> soundEffectDictionary = new();

    private Texture2D pixel;
    private KeyboardState prevState;

    private (string text, double frame) popup;
    private (int w, int h) resolution = (128, 128);


    private readonly double elapsedSeconds = 0.0;
    private readonly string graphicsFolderPath = "Content/Graphics";
    private readonly string musicFolderPath = "Content/Music";
    private readonly string sfxFolderPath = "Content/Sfx";

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

        UpdateViewport();
        
        scenes.Add(new CompetitiveScene());
        scenes.Add(new PcraftSingleplayer());
        scenes.Add(new PcraftSpeedrun());
        scenes.Add(new PcraftFilter());
        scenes.Add(new LoadSeed());
        scenes.Add(new Visualiser());
        scenes.Add(new ControlsOptions());
        scenes.Add(new CreditsScene());
        scenes.Add(new ExitScene());

        popup = ("", 0);
    }


    protected override void Update(GameTime gameTime)
    {
        this.TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / CurrentCart.Fps));

        CSharpCraft.Pico8.Pico8.Update();

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
            ScheduleScene(() => new TitleScreen(false));
            popup = ("quit (ctrl-q)", 1.5);
        }

        if ((state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl)) && state.IsKeyDown(Keys.R) && !prevState.IsKeyDown(Keys.R))
        {
            ScheduleScene(() => CurrentCart);
        }

        if ((state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl)) && state.IsKeyDown(Keys.M) && !prevState.IsKeyDown(Keys.M))
        {
            optionsFile.Gen_Sound_On = !optionsFile.Gen_Sound_On;
            OptionsFile.JsonWrite(optionsFile);
            Mute();
            popup = ($"sound {(optionsFile.Gen_Sound_On ? "on" : "off")} (ctrl-m)", 1.5);
        }

        if ((state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl)) && state.IsKeyDown(Keys.F) && !prevState.IsKeyDown(Keys.F))
        {
            optionsFile.Gen_Fullscreen = !optionsFile.Gen_Fullscreen;
            OptionsFile.JsonWrite(optionsFile);
            GameRendering.Current.ApplyDisplaySettings(
                optionsFile.Gen_Fullscreen,
                optionsFile.Gen_Window_Width / 128 * Resolution.w,
                optionsFile.Gen_Window_Height / 128 * Resolution.h);
            UpdateViewport();
            popup = ($"fullscreen {(optionsFile.Gen_Fullscreen ? "on" : "off")} (ctrl-f)", 1.5);
        }

        prevState = state;

        base.Update(gameTime);
    }


    private void Popup(string s, int x1, int y1, int x2, int y2)
    {
        Rectfill(x1, y1, x2, y2, 8);
        Print(s, 1, y1 + 1, 15);
    }


    protected override void Draw(GameTime gameTime)
    {
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

        CSharpCraft.Pico8.Pico8.Draw();

        int clampFrame = Math.Abs((int)Math.Floor(popup.frame)) > 7 ? 7 : Math.Abs((int)Math.Floor(popup.frame));
        if (Math.Abs(popup.frame) > 0) { Popup(popup.text, 0, Resolution.h - clampFrame, Resolution.w - 1, Resolution.h - clampFrame + 7); }

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

        OptionsFile.Current = optionsFile;

        // Create sub-systems
        var inputManager = new InputStateManager();
        var graphicsAPI = new GraphicsAPI(batch, pixel, GameOrchestrator.DefaultColors, FixMath.F32.Zero, FixMath.F32.Zero, (1, 1));
        var audioAPI = new AudioAPI(null, null, soundEffectDictionary, () => new(), () => 0, () => optionsFile.Gen_Sound_On, () => optionsFile.Gen_Sfx_Vol);
        var sceneManager = new SceneManager();
        var paletteManager = new PaletteManager(GameOrchestrator.DefaultColors);

        orchestrator = new GameOrchestrator(
            new TitleScreen(true),
            new GameHostContext(
                batch,
                pixel,
                graphics,
                GraphicsDevice,
                Window,
                textureDictionary,
                optionsFile,
                optionsFile,
                scenes),
            inputManager,
            graphicsAPI,
            audioAPI,
            sceneManager,
            new CartDataLoader(),
            paletteManager);

        orchestrator.Initialize();

        GameRendering.Current = new FnaTextureRenderer(batch, textureDictionary, Window, graphics, () => orchestrator.Cell);
    }


    protected override void UnloadContent()
    {
        try
        {
            AccountHandler.Shutdown();
            RoomHandler.Shutdown();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during AccountHandler shutdown: {ex.Message}");
        }

        batch.Dispose();
        pixel.Dispose();
        CSharpCraft.Pico8.Pico8.Dispose();

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

    protected override void OnExiting(object sender, EventArgs args)
    {
        try
        {
            AccountHandler.Shutdown();
            RoomHandler.Shutdown();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during AccountHandler shutdown on exit: {ex.Message}");
        }

        base.OnExiting(sender, args);
    }

    private void Window_ClientSizeChanged(object sender, EventArgs e)
    {
        UpdateViewport();
    }

}
