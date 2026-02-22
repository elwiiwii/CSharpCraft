using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using FixMath;
using CSharpCraft.Pico8.Services;

namespace CSharpCraft.Pico8;

public class Pico8Functions : IDisposable
{
    internal SpriteBatch Batch { get; }
    internal (F32 x, F32 y) CameraOffset { get; set; } = (F32.Zero, F32.Zero);
    public (int Width, int Height) Cell { get; internal set; }
    internal GraphicsDeviceManager Graphics { get; }
    internal GraphicsDevice GraphicsDevice { get; }
    private Dictionary<string, SoundEffect> MusicDictionary { get; }
    private Texture2D Pixel { get; }
    public (int w, int h) Resolution { get; private set; } = (128, 128);
    public List<IScene> Scenes { get; }
    private Dictionary<string, SoundEffect> SoundEffectDictionary { get; }
    internal Dictionary<string, Texture2D> TextureDictionary { get; }
    public object? TitleSceneInstance { get; }
    internal GameWindow Window { get; }
    internal IInputBindingProvider InputBindings { get; set; }
    internal IAudioGraphicsSettings Settings { get; set; }

    // pico-8 colors https://pico-8.fandom.com/wiki/Palette
    public List<Color> Colors { get; } =
    [
        Pico8Utils.HexToColor("000000"), // 00 black
        Pico8Utils.HexToColor("1D2B53"), // 01 dark-blue
        Pico8Utils.HexToColor("7E2553"), // 02 dark-purple
        Pico8Utils.HexToColor("008751"), // 03 dark-green
        Pico8Utils.HexToColor("AB5236"), // 04 brown
        Pico8Utils.HexToColor("5F574F"), // 05 dark-grey
        Pico8Utils.HexToColor("C2C3C7"), // 06 light-grey
        Pico8Utils.HexToColor("FFF1E8"), // 07 white
        Pico8Utils.HexToColor("FF004D"), // 08 red
        Pico8Utils.HexToColor("FFA300"), // 09 orange
        Pico8Utils.HexToColor("FFEC27"), // 10 yellow
        Pico8Utils.HexToColor("00E436"), // 11 green
        Pico8Utils.HexToColor("29ADFF"), // 12 blue
        Pico8Utils.HexToColor("83769C"), // 13 lavender
        Pico8Utils.HexToColor("FF77A8"), // 14 pink
        Pico8Utils.HexToColor("FFCCAA"), // 15 light-peach
        
        Pico8Utils.HexToColor("291814"), // 16 brownish-black
        Pico8Utils.HexToColor("111D35"), // 17 darker-blue
        Pico8Utils.HexToColor("422136"), // 18 darker-purple
        Pico8Utils.HexToColor("125359"), // 19 blue-green
        Pico8Utils.HexToColor("742F29"), // 20 dark-brown
        Pico8Utils.HexToColor("49333B"), // 21 darker-grey
        Pico8Utils.HexToColor("A28879"), // 22 medium-grey
        Pico8Utils.HexToColor("F3EF7D"), // 23 light-yellow
        Pico8Utils.HexToColor("BE1250"), // 24 dark-red
        Pico8Utils.HexToColor("FF6C24"), // 25 dark-orange
        Pico8Utils.HexToColor("A8E72E"), // 26 lime-green
        Pico8Utils.HexToColor("00B543"), // 27 medium-green
        Pico8Utils.HexToColor("065AB5"), // 28 true-blue
        Pico8Utils.HexToColor("754665"), // 29 mauve
        Pico8Utils.HexToColor("FF6E59"), // 30 dark-peach
        Pico8Utils.HexToColor("FF9D81"), // 31 peach
    ];
    // Manager instances for core systems (Phase 2 refactoring)
    private AudioChannels? _audioChannels;
    private MusicManager? _musicManager;
    private PaletteManager? _paletteManager;
    private SpriteCache? _spriteCache;
    private IGraphicsAPI? _graphicsAPI;
    private IAudioAPI? _audioAPI;
    private TrackManager? _trackManager;
    private MapManager? _mapManager;

