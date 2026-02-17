using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FixMath;
using System.Collections.Generic;
using System.Linq;

namespace CSharpCraft.Pico8.Services;

/// <summary>
/// Service responsible for all graphics rendering operations
/// Extracted from Pico8Functions to enable service-based architecture
/// Phase 3: Service Extraction - Part 1
/// </summary>
public class GraphicsService : IDisposable
{
    private readonly SpriteBatch _batch;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly List<Color> _colors;
    private readonly List<PalCol> _palColors;
    private readonly Color[] _sprites;
    private readonly Dictionary<int[], Texture2D> _spriteTextures;
    private readonly Texture2D _pixel;
    private readonly CosDict _cosDict;
    private readonly SinDict _sinDict;
    private readonly IntArrayEqualityComparer _equalityComparer;

    public (F32 x, F32 y) CameraOffset { get; set; } = (F32.Zero, F32.Zero);
    public (int w, int h) Resolution { get; }
    public (int w, int h) Cell { get; set; }

    public GraphicsService(
        SpriteBatch batch,
        GraphicsDevice graphicsDevice,
        List<Color> colors,
        List<PalCol> palColors,
        Color[] sprites,
        Dictionary<int[], Texture2D> spriteTextures,
        Texture2D pixel,
        CosDict cosDict,
        SinDict sinDict,
        (int w, int h) resolution = default)
    {
        _batch = batch;
        _graphicsDevice = graphicsDevice;
        _colors = colors;
        _palColors = palColors;
        _sprites = sprites;
        _spriteTextures = spriteTextures;
        _pixel = pixel;
        _cosDict = cosDict;
        _sinDict = sinDict;
        _equalityComparer = new IntArrayEqualityComparer();
        Resolution = resolution == default ? (128, 128) : resolution;
        Cell = (1, 1);
    }

    /// <summary>
    /// Clear the screen to a color (Pico-8: Cls)
    /// </summary>
    public void Cls(int col = 0)
    {
        Color clearColor = col >= 0 && col < _colors.Count ? _colors[col] : Color.Black;
        _graphicsDevice.Clear(clearColor);
    }

    /// <summary>
    /// Draw a circle outline (Pico-8: Circ)
    /// </summary>
    public void Circ(F32 x, F32 y, double r, int c)
    {
        if (r < 0) return;

        int xFlr = F32.FloorToInt(x);
        int yFlr = F32.FloorToInt(y);
        int rFlr = (int)Math.Floor(r);
        Color drawCol = _palColors.FindAll(pc => pc.C0 == _colors[c]).Count > 0 
            ? _palColors.First(pc => pc.C0 == _colors[c]).C1 
            : _colors[c];

        for (int i = xFlr - rFlr; i <= xFlr + rFlr; i++)
        {
            for (int j = yFlr - rFlr; j <= yFlr + rFlr; j++)
            {
                if ((i - xFlr) * (i - xFlr) + (j - yFlr) * (j - yFlr) >= rFlr * rFlr && (i - xFlr) * (i - xFlr) + (j - yFlr) * (j - yFlr) <= (rFlr + 1) * (rFlr + 1))
                {
                    Pset(F32.FromInt(i), F32.FromInt(j), c);
                }
            }
        }
    }

    /// <summary>
    /// Draw a filled circle (Pico-8: Circfill)
    /// </summary>
    public void Circfill(F32 x, F32 y, double r, int c)
    {
        if (r < 0) return;

        int xFlr = F32.FloorToInt(x);
        int yFlr = F32.FloorToInt(y);
        int rFlr = (int)Math.Ceiling(r);

        for (int i = xFlr - rFlr; i <= xFlr + rFlr; i++)
        {
            for (int j = yFlr - rFlr; j <= yFlr + rFlr; j++)
            {
                if ((i - xFlr) * (i - xFlr) + (j - yFlr) * (j - yFlr) <= rFlr * rFlr)
                {
                    Pset(F32.FromInt(i), F32.FromInt(j), c);
                }
            }
        }
    }

