using System;
using FixMath;
using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8
{
    /// <summary>
    /// GraphicsOrchestrator - Graphics coordination and state management
    /// 
    /// Responsibility: Coordinate graphics operations and track graphics state
    /// - Delegates drawing primitives to IGraphicsAPI
    /// - Tracks camera offset state
    /// - Manages display configuration (Cell size, Resolution)
    /// - Manages graphics-specific coordination (camera, palette)
    /// 
    /// SRP 5/5: This class has ONE reason to change: graphics coordination logic
    /// 
    /// Note: GameOrchestrator composes this for graphics operations.
    /// Pico8 static class delegates through GameOrchestrator.Graphics to here.
    /// </summary>
    public class GraphicsOrchestrator
    {
        private readonly IGraphicsAPI _graphicsAPI;
        private readonly IPaletteManager? _paletteManager;
        private (F32 x, F32 y) _cameraOffset;
        private (int Width, int Height) _cell;
        private (int w, int h) _resolution;

        /// <summary>
        /// Exposes the underlying IGraphicsAPI for testing and advanced access
        /// </summary>
        public IGraphicsAPI API => _graphicsAPI;

        /// <summary>
        /// Exposes the palette manager for testing and advanced access
        /// </summary>
        public IPaletteManager? PaletteManager => _paletteManager;

        /// <summary>
        /// Current camera offset position
        /// </summary>
        public (F32 x, F32 y) CameraOffset => _cameraOffset;

        /// <summary>
        /// Current cell/tile size (physical pixels per PICO-8 pixel)
        /// </summary>
        public (int Width, int Height) Cell => _cell;

        /// <summary>
        /// Current virtual resolution (PICO-8 canvas size)
        /// </summary>
        public (int w, int h) Resolution => _resolution;

        /// <summary>
        /// Initialize GraphicsOrchestrator with required IGraphicsAPI and optional palette
        /// </summary>
        public GraphicsOrchestrator(IGraphicsAPI graphicsAPI, IPaletteManager? paletteManager = null)
        {
            _graphicsAPI = graphicsAPI ?? throw new ArgumentNullException(nameof(graphicsAPI));
            _paletteManager = paletteManager;
            _cameraOffset = (F32.Zero, F32.Zero);
            _cell = (1, 1);
            _resolution = (128, 128);
        }

        #region DRAWING PRIMITIVES

        /// <summary>
        /// Draw a circle outline (delegates to IGraphicsAPI)
        /// </summary>
        public void Circ(F32 x, F32 y, double r, int c)
        {
            _graphicsAPI.Circ(x, y, r, c);
        }

        /// <summary>
        /// Draw a filled circle (delegates to IGraphicsAPI)
        /// </summary>
        public void Circfill(F32 x, F32 y, double r, int c)
        {
            _graphicsAPI.Circfill(x, y, r, c);
        }

        /// <summary>
        /// Draw a rectangle outline (delegates to IGraphicsAPI)
        /// </summary>
        public void Rect(double x1, double y1, double x2, double y2, double c)
        {
            _graphicsAPI.Rect(x1, y1, x2, y2, c);
        }

        /// <summary>
        /// Draw a filled rectangle (delegates to IGraphicsAPI)
        /// </summary>
        public void Rectfill(double x1, double y1, double x2, double y2, double c)
        {
            _graphicsAPI.Rectfill(x1, y1, x2, y2, c);
        }

        /// <summary>
        /// Clear screen to specified color (delegates to IGraphicsAPI)
        /// </summary>
        public void Cls(int col = 0)
        {
            _graphicsAPI.Cls(col);
        }

        /// <summary>
        /// Draw a single pixel (delegates to IGraphicsAPI)
        /// </summary>
        public void Pset(F32 x, F32 y, double c)
        {
            _graphicsAPI.Pset(x, y, c);
        }

        #endregion

        #region PALETTE OPERATIONS

        /// <summary>
        /// Reset all palette remappings to defaults (PICO-8 pal with no args)
        /// </summary>
        public void Pal()
        {
            _paletteManager?.ResetPalette();
        }

        /// <summary>
        /// Set palette color remapping (PICO-8 pal)
        /// </summary>
        public void Pal(int c0, int c1)
        {
            _paletteManager?.SetPalette(c0, c1);
        }

        /// <summary>
        /// Reset all transparency settings (PICO-8 palt with no args)
        /// </summary>
        public void Palt()
        {
            _paletteManager?.ResetTransparency();
        }

        /// <summary>
        /// Set transparency for a palette color (PICO-8 palt)
        /// </summary>
        public void Palt(int col, bool transparent)
        {
            _paletteManager?.SetTransparency(col, transparent);
        }

        #endregion

        #region CAMERA STATE

        /// <summary>
        /// Set camera offset position
        /// </summary>
        public void SetCamera(F32 x, F32 y)
        {
            _cameraOffset = (x, y);
        }

        /// <summary>
        /// Reset camera offset to origin (0, 0)
        /// </summary>
        public void ResetCamera()
        {
            _cameraOffset = (F32.Zero, F32.Zero);
        }

        #endregion

        #region DISPLAY CONFIG

        /// <summary>
        /// Set display configuration (virtual resolution and cell size).
        /// Called when a scene is loaded or display settings change.
        /// </summary>
        public void SetDisplayConfig((int w, int h) resolution, (int Width, int Height) cell)
        {
            _resolution = resolution;
            _cell = cell;
        }

        /// <summary>
        /// Get palette color by PICO-8 index (delegates to IGraphicsAPI)
        /// </summary>
        public Color GetColor(int index)
        {
            return _graphicsAPI.GetColor(index);
        }

        #endregion

        #region RENDERING OPERATIONS

        /// <summary>
        /// Draw text at position (PICO-8 print)
        /// </summary>
        public void Print(string text, double x, double y, double c)
        {
            _graphicsAPI.Print(text, x, y, c);
        }

        /// <summary>
        /// Draw sprite (PICO-8 spr)
        /// </summary>
        public void Spr(double n, double x, double y, double w = 1.0, double h = 1.0, bool flip_x = false, bool flip_y = false)
        {
            _graphicsAPI.Spr(n, x, y, w, h, flip_x, flip_y);
        }

        /// <summary>
        /// Draw stretched sprite region (PICO-8 sspr)
        /// </summary>
        public void Sspr(double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh, bool flip_x = false, bool flip_y = false)
        {
            _graphicsAPI.Sspr(sx, sy, sw, sh, dx, dy, dw, dh, flip_x, flip_y);
        }

        /// <summary>
        /// Draw tilemap region (PICO-8 map)
        /// </summary>
        public void Map(double celx, double cely, double sx, double sy, double celw, double celh, int flags = 0)
        {
            _graphicsAPI.Map(celx, cely, sx, sy, celw, celh, flags);
        }

        /// <summary>
        /// Draw a line (PICO-8 line)
        /// </summary>
        public void Line(F32 x0, F32 y0, F32 x1, F32 y1, int c)
        {
            _graphicsAPI.Line(x0, y0, x1, y1, c);
        }

        /// <summary>
        /// Copy memory (PICO-8 memcpy)
        /// </summary>
        public void Memcpy(int destaddr, int sourceaddr, int len)
        {
            _graphicsAPI.Memcpy(destaddr, sourceaddr, len);
        }

        /// <summary>
        /// Reload cart data (PICO-8 reload)
        /// </summary>
        public void Reload(int dest = 0, int source = 0, int len = 0, string filename = "")
        {
            _graphicsAPI.Reload(dest, source, len, filename);
        }

        #endregion
    }
}
