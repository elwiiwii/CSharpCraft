namespace CSharpCraft.Pico8;

/// <summary>
/// Static accessor for the popup notification service.
/// Separate from Pico8 static class to preserve SRP — Pico8 is PICO-8 API only,
/// Notifications is the game's notification concern.
/// 
/// Set once during game initialization via Notifications.Initialize(), consumed by scenes and system operations.
/// Tests call Initialize() with a Mock&lt;IPopupService&gt; before each test.
/// 
/// Uses AsyncLocal for thread-safe test isolation (same pattern as GameRendering).
/// </summary>
public static class Notifications
{
    private static readonly AsyncLocal<IPopupService?> _current = new();

    /// <summary>
    /// The current popup service instance.
    /// Throws if not initialized.
    /// </summary>
    public static IPopupService Current =>
        _current.Value ?? throw new InvalidOperationException(
            "Notifications has not been initialized. Call Notifications.Initialize() during startup.");

    /// <summary>
    /// Initialize the static Notifications accessor with a popup service instance.
    /// Must be called once during game initialization.
    /// </summary>
    public static void Initialize(IPopupService popupService)
    {
        _current.Value = popupService ?? throw new ArgumentNullException(nameof(popupService));
    }

    /// <summary>
    /// Show an informational popup notification.
    /// </summary>
    public static void Show(string text)
        => Current.Show(text, PopupSeverity.Info);

    /// <summary>
    /// Show an error popup notification.
    /// </summary>
    public static void ShowError(string text)
        => Current.Show(text, PopupSeverity.Error);

    /// <summary>
    /// Reset to uninitialized state. Used in test teardown.
    /// </summary>
    public static void Reset() => _current.Value = null;
}
