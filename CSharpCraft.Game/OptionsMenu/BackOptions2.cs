using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu;

public class BackOptions2 : IScene, IDisposable
{

    public string SceneName { get => "options"; }
    public double Fps { get => 60.0; }
    public (int w, int h) Resolution { get => (128, 128); }
    private Pico8Functions p8 = null!;

    GeneralOptions drawScene = new(-1);

    public void Init()
    {

        drawScene.Init();
    }

    public void Update()
    {
        if (p8.Btnp(3)) { p8.ScheduleScene(() => new GeneralOptionsTitle()); return; }
        if (p8.Btnp(4) || p8.Btnp(5)) { p8.ScheduleScene(() => new TitleScreen(false)); return; }
    }

    public void Draw()
    {
        p8.Cls();

        drawScene.Draw();

        GameRendering.Current.Draw("OptionsBackground1", new Vector2(0, 0), Color.White, p8.Cell.Width, p8.Cell.Height);

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
