using CSharpCraft.OptionsMenu;
using CSharpCraft.Pcraft;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using FixMath;
using System.Reflection;

namespace CSharpCraft.Pico8
{
    public class Pico8Functions : IDisposable
    {
        public SpriteBatch Batch { get; }
        public GraphicsDeviceManager Graphics { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public Dictionary<string, SoundEffect> MusicDictionary { get; }
        public OptionsFile OptionsFile { get; set; }
        public Texture2D Pixel { get; }
        public List<IScene> Scenes { get; }
        public Dictionary<string, SoundEffect> SoundEffectDictionary { get; }
        public Dictionary<string, Texture2D> TextureDictionary { get; }

        private int[] _flags;
        private int[] _map;
        private int[] _sprites;
        private Dictionary<string, List<(List<(string name, bool loop)> tracks, int group)>> _music;
        private Dictionary<string, Dictionary<int, string>> _sfx;
        private (int, int) CameraOffset = (0, 0);
        public IScene _cart;
        public List<List<(string name, SoundEffectInstance track, bool loop, int group)>> channelMusic = [];
        public List<SoundEffectInstance> channel0 = [];
        public List<SoundEffectInstance> channel1 = [];
        public List<SoundEffectInstance> channel2 = [];
        public List<SoundEffectInstance> channel3 = [];
        private bool prev0;
        private bool prev1;
        private bool prev2;
        private bool prev3;
        private bool prev4;
        private bool prev5;
        private bool prev6;
        private bool[] lockout;
        private int heldCount0;
        private int heldCount1;
        private int heldCount2;
        private int heldCount3;
        private int heldCount4;
        private int heldCount5;
        private bool isPaused;
        private Dictionary<int[], Texture2D> spriteTextures = new(new EqualityComparer());
        private List<MenuItem> menuItems;
        private int menuSelected;
        private int curSoundtrack;
        private int curSfxPack;
        private int curTrack;
        private (List<SoundEffectInstance> fromSong, List<SoundEffectInstance> toSong) musicTransition;
        private int? lastMusicCall;
        private CosDict cosDict = new();
        private SinDict sinDict = new();

        public Pico8Functions(IScene cart, List<IScene> _scenes, Dictionary<string, Texture2D> _textureDictionary, Dictionary<string, SoundEffect> _soundEffectDictionary, Dictionary<string, SoundEffect> _musicDictionary, Texture2D _pixel, SpriteBatch _batch, GraphicsDeviceManager _graphics, GraphicsDevice _graphicsDevice, OptionsFile _optionsFile)
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

            prev0 = false;
            prev1 = false;
            prev2 = false;
            prev3 = false;
            prev4 = false;
            prev5 = false;
            prev6 = false;

            lockout = [true, true, true, true, true, true, true];

            heldCount0 = 0;
            heldCount1 = 0;
            heldCount2 = 0;
            heldCount3 = 0;
            heldCount4 = 0;
            heldCount5 = 0;

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
            menuItems = [];

            LoadCart(cart);
        }

        public void LoadCart(IScene cart)
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
            menuItems = [];

            _cart = cart;

            lockout = [true, true, true, true, true, true, true];

            heldCount0 = 0;
            heldCount1 = 0;
            heldCount2 = 0;
            heldCount3 = 0;
            heldCount4 = 0;
            heldCount5 = 0;

            SoundDispose();

            void Continue()
            {
                if (Btnp(4) || Btnp(5))
                {
                    isPaused = false;
                }
            }
            Menuitem(0, () => "continue", () => Continue());