    /// <summary>
    /// Draw a line (Pico-8: Line)
    /// </summary>
    public void Line(double x1, double y1, double x2, double y2, int c)
    {
        double dx = Math.Abs(x2 - x1);
        double dy = Math.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        double err = dx - dy;

        double x = x1;
        double y = y1;

        while (true)
        {
            Pset(F32.FromInt((int)x), F32.FromInt((int)y), c);

            if (Math.Abs(x - x2) < 0.01 && Math.Abs(y - y2) < 0.01) break;

            double e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    /// <summary>
    /// Draw a filled rectangle (Pico-8: Rectfill)
    /// </summary>
    public void Rectfill(double x1, double y1, double x2, double y2, int c)
    {
        int xi1 = (int)Math.Min(x1, x2);
        int yi1 = (int)Math.Min(y1, y2);
        int xi2 = (int)Math.Max(x1, x2);
        int yi2 = (int)Math.Max(y1, y2);

        for (int i = xi1; i <= xi2; i++)
        {
            for (int j = yi1; j <= yi2; j++)
            {
                Pset(F32.FromInt(i), F32.FromInt(j), c);
            }
        }
    }

    /// <summary>
    /// Draw a filled rectangle with Color (Pico-8: Rectfill)
    /// </summary>
    public void Rectfill(double x1, double y1, double x2, double y2, Color c)
    {
        int xi1 = (int)Math.Min(x1, x2);
        int yi1 = (int)Math.Min(y1, y2);
        int xi2 = (int)Math.Max(x1, x2);
        int yi2 = (int)Math.Max(y1, y2);

        int width = xi2 - xi1 + 1;
        int height = yi2 - yi1 + 1;

        var pixelData = new Color[width * height];
        for (int i = 0; i < pixelData.Length; i++)
        {
            pixelData[i] = c;
        }

        var rect = new Rectangle(xi1, yi1, width, height);
        _batch.Draw(_pixel, rect, c);
    }

    /// <summary>
    /// Draw a rectangle outline (Pico-8: Rect)
    /// </summary>
    public void Rect(double x1, double y1, double x2, double y2, int c)
    {
        int xi1 = (int)Math.Min(x1, x2);
        int yi1 = (int)Math.Min(y1, y2);
        int xi2 = (int)Math.Max(x1, x2);
        int yi2 = (int)Math.Max(y1, y2);

        // Top
        for (int i = xi1; i <= xi2; i++) Pset(F32.FromInt(i), F32.FromInt(yi1), c);
        // Bottom
        for (int i = xi1; i <= xi2; i++) Pset(F32.FromInt(i), F32.FromInt(yi2), c);
        // Left
        for (int j = yi1; j <= yi2; j++) Pset(F32.FromInt(xi1), F32.FromInt(j), c);
        // Right
        for (int j = yi1; j <= yi2; j++) Pset(F32.FromInt(xi2), F32.FromInt(j), c);
    }

    /// <summary>
    /// Draw a rectangle outline with Color (Pico-8: Rect)
    /// </summary>
    public void Rect(double x1, double y1, double x2, double y2, Color c)
    {
        int xi1 = (int)Math.Min(x1, x2);
        int yi1 = (int)Math.Min(y1, y2);
        int xi2 = (int)Math.Max(x1, x2);
        int yi2 = (int)Math.Max(y1, y2);

        // Draw rectangle outline using pixel batch
        _batch.Draw(_pixel, new Rectangle(xi1, yi1, xi2 - xi1, 1), c);
        _batch.Draw(_pixel, new Rectangle(xi1, yi2, xi2 - xi1, 1), c);
        _batch.Draw(_pixel, new Rectangle(xi1, yi1, 1, yi2 - yi1), c);
        _batch.Draw(_pixel, new Rectangle(xi2, yi1, 1, yi2 - yi1), c);
    }

    /// <summary>
    /// Draw a pixel (Pico-8: Pset)
    /// </summary>
    public void Pset(F32 x, F32 y, double c)
    {
        int xi = F32.FloorToInt(x);
        int yi = F32.FloorToInt(y);

        if (c < 0 || c >= _colors.Count) return;
        if (xi < 0 || xi >= 128 || yi < 0 || yi >= 128) return;

        Color drawCol = _palColors.FindAll(pc => pc.C0 == _colors[(int)c]).Count > 0
            ? _palColors.First(pc => pc.C0 == _colors[(int)c]).C1
            : _colors[(int)c];

        _batch.Draw(_pixel, new Vector2(xi + CameraOffset.x.Float, yi + CameraOffset.y.Float), drawCol);
    }

    /// <summary>
    /// Print text (Pico-8: Print)
    /// </summary>
    public void Print(string str, double x, double y, double c)
    {
        int xi = (int)x;
        int yi = (int)y;

        foreach (char ch in str)
        {
            int charIndex = ch - ' ';
            if (charIndex >= 0 && charIndex < 96)
            {
                // Simple monospace text rendering
                Pset(F32.FromInt(xi), F32.FromInt(yi), c);
                xi += 4; // Character width
            }
        }
    }

    /// <summary>
    /// Set camera position (Pico-8: Camera)
    /// </summary>
    public void Camera(F32 x, F32 y)
    {
        CameraOffset = (x, y);
    }

    /// <summary>
    /// Reset camera to origin (Pico-8: Camera with no args)
    /// </summary>
    public void ResetCamera()
    {
        CameraOffset = (F32.Zero, F32.Zero);
    }

    /// <summary>
    /// Reset palette to default (Pico-8: Pal)
    /// </summary>
    public void Pal()
    {
        _palColors.Clear();
        _palColors.Add(new PalCol(Color.Black, Color.Black, true));
    }

    /// <summary>
    /// Map color from one to another (Pico-8: Pal)
    /// </summary>
    public void Pal(int c0, int c1)
    {
        if (c0 < 0 || c0 >= _colors.Count || c1 < 0 || c1 >= _colors.Count) return;

        Color fromColor = _colors[c0];
        Color toColor = _colors[c1];

        var existing = _palColors.FirstOrDefault(pc => pc.C0 == fromColor);
        if (existing != null)
        {
            _palColors.Remove(existing);
        }

        _palColors.Add(new PalCol(fromColor, toColor, true));
    }

    /// <summary>
    /// Map color with Color struct (Pico-8: Pal)
    /// </summary>
    public void Pal(Color c0, Color c1)
    {
        var existing = _palColors.FirstOrDefault(pc => pc.C0 == c0);
        if (existing != null)
        {
            _palColors.Remove(existing);
        }

        _palColors.Add(new PalCol(c0, c1, true));
    }

    /// <summary>
    /// Reset transparency to default (Pico-8: Palt)
    /// </summary>
    public void Palt()
    {
        foreach (var pal in _palColors)
        {
            pal.Trans = false;
        }
    }

    /// <summary>
    /// Set transparency for a color (Pico-8: Palt)
    /// </summary>
    public void Palt(int col, bool t)
    {
        if (col < 0 || col >= _colors.Count) return;

        var palEntry = _palColors.FirstOrDefault(pc => pc.C0 == _colors[col]);
        if (palEntry != null)
        {
            palEntry.Trans = t;
        }
    }

    /// <summary>
    /// Set transparency for a Color (Pico-8: Palt)
    /// </summary>
    public void Palt(Color col, bool t)
    {
        var palEntry = _palColors.FirstOrDefault(pc => pc.C0 == col);
        if (palEntry != null)
        {
            palEntry.Trans = t;
        }
    }

    public void Dispose()
    {
        _batch?.Dispose();
        _pixel?.Dispose();
    }
}
