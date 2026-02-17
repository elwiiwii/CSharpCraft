namespace CSharpCraft.Pico8;

/// <summary>
/// Manages music and sound effects playback
/// </summary>
public interface IAudioManager
{
    /// <summary>
    /// Play music track (Pico-8 music function)
    /// </summary>
    /// <param name="trackId">Track identifier</param>
    /// <param name="fadeMs">Fade-in duration in milliseconds (default 0)</param>
    void PlayMusic(int trackId, double fadeMs = 0);

    /// <summary>
    /// Play sound effect (Pico-8 sfx function)
    /// </summary>
    /// <param name="sfxId">Sound effect identifier</param>
    /// <param name="channel">Audio channel (0-3, default -1 for auto)</param>
    /// <param name="offset">Playback offset in samples (default 0)</param>
    void PlaySfx(int sfxId, int channel = -1, int offset = 0);

    /// <summary>
    /// Stop all audio playback
    /// </summary>
    void Mute();

    /// <summary>
    /// Get currently playing music track
    /// </summary>
    int? GetCurrentTrack();

    /// <summary>
    /// Register a music library
    /// </summary>
    void SetMusicLibrary(Dictionary<string, List<SongInst>> musicLibrary);

    /// <summary>
    /// Register a sound effects library
    /// </summary>
    void SetSfxLibrary(Dictionary<string, Dictionary<int, string>> sfxLibrary);
}