            void Options()
            {
                if (Btnp(4) || Btnp(5))
                {
                    menuSelected = 0;
                    menuItems.Clear();

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
                    Menuitem(0, () => $"sound:{(OptionsFile.Gen_Sound_On ? "on" : "off")}", () => Sound());

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
                    Menuitem(1, () => $"music vol:{OptionsFile.Gen_Music_Vol}%", () => MusicVol());

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
                    Menuitem(2, () => $"sfx vol:{OptionsFile.Gen_Sfx_Vol}%", () => SfxVol());

                    void Fullscreen()
                    {
                        if (Btnp(4) || Btnp(5))
                        {
                            PropertyInfo? propertyName = typeof(OptionsFile).GetProperty("Gen_Fullscreen");
                            if (propertyName is null) { return; }
                            propertyName.SetValue(OptionsFile, !OptionsFile.Gen_Fullscreen);
                            OptionsFile.JsonWrite(OptionsFile);
                            Graphics.ToggleFullScreen();
                        }
                    }
                    Menuitem(3, () => $"fullscreen:{(OptionsFile.Gen_Fullscreen ? "on" : "off")}", () => Fullscreen());

                    void Back()
                    {
                        if (Btnp(4) || Btnp(5))
                        {
                            menuSelected = 0;
                            menuItems.Clear();
                            Menuitem(0, () => "continue", () => Continue());
                            Menuitem(1, () => "options", () => Options());
                            Menuitem(2, () => "reset cart", () => ResetCart());
                            Menuitem(3, () => "exit", () => Exit());
                        }
                    }
                    Menuitem(4, () => "back", () => Back());

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
                        Menuitem(3, () => $"sfx:{_sfx.ElementAt(OptionsFile.Pcraft_Sfx_Pack).Key}", () => Sfx());
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
                        Menuitem(3, () => $"music:{_music.ElementAt(OptionsFile.Pcraft_Soundtrack).Key}", () => Soundtrack());
                    }
                }
            }
            Menuitem(1, () => "options", () => Options());

            void ResetCart()
            {
                if (Btnp(4) || Btnp(5))
                {
                    LoadCart(_cart);
                }
            }
            Menuitem(2, () => "reset cart", () => ResetCart());

            void Exit()
            {
                if (Btnp(4) || Btnp(5))
                {
                    LoadCart(new TitleScreen(false));
                }
            }
            Menuitem(3, () => "exit", () => Exit());

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
            if (!(_cart.SceneName == "TitleScreen") && Btnp(6))
            {
                isPaused = !isPaused;
                menuSelected = 0;
            }
            prev6 = Btn(6);

            if (isPaused)
            {
                for (int i = 0; i <= 6; i++)
                {
                    if (Ptn(i)) { lockout[i] = true; } else { lockout[i] = false; }
                }

                if (Btnp(0) || Btnp(1) || Btnp(4) || Btnp(5)) { menuItems[menuSelected].Function(); }

                if (Btnp(2)) { menuSelected -= 1; }
                if (Btnp(3)) { menuSelected += 1; }
                menuSelected = GeneralFunctions.Loop(menuSelected, menuItems);

                foreach (List<(string name, SoundEffectInstance track, bool loop, int group)> song in channelMusic)
                {
                    song[curTrack].track.Pause();
                }
                foreach (List<SoundEffectInstance> channel in new List<List<SoundEffectInstance>>([channel0, channel1, channel2, channel3]))
                {
                    foreach (SoundEffectInstance sfx in channel)
                    {
                        sfx.Pause();
                    }
                }
            }
            else
            {
                for (int i = 0; i <= 6; i++)
                {
                    if (!Ptn(i)) { lockout[i] = false; }
                }

                foreach (List<(string name, SoundEffectInstance track, bool loop, int group)> song in channelMusic)
                {
                    if (song[curTrack].track.State == SoundState.Paused) { song[curTrack].track.Play(); }
                }
                foreach (List<SoundEffectInstance> channel in new List<List<SoundEffectInstance>>([channel0, channel1, channel2, channel3]))
                {
                    foreach (SoundEffectInstance sfx in channel)
                    {
                        if (sfx.State == SoundState.Paused) { sfx.Play(); }
                    }
                }

                _cart.Update();
            }
            
            prev0 = Btn(0);
            prev1 = Btn(1);
            prev2 = Btn(2);
            prev3 = Btn(3);
            prev4 = Btn(4);
            prev5 = Btn(5);

            heldCount0 = prev0 ? heldCount0 + 1 : 0;
            heldCount1 = prev1 ? heldCount1 + 1 : 0;
            heldCount2 = prev2 ? heldCount2 + 1 : 0;
            heldCount3 = prev3 ? heldCount3 + 1 : 0;
            heldCount4 = prev4 ? heldCount4 + 1 : 0;
            heldCount5 = prev5 ? heldCount5 + 1 : 0;

            float fadeStep = OptionsFile.Gen_Music_Vol / 1600.0f;

