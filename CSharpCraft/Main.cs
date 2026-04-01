using System.Collections.Concurrent;
using CSharpCraft.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
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

    private SpriteBatch? _batch;
    private readonly GraphicsDeviceManager _graphics;
    private Texture2D? _pixel;
    private readonly Dictionary<string, Texture2D> _textureDictionary = [];
    private readonly Dictionary<string, SoundEffect> _musicDictionary = [];
    private readonly Dictionary<string, SoundEffect> _soundEffectDictionary = [];
    private readonly string _graphicsFolderPath = "Content/Graphics";
    private readonly string _musicFolderPath = "Content/Music";
    private readonly string _sfxFolderPath = "Content/Sfx";

    private readonly GameOrchestrator _orchestrator = new();
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
    }

    protected override void Update(GameTime gameTime)
    {
        // Drain the SDL event buffer and forward events to the input manager
        var frameEvents = new List<InputEvent>();
        while (_eventBuffer.TryDequeue(out InputEvent? evt))
            frameEvents.Add(evt);

        // Events arrive roughly in timestamp order from SDL but sort for safety
        frameEvents.Sort(static (a, b) => a.TimestampNs.CompareTo(b.TimestampNs));

        _orchestrator.UpdateInput(gameTime.ElapsedGameTime, frameEvents);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _batch!.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

        _batch!.End();

        base.Draw(gameTime);
    }


    protected override void LoadContent()
    {
        _batch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);

        string[] graphicsFiles = Directory.GetFiles(_graphicsFolderPath, "*.png");
        foreach (string file in graphicsFiles)
        {
            using Stream stream = TitleContainer.OpenStream(file);
            {
                Texture2D texture = Texture2D.FromStream(GraphicsDevice, stream);
                string textureName = Path.GetFileNameWithoutExtension(file);
                _textureDictionary.Add(textureName, texture);
            }
        }

        string[] musicFiles = Directory.GetFiles(_musicFolderPath, "*.wav");
        foreach (string file in musicFiles)
        {
            using Stream stream = TitleContainer.OpenStream(file);
            {
                SoundEffect music = SoundEffect.FromStream(stream);
                string musicName = Path.GetFileNameWithoutExtension(file);
                _musicDictionary.Add(musicName, music);
            }
        }

        string[] sfxFiles = Directory.GetFiles(_sfxFolderPath, "*.wav");
        foreach (string file in sfxFiles)
        {
            using Stream stream = TitleContainer.OpenStream(file);
            {
                SoundEffect soundEffect = SoundEffect.FromStream(stream);
                string soundEffectName = Path.GetFileNameWithoutExtension(file);
                _soundEffectDictionary.Add(soundEffectName, soundEffect);
            }
        }
    }


    protected override void UnloadContent()
    {
        SDL.SDL_RemoveEventWatch(_eventWatch, IntPtr.Zero);

        _batch!.Dispose();
        _pixel!.Dispose();

        foreach (Texture2D texture in _textureDictionary.Values)
        {
            texture.Dispose();
        }
        foreach (SoundEffect music in _musicDictionary.Values)
        {
            music.Dispose();
        }
        foreach (SoundEffect soundEffect in _soundEffectDictionary.Values)
        {
            soundEffect.Dispose();
        }

        base.UnloadContent();
    }

    protected override void OnExiting(object sender, EventArgs args)
    {
        base.OnExiting(sender, args);
    }

    private void Window_ClientSizeChanged(object? sender, EventArgs e)
    {
    }

    private unsafe bool OnSdlEvent(IntPtr userdata, SDL.SDL_Event* evt)
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