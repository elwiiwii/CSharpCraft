namespace CSharpCraft.Pico8.Menu;

/// <summary>
/// Renders the pause menu overlay on top of the game scene.
/// Draws bordered menu with selection arrow and item text.
/// Separated from popup notifications (IPopupService) — different concerns.
/// </summary>
public interface IPauseMenuRenderer
{
    /// <summary>
    /// Draw the pause menu overlay with the given menu items and selection.
    /// </summary>
    /// <param name="menuItems">The list of menu items to display.</param>
    /// <param name="selectedIndex">The currently selected menu item index.</param>
    /// <param name="cell">The current cell size (Width, Height) for scaling.</param>
    void DrawPauseMenu(List<MenuItem> menuItems, int selectedIndex, (int Width, int Height) cell);
}
