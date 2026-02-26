using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Pico8;

/// <summary>
/// Manages palette remapping and color lookups with efficient O(1) dictionary lookups.
/// Replaces inefficient FindAll() calls in hot rendering paths.
/// </summary>
public class PaletteManager : IPaletteManager
{
    private readonly List<Color> _staticPalette;
    private readonly Dictionary<Color, PalCol> _paletteMap = [];

    public PaletteManager(List<Color> staticPalette)
    {
        _staticPalette = staticPalette;
        // Initialize with default: black transparent
        _paletteMap[Color.Black] = new(Color.Black, Color.Black, true);
    }

    /// <summary>
    /// Get the static Pico-8 palette colors (read-only).
    /// </summary>
    public List<Color> StaticPalette => _staticPalette;

    /// <summary>
    /// Get the dynamic palette remapping dictionary.
    /// </summary>
    public Dictionary<Color, PalCol> PaletteMap => _paletteMap;

    /// <summary>
    /// Look up a palette color remapping. O(1) using dictionary instead of O(n) FindAll.
    /// </summary>
    public PalCol? TryGetRemapping(Color color)
    {
        _paletteMap.TryGetValue(color, out var result);
        return result;
    }

    /// <summary>
    /// Set palette remapping from one color to another integer index.
    /// </summary>
    public void SetPalette(int c0, int c1)
    {
        if (c0 < 0 || c0 >= _staticPalette.Count || c1 < 0 || c1 >= _staticPalette.Count)
            throw new ArgumentOutOfRangeException("Color indices must be in valid palette range");

        Color fromColor = _staticPalette[c0];
        Color toColor = _staticPalette[c1];

        // Remove existing remapping for this color
        _paletteMap.Remove(fromColor);
        // Add new remapping
        _paletteMap[fromColor] = new(fromColor, toColor, false);
    }

    /// <summary>
    /// Set palette remapping from one Color to another Color.
    /// </summary>
    public void SetPalette(Color c0, Color c1)
    {
        // Remove existing remapping
        _paletteMap.Remove(c0);
        // Add new remapping
        _paletteMap[c0] = new(c0, c1, false);
    }

    /// <summary>
    /// Reset all palette remappings to defaults.
    /// </summary>
    public void ResetPalette()
    {
        _paletteMap.Clear();
        _paletteMap[Color.Black] = new(Color.Black, Color.Black, true);
    }

    /// <summary>
    /// Set transparency (palt) for a palette index.
    /// </summary>
    public void SetTransparency(int colorIndex, bool transparent)
    {
        if (colorIndex < 0 || colorIndex >= _staticPalette.Count)
            throw new ArgumentOutOfRangeException(nameof(colorIndex), "Color index must be in valid palette range");

        Color color = _staticPalette[colorIndex];

        if (!_paletteMap.TryGetValue(color, out var existing))
        {
            existing = new(color, color, transparent);
            _paletteMap[color] = existing;
        }
        else
        {
            existing.Trans = transparent;
        }
    }

    /// <summary>
    /// Set transparency (palt) for a specific Color.
    /// </summary>
    public void SetTransparency(Color color, bool transparent)
    {
        if (!_paletteMap.TryGetValue(color, out var existing))
        {
            existing = new(color, color, transparent);
            _paletteMap[color] = existing;
        }
        else
        {
            existing.Trans = transparent;
        }
    }

    /// <summary>
    /// Reset all transparency settings (palt() with no args).
    /// </summary>
    public void ResetTransparency()
    {
        foreach (var kvp in _paletteMap)
        {
            kvp.Value.Trans = false;
        }

        // Black is always transparent in Pico-8
        if (_paletteMap.TryGetValue(Color.Black, out var black))
        {
            black.Trans = true;
        }
    }

    /// <summary>
    /// Get all palette remappings as a list (for compatibility with old code).
    /// </summary>
    public List<PalCol> GetAllRemappings()
    {
        return _paletteMap.Values.ToList();
    }
}
