using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FixMath;

namespace CSharpCraft.Pico8;

/// <summary>
/// Manages rendering and graphics operations
/// </summary>
public interface IGraphicsEngine
{
    /// <summary>
    /// Get the sprite batch for direct rendering
    /// </summary>
    SpriteBatch Batch { get; }

    /// <summary>
    /// Get the graphics device
    /// </summary>
    GraphicsDevice GraphicsDevice { get; }

    /// <summary>
    /// Get current camera offset
    /// </summary>
    (F32 x, F32 y) CameraOffset { get; }

    /// <summary>
    /// Get current cell/tile size
    /// </summary>
    (int Width, int Height) Cell { get; }

    /// <summary>
    /// Get Pico-8 palette colors
    /// </summary>
    List<Color> Colors { get; }

    /// <summary>
    /// Get texture dictionary for sprite lookups
    /// </summary>
    Dictionary<string, Texture2D> TextureDictionary { get; }

    /// <summary>
    /// Get sprite texture data
    /// </summary>
    Color[] GetSpriteData();

    /// <summary>
    /// Get flag data
    /// </summary>
    int[] GetFlagData();

    /// <summary>
    /// Get tilemap data
    /// </summary>
    int[] GetMapData();

    /// <summary>
    /// Set camera position (Pico-8 camera function)
    /// </summary>
    void SetCamera(F32 x, F32 y);

    /// <summary>
    /// Reset camera to origin
    /// </summary>
    void ResetCamera();

    /// <summary>
    /// Clear screen to color (Pico-8 cls function)
    /// </summary>
    void Cls(int color = 0);

    /// <summary>
    /// Draw circle
    /// </summary>
    void Circle(F32 x, F32 y, double radius, int color);

    /// <summary>
    /// Draw filled circle
    /// </summary>
    void CircleFilled(F32 x, F32 y, double radius, int color);

    /// <summary>
    /// Draw rectangle
    /// </summary>
    void Rectangle(F32 x, F32 y, F32 w, F32 h, int color);

    /// <summary>
    /// Draw filled rectangle
    /// </summary>
    void RectangleFilled(F32 x, F32 y, F32 w, F32 h, int color);

    /// <summary>
    /// Draw line
    /// </summary>
    void Line(F32 x0, F32 y0, F32 x1, F32 y1, int color);

    /// <summary>
    /// Draw sprite (Pico-8 spr function)
    /// </summary>
    void DrawSprite(int spriteNum, F32 x, F32 y, F32? scaleX = null, F32? scaleY = null, bool flipX = false, bool flipY = false);

    /// <summary>
    /// Draw tilemap (Pico-8 map function)
    /// </summary>
    void DrawMap(double celx, double cely, double sx, double sy, double celw, double celh, int flags = 0);

    /// <summary>
    /// Draw text
    /// </summary>
    void Print(string text, F32 x, F32 y, int color);

    /// <summary>
    /// Set palette color mapping (Pico-8 pal function)
    /// </summary>
    void SetPalette(int c0, int c1);

    /// <summary>
    /// Reset palette
    /// </summary>
    void ResetPalette();

    /// <summary>
    /// Draw surface/filled polygon - complex primitive
    /// </summary>
    void Surface(List<(F32 x, F32 y)> vertices, int color);

    /// <summary>
    /// Set sprite data
    /// </summary>
    void SetSpriteData(Color[] spriteData);

    /// <summary>
    /// Set flag data
    /// </summary>
    void SetFlagData(int[] flagData);

    /// <summary>
    /// Set tilemap data
    /// </summary>
    void SetMapData(int[] mapData);
    
    /// <summary>
    /// Get sprite dimensions
    /// </summary>
    (int width, int height) GetSpriteDimensions();
}
