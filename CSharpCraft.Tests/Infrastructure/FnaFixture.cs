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

    public void Dispose()
    {
        _game.Dispose();
    }

    /// <summary>
    /// Creates a temporary directory containing a minimal silent WAV file for each base name.
    /// The caller is responsible for deleting the directory when done.
    /// </summary>
    public static string CreateTempSfxDirectory(params string[] sfxBaseNames)
    {
        string dir = Directory.CreateTempSubdirectory("cscraft_test_sfx_").FullName;
        foreach (string name in sfxBaseNames)
            CreateSilentWavFile(Path.Combine(dir, name + ".wav"));
        return dir;
    }

    private static void CreateSilentWavFile(string path)
    {
        // 16-bit mono PCM WAV, 100ms silence at 44100 Hz (4410 samples)
        const int sampleRate = 44100;
        const int sampleCount = 4410;
        const int dataSize = sampleCount * 2;
        using BinaryWriter bw = new(File.Create(path));
        bw.Write(new byte[] { 0x52, 0x49, 0x46, 0x46 }); // "RIFF"
        bw.Write(36 + dataSize);                           // ChunkSize
        bw.Write(new byte[] { 0x57, 0x41, 0x56, 0x45 }); // "WAVE"
        bw.Write(new byte[] { 0x66, 0x6D, 0x74, 0x20 }); // "fmt "
        bw.Write(16);                                      // SubChunk1Size
        bw.Write((short)1);                                // AudioFormat = PCM
        bw.Write((short)1);                                // NumChannels = 1
        bw.Write(sampleRate);                              // SampleRate
        bw.Write(sampleRate * 2);                          // ByteRate
        bw.Write((short)2);                                // BlockAlign
        bw.Write((short)16);                               // BitsPerSample
        bw.Write(new byte[] { 0x64, 0x61, 0x74, 0x61 }); // "data"
        bw.Write(dataSize);                                // SubChunk2Size
        bw.Write(new byte[dataSize]);                      // silence
    }

    /// <summary>
    /// Creates a temporary directory containing a silent OGG file for each requested filename.
    /// The directory is cleaned up when the returned <see cref="TempMusicDirectory"/> is disposed.
    /// </summary>
    public static TempMusicDirectory CreateTempMusicDirectory(params string[] filenames)
    {
        string dir = Directory.CreateTempSubdirectory("cscraft_test_music_").FullName;
        string silentOgg = Path.Combine(AppContext.BaseDirectory, "TestAssets", "silent.ogg");

        foreach (string filename in filenames)
            File.Copy(silentOgg, Path.Combine(dir, filename), overwrite: true);

        return new TempMusicDirectory(dir);
    }

    private static void ConfigureGraphicsBackend()
    {
        Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL3");
        Environment.SetEnvironmentVariable("FNA_NO_OPENGL_INTERCEPTION", "1");
        Environment.SetEnvironmentVariable("FNA3D_FORCE_DRIVER", "SDLGPU");
        Environment.SetEnvironmentVariable("SDL_GPU_DRIVER", "vulkan");

        _ = SDL.SDL_SetHintWithPriority("FNA_PLATFORM_BACKEND", "SDL3", SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
        _ = SDL.SDL_SetHintWithPriority("FNA_NO_OPENGL_INTERCEPTION", "1", SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
        _ = SDL.SDL_SetHintWithPriority("FNA3D_FORCE_DRIVER", "SDLGPU", SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
        _ = SDL.SDL_SetHintWithPriority("SDL_GPU_DRIVER", "vulkan", SDL.SDL_HintPriority.SDL_HINT_OVERRIDE);
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

public sealed class TempMusicDirectory : IDisposable
{
    public string Path { get; }

    internal TempMusicDirectory(string path)
    {
        Path = path;
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}
