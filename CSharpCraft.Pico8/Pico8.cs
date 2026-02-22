using System;
using System.Collections.Generic;
using System.Threading;
using FixMath;

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

        #region INPUT API

        /// <summary>
        /// Check if a button is currently pressed (PICO-8 btn)
        /// </summary>
        public static bool Btn(int button, int player = 0)
            => Orch.InputManager.Btn(button, player);

        /// <summary>
        /// Check if a button was pressed this frame (PICO-8 btnp)
        /// </summary>
        public static bool Btnp(int button, int player = 0)
            => Orch.InputManager.Btnp(button, player);

        #endregion

        #region GRAPHICS API

        /// <summary>
        /// Clear screen to specified color (PICO-8 cls)
        /// </summary>
        public static void Cls(int color = 0)
            => Orch.Graphics.Cls(color);

        /// <summary>
        /// Draw a circle outline (PICO-8 circ)
        /// </summary>
        public static void Circ(F32 x, F32 y, double radius, int color)
            => Orch.Graphics.Circ(x, y, radius, color);

        /// <summary>
        /// Draw a filled circle (PICO-8 circfill)
        /// </summary>
        public static void Circfill(F32 x, F32 y, double radius, int color)
            => Orch.Graphics.Circfill(x, y, radius, color);

        /// <summary>
        /// Draw a rectangle outline (PICO-8 rect)
        /// </summary>
        public static void Rect(double x1, double y1, double x2, double y2, double color)
            => Orch.Graphics.Rect(x1, y1, x2, y2, color);

        /// <summary>
        /// Draw a filled rectangle (PICO-8 rectfill)
        /// </summary>
        public static void Rectfill(double x1, double y1, double x2, double y2, double color)
            => Orch.Graphics.Rectfill(x1, y1, x2, y2, color);

        /// <summary>
        /// Draw text at position (PICO-8 print)
        /// </summary>
        public static void Print(string text, double x, double y, double c)
            => Orch.Graphics.Print(text, x, y, c);

        /// <summary>
        /// Draw sprite (PICO-8 spr)
        /// </summary>
        public static void Spr(double n, double x, double y, double w = 1.0, double h = 1.0, bool flip_x = false, bool flip_y = false)
            => Orch.Graphics.Spr(n, x, y, w, h, flip_x, flip_y);

        /// <summary>
        /// Draw stretched sprite region (PICO-8 sspr)
        /// </summary>
        public static void Sspr(double sx, double sy, double sw, double sh, double dx, double dy, double dw = -1, double dh = -1, bool flip_x = false, bool flip_y = false)
            => Orch.Graphics.Sspr(sx, sy, sw, sh, dx, dy, dw, dh, flip_x, flip_y);

        /// <summary>
        /// Draw tilemap region (PICO-8 map)
        /// </summary>
        public static void Map(double celx, double cely, double sx, double sy, double celw, double celh, int flags = 0)
            => Orch.Graphics.Map(celx, cely, sx, sy, celw, celh, flags);

        /// <summary>
        /// Draw a line (PICO-8 line)
        /// </summary>
        public static void Line(F32 x0, F32 y0, F32 x1, F32 y1, int c)
            => Orch.Graphics.Line(x0, y0, x1, y1, c);

        /// <summary>
        /// Set pixel (PICO-8 pset)
        /// </summary>
        public static void Pset(F32 x, F32 y, double c)
            => Orch.Graphics.Pset(x, y, c);

        /// <summary>
        /// Copy memory (PICO-8 memcpy)
        /// </summary>
        public static void Memcpy(int destaddr, int sourceaddr, int len)
            => Orch.Graphics.Memcpy(destaddr, sourceaddr, len);

        /// <summary>
        /// Reload cart data (PICO-8 reload)
        /// </summary>
        public static void Reload(int dest = 0, int source = 0, int len = 0, string filename = "")
            => Orch.Graphics.Reload(dest, source, len, filename);

        #endregion

        #region CAMERA API

        /// <summary>
        /// Reset camera offset to origin (PICO-8 camera with no args)
        /// </summary>
        public static void Camera()
            => Orch.Graphics.ResetCamera();

        /// <summary>
        /// Set camera offset position (PICO-8 camera)
        /// </summary>
        public static void Camera(F32 x, F32 y)
            => Orch.Graphics.SetCamera(x, y);

        #endregion

        #region PALETTE API

        /// <summary>
        /// Reset all palette remappings (PICO-8 pal with no args)
        /// </summary>
        public static void Pal()
            => Orch.Graphics.Pal();

        /// <summary>
        /// Set palette color remapping (PICO-8 pal)
        /// </summary>
        public static void Pal(int c0, int c1)
            => Orch.Graphics.Pal(c0, c1);

        /// <summary>
        /// Reset all transparency settings (PICO-8 palt with no args)
        /// </summary>
        public static void Palt()
            => Orch.Graphics.Palt();

        /// <summary>
        /// Set transparency for a palette color (PICO-8 palt)
        /// </summary>
        public static void Palt(int col, bool transparent)
            => Orch.Graphics.Palt(col, transparent);

        #endregion

        #region SCENE MANAGEMENT API

        /// <summary>
        /// Schedule a scene transition (PICO-8 load equivalent)
        /// </summary>
        public static void ScheduleScene(Func<IScene> sceneFactory)
            => Orch.SceneManager.ScheduleScene(sceneFactory);

        #endregion

        #region MAP DATA API

        /// <summary>
        /// Get map tile at cell position (PICO-8 mget)
        /// </summary>
        public static int Mget(double celx, double cely)
            => Orch.MapManager!.Mget(celx, cely);

        /// <summary>
        /// Set map tile at cell position (PICO-8 mset)
        /// </summary>
        public static void Mset(double celx, double cely, double snum = 0)
            => Orch.MapManager!.Mset(celx, cely, snum);

        /// <summary>
        /// Get sprite flag data (PICO-8 fget)
        /// </summary>
        public static int Fget(int n)
            => Orch.MapManager!.Fget(n);

        #endregion

        #region AUDIO API

        /// <summary>
        /// Play a sound effect (PICO-8 sfx)
        /// </summary>
        public static void Sfx(double n, double channel = -1.0, double offset = 0.0, double length = 31.0)
            => Orch.Audio.Sfx(n, channel, offset, length);

        /// <summary>
        /// Play background music (PICO-8 music)
        /// </summary>
        public static void Music(int n, double fadeMs = 0)
            => Orch.Audio.Music(n, fadeMs);

        /// <summary>
        /// Mute all audio (PICO-8 mute)
        /// </summary>
        public static void Mute()
            => Orch.Audio.Mute();

        #endregion

        #region DATA UTILITY API

        /// <summary>
        /// Add a value to a list (PICO-8 add)
        /// If index is -1, appends to end; otherwise inserts at index.
        /// </summary>
        public static T Add<T>(List<T> table, T value, int index = -1)
        {
            if (index == -1)
                table.Add(value);
            else
                table.Insert(index, value);
            return value;
        }

        /// <summary>
        /// Remove the first occurrence of a value from a list (PICO-8 del)
        /// </summary>
        public static void Del<T>(List<T> table, T value)
        {
            table.Remove(value);
        }

        #endregion

        #region MATH API

        /// <summary>
        /// Cosine function (angle in 0..1 range where 1 = full turn)
        /// </summary>
        public static float Cos(float angle)
        {
            return MathF.Cos(angle * MathF.PI * 2);
        }

        /// <summary>
        /// Sine function (angle in 0..1 range where 1 = full turn)
        /// Note: PICO-8 sin is inverted compared to standard math
        /// </summary>
        public static float Sin(float angle)
        {
            return -MathF.Sin(angle * MathF.PI * 2);
        }

        /// <summary>
        /// Get random value from 0 to max (PICO-8 rnd)
        /// </summary>
        public static float Rnd(float max = 1.0f)
            => (float)Rand.NextDouble() * max;

        /// <summary>
        /// Return the middle value of three numbers (PICO-8 mid)
        /// </summary>
        public static float Mid(float a, float b, float c)
        {
            if (a > b)
            {
                if (b > c) return b;
                if (a > c) return c;
                return a;
            }
            else
            {
                if (a > c) return a;
                if (b > c) return c;
                return b;
            }
        }

        /// <summary>
        /// Floor function - round down (PICO-8 flr)
        /// </summary>
        public static float Flr(float value)
        {
            return MathF.Floor(value);
        }

        /// <summary>
        /// Ceiling function - round up (PICO-8 ceil)
        /// </summary>
        public static float Ceil(float value)
        {
            return MathF.Ceiling(value);
        }

        /// <summary>
        /// Return absolute value (PICO-8 abs)
        /// </summary>
        public static float Abs(float value) => MathF.Abs(value);

        /// <summary>
        /// Return sign of value (-1, 0, or 1) (PICO-8 sgn)
        /// </summary>
        public static int Sgn(float value)
        {
            if (value > 0) return 1;
            if (value < 0) return -1;
            return 0;
        }

        /// <summary>
        /// Return minimum value (PICO-8 min)
        /// </summary>
        public static float Min(float a, float b) => MathF.Min(a, b);

        /// <summary>
        /// Return maximum value (PICO-8 max)
        /// </summary>
        public static float Max(float a, float b) => MathF.Max(a, b);

        /// <summary>
        /// PICO-8 modulus - always returns non-negative result
        /// </summary>
        public static F32 Mod(F32 x, int m)
        {
            F32 result = x % m;
            if (result < 0) result = result + m;
            return result;
        }

        /// <summary>
        /// Set the random seed for deterministic random sequences (PICO-8 srand)
        /// </summary>
        public static void Srand(int seed)
            => _random.Value = new Random(seed);

        #endregion

        #region STUB API

        /// <summary>
        /// Cart data persistence stub (PICO-8 cartdata)
        /// </summary>
        public static void CartData(string id)
        {
            // Stub: persistent cart data not yet implemented
        }

        /// <summary>
        /// Cart store stub (PICO-8 cstore)
        /// </summary>
        public static void Cstore()
        {
            // Stub: cart store not yet implemented
        }

        /// <summary>
        /// Load cart stub (PICO-8 load)
        /// </summary>
        public static void Load(string fileName)
        {
            // Stub: cart loading not yet implemented
        }

        /// <summary>
        /// Get persistent data value stub (PICO-8 dget)
        /// Returns index as F32 (matches existing behavior)
        /// </summary>
        public static F32 Dget(int index)
        {
            return F32.FromInt(index);
        }

        /// <summary>
        /// Set persistent data value stub (PICO-8 dset)
        /// </summary>
        public static void Dset(int index, double value)
        {
            // Stub: persistent data not yet implemented
        }

        #endregion

        #region HELPERS

        // Validation is handled by the Orch property accessor.
        // AsyncLocal provides execution-context isolation for thread safety.

        #endregion
    }
}
