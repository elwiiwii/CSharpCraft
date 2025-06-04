using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu;

public class BackOptions1 : IScene, IDisposable
{

    public string SceneName { get => "options"; }
    public double Fps { get => 60.0; }
    private Pico8Functions p8;

    KeyboardOptions drawScene = new(-2);

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        drawScene.Init(p8);
    }

    public void Update()
    {
        if (p8.Btnp(3)) { p8.ScheduleScene(() => new ControlsOptions()); return; }
        if (p8.Btnp(4) || p8.Btnp(5)) { p8.ScheduleScene(() => new TitleScreen(false)); return; }
    }

    public void Draw()
    {
        p8.Cls();

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);

        drawScene.Draw();
        
        p8.Batch.Draw(p8.TextureDictionary["OptionsBackground0"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

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
