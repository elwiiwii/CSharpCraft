﻿using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu;

public class GeneralOptionsTitle : IScene, IDisposable
{

    public string SceneName { get => "options"; }
    private Pico8Functions p8;

    GeneralOptions drawScene = new(-1);

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        drawScene.Init(p8);
    }

    public void Update()
    {
        if (p8.Btnp(0)) { p8.LoadCart(new ControlsOptions()); return; }
        if (p8.Btnp(2)) { p8.LoadCart(new BackOptions2()); return; }
        if (p8.Btnp(3)) { p8.LoadCart(new GeneralOptions()); return; }
    }

    public void Draw()
    {
        p8.Cls();

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);

        drawScene.Draw();

        p8.Batch.Draw(p8.TextureDictionary["OptionsBackground3"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

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
