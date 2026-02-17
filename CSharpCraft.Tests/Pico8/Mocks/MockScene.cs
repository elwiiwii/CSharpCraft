using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8.Mocks;

/// <summary>
/// Mock implementation of IScene for testing
/// </summary>
public class MockScene : IScene
{
    public string SceneName => "MockScene";
    public double Fps => 60;
    public (int w, int h) Resolution => (128, 128);
    public string SpriteImage => string.Empty;
    public string SpriteData => string.Empty;
    public string FlagData => string.Empty;
    public (int x, int y) MapDimensions => (16, 16);
    public string MapData => string.Empty;
    public Dictionary<string, List<SongInst>> Music => new();
    public Dictionary<string, Dictionary<int, string>> Sfx => new();

    public bool InitCalled { get; private set; }
    public bool UpdateCalled { get; private set; }
    public bool DrawCalled { get; private set; }

    public void Init(IGraphicsEngine? graphics, IAudioManager? audio, IInputManager? input, ISceneManager? sceneManager, IGameClock? gameClock)
    {
        InitCalled = true;
    }

    public void Update()
    {
        UpdateCalled = true;
    }

    public void Draw()
    {
        DrawCalled = true;
    }

    public void Dispose()
    {
    }
}
