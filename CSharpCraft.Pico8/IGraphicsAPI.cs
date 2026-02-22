using Microsoft.Xna.Framework;
using FixMath;

namespace CSharpCraft.Pico8;

/// <summary>
/// Defines the Pico-8 graphics API for drawing primitives.
/// This interface abstracts drawing operations to allow for testing and reuse.
/// </summary>
public interface IGraphicsAPI
{
    /// <summary>
    /// Draw a single pixel at (x, y) with color c.
    /// https://pico-8.fandom.com/wiki/Pset
    /// </summary>
    void Pset(F32 x, F32 y, double c);

    /// <summary>
    /// Draw an unfilled rectangle from (x1, y1) to (x2, y2) with color c.
    /// https://pico-8.fandom.com/wiki/Rect
    /// </summary>
    void Rect(double x1, double y1, double x2, double y2, double c);

    /// <summary>
    /// Draw an unfilled rectangle from (x1, y1) to (x2, y2) with color c.
    /// https://pico-8.fandom.com/wiki/Rect
    /// </summary>
    void Rect(double x1, double y1, double x2, double y2, Color c);

    /// <summary>
    /// Draw a filled rectangle from (x1, y1) to (x2, y2) with color c.
    /// https://pico-8.fandom.com/wiki/Rectfill
    /// </summary>
    void Rectfill(double x1, double y1, double x2, double y2, double c);

    /// <summary>
    /// Draw a filled rectangle from (x1, y1) to (x2, y2) with color c.
    /// https://pico-8.fandom.com/wiki/Rectfill
    /// </summary>
    void Rectfill(double x1, double y1, double x2, double y2, Color c);

    /// <summary>
    /// Draw a circle at (x, y) with radius r and color c.
    /// https://pico-8.fandom.com/wiki/Circ
    /// </summary>
    void Circ(F32 x, F32 y, double r, int c);

    /// <summary>
    /// Draw a filled circle at (x, y) with radius r and color c.
    /// https://pico-8.fandom.com/wiki/Circfill
    /// </summary>
    void Circfill(F32 x, F32 y, double r, int c);

    /// <summary>
    /// Clear the screen to color col (default 0).
    /// https://pico-8.fandom.com/wiki/Cls
    /// </summary>
    void Cls(int col = 0);

    /// <summary>
    /// Draw text at position (x, y) with color c.
    /// https://pico-8.fandom.com/wiki/Print
    /// </summary>
    void Print(string text, double x, double y, double c);

    /// <summary>
    /// Draw large text at position (x, y) with an XNA Color.
    /// Uses a custom big font texture rather than the PICO-8 built-in font.
    /// </summary>
    void PrintBig(string text, int x, int y, Color color);

    /// <summary>
    /// Draw sprite n at (x, y) with optional size and flip.
    /// https://pico-8.fandom.com/wiki/Spr
    /// </summary>
    void Spr(double n, double x, double y, double w = 1.0, double h = 1.0, bool flip_x = false, bool flip_y = false);

    /// <summary>
    /// Draw a stretched sprite region from spritesheet.
    /// https://pico-8.fandom.com/wiki/Sspr
    /// </summary>
    void Sspr(double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh, bool flip_x = false, bool flip_y = false);

    /// <summary>
    /// Draw tilemap region.
    /// https://pico-8.fandom.com/wiki/Map
    /// </summary>
    void Map(double celx, double cely, double sx, double sy, double celw, double celh, int flags = 0);

    /// <summary>
    /// Draw a line from (x0, y0) to (x1, y1) with color c.
    /// https://pico-8.fandom.com/wiki/Line
    /// </summary>
    void Line(F32 x0, F32 y0, F32 x1, F32 y1, int c);

    /// <summary>
    /// Copy memory from source to destination.
    /// https://pico-8.fandom.com/wiki/Memcpy
    /// </summary>
    void Memcpy(int destaddr, int sourceaddr, int len);

    /// <summary>
    /// Reload cart data from source.
    /// https://pico-8.fandom.com/wiki/Reload
    /// </summary>
    void Reload(int dest = 0, int source = 0, int len = 0, string filename = "");

    /// <summary>
    /// Get a palette color by PICO-8 index (0-15).
    /// Returns the platform-native Color representation.
    /// </summary>
    Color GetColor(int index);

    /// <summary>
    /// Draw a scaled pixel (1x1 texture) at a position with color and scale.
    /// Used for debug overlays and visualizations.
    /// </summary>
    void DrawPixelScaled(Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, int effects, float layerDepth);
}