    // Phase 9: State container and output facade (SRP improvement)
    private IGameState? _gameState;
    private IOutputFacade? _outputFacade;

    // Legacy palette access for compatibility
    internal List<PalCol> PalColors => _paletteManager?.GetAllRemappings() ?? [];

    private int[] _flags;
    private int[] _map;
    private Color[] _sprites;
    private Dictionary<string, List<SongInst>> _music;
    private Dictionary<string, Dictionary<int, string>> _sfx;

    public IScene _cart;
    private IInputStateManager? _inputStateManager;
    private SceneStateManager? _sceneStateManager;
    private PauseMenuState? _pauseMenuState;
    private readonly CosDict cosDict = new();
    private readonly SinDict sinDict = new();
    Random random = new();

    public Pico8Functions(
        IScene cart, 
        object? titleScreen, 
        List<IScene> scenes, 
        Dictionary<string, Texture2D> textureDictionary, 
        Dictionary<string, SoundEffect> soundEffectDictionary, 
        Dictionary<string, SoundEffect> musicDictionary, 
        Texture2D pixel, 
        SpriteBatch batch, 
        GraphicsDeviceManager graphics, 
        GraphicsDevice graphicsDevice, 
        GameWindow window, 
        IAudioGraphicsSettings settings,
        IInputBindingProvider inputBindings,
        IServiceFactory? serviceFactory = null)
    {
        // Use default factory if none provided
        serviceFactory ??= new ServiceFactory();

        // Initialize basic properties
        Batch = batch;
        Graphics = graphics;
        GraphicsDevice = graphicsDevice;
        MusicDictionary = musicDictionary;
        Pixel = pixel;
        Scenes = scenes;
        SoundEffectDictionary = soundEffectDictionary;
        TextureDictionary = textureDictionary;
        TitleSceneInstance = titleScreen;
        Window = window;
        InputBindings = inputBindings;
        Settings = settings;

        _sprites = [];
        _flags = [];
        _map = [];
        _music = [];
        _sfx = [];
        _cart = cart;

        // Initialize manager instances using factory (DIP - Phase 6+7+8)
        _inputStateManager = serviceFactory.CreateInputStateManager();
        _audioChannels = serviceFactory.CreateAudioChannels();
        _spriteCache = serviceFactory.CreateSpriteCache();
        _paletteManager = serviceFactory.CreatePaletteManager(Colors);
        _musicManager = serviceFactory.CreateMusicManager(
            () => _music,
            () => musicDictionary,
            () => Settings,
            SoundDispose);
        _trackManager = (TrackManager)serviceFactory.CreateTrackManager(
            () => _music,
            () => _sfx,
            Settings);
        _mapManager = (MapManager)serviceFactory.CreateMapManager(
            _map,
            _flags,
            _cart.MapDimensions);
        _graphicsAPI = serviceFactory.CreateGraphicsAPI(
            batch,
            pixel,
            Colors,
            CameraOffset.x,
            CameraOffset.y,
            Cell);
        _audioAPI = serviceFactory.CreateAudioAPI(
            _audioChannels,
            _musicManager,
            soundEffectDictionary,
            () => _sfx,
            () => Settings.CurrentSfxPack,
            () => Settings.SoundEnabled,
            () => Settings.SfxVolume);

        // Phase 9: Create state container and output facade (SRP improvement)
        _gameState = serviceFactory.CreateGameState(
            cart,
            _map,
            _flags,
            _sprites,
            _music,
            _sfx);
        _outputFacade = serviceFactory.CreateOutputFacade(
            _graphicsAPI,
            _audioAPI);

        // Initialize state managers for scene and pause menu (Phase 7 - using factory)
        _sceneStateManager = serviceFactory.CreateSceneStateManager(cart);
        _pauseMenuState = serviceFactory.CreatePauseMenuState(this);

        LoadCart(cart);
    }

