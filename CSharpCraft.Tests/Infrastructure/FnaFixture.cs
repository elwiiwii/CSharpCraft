using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL3;

namespace CSharpCraft.Tests.Infrastructure;

/// <summary>
/// xUnit class fixture that spins up a real FNA <see cref="Game"/>,
/// initializing the graphics subsystem. Shared across all tests in the
/// "Fna" collection.
/// </summary>
public sealed class FnaFixture : IDisposable
{
    private readonly TestGame _game;

    public FnaFixture()
    {
        ConfigureGraphicsBackend();

        _game = new TestGame();
        _game.Run();
    }

    public GraphicsDevice GraphicsDevice => _game.GraphicsDevice;
    public GraphicsDeviceManager GraphicsDeviceManager => _game.GraphicsDeviceManager;
    public GameWindow Window => _game.Window;

    public void Dispose() => _game.Dispose();

    private static void ConfigureGraphicsBackend()
    {
        Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL3");
        Environment.SetEnvironmentVariable("FNA_NO_OPENGL_INTERCEPTION", "1");
        Environment.SetEnvironmentVariable("FNA3D_FORCE_DRIVER", "SDLGPU");
        Environment.SetEnvironmentVariable("SDL_GPU_DRIVER", "vulkan");

        SDL.SDL_SetHintWithPriority("FNA_PLATFORM_BACKEND",     "SDL3",   SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
        SDL.SDL_SetHintWithPriority("FNA_NO_OPENGL_INTERCEPTION", "1",    SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
        SDL.SDL_SetHintWithPriority("FNA3D_FORCE_DRIVER",        "SDLGPU",SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
        SDL.SDL_SetHintWithPriority("SDL_GPU_DRIVER",            "vulkan", SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
    }

    private sealed class TestGame : Game
    {
        public GraphicsDeviceManager GraphicsDeviceManager { get; }

        public TestGame()
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            base.Initialize();
            Exit();
        }
    }
}
