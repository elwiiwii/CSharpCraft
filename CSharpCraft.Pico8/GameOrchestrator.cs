using System;
using CSharpCraft.Pico8.Services;

namespace CSharpCraft.Pico8
{
    /// <summary>
    /// GameOrchestrator - Central game coordinator and orchestrator
    /// 
    /// Responsibility: Pure game loop orchestration and state management
    /// - Scene management (load, transition, update)
    /// - Game state tracking (paused, running, etc.)
    /// - Pause menu coordination
    /// - Holds all manager references (Graphics, Audio, Input)
    /// 
    /// SRP 5/5: This class has ONE reason to change: game orchestration logic
    /// 
    /// Note: The static Pico8 class delegates to this for all operations.
    /// This separation ensures API facade (Pico8) vs orchestration (this) are distinct.
    /// </summary>
    public class GameOrchestrator
    {
        private readonly IInputStateManager _inputManager;
        private readonly GraphicsOrchestrator _graphicsOrch;
        private readonly AudioOrchestrator _audioOrch;
        private readonly ISceneManager _sceneManager;
        private readonly IMapManager? _mapManager;
        private readonly IGameState? _gameState;

        private IScene? _currentScene;
        private bool _initialized;
        private bool _isPaused;

        // Public properties for access to sub-orchestrators (used by Pico8 static class)
        public IInputStateManager InputManager => _inputManager;
        public GraphicsOrchestrator Graphics => _graphicsOrch;
        public AudioOrchestrator Audio => _audioOrch;
        public ISceneManager SceneManager => _sceneManager;
        public IMapManager? MapManager => _mapManager;
        public IGameState? GameState => _gameState;
        public IScene? CurrentScene => _currentScene;
        public bool IsPaused => _isPaused;

        /// <summary>
        /// Initialize GameOrchestrator with required managers and game state.
        /// IGraphicsAPI and IAudioAPI are wrapped in sub-orchestrators for
        /// state tracking and coordination.
        /// </summary>
        public GameOrchestrator(
            IInputStateManager inputManager,
            IGraphicsAPI graphicsAPI,
            IAudioAPI audioAPI,
            ISceneManager sceneManager,
            IPaletteManager? paletteManager = null,
            IMapManager? mapManager = null,
            IGameState? gameState = null)
        {
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            ArgumentNullException.ThrowIfNull(graphicsAPI, nameof(graphicsAPI));
            ArgumentNullException.ThrowIfNull(audioAPI, nameof(audioAPI));
            _graphicsOrch = new GraphicsOrchestrator(graphicsAPI, paletteManager);
            _audioOrch = new AudioOrchestrator(audioAPI);
            _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
            _mapManager = mapManager;
            _gameState = gameState;
            _isPaused = false;
            _currentScene = null;
        }

        /// <summary>
        /// Initialize the game orchestrator
        /// Must be called before any game operations (Update, Draw, LoadScene)
        /// </summary>
        public void Initialize()
        {
            Pico8.Initialize(this);
            _initialized = true;
        }

        /// <summary>
        /// Load a new scene and transition to it
        /// </summary>
        public void LoadScene(IScene scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            _currentScene = scene;
            _currentScene.Init();
        }

        /// <summary>
        /// Pause the game (pause menu open, scene update halted)
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
        }

        /// <summary>
        /// Resume from pause (scene update resumes)
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
        }

        /// <summary>
        /// Update game state and current scene
        /// Called once per frame
        /// </summary>
        public void Update()
        {
            if (!_initialized)
                throw new InvalidOperationException(
                    "GameOrchestrator must be initialized before Update() is called. Call Initialize() first.");

            // If paused, don't update scene
            if (_isPaused)
                return;

            // Update current scene
            _currentScene?.Update();
        }

        /// <summary>
        /// Draw current scene and overlay UI elements (like pause menu)
        /// Called once per frame
        /// </summary>
        public void Draw()
        {
            if (!_initialized)
                throw new InvalidOperationException(
                    "GameOrchestrator must be initialized before Draw() is called. Call Initialize() first.");

            // Clear screen
            _graphicsOrch.Cls(0);

            // Draw current scene
            _currentScene?.Draw();

            // Draw pause menu overlay if paused
            if (_isPaused)
            {
                DrawPauseMenu();
            }
        }

        /// <summary>
        /// Helper method to draw pause menu UI overlay
        /// Implementation will be finalized in Phase 3
        /// </summary>
        private void DrawPauseMenu()
        {
            // TODO: Phase 3 - Implement pause menu UI rendering
            // This should draw a semi-transparent overlay with pause menu options
        }
    }
}
