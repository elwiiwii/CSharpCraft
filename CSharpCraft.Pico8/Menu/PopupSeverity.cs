namespace CSharpCraft.Pico8.Menu;

/// <summary>
/// Severity levels for popup notifications.
/// Controls visual styling (color coding) of the popup bar.
/// </summary>
public enum PopupSeverity
{
    /// <summary>
    /// Informational popup (e.g., hotkey confirmations like "sound on").
    /// Displayed with PICO-8 color 8 (red) background & color 15 (peach) text.
    /// </summary>
    Info,

    /// <summary>
    /// Error popup (e.g., unhandled scene exceptions, network errors).
    /// Displayed with PICO-8 color 2 (dark purple) background & color 7 (white) text.
    /// </summary>
    Error
}
