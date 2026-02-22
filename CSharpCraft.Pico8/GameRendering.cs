namespace CSharpCraft.Pico8;

/// <summary>
/// Static accessor for the game-layer texture renderer.
/// Separate from Pico8 static class to preserve SRP — Pico8 is PICO-8 API only,
/// GameRendering is the game's custom rendering concern.
/// 
/// Set once during game initialization (Main.cs), consumed by scenes.
/// Tests set a Mock&lt;ITextureRenderer&gt; before each test.
/// 
/// Uses AsyncLocal for thread-safe test isolation (same pattern as Pico8.cs).
/// </summary>
public static class GameRendering
{
    private static readonly AsyncLocal<ITextureRenderer?> _current = new();

    /// <summary>
    /// The current texture renderer instance.
    /// Throws if not initialized.
    /// </summary>
    public static ITextureRenderer Current
    {
        get => _current.Value ?? throw new InvalidOperationException(
            "GameRendering has not been initialized. Set GameRendering.Current during startup.");
        set => _current.Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Reset to uninitialized state. Used in test teardown.
    /// </summary>
    public static void Reset() => _current.Value = null;
}
