namespace CSharpCraft.Pico8;

/// <summary>
/// Narrow interface exposing only the operations PauseMenuBuilder needs.
/// Breaks the circular dependency: GameOrchestrator → PauseMenuState → PauseMenuBuilder → GameOrchestrator
/// by replacing the concrete GameOrchestrator dependency with this interface.
/// </summary>
public interface IPauseMenuContext
{
    /// <summary>
    /// Audio/graphics settings for volume, sound toggle, and fullscreen.
    /// </summary>
    IAudioGraphicsSettings Settings { get; }

    /// <summary>
    /// Available scenes (used to find the title screen for "exit" menu item).
    /// </summary>
    List<IScene> Scenes { get; }

    /// <summary>
    /// Current display resolution.
    /// </summary>
    (int w, int h) Resolution { get; }

    /// <summary>
    /// Track manager for music/SFX selection. Null if no tracks loaded.
    /// </summary>
    ITrackManager? TrackManager { get; }

    /// <summary>
    /// The last music call index (from MusicManager), used for soundtrack switching.
    /// </summary>
    int? LastMusicCall { get; }

    /// <summary>
    /// Reload the current cart (reset).
    /// </summary>
    void ReloadCart();

    /// <summary>
    /// Load a specific cart/scene.
    /// </summary>
    void LoadCart(IScene scene);

    /// <summary>
    /// Stop all audio playback.
    /// </summary>
    void SoundDispose();

    /// <summary>
    /// Toggle fullscreen mode, applying graphics changes and viewport update.
    /// Encapsulates GraphicsDeviceManager manipulation.
    /// </summary>
    void ToggleFullscreen();
}
