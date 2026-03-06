using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Tests.Infrastructure;

/// <summary>
/// xUnit class fixture that spins up a real FNA <see cref="GraphicsDevice"/> headlessly.
/// Requires the test process to have been launched with:
///   SDL_VIDEODRIVER=offscreen, FNA3D_FORCE_DRIVER=OpenGL, LIBGL_ALWAYS_SOFTWARE=1
/// (set via CSharpCraft.runsettings so they are in effect before any native library loads).
/// </summary>
public sealed class GraphicsFixture : IDisposable
{
    private readonly TestGame _game;

    public GraphicsFixture()
    {
        _game = new TestGame();
        // Run() blocks until the SDL event loop exits.
        // TestGame.Initialize() calls Exit() after base.Initialize(), which creates
        // GraphicsDevice and then signals the loop to stop on its next iteration.
        _game.Run();
    }

    public GraphicsDevice GraphicsDevice => _game.GraphicsDevice;

    public void Dispose() => _game.Dispose();

    private sealed class TestGame : Game
    {
        public TestGame()
        {
            // Registering GraphicsDeviceManager is sufficient; it wires itself to
            // the Game's Services and creates GraphicsDevice during Initialize().
            _ = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            // base.Initialize() triggers GraphicsDeviceManager.Initialize(),
            // which calls CreateDevice() and populates Game.GraphicsDevice.
            base.Initialize();

            // Signal the SDL loop to exit on the next tick.
            // GraphicsDevice remains valid until Dispose() is called.
            Exit();
        }
    }
}
