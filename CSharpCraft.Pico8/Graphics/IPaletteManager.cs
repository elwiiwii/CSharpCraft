using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Pico8.Graphics;

/// <summary>
/// Defines palette management operations for the PICO-8 color system.
/// Manages color remapping (pal) and transparency (palt) state.
/// </summary>
public interface IPaletteManager
{
    /// <summary>
    /// Set palette remapping: color index c0 → color index c1
    /// </summary>
    void SetPalette(int c0, int c1);

    /// <summary>
    /// Set palette remapping from one Color to another Color
    /// </summary>
    void SetPalette(Color c0, Color c1);

    /// <summary>
    /// Reset all palette remappings to defaults
    /// </summary>
    void ResetPalette();

    /// <summary>
    /// Set transparency for a palette color index
    /// </summary>
    void SetTransparency(int colorIndex, bool transparent);

    /// <summary>
    /// Reset all transparency settings (black = transparent by default)
    /// </summary>
    void ResetTransparency();
}
