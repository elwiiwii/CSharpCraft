using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FixMath;

namespace CSharpCraft.Tests.Pico8.Mocks;

/// <summary>
/// Mock graphics engine that tracks drawing operations for testing
/// Does not perform actual rendering - only records operations
/// </summary>
public class MockGraphicsEngine : IGraphicsEngine
{
    public SpriteBatch Batch { get; }
    public GraphicsDevice GraphicsDevice { get; }
    public (F32 x, F32 y) CameraOffset { get; private set; } = (F32.Zero, F32.Zero);
    public (int Width, int Height) Cell { get; } = (1, 1);
    public List<Color> Colors { get; } = new();
    public Dictionary<string, Texture2D> TextureDictionary { get; } = new();

    // Track drawing operations for verification
    public List<(string operation, float x, float y, object? param)> DrawCalls { get; } = new();

    public MockGraphicsEngine()
    {
        // Create null-coalesced dummy implementations - not used for rendering tests
        Batch = null!;
        GraphicsDevice = null!;
    }

    public Color[] GetSpriteData() => Array.Empty<Color>();
    public int[] GetFlagData() => Array.Empty<int>();
    public int[] GetMapData() => Array.Empty<int>();

    public void SetCamera(F32 x, F32 y) => CameraOffset = (x, y);
    public void ResetCamera() => CameraOffset = (F32.Zero, F32.Zero);

    public void Cls(int color = 0)
    {
        DrawCalls.Add(("Cls", 0, 0, color));
    }

    public void Circle(F32 x, F32 y, double radius, int color)
    {
        DrawCalls.Add(("Circle", x.Float, y.Float, (radius, color)));
    }

    public void CircleFilled(F32 x, F32 y, double radius, int color)
    {
        DrawCalls.Add(("CircleFilled", x.Float, y.Float, (radius, color)));
    }

    public void Rectangle(F32 x, F32 y, F32 w, F32 h, int color)
    {
        DrawCalls.Add(("Rectangle", x.Float, y.Float, (w.Float, h.Float, color)));
    }

    public void RectangleFilled(F32 x, F32 y, F32 w, F32 h, int color)
    {
        DrawCalls.Add(("RectangleFilled", x.Float, y.Float, (w.Float, h.Float, color)));
    }

    public void Line(F32 x0, F32 y0, F32 x1, F32 y1, int color)
    {
        DrawCalls.Add(("Line", x0.Float, y0.Float, (x1.Float, y1.Float, color)));
    }

    public void DrawSprite(int spriteNum, F32 x, F32 y, F32? scaleX = null, F32? scaleY = null, bool flipX = false, bool flipY = false)
    {
        DrawCalls.Add(("DrawSprite", x.Float, y.Float, spriteNum));
    }

    public void DrawMap(double celx, double cely, double sx, double sy, double celw, double celh, int flags = 0)
    {
        DrawCalls.Add(("DrawMap", (float)celx, (float)cely, flags));
    }

    public void Print(string text, F32 x, F32 y, int color)
    {
        DrawCalls.Add(("Print", x.Float, y.Float, (text, color)));
    }

    public void SetPalette(int c0, int c1)
    {
        DrawCalls.Add(("SetPalette", 0, 0, (c0, c1)));
    }

    public void ResetPalette()
    {
        DrawCalls.Add(("ResetPalette", 0, 0, null));
    }

    public void Surface(List<(F32 x, F32 y)> vertices, int color)
    {
        DrawCalls.Add(("Surface", 0, 0, (vertices.Count, color)));
    }

    public void SetSpriteData(Color[] spriteData) { }
    public void SetFlagData(int[] flagData) { }
    public void SetMapData(int[] mapData) { }
    public (int width, int height) GetSpriteDimensions() => (128, 128);
}
