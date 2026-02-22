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


}
