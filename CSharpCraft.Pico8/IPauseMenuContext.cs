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
    /// Toggle fullscreen mode, applying graphics changes, viewport update,
    /// settings persistence, and showing a notification popup.
    /// </summary>
    void ToggleFullscreen();

    /// <summary>
    /// Toggle sound on/off, persisting the change and showing a notification popup.
    /// Handles muting audio when disabled.
    /// </summary>
    void ToggleSound();

    /// <summary>
    /// Quit to the title screen (first scene in Scenes list) and show a notification popup.
    /// </summary>
    void QuitToTitle();

    /// <summary>
    /// Play background music by index. Used by soundtrack switching in the pause menu.
    /// </summary>
    void PlayMusic(int n);
}
