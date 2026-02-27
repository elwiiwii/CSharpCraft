using CSharpCraft.OptionsMenu;
using CSharpCraft.Pcraft;
using CSharpCraft.Pico8;
using CSharpCraft.Credits;
using CSharpCraft.Competitive;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

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
    }


    protected override void Initialize()
    {
        base.Initialize();

        CSharpCraft.Pico8.Pico8.UpdateViewport();
        
        scenes.Add(new CompetitiveScene());
        scenes.Add(new PcraftSingleplayer());
        scenes.Add(new PcraftSpeedrun());
        scenes.Add(new PcraftFilter());
        scenes.Add(new LoadSeed());
        scenes.Add(new Visualiser());
        scenes.Add(new ControlsOptions());
        scenes.Add(new CreditsScene());
        scenes.Add(new ExitScene());
    }


    protected override void Update(GameTime gameTime)
    {
        this.TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / CSharpCraft.Pico8.Pico8.CurrentCart.Fps));

        CSharpCraft.Pico8.Pico8.Update();

        base.Update(gameTime);
    }


    protected override void Draw(GameTime gameTime)
    {
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

        CSharpCraft.Pico8.Pico8.Draw();

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
        var graphicsAPI = new GraphicsAPI(batch, pixel, Pico8Utils.DefaultColors, FixMath.F32.Zero, FixMath.F32.Zero, (1, 1));
        var sceneManager = new SceneManager();
        var paletteManager = new PaletteManager(Pico8Utils.DefaultColors);

        // Audio wiring: AudioChannels → MusicManager → AudioAPI
        // MusicManager's soundDispose callback needs AudioAPI.StopAll(), but AudioAPI
        // needs MusicManager. Resolve via late-bound captured reference.
        var audioChannels = new CSharpCraft.Pico8.Audio.AudioChannels();
        AudioAPI? audioAPIRef = null;
        IAudioSettings audioSettings = optionsFile;
        IDisplaySettings displaySettings = optionsFile;

        var musicManager = new MusicManager(
            () => CSharpCraft.Pico8.Pico8.CurrentCart.Music,
            () => musicDictionary,
            () => audioSettings,
            () => audioAPIRef?.StopAll()
        );

        audioAPIRef = new AudioAPI(
            audioChannels,
            musicManager,
            soundEffectDictionary,
            () => CSharpCraft.Pico8.Pico8.CurrentCart.Sfx,
            () => audioSettings.CurrentSfxPack,
            () => optionsFile.Gen_Sound_On,
            () => optionsFile.Gen_Sfx_Vol
        );
        IAudioAPI audioAPI = audioAPIRef;

        orchestrator = new GameOrchestrator(
            inputManager,
            graphicsAPI,
            audioAPI,
            sceneManager,
            cartDataLoader: new CartDataLoader(),
            host: new GameHostContext(
                batch,
                pixel,
                graphics,
                GraphicsDevice,
                Window,
                textureDictionary,
                musicDictionary,
                soundEffectDictionary,
                optionsFile,
                optionsFile,
                optionsFile,
                scenes),
            cart: new TitleScreen(true),
            titleSceneFactory: () => new TitleScreen(false),
            paletteManager: paletteManager);

        orchestrator.Initialize();

        GameRendering.Initialize(new FnaTextureRenderer(batch, textureDictionary, Window, graphics, () => orchestrator.Cell));

        var popupService = new PopupService(graphicsAPI);
        Notifications.Initialize(popupService);

        orchestrator.PauseMenuRenderer = new PauseMenuRenderer(graphicsAPI, GameRendering.Current);
        orchestrator.PopupService = popupService;
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
        CSharpCraft.Pico8.Pico8.UpdateViewport();
    }

}
