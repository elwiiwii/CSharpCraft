using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8;

/// <summary>
/// Abstraction for rendering named textures at scaled positions.
/// This is a game-layer concern (not a PICO-8 concept) — kept separate
/// from the Pico8 static API to preserve SRP/ISP.
/// 
/// Scenes access this via GameRendering.Current (static accessor pattern).
/// Implementation wraps SpriteBatch + TextureDictionary for FNA rendering.
/// </summary>
public interface ITextureRenderer
{
    /// <summary>
    /// Draw a named texture at a position with color tint and cell-based scaling.
    /// Covers 81 of 83 legacy p8.Batch.Draw calls (null sourceRectangle).
    /// </summary>
    void Draw(string textureName, Vector2 position, Color color,
        float scaleX, float scaleY, bool flipX = false, bool flipY = false);

    /// <summary>
    /// Draw a named texture at a position with a source rectangle for atlas clipping.
    /// Covers 2 of 83 legacy p8.Batch.Draw calls (non-null sourceRectangle).
    /// </summary>
    void Draw(string textureName, Vector2 position, Rectangle sourceRect, Color color,
        float scaleX, float scaleY, bool flipX = false, bool flipY = false);

    /// <summary>
    /// Clear the graphics device to a solid color.
    /// Replaces 16 p8.Batch.GraphicsDevice.Clear(Color.Black) calls.
    /// </summary>
    void ClearDevice(Color color);

    /// <summary>
    /// Calculate cursor position adjusted for window/viewport offset.
    /// Replaces 33 copy-pasted cursor coordinate calculations across 15 files.
    /// </summary>
    (float X, float Y) GetCursorPosition(int mouseX, int mouseY);

    /// <summary>
    /// Apply fullscreen/resolution display settings.
    /// Replaces 10 p8.Graphics calls in GeneralOptions.cs.
    /// </summary>
    void ApplyDisplaySettings(bool fullscreen, int width, int height);

    /// <summary>
    /// Get the pixel width of a named texture.
    /// Used for texture metadata queries (e.g., sprite atlas dimensions).
    /// </summary>
    int GetTextureWidth(string textureName);
}
