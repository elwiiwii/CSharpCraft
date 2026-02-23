using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using FixMath;

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

        // Managers
        private IMapManager? _mapManager;
        private AudioChannels? _audioChannels;
        private MusicManager? _musicManager;
        private PaletteManager? _paletteManager;
        private SpriteCache? _spriteCache;
        private ITrackManager? _trackManager;
        private PauseMenuState? _pauseMenuState;

        // State
        private IScene _currentCart;
        private readonly List<IScene> _scenes;
        private readonly IInputBindingProvider _inputBindings;
        private readonly IAudioGraphicsSettings _settings;
        private readonly SpriteBatch _batch;
        private readonly Texture2D _pixel;
        private readonly GraphicsDeviceManager _graphics;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly GameWindow _window;
        private readonly Dictionary<string, Texture2D> _textureDictionary;
        private readonly Dictionary<string, SoundEffect> _soundEffectDictionary;
        private readonly Dictionary<string, SoundEffect> _musicDictionary;
        private readonly List<Color> _colors;
        private readonly CosDict _cosDict = new();
        private readonly SinDict _sinDict = new();
        private Random _random = new();

        // Parsed cart data (managed by ICartDataLoader)
        private CartData _cartData = CartData.Empty;

        private (int Width, int Height) _cell = (1, 1);
        private (int w, int h) _resolution = (128, 128);
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
        /// The currently loaded scene (alias for CurrentCart, used in tests).
        /// </summary>
        public IScene CurrentScene => _currentCart;

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
        public Dictionary<string, Texture2D> TextureDictionary => _textureDictionary;
        public SpriteBatch Batch => _batch;
        public Texture2D Pixel => _pixel;
        public GraphicsDevice GraphicsDevice => _graphicsDevice;
        public GraphicsDeviceManager GraphicsManager => _graphics;
        public GameWindow Window => _window;
        public (int Width, int Height) Cell => _cell;
        public (int w, int h) Resolution => _resolution;
        public List<PalCol> PalColors => _paletteManager?.GetAllRemappings() ?? [];
        public SpriteCache? SpriteCache => _spriteCache;
        public Color[] Sprites => _cartData.Sprites;
        public CartData CartData => _cartData;

        // IPauseMenuContext implementation
        public ITrackManager? TrackManager => _trackManager;
        public int? LastMusicCall => _musicManager?.LastMusicCall;

        /// <summary>
        /// Standard PICO-8 color palette.
        /// </summary>
        public static List<Color> DefaultColors =>
        [
            Pico8Utils.HexToColor("000000"), Pico8Utils.HexToColor("1D2B53"),
            Pico8Utils.HexToColor("7E2553"), Pico8Utils.HexToColor("008751"),
            Pico8Utils.HexToColor("AB5236"), Pico8Utils.HexToColor("5F574F"),
            Pico8Utils.HexToColor("C2C3C7"), Pico8Utils.HexToColor("FFF1E8"),
            Pico8Utils.HexToColor("FF004D"), Pico8Utils.HexToColor("FFA300"),
            Pico8Utils.HexToColor("FFEC27"), Pico8Utils.HexToColor("00E436"),
            Pico8Utils.HexToColor("29ADFF"), Pico8Utils.HexToColor("83769C"),
            Pico8Utils.HexToColor("FF77A8"), Pico8Utils.HexToColor("FFCCAA"),
            Pico8Utils.HexToColor("291814"), Pico8Utils.HexToColor("111D35"),
            Pico8Utils.HexToColor("422136"), Pico8Utils.HexToColor("125359"),
            Pico8Utils.HexToColor("742F29"), Pico8Utils.HexToColor("49333B"),
            Pico8Utils.HexToColor("A28879"), Pico8Utils.HexToColor("F3EF7D"),
            Pico8Utils.HexToColor("BE1250"), Pico8Utils.HexToColor("FF6C24"),
            Pico8Utils.HexToColor("A8E72E"), Pico8Utils.HexToColor("00B543"),
            Pico8Utils.HexToColor("065AB5"), Pico8Utils.HexToColor("754665"),
            Pico8Utils.HexToColor("FF6E59"), Pico8Utils.HexToColor("FF9D81"),
        ];

        public GameOrchestrator(
            IScene cart,
            List<IScene> scenes,
            Dictionary<string, Texture2D> textureDictionary,
            Dictionary<string, SoundEffect> soundEffectDictionary,
            Dictionary<string, SoundEffect> musicDictionary,
            Texture2D pixel,
            SpriteBatch batch,
            GraphicsDeviceManager graphics,
            GraphicsDevice graphicsDevice,
            GameWindow window,
            IAudioGraphicsSettings settings,
            IInputBindingProvider inputBindings,
            IInputStateManager inputManager,
            IGraphicsAPI graphicsAPI,
            IAudioAPI audioAPI,
            ISceneManager sceneManager,
            ICartDataLoader cartDataLoader,
            IPaletteManager? paletteManager = null,
            IMapManager? mapManager = null,
            IServiceFactory? serviceFactory = null)
        {
            _currentCart = cart ?? throw new ArgumentNullException(nameof(cart));
            _scenes = scenes ?? throw new ArgumentNullException(nameof(scenes));
            _textureDictionary = textureDictionary;
            _soundEffectDictionary = soundEffectDictionary;
            _musicDictionary = musicDictionary;
            _pixel = pixel;
            _batch = batch;
            _graphics = graphics;
            _graphicsDevice = graphicsDevice;
            _window = window;
            _settings = settings;
            _inputBindings = inputBindings;

            _serviceFactory = serviceFactory ?? new ServiceFactory();
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            _cartDataLoader = cartDataLoader ?? throw new ArgumentNullException(nameof(cartDataLoader));
            _graphicsOrch = _serviceFactory.CreateGraphicsOrchestrator(graphicsAPI, paletteManager);
            _audioOrch = _serviceFactory.CreateAudioOrchestrator(audioAPI);
            _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
            _mapManager = mapManager;
            _colors = DefaultColors;
            _pauseMenuState = _serviceFactory.CreatePauseMenuState(this);
        }

        /// <summary>
        /// Simplified constructor for testing.
        /// </summary>
        public GameOrchestrator(
            IInputStateManager inputManager,
            IGraphicsAPI graphicsAPI,
            IAudioAPI audioAPI,
            ISceneManager sceneManager,
            ICartDataLoader? cartDataLoader = null,
            IPaletteManager? paletteManager = null,
            IMapManager? mapManager = null,
            IServiceFactory? serviceFactory = null)
        {
            _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
            ArgumentNullException.ThrowIfNull(graphicsAPI, nameof(graphicsAPI));
            ArgumentNullException.ThrowIfNull(audioAPI, nameof(audioAPI));
            _serviceFactory = serviceFactory ?? new ServiceFactory();
            _cartDataLoader = cartDataLoader ?? new CartDataLoader();
            _graphicsOrch = _serviceFactory.CreateGraphicsOrchestrator(graphicsAPI, paletteManager);
            _audioOrch = _serviceFactory.CreateAudioOrchestrator(audioAPI);
            _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
            _mapManager = mapManager;

            // Test defaults
            _currentCart = null!;
            _scenes = [];
            _textureDictionary = [];
            _soundEffectDictionary = [];
            _musicDictionary = [];
            _pixel = null!;
            _batch = null!;
            _graphics = null!;
            _graphicsDevice = null!;
            _window = null!;
            _settings = null!;
            _inputBindings = null!;
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

            _cell = (_graphicsDevice.Viewport.Width / _currentCart.Resolution.w,
                     _graphicsDevice.Viewport.Height / _currentCart.Resolution.h);

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
                _currentCart.Update();

                var scheduledScene = _sceneManager.GetAndClearScheduledScene();
                if (scheduledScene is not null)
                {
                    LoadCart(scheduledScene());
                }
            }

            _inputManager.Update();
            _musicManager?.Update();
        }

        private void PlaySound(bool play)
        {
            if (play) { _musicManager?.Resume(); _audioChannels?.ResumeAll(); }
            else { _musicManager?.Pause(); _audioChannels?.PauseAll(); }
        }

        public void Draw()
        {
            if (!_initialized)
                throw new InvalidOperationException("GameOrchestrator must be initialized before Draw().");

            _graphicsOrch.Pal();
            _graphicsOrch.Palt();
            _currentCart.Draw();

            if (_pauseMenuState?.IsPaused ?? false)
            {
                DrawPauseMenu();
            }
        }

        private void DrawPauseMenu()
        {
            var curMenuItems = _pauseMenuState!.CurrentMenuItems;
            var menuSelected = _pauseMenuState.SelectedIndex;

            Vector2 size = new(_cell.Width, _cell.Height);

            int i = (int)Math.Floor(64 - (curMenuItems.Count / 2.0) * 8);

            int xborder = 23;
            _graphicsOrch.Rectfill(0 + xborder, i - 7, 127 - xborder, i + curMenuItems.Count * 8 + 2, 0);
            _graphicsOrch.Rectfill(0 + xborder + 1, i - 7 + 1, 127 - xborder - 1, i + curMenuItems.Count * 8 + 2 - 1, 7);
            _graphicsOrch.Rectfill(0 + xborder + 2, i - 7 + 2, 127 - xborder - 2, i + curMenuItems.Count * 8 + 2 - 2, 0);

            _batch.Draw(_textureDictionary["PauseArrow"],
                new Vector2((xborder + 4) * _cell.Width, (i - 1 + menuSelected * 8) * _cell.Height),
                null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            for (int j = 0; j < curMenuItems.Count; j++)
            {
                int indent = menuSelected == j ? 1 : 0;
                _graphicsOrch.Print(curMenuItems[j].GetName(), xborder + indent + 12, i, 7);
                i += 8;
            }
        }

        public void UpdateViewport()
        {
            if (_window == null || _graphics == null || _graphicsDevice == null || _currentCart == null)
                return;

            double windowWidth = _window.ClientBounds.Width;
            double windowHeight = _window.ClientBounds.Height;

            if (!_graphics.IsFullScreen)
            {
                windowWidth /= _resolution.w;
                windowHeight /= _resolution.h;
                windowWidth *= _currentCart.Resolution.w;
                windowHeight *= _currentCart.Resolution.h;

                _graphics.PreferredBackBufferWidth = (int)windowWidth;
                _graphics.PreferredBackBufferHeight = (int)windowHeight;
                _graphics.ApplyChanges();
            }
            _resolution = _currentCart.Resolution;

            int scale = Math.Min((int)windowWidth / _currentCart.Resolution.w, (int)windowHeight / _currentCart.Resolution.h);
            int width = _currentCart.Resolution.w * scale;
            int height = _currentCart.Resolution.h * scale;

            double centerX = windowWidth / 2.0;
            double centerY = windowHeight / 2.0;

            int left = (int)Math.Round(centerX - width / 2.0);
            int top = (int)Math.Round(centerY - height / 2.0);

            _graphicsDevice.Viewport = new Viewport(left, top, width, height);
        }

        public void SoundDispose()
        {
            _musicManager?.StopAll();
            _audioChannels?.StopAll();
        }

        /// <summary>
        /// Set display configuration (virtual resolution and cell size).
        /// In production, these are computed from viewport + scene resolution.
        /// This method provides test access to set them directly.
        /// </summary>
        public void SetDisplayConfig((int w, int h) resolution, (int Width, int Height) cell)
        {
            _resolution = resolution;
            _cell = cell;
        }

        /// <summary>
        /// Toggle fullscreen mode, applying graphics and viewport changes.
        /// Encapsulates GraphicsDeviceManager manipulation so PauseMenuBuilder
        /// doesn't need direct access to GraphicsManager.
        /// </summary>
        public void ToggleFullscreen()
        {
            if (_settings == null || _graphics == null) return;
            _settings.IsFullscreen = !_settings.IsFullscreen;
            _graphics.IsFullScreen = _settings.IsFullscreen;
            _graphics.PreferredBackBufferWidth = _settings.WindowWidth / 128 * _resolution.w;
            _graphics.PreferredBackBufferHeight = _settings.WindowHeight / 128 * _resolution.h;
            _graphics.ApplyChanges();
            UpdateViewport();
        }

        private void DisposeManagers()
        {
            _musicManager?.StopAll();
            _audioChannels?.StopAll();
            _spriteCache?.Dispose();
        }

        public void Dispose()
        {
            DisposeManagers();
            _paletteManager = null;
            _musicManager = null;
            _audioChannels = null;
            _spriteCache = null;
        }
    }
}
