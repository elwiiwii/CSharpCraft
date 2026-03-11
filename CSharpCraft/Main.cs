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

    private SpriteBatch? _batch;
    private readonly GraphicsDeviceManager _graphics;
    private Texture2D? _pixel;
    private readonly Dictionary<string, Texture2D> _textureDictionary = [];
    private readonly Dictionary<string, SoundEffect> _musicDictionary = [];
    private readonly Dictionary<string, SoundEffect> _soundEffectDictionary = [];
    private readonly string _graphicsFolderPath = "Content/Graphics";
    private readonly string _musicFolderPath = "Content/Music";
    private readonly string _sfxFolderPath = "Content/Sfx";

    private FNAGame()
    {
        _graphics = new GraphicsDeviceManager(this);

        // Allow the user to resize the window
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

        // All content loaded will be in a "Content" folder
        Content.RootDirectory = "Content";

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / 30.0));
        _graphics.SynchronizeWithVerticalRetrace = true;
        //IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
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
}