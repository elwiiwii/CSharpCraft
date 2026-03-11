using CSharpCraft.OptionsMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using FixMath;
using System.Reflection;
using CSharpCraft.Competitive;

namespace CSharpCraft.Pico8;

public class Pico8Functions : IDisposable
{
    public SpriteBatch Batch { get; }
    public (F32 x, F32 y) CameraOffset { get; internal set; } = (F32.Zero, F32.Zero);
    public (int Width, int Height) Cell { get; internal set; }
    public GraphicsDeviceManager Graphics { get; }
    public GraphicsDevice GraphicsDevice { get; }
    public Dictionary<string, SoundEffect> MusicDictionary { get; }
    public OptionsFile OptionsFile { get; set; }
    public Texture2D Pixel { get; }
    public (int w, int h) Resolution { get; private set; } = (128, 128);
    public List<IScene> Scenes { get; }
    public Dictionary<string, SoundEffect> SoundEffectDictionary { get; }
    public Dictionary<string, Texture2D> TextureDictionary { get; }
    public TitleScreen TitleScreen { get; }
    public GameWindow Window { get; }

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
    public List<PalCol> PalColors { get; } = [new(Color.Black, Color.Black, true)];

    private int[] _flags;
    public int[] _map;
    private Color[] _sprites;
    private Dictionary<string, List<SongInst>> _music;
    private Dictionary<string, Dictionary<int, string>> _sfx;

    public IScene _cart;
    public List<List<MusicInst>> channelMusic = [];
    public List<SoundEffectInstance> channel0 = [];
    public List<SoundEffectInstance> channel1 = [];
    public List<SoundEffectInstance> channel2 = [];
    public List<SoundEffectInstance> channel3 = [];
    private readonly P8Btns buttons;
    private bool isPaused;
    private readonly Dictionary<int[], Texture2D> spriteTextures = new(new IntArrayEqualityComparer());
    private List<MenuItem> mainMenuItems;
    private List<MenuItem> curMenuItems;
    private int menuSelected;
    private int curSoundtrack;
    private int curSfxPack;
    private int curTrack;
    private (List<SoundEffectInstance> fromSong, List<SoundEffectInstance> toSong) musicTransition;
    private int? lastMusicCall;
    private readonly CosDict cosDict = new();
    private readonly SinDict sinDict = new();
    Random random = new();
    private Func<IScene>? scheduledSceneChange;

    public Pico8Functions(IScene cart, TitleScreen _titleScreen, List<IScene> _scenes, Dictionary<string, Texture2D> _textureDictionary, Dictionary<string, SoundEffect> _soundEffectDictionary, Dictionary<string, SoundEffect> _musicDictionary, Texture2D _pixel, SpriteBatch _batch, GraphicsDeviceManager _graphics, GraphicsDevice _graphicsDevice, GameWindow _window, OptionsFile _optionsFile)
    {
        Batch = _batch;
        Graphics = _graphics;
        GraphicsDevice = _graphicsDevice;
        MusicDictionary = _musicDictionary;
        OptionsFile = _optionsFile;
        Pixel = _pixel;
        Scenes = _scenes;
        SoundEffectDictionary = _soundEffectDictionary;
        TextureDictionary = _textureDictionary;
        TitleScreen = _titleScreen;
        Window = _window;

        buttons = new();
        buttons.Reset(this);

        isPaused = false;

        menuSelected = 0;
        musicTransition = new();
        lastMusicCall = null;

        curSoundtrack = OptionsFile.Pcraft_Soundtrack;
        curSfxPack = OptionsFile.Pcraft_Sfx_Pack;

        _sprites = [];
        _flags = [];
        _map = [];
        _music = [];
        _sfx = [];
        mainMenuItems = [];
        curMenuItems = [];

        LoadCart(cart);
    }

    public void ScheduleScene(Func<IScene> sceneFactory)
    {
        scheduledSceneChange = sceneFactory;
    }

