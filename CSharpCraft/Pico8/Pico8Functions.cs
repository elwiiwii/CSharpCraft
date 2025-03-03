using CSharpCraft.OptionsMenu;
using CSharpCraft.Pcraft;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using FixMath;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Media;
using System.Reflection.Metadata.Ecma335;
using System.Drawing;

namespace CSharpCraft.Pico8
{
    public class Pico8Functions : IDisposable
    {
        public SpriteBatch batch { get; }
        public GraphicsDevice graphicsDevice { get; }
        public Dictionary<string, SoundEffect> musicDictionary { get; }
        public OptionsFile optionsFile { get; }
        public Texture2D pixel { get; }
        public List<IScene> scenes { get; }
        public Dictionary<string, SoundEffect> soundEffectDictionary { get; }
        public Dictionary<string, Texture2D> textureDictionary { get; }

        private int[] _flags;
        private int[] _map;
        private int[] _sprites;
        private (int, int) CameraOffset = (0, 0);
        public IScene _cart;
        private List<SoundEffectInstance>? channelMusic = [];
        private List<SoundEffectInstance>? channel0 = [];
        private List<SoundEffectInstance>? channel1 = [];
        private List<SoundEffectInstance>? channel2 = [];
        private List<SoundEffectInstance>? channel3 = [];
        private bool fade1;
        private bool fade4;
        private bool prev0;
        private bool prev1;
        private bool prev2;
        private bool prev3;
        private bool prev4;
        private bool prev5;
        private bool prev6;
        private bool isPaused;
        private Dictionary<int, Texture2D> spriteTextures = [];
        private List<(string name, Action function)> menuItems;
        private int menuSelected;

        public Pico8Functions(IScene cart, List<IScene> _scenes, Dictionary<string, Texture2D> _textureDictionary, Dictionary<string, SoundEffect> _soundEffectDictionary, Dictionary<string, SoundEffect> _musicDictionary, Texture2D _pixel, SpriteBatch _batch, GraphicsDevice _graphicsDevice, OptionsFile _optionsFile)
        {
            batch = _batch;
            graphicsDevice = _graphicsDevice;
            musicDictionary = _musicDictionary;
            optionsFile = _optionsFile;
            pixel = _pixel;
            scenes = _scenes;
            soundEffectDictionary = _soundEffectDictionary;
            textureDictionary = _textureDictionary;

            prev0 = false;
            prev1 = false;
            prev2 = false;
            prev3 = false;
            prev4 = false;
            prev5 = false;
            prev6 = false;
            isPaused = false;

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
            menuItems = [];

            _cart = cart;
            
            SoundDispose();

            SoundEffectInstance instance1 = musicDictionary[$"music_1"].CreateInstance();
            SoundEffectInstance instance4 = musicDictionary[$"music_4"].CreateInstance();
            channelMusic.Add(instance1);
            channelMusic.Add(instance4);
            instance1.IsLooped = true;
            instance4.IsLooped = true;
            instance1.Play();
            instance4.Play();
            instance1.Volume = 0.0f;
            instance4.Volume = 0.0f;
            fade1 = false;
            fade4 = false;

            void Continue()
            {
                isPaused = false;
            }
            Menuitem(0, "continue", () => Continue());

            void Options()
            {
                
            }
            Menuitem(1, "options", () => Options());

            void ResetCart()
            {
                LoadCart(_cart);
            }
            Menuitem(2, "reset cart", () => ResetCart());

            void Exit()
            {
                LoadCart(new TitleScreen());
            }
            Menuitem(3, "exit", () => Exit());

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
                if (Btnp(5) || Btnp(4)) { menuItems[menuSelected].function(); }

                if (Btnp(2)) { menuSelected -= 1; }
                if (Btnp(3)) { menuSelected += 1; }
                menuSelected = GeneralFunctions.Loop(menuSelected, menuItems);
            }
            else
            {
                _cart.Update();
            }

            prev0 = Btn(0);
            prev1 = Btn(1);
            prev2 = Btn(2);
            prev3 = Btn(3);
            prev4 = Btn(4);
            prev5 = Btn(5);

