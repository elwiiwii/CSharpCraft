namespace CSharpCraft.Pico8;

public interface IScene
{
    string SceneName { get; }
    double Fps { get; }
    void Init(Pico8Functions p8);
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
