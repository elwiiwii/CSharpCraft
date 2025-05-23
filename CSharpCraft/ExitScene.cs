using CSharpCraft.Pico8;

namespace CSharpCraft;

public class ExitScene : IScene, IDisposable
{
    public string SceneName { get => "exit"; }
    public double Fps { get => 60.0; }

    public void Init(Pico8Functions pico8)
    {
        Environment.Exit(0);
    }

    public void Update()
    {

    }

    public void Draw()
    {
        
    }
    public string SpriteImage => "";
    public string SpriteData => @"";
    public string FlagData => @"";
    public (int x, int y) MapDimensions => (0, 0);
    public string MapData => @"";
    public Dictionary<string, List<SongInst>> Music => new();
    public Dictionary<string, Dictionary<int, string>> Sfx => new();
    public void Dispose()
    {

    }

}
