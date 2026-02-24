using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft.Pico8
{
    /// <summary>
    /// GameOrchestrator - Central game coordinator and orchestrator.
    /// Replaces Pico8Functions as the single source of truth for the game loop.
    /// The static Pico8 class delegates to this for all operations.
    /// </summary>
    public class GameOrchestrator : IDisposable, IPauseMenuContext
    {
        // Sub-orchestrators
        private readonly IInputStateManager _inputManager;
        private readonly GraphicsOrchestrator _graphicsOrch;
        private readonly AudioOrchestrator _audioOrch;
        private readonly ISceneManager _sceneManager;
        private readonly ICartDataLoader _cartDataLoader;
        private readonly IServiceFactory _serviceFactory;
        private readonly IDisplayManager _displayManager;
        private IPauseMenuRenderer? _pauseMenuRenderer;
        private IPopupService? _popupService;

        // Managers
        private IMapManager? _mapManager;
        private PaletteManager? _paletteManager;
        private SpriteCache? _spriteCache;
        private ITrackManager? _trackManager;
        private PauseMenuState? _pauseMenuState;

        // State
        private IScene _currentCart;
        private readonly Func<IScene> _titleSceneFactory;
        private readonly List<IScene> _scenes;
        private readonly IInputBindingProvider _inputBindings;
        private readonly IAudioGraphicsSettings _settings;
        private readonly Dictionary<string, Texture2D> _textureDictionary;
        private readonly List<Color> _colors;

        // Parsed cart data (managed by ICartDataLoader)
        private CartData _cartData = CartData.Empty;

        private bool _initialized;

        // Public properties
        public IInputStateManager InputManager => _inputManager;
        public GraphicsOrchestrator Graphics => _graphicsOrch;
        public AudioOrchestrator Audio => _audioOrch;
        public ISceneManager SceneManager => _sceneManager;
        public IMapManager? MapManager => _mapManager;
        public IScene CurrentCart => _currentCart;
        public bool IsPaused => _pauseMenuState?.IsPaused ?? false;

        /// <summary>
        /// Set the pause menu renderer for drawing the pause menu overlay.
        /// Set after construction since ITextureRenderer may not be available during construction.
        /// </summary>
        public IPauseMenuRenderer? PauseMenuRenderer { set => _pauseMenuRenderer = value; }

        /// <summary>
        /// Set the popup service for notification display.
        /// Set after construction since IGraphicsAPI may not be ready during construction.
        /// </summary>
        public IPopupService? PopupService { set => _popupService = value; }

        /// <summary>
        /// Pause the game.
        /// </summary>
        public void Pause()
        {
            if (_pauseMenuState is not null && !_pauseMenuState.IsPaused)
                _pauseMenuState.TogglePause();
        }

        /// <summary>
        /// Resume the game from pause.
        /// </summary>
        public void Resume()
        {
            if (_pauseMenuState is not null && _pauseMenuState.IsPaused)
                _pauseMenuState.TogglePause();
        }
        public IInputBindingProvider InputBindings => _inputBindings;
        public IAudioGraphicsSettings Settings => _settings;
        public List<IScene> Scenes => _scenes;
        public List<Color> Colors => _colors;
        public (int Width, int Height) Cell => _displayManager.Cell;
        public (int w, int h) Resolution => _displayManager.Resolution;
        public List<PalCol> PalColors => _paletteManager?.GetAllRemappings() ?? [];
        public SpriteCache? SpriteCache => _spriteCache;
        public Color[] Sprites => _cartData.Sprites;
        public CartData CartData => _cartData;

        // IPauseMenuContext implementation
        public ITrackManager? TrackManager => _trackManager;
        public int? LastMusicCall => _audioOrch.LastMusicCall;

        /// <summary>
        /// Standard PICO-8 color palette (delegates to Pico8Utils.DefaultColors).
        /// </summary>
        public static List<Color> DefaultColors => Pico8Utils.DefaultColors;

        /// <summary>
        /// Unified constructor. The four core services (inputManager, graphicsAPI,
        /// audioAPI, sceneManager) are required. All other dependencies are optional
        /// with safe Null Object defaults — no null! fragility.
        /// 
        /// Production: pass host + cart + cartDataLoader + titleSceneFactory.
        /// Testing: pass just the 4 required params (all others default safely).
        /// </summary>
        public GameOrchestrator(
            IInputStateManager inputManager,
            IGraphicsAPI graphicsAPI,
            IAudioAPI audioAPI,
            ISceneManager sceneManager,
            ICartDataLoader? cartDataLoader = null,
            GameHostContext? host = null,
            IScene? cart = null,
            Func<IScene>? titleSceneFactory = null,
            IPaletteManager? paletteManager = null,
            IMapManager? mapManager = null,
            IServiceFactory? serviceFactory = null,
            IDisplayManager? displayManager = null)
        {
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            ArgumentNullException.ThrowIfNull(graphicsAPI, nameof(graphicsAPI));
            ArgumentNullException.ThrowIfNull(audioAPI, nameof(audioAPI));
            _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));

            // Scene & host defaults (Null Object pattern — no null! anywhere)
            _currentCart = cart ?? NullScene.Instance;
            _titleSceneFactory = titleSceneFactory ?? (() => _currentCart);
            _scenes = host?.Scenes ?? [];
            _textureDictionary = host?.TextureDictionary ?? [];
            _settings = host?.Settings ?? InMemorySettings.Default;
            _inputBindings = host?.InputBindings ?? DefaultInputBindings.Instance;

            _displayManager = displayManager
                ?? (host != null
                    ? new DisplayManager(host.Graphics, host.GraphicsDevice, host.Window, host.Settings)
                    : new DisplayManager(null, null, null, null));

            _serviceFactory = serviceFactory ?? new ServiceFactory();
            _cartDataLoader = cartDataLoader ?? new CartDataLoader();
            _graphicsOrch = _serviceFactory.CreateGraphicsOrchestrator(graphicsAPI, paletteManager);
            _audioOrch = _serviceFactory.CreateAudioOrchestrator(audioAPI);
            _mapManager = mapManager;
            _colors = DefaultColors;
            _pauseMenuState = _serviceFactory.CreatePauseMenuState(this);
        }

        public void Initialize()
        {
            Pico8.Initialize(this);
            _initialized = true;
            if (_currentCart != null)
            {
                LoadCart(_currentCart);
            }
        }

        public void LoadCart(IScene cart)
        {
            if (cart == null) throw new ArgumentNullException(nameof(cart));

            _currentCart?.Dispose();
            _cartData = CartData.Empty;
            _currentCart = cart;

            _inputManager.Reset();
            SoundDispose();
            UpdateViewport();

            _pauseMenuState?.Reset();
            _pauseMenuState?.InitializeMenuStructure();

            _sceneManager.TransitionToScene(cart);

            Reload();
            _currentCart.Init();
        }

        /// <summary>
        /// Load a scene (alias for LoadCart).
        /// </summary>
        public void LoadScene(IScene scene) => LoadCart(scene);

        public void ReloadCart() => LoadCart(_currentCart);

        public void ScheduleScene(Func<IScene> sceneFactory)
        {
            _sceneManager.ScheduleScene(sceneFactory);
        }

        private void Reload()
        {
            DisposeManagers();

            _cartData = _cartDataLoader.Load(_currentCart, _colors, _textureDictionary);

            _trackManager = _serviceFactory.CreateTrackManager(
                () => _cartData.Music,
                () => _cartData.Sfx,
                _settings);
        }

        public void Update()
        {
            if (!_initialized)
                throw new InvalidOperationException("GameOrchestrator must be initialized before Update().");

            _displayManager.RecalculateCell(_currentCart.Resolution);

            // System hotkeys (Ctrl+Q/R/M/F)
            HandleHotkeys();

            if (!(_currentCart.SceneName == "TitleScreen") && _inputManager.Btnp(6))
            {
                _pauseMenuState?.TogglePause();
            }
            _inputManager.UpdatePauseButton();

            if (_pauseMenuState?.IsPaused ?? false)
            {
                _inputManager.SetPauseMode(true);
                _inputManager.UpdateLockout();

                _pauseMenuState?.HandleMenuInput(
                    _inputManager.Btnp(2),
                    _inputManager.Btnp(3),
                    _inputManager.Btnp(0) || _inputManager.Btnp(1) ||
                    _inputManager.Btnp(4) || _inputManager.Btnp(5));

                PlaySound(false);
            }
            else
            {
                _inputManager.SetPauseMode(false);
                _inputManager.UpdateLockout();

                PlaySound(true);

                try
                {
                    _currentCart.Update();
                }
                catch (Exception ex)
                {
                    HandleSceneException(ex, "Update");
                }

                var scheduledScene = _sceneManager.GetAndClearScheduledScene();
                if (scheduledScene is not null)
                {
                    LoadCart(scheduledScene());
                }
            }

            _inputManager.Update();
            _audioOrch.Update();
            _popupService?.Update();
        }

        private void HandleHotkeys()
        {
            bool ctrl = _inputManager.IsKeyDown(Keys.LeftControl) || _inputManager.IsKeyDown(Keys.RightControl);
            if (!ctrl) return;

            if (_inputManager.IsKeyJustPressed(Keys.Q))
            {
                QuitToTitle();
            }
            else if (_inputManager.IsKeyJustPressed(Keys.R))
            {
                ReloadCart();
            }
            else if (_inputManager.IsKeyJustPressed(Keys.M))
            {
                ToggleSound();
            }
            else if (_inputManager.IsKeyJustPressed(Keys.F))
            {
                ToggleFullscreen();
            }
        }

        private void HandleSceneException(Exception ex, string phase)
        {
            Console.WriteLine($"Scene error in {phase}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);

            // Only show a new error popup if one isn't already active (debounce)
            if (_popupService is not null && !_popupService.HasActivePopup)
            {
                Notifications.ShowError(ex.Message);
            }
        }

        private void PlaySound(bool play)
        {
            _audioOrch.PlaySound(play);
        }

        public void Draw()
        {
            if (!_initialized)
                throw new InvalidOperationException("GameOrchestrator must be initialized before Draw().");

            _graphicsOrch.Pal();
            _graphicsOrch.Palt();

            try
            {
                _currentCart.Draw();
            }
            catch (Exception ex)
            {
                HandleSceneException(ex, "Draw");
            }

            if (_pauseMenuState?.IsPaused ?? false)
            {
                _pauseMenuRenderer?.DrawPauseMenu(
                    _pauseMenuState.CurrentMenuItems,
                    _pauseMenuState.SelectedIndex,
                    _displayManager.Cell);
            }

            _popupService?.Draw(_displayManager.Resolution);
        }

        public void UpdateViewport()
        {
            _displayManager.UpdateViewport(_currentCart);
        }

        public void SoundDispose()
        {
            _audioOrch.SoundDispose();
        }

        /// <summary>
        /// Set display configuration (virtual resolution and cell size).
        /// In production, these are computed from viewport + scene resolution.
        /// This method provides test access to set them directly.
        /// </summary>
        public void SetDisplayConfig((int w, int h) resolution, (int Width, int Height) cell)
        {
            _displayManager.SetDisplayConfig(resolution, cell);
        }

        /// <summary>
        /// Toggle fullscreen mode, applying graphics and viewport changes.
        /// Persists settings and shows a notification popup.
        /// </summary>
        public void ToggleFullscreen()
        {
            _settings.IsFullscreen = !_settings.IsFullscreen;
            _settings.Save();
            _displayManager.ToggleFullscreen(_currentCart);
            Notifications.Show($"fullscreen {(_settings.IsFullscreen ? "on" : "off")} (ctrl-f)");
        }

        /// <summary>
        /// Toggle sound on/off. Persists settings, mutes audio if disabled,
        /// and shows a notification popup.
        /// </summary>
        public void ToggleSound()
        {
            _settings.SoundEnabled = !_settings.SoundEnabled;
            _settings.Save();
            if (!_settings.SoundEnabled)
            {
                _audioOrch.Mute();
            }
            Notifications.Show($"sound {(_settings.SoundEnabled ? "on" : "off")} (ctrl-m)");
        }

        /// <summary>
        /// Quit to the title screen (first scene in Scenes list).
        /// Shows a notification popup.
        /// </summary>
        public void QuitToTitle()
        {
            ScheduleScene(_titleSceneFactory);
            Notifications.Show("quit (ctrl-q)");
        }

        private void DisposeManagers()
        {
            _audioOrch.DisposeAudio();
            _spriteCache?.Dispose();
        }

        public void Dispose()
        {
            DisposeManagers();
            _paletteManager = null;
            _spriteCache = null;
        }
    }
}
