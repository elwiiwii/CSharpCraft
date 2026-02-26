namespace CSharpCraft.Pico8.Graphics;

/// <summary>
/// Display configuration settings.
/// Split from IAudioGraphicsSettings for interface segregation — consumers that only
/// need display settings (DisplayManager) depend on this focused interface.
/// </summary>
public interface IDisplaySettings
{
    /// <summary>
    /// Whether fullscreen is enabled.
    /// </summary>
    bool IsFullscreen { get; set; }

    /// <summary>
    /// Window width in pixels.
    /// </summary>
    int WindowWidth { get; set; }

    /// <summary>
    /// Window height in pixels.
    /// </summary>
    int WindowHeight { get; set; }

    /// <summary>
    /// Persist settings to storage.
    /// </summary>
    void Save();
}
