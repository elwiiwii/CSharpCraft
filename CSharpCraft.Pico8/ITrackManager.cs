namespace CSharpCraft.Pico8;

/// <summary>
/// Defines track management for music and sound effects selection.
/// Allows incrementing/decrementing between available tracks and packs.
/// </summary>
public interface ITrackManager
{
    /// <summary>
    /// Gets the number of available music tracks.
    /// </summary>
    int MusicCount { get; }

    /// <summary>
    /// Gets the number of available sound effect packs.
    /// </summary>
    int SfxCount { get; }

    /// <summary>
    /// Gets the name of the currently selected soundtrack.
    /// Returns "music" if no soundtracks are available.
    /// </summary>
    string GetCurrentSoundtrackName();

    /// <summary>
    /// Gets the name of the currently selected sound effect pack.
    /// Returns "sfx" if no packs are available.
    /// </summary>
    string GetCurrentSfxPackName();

    /// <summary>
    /// Moves to the previous soundtrack, wrapping around if necessary.
    /// Does nothing if only one or zero soundtracks exist.
    /// </summary>
    void DecrementSoundtrack();

    /// <summary>
    /// Moves to the next soundtrack, wrapping around if necessary.
    /// Does nothing if only one or zero soundtracks exist.
    /// </summary>
    void IncrementSoundtrack();

    /// <summary>
    /// Moves to the previous sound effect pack, wrapping around if necessary.
    /// Does nothing if only one or zero packs exist.
    /// </summary>
    void DecrementSfxPack();

    /// <summary>
    /// Moves to the next sound effect pack, wrapping around if necessary.
    /// Does nothing if only one or zero packs exist.
    /// </summary>
    void IncrementSfxPack();
}
