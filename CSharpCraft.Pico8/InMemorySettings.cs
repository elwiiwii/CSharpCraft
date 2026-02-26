namespace CSharpCraft.Pico8;

/// <summary>
/// In-memory implementation of IAudioSettings and IDisplaySettings.
/// Stores all values in memory with no persistence.
/// Save() is a no-op — used as a safe default when no real settings provider exists (e.g., in tests).
/// </summary>
public sealed class InMemorySettings : IAudioSettings, IDisplaySettings
{
    /// <summary>
    /// Default instance with sensible initial values.
    /// </summary>
    public static readonly InMemorySettings Default = new();

    public bool SoundEnabled { get; set; } = true;
    public int MusicVolume { get; set; } = 100;
    public int SfxVolume { get; set; } = 100;
    public int CurrentSoundtrack { get; set; }
    public int CurrentSfxPack { get; set; }
    public bool IsFullscreen { get; set; }
    public int WindowWidth { get; set; } = 640;
    public int WindowHeight { get; set; } = 480;

    public void Save() { /* No-op: in-memory only */ }
}
