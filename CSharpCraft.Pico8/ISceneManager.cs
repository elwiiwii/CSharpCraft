namespace CSharpCraft.Pico8;

/// <summary>
/// Manages scene transitions and scene lifecycle
/// </summary>
public interface ISceneManager
{
    /// <summary>
    /// Get the currently active scene
    /// </summary>
    IScene? CurrentScene { get; }

    /// <summary>
    /// Register a scene in the scene registry
    /// </summary>
    void RegisterScene(IScene scene);

    /// <summary>
    /// Schedule a scene change to occur on the next frame
    /// (Acts as factory-based transition to ensure proper initialization)
    /// </summary>
    void ScheduleScene(Func<IScene> sceneFactory);

    /// <summary>
    /// Immediately transition to a specific scene
    /// </summary>
    void TransitionToScene(IScene scene);

    /// <summary>
    /// Get all registered scenes
    /// </summary>
    List<IScene> GetRegisteredScenes();

    /// <summary>
    /// Process any pending scene changes
    /// </summary>
    void ProcessPendingTransitions();
}
