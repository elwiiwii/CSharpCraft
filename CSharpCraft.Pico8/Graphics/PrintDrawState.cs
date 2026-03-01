using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8.Graphics;

/// <summary>
/// Holds the mutable cursor, colour, and rendering-flag state used by
/// <see cref="GraphicsManager.Print"/>.
///
/// <para><b>Persistent fields</b> (<see cref="CursorX"/>, <see cref="CursorY"/>,
/// <see cref="ForegroundColor"/>) survive across multiple <c>Print</c> calls.</para>
///
/// <para><b>Per-call fields</b> (everything else) are reset to their defaults
/// by <see cref="Reset"/> at the start of each <c>Print</c> call.</para>
/// </summary>
public class PrintDrawState
{
    // ── Persistent across Print calls ────────────────────────────────────────

    /// <summary>Current text cursor horizontal position, in screen pixels.</summary>
    public int CursorX { get; set; }

    /// <summary>Current text cursor vertical position, in screen pixels.</summary>
    public int CursorY { get; set; }

    /// <summary>Active foreground (glyph) colour.  Set by the caller and by inline <c>\x04</c> codes.</summary>
    public Color ForegroundColor { get; set; } = Color.White;

    // ── Per-call state (reset at the start of each Print via Reset()) ─────────

    /// <summary>When <c>true</c>, each glyph is rendered at double width.</summary>
    public bool Wide { get; set; }

    /// <summary>When <c>true</c>, each glyph is rendered at double height.</summary>
    public bool Tall { get; set; }

    /// <summary>When <c>true</c>, foreground and background colours are swapped and a solid background box is drawn.</summary>
    public bool Invert { get; set; }

    /// <summary>When <c>true</c>, a solid background box in <see cref="BackgroundColor"/> is drawn behind each glyph.</summary>
    public bool SolidBackground { get; set; }

    /// <summary>When <c>true</c>, a 1-pixel underline is drawn below each glyph.</summary>
    public bool Underline { get; set; }

    /// <summary>When <c>true</c>, an extra 1-pixel gap is added after each glyph advance.</summary>
    public bool Padding { get; set; }

    /// <summary>Background colour, used for solid-background and invert modes.</summary>
    public Color BackgroundColor { get; set; } = Color.Black;

    /// <summary>Number of character cells per tab stop.</summary>
    public int TabWidth { get; set; } = 4;

    /// <summary>
    /// When non-<c>null</c>, each glyph is drawn up to 8 times in this colour at
    /// 1-pixel offsets (controlled by <see cref="OutlineMask"/>) to produce an outline.
    /// </summary>
    public Color? OutlineColor { get; set; }

    /// <summary>
    /// Bitmask that selects which of the 8 surrounding directions participate in
    /// the outline: bit 0 = NW, 1 = N, 2 = NE, 3 = W, 4 = E, 5 = SW, 6 = S, 7 = SE.
    /// Default <c>0xFF</c> enables all 8 directions.
    /// </summary>
    public byte OutlineMask { get; set; } = 0xFF;

    /// <summary>
    /// Left-margin X set at the start of each <c>Print</c> call; newlines return
    /// <see cref="CursorX"/> to this value.
    /// </summary>
    public int HomeX { get; set; }

    /// <summary>Top-margin Y set at the start of each <c>Print</c> call.</summary>
    public int HomeY { get; set; }

    /// <summary>
    /// Maximum <see cref="CursorX"/> before automatic line-wrap.
    /// Defaults to <see cref="int.MaxValue"/> (no wrapping).
    /// </summary>
    public int RightBorder { get; set; } = int.MaxValue;

    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Resets all per-call flags and derived fields to defaults, preserving
    /// <see cref="CursorX"/>, <see cref="CursorY"/>, and <see cref="ForegroundColor"/>.
    /// Also captures the current cursor position as the home position for the call.
    /// </summary>
    public void Reset()
    {
        Wide            = false;
        Tall            = false;
        Invert          = false;
        SolidBackground = false;
        Underline       = false;
        Padding         = false;
        BackgroundColor = Color.Black;
        TabWidth        = 4;
        OutlineColor    = null;
        OutlineMask     = 0xFF;
        HomeX           = CursorX;
        HomeY           = CursorY;
        RightBorder     = int.MaxValue;
    }
}
