namespace CSharpCraft.Pico8.Audio;

/// <summary>
/// Defines the Pico-8 audio API for sound effects and music playback.
/// This interface abstracts audio operations to allow for testing and reuse.
/// </summary>
public interface IAudioAPI
{
    /// <summary>
    /// Play a sound effect n on the specified channel.
    /// https://pico-8.fandom.com/wiki/Sfx
    /// </summary>
    /// <param name="n">Sound effect ID</param>
    /// <param name="channel">Channel 0-3, or -1 for automatic</param>
    /// <param name="offset">Offset into the sound</param>
    /// <param name="length">Length of the sound to play</param>
    void Sfx(double n, double channel = -1.0, double offset = 0.0, double length = 31.0);

    /// <summary>
    /// Play music track n with optional fade-in.
    /// https://pico-8.fandom.com/wiki/Music
    /// </summary>
    /// <param name="n">Music track ID</param>
    /// <param name="fadems">Fade-in duration in milliseconds</param>
    void Music(int n, double fadems = 0);

    /// <summary>
    /// Mute all audio (both music and sound effects).
    /// </summary>
    void Mute();

    /// <summary>
    /// Pause all audio playback (music and sound effects).
    /// </summary>
    void Pause();

    /// <summary>
    /// Resume all paused audio playback.
    /// </summary>
    void Resume();

    /// <summary>
    /// Stop and clear all audio (music and sound effects).
    /// </summary>
    void StopAll();

    /// <summary>
    /// Advance audio state (music transitions, fades, etc.).
    /// </summary>
    void Update();
}
