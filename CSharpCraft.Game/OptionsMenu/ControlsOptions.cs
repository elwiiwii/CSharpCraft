using CSharpCraft.Pico8;
using static CSharpCraft.Pico8.Pico8;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu;

public class ControlsOptions : IScene, IDisposable
{

    public string SceneName { get => "options"; }
    public double Fps { get => 60.0; }
    public (int w, int h) Resolution { get => (128, 128); }

    KeyboardOptions drawScene = new(-2);

    public void Init()
    {

        drawScene.Init();
    }

    public void Update()
    {
        if (Btnp(1)) { ScheduleScene(() => new GeneralOptionsTitle()); return; }
        if (Btnp(2)) { ScheduleScene(() => new BackOptions1()); return; }
        if (Btnp(3)) { ScheduleScene(() => new KeyboardOptions()); return; }
    }

    public void Draw()
    {
        Cls();

        drawScene.Draw();

        GameRendering.Current.Draw("OptionsBackground2", new Vector2(0, 0), Color.White, CellWidth, CellHeight);

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
