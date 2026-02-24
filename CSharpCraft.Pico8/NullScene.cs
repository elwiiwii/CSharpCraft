namespace CSharpCraft.Pico8;

/// <summary>
/// Null Object implementation of IScene.
/// Provides safe no-op defaults for all scene operations.
/// Used as a constructor default when no real scene is provided (e.g., in tests).
/// </summary>
public sealed class NullScene : IScene
{
    /// <summary>
    /// Singleton instance — NullScene is stateless and can be safely shared.
    /// </summary>
    public static readonly NullScene Instance = new();

    public string SceneName => "NullScene";
    public double Fps => 30.0;
    public (int w, int h) Resolution => (128, 128);

    public string SpriteImage => "";
    public string SpriteData => "";
    public string FlagData => "";
    public (int x, int y) MapDimensions => (0, 0);
    public string MapData => "";
    public Dictionary<string, List<SongInst>> Music => [];
    public Dictionary<string, Dictionary<int, string>> Sfx => [];

    public void Init() { }
    public void Update() { }
    public void Draw() { }
    public void Dispose() { }
}
