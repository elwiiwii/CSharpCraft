using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8.Graphics;

/// <summary>
/// Describes a fixed-size pixel font stored as a sprite atlas.
/// Glyph pixels should be white on a transparent background so <see cref="SpriteBatch"/>
/// tinting produces the correct foreground colour.
/// </summary>
/// <param name="Texture">Atlas texture.</param>
/// <param name="CharWidth">
/// Width of one standard (code-point &lt; U+0080) glyph cell in the atlas, in pixels.
/// </param>
/// <param name="CharHeight">Height of every glyph cell in the atlas, in pixels.</param>
/// <param name="ExtCharWidth">
/// Width of one extended (code-point ≥ U+0080) glyph cell in the atlas, in pixels.
/// Pass ≤ 0 to use <c>CharWidth * 2</c> (the P8SCII default).
/// </param>
public record FontAtlas(Texture2D Texture, int CharWidth, int CharHeight, int ExtCharWidth = -1)
{
    private int EffectiveExtCharWidth => ExtCharWidth > 0 ? ExtCharWidth : CharWidth * 2;

    /// <summary>
    /// Returns the source rectangle that selects <paramref name="ch"/> from the atlas texture.
    /// </summary>
    /// <remarks>
    /// Layout formula from zepto8:
    /// standard chars (ch &lt; 0x80) → <c>offset = ch</c>;
    /// extended chars (ch ≥ 0x80)  → <c>offset = 2*ch − 0x80</c> (occupies two standard slots).
    /// </remarks>
    public Rectangle GetSourceRect(char ch)
    {
        int offset = ch < '\x80' ? ch : 2 * ch - 0x80;
        int cols   = Texture.Width / CharWidth;
        return new Rectangle(
            (offset % cols) * CharWidth,
            (offset / cols) * CharHeight,
            GetCharWidth(ch),
            CharHeight);
    }

    /// <summary>Returns the atlas pixel width of <paramref name="ch"/> (before any Wide/Tall scaling).</summary>
    public int GetCharWidth(char ch) => ch < '\x80' ? CharWidth : EffectiveExtCharWidth;
}
