namespace CSharpCraft.Pico8;

/// <summary>
/// Manages viewport, fullscreen, and display configuration.
/// Owns cell size, virtual resolution, and FNA platform display objects
/// (GraphicsDeviceManager, GraphicsDevice, GameWindow).
/// 
/// Extracted from GameOrchestrator (Phase 7) to give display management
/// a single owner and remove platform dependencies from the orchestrator.
/// </summary>
public interface IDisplayManager
{
    /// <summary>
    /// Current cell/tile size (physical pixels per virtual pixel).
    /// Recomputed each frame from viewport dimensions / scene resolution.
    /// </summary>
    (int Width, int Height) Cell { get; }

    /// <summary>
    /// Current virtual resolution (e.g., 128×128 for standard PICO-8, 192×128 for widescreen).
    /// Updated when a new scene is loaded via UpdateViewport.
    /// </summary>
    (int w, int h) Resolution { get; }

    /// <summary>
    /// Recalculate viewport and resolution based on current scene.
    /// Called on window resize, scene load, and fullscreen toggle.
    /// </summary>
    void UpdateViewport(IScene currentCart);

    /// <summary>
    /// Toggle fullscreen mode and update viewport.
    /// </summary>
    void ToggleFullscreen(IScene currentCart);

    /// <summary>
    /// Recalculate cell size from current viewport and scene resolution.
    /// Called every frame from Update().
    /// </summary>
    void RecalculateCell((int w, int h) sceneResolution);

    /// <summary>
    /// Set display configuration directly (for testing).
    /// In production, cell and resolution are computed from viewport + scene.
    /// </summary>
    void SetDisplayConfig((int w, int h) resolution, (int Width, int Height) cell);
}
