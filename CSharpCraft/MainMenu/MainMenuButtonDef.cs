namespace CSharpCraft.MainMenu;

/// <summary>
/// Immutable data describing a single main-menu button.
/// </summary>
internal record MainMenuButtonDef(
    // Monochrome sprite source rect (default / idle state)
    int MonoSx, int MonoSy, int MonoSw, int MonoSh,
    // Full-color sprite source rect (revealed on hover)
    int ColorSx, int ColorSy, int ColorSw, int ColorSh,
    // Destination rect on screen (also used as the hover hit-box)
    int DestX, int DestY, int DestW, int DestH,
    // Label text drawn beneath the sprite (empty for icon-only buttons)
    string Label = "", int LabelX = 0, int LabelY = 0, int LabelCol = 17,
    // When true the button is frozen: mono + tint overlay + pre-blended text
    bool IsEnabled = true);
