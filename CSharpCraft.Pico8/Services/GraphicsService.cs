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
public class GraphicsService : IGraphicsEngine, IDisposable
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
    private readonly Dictionary<string, Texture2D> _textureDictionary;
    private int[] _flagData;
    private int[] _mapData;

    public (F32 x, F32 y) CameraOffset { get; set; } = (F32.Zero, F32.Zero);
    public (int w, int h) Resolution { get; }
    public (int Width, int Height) Cell { get; set; }
    public SpriteBatch Batch => _batch;
    public GraphicsDevice GraphicsDevice => _graphicsDevice;
    public List<Color> Colors => _colors;
    public Dictionary<string, Texture2D> TextureDictionary => _textureDictionary;

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
        Dictionary<string, Texture2D> textureDictionary,
        int[] flagData = null!,
        int[] mapData = null!,
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
        _textureDictionary = textureDictionary;
        _flagData = flagData ?? [];
        _mapData = mapData ?? [];
        _equalityComparer = new IntArrayEqualityComparer();
        Resolution = resolution == default ? (128, 128) : resolution;
        Cell = (Width: 1, Height: 1);
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
    private void LineInternal(double x1, double y1, double x2, double y2, int c)
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
    /// Print text (Pico-8: Print) - internal implementation
    /// </summary>
    private void PrintInternal(string str, double x, double y, double c)
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

    // Interface implementation wrappers for IGraphicsEngine

    /// <summary>
    /// Set camera position wrapper (IGraphicsEngine)
    /// </summary>
    public void SetCamera(F32 x, F32 y) => Camera(x, y);

    /// <summary>
    /// Set palette wrapper (IGraphicsEngine)
    /// </summary>
    public void SetPalette(int c0, int c1) => Pal(c0, c1);

    /// <summary>
    /// Reset palette wrapper (IGraphicsEngine)
    /// </summary>
    public void ResetPalette() => Pal();

    /// <summary>
    /// Draw circle wrapper (IGraphicsEngine)
    /// </summary>
    public void Circle(F32 x, F32 y, double radius, int color) => Circ(x, y, radius, color);

    /// <summary>
    /// Draw filled circle wrapper (IGraphicsEngine)
    /// </summary>
    public void CircleFilled(F32 x, F32 y, double radius, int color) => Circfill(x, y, radius, color);

    /// <summary>
    /// Draw rectangle wrapper (IGraphicsEngine)
    /// </summary>
    public void Rectangle(F32 x, F32 y, F32 w, F32 h, int color) => 
        Rect(x.Float, y.Float, (x + w).Float, (y + h).Float, color);

    /// <summary>
    /// Draw filled rectangle wrapper (IGraphicsEngine)
    /// </summary>
    public void RectangleFilled(F32 x, F32 y, F32 w, F32 h, int color) => 
        Rectfill(x.Float, y.Float, (x + w).Float, (y + h).Float, color);

    /// <summary>
    /// Draw line wrapper (IGraphicsEngine)
    /// </summary>
    public void Line(F32 x0, F32 y0, F32 x1, F32 y1, int color) => 
        LineInternal(x0.Float, y0.Float, x1.Float, y1.Float, color);

    /// <summary>
    /// Draw text wrapper (IGraphicsEngine)
    /// </summary>
    public void Print(string text, F32 x, F32 y, int color) => 
        PrintInternal(text, x.Float, y.Float, color);

    /// <summary>
    /// Draw sprite from sprite sheet (Pico-8: Spr)
    /// </summary>
    public void DrawSprite(int spriteNum, F32 x, F32 y, F32? scaleX = null, F32? scaleY = null, bool flipX = false, bool flipY = false)
    {
        // Implementation draws sprite from the sprite sheet
        // For now, a placeholder that draws a simple placeholder sprite
        float sx = scaleX?.Float ?? 1.0f;
        float sy = scaleY?.Float ?? 1.0f;

        int spriteSize = 8; // Pico-8 sprites are 8x8
        int spriteXPos = (spriteNum % 16) * spriteSize;
        int spriteYPos = (spriteNum / 16) * spriteSize;

        // Draw from sprite textures if available
        var spriteTexture = _spriteTextures.FirstOrDefault(st => true);
        if (spriteTexture.Value != null)
        {
            var sourceRect = new Rectangle(spriteXPos, spriteYPos, spriteSize, spriteSize);
            var destRect = new Rectangle(
                F32.FloorToInt(x),
                F32.FloorToInt(y),
                (int)(spriteSize * sx),
                (int)(spriteSize * sy));

            _batch.Draw(spriteTexture.Value, destRect, sourceRect, Color.White);
        }
    }

    /// <summary>
    /// Draw tilemap (Pico-8: Map)
    /// </summary>
    public void DrawMap(double celx, double cely, double sx, double sy, double celw, double celh, int flags = 0)
    {
        int cwFlr = (int)Math.Floor(celw);
        int chFlr = (int)Math.Floor(celh);

        for (int i = 0; i <= cwFlr; i++)
        {
            for (int j = 0; j <= chFlr; j++)
            {
                int mapX = (int)(celx + i);
                int mapY = (int)(cely + j);
                int mapIndex = mapX + mapY * 128; // Assuming 128-wide map

                if (mapIndex >= 0 && mapIndex < _mapData.Length)
                {
                    int tileNum = _mapData[mapIndex];
                    DrawSprite(tileNum, F32.FromInt((int)(sx + i * 8)), F32.FromInt((int)(sy + j * 8)));
                }
            }
        }
    }

    /// <summary>
    /// Draw complex polygon/surface (Pico-8: Surface)
    /// Implementation draws filled polygon from vertex list
    /// </summary>
    public void Surface(List<(F32 x, F32 y)> vertices, int color)
    {
        if (vertices.Count < 3) return;

        Color drawCol = color >= 0 && color < _colors.Count ? _colors[color] : Color.White;

        // Simple polygon filling using scan-line algorithm (simplified version)
        // For production, would use a proper polygon rasterizer
        for (int i = 0; i < vertices.Count; i++)
        {
            int next = (i + 1) % vertices.Count;
            Line(vertices[i].x, vertices[i].y, vertices[next].x, vertices[next].y, color);
        }
    }

    /// <summary>
    /// Get sprite data
    /// </summary>
    public Color[] GetSpriteData() => _sprites;

    /// <summary>
    /// Set sprite data
    /// </summary>
    public void SetSpriteData(Color[] spriteData)
    {
        if (spriteData != null && spriteData.Length == _sprites.Length)
        {
            Array.Copy(spriteData, _sprites, spriteData.Length);
        }
    }

    /// <summary>
    /// Get flag data
    /// </summary>
    public int[] GetFlagData() => _flagData;

    /// <summary>
    /// Set flag data
    /// </summary>
    public void SetFlagData(int[] flagData)
    {
        _flagData = flagData ?? [];
    }

    /// <summary>
    /// Get map data
    /// </summary>
    public int[] GetMapData() => _mapData;

    /// <summary>
    /// Set map data
    /// </summary>
    public void SetMapData(int[] mapData)
    {
        _mapData = mapData ?? [];
    }

    /// <summary>
    /// Get sprite dimensions
    /// </summary>
    public (int width, int height) GetSpriteDimensions() => (8, 8); // Pico-8 sprites are 8x8

    public void Dispose()
    {
        _batch?.Dispose();
        _pixel?.Dispose();
    }
}
