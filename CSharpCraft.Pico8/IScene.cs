namespace CSharpCraft.Pico8;

public interface IScene
{
    string SceneName { get; }
    double Fps { get; }
    (int w, int h) Resolution { get; }
    
    /// <summary>
    /// Initialize the scene with dependency-injected services
    /// </summary>
    void Init(IGraphicsEngine graphics, IAudioManager audio, IInputManager input, ISceneManager sceneManager, IGameClock gameClock);
    
    void Update();
    void Draw();
    string SpriteImage { get; }
    string SpriteData { get; }
    string FlagData { get; }
    (int x, int y) MapDimensions { get; }
    string MapData { get; }
    Dictionary<string, List<SongInst>> Music { get; }
    Dictionary<string, Dictionary<int, string>> Sfx { get; }
    void Dispose();
}