    // Helper accessors for PauseMenuBuilder and menu state management
    internal int SfxCount => _trackManager?.SfxCount ?? 0;
    internal int MusicCount => _trackManager?.MusicCount ?? 0;
    internal int? LastMusicCall => _musicManager?.LastMusicCall;

    internal string GetCurrentSfxPackName() => _trackManager?.GetCurrentSfxPackName() ?? "sfx";
    internal string GetCurrentSoundtrackName() => _trackManager?.GetCurrentSoundtrackName() ?? "music";

    internal void DecrementSfxPack()
    {
        _trackManager?.DecrementSfxPack();
    }

    internal void IncrementSfxPack()
    {
        _trackManager?.IncrementSfxPack();
    }

    internal void DecrementSoundtrack()
    {
        _trackManager?.DecrementSoundtrack();
    }

    internal void IncrementSoundtrack()
    {
        _trackManager?.IncrementSoundtrack();
    }

    internal void ReloadCart() => LoadCart(_cart);

    public void ScheduleScene(Func<IScene> sceneFactory)
    {
        _sceneStateManager?.ScheduleScene(sceneFactory);
    }

    internal void LoadCart(IScene cart)
    {
        _cart?.Dispose();
        _sprites = [];
        _flags = [];
        _map = [];
        _music = cart.Music;
        _sfx = cart.Sfx;

        _cart = cart;
        _inputStateManager?.Reset(this);
        SoundDispose();
        UpdateViewport();

        // Initialize pause menu state for new scene (Phase 7 - extracted)
        _pauseMenuState?.Reset();
        _pauseMenuState?.InitializeMenuStructure();

        // Update scene manager state (Phase 7 - extracted)
        _sceneStateManager?.SetCurrentScene(cart);

        Reload();
        Init();
    }


    internal void Init()
    {
        _pauseMenuState?.Reset();
        _cart.Init();
    }


    public void Update()
    {
        Cell = (GraphicsDevice.Viewport.Width / _cart.Resolution.w, GraphicsDevice.Viewport.Height / _cart.Resolution.h);
        if (!(_cart.SceneName == "TitleScreen") && Btnp(6))
        {
            _pauseMenuState?.TogglePause();
        }
        _inputStateManager?.UpdatePauseButton(this);

        if (_pauseMenuState?.IsPaused ?? false)
        {
            _inputStateManager?.SetPauseMode(true);
            _inputStateManager?.UpdateLockout(this);

            // Handle menu input (Phase 7 - extracted)
            _pauseMenuState?.HandleMenuInput(
                Btnp(2), // up
                Btnp(3), // down
                Btnp(0) || Btnp(1) || Btnp(4) || Btnp(5)); // select

            PlaySound(false);
        }
        else
        {
            _inputStateManager?.SetPauseMode(false);
            _inputStateManager?.UpdateLockout(this);

            PlaySound(true);

            _cart.Update();

            // Handle scheduled scene changes (Phase 7 - extracted)
            var scheduledScene = _sceneStateManager?.GetAndClearScheduledScene();
            if (scheduledScene is not null)
            {
                LoadCart(scheduledScene());
            }
        }

        _inputStateManager?.Update(this);

        // Delegate music state updates to MusicManager (extracted 40+ lines)
        _musicManager?.Update();
    }

    private void PlaySound(bool play)
    {
        if (play)
        {
            _musicManager?.Resume();
            _audioChannels?.ResumeAll();
        }
        else
        {
            _musicManager?.Pause();
            _audioChannels?.PauseAll();
        }
    }

