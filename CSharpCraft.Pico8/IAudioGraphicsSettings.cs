namespace CSharpCraft.Pico8;

/// <summary>
/// Provides audio and graphics configuration settings.
/// Abstracts away dependency on Game-layer OptionsFile class.
/// </summary>
public interface IAudioGraphicsSettings
{
    /// <summary>
    /// Whether sound/music is enabled
    /// </summary>
    bool SoundEnabled { get; set; }

    /// <summary>
    /// Music volume (0-100)
    /// </summary>
    int MusicVolume { get; set; }

    /// <summary>
    /// Sound effects volume (0-100)
    /// </summary>
    int SfxVolume { get; set; }

    /// <summary>
    /// Current soundtrack index
    /// </summary>
    int CurrentSoundtrack { get; set; }

    /// <summary>
    /// Current SFX pack index
    /// </summary>
    int CurrentSfxPack { get; set; }

    /// <summary>
    /// Whether fullscreen is enabled
    /// </summary>
    bool IsFullscreen { get; set; }

    /// <summary>
    /// Window width in pixels
    /// </summary>
    int WindowWidth { get; set; }

    /// <summary>
    /// Window height in pixels
    /// </summary>
    int WindowHeight { get; set; }

    /// <summary>
    /// Persist settings to storage.
    /// Game-layer implementations write to a file; test implementations can no-op.
    /// </summary>
    void Save();
}
