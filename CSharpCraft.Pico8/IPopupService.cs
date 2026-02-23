namespace CSharpCraft.Pico8;

/// <summary>
/// Service for displaying popup notifications over the game scene.
/// Manages popup animation lifecycle (show → grow in → pause → shrink out → deactivate).
/// Supports severity levels for visual differentiation (info vs error).
/// </summary>
public interface IPopupService
{
    /// <summary>
    /// Show a popup notification with the given text and severity.
    /// The popup animates in, pauses, then animates out automatically.
    /// </summary>
    /// <param name="text">The message text to display.</param>
    /// <param name="severity">The severity level controlling visual styling.</param>
    void Show(string text, PopupSeverity severity = PopupSeverity.Info);

    /// <summary>
    /// Update the popup animation state. Call once per frame.
    /// </summary>
    void Update();

    /// <summary>
    /// Draw the popup overlay if active.
    /// </summary>
    /// <param name="resolution">The virtual resolution (w, h) for positioning.</param>
    void Draw((int w, int h) resolution);

    /// <summary>
    /// Whether a popup is currently being displayed (animation in progress).
    /// </summary>
    bool HasActivePopup { get; }
}
