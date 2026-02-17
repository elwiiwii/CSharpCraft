using System;
using System.Collections.Generic;

namespace CSharpCraft.Pico8.Services;

/// <summary>
/// Service responsible for scene management and transitions
/// Extracted from Pico8Functions to enable service-based architecture
/// Phase 3: Service Extraction - Part 4
/// </summary>
public class SceneManagerService : ISceneManager
{
    private readonly List<IScene> _scenes;
    private IScene? _currentScene;
    private Func<IScene>? _scheduledSceneChange;
    private readonly IGraphicsEngine? _graphicsEngine;
    private readonly IAudioManager? _audioManager;
    private readonly IInputManager? _inputManager;
    private readonly IGameClock? _gameClock;

    public IScene? CurrentScene => _currentScene;
    public Func<IScene>? ScheduledSceneChange => _scheduledSceneChange;

    public SceneManagerService(
        List<IScene> scenes,
        IScene? initialScene = null,
        IGraphicsEngine? graphicsEngine = null,
        IAudioManager? audioManager = null,
        IInputManager? inputManager = null,
        IGameClock? gameClock = null)
    {
        _scenes = scenes ?? throw new ArgumentNullException(nameof(scenes));
        _currentScene = initialScene;
        _graphicsEngine = graphicsEngine;
        _audioManager = audioManager;
        _inputManager = inputManager;
        _gameClock = gameClock;
    }

    /// <summary>
    /// Get scene by name
    /// </summary>
    public IScene? GetSceneByName(string sceneName)
    {
        return _scenes.Find(s => s.SceneName == sceneName);
    }

    /// <summary>
    /// Get scene at index
    /// </summary>
    public IScene? GetSceneAt(int index)
    {
        if (index >= 0 && index < _scenes.Count)
            return _scenes[index];
        return null;
    }

    /// <summary>
    /// Get all scenes
    /// </summary>
    public IReadOnlyList<IScene> GetAllScenes() => _scenes.AsReadOnly();

    /// <summary>
    /// Load a scene immediately
    /// </summary>
    public void LoadScene(IScene scene)
    {
        if (scene == null) throw new ArgumentNullException(nameof(scene));

        // Dispose previous scene
        _currentScene?.Dispose();

        // Initialize new scene with available services
        _currentScene = scene;
        _currentScene.Init(_graphicsEngine, _audioManager, _inputManager, this, _gameClock);
    }

    /// <summary>
    /// Load scene by name
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        var scene = GetSceneByName(sceneName);
        if (scene != null)
        {
            LoadScene(scene);
        }
    }

    /// <summary>
    /// Load scene at index
    /// </summary>
    public void LoadSceneAt(int index)
    {
        var scene = GetSceneAt(index);
        if (scene != null)
        {
            LoadScene(scene);
        }
    }

    /// <summary>
    /// Schedule a scene change to occur on next update
    /// Useful for deferred scene transitions
    /// </summary>
    public void ScheduleScene(Func<IScene> sceneFactory)
    {
        _scheduledSceneChange = sceneFactory ?? throw new ArgumentNullException(nameof(sceneFactory));
    }

    /// <summary>
    /// Immediately transition to a specific scene
    /// </summary>
    public void TransitionToScene(IScene scene)
    {
        LoadScene(scene);
    }

    /// <summary>
    /// Get all registered scenes
    /// </summary>
    public List<IScene> GetRegisteredScenes()
    {
        return new List<IScene>(_scenes);
    }

    /// <summary>
    /// Process any pending scene transitions
    /// </summary>
    public void ProcessPendingTransitions()
    {
        if (_scheduledSceneChange != null)
        {
            var newScene = _scheduledSceneChange();
            _scheduledSceneChange = null;
            LoadScene(newScene);
        }
    }

    /// <summary>
    /// Deprecated: Use ScheduleScene instead
    /// </summary>
    [Obsolete("Use ScheduleScene instead", false)]
    public void ScheduleSceneChange(Func<IScene> sceneFactory)
    {
        ScheduleScene(sceneFactory);
    }

    /// <summary>
    /// Update current scene
    /// </summary>
    public void Update()
    {
        _currentScene?.Update();
    }

    /// <summary>
    /// Draw current scene
    /// </summary>
    public void Draw()
    {
        _currentScene?.Draw();
    }

    /// <summary>
    /// Get FPS of current scene
    /// </summary>
    public double GetCurrentSceneFps()
    {
        return _currentScene?.Fps ?? 60.0;
    }

    /// <summary>
    /// Get resolution of current scene
    /// </summary>
    public (int w, int h) GetCurrentSceneResolution()
    {
        return _currentScene?.Resolution ?? (128, 128);
    }

    /// <summary>
    /// Add a new scene to the scene list
    /// </summary>
    public void RegisterScene(IScene scene)
    {
        if (scene != null && !_scenes.Contains(scene))
        {
            _scenes.Add(scene);
        }
    }

    /// <summary>
    /// Remove a scene from the scene list
    /// </summary>
    public void UnregisterScene(IScene scene)
    {
        if (scene != null)
        {
            _scenes.Remove(scene);
        }
    }

    /// <summary>
    /// Get scene count
    /// </summary>
    public int GetSceneCount() => _scenes.Count;

    public void Dispose()
    {
        _currentScene?.Dispose();
        foreach (var scene in _scenes)
        {
            scene?.Dispose();
        }
    }
}
