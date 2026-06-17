using CSharpCraft.Input;
using CSharpCraft.PcraftScenes;
using CSharpCraft.Settings;
using Microsoft.Xna.Framework;
using PSharp8.Input;

namespace CSharpCraft;

internal class FnaGame : Game
{
    [STAThread]
    private static void Main(string[] args)
    {
        Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL3");
        Environment.SetEnvironmentVariable("FNA_NO_OPENGL_INTERCEPTION", "1");
        ArgumentNullException.ThrowIfNull(args);

        using FnaGame g = new();
        g.Run();
    }

    private readonly GraphicsDeviceManager _graphics;
    private const string GraphicsFolderPath = "Content/Graphics";
    private const string MusicFolderPath = "Content/Music";
    private const string SfxFolderPath = "Content/Sfx";

    private GameOrchestrator? _orchestrator;
    private SettingsManager? _settingsManager;
    private PollingInputProvider? _inputProvider;

    private FnaGame()
    {
        _graphics = new GraphicsDeviceManager(this);

        // Allow the user to resize the window
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += Window_ClientSizeChanged;

        // All content loaded will be in a "Content" folder
        Content.RootDirectory = "Content";

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / 60.0));
        //_graphics.SynchronizeWithVerticalRetrace = true;
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();

        _inputProvider = new PollingInputProvider(InputBindings.Default);

        MainScene scene = new();
        //PcraftGameScene scene = new(seed: 12345);
        //PcraftPreviewScene scene = new();
        //FilterComparisonScene scene = new();
        _orchestrator = new GameOrchestrator(
            musicDirectory: MusicFolderPath,
            sfxDirectory: SfxFolderPath,
            texturesDirectory: GraphicsFolderPath,
            defaultScene: scene,
            graphicsDevice: GraphicsDevice,
            graphicsDeviceManager: _graphics,
            window: Window,
            inputProvider: _inputProvider);
        Pico8.Initialize(_orchestrator);
        _orchestrator.LoadSoundtracks(scene.Music, "new!");
        _orchestrator.LoadSfxPacks(scene.Sfx, "soft");

        string configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CSharpCraft");
        _settingsManager = new SettingsManager(configDir, _orchestrator, _graphics);
    }

    protected override void Update(GameTime gameTime)
    {
        // base.Update() must be called first so FNA processes platform events,
        // making Keyboard/GamePad/Mouse.GetState() reflect the current frame.
        base.Update(gameTime);

        _settingsManager!.Update();
        _orchestrator!.UpdateInput(gameTime.ElapsedGameTime);
        _orchestrator!.Update(gameTime.ElapsedGameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _orchestrator!.Draw(gameTime.ElapsedGameTime);
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        _settingsManager?.Dispose();
        _orchestrator?.Dispose();

        base.UnloadContent();
    }

    //protected override void OnExiting(object sender, EventArgs args)
    //{
    //    base.OnExiting(sender, args);
    //}

    private void Window_ClientSizeChanged(object? sender, EventArgs args)
    {
        int newW = Window.ClientBounds.Width;
        int newH = Window.ClientBounds.Height;
        if (newW > 0 && newH > 0 &&
            (_graphics.PreferredBackBufferWidth != newW ||
             _graphics.PreferredBackBufferHeight != newH))
        {
            _graphics.PreferredBackBufferWidth = newW;
            _graphics.PreferredBackBufferHeight = newH;
            _graphics.ApplyChanges();
        }
    }
}
