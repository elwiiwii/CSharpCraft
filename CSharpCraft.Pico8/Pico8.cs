using System;
using System.Collections.Generic;
using System.Threading;
using FixMath;
using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8
{
    /// <summary>
    /// Pico8 - Static PICO-8 API Facade
    /// 
    /// Responsibility: Pure PICO-8 API delegation (no orchestration logic)
    /// - Provides familiar PICO-8 static API: Btn(), Circ(), Music(), etc.
    /// - Delegates all operations to GameOrchestrator
    /// - Zero state management, zero game logic
    /// - Uses AsyncLocal for thread-safe execution context isolation
    /// 
    /// SRP 5/5: This class has ONE reason to change: PICO-8 API evolution
    /// </summary>
    public static class Pico8
    {
        private static readonly AsyncLocal<GameOrchestrator?> _orchestrator = new();
        private static readonly AsyncLocal<Random?> _random = new();

        /// <summary>
        /// Validated orchestrator accessor. Throws if not initialized.
        /// </summary>
        private static GameOrchestrator Orch =>
            _orchestrator.Value ?? throw new InvalidOperationException(
                "Pico8 static API has not been initialized. Call Pico8.Initialize(orchestrator) first.");

        /// <summary>
        /// Context-local random instance. Lazily created if not set via Srand.
        /// </summary>
        private static Random Rand => _random.Value ??= new Random();

        /// <summary>
        /// Initialize the static Pico8 API with a GameOrchestrator instance.
        /// Must be called once during game initialization.
        /// </summary>
        public static void Initialize(GameOrchestrator orchestrator)
        {
            _orchestrator.Value = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            _random.Value = new Random();
        }

        public static List<Color> Palette => [
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

        #region INPUT API

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Btn
        /// </summary>
        public static bool Btn(int button, int player = 0)
            => Orch.InputManager.Btn(button, player);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Btnp
        /// </summary>
        public static bool Btnp(int button, int player = 0)
            => Orch.InputManager.Btnp(button, player);

        #endregion

        #region GRAPHICS API

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Cls
        /// </summary>
        public static void Cls(double color = 0)
            => Orch.GraphicsManager.Cls(Palette[(int)color]);

        public static void Cls(Color color)
            => Orch.GraphicsManager.Cls(color);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Circ
        /// </summary>
        public static void Circ(double x, double y, double radius, double color)
            => Orch.GraphicsManager.Circ((int)x, (int)y, (int)radius, Palette[(int)color]);

        public static void Circ(double x, double y, double radius, Color color)
            => Orch.GraphicsManager.Circ((int)x, (int)y, (int)radius, color);

        public static void Circ(F32 x, F32 y, F32 radius, F32 color)
            => Orch.GraphicsManager.Circ(F32.FloorToInt(x), F32.FloorToInt(y),
                                        F32.FloorToInt(radius), Palette[F32.FloorToInt(color)]);

        public static void Circ(F32 x, F32 y, F32 radius, Color color)
            => Orch.GraphicsManager.Circ(F32.FloorToInt(x), F32.FloorToInt(y),
                                        F32.FloorToInt(radius), color);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Circfill
        /// </summary>
        public static void Circfill(double x, double y, double radius, double color)
            => Orch.GraphicsManager.Circfill((int)x, (int)y, (int)radius, Palette[(int)color]);

        public static void Circfill(double x, double y, double radius, Color color)
            => Orch.GraphicsManager.Circfill((int)x, (int)y, (int)radius, color);

        public static void Circfill(F32 x, F32 y, F32 radius, F32 color)
            => Orch.GraphicsManager.Circfill(F32.FloorToInt(x), F32.FloorToInt(y),
                                            F32.FloorToInt(radius), Palette[F32.FloorToInt(color)]);

        public static void Circfill(F32 x, F32 y, F32 radius, Color color)
            => Orch.GraphicsManager.Circfill(F32.FloorToInt(x), F32.FloorToInt(y),
                                            F32.FloorToInt(radius), color);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Rect
        /// </summary>
        public static void Rect(double xLeft, double yTop, double xRight, double yBottom, double color)
            => Orch.GraphicsManager.Rect((int)xLeft, (int)yTop, (int)xRight, (int)yBottom, Palette[(int)color]);

        public static void Rect(double xLeft, double yTop, double xRight, double yBottom, Color color)
            => Orch.GraphicsManager.Rect((int)xLeft, (int)yTop, (int)xRight, (int)yBottom, color);

        public static void Rect(F32 xLeft, F32 yTop, F32 xRight, F32 yBottom, F32 color)
            => Orch.GraphicsManager.Rect(F32.FloorToInt(xLeft), F32.FloorToInt(yTop),
                                        F32.FloorToInt(xRight), F32.FloorToInt(yBottom),
                                        Palette[F32.FloorToInt(color)]);

        public static void Rect(F32 xLeft, F32 yTop, F32 xRight, F32 yBottom, Color color)
            => Orch.GraphicsManager.Rect(F32.FloorToInt(xLeft), F32.FloorToInt(yTop),
                                        F32.FloorToInt(xRight), F32.FloorToInt(yBottom),
                                        color);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Rectfill
        /// </summary>
        public static void Rectfill(double xLeft, double yTop, double xRight, double yBottom, double color)
            => Orch.GraphicsManager.Rectfill((int)xLeft, (int)yTop, (int)xRight, (int)yBottom, Palette[(int)color]);

        public static void Rectfill(double xLeft, double yTop, double xRight, double yBottom, Color color)
            => Orch.GraphicsManager.Rectfill((int)xLeft, (int)yTop, (int)xRight, (int)yBottom, color);

        public static void Rectfill(F32 xLeft, F32 yTop, F32 xRight, F32 yBottom, F32 color)
            => Orch.GraphicsManager.Rectfill(F32.FloorToInt(xLeft), F32.FloorToInt(yTop),
                                            F32.FloorToInt(xRight), F32.FloorToInt(yBottom),
                                            Palette[F32.FloorToInt(color)]);

        public static void Rectfill(F32 xLeft, F32 yTop, F32 xRight, F32 yBottom, Color color)
            => Orch.GraphicsManager.Rectfill(F32.FloorToInt(xLeft), F32.FloorToInt(yTop),
                                            F32.FloorToInt(xRight), F32.FloorToInt(yBottom),
                                            color);
        
        /// <summary>
        /// https://www.lexaloffle.com/bbs/?tid=150992
        /// </summary>
        public static void Rrect(double xLeft, double yTop, double xRight, double yBottom, double color)
            => Orch.GraphicsManager.Rect((int)xLeft, (int)yTop, (int)xRight, (int)yBottom, Palette[(int)color]);

        public static void Rrect(double xLeft, double yTop, double xRight, double yBottom, Color color)
            => Orch.GraphicsManager.Rect((int)xLeft, (int)yTop, (int)xRight, (int)yBottom, color);

        public static void Rrect(F32 xLeft, F32 yTop, F32 xRight, F32 yBottom, F32 color)
            => Orch.GraphicsManager.Rect(F32.FloorToInt(xLeft), F32.FloorToInt(yTop),
                                        F32.FloorToInt(xRight), F32.FloorToInt(yBottom),
                                        Palette[F32.FloorToInt(color)]);

        public static void Rrect(F32 xLeft, F32 yTop, F32 xRight, F32 yBottom, Color color)
            => Orch.GraphicsManager.Rect(F32.FloorToInt(xLeft), F32.FloorToInt(yTop),
                                        F32.FloorToInt(xRight), F32.FloorToInt(yBottom),
                                        color);

        /// <summary>
        /// https://www.lexaloffle.com/bbs/?tid=150992
        /// </summary>
        public static void Rrectfill(double xLeft, double yTop, double xRight, double yBottom, double radius, double color)
            => Orch.GraphicsManager.Rectfill((int)xLeft, (int)yTop, (int)xRight, (int)yBottom, (int)radius, Palette[(int)color]);

        public static void Rrectfill(double xLeft, double yTop, double xRight, double yBottom, double radius, Color color)
            => Orch.GraphicsManager.Rectfill((int)xLeft, (int)yTop, (int)xRight, (int)yBottom, (int)radius, color);

        public static void Rrectfill(F32 xLeft, F32 yTop, F32 xRight, F32 yBottom, F32 radius, F32 color)
            => Orch.GraphicsManager.Rectfill(F32.FloorToInt(xLeft), F32.FloorToInt(yTop),
                                            F32.FloorToInt(xRight), F32.FloorToInt(yBottom),
                                            F32.FloorToInt(radius), Palette[F32.FloorToInt(color)]);

        public static void Rrectfill(F32 xLeft, F32 yTop, F32 xRight, F32 yBottom, F32 radius, Color color)
            => Orch.GraphicsManager.Rectfill(F32.FloorToInt(xLeft), F32.FloorToInt(yTop),
                                            F32.FloorToInt(xRight), F32.FloorToInt(yBottom),
                                            F32.FloorToInt(radius), color);
        
        /// <summary>
        /// https://pico-8.fandom.com/wiki/Print
        /// </summary>
        public static void Print(string text, double x, double y, double color, string? texture = null)
            => Orch.GraphicsManager.Print(text, (int)x, (int)y, Palette[(int)color], texture);

        public static void Print(string text, double x, double y, Color color, string? texture = null)
            => Orch.GraphicsManager.Print(text, (int)x, (int)y, color, texture);

        public static void Print(string text, F32 x, F32 y, F32 color, string? texture = null)
            => Orch.GraphicsManager.Print(text, F32.FloorToInt(x), F32.FloorToInt(y), Palette[F32.FloorToInt(color)], texture);

        public static void Print(string text, F32 x, F32 y, Color color, string? texture = null)
            => Orch.GraphicsManager.Print(text, F32.FloorToInt(x), F32.FloorToInt(y), color, texture);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Spr
        /// </summary>
        public static void Spr(double index, double x = 0, double y = 0, double width = 1.0, double height = 1.0, bool flip_x = false, bool flip_y = false)
            => Orch.GraphicsManager.Spr((int)index, (int)x, (int)y, (int)width, (int)height, flip_x, flip_y);

        public static void Spr(F32 index, F32 x, F32 y, F32 width, F32 height, bool flip_x = false, bool flip_y = false)
            => Orch.GraphicsManager.Spr(F32.FloorToInt(index), F32.FloorToInt(x), F32.FloorToInt(y),
                                        F32.FloorToInt(width), F32.FloorToInt(height), flip_x, flip_y);
        
        /// <summary>
        /// https://pico-8.fandom.com/wiki/Sspr
        /// </summary>
        public static void Sspr(double selX, double selY, double selWidth, double selHeight, double drawX, double drawY, double drawWidth, double drawHeight, bool flip_x = false, bool flip_y = false)
            => Orch.GraphicsManager.Sspr((int)selX, (int)selY, (int)selWidth, (int)selHeight, (int)drawX, (int)drawY, (int)drawWidth, (int)drawHeight, flip_x, flip_y);

        public static void Sspr(F32 selX, F32 selY, F32 selWidth, F32 selHeight, F32 drawX, F32 drawY, F32 drawWidth, F32 drawHeight, bool flip_x = false, bool flip_y = false)
            => Orch.GraphicsManager.Sspr(F32.FloorToInt(selX), F32.FloorToInt(selY), F32.FloorToInt(selWidth), F32.FloorToInt(selHeight),
                                        F32.FloorToInt(drawX), F32.FloorToInt(drawY), F32.FloorToInt(drawWidth), F32.FloorToInt(drawHeight), flip_x, flip_y);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Map
        /// </summary>
        public static void Map(double cellX, double cellY, double screenX, double screenY, double cellWidth, double cellHeight, int flags = 0)
            => Orch.GraphicsManager.Map((int)cellX, (int)cellY, (int)screenX, (int)screenY, (int)cellWidth, (int)cellHeight, flags);
        
        public static void Map(F32 cellX, F32 cellY, F32 screenX, F32 screenY, F32 cellWidth, F32 cellHeight, int flags = 0)
            => Orch.GraphicsManager.Map(F32.FloorToInt(cellX), F32.FloorToInt(cellY), F32.FloorToInt(screenX), F32.FloorToInt(screenY),
                                        F32.FloorToInt(cellWidth), F32.FloorToInt(cellHeight), flags);
        
        /// <summary>
        /// https://pico-8.fandom.com/wiki/Line
        /// </summary>
        public static void Line(double xStart, double yStart, double xEnd, double yEnd, double color)
            => Orch.GraphicsManager.Line((int)xStart, (int)yStart, (int)xEnd, (int)yEnd, Palette[(int)color]);

        public static void Line(double xStart, double yStart, double xEnd, double yEnd, Color color)
            => Orch.GraphicsManager.Line((int)xStart, (int)yStart, (int)xEnd, (int)yEnd, color);

        public static void Line(F32 xStart, F32 yStart, F32 xEnd, F32 yEnd, F32 color)
            => Orch.GraphicsManager.Line(F32.FloorToInt(xStart), F32.FloorToInt(yStart),
                                        F32.FloorToInt(xEnd), F32.FloorToInt(yEnd),
                                        Palette[F32.FloorToInt(color)]);

        public static void Line(F32 xStart, F32 yStart, F32 xEnd, F32 yEnd, Color color)
            => Orch.GraphicsManager.Line(F32.FloorToInt(xStart), F32.FloorToInt(yStart),
                                        F32.FloorToInt(xEnd), F32.FloorToInt(yEnd),
                                        color);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Pset
        /// </summary>
        public static void Pset(double x, double y, double color)
            => Orch.GraphicsManager.Pset((int)x, (int)y, Palette[(int)color]);

        public static void Pset(double x, double y, Color color)
            => Orch.GraphicsManager.Pset((int)x, (int)y, color);

        public static void Pset(F32 x, F32 y, F32 color)
            => Orch.GraphicsManager.Pset(F32.FloorToInt(x), F32.FloorToInt(y), Palette[F32.FloorToInt(color)]);

        public static void Pset(F32 x, F32 y, Color color)
            => Orch.GraphicsManager.Pset(F32.FloorToInt(x), F32.FloorToInt(y), color);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Camera
        /// </summary>
        public static void Camera()
            => Orch.GraphicsManager.SetCamera(0, 0);

        public static void Camera(double x, double y)
            => Orch.GraphicsManager.SetCamera((int)x, (int)y);

        public static void Camera(F32 x, F32 y)
            => Orch.GraphicsManager.SetCamera(F32.FloorToInt(x), F32.FloorToInt(y));

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Pal
        /// </summary>
        public static void Pal()
            => Orch.GraphicsManager.Pal();

        public static void Pal(double index, double color)
            => Orch.GraphicsManager.Pal((int)index, Palette[(int)color]);

        public static void Pal(double index, Color color)
            => Orch.GraphicsManager.Pal((int)index, color);

        public static void Pal(F32 index, F32 color)
            => Orch.GraphicsManager.Pal(F32.FloorToInt(index), Palette[F32.FloorToInt(color)]);

        public static void Pal(F32 index, Color color)
            => Orch.GraphicsManager.Pal(F32.FloorToInt(index), color);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Palt
        /// </summary>
        public static void Palt()
            => Orch.GraphicsManager.Palt();

        public static void Palt(double index, bool isTransparent)
            => Orch.GraphicsManager.Palt((int)index, isTransparent);

        public static void Palt(double index, double opacity)
            => Orch.GraphicsManager.Palt((int)index, (int)opacity);

        public static void Palt(F32 index, bool isTransparent)
            => Orch.GraphicsManager.Palt(F32.FloorToInt(index), isTransparent);

        public static void Palt(F32 index, F32 opacity)
            => Orch.GraphicsManager.Palt(F32.FloorToInt(index), F32.FloorToInt(opacity));

        #endregion

        #region SCENE MANAGEMENT API

        /// <summary>
        /// Schedule a scene transition (PICO-8 load equivalent)
        /// </summary>
        public static void ScheduleScene(Func<IScene> sceneFactory)
            => Orch.SceneManager.ScheduleScene(sceneFactory);

        #endregion

        #region MEMORY API

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Mget
        /// </summary>
        public static int Mget(double celx, double cely)
            => Orch.MemoryManager.Mget(celx, cely);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Mset
        /// </summary>
        public static void Mset(double celx, double cely, double snum = 0)
            => Orch.MemoryManager.Mset(celx, cely, snum);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Fget
        /// </summary>
        public static int Fget(int n)
            => Orch.MemoryManager.Fget(n);

        /// <summary>
        /// Get reference to raw map data array.
        /// Used for bulk access (e.g., saving/loading seeds).
        /// </summary>
        public static int[] GetMapData()
            => Orch.MemoryManager.GetMapData();

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Memcpy
        /// </summary>
        public static void Memcpy(int destaddr, int sourceaddr, int len)
            => Orch.MemoryManager.Memcpy(destaddr, sourceaddr, len);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Reload
        /// </summary>
        public static void Reload(int dest = 0, int source = 0, int len = 0, string filename = "")
            => Orch.MemoryManager.Reload(dest, source, len, filename);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Cartdata
        /// </summary>
        public static void CartData(string id)
            => Orch.MemoryManager.CartData(id);
        

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Cstore
        /// </summary>
        public static void Cstore()
            => Orch.MemoryManager.Cstore();

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Load
        /// </summary>
        public static void Load(string fileName)
            => Orch.MemoryManager.Load(fileName);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Dget
        /// </summary>
        public static F32 Dget(int index)
            => Orch.MemoryManager.Dget(index);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Dset
        /// </summary>
        public static void Dset(int index, double value)
            => Orch.MemoryManager.Dset(index, value);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Menuitem
        /// </summary>
        public static void Menuitem(int index, Func<string> getName, Action? callback = null)
            => Orch.MemoryManager.Menuitem(index, getName, callback);

        #endregion

        #region AUDIO API

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Sfx
        /// </summary>
        public static void Sfx(double n, double channel = -1.0, double offset = 0.0, double length = 31.0)
            => Orch.AudioManager.Sfx(n, channel, offset, length);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Music
        /// </summary>
        public static void Music(int n, double fadeMs = 0)
            => Orch.AudioManager.Music(n, fadeMs);

        /// <summary>
        /// Mute all audio (PICO-8 mute)
        /// </summary>
        public static void Mute()
            => Orch.AudioManager.Mute();

        #endregion

        //#region DATA UTILITY API

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Add
        /// </summary>
        //public static T Add<T>(List<T> table, T value, int index = -1)
        //{
        //    if (index == -1)
        //        table.Add(value);
        //    else
        //        table.Insert(index, value);
        //    return value;
        //}

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Del
        /// </summary>
        //public static void Del<T>(List<T> table, T value)
        //{
        //    table.Remove(value);
        //}

        //#endregion

        #region MATH API

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Abs
        /// </summary>
        public static F32 Abs(double value)
            => Orch.MathManager.Abs(F32.FromDouble(value));

        public static F32 Abs(F32 value)
            => Orch.MathManager.Abs(value);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Ceil
        /// </summary>
        public static F32 Ceil(double value)
            => Orch.MathManager.Ceil(F32.FromDouble(value));

        public static F32 Ceil(F32 value)
            => Orch.MathManager.Ceil(value);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Cos
        /// </summary>
        public static F32 Cos(double angle)
            => Orch.MathManager.Cos(angle);

        public static F32 Cos(F32 angle)
            => Orch.MathManager.Cos(angle);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Flr
        /// </summary>
        public static F32 Flr(double value)
            => Orch.MathManager.Flr(F32.FromDouble(value));

        public static F32 Flr(F32 value)
            => Orch.MathManager.Flr(value);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Max
        /// </summary>
        public static F32 Max(double first, double second = 0)
            => Orch.MathManager.Max(F32.FromDouble(first), F32.FromDouble(second));

        public static F32 Max(F32 first, F32 second)
            => Orch.MathManager.Max(first, second);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Mid
        /// </summary>
        public static F32 Mid(double first, double second, double third)
            => Orch.MathManager.Mid(F32.FromDouble(first), F32.FromDouble(second), F32.FromDouble(third));

        public static F32 Mid(F32 first, F32 second, F32 third)
            => Orch.MathManager.Mid(first, second, third);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Min
        /// </summary>
        public static F32 Min(double first, double second = 0)
            => Orch.MathManager.Min(F32.FromDouble(first), F32.FromDouble(second));

        public static F32 Min(F32 first, F32 second)
            => Orch.MathManager.Min(first, second);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Lua
        /// </summary>
        public static F32 Mod(double first, double second)
            => Orch.MathManager.Mod(F32.FromDouble(first), F32.FromDouble(second));

        public static F32 Mod(F32 first, F32 second)
            => Orch.MathManager.Mod(first, second);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Rnd
        /// </summary>
        public static F32 Rnd()
            => Orch.MathManager.Rnd(F32.One, null);
    
        public static F32 Rnd(double max, Random? random = null)
            => Orch.MathManager.Rnd(F32.FromDouble(max), random);

        public static F32 Rnd(F32 max, Random? random = null)
            => Orch.MathManager.Rnd(max, random);

        public static F32 Rnd(double max, object reference)
            => Orch.MathManager.Rnd(F32.FromDouble(max), reference);

        public static F32 Rnd(F32 max, object reference)
            => Orch.MathManager.Rnd(max, reference);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Sgn
        /// </summary>
        public static F32 Sgn(double value)
            => Orch.MathManager.Sgn(F32.FromDouble(value));

        public static F32 Sgn(F32 value)
            => Orch.MathManager.Sgn(value);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Sin
        /// </summary>
        public static F32 Sin(double angle)
            => Orch.MathManager.Sin(angle);

        public static F32 Sin(F32 angle)
            => Orch.MathManager.Sin(angle);

        /// <summary>
        /// https://pico-8.fandom.com/wiki/Srand
        /// </summary>
        public static void Srand(double seed, Random? random = null)
            => Orch.MathManager.Srand(F32.FromDouble(seed), random);

        public static void Srand(F32 seed, Random? random = null)
            => Orch.MathManager.Srand(seed, random);

        public static void Srand(double seed, object reference)
            => Orch.MathManager.Srand(F32.FromDouble(seed), reference);

        public static void Srand(F32 seed, object reference)
            => Orch.MathManager.Srand(seed, reference);

        #endregion

        #region API EXTENSIONS

        /// <summary>
        /// Draw a scaled pixel (1x1 texture) at position with color and scale.
        /// Used for debug overlays and visualizations.
        /// </summary>
        public static void DrawPixelScaled(Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, int effects, float layerDepth)
            => Orch.APIExtensions.DrawPixelScaled(position, color, rotation, origin, scale, effects, layerDepth);

        //#endregion

        //#region HELPERS

        // Validation is handled by the Orch property accessor.
        // AsyncLocal provides execution-context isolation for thread safety.

        //#endregion

        //#region GAME LOOP API

        /// <summary>
        /// Load a new cart/scene.
        /// </summary>
        //public static void LoadCart(IScene cart)
        //    => Orch.LoadCart(cart);

        /// <summary>
        /// Reload the current cart.
        /// </summary>
        //public static void ReloadCart()
        //    => Orch.ReloadCart();

        /// <summary>
        /// Dispose audio resources.
        /// </summary>
        //public static void SoundDispose()
        //    => Orch.SoundDispose();

        /// <summary>
        /// Update the game loop (called once per frame).
        /// </summary>
        //public static void Update()
        //    => Orch.Update();

        /// <summary>
        /// Draw the current frame (called once per frame).
        /// </summary>
        //public static void Draw()
        //    => Orch.Draw();

        /// <summary>
        /// Update the viewport after window resize or fullscreen toggle.
        /// </summary>
        //public static void UpdateViewport()
        //    => Orch.UpdateViewport();

        /// <summary>
        /// Dispose all resources.
        /// </summary>
        //public static void Dispose()
        //    => Orch.Dispose();

        //#endregion
    }
}
