using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8;

/// <summary>
/// Manages viewport calculation, fullscreen toggling, and display configuration.
/// Owns the FNA platform objects (GraphicsDeviceManager, GraphicsDevice, GameWindow)
/// and the derived display state (cell size, virtual resolution).
///
/// Extracted from GameOrchestrator (Phase 7) — all viewport/display math
/// now lives here instead of scattered across orchestrator methods.
/// </summary>
public class DisplayManager : IDisplayManager
{
    private readonly GraphicsDeviceManager? _graphics;
    private readonly GraphicsDevice? _graphicsDevice;
    private readonly GameWindow? _window;
    private readonly IAudioGraphicsSettings? _settings;

    private (int Width, int Height) _cell = (1, 1);
    private (int w, int h) _resolution = (128, 128);

    public DisplayManager(
        GraphicsDeviceManager? graphics,
        GraphicsDevice? graphicsDevice,
        GameWindow? window,
        IAudioGraphicsSettings? settings)
    {
        _graphics = graphics;
        _graphicsDevice = graphicsDevice;
        _window = window;
        _settings = settings;
    }

    /// <inheritdoc />
    public (int Width, int Height) Cell => _cell;

    /// <inheritdoc />
    public (int w, int h) Resolution => _resolution;

    /// <inheritdoc />
    public void UpdateViewport(IScene currentCart)
    {
        if (_window == null || _graphics == null || _graphicsDevice == null || currentCart == null)
            return;

        double windowWidth = _window.ClientBounds.Width;
        double windowHeight = _window.ClientBounds.Height;

        if (!_graphics.IsFullScreen)
        {
            windowWidth /= _resolution.w;
            windowHeight /= _resolution.h;
            windowWidth *= currentCart.Resolution.w;
            windowHeight *= currentCart.Resolution.h;

            _graphics.PreferredBackBufferWidth = (int)windowWidth;
            _graphics.PreferredBackBufferHeight = (int)windowHeight;
            _graphics.ApplyChanges();
        }
        _resolution = currentCart.Resolution;

        int scale = Math.Min((int)windowWidth / currentCart.Resolution.w, (int)windowHeight / currentCart.Resolution.h);
        int width = currentCart.Resolution.w * scale;
        int height = currentCart.Resolution.h * scale;

        double centerX = windowWidth / 2.0;
        double centerY = windowHeight / 2.0;

        int left = (int)Math.Round(centerX - width / 2.0);
        int top = (int)Math.Round(centerY - height / 2.0);

        _graphicsDevice.Viewport = new Viewport(left, top, width, height);
    }

    /// <inheritdoc />
    public void ToggleFullscreen(IScene currentCart)
    {
        if (_settings == null || _graphics == null) return;
        _settings.IsFullscreen = !_settings.IsFullscreen;
        _graphics.IsFullScreen = _settings.IsFullscreen;
        _graphics.PreferredBackBufferWidth = _settings.WindowWidth / 128 * _resolution.w;
        _graphics.PreferredBackBufferHeight = _settings.WindowHeight / 128 * _resolution.h;
        _graphics.ApplyChanges();
        UpdateViewport(currentCart);
    }

    /// <inheritdoc />
    public void RecalculateCell((int w, int h) sceneResolution)
    {
        if (_graphicsDevice == null) return;
        _cell = (_graphicsDevice.Viewport.Width / sceneResolution.w,
                 _graphicsDevice.Viewport.Height / sceneResolution.h);
    }

    /// <inheritdoc />
    public void SetDisplayConfig((int w, int h) resolution, (int Width, int Height) cell)
    {
        _resolution = resolution;
        _cell = cell;
    }
}