    public void Draw()
    {
        Pal();
        Palt();
        _cart.Draw();

        if (_pauseMenuState?.IsPaused ?? false)
        {
            var curMenuItems = _pauseMenuState.CurrentMenuItems;
            var menuSelected = _pauseMenuState.SelectedIndex;

            Vector2 size = new(Cell.Width, Cell.Height);

            int i = (int)Math.Floor(64 - (curMenuItems.Count / 2.0) * 8);

            int xborder = 23;
            Rectfill(0 + xborder, i - 7, 127 - xborder, i + curMenuItems.Count * 8 + 2, 0);
            Rectfill(0 + xborder + 1, i - 7 + 1, 127 - xborder - 1, i + curMenuItems.Count * 8 + 2 - 1, 7);
            Rectfill(0 + xborder + 2, i - 7 + 2, 127 - xborder - 2, i + curMenuItems.Count * 8 + 2 - 2, 0);

            Batch.Draw(TextureDictionary["PauseArrow"], new Vector2((xborder + 4) * Cell.Width, (i - 1 + menuSelected * 8) * Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            for (int j = 0; j < curMenuItems.Count; j++)
            {
                int indent = menuSelected == j ? 1 : 0;
                Print(curMenuItems[j].GetName(), xborder + indent + 12, i, 7);
                i += 8;
            }
        }
    }


    public T Add<T>(List<T> table, T value, int index = -1) // https://pico-8.fandom.com/wiki/Add
    {
        if (index == -1) { table.Add(value); return value; }
        table.Insert(index, value);
        return value;
    }


    public bool Btn(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
    {
        // Delegate to input state manager
        // Current stub implementation returns false - will be properly implemented later
        return _inputStateManager?.Btn(i, p) ?? false;
    }


    public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
    {
        // Delegate to input state manager
        // Current stub implementation returns false - will be properly implemented later
        return _inputStateManager?.Btnp(i, p) ?? false;
    }


    public void Camera() // https://pico-8.fandom.com/wiki/Camera
    {
        CameraOffset = (F32.Zero, F32.Zero);
    }


    public void Camera(F32 x, F32 y) // https://pico-8.fandom.com/wiki/Camera
    {
        CameraOffset = (x, y);
    }


    internal static void CartData(string id) // https://pico-8.fandom.com/wiki/Cartdata
    {

    }


    public void Circ(F32 x, F32 y, double r, int c) // https://pico-8.fandom.com/wiki/Circ
    {
        _outputFacade?.DrawCircle(x.Double, y.Double, r, c);
    }


    public void Circfill(F32 x, F32 y, double r, int c) // https://pico-8.fandom.com/wiki/Circfill
    {
        _outputFacade?.FillCircle(x.Double, y.Double, r, c);
    }


    public void Cls(int col = 0) // https://pico-8.fandom.com/wiki/Cls
    {
        _outputFacade?.ClearScreen(col);
    }


    public F32 Cos(F32 angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Cos
    {
        angle = Mod(angle, 1);
        return F32.FromRaw((int)(cosDict.LookupTable[angle.Raw / 10.0] * 10));
    }


    internal static void Cstore() // https://pico-8.fandom.com/wiki/Cstore
    {

    }


    public void Del<T>(List<T> table, T value) // https://pico-8.fandom.com/wiki/Del
    {
        table?.Remove(value);
    }


    internal static F32 Dget(int index) // https://pico-8.fandom.com/wiki/Dget
    {
        return F32.FromInt(index);
    }


    internal static void Dset(int index, double value) // https://pico-8.fandom.com/wiki/Dset
    {

    }


    private int Fget(int n) // https://pico-8.fandom.com/wiki/Fget
    {
        return _mapManager?.Fget(n) ?? 0;
    }


    internal static void Load(string fileName)
    {

    }


    public void Map(double celx, double cely, double sx, double sy, double celw, double celh, int flags = 0) // https://pico-8.fandom.com/wiki/Map
    {
        int cwFlr = (int)Math.Floor(celw);
        int chFlr = (int)Math.Floor(celh);

        for (int i = 0; i <= cwFlr; i++)
        {
            for (int j = 0; j <= chFlr; j++)
            {
                int mapTile = Mget(celx + i, cely + j);
                if (flags == 0 || flags == Fget(mapTile))
                {
                    Spr(mapTile, sx + i * 8, sy + j * 8);
                }
            }
        }
    }


    public void Memcpy(int destaddr, int sourceaddr, int len) // https://pico-8.fandom.com/wiki/Memcpy - https://pico-8.fandom.com/wiki/Memory
    {
        if (destaddr == 0x1000 && sourceaddr == 0x2000 && len == 0x1000)
        {
            Dispose();
            Color[] secondHalf = Pico8Utils.MapDataToColorArray(this, _cart.MapData.Substring(0, _cart.MapDimensions.x * _cart.MapDimensions.y), 1);
            secondHalf.CopyTo(_sprites, _cart.MapDimensions.x * _cart.MapDimensions.y);
        }
    }


    internal void Menuitem(int pos, Func<string> getName, Action function, List<MenuItem>? list = null) // https://pico-8.fandom.com/wiki/Menuitem
    {
        list ??= _pauseMenuState?.CurrentMenuItems ?? [];
        list.Insert(pos, new MenuItem(getName, function));
    }


    public int Mget(double celx, double cely) // https://pico-8.fandom.com/wiki/Mget
    {
        return _mapManager?.Mget(celx, cely) ?? 0;
    }


    public static F32 Mod(F32 x, int m)
    {
        F32 r = x % m;
        return r < 0 ? r + m : r;
    }


    public void Mset(double celx, double cely, double snum = 0) // https://pico-8.fandom.com/wiki/Mset
    {
        _mapManager?.Mset(celx, cely, snum);
    }


    public void Music(int n, double fadems = 0) // https://pico-8.fandom.com/wiki/Music
    {
        _outputFacade?.PlayMusic(n, (int)fadems);
    }


    public void Mute()
    {
        _outputFacade?.MuteAudio();
    }


    public void Pal() // https://pico-8.fandom.com/wiki/Pal
    {
        _paletteManager?.ResetPalette();
    }


    public void Pal(int c0, int c1) // https://pico-8.fandom.com/wiki/Pal
    {
        _paletteManager?.SetPalette(c0, c1);
    }


    public void Pal(Color c0, Color c1) // https://pico-8.fandom.com/wiki/Pal
    {
        _paletteManager?.SetPalette(c0, c1);
    }


    public void Palt() // https://pico-8.fandom.com/wiki/Palt
    {
        _paletteManager?.ResetTransparency();
    }


    public void Palt(int col, bool t) // https://pico-8.fandom.com/wiki/Palt
    {
        _paletteManager?.SetTransparency(col, t);
    }


    public void Palt(Color col, bool t) // https://pico-8.fandom.com/wiki/Palt
    {
        _paletteManager?.SetTransparency(col, t);
    }


    public void Print(string str, double x, double y, double c) // https://pico-8.fandom.com/wiki/Print
    {
        // Fallback implementation if service not available
        int xFlr = (int)Math.Floor(x);
        int yFlr = (int)Math.Floor(y);
        int cFlr = (int)Math.Floor(c);

        int charWidth = 4;

        for (int s = 0; s < str.Length; s++)
        {
            char letter = str[s];

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Font.chars[letter][i, j] == 1)
                    {
                        int charStartX = (s * charWidth + xFlr + j - F32.FloorToInt(CameraOffset.x)) * Cell.Width;
                        int charStartY = (yFlr + i - F32.FloorToInt(CameraOffset.y)) * Cell.Height;

                        Vector2 position = new(charStartX, charStartY);
                        Vector2 size = new(Cell.Width, Cell.Height);

                        Batch.Draw(Pixel, position, null, Colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }
    }


    public void PrintBig(string text, int x, int y, Color color)
    {
        Texture2D fontTexture = TextureDictionary["BigFont"];
        const int charWidth = 8;
        const int charHeight = 12;
        const string fontChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-";
        int charsPerRow = fontTexture.Width / charWidth;

        foreach (char c in text)
        {
            int charIndex = fontChars.IndexOf(c);
            if (charIndex < 0)
            {
                x += charWidth;
                continue;
            }

            int srcX = (charIndex % charsPerRow) * charWidth;
            int srcY = (charIndex / charsPerRow) * charHeight;
            Rectangle srcRect = new(srcX, srcY, charWidth, charHeight);

            Color[] pixelData = new Color[charWidth * charHeight];
            fontTexture.GetData(0, srcRect, pixelData, 0, pixelData.Length);

            for (int py = 0; py < charHeight; py++)
            {
                for (int px = 0; px < charWidth; px++)
                {
                    if (pixelData[py * charWidth + px].A > 0)
                    {
                        Batch.Draw(fontTexture, new Rectangle((x + px - F32.FloorToInt(CameraOffset.x)) * Cell.Width, (y + py - F32.FloorToInt(CameraOffset.y)) * Cell.Height, Cell.Width, Cell.Height), new Rectangle(srcX + px, srcY + py, 1, 1), color);
                    }
                }
            }

            x += charWidth;
        }
    }


    public void Pset(F32 x, F32 y, double c) // https://pico-8.fandom.com/wiki/Pset
    {
        _outputFacade?.SetPixel(x.Double, y.Double, (int)c);
    }


    public void Rect(double x1, double y1, double x2, double y2, double c) // https://pico-8.fandom.com/wiki/Rect
    {
        _outputFacade?.DrawRect(x1, y1, x2, y2, (int)c);
    }


    public void Rect(double x1, double y1, double x2, double y2, Color c) // https://pico-8.fandom.com/wiki/Rect
    {
        _outputFacade?.DrawRect(x1, y1, x2, y2, (int)c.PackedValue);
    }


    public void Rectfill(double x1, double y1, double x2, double y2, double c) // https://pico-8.fandom.com/wiki/Rectfill
    {
        _outputFacade?.FillRect(x1, y1, x2, y2, (int)c);
    }

    public void Rectfill(double x1, double y1, double x2, double y2, Color c) // https://pico-8.fandom.com/wiki/Rectfill
    {
        _outputFacade?.FillRect(x1, y1, x2, y2, (int)c.PackedValue);
    }


    public void Reload(int i1 = 0, int i2 = 0, int i3 = 0, string s = "") // https://pico-8.fandom.com/wiki/Reload
    {
        Dispose();

        _sprites = !string.IsNullOrEmpty(_cart.SpriteData) ? Pico8Utils.DataToColorArray(this, _cart.SpriteData, 1) : _sprites;
        _sprites = !string.IsNullOrEmpty(_cart.SpriteImage) ? Pico8Utils.ImageToColorArray(this, _cart.SpriteImage) : _sprites;
        _flags = Pico8Utils.DataToArray(_cart.FlagData, 2);
        if (_cart.MapDimensions.x * _cart.MapDimensions.y != _cart.MapData.Length / 2)
            throw new Exception($"Map dimensions do not match map data length. Map dimensions: {_cart.MapDimensions.x}x{_cart.MapDimensions.y}, Map data length: {_cart.MapData.Length / 2}");
        _map = Pico8Utils.MapDataToArray(_cart.MapData);
    }


    public F32 Rnd(double limit = 1.0, Random? r = null) // https://pico-8.fandom.com/wiki/Rnd
    {
        if (r is null) { r = random; }
        return F32.FromDouble(r.NextDouble() * limit);
    }


    public void Sfx(double n, double channel = -1.0, double offset = 0.0, double length = 31.0) // https://pico-8.fandom.com/wiki/Sfx
    {
        _outputFacade?.PlaySound((int)n, (int)channel, (int)offset, (int)length);
    }


    public F32 Sin(F32 angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Sin
    {
        angle = Mod(angle, 1);
        return F32.FromRaw((int)(sinDict.LookupTable[angle.Raw / 10.0] * 10));
    }


    private int Sget(double x, double y) // https://pico-8.fandom.com/wiki/Sget
    {
        int xFlr = (int)Math.Floor(x);
        int yFlr = (int)Math.Floor(y);

        if (xFlr < 0 || yFlr < 0 || xFlr > 127 || yFlr > 127)
        {
            return 0;
        }

        Color col = _sprites[xFlr + yFlr * 128];
        if (Colors.Contains(col))
        {
            return Colors.IndexOf(col);
        }
        return 0;
    }


    private void Srand(int seed) // https://pico-8.fandom.com/wiki/Srand
    {
        random = new Random(seed);
    }


    private void Sset(double x, double y, double col) // https://pico-8.fandom.com/wiki/Sset
    {
        int xFlr = (int)Math.Floor(x);
        int yFlr = (int)Math.Floor(y);
        int colFlr = (int)Math.Floor(col);

        if (xFlr < 0 || yFlr < 0 || xFlr > 127 || yFlr > 127)
        {
            _sprites[xFlr + yFlr * 128] = Colors[colFlr];
        }
    }

    public void Spr(double spriteNumber, double x, double y, double w = 1.0, double h = 1.0, bool flip_x = false, bool flip_y = false) // https://pico-8.fandom.com/wiki/Spr
    {
        if (_spriteCache is null)
            return;

        int spriteNumberFlr = (int)Math.Floor(spriteNumber);
        int xFlr = (int)Math.Floor(x) - 8;
        int yFlr = (int)Math.Floor(y) - 8;
        int wFlr = (int)Math.Floor(w);
        int hFlr = (int)Math.Floor(h);

        int spriteWidth = 8;
        int spriteHeight = 8;

        int spriteX = spriteNumberFlr % 16 * spriteWidth;
        int spriteY = spriteNumberFlr / 16 * spriteHeight;

        // Build cache key from sprite number + palette state  
        List<int> cacheList = [spriteNumberFlr];
        var palRemappings = _paletteManager?.PaletteMap.Values.ToList() ?? new();
        foreach (var palRemapping in palRemappings)
        {
            cacheList.Add((int)palRemapping.C0.PackedValue);
            cacheList.Add((int)palRemapping.C1.PackedValue);
            cacheList.Add(palRemapping.Trans ? 0 : 1);
        }
        int[] cache = cacheList.ToArray();

        // Try to get from cache, otherwise create texture
        if (!_spriteCache.TryGetTexture(cache, out Texture2D? texture))
        {
            texture = Pico8Utils.CreateTextureFromSpriteData(this, _sprites, spriteX, spriteY, spriteWidth * wFlr, spriteHeight * hFlr);
            if (texture != null)
                _spriteCache.AddTexture(cache, texture);
        }

        if (texture is null)
            return;

        Vector2 position = new(((flip_x ? xFlr + 2 * spriteWidth * wFlr - spriteWidth : xFlr + spriteWidth) - F32.FloorToInt(CameraOffset.x)) * Cell.Width, ((flip_y ? yFlr + 2 * spriteHeight * hFlr - spriteHeight : yFlr + spriteHeight) - F32.FloorToInt(CameraOffset.y)) * Cell.Height);
        Vector2 size = new(Cell.Width, Cell.Height);
        SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flip_y ? SpriteEffects.FlipVertically : SpriteEffects.None);

        Batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
    }


    public void Sspr(double sx, double sy, double sw, double sh, double dx, double dy, double dw = -1, double dh = -1, bool flip_x = false, bool flip_y = false) // https://pico-8.fandom.com/wiki/Sspr
    {
        if (_spriteCache is null)
            return;

        int sxFlr = (int)Math.Floor(sx);
        int syFlr = (int)Math.Floor(sy);
        int swFlr = (int)Math.Floor(sw);
        int shFlr = (int)Math.Floor(sh);
        int dxFlr = (int)Math.Floor(dw) > 8 ? (int)Math.Floor(dx) - 4 : (int)Math.Floor(dx) - 8;
        int dyFlr = (int)Math.Floor(dh) > 8 ? (int)Math.Floor(dy) - 4 : (int)Math.Floor(dy) - 8;
        int dwFlr = dw == -1 ? swFlr : (int)Math.Floor(dw) > 8 ? (int)Math.Floor(dw / 4) + 1 : (int)Math.Floor(dw / 8);
        int dhFlr = dh == -1 ? shFlr : (int)Math.Floor(dh) > 8 ? (int)Math.Floor(dh / 4) + 1 : (int)Math.Floor(dh / 8);

        int spriteWidth = swFlr;
        int spriteHeight = shFlr;

        int spriteNumberFlr = sxFlr * 100 + syFlr * 100 + swFlr * 100 + shFlr * 100;

        // Build cache key from sprite number + palette state
        List<int> cacheList = [spriteNumberFlr];
        var palRemappings = _paletteManager?.PaletteMap.Values.ToList() ?? new();
        foreach (var palRemapping in palRemappings)
        {
            cacheList.Add((int)palRemapping.C0.PackedValue);
            cacheList.Add((int)palRemapping.C1.PackedValue);
            cacheList.Add(palRemapping.Trans ? 0 : 1);
        }
        int[] cache = cacheList.ToArray();

        // Try to get from cache, otherwise create texture
        if (!_spriteCache.TryGetTexture(cache, out Texture2D? texture))
        {
            texture = Pico8Utils.CreateTextureFromSpriteData(this, _sprites, sxFlr, syFlr, swFlr, shFlr);
            if (texture != null)
                _spriteCache.AddTexture(cache, texture);
        }

        if (texture is null)
            return;

        Vector2 position = new(((flip_x ? dxFlr + 2 * spriteWidth * swFlr - spriteWidth : dxFlr + spriteWidth) - F32.FloorToInt(CameraOffset.x)) * Cell.Width, ((flip_y ? dyFlr + 2 * spriteHeight * shFlr - spriteHeight : dyFlr + spriteHeight) - F32.FloorToInt(CameraOffset.y)) * Cell.Height);
        Vector2 size = new(dwFlr * Cell.Width, dhFlr * Cell.Height);
        SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flip_y ? SpriteEffects.FlipVertically : SpriteEffects.None);

        Batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
    }


    public void Dispose()
    {
        // Dispose manager instances
        _musicManager?.StopAll();
        _audioChannels?.StopAll();
        _spriteCache?.Dispose();
        _paletteManager = null;
        _musicManager = null;
        _audioChannels = null;
        _spriteCache = null;
    }


    internal void SoundDispose()
    {
        // Delegate to managers
        _musicManager?.StopAll();
        _audioChannels?.StopAll();
    }

    public void UpdateViewport()
    {
        double windowWidth = Window.ClientBounds.Width;
        double windowHeight = Window.ClientBounds.Height;

        if (!Graphics.IsFullScreen)
        {
            windowWidth /= Resolution.w;
            windowHeight /= Resolution.h;
            windowWidth *= _cart.Resolution.w;
            windowHeight *= _cart.Resolution.h;

            Graphics.PreferredBackBufferWidth = (int)windowWidth;
            Graphics.PreferredBackBufferHeight = (int)windowHeight;
            Graphics.ApplyChanges();
        }
        Resolution = _cart.Resolution;

        int scale = Math.Min((int)windowWidth / _cart.Resolution.w, (int)windowHeight / _cart.Resolution.h);
        int width = _cart.Resolution.w * scale;
        int height = _cart.Resolution.h * scale;

        // Calculate the exact center of the client area
        double centerX = windowWidth / 2.0;
        double centerY = windowHeight / 2.0;

        // Calculate the top left corner of the square so that its center aligns with the client area's center
        int left = (int)Math.Round(centerX - width / 2.0);
        int top = (int)Math.Round(centerY - height / 2.0);

        // Set the viewport to the square area
        GraphicsDevice.Viewport = new Viewport(left, top, width, height);
    }
}