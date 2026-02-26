namespace CSharpCraft.Pico8.Audio;

/// <summary>
/// Audio configuration settings.
/// Split from IAudioGraphicsSettings for interface segregation — consumers that only
/// need audio settings (MusicManager, TrackManager) depend on this focused interface.
/// </summary>
public interface IAudioSettings
{
    /// <summary>
    /// Whether sound/music is enabled.
    /// </summary>
    bool SoundEnabled { get; set; }

    /// <summary>
    /// Music volume (0-100).
    /// </summary>
    int MusicVolume { get; set; }

    /// <summary>
    /// Sound effects volume (0-100).
    /// </summary>
    int SfxVolume { get; set; }

    /// <summary>
    /// Current soundtrack index.
    /// </summary>
    int CurrentSoundtrack { get; set; }

    /// <summary>
    /// Current SFX pack index.
    /// </summary>
    int CurrentSfxPack { get; set; }

    /// <summary>
    /// Persist settings to storage.
    /// </summary>
    void Save();
}
