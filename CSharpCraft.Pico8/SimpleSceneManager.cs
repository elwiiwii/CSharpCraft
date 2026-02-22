namespace CSharpCraft.Pico8;

/// <summary>
/// Lightweight scene state manager for Pico8Functions coordination.
/// Tracks current scene and pending scene transitions.
/// Phase 7: Scene & Menu Management Extraction
/// </summary>
public class SceneStateManager
{
    private IScene _currentScene;
    private Func<IScene>? _scheduledSceneFactory;

    /// <summary>
    /// Gets the currently loaded scene.
    /// </summary>
    public IScene CurrentScene => _currentScene;

    /// <summary>
    /// Gets whether a scene transition is scheduled.
    /// </summary>
    public bool HasPendingTransition => _scheduledSceneFactory != null;

    public SceneStateManager(IScene initialScene)
    {
        _currentScene = initialScene ?? throw new ArgumentNullException(nameof(initialScene));
    }

    /// <summary>
    /// Schedules a scene transition to occur on next update.
    /// </summary>
    public void ScheduleScene(Func<IScene> sceneFactory)
    {
        if (sceneFactory == null) throw new ArgumentNullException(nameof(sceneFactory));
        _scheduledSceneFactory = sceneFactory;
    }

    /// <summary>
    /// Gets and clears the scheduled scene factory.
    /// Returns null if no scene is scheduled.
    /// </summary>
    public Func<IScene>? GetAndClearScheduledScene()
    {
        var factory = _scheduledSceneFactory;
        _scheduledSceneFactory = null;
        return factory;
    }

    /// <summary>
    /// Updates the current scene after Pico8Functions loads a new one.
    /// </summary>
    public void SetCurrentScene(IScene scene)
    {
        _currentScene = scene ?? throw new ArgumentNullException(nameof(scene));
    }
}
