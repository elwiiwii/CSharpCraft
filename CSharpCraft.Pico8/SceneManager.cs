namespace CSharpCraft.Pico8;

/// <summary>
/// Default implementation of ISceneManager.
/// Manages scene transitions and scene lifecycle.
/// </summary>
public class SceneManager : ISceneManager
{
    private IScene? _currentScene;
    private Func<IScene>? _scheduledSceneFactory;
    private readonly List<IScene> _registeredScenes = [];

    public IScene? CurrentScene => _currentScene;

    public void RegisterScene(IScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (!_registeredScenes.Contains(scene))
        {
            _registeredScenes.Add(scene);
        }
    }

    public void ScheduleScene(Func<IScene> sceneFactory)
    {
        ArgumentNullException.ThrowIfNull(sceneFactory);
        _scheduledSceneFactory = sceneFactory;
    }

    public void TransitionToScene(IScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _currentScene = scene;
    }

    public List<IScene> GetRegisteredScenes() => [.. _registeredScenes];

    public Func<IScene>? GetAndClearScheduledScene()
    {
        var factory = _scheduledSceneFactory;
        _scheduledSceneFactory = null;
        return factory;
    }
}
