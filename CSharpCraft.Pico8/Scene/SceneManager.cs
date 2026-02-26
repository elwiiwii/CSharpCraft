using System.Collections.Immutable;

namespace CSharpCraft.Pico8.Scene;

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
        _ = scene ?? throw new ArgumentNullException(nameof(scene));
        if (!_registeredScenes.Contains(scene))
        {
            _registeredScenes.Add(scene);
        }
    }

    public void ScheduleScene(Func<IScene> sceneFactory)
    {
        _scheduledSceneFactory = sceneFactory ?? throw new ArgumentNullException(nameof(sceneFactory));
    }

    public void TransitionToScene(IScene scene)
    {
        _currentScene = scene ?? throw new ArgumentNullException(nameof(scene));
    }

    public List<IScene> GetRegisteredScenes() => _registeredScenes.ToList();

    public Func<IScene>? GetAndClearScheduledScene()
    {
        var factory = _scheduledSceneFactory;
        _scheduledSceneFactory = null;
        return factory;
    }
}
