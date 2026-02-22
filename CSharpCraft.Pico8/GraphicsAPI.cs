using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FixMath;

namespace CSharpCraft.Pico8;

/// <summary>
/// Implementation of Pico-8 graphics primitives using XNA/FNA.
/// This class encapsulates all drawing operations and depends on graphics context.
/// </summary>
public class GraphicsAPI : IGraphicsAPI
{
    private readonly SpriteBatch batch;
    private readonly Texture2D pixel;
    private readonly List<Color> colors;
    private readonly F32 cameraOffsetX;
    private readonly F32 cameraOffsetY;
    private readonly (int Width, int Height) cell;

    public GraphicsAPI(
        SpriteBatch batch,
        Texture2D pixel,
        List<Color> colors,
        F32 cameraOffsetX,
        F32 cameraOffsetY,
        (int Width, int Height) cell)
    {
        this.batch = batch;
        this.pixel = pixel;
        this.colors = colors;
        this.cameraOffsetX = cameraOffsetX;
        this.cameraOffsetY = cameraOffsetY;
        this.cell = cell;
    }

    public void Pset(F32 x, F32 y, double c)
    {
        int xFlr = F32.FloorToInt(x);
        int yFlr = F32.FloorToInt(y);
        int cFlr = (int)Math.Floor(c);

        Vector2 position = new((xFlr - F32.FloorToInt(cameraOffsetX)) * cell.Width, (yFlr - F32.FloorToInt(cameraOffsetY)) * cell.Height);
        Vector2 size = new(cell.Width, cell.Height);

        batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
    }

    public void Rect(double x1, double y1, double x2, double y2, double c)
    {
        int x1Flr = (int)Math.Floor(Math.Min(x1, x2));
        int y1Flr = (int)Math.Floor(Math.Min(y1, y2));
        int x2Flr = (int)Math.Floor(Math.Max(x1, x2));
        int y2Flr = (int)Math.Floor(Math.Max(y1, y2));
        int cFlr = (int)Math.Floor(c);

        Rectfill(x1Flr, y1Flr, x2Flr, y1Flr, cFlr);
        Rectfill(x1Flr, y2Flr, x2Flr, y2Flr, cFlr);
        Rectfill(x1Flr, y1Flr, x1Flr, y2Flr, cFlr);
        Rectfill(x2Flr, y1Flr, x2Flr, y2Flr, cFlr);
    }

    public void Rect(double x1, double y1, double x2, double y2, Color c)
    {
        int x1Flr = (int)Math.Floor(Math.Min(x1, x2));
        int y1Flr = (int)Math.Floor(Math.Min(y1, y2));
        int x2Flr = (int)Math.Floor(Math.Max(x1, x2));
        int y2Flr = (int)Math.Floor(Math.Max(y1, y2));

        Rectfill(x1Flr, y1Flr, x2Flr, y1Flr, c);
        Rectfill(x1Flr, y2Flr, x2Flr, y2Flr, c);
        Rectfill(x1Flr, y1Flr, x1Flr, y2Flr, c);
        Rectfill(x2Flr, y1Flr, x2Flr, y2Flr, c);
    }

    public void Rectfill(double x1, double y1, double x2, double y2, double c)
    {
        int x1Flr = (int)Math.Floor(Math.Min(x1, x2));
        int y1Flr = (int)Math.Floor(Math.Min(y1, y2));
        int x2Flr = (int)Math.Floor(Math.Max(x1, x2));
        int y2Flr = (int)Math.Floor(Math.Max(y1, y2));
        int cFlr = (int)Math.Floor(c);

        int rectStartX = (x1Flr - F32.FloorToInt(cameraOffsetX)) * cell.Width;
        int rectStartY = (y1Flr - F32.FloorToInt(cameraOffsetY)) * cell.Height;

        int rectSizeX = (x2Flr - x1Flr + 1) * cell.Width;
        int rectSizeY = (y2Flr - y1Flr + 1) * cell.Height;

        Vector2 position = new(rectStartX, rectStartY);
        Vector2 size = new(rectSizeX, rectSizeY);

        batch.Draw(pixel, position, null, colors[cFlr], 0, Vector2.Zero, size, SpriteEffects.None, 0);
    }

    public void Rectfill(double x1, double y1, double x2, double y2, Color c)
    {
        int x1Flr = (int)Math.Floor(Math.Min(x1, x2));
        int y1Flr = (int)Math.Floor(Math.Min(y1, y2));
        int x2Flr = (int)Math.Floor(Math.Max(x1, x2));
        int y2Flr = (int)Math.Floor(Math.Max(y1, y2));

        int rectStartX = (x1Flr - F32.FloorToInt(cameraOffsetX)) * cell.Width;
        int rectStartY = (y1Flr - F32.FloorToInt(cameraOffsetY)) * cell.Height;

        int rectSizeX = (x2Flr - x1Flr + 1) * cell.Width;
        int rectSizeY = (y2Flr - y1Flr + 1) * cell.Height;

        Vector2 position = new(rectStartX, rectStartY);
        Vector2 size = new(rectSizeX, rectSizeY);

        batch.Draw(pixel, position, null, c, 0, Vector2.Zero, size, SpriteEffects.None, 0);
    }

    public void Circ(F32 x, F32 y, double r, int c)
    {
        // TODO: Phase 2 - Implement circle drawing using line-based rasterization
        // Uses Bresenham's circle algorithm to draw circle outline
    }

    public void Circfill(F32 x, F32 y, double r, int c)
    {
        // TODO: Phase 2 - Implement filled circle drawing
        // Uses Bresenham's circle algorithm with horizontal line fills
    }

    public void Cls(int col = 0)
    {
        // TODO: Phase 2 - Clear screen to specified color
        // Draws a full-screen rectangle with the specified color
    }
}
