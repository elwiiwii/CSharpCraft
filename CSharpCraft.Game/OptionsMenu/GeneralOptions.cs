using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu;

public class GeneralOptions(int startIndex = 0) : IScene, IDisposable
{

    public string SceneName { get => "options"; }
    public double Fps { get => 60.0; }
    public (int w, int h) Resolution { get => (128, 128); }
    private Pico8Functions p8 = null!;

    private int menuSelected;

    private record SettingDescriptor(
        string DisplayName,
        Func<OptionsFile, string> GetDisplay,
        Action<OptionsFile, Pico8Functions, int>? OnChange);

    private static readonly List<SettingDescriptor> Settings =
    [
        new("sound_on",
            f => f.Gen_Sound_On.ToString().ToLower(),
            (f, p8, dir) => { f.Gen_Sound_On = !f.Gen_Sound_On; OptionsFile.JsonWrite(f); p8.Mute(); }),

        new("music_vol",
            f => f.Gen_Music_Vol.ToString(),
            (f, p8, dir) =>
            {
                f.Gen_Music_Vol = dir < 0
                    ? Math.Max(0, f.Gen_Music_Vol - 10)
                    : Math.Min(100, f.Gen_Music_Vol + 10);
                OptionsFile.JsonWrite(f);
            }),

        new("sfx_vol",
            f => f.Gen_Sfx_Vol.ToString(),
            (f, p8, dir) =>
            {
                f.Gen_Sfx_Vol = dir < 0
                    ? Math.Max(0, f.Gen_Sfx_Vol - 10)
                    : Math.Min(100, f.Gen_Sfx_Vol + 10);
                OptionsFile.JsonWrite(f);
            }),

        new("fullscreen",
            f => f.Gen_Fullscreen.ToString().ToLower(),
            (f, p8, dir) =>
            {
                f.Gen_Fullscreen = !f.Gen_Fullscreen;
                OptionsFile.JsonWrite(f);
                p8.Graphics.IsFullScreen = f.Gen_Fullscreen;
                p8.Graphics.PreferredBackBufferWidth = f.Gen_Window_Width / 128 * p8.Resolution.w;
                p8.Graphics.PreferredBackBufferHeight = f.Gen_Window_Height / 128 * p8.Resolution.h;
                p8.Graphics.ApplyChanges();
                p8.UpdateViewport();
            }),

        new("window_width",
            f => f.Gen_Window_Width.ToString(),
            (f, p8, dir) =>
            {
                f.Gen_Window_Width = dir < 0
                    ? Math.Max(128, f.Gen_Window_Width - 128)
                    : Math.Min(16384, f.Gen_Window_Width + 128);
                OptionsFile.JsonWrite(f);
                p8.Graphics.PreferredBackBufferWidth = f.Gen_Window_Width;
                p8.Graphics.PreferredBackBufferHeight = f.Gen_Window_Height;
                p8.Graphics.ApplyChanges();
            }),

        new("window_height",
            f => f.Gen_Window_Height.ToString(),
            (f, p8, dir) =>
            {
                f.Gen_Window_Height = dir < 0
                    ? Math.Max(128, f.Gen_Window_Height - 128)
                    : Math.Min(16384, f.Gen_Window_Height + 128);
                OptionsFile.JsonWrite(f);
                p8.Graphics.PreferredBackBufferWidth = f.Gen_Window_Width;
                p8.Graphics.PreferredBackBufferHeight = f.Gen_Window_Height;
                p8.Graphics.ApplyChanges();
            }),
    ];

    public void Init()
    {

        menuSelected = startIndex;
    }

    public void Update()
    {
        if (menuSelected < 0) { menuSelected = 0; }

        var setting = Settings[menuSelected];
        var optionsFile = OptionsFile.Current;

        if (p8.Btnp(0) && setting.OnChange != null)
        {
            setting.OnChange(optionsFile, p8, -1);
        }
        if (p8.Btnp(1) && setting.OnChange != null)
        {
            setting.OnChange(optionsFile, p8, 1);
        }

        if (p8.Btnp(2)) { menuSelected -= 1; }
        if (p8.Btnp(3)) { menuSelected += 1; }

        if (menuSelected < 0) { p8.ScheduleScene(() => new GeneralOptionsTitle()); return; }
        menuSelected = GeneralFunctions.Loop(menuSelected, Settings.Count);
    }

    public void Draw()
    {
        p8.Cls();

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        
        p8.Batch.Draw(p8.TextureDictionary["OptionsBackground5"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        var optionsFile = OptionsFile.Current;
        int x = 15;
        int y = 43;
        int step = 8;

        if (menuSelected > -1)
        {
            Vector2 position5 = new((x - 4) * p8.Cell.Width, (menuSelected * step + y) * p8.Cell.Height);
            p8.Batch.Draw(p8.TextureDictionary["Arrow"], position5, null, p8.Colors[6], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
        }

        p8.Batch.Draw(p8.TextureDictionary["Checker"], new Vector2(x * p8.Cell.Width, (y - 5) * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        foreach (var setting in Settings)
        {
            p8.Print($"{setting.DisplayName} : {setting.GetDisplay(optionsFile)}", x + 2, y, 6);
            y += step;
        }

        p8.Batch.Draw(p8.TextureDictionary["Checker"], new Vector2(x * p8.Cell.Width, (y + 1) * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipVertically, 0);
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