            if (fade1)
            {
                if (channelMusic[0].Volume < 1.0f)
                {
                    channelMusic[0].Volume += 0.05f;
                }
                if (channelMusic[1].Volume > 0.0f)
                {
                    channelMusic[1].Volume -= 0.05f;
                }
                if (channelMusic[0].Volume >= 1.0f && channelMusic[1].Volume <= 0.0f)
                {
                    fade1 = false;
                }
            }
            else if (fade4)
            {
                if (channelMusic[1].Volume < 1.0f)
                {
                    channelMusic[1].Volume += 0.05f;
                }
                if (channelMusic[0].Volume > 0.0f)
                {
                    channelMusic[0].Volume -= 0.05f;
                }
                if (channelMusic[1].Volume >= 1.0f && channelMusic[0].Volume <= 0.0f)
                {
                    fade4 = false;
                }
            }
        }


        public void Draw()
        {
            Pal();
            Palt();

            if (isPaused)
            {
                int viewportWidth = graphicsDevice.Viewport.Width;
                int viewportHeight = graphicsDevice.Viewport.Height;

                // Calculate the size of each cell
                int cellW = viewportWidth / 128;
                int cellH = viewportHeight / 128;

                Vector2 size = new(cellW, cellH);

                int i = (int)Math.Floor(64 - (menuItems.Count / 2.0) * 8);

                int xborder = 23;
                Rectfill(0 + xborder, i - 7, 127 - xborder, i + menuItems.Count * 8 + 2, 0);
                Rectfill(0 + xborder + 1, i - 7 + 1, 127 - xborder - 1, i + menuItems.Count * 8 + 2 - 1, 7);
                Rectfill(0 + xborder + 2, i - 7 + 2, 127 - xborder - 2, i + menuItems.Count * 8 + 2 - 2, 0);

                batch.Draw(textureDictionary["PauseArrow"], new Vector2((xborder + 4) * cellW, (i - 1 + menuSelected * 8) * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

                for(int j = 0; j < menuItems.Count; j++)
                {
                    int indent = menuSelected == j ? 1 : 0;
                    Print(menuItems[j].name, xborder + indent + 12, i, 7);
                    i += 8;
                }
            }
            else
            {
                _cart.Draw();
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
        public int?[] palColors = [null, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];


        private Texture2D CreateTextureFromSpriteData(int[] spriteData, int spriteX, int spriteY, int spriteWidth, int spriteHeight)
        {
            Texture2D texture = new(graphicsDevice, spriteWidth, spriteHeight);

            Color[] colorData = new Color[spriteWidth * spriteHeight];

            for (int i = spriteX + spriteY * 128, j = 0; j < spriteWidth * spriteHeight; i++, j++)
            {
                int index = spriteData[i] % 16;
                int? palSwapCol = palColors[index];
                if (palSwapCol is not null)
                {
                    Color color = colors[(int)palSwapCol];
                    colorData[j] = color;
                }

                if (i % spriteWidth == spriteWidth - 1) { i += 128 - spriteWidth; }
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


        public bool Btn(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btn
        {
            KeyboardState keyb_state = Keyboard.GetState();
            GamePadState con_state = GamePad.GetState(PlayerIndex.One);

            switch (i)
            {
                case 0:
                    return keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Left.Bind1)) ||
                        keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Left.Bind2)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Left.Bind1)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Left.Bind2));
                case 1:
                    return keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Right.Bind1)) ||
                        keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Right.Bind2)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Right.Bind1)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Right.Bind2));
                case 2:
                    return keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Up.Bind1)) ||
                        keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Up.Bind2)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Up.Bind1)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Up.Bind2));
                case 3:
                    return keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Down.Bind1)) ||
                        keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Down.Bind2)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Down.Bind1)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Down.Bind2));
                case 4:
                    return keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Menu.Bind1)) ||
                        keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Menu.Bind2)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Menu.Bind1)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Menu.Bind2));
                case 5:
                    return keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Use.Bind1)) ||
                        keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Use.Bind2)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Use.Bind1)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Use.Bind2));
                case 6:
                    return keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Pause.Bind1)) ||
                        keyb_state.IsKeyDown((Keys)Enum.Parse(typeof(Keys), optionsFile.Keyb_Pause.Bind2)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Pause.Bind1)) ||
                        con_state.IsButtonDown((Buttons)Enum.Parse(typeof(Buttons), optionsFile.Con_Pause.Bind2));
                default:
                    return false;
            }
        }


        public bool Btnp(int i, int p = 0) // https://pico-8.fandom.com/wiki/Btnp
        {
            if (i == 0 && Btn(i) && !prev0) { return true; }
            if (i == 1 && Btn(i) && !prev1) { return true; }
            if (i == 2 && Btn(i) && !prev2) { return true; }
            if (i == 3 && Btn(i) && !prev3) { return true; }
            if (i == 4 && Btn(i) && !prev4) { return true; }
            if (i == 5 && Btn(i) && !prev5) { return true; }
            if (i == 6 && Btn(i) && !prev6) { return true; }
            else { return false; }
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


        public void Circ(double x, double y, double r, int c) // https://pico-8.fandom.com/wiki/Circ
        {
            if (r < 0) return;

            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            int rFlr = (int)Math.Floor(r);
            int drawCol = palColors[c] is null ? c : (int)palColors[c];

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

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
                        batch.Draw(pixel, position, null, colors[drawCol], 0, Vector2.Zero, size, SpriteEffects.None, 0);
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
            int drawCol = palColors[c] is null ? c : (int)palColors[c];

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

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
                        batch.Draw(pixel, position, null, colors[drawCol], 0, Vector2.Zero, size, SpriteEffects.None, 0);
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
            int drawCol = palColors[c] is null ? c : (int)palColors[c];

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

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
                        batch.Draw(pixel, position, null, colors[drawCol], 0, Vector2.Zero, size, SpriteEffects.None, 0);
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
            int drawCol = palColors[c] is null ? c : (int)palColors[c];

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

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
                        batch.Draw(pixel, position, null, colors[drawCol], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    }
                }
            }
        }


        public void Cls(double color = 0) // https://pico-8.fandom.com/wiki/Cls
        {
            int colorFlr = (int)Math.Floor(color);

            int clearCol = palColors[colorFlr] is null ? colorFlr : (int)palColors[colorFlr];
            graphicsDevice.Clear(colors[clearCol]);
        }


        public F32 Cos(F32 angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Cos
        {
            F32 modAngle = Mod(angle, 1);
            double radians = (modAngle * 2 * F32.Pi).Double;
            double val = Math.Cos(radians);
            return F32.FromDouble(val);
        }


        public void Del<T>(List<T> table, T value) // https://pico-8.fandom.com/wiki/Del
        {
            table.Remove(value);
        }


        public int Fget(int n) // https://pico-8.fandom.com/wiki/Fget
        {
            return _flags[n];
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


        public void Menuitem(int pos, string name, Action function)
        {
            menuItems.Insert(pos, (name, function));
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


        public void Music(double n, double fadems = 0, double channelmask = 0) // https://pico-8.fandom.com/wiki/Music
        {
            int nFlr = (int)Math.Floor(n);

            //if (channelMusic is not null)
            //{
            //    foreach (SoundEffect song in channelMusic)
            //    {
            //        //song.Dispose();
            //        song.Volume = 0.0f;
            //    }
            //}

            if (nFlr == 1)
            {
                //SoundEffectInstance instance = musicDictionary[$"music_{nFlr}"].CreateInstance();
                //channelMusic.Add(instance);
                //instance.Play();
                fade1 = true;
                fade4 = false;
            }
            else if (nFlr == 4)
            {
                //SoundEffectInstance instance = musicDictionary[$"music_{nFlr}"].CreateInstance();
                //channelMusic.Add(instance);
                //instance.Play();
                fade4 = true;
                fade1 = false;
            }
            else
            {
                return;
            }

        }


        public void Pal() // https://pico-8.fandom.com/wiki/Pal
        {
            palColors = [null, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        }


        public void Pal(double c0, double c1) // https://pico-8.fandom.com/wiki/Pal
        {
            int c0Flr = (int)Math.Floor(c0);
            int c1Flr = (int)Math.Floor(c1);

            palColors[c0Flr] = c1Flr;
        }


        public void Palt() // https://pico-8.fandom.com/wiki/Palt
        {
            palColors = [null, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
        }


        public void Palt(double col, bool t) // https://pico-8.fandom.com/wiki/Palt
        {
            int colFlr = (int)Math.Floor(col);

            if (t)
            {
                palColors[colFlr] = null;
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
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

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

                            batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                        }
                    }
                }
            }
        }


        public void Pset(double x, double y, double c) // https://pico-8.fandom.com/wiki/Pset
        {
            int xFlr = (int)Math.Floor(x);
            int yFlr = (int)Math.Floor(y);
            //float yFlr = (float)(Math.Floor(y) - 0.5);
            int cFlr = (int)Math.Floor(c);

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            // Calculate the position and size of the line
            Vector2 position = new((xFlr - CameraOffset.Item1) * cellWidth, (yFlr - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);

            // Draw the line
            batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }


        public void Rectfill(double x1, double y1, double x2, double y2, double c) // https://pico-8.fandom.com/wiki/Rectfill
        {
            int x1Flr = (int)Math.Floor(x1);
            int y1Flr = (int)Math.Floor(y1);
            int x2Flr = (int)Math.Floor(x2);
            int y2Flr = (int)Math.Floor(y2);
            int cFlr = (int)Math.Floor(c);

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

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

            batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }


        public void Reload() // https://pico-8.fandom.com/wiki/Reload
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

                SoundEffectInstance instance = soundEffectDictionary[$"sfx_{nFlr}"].CreateInstance();

                c.Add(instance);

                instance.Play();
            }
            else
            {
                return;
            }

        }


        public F32 Sin(F32 angle) // angle is in pico 8 turns https://pico-8.fandom.com/wiki/Sin
        {
            F32 modAngle = Mod(angle, 1);
            double radians = (modAngle * 2 * F32.Pi).Double;
            double val = -Math.Sin(radians);
            return F32.FromDouble(val);
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

            int colorCache = 0;

            for (int i = 0; i < palColors.Length; i++)
            {
                if (palColors[i] != i)
                {
                    for (int j = 0; j < palColors.Length; j++)
                    {
                        if (palColors[i] is null)
                        {
                            colorCache += i * -1000;
                            break;
                        }
                        else if (palColors[i] == j)
                        {
                            colorCache += (i * 100 + j) * 1000;
                            break;
                        }
                    }
                }
            }

            if (!spriteTextures.TryGetValue(spriteNumberFlr + colorCache, out Texture2D texture))
            {
                texture = CreateTextureFromSpriteData(_sprites, spriteX, spriteY, spriteWidth * wFlr, spriteHeight * hFlr);
                spriteTextures[spriteNumberFlr + colorCache] = texture;
            }

            // Get the size of the viewport
            int viewportWidth = batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = batch.GraphicsDevice.Viewport.Height;

            //Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(((flip_x ? xFlr + 2 * spriteWidth * wFlr - spriteWidth : xFlr + spriteWidth) - CameraOffset.Item1) * cellWidth, ((flip_y ? yFlr + 2 * spriteHeight * hFlr - spriteHeight : yFlr + spriteHeight) - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(cellWidth, cellHeight);
            SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flip_y ? SpriteEffects.FlipVertically : SpriteEffects.None);

            batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
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

            int colorCache = 0;

            for (int i = 0; i < palColors.Length; i++)
            {
                if (palColors[i] != i)
                {
                    for (int j = 0; j < palColors.Length; j++)
                    {
                        if (palColors[i] is null)
                        {
                            colorCache += i * -1000;
                            break;
                        }
                        else if (palColors[i] == j)
                        {
                            colorCache += (i * 100 + j) * 1000;
                            break;
                        }
                    }
                }
            }

            int spriteNumberFlr = sxFlr * 100 + syFlr * 100 + swFlr * 100 + shFlr * 100;

            if (!spriteTextures.TryGetValue(spriteNumberFlr + colorCache, out Texture2D texture))
            {
                texture = CreateTextureFromSpriteData(_sprites, sxFlr, syFlr, swFlr, shFlr);
                spriteTextures[spriteNumberFlr + colorCache] = texture;
            }

            // Get the size of the viewport
            int viewportWidth = batch.GraphicsDevice.Viewport.Width;
            int viewportHeight = batch.GraphicsDevice.Viewport.Height;

            //Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            Vector2 position = new(((flip_x ? dxFlr + 2 * spriteWidth * swFlr - spriteWidth : dxFlr + spriteWidth) - CameraOffset.Item1) * cellWidth, ((flip_y ? dyFlr + 2 * spriteHeight * shFlr - spriteHeight : dyFlr + spriteHeight) - CameraOffset.Item2) * cellHeight);
            Vector2 size = new(dwFlr * cellWidth, dhFlr * cellHeight);
            SpriteEffects effects = (flip_x ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (flip_y ? SpriteEffects.FlipVertically : SpriteEffects.None);

            batch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, size, effects, 0);
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
                foreach (SoundEffectInstance song in channelMusic)
                {
                    song.Dispose();
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