    private void LoadCart(IScene cart)
    {
        if (_cart is not null)
        {
            _cart.Dispose();
        }
        _sprites = [];
        _flags = [];
        _map = [];
        _music = cart.Music;
        _sfx = cart.Sfx;
        mainMenuItems = [];
        curMenuItems = [];

        _cart = cart;

        buttons.Reset(this);

        SoundDispose();

        UpdateViewport();

        void Continue()
        {
            if (Btnp(4) || Btnp(5))
            {
                isPaused = false;
            }
        }
        Menuitem(0, () => "continue", () => Continue(), mainMenuItems);

        void Options()
        {
            if (Btnp(4) || Btnp(5))
            {
                menuSelected = 0;
                mainMenuItems.Clear();
                foreach (var item in curMenuItems)
                {
                    mainMenuItems.Add(item.Clone());
                }
                curMenuItems.Clear();

                void Sound()
                {
                    if (Btnp(0) || Btnp(1) || Btnp(4) || Btnp(5))
                    {
                        PropertyInfo? propertyName = typeof(OptionsFile).GetProperty("Gen_Sound_On");
                        if (propertyName is null) { return; }
                        propertyName.SetValue(OptionsFile, !OptionsFile.Gen_Sound_On);
                        OptionsFile.JsonWrite(OptionsFile);
                        if (!OptionsFile.Gen_Sound_On) { Mute(); }
                    }
                }
                Menuitem(0, () => $"sound:{(OptionsFile.Gen_Sound_On ? "on" : "off")}", () => Sound(), curMenuItems);

                void MusicVol()
                {
                    if (Btnp(0))
                    {
                        PropertyInfo? propertyName = typeof(OptionsFile).GetProperty("Gen_Music_Vol");
                        if (propertyName is null) { return; }
                        propertyName.SetValue(OptionsFile, Math.Max(OptionsFile.Gen_Music_Vol - 10, 0));
                        OptionsFile.JsonWrite(OptionsFile);
                    }
                    if (Btnp(1))
                    {
                        PropertyInfo? propertyName = typeof(OptionsFile).GetProperty("Gen_Music_Vol");
                        if (propertyName is null) { return; }
                        propertyName.SetValue(OptionsFile, Math.Min(OptionsFile.Gen_Music_Vol + 10, 100));
                        OptionsFile.JsonWrite(OptionsFile);
                    }
                }
                Menuitem(1, () => $"music vol:{OptionsFile.Gen_Music_Vol}%", () => MusicVol(), curMenuItems);

                void SfxVol()
                {
                    if (Btnp(0))
                    {
                        PropertyInfo? propertyName = typeof(OptionsFile).GetProperty("Gen_Sfx_Vol");
                        if (propertyName is null) { return; }
                        propertyName.SetValue(OptionsFile, Math.Max(OptionsFile.Gen_Sfx_Vol - 10, 0));
                        OptionsFile.JsonWrite(OptionsFile);
                    }
                    if (Btnp(1))
                    {
                        PropertyInfo? propertyName = typeof(OptionsFile).GetProperty("Gen_Sfx_Vol");
                        if (propertyName is null) { return; }
                        propertyName.SetValue(OptionsFile, Math.Min(OptionsFile.Gen_Sfx_Vol + 10, 100));
                        OptionsFile.JsonWrite(OptionsFile);
                    }
                }
                Menuitem(2, () => $"sfx vol:{OptionsFile.Gen_Sfx_Vol}%", () => SfxVol(), curMenuItems);

                void Fullscreen()
                {
                    if (Btnp(4) || Btnp(5))
                    {
                        PropertyInfo? propertyName = typeof(OptionsFile).GetProperty("Gen_Fullscreen");
                        if (propertyName is null) { return; }
                        propertyName.SetValue(OptionsFile, !OptionsFile.Gen_Fullscreen);
                        OptionsFile.JsonWrite(OptionsFile);
                        Graphics.IsFullScreen = OptionsFile.Gen_Fullscreen;
                        Graphics.PreferredBackBufferWidth = OptionsFile.Gen_Window_Width / 128 * Resolution.w;
                        Graphics.PreferredBackBufferHeight = OptionsFile.Gen_Window_Height / 128 * Resolution.h;
                        Graphics.ApplyChanges();
                        UpdateViewport();
                    }
                }
                Menuitem(3, () => $"fullscreen:{(OptionsFile.Gen_Fullscreen ? "on" : "off")}", () => Fullscreen(), curMenuItems);

                void Back()
                {
                    if (Btnp(4) || Btnp(5))
                    {
                        menuSelected = 0;
                        curMenuItems.Clear();
                        foreach (var item in mainMenuItems)
                        {
                            curMenuItems.Add(item.Clone());
                        }
                    }
                }
                Menuitem(4, () => "back", () => Back(), curMenuItems);

                void Sfx()
                {
                    if (Btnp(0))
                    {
                        curSfxPack -= 1;
                    }
                    if (Btnp(1))
                    {
                        curSfxPack += 1;
                    }
                    curSfxPack = GeneralFunctions.Loop(curSfxPack, _sfx.Count);
                    PropertyInfo? propertyName = typeof(OptionsFile).GetProperty("Pcraft_Sfx_Pack");
                    if (propertyName is null) { return; }
                    propertyName.SetValue(OptionsFile, curSfxPack);
                    OptionsFile.JsonWrite(OptionsFile);
                }
                if (_sfx.Count > 1)
                {
                    Menuitem(3, () => $"sfx:{_sfx.ElementAt(OptionsFile.Pcraft_Sfx_Pack).Key}", () => Sfx(), curMenuItems);
                }

                void Soundtrack()
                {
                    if (Btnp(0))
                    {
                        curSoundtrack -= 1;
                    }
                    if (Btnp(1))
                    {
                        curSoundtrack += 1;
                    }
                    curSoundtrack = GeneralFunctions.Loop(curSoundtrack, _music.Count);
                    PropertyInfo? propertyName = typeof(OptionsFile).GetProperty("Pcraft_Soundtrack");
                    if (propertyName is null) { return; }
                    propertyName.SetValue(OptionsFile, curSoundtrack);
                    OptionsFile.JsonWrite(OptionsFile);
                    if (Btnp(0) || Btnp(1))
                    {
                        SoundDispose();
                        if (lastMusicCall is not null) { Music((int)lastMusicCall); }
                    }
                }
                if (_music.Count > 1)
                {
                    Menuitem(3, () => $"music:{_music.ElementAt(OptionsFile.Pcraft_Soundtrack).Key}", () => Soundtrack(), curMenuItems);
                }
            }
        }
        Menuitem(1, () => "options", () => Options(), mainMenuItems);

        void ResetCart()
        {
            if (Btnp(4) || Btnp(5))
            {
                LoadCart(_cart);
            }
        }
        Menuitem(2, () => "reset cart", () => ResetCart(), mainMenuItems);

        void Exit()
        {
            if (Btnp(4) || Btnp(5))
            {
                LoadCart(new TitleScreen(false));
            }
        }
        Menuitem(3, () => "exit", () => Exit(), mainMenuItems);

        curMenuItems.Clear();
        foreach (var item in mainMenuItems)
        {
            curMenuItems.Add(item.Clone());
        }

        Reload();
        Init();
    }


    public void Init()
    {
        isPaused = false;
        _cart.Init(this);
    }


    public void Update()
    {
        Cell = (GraphicsDevice.Viewport.Width / _cart.Resolution.w, GraphicsDevice.Viewport.Height / _cart.Resolution.h);
        if (!(_cart.SceneName == "TitleScreen") && Btnp(6))
        {
            isPaused = !isPaused;
            menuSelected = 0;
        }
        buttons.UpPause(this);

        if (isPaused)
        {
            buttons.UpLockout(this, isPaused);

            if (Btnp(0) || Btnp(1) || Btnp(4) || Btnp(5)) { curMenuItems[menuSelected].Function(); }

            if (Btnp(2)) { menuSelected -= 1; }
            if (Btnp(3)) { menuSelected += 1; }
            menuSelected = GeneralFunctions.Loop(menuSelected, curMenuItems);

            PlaySound(false);
        }
        else
        {
            buttons.UpLockout(this, isPaused);

            PlaySound(true);

            _cart.Update();

            if (scheduledSceneChange is not null)
            {
                LoadCart(scheduledSceneChange());
                scheduledSceneChange = null;
            }
        }

        buttons.Update(this);

        float fadeStep = OptionsFile.Gen_Music_Vol / 1600.0f;

        foreach (List<MusicInst> song in channelMusic)
        {
            if (song[curTrack].Track.State == SoundState.Stopped)
            {
                if (song.Count > curTrack + 1)
                {
                    curTrack += 1;
                    song[curTrack].Track.IsLooped = song[curTrack].Loop;
                    song[curTrack].Track.Play();
                    if (OptionsFile.Gen_Sound_On) { song[curTrack].Track.Volume = OptionsFile.Gen_Music_Vol / 100.0f; }
                }
            }

            if (musicTransition.fromSong is null || musicTransition.toSong is null)
            {
                musicTransition = new();
                if (OptionsFile.Gen_Sound_On && lastMusicCall is not null && song[curTrack].Name == _music.ElementAt(curSoundtrack).Value[(int)lastMusicCall].Tracks[curTrack].name)
                {
                    song[curTrack].Track.Volume = OptionsFile.Gen_Music_Vol / 100.0f;
                }
            }
        }

        if (OptionsFile.Gen_Sound_On && musicTransition.fromSong is not null && musicTransition.toSong is not null && musicTransition.fromSong[curTrack].State == SoundState.Playing && musicTransition.toSong[curTrack].State == SoundState.Playing)
        {
            if (musicTransition.fromSong[curTrack].Volume > 0.0f)
            {
                musicTransition.fromSong[curTrack].Volume -= fadeStep;
            }
            if (musicTransition.toSong[curTrack].Volume < OptionsFile.Gen_Music_Vol / 100.0f)
            {
                musicTransition.toSong[curTrack].Volume += fadeStep;
            }
            if (musicTransition.fromSong[curTrack].Volume <= 0.0f && musicTransition.toSong[curTrack].Volume >= OptionsFile.Gen_Music_Vol / 100.0f)
            {
                musicTransition.fromSong[curTrack].Volume = 0.0f;
                musicTransition.toSong[curTrack].Volume = OptionsFile.Gen_Music_Vol / 100.0f;
                musicTransition = new();
            }
        }
    }

    private void PlaySound(bool play)
    {
        foreach (List<MusicInst> song in channelMusic)
        {
            if (song[curTrack].Track.State == SoundState.Paused && play) { song[curTrack].Track.Play(); }
            else if (song[curTrack].Track.State == SoundState.Playing && !play) { song[curTrack].Track.Pause(); }
        }
        foreach (List<SoundEffectInstance> channel in new List<List<SoundEffectInstance>>([channel0, channel1, channel2, channel3]))
        {
            foreach (SoundEffectInstance sfx in channel)
            {
                if (sfx.State == SoundState.Paused && play) { sfx.Play(); }
                else if (sfx.State == SoundState.Playing && !play) { sfx.Pause(); }
            }
        }
    }

    public void Draw()
    {
        Pal();
        Palt();
        _cart.Draw();

        if (isPaused)
        {
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
        return (isPaused || !buttons.Lockout[i]) && Pico8Utils.Ptn(this, i);
    }


    public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
    {
        int initDelay = 15;
        int repDelay = 4;
        if (i != 6 && Btn(i) && (!buttons.Prev[i] || buttons.HeldCount[i] == initDelay || buttons.HeldCount[i] > initDelay && (buttons.HeldCount[i] - initDelay) % repDelay == 0)) { return true; }
        else if (i == 6 && Btn(i) && !buttons.Prev[6]) { return true; }
        return false;
    }


    public void Camera() // https://pico-8.fandom.com/wiki/Camera
    {
        CameraOffset = (F32.Zero, F32.Zero);
    }


    public void Camera(F32 x, F32 y) // https://pico-8.fandom.com/wiki/Camera
    {
        CameraOffset = (x, y);
    }


    public static void CartData(string id) // https://pico-8.fandom.com/wiki/Cartdata
    {

    }


    public void Circ(F32 x, F32 y, double r, int c) // https://pico-8.fandom.com/wiki/Circ
    {
        if (r < 0) return;

        int xFlr = F32.FloorToInt(x);
        int yFlr = F32.FloorToInt(y);
        int rFlr = (int)Math.Floor(r);
        Color drawCol = PalColors.FindAll(x => x.C0 == Colors[c]).Count > 0 ? PalColors.First(x => x.C0 == Colors[c]).C1 : Colors[c];

        for (int i = xFlr - rFlr; i <= xFlr + rFlr; i++)
        {
            for (int j = yFlr - rFlr; j <= yFlr + rFlr; j++)
            {
                // Check if the point 0.36 units into the grid space from the center of the circle is within the circle
                double offsetX = i < xFlr ? 0.35D : -0.35D;
                double offsetY = j < yFlr ? 0.35D : -0.35D;
                double gridCenterX = i + offsetX;
                double gridCenterY = j + offsetY;

                bool isCurrentInCircle = Math.Pow(gridCenterX - xFlr, 2) + Math.Pow(gridCenterY - yFlr, 2) <= rFlr * rFlr;

                // Check all four adjacent grid spaces
                bool isRightOutsideCircle = Math.Pow(i + 1 + offsetX - xFlr, 2) + Math.Pow(j + offsetY - yFlr, 2) > rFlr * rFlr;
                bool isLeftOutsideCircle = Math.Pow(i - 1 + offsetX - xFlr, 2) + Math.Pow(j + offsetY - yFlr, 2) > rFlr * rFlr;
                bool isUpOutsideCircle = Math.Pow(i + offsetX - xFlr, 2) + Math.Pow(j + 1 + offsetY - yFlr, 2) > rFlr * rFlr;
                bool isDownOutsideCircle = Math.Pow(i + offsetX - xFlr, 2) + Math.Pow(j - 1 + offsetY - yFlr, 2) > rFlr * rFlr;

                if (isCurrentInCircle && (isRightOutsideCircle || isLeftOutsideCircle || isUpOutsideCircle || isDownOutsideCircle))
                {
                    // Calculate the position and size of the line
                    Vector2 position = new((i - F32.FloorToInt(CameraOffset.x)) * Cell.Width, (j - F32.FloorToInt(CameraOffset.y)) * Cell.Height);
                    Vector2 size = new(Cell.Width, Cell.Height);

                    // Draw the line
                    Batch.Draw(Pixel, position, null, drawCol, 0, Vector2.Zero, size, SpriteEffects.None, 0);
                }
            }
        }
    }


    public void Circfill(F32 x, F32 y, double r, int c) // https://pico-8.fandom.com/wiki/Circfill
    {
        if (r < 0) return;

        int xFlr = F32.FloorToInt(x);
        int yFlr = F32.FloorToInt(y);
        int rFlr = (int)Math.Floor(r);
        Color drawCol = PalColors.FindAll(x => x.C0 == Colors[c]).Count > 0 ? PalColors.First(x => x.C0 == Colors[c]).C1 : Colors[c];

        for (int i = xFlr - rFlr; i <= xFlr + rFlr; i++)
        {
            for (int j = yFlr - rFlr; j <= yFlr + rFlr; j++)
            {
                // Check if the point 0.36 units into the grid space from the center of the circle is within the circle
                double offsetX = i < xFlr ? 0.35D : -0.35D;
                double offsetY = j < yFlr ? 0.35D : -0.35D;
                double gridCenterX = i + offsetX;
                double gridCenterY = j + offsetY;

                if (Math.Pow(gridCenterX - xFlr, 2) + Math.Pow(gridCenterY - yFlr, 2) <= rFlr * rFlr)
                {
                    // Calculate the position and size
                    Vector2 position = new((i - F32.FloorToInt(CameraOffset.x)) * Cell.Width, (j - F32.FloorToInt(CameraOffset.y)) * Cell.Height);
                    Vector2 size = new(Cell.Width, Cell.Height);

                    // Draw
                    Batch.Draw(Pixel, position, null, drawCol, 0, Vector2.Zero, size, SpriteEffects.None, 0);
                }
            }
        }
    }


    public void Cls(int col = 0) // https://pico-8.fandom.com/wiki/Cls
    {
        Color clearCol = PalColors.FindAll(x => x.C0 == Colors[col]).Count > 0 ? PalColors.First(x => x.C0 == Colors[col]).C1 : Colors[col];
        GraphicsDevice.Clear(clearCol);
    }


    public F32 Cos(F32 angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Cos
    {
        angle = Mod(angle, 1);
        F32 val = F32.FromRaw((int)(cosDict.LookupTable[angle.Raw / 10.0] * 10));
        return val;
    }


    public static void Cstore() // https://pico-8.fandom.com/wiki/Cstore
    {

    }


    public void Del<T>(List<T> table, T value) // https://pico-8.fandom.com/wiki/Del
    {
        table.Remove(value);
    }


    public static F32 Dget(int index) // https://pico-8.fandom.com/wiki/Dget
    {
        return F32.FromInt(index);
    }


    public static void Dset(int index, double value) // https://pico-8.fandom.com/wiki/Dset
    {

    }


    public int Fget(int n) // https://pico-8.fandom.com/wiki/Fget
    {
        return _flags[n];
    }


    public static void Load(string fileName)
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
                if (flags == 0 || flags == Fget(Mget(celx + i, cely + j)))
                {
                    Spr(Mget(i + celx, j + cely), sx + i * 8, sy + j * 8);
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


    public void Menuitem(int pos, Func<string> getName, Action function, List<MenuItem>? list = null) // https://pico-8.fandom.com/wiki/Menuitem
    {
        if (list is null) { list = curMenuItems; }
        list.Insert(pos, new MenuItem(getName, function));
    }


    public int Mget(double celx, double cely) // https://pico-8.fandom.com/wiki/Mget
    {
        int xFlr = Math.Abs((int)Math.Floor(celx));
        int yFlr = Math.Abs((int)Math.Floor(cely));

        return _map[xFlr + yFlr * _cart.MapDimensions.x];
    }


    public static F32 Mod(F32 x, int m)
    {
        F32 r = x % m;
        return r < 0 ? r + m : r;
    }


    public void Mset(double celx, double cely, double snum = 0) // https://pico-8.fandom.com/wiki/Mset
    {
        int xFlr = (int)Math.Floor(celx);
        int yFlr = (int)Math.Floor(cely);
        int sFlr = (int)Math.Floor(snum);

        _map[xFlr + yFlr * _cart.MapDimensions.x] = sFlr;
    }


    public void Music(int n, double fadems = 0) // https://pico-8.fandom.com/wiki/Music
    {
        lastMusicCall = n;
        SongInst curSong = _music.ElementAt(OptionsFile.Pcraft_Soundtrack).Value[n];

        if (channelMusic.Count > 0 && channelMusic[0][0].Group == curSong.Group)
        {
            foreach (List<MusicInst> item in channelMusic)
            {
                if (item[0].Name != curSong.Tracks[0].name)
                {
                    foreach (MusicInst sfxInst in item)
                    {
                        musicTransition.fromSong = [];
                        musicTransition.fromSong.Add(sfxInst.Track);
                    }
                }
                else
                {
                    foreach (MusicInst sfxInst in item)
                    {
                        musicTransition.toSong = [];
                        musicTransition.toSong.Add(sfxInst.Track);
                    }
                }
            }
        }
        else
        {
            SoundDispose();

            foreach (SongInst song in _music.ElementAt(OptionsFile.Pcraft_Soundtrack).Value)
            {
                if (song.Group == curSong.Group)
                {
                    List<MusicInst> listOfTracks = [];
                    foreach ((string name, bool loop) in song.Tracks)
                    {
                        listOfTracks.Add(new(name, MusicDictionary[name].CreateInstance(), loop, song.Group));
                    }
                    channelMusic.Add(listOfTracks);
                }
            }

            foreach (List<MusicInst> item in channelMusic)
            {
                item[0].Track.IsLooped = item[0].Loop;
                item[0].Track.Play();
                item[0].Track.Volume = item[0].Name == curSong.Tracks[0].name ? OptionsFile.Gen_Sound_On ? OptionsFile.Gen_Music_Vol / 100.0f : 0 : 0;
                curTrack = 0;
            }
        }
    }


    public void Mute()
    {
        foreach (List<MusicInst> song in channelMusic)
        {
            foreach (MusicInst track in song)
            {
                track.Track.Volume = 0.0f;
            }
        }
        foreach (List<SoundEffectInstance> channel in new List<List<SoundEffectInstance>>([channel0, channel1, channel2, channel3]))
        {
            foreach (SoundEffectInstance sfx in channel)
            {
                sfx.Volume = 0.0f;
            }
        }
    }


    public void Pal() // https://pico-8.fandom.com/wiki/Pal
    {
        PalColors.Clear();
        PalColors.Add(new(Colors[0], Colors[0], true));
    }


    public void Pal(int c0, int c1) // https://pico-8.fandom.com/wiki/Pal
    {
        PalColors.FindAll(x => x.C0 == Colors[c0]).ForEach(x => PalColors.Remove(x));
        PalColors.Add(new(Colors[c0], Colors[c1], false));
    }


    public void Pal(Color c0, Color c1) // https://pico-8.fandom.com/wiki/Pal
    {
        PalColors.FindAll(x => x.C0 == c0).ForEach(x => PalColors.Remove(x));
        PalColors.Add(new(c0, c1, false));
    }


    public void Palt() // https://pico-8.fandom.com/wiki/Palt
    {
        PalColors.ForEach(x => x.Trans = false);
        PalColors[0].Trans = true;
    }


    public void Palt(int col, bool t) // https://pico-8.fandom.com/wiki/Palt
    {
        if (PalColors.FindAll(x => x.C0 == Colors[col]).Count <= 0) { PalColors.Add(new(Colors[col], Colors[col], t)); }
        PalColors.FindAll(x => x.C0 == Colors[col]).ForEach(x => x.Trans = t);
    }


    public void Palt(Color col, bool t) // https://pico-8.fandom.com/wiki/Palt
    {
        if (PalColors.FindAll(x => x.C0 == col).Count <= 0) { PalColors.Add(new(col, col, t)); }
        PalColors.FindAll(x => x.C0 == col).ForEach(x => x.Trans = t);
    }


    public void Print(string str, double x, double y, double c) // https://pico-8.fandom.com/wiki/Print
    {
        int xFlr = (int)Math.Floor(x);
        int yFlr = (int)Math.Floor(y);
        int cFlr = (int)Math.Floor(c);

        int charWidth = 4;
        //int charHeight = 5;

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
                        //int charEndX = charStartX + Cell.Width - CameraOffset.x;
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
        int xFlr = F32.FloorToInt(x);
        int yFlr = F32.FloorToInt(y);
        //float yFlr = (float)(Math.Floor(y) - 0.5);
        int cFlr = (int)Math.Floor(c);

        // Calculate the position and size of the line
        Vector2 position = new((xFlr - F32.FloorToInt(CameraOffset.x)) * Cell.Width, (yFlr - F32.FloorToInt(CameraOffset.y)) * Cell.Height);
        Vector2 size = new(Cell.Width, Cell.Height);

        // Draw the line
        Batch.Draw(Pixel, position, null, Colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
    }


    public void Rect(double x1, double y1, double x2, double y2, double c) // https://pico-8.fandom.com/wiki/Rect
    {
        int x1Flr = (int)Math.Floor(Math.Min(x1, x2));
        int y1Flr = (int)Math.Floor(Math.Min(y1, y2));
        int x2Flr = (int)Math.Floor(Math.Max(x1, x2));
        int y2Flr = (int)Math.Floor(Math.Max(y1, y2));
        int cFlr = (int)Math.Floor(c);

        Rectfill(x1Flr, y1Flr, x2Flr, y1Flr, cFlr);
        Rectfill(x1Flr, y2Flr, x2Flr, y2Flr, cFlr);
        Rectfill(x1Flr, y1Flr, x1Flr, y2Flr, cFlr);
        Rectfill(x2Flr, y1Flr, x2Flr, y2Flr, cFlr);
    }


    public void Rect(double x1, double y1, double x2, double y2, Color c) // https://pico-8.fandom.com/wiki/Rect
    {
        int x1Flr = (int)Math.Floor(Math.Min(x1, x2));
        int y1Flr = (int)Math.Floor(Math.Min(y1, y2));
        int x2Flr = (int)Math.Floor(Math.Max(x1, x2));
        int y2Flr = (int)Math.Floor(Math.Max(y1, y2));

        Rectfill(x1Flr, y1Flr, x2Flr, y1Flr, c);
        Rectfill(x1Flr, y2Flr, x2Flr, y2Flr, c);
        Rectfill(x1Flr, y1Flr, x1Flr, y2Flr, c);
        Rectfill(x2Flr, y1Flr, x2Flr, y2Flr, c);
    }


    public void Rectfill(double x1, double y1, double x2, double y2, double c) // https://pico-8.fandom.com/wiki/Rectfill
    {
        int x1Flr = (int)Math.Floor(Math.Min(x1, x2));
        int y1Flr = (int)Math.Floor(Math.Min(y1, y2));
        int x2Flr = (int)Math.Floor(Math.Max(x1, x2));
        int y2Flr = (int)Math.Floor(Math.Max(y1, y2));
        int cFlr = (int)Math.Floor(c);

        int rectStartX = (x1Flr - F32.FloorToInt(CameraOffset.x)) * Cell.Width;
        int rectStartY = (y1Flr - F32.FloorToInt(CameraOffset.y)) * Cell.Height;

        int rectSizeX = (x2Flr - x1Flr + 1) * Cell.Width;
        int rectSizeY = (y2Flr - y1Flr + 1) * Cell.Height;

        //int rectEndX = (x2Flr - CameraOffset.x) * Cell.Width;
        //int rectThickness = (y2Flr - y1Flr) * Cell.Height;
        //batch.DrawLine(pixel, new Vector2(rectStartX, rectStartY), new Vector2(rectEndX, rectStartY), colors[cFlr], rectThickness);

        Vector2 position = new(rectStartX, rectStartY);
        Vector2 size = new(rectSizeX, rectSizeY);

        Batch.Draw(Pixel, position, null, Colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
    }

    public void Rectfill(double x1, double y1, double x2, double y2, Color c) // https://pico-8.fandom.com/wiki/Rectfill
    {
        int x1Flr = (int)Math.Floor(Math.Min(x1, x2));
        int y1Flr = (int)Math.Floor(Math.Min(y1, y2));
        int x2Flr = (int)Math.Floor(Math.Max(x1, x2));
        int y2Flr = (int)Math.Floor(Math.Max(y1, y2));

        int rectStartX = (x1Flr - F32.FloorToInt(CameraOffset.x)) * Cell.Width;
        int rectStartY = (y1Flr - F32.FloorToInt(CameraOffset.y)) * Cell.Height;

        int rectSizeX = (x2Flr - x1Flr + 1) * Cell.Width;
        int rectSizeY = (y2Flr - y1Flr + 1) * Cell.Height;

        //int rectEndX = (x2Flr - CameraOffset.x) * Cell.Width;
        //int rectThickness = (y2Flr - y1Flr) * Cell.Height;
        //batch.DrawLine(pixel, new Vector2(rectStartX, rectStartY), new Vector2(rectEndX, rectStartY), colors[cFlr], rectThickness);

        Vector2 position = new(rectStartX, rectStartY);
        Vector2 size = new(rectSizeX, rectSizeY);

        Batch.Draw(Pixel, position, null, c, 0, Vector2.Zero, size, SpriteEffects.None, 0);
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
        int nFlr = (int)Math.Floor(n);
        int channelFlr = (int)Math.Floor(channel);

        List<SoundEffectInstance>? c = channelFlr switch
        {
            0 => channel0,
            1 => channel1,
            2 => channel2,
            3 => channel3,
            _ => throw new ArgumentOutOfRangeException(nameof(channel)),
        };

        if (c is not null)
        {
            foreach (SoundEffectInstance sfxInstance in c)
            {
                sfxInstance.Dispose();
            }

            SoundEffectInstance instance = SoundEffectDictionary[_sfx.ElementAt(OptionsFile.Pcraft_Sfx_Pack).Value[nFlr]].CreateInstance();

            c.Add(instance);

            instance.Play();
            instance.Volume = OptionsFile.Gen_Sound_On ? OptionsFile.Gen_Sfx_Vol / 100.0f : 0;
        }
        else
        {
            return;
        }
    }


    public F32 Sin(F32 angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Sin
    {
        angle = Mod(angle, 1);
        F32 val = F32.FromRaw((int)(sinDict.LookupTable[angle.Raw / 10.0] * 10));
        return val;
    }


    public int Sget(double x, double y) // https://pico-8.fandom.com/wiki/Sget
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


    public void Srand(int seed) // https://pico-8.fandom.com/wiki/Srand
    {
        random = new(seed);
    }


    public void Sset(double x, double y, double col) // https://pico-8.fandom.com/wiki/Sset
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
        int spriteNumberFlr = (int)Math.Floor(spriteNumber);
        int xFlr = (int)Math.Floor(x) - 8;
        int yFlr = (int)Math.Floor(y) - 8;
        int wFlr = (int)Math.Floor(w);
        int hFlr = (int)Math.Floor(h);

        int spriteWidth = 8;
        int spriteHeight = 8;

        int spriteX = spriteNumberFlr % 16 * spriteWidth;
        int spriteY = spriteNumberFlr / 16 * spriteHeight;

        List<int> cacheList = [spriteNumberFlr];
        for (int i = 0; i < PalColors.Count; i++)
        {
            cacheList.Add((int)PalColors[i].C0.PackedValue);
            cacheList.Add((int)PalColors[i].C1.PackedValue);
            cacheList.Add(PalColors[i].Trans ? 0 : 1);
        }
        int[] cache = cacheList.ToArray();
        if (!spriteTextures.TryGetValue(cache, out Texture2D? texture))
        {
            texture = Pico8Utils.CreateTextureFromSpriteData(this, _sprites, spriteX, spriteY, spriteWidth * wFlr, spriteHeight * hFlr);
            spriteTextures[cache] = texture;
        }

        Vector2 position = new(((flip_x ? xFlr + 2 * spriteWidth * wFlr - spriteWidth : xFlr + spriteWidth) - F32.FloorToInt(CameraOffset.x)) * Cell.Width, ((flip_y ? yFlr + 2 * spriteHeight * hFlr - spriteHeight : yFlr + spriteHeight) - F32.FloorToInt(CameraOffset.y)) * Cell.Height);
        Vector2 size = new(Cell.Width, Cell.Height);
        SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flip_y ? SpriteEffects.FlipVertically : SpriteEffects.None);

        Batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
    }


    public void Sspr(double sx, double sy, double sw, double sh, double dx, double dy, double dw = -1, double dh = -1, bool flip_x = false, bool flip_y = false) // https://pico-8.fandom.com/wiki/Sspr
    {
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

        List<int> cacheList = [spriteNumberFlr];
        for (int i = 0; i < PalColors.Count; i++)
        {
            cacheList.Add((int)PalColors[i].C0.PackedValue);
            cacheList.Add((int)PalColors[i].C1.PackedValue);
            cacheList.Add(PalColors[i].Trans ? 0 : 1);
        }
        int[] cache = cacheList.ToArray();
        if (!spriteTextures.TryGetValue(cache, out Texture2D? texture))
        {
            texture = Pico8Utils.CreateTextureFromSpriteData(this, _sprites, sxFlr, syFlr, swFlr, shFlr);
            spriteTextures[cache] = texture;
        }

        Vector2 position = new(((flip_x ? dxFlr + 2 * spriteWidth * swFlr - spriteWidth : dxFlr + spriteWidth) - F32.FloorToInt(CameraOffset.x)) * Cell.Width, ((flip_y ? dyFlr + 2 * spriteHeight * shFlr - spriteHeight : dyFlr + spriteHeight) - F32.FloorToInt(CameraOffset.y)) * Cell.Height);
        Vector2 size = new(dwFlr * Cell.Width, dhFlr * Cell.Height);
        SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flip_y ? SpriteEffects.FlipVertically : SpriteEffects.None);

        Batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
    }


    public void Dispose()
    {
        foreach (Texture2D texture in spriteTextures.Values)
        {
            texture.Dispose();
        }
        spriteTextures.Clear();
    }


    public void SoundDispose()
    {
        if (channelMusic is not null)
        {
            foreach (List<MusicInst> songList in channelMusic)
            {
                foreach (MusicInst song in songList)
                {
                    song.Track.Dispose();
                }
            }
            channelMusic = [];
        }

        for (int i = 0; i < 4; i++)
        {
            List<SoundEffectInstance>? c = i switch
            {
                0 => channel0,
                1 => channel1,
                2 => channel2,
                3 => channel3,
                _ => throw new ArgumentOutOfRangeException(nameof(i)),
            };
            if (c is not null)
            {
                foreach (SoundEffectInstance sfxInstance in c)
                {
                    sfxInstance.Dispose();
                }
            }
        }
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