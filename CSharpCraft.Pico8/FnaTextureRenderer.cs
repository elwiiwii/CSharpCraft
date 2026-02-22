using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8;

/// <summary>
/// FNA/XNA concrete implementation of ITextureRenderer.
/// Wraps SpriteBatch, TextureDictionary, GameWindow, and GraphicsDeviceManager
/// to provide a testable abstraction for game-layer texture rendering.
/// 
/// Scenes access this via GameRendering.Current — never directly.
/// </summary>
public class FnaTextureRenderer : ITextureRenderer
{
    private readonly SpriteBatch _batch;
    private readonly Dictionary<string, Texture2D> _textures;
    private readonly GameWindow? _window;
    private readonly GraphicsDeviceManager? _graphics;

    public FnaTextureRenderer(
        SpriteBatch batch,
        Dictionary<string, Texture2D> textures,
        GameWindow? window,
        GraphicsDeviceManager? graphics)
    {
        _batch = batch ?? throw new ArgumentNullException(nameof(batch));
        _textures = textures ?? throw new ArgumentNullException(nameof(textures));
        _window = window;
        _graphics = graphics;
    }

    /// <inheritdoc />
    public void Draw(string textureName, Vector2 position, Color color,
        float scaleX, float scaleY, bool flipX = false, bool flipY = false)
    {
        var texture = _textures[textureName];
        var effects = GetSpriteEffects(flipX, flipY);
        var scale = new Vector2(scaleX, scaleY);

        _batch.Draw(texture, position, null, color, 0f, Vector2.Zero,
            scale, effects, 0f);
    }

    /// <inheritdoc />
    public void Draw(string textureName, Vector2 position, Rectangle sourceRect,
        Color color, float scaleX, float scaleY, bool flipX = false, bool flipY = false)
    {
        var texture = _textures[textureName];
        var effects = GetSpriteEffects(flipX, flipY);
        var scale = new Vector2(scaleX, scaleY);

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
        if (_window == null)
            return (mouseX, mouseY);

        var viewport = _batch.GraphicsDevice.Viewport;
        return CalculateCursorPosition(
            mouseX, mouseY,
            _window.ClientBounds.Width, _window.ClientBounds.Height,
            viewport.Width, viewport.Height);
    }

    /// <inheritdoc />
    public void ApplyDisplaySettings(bool fullscreen, int width, int height)
    {
        if (_graphics == null) return;

        _graphics.IsFullScreen = fullscreen;
        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();
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
