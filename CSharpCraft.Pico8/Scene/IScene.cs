namespace CSharpCraft.Pico8;

/// <summary>
/// Scene lifecycle interface.
/// Scenes access the PICO-8 API through the static Pico8 class.
/// No dependency injection parameters — the static API is always available
/// after GameOrchestrator.Initialize() has been called.
/// </summary>
public interface IScene
{
    string SceneName { get; }
    double Fps { get; }
    (int w, int h) Resolution { get; }
    
    /// <summary>
    /// Initialize the scene. Use the static Pico8 API for all game operations.
    /// </summary>
    void Init();
    
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

