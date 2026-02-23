using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8;

/// <summary>
/// FNA/XNA concrete implementation of ITextureRenderer.
/// Wraps SpriteBatch, TextureDictionary, GameWindow, and GraphicsDeviceManager
/// to provide a testable abstraction for game-layer texture rendering.
///
/// Draw methods accept virtual coordinates and convert to physical pixels
/// using the cellProvider (supplies current cell/tile size from IDisplayManager).
/// 
/// Scenes access this via GameRendering.Current — never directly.
/// </summary>
public class FnaTextureRenderer(
    SpriteBatch batch,
    Dictionary<string, Texture2D> textures,
    GameWindow? window,
    GraphicsDeviceManager? graphics,
    Func<(int Width, int Height)>? cellProvider = null) : ITextureRenderer
{
    private readonly SpriteBatch _batch = batch ?? throw new ArgumentNullException(nameof(batch));
    private readonly Dictionary<string, Texture2D> _textures = textures ?? throw new ArgumentNullException(nameof(textures));
    private readonly Func<(int Width, int Height)> _cellProvider = cellProvider ?? (() => (1, 1));

    /// <inheritdoc />
    public void Draw(string textureName, double x, double y, Color color,
        double scaleX = 1, double scaleY = 1, bool flipX = false, bool flipY = false)
    {
        var texture = _textures[textureName];
        var effects = GetSpriteEffects(flipX, flipY);
        var cell = _cellProvider();
        var position = new Vector2((float)(x * cell.Width), (float)(y * cell.Height));
        var scale = new Vector2((float)(scaleX * cell.Width), (float)(scaleY * cell.Height));

        _batch.Draw(texture, position, null, color, 0f, Vector2.Zero,
            scale, effects, 0f);
    }

    /// <inheritdoc />
    public void Draw(string textureName, double x, double y, Rectangle sourceRect,
        Color color, double scaleX = 1, double scaleY = 1, bool flipX = false, bool flipY = false)
    {
        var texture = _textures[textureName];
        var effects = GetSpriteEffects(flipX, flipY);
        var cell = _cellProvider();
        var position = new Vector2((float)(x * cell.Width), (float)(y * cell.Height));
        var scale = new Vector2((float)(scaleX * cell.Width), (float)(scaleY * cell.Height));

        _batch.Draw(texture, position, sourceRect, color, 0f, Vector2.Zero,
            scale, effects, 0f);
    }

    /// <inheritdoc />
    public void ClearDevice(Color color)
    {
        _batch.GraphicsDevice.Clear(color);
    }

    /// <inheritdoc />
    public (float X, float Y) GetCursorPosition(int mouseX, int mouseY)
    {
        if (window == null)
            return (mouseX, mouseY);

        var viewport = _batch.GraphicsDevice.Viewport;
        return CalculateCursorPosition(
            mouseX, mouseY,
            window.ClientBounds.Width, window.ClientBounds.Height,
            viewport.Width, viewport.Height);
    }

    /// <inheritdoc />
    public void ApplyDisplaySettings(bool fullscreen, int width, int height)
    {
        if (graphics == null) return;

        graphics.IsFullScreen = fullscreen;
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;
        graphics.ApplyChanges();
    }

    /// <inheritdoc />
    public int GetTextureWidth(string textureName)
    {
        return _textures[textureName].Width;
    }

    /// <summary>
    /// Pure cursor position calculation — testable without FNA dependencies.
    /// Replicates the formula copy-pasted across 15 scene files:
    ///   mouseX - ((windowWidth - viewportWidth) / 2.0f)
    ///   mouseY - ((windowHeight - viewportHeight) / 2.0f)
    /// </summary>
    public static (float X, float Y) CalculateCursorPosition(
        int mouseX, int mouseY,
        int windowWidth, int windowHeight,
        int viewportWidth, int viewportHeight)
    {
        float x = mouseX - ((windowWidth - viewportWidth) / 2.0f);
        float y = mouseY - ((windowHeight - viewportHeight) / 2.0f);
        return (x, y);
    }

    private static SpriteEffects GetSpriteEffects(bool flipX, bool flipY)
    {
        var effects = SpriteEffects.None;
        if (flipX) effects |= SpriteEffects.FlipHorizontally;
        if (flipY) effects |= SpriteEffects.FlipVertically;
        return effects;
    }
}