            foreach (List<(string name, SoundEffectInstance track, bool loop, int group)> song in channelMusic)
            {
                if (song[curTrack].track.State == SoundState.Stopped)
                {
                    if (song.Count <= curTrack + 2)
                    {
                        curTrack += 1;
                        song[curTrack].track.IsLooped = song[curTrack].loop;
                        song[curTrack].track.Play();
                        if (OptionsFile.Gen_Sound_On) { song[curTrack].track.Volume = OptionsFile.Gen_Music_Vol / 100.0f; }
                    }
                }

                if (musicTransition.Item1 is null || musicTransition.Item2 is null)
                {
                    musicTransition = new();
                    if (OptionsFile.Gen_Sound_On && lastMusicCall is not null && song[curTrack].name == _music.ElementAt(curSoundtrack).Value[(int)lastMusicCall].tracks[curTrack].name)
                    {
                        song[curTrack].track.Volume = OptionsFile.Gen_Music_Vol / 100.0f;
                    }
                }
            }

            if (OptionsFile.Gen_Sound_On && musicTransition.Item1 is not null && musicTransition.Item2 is not null && musicTransition.Item1[curTrack].State == SoundState.Playing && musicTransition.Item2[curTrack].State == SoundState.Playing)
            {
                if (musicTransition.Item1[curTrack].Volume > 0.0f)
                {
                    musicTransition.Item1[curTrack].Volume -= fadeStep;
                }
                if (musicTransition.Item2[curTrack].Volume < OptionsFile.Gen_Music_Vol / 100.0f)
                {
                    musicTransition.Item2[curTrack].Volume += fadeStep;
                }
                if (musicTransition.Item1[curTrack].Volume <= 0.0f && musicTransition.Item2[curTrack].Volume >= OptionsFile.Gen_Music_Vol / 100.0f)
                {
                    musicTransition.Item1[curTrack].Volume = 0.0f;
                    musicTransition.Item2[curTrack].Volume = OptionsFile.Gen_Music_Vol / 100.0f;
                    musicTransition = new();
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
                int viewportWidth = GraphicsDevice.Viewport.Width;
                int viewportHeight = GraphicsDevice.Viewport.Height;

                // Calculate the size of each cell
                int cellW = viewportWidth / 128;
                int cellH = viewportHeight / 128;

                Vector2 size = new(cellW, cellH);

                int i = (int)Math.Floor(64 - (menuItems.Count / 2.0) * 8);

                int xborder = 23;
                Rectfill(0 + xborder, i - 7, 127 - xborder, i + menuItems.Count * 8 + 2, 0);
                Rectfill(0 + xborder + 1, i - 7 + 1, 127 - xborder - 1, i + menuItems.Count * 8 + 2 - 1, 7);
                Rectfill(0 + xborder + 2, i - 7 + 2, 127 - xborder - 2, i + menuItems.Count * 8 + 2 - 2, 0);

                Batch.Draw(TextureDictionary["PauseArrow"], new Vector2((xborder + 4) * cellW, (i - 1 + menuSelected * 8) * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

                for(int j = 0; j < menuItems.Count; j++)
                {
                    int indent = menuSelected == j ? 1 : 0;
                    Print(menuItems[j].GetName(), xborder + indent + 12, i, 7);
                    i += 8;
                }
            }
        }


        private static int[] DataToArray(string s, int n)
        {
            int[] val = new int[s.Length / n];
            for (int i = 0; i < s.Length / n; i++)
            {
                val[i] = Convert.ToInt32($"0x{s.Substring(i * n, n)}", 16);
            }
            
            return val;
        }

        private string MapFlip(string s)
        {
            return string.Concat(
                Enumerable.Range(0, (int)Math.Ceiling(s.Length / 2.0))
                    .Select(i => new string(s
                        .Skip(i * 2)
                        .Take(2)
                        .Reverse()
                        .ToArray()))
            );
        }

        private static Color HexToColor(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return new Color(r, g, b);
        }


        // pico-8 colors https://pico-8.fandom.com/wiki/Palette
        public Color[] colors =
        [
            HexToColor("000000"), // 00 black
            HexToColor("1D2B53"), // 01 dark-blue
            HexToColor("7E2553"), // 02 dark-purple
            HexToColor("008751"), // 03 dark-green
            HexToColor("AB5236"), // 04 brown
            HexToColor("5F574F"), // 05 dark-grey
            HexToColor("C2C3C7"), // 06 light-grey
            HexToColor("FFF1E8"), // 07 white
            HexToColor("FF004D"), // 08 red
            HexToColor("FFA300"), // 09 orange
            HexToColor("FFEC27"), // 10 yellow
            HexToColor("00E436"), // 11 green
            HexToColor("29ADFF"), // 12 blue
            HexToColor("83769C"), // 13 lavender
            HexToColor("FF77A8"), // 14 pink
            HexToColor("FFCCAA"), // 15 light-peach
            
            /*
            HexToColor("291814"), // 16 brownish-black
            HexToColor("111D35"), // 17 darker-blue
            HexToColor("422136"), // 18 darker-purple
            HexToColor("125359"), // 19 blue-green
            HexToColor("742F29"), // 20 dark-brown
            HexToColor("49333B"), // 21 darker-grey
            HexToColor("A28879"), // 22 medium-grey
            HexToColor("F3EF7D"), // 23 light-yellow
            HexToColor("BE1250"), // 24 dark-red
            HexToColor("FF6C24"), // 25 dark-orange
            HexToColor("A8E72E"), // 26 lime-green
            HexToColor("00B543"), // 27 medium-green
            HexToColor("065AB5"), // 28 true-blue
            HexToColor("754665"), // 29 mauve
            HexToColor("FF6E59"), // 30 dark-peach
            HexToColor("FF9D81"), // 31 peach
            */
        ];
        public int[] palColors = [-1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];


        private Texture2D CreateTextureFromSpriteData(int[] spriteData, int spriteX, int spriteY, int spriteWidth, int spriteHeight)
        {
            Texture2D texture = new(GraphicsDevice, spriteWidth, spriteHeight);

            Color[] colorData = new Color[spriteWidth * spriteHeight];

            for (int i = spriteX + spriteY * 128, j = 0; j < spriteWidth * spriteHeight; i++, j++)
            {
                int index = spriteData[i] % 16;
                int palSwapCol = palColors[index];
                if (palSwapCol != -1)
                {
                    Color color = colors[(int)palSwapCol];
                    colorData[j] = color;
                }

                if (j % spriteWidth == spriteWidth - 1) { i += 128 - spriteWidth; }
            }

            texture.SetData(colorData);

            return texture;
        }


        public Entity Add(List<Entity> table, Entity value, int index = -1) // https://pico-8.fandom.com/wiki/Add
        {
            if (index == -1) { table.Add(value); return value; }
            table.Insert(index, value);
            return value;
        }


        public Material Add(List<Material> table, Material value, int index = -1) // https://pico-8.fandom.com/wiki/Add
        {
            if (index == -1) { table.Add(value); return value; }
            table.Insert(index, value);
            return value;
        }


        private bool IsBindingDown(int device, string bind)
        {
            KeyboardState Kbm_state = Keyboard.GetState();
            GamePadState con_state = GamePad.GetState(PlayerIndex.One);

            if (device == 0)
            {
                if (Enum.TryParse(bind, out Keys key))
                {
                    return Kbm_state.IsKeyDown(key);
                }

                if (IsMouseButton(bind))
                {
                    return true;
                }
            }
            else
            {
                if (Enum.TryParse(bind, out Buttons button))
                {
                    return con_state.IsButtonDown(button);
                }
            }

            return false;
        }


        private bool IsMouseButton(string bind)
        {
            MouseState mouse_state = Mouse.GetState();

            switch (bind)
            {
                case "LeftButton":
                    return mouse_state.LeftButton == ButtonState.Pressed;
                case "RightButton":
                    return mouse_state.RightButton == ButtonState.Pressed;
                case "MiddleButton":
                    return mouse_state.MiddleButton == ButtonState.Pressed;
                case "XButton1":
                    return mouse_state.XButton1 == ButtonState.Pressed;
                case "XButton2":
                    return mouse_state.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
        }


        public bool Btn(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
        {
            return (isPaused || !lockout[i]) && Ptn(i);
        }


        private bool Ptn(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
        {
            switch (i)
            {
                case 0:
                    return IsBindingDown(0, OptionsFile.Kbm_Left.Bind1) ||
                        IsBindingDown(0, OptionsFile.Kbm_Left.Bind2) ||
                        IsBindingDown(1, OptionsFile.Con_Left.Bind1) ||
                        IsBindingDown(1, OptionsFile.Con_Left.Bind2);
                case 1:
                    return IsBindingDown(0, OptionsFile.Kbm_Right.Bind1) ||
                        IsBindingDown(0, OptionsFile.Kbm_Right.Bind2) ||
                        IsBindingDown(1, OptionsFile.Con_Right.Bind1) ||
                        IsBindingDown(1, OptionsFile.Con_Right.Bind2);
                case 2:
                    return IsBindingDown(0, OptionsFile.Kbm_Up.Bind1) ||
                        IsBindingDown(0, OptionsFile.Kbm_Up.Bind2) ||
                        IsBindingDown(1, OptionsFile.Con_Up.Bind1) ||
                        IsBindingDown(1, OptionsFile.Con_Up.Bind2);
                case 3:
                    return IsBindingDown(0, OptionsFile.Kbm_Down.Bind1) ||
                        IsBindingDown(0, OptionsFile.Kbm_Down.Bind2) ||
                        IsBindingDown(1, OptionsFile.Con_Down.Bind1) ||
                        IsBindingDown(1, OptionsFile.Con_Down.Bind2);
                case 4:
                    return IsBindingDown(0, OptionsFile.Kbm_Menu.Bind1) ||
                        IsBindingDown(0, OptionsFile.Kbm_Menu.Bind2) ||
                        IsBindingDown(1, OptionsFile.Con_Menu.Bind1) ||
                        IsBindingDown(1, OptionsFile.Con_Menu.Bind2);
                case 5:
                    return IsBindingDown(0, OptionsFile.Kbm_Use.Bind1) ||
                        IsBindingDown(0, OptionsFile.Kbm_Use.Bind2) ||
                        IsBindingDown(1, OptionsFile.Con_Use.Bind1) ||
                        IsBindingDown(1, OptionsFile.Con_Use.Bind2);
                case 6:
                    return IsBindingDown(0, OptionsFile.Kbm_Pause.Bind1) ||
                        IsBindingDown(0, OptionsFile.Kbm_Pause.Bind2) ||
                        IsBindingDown(1, OptionsFile.Con_Pause.Bind1) ||
                        IsBindingDown(1, OptionsFile.Con_Pause.Bind2);
                default:
                    return false;
            }
        }


        public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
        {
            int initDelay = 15;
            int repDelay = 4;
            if (i == 0 && Btn(i) && (!prev0 || heldCount0 == initDelay || heldCount0 > initDelay && (heldCount0 - initDelay) % repDelay == 0)) { return true; }
            if (i == 1 && Btn(i) && (!prev1 || heldCount1 == initDelay || heldCount1 > initDelay && (heldCount1 - initDelay) % repDelay == 0)) { return true; }
            if (i == 2 && Btn(i) && (!prev2 || heldCount2 == initDelay || heldCount2 > initDelay && (heldCount2 - initDelay) % repDelay == 0)) { return true; }
            if (i == 3 && Btn(i) && (!prev3 || heldCount3 == initDelay || heldCount3 > initDelay && (heldCount3 - initDelay) % repDelay == 0)) { return true; }
            if (i == 4 && Btn(i) && (!prev4 || heldCount4 == initDelay || heldCount4 > initDelay && (heldCount4 - initDelay) % repDelay == 0)) { return true; }
            if (i == 5 && Btn(i) && (!prev5 || heldCount5 == initDelay || heldCount5 > initDelay && (heldCount5 - initDelay) % repDelay == 0)) { return true; }
            if (i == 6 && Btn(i) && !prev6) { return true; }
            return false;
        }


        public void Camera() // https://pico-8.fandom.com/wiki/Camera
        {
            CameraOffset = (0, 0);
        }


        public void Camera(F32 x, F32 y) // https://pico-8.fandom.com/wiki/Camera
        {
            int xFlr = F32.FloorToInt(x);
            int yFlr = F32.FloorToInt(y);

            CameraOffset = (xFlr, yFlr);
        }


        public void CartData(string id)
        {

        }


        public void Circ(double x, double y, double r, int c) // https://pico-8.fandom.com/wiki/Circ
        {
            if (r < 0) return;

            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int rFlr = (int)Math.Floor(r);
            int drawCol = palColors[c] == -1 ? c : (int)palColors[c];

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;
            
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
                        Vector2 position = new((i - CameraOffset.Item1) * cellWidth, (j - CameraOffset.Item2) * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw the line
                        Batch.Draw(Pixel, position, null, colors[drawCol], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
            
        }


        public void Circ(F32 x, F32 y, double r, int c) // https://pico-8.fandom.com/wiki/Circ
        {
            if (r < 0) return;

            int xFlr = F32.FloorToInt(x);
            int yFlr = F32.FloorToInt(y);
            int rFlr = (int)Math.Floor(r);
            int drawCol = palColors[c] == -1 ? c : (int)palColors[c];

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

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
                        Vector2 position = new((i - CameraOffset.Item1) * cellWidth, (j - CameraOffset.Item2) * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw the line
                        Batch.Draw(Pixel, position, null, colors[drawCol], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }


        public void Circfill(double x, double y, double r, int c) // https://pico-8.fandom.com/wiki/Circfill
        {
            if (r < 0) return;

            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int rFlr = (int)Math.Floor(r);
            int drawCol = palColors[c] == -1 ? c : (int)palColors[c];

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

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
                        Vector2 position = new((i - CameraOffset.Item1) * cellWidth, (j - CameraOffset.Item2) * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw
                        Batch.Draw(Pixel, position, null, colors[drawCol], 0, Vector2.Zero, size, SpriteEffects.None, 0);
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
            int drawCol = palColors[c] == -1 ? c : (int)palColors[c];

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

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
                        Vector2 position = new((i - CameraOffset.Item1) * cellWidth, (j - CameraOffset.Item2) * cellHeight);
                        Vector2 size = new(cellWidth, cellHeight);

                        // Draw
                        Batch.Draw(Pixel, position, null, colors[drawCol], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }


        public void Cls(double color = 0) // https://pico-8.fandom.com/wiki/Cls
        {
            int colorFlr = (int)Math.Floor(color);

            int clearCol = palColors[colorFlr] == -1 ? colorFlr : (int)palColors[colorFlr];
            GraphicsDevice.Clear(colors[clearCol]);
        }


        public F32 Cos(F32 angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Cos
        {
            angle = Mod(angle, 1);
            F32 val = F32.FromRaw((int)(cosDict.LookupTable[angle.Raw / 10.0] * 10));
            return val;
        }


        public void Cstore()
        {

        }


        public void Del<T>(List<T> table, T value) // https://pico-8.fandom.com/wiki/Del
        {
            table.Remove(value);
        }


        public F32 Dget(int index) // https://pico-8.fandom.com/wiki/Dget
        {
            return F32.FromInt(index);
        }


        public void Dset(int index, double value) // https://pico-8.fandom.com/wiki/Dset
        {

        }


        public int Fget(int n) // https://pico-8.fandom.com/wiki/Fget
        {
            return _flags[n];
        }


        public void Load(string fileName)
        {

        }


        public void Map(double celx, double cely, double sx, double sy, double celw, double celh, int flags = 0) // https://pico-8.fandom.com/wiki/Map
        {
            int cxFlr = (int)Math.Floor(celx);
            int cyFlr = (int)Math.Floor(cely);
            int sxFlr = (int)Math.Floor(sx);
            int syFlr = (int)Math.Floor(sy);
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
                int[] secondHalf = DataToArray(MapFlip(_cart.MapData.Substring(0, 8192)), 1);
                secondHalf.CopyTo(_sprites, 8192);
            }
        }


        public void Menuitem(int pos, Func<string> getName, Action function)
        {
            menuItems.Insert(pos, new MenuItem(getName, function));
        }


        public int Mget(double celx, double cely) // https://pico-8.fandom.com/wiki/Mget
        {
            int xFlr = Math.Abs((int)Math.Floor(celx));
            int yFlr = Math.Abs((int)Math.Floor(cely));

            return _map[xFlr + yFlr * 128];
        }


        public F32 Mod(F32 x, int m)
        {
            F32 r = x % m;
            return r < 0 ? r + m : r;
        }


        public void Mset(double celx, double cely, double snum = 0) // https://pico-8.fandom.com/wiki/Mset
        {
            int xFlr = (int)Math.Floor(celx);
            int yFlr = (int)Math.Floor(cely);
            int sFlr = (int)Math.Floor(snum);

            _map[xFlr + yFlr * 128] = sFlr;
        }


        public void Music(int n, double fadems = 0) // https://pico-8.fandom.com/wiki/Music
        {
            lastMusicCall = n;
            (List<(string name, bool loop)> tracks, int group) curSong = _music.ElementAt(OptionsFile.Pcraft_Soundtrack).Value[n];

            if (channelMusic.Count > 0 && channelMusic[0][0].group == curSong.group)
            {
                foreach (List<(string name, SoundEffectInstance track, bool loop, int group)> item in channelMusic)
                {
                    if (item[0].name != curSong.tracks[0].name)
                    {
                        foreach ((string name, SoundEffectInstance track, bool loop, int group) sfxInst in item)
                        {
                            musicTransition.fromSong = [];
                            musicTransition.fromSong.Add(sfxInst.track);
                        }
                    }
                    else
                    {
                        foreach ((string name, SoundEffectInstance track, bool loop, int group) sfxInst in item)
                        {
                            musicTransition.toSong = [];
                            musicTransition.toSong.Add(sfxInst.track);
                        }
                    }
                }
            }
            else
            {
                SoundDispose();

                foreach ((List<(string name, bool loop)> tracks, int group) song in _music.ElementAt(OptionsFile.Pcraft_Soundtrack).Value)
                {
                    if (song.group == curSong.group)
                    {
                        List<(string name, SoundEffectInstance track, bool loop, int group)> listOfTracks = [];
                        foreach ((string name, bool loop) track in song.tracks)
                        {
                            listOfTracks.Add((track.name, MusicDictionary[track.name].CreateInstance(), track.loop, song.group));
                        }
                        channelMusic.Add(listOfTracks);
                    }
                }

                foreach (List<(string name, SoundEffectInstance track, bool loop, int group)> item in channelMusic)
                {
                    item[0].track.IsLooped = item[0].loop;
                    item[0].track.Play();
                    item[0].track.Volume = item[0].name == curSong.tracks[0].name ? OptionsFile.Gen_Sound_On ? OptionsFile.Gen_Music_Vol / 100.0f : 0 : 0;
                    curTrack = 0;
                }
            }
        }


        public void Mute()
        {
            foreach (List<(string name, SoundEffectInstance track, bool loop, int group)> song in channelMusic)
            {
                foreach ((string name, SoundEffectInstance track, bool loop, int group) track in song)
                {
                    track.track.Volume = 0.0f;
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
            palColors = [-1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        }


        public void Pal(double c0, double c1) // https://pico-8.fandom.com/wiki/Pal
        {
            int c0Flr = (int)Math.Floor(c0);
            int c1Flr = (int)Math.Floor(c1);

            palColors[c0Flr] = c1Flr;
        }


        public void Palt() // https://pico-8.fandom.com/wiki/Palt
        {
            palColors = [-1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        }


        public void Palt(double col, bool t) // https://pico-8.fandom.com/wiki/Palt
        {
            int colFlr = (int)Math.Floor(col);

            if (t)
            {
                palColors[colFlr] = -1;
            }
            else
            {
                palColors[colFlr] = colFlr;
            }
        }


        public void Print(string str, double x, double y, double c) // https://pico-8.fandom.com/wiki/Print
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int cFlr = (int)Math.Floor(c);

            int charWidth = 4;
            //int charHeight = 5;

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            for (int s = 0; s < str.Length; s++)
            {
                char letter = str[s];

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (Font.chars[letter][i, j] == 1)
                        {
                            int charStartX = (s * charWidth + xFlr + j - CameraOffset.Item1) * cellWidth;
                            //int charEndX = charStartX + cellWidth - CameraOffset.Item1;
                            int charStartY = (yFlr + i - CameraOffset.Item2) * cellHeight;

                            Vector2 position = new(charStartX, charStartY);
                            Vector2 size = new(cellWidth, cellHeight);

                            Batch.Draw(Pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                        }
                    }
                }
            }
        }


        public void Pset(F32 x, F32 y, double c) // https://pico-8.fandom.com/wiki/Pset
        {
            int xFlr = F32.FloorToInt(x);
            int yFlr = F32.FloorToInt(y);
            //float yFlr = (float)(Math.Floor(y) - 0.5);
            int cFlr = (int)Math.Floor(c);

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            // Calculate the position and size of the line
            Vector2 position = new((xFlr - CameraOffset.Item1) * cellWidth, (yFlr - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);

            // Draw the line
            Batch.Draw(Pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }


        public void Rectfill(double x1, double y1, double x2, double y2, double c) // https://pico-8.fandom.com/wiki/Rectfill
        {
            int x1Flr = (int)Math.Floor(Math.Min(x1, x2));
            int y1Flr = (int)Math.Floor(Math.Min(y1, y2));
            int x2Flr = (int)Math.Floor(Math.Max(x1, x2));
            int y2Flr = (int)Math.Floor(Math.Max(y1, y2));
            int cFlr = (int)Math.Floor(c);

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            int rectStartX = (x1Flr - CameraOffset.Item1) * cellWidth;
            int rectStartY = (y1Flr - CameraOffset.Item2) * cellHeight;

            int rectSizeX = (x2Flr - x1Flr + 1) * cellWidth;
            int rectSizeY = (y2Flr - y1Flr + 1) * cellHeight;

            //int rectEndX = (x2Flr - CameraOffset.Item1) * cellWidth;
            //int rectThickness = (y2Flr - y1Flr) * cellHeight;
            //batch.DrawLine(pixel, new Vector2(rectStartX, rectStartY), new Vector2(rectEndX, rectStartY), colors[cFlr], rectThickness);

            Vector2 position = new(rectStartX, rectStartY);
            Vector2 size = new(rectSizeX, rectSizeY);

            Batch.Draw(Pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }


        public void Reload(int i1 = 0, int i2 = 0, int i3 = 0, string s = "") // https://pico-8.fandom.com/wiki/Reload
        {
            Dispose();

            _sprites = DataToArray(_cart.SpriteData, 1);
            _flags = DataToArray(_cart.FlagData, 2);
            _map = DataToArray(_cart.MapData, 2);
        }


        public F32 Rnd(double limit = 1.0) // https://pico-8.fandom.com/wiki/Rnd
        {
            Random random = new();
            F32 n = F32.FromDouble(random.NextDouble() * limit);
            return n;
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


        public int Sget(double x, double y)
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);

            if (xFlr < 0 || yFlr < 0 || xFlr > 127 || yFlr > 127)
            {
                return 0;
            }

            return _sprites[xFlr + yFlr * 128];
        }


        public void Sset(double x, double y, double col)
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int colFlr = (int)Math.Floor(col);

            if (xFlr < 0 || yFlr < 0 || xFlr > 127 || yFlr > 127)
            {
                _sprites[xFlr + yFlr * 128] = colFlr;
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

            int[] cache = palColors;
            Array.Resize(ref cache, 17);
            cache[16] = spriteNumberFlr;
            if (!spriteTextures.TryGetValue(cache, out Texture2D? texture))
            {
                texture = CreateTextureFromSpriteData(_sprites, spriteX, spriteY, spriteWidth * wFlr, spriteHeight * hFlr);
                spriteTextures[cache] = texture;
            }

            // Get the size of the viewport
            int viewportWidth = Batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = Batch.GraphicsDevice.Viewport.Height;

            //Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(((flip_x ? xFlr + 2 * spriteWidth * wFlr - spriteWidth : xFlr + spriteWidth) - CameraOffset.Item1) * cellWidth, ((flip_y ? yFlr + 2 * spriteHeight * hFlr - spriteHeight : yFlr + spriteHeight) - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);
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

            int[] cache = palColors;
            Array.Resize(ref cache, 17);
            cache[16] = spriteNumberFlr;
            if (!spriteTextures.TryGetValue(cache, out Texture2D? texture))
            {
                texture = CreateTextureFromSpriteData(_sprites, sxFlr, syFlr, swFlr, shFlr);
                spriteTextures[cache] = texture;
            }

            // Get the size of the viewport
            int viewportWidth = Batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = Batch.GraphicsDevice.Viewport.Height;

            //Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(((flip_x ? dxFlr + 2 * spriteWidth * swFlr - spriteWidth : dxFlr + spriteWidth) - CameraOffset.Item1) * cellWidth, ((flip_y ? dyFlr + 2 * spriteHeight * shFlr - spriteHeight : dyFlr + spriteHeight) - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(dwFlr * cellWidth, dhFlr * cellHeight);
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
                foreach (List<(string name, SoundEffectInstance track, bool loop, int group)> songList in channelMusic)
                {
                    foreach ((string name, SoundEffectInstance track, bool loop, int group) song in songList)
                    {
                        song.track.Dispose();
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



    }
}