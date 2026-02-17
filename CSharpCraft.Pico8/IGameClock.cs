namespace CSharpCraft.Pico8;

/// <summary>
/// Manages game timing, FPS, and frame delta calculations
/// </summary>
public interface IGameClock
{
    /// <summary>
    /// Get target FPS
    /// </summary>
    double TargetFps { get; }

    /// <summary>
    /// Get actual FPS (measured over recent frames)
    /// </summary>
    double CurrentFps { get; }

    /// <summary>
    /// Get delta time in seconds since last frame
    /// </summary>
    double DeltaTime { get; }

    /// <summary>
    /// Get total elapsed game time
    /// </summary>
    TimeSpan ElapsedGameTime { get; }

    /// <summary>
    /// Set target FPS for current scene
    /// </summary>
    void SetTargetFps(double fps);

    /// <summary>
    /// Update frame timing (call once per frame)
    /// </summary>
    void Update();
}
