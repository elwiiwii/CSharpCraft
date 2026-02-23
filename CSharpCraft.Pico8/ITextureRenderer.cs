using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8;

/// <summary>
/// Abstraction for rendering named textures at scaled positions.
/// This is a game-layer concern (not a PICO-8 concept) — kept separate
/// from the Pico8 static API to preserve SRP/ISP.
/// 
/// Scenes access this via GameRendering.Current (static accessor pattern).
/// Implementation wraps SpriteBatch + TextureDictionary for FNA rendering.
///
/// Draw methods accept virtual coordinates (PICO-8 pixel space).
/// The implementation converts to physical pixels using the current cell size.
/// Scale parameters default to 1.0 (one virtual pixel = one cell).
/// </summary>
public interface ITextureRenderer
{
    /// <summary>
    /// Draw a named texture at a virtual position with color tint and optional scaling.
    /// Position (x, y) is in virtual PICO-8 coordinates.
    /// Scale defaults to 1.0 (texture renders at 1:1 virtual pixel size).
    /// </summary>
    void Draw(string textureName, double x, double y, Color color,
        double scaleX = 1, double scaleY = 1, bool flipX = false, bool flipY = false);

    /// <summary>
    /// Draw a named texture at a virtual position with a source rectangle for atlas clipping.
    /// Position (x, y) is in virtual PICO-8 coordinates.
    /// Scale defaults to 1.0 (texture renders at 1:1 virtual pixel size).
    /// </summary>
    void Draw(string textureName, double x, double y, Rectangle sourceRect, Color color,
        double scaleX = 1, double scaleY = 1, bool flipX = false, bool flipY = false);

    /// <summary>
    /// Clear the graphics device to a solid color.
    /// </summary>
    void ClearDevice(Color color);

    /// <summary>
    /// Calculate cursor position adjusted for window/viewport offset.
    /// Returns physical pixel coordinates.
    /// </summary>
    (float X, float Y) GetCursorPosition(int mouseX, int mouseY);

    /// <summary>
    /// Apply fullscreen/resolution display settings.
    /// </summary>
    void ApplyDisplaySettings(bool fullscreen, int width, int height);

    /// <summary>
    /// Get the pixel width of a named texture.
    /// </summary>
    int GetTextureWidth(string textureName);
}
