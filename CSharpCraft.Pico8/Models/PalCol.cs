using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8.Models;

/// <summary>
/// Palette color remapping entry (source color, target color, transparency flag).
/// </summary>
public record PalCol(Color C0, Color C1, bool Trans)
{
    public Color C0 { get; set; } = C0;
    public Color C1 { get; set; } = C1;
    public bool Trans { get; set; } = Trans;
}
