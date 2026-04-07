using System.Collections.Concurrent;
using CSharpCraft.Input;
using CSharpCraft.Pcraft;
using CSharpCraft.Settings;
using Microsoft.Xna.Framework;
using PSharp8.Input;
using SDL3;

namespace CSharpCraft;

unsafe class FNAGame : Game
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

    private readonly GraphicsDeviceManager _graphics;
    private readonly string _graphicsFolderPath = "Content/Graphics";
    private readonly string _musicFolderPath = "Content/Music";
    private readonly string _sfxFolderPath = "Content/Sfx";

    private GameOrchestrator? _orchestrator;
    private SettingsManager? _settingsManager;
    // Kept as a field to prevent the delegate from being garbage-collected
    private readonly SDL.SDL_EventFilter _eventWatch;
    private readonly ConcurrentQueue<InputEvent> _eventBuffer = new();

    private FNAGame()
    {
        _graphics = new GraphicsDeviceManager(this);

        // Allow the user to resize the window
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

        // All content loaded will be in a "Content" folder
        Content.RootDirectory = "Content";

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / 60.0));
        _graphics.SynchronizeWithVerticalRetrace = true;
        //IsMouseVisible = true;

        _eventWatch = OnSdlEvent;
    }

    protected override void Initialize()
    {
        base.Initialize();

        SDL.SDL_AddEventWatch(_eventWatch, IntPtr.Zero);

        var scene = new PcraftSceneBase();
        _orchestrator = new GameOrchestrator(
            musicDirectory: _musicFolderPath,
            sfxDirectory: _sfxFolderPath,
            texturesDirectory: _graphicsFolderPath,
            defaultScene: scene,
            graphicsDevice: GraphicsDevice,
            graphicsDeviceManager: _graphics,
            window: Window);
        Pico8.Initialize(_orchestrator);
        _orchestrator.LoadSoundtracks(scene.Music, "new!");
        _orchestrator.LoadSfxPacks(scene.Sfx, "soft");

        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CSharpCraft");
        _settingsManager = new SettingsManager(configDir, _orchestrator, _graphics);
    }

    protected override void Update(GameTime gameTime)
    {
        _settingsManager!.Update();

        // Drain the SDL event buffer and forward events to the input manager
        var frameEvents = new List<InputEvent>();
        while (_eventBuffer.TryDequeue(out InputEvent? evt))
            frameEvents.Add(evt);

        // Events arrive roughly in timestamp order from SDL but sort for safety
        frameEvents.Sort(static (a, b) => a.TimestampNs.CompareTo(b.TimestampNs));

        _orchestrator!.UpdateInput(gameTime.ElapsedGameTime, frameEvents);
        _orchestrator!.Update(gameTime.ElapsedGameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _orchestrator!.Draw(gameTime.ElapsedGameTime);
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        SDL.SDL_RemoveEventWatch(_eventWatch, IntPtr.Zero);

        _settingsManager?.Dispose();
        _orchestrator?.Dispose();

        base.UnloadContent();
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        base.OnExiting(sender, args);
    }

    private void Window_ClientSizeChanged(object? sender, EventArgs args)
    {
        int newW = Window.ClientBounds.Width;
        int newH = Window.ClientBounds.Height;
        if (newW > 0 && newH > 0 &&
            (_graphics.PreferredBackBufferWidth != newW ||
             _graphics.PreferredBackBufferHeight != newH))
        {
            _graphics.PreferredBackBufferWidth  = newW;
            _graphics.PreferredBackBufferHeight = newH;
            _graphics.ApplyChanges();
        }
    }

    private bool OnSdlEvent(IntPtr userdata, SDL.SDL_Event* evt)
    {
        uint type = evt->type;
        InputEvent? inputEvent = null;

        if (type == (uint)SDL.SDL_EventType.SDL_EVENT_KEY_DOWN)
            inputEvent = InputEventTranslator.TranslateKeyboard(evt->key.key, isDown: true, evt->key.timestamp);
        else if (type == (uint)SDL.SDL_EventType.SDL_EVENT_KEY_UP)
            inputEvent = InputEventTranslator.TranslateKeyboard(evt->key.key, isDown: false, evt->key.timestamp);
        else if (type == (uint)SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN)
            inputEvent = InputEventTranslator.TranslateMouse(evt->button.button, isDown: true, evt->button.timestamp);
        else if (type == (uint)SDL.SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
            inputEvent = InputEventTranslator.TranslateMouse(evt->button.button, isDown: false, evt->button.timestamp);
        else if (type == (uint)SDL.SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_DOWN)
            inputEvent = InputEventTranslator.TranslateGamepad(evt->gbutton.button, isDown: true, evt->gbutton.timestamp);
        else if (type == (uint)SDL.SDL_EventType.SDL_EVENT_GAMEPAD_BUTTON_UP)
            inputEvent = InputEventTranslator.TranslateGamepad(evt->gbutton.button, isDown: false, evt->gbutton.timestamp);

        if (inputEvent is not null)
            _eventBuffer.Enqueue(inputEvent);

        // Return false so FNA still receives the event
        return false;
    }
}