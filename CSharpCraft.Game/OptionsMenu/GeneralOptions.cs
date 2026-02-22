using CSharpCraft.Pico8;
using static CSharpCraft.Pico8.Pico8;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu;

public class GeneralOptions(int startIndex = 0) : IScene, IDisposable
{

    public string SceneName { get => "options"; }
    public double Fps { get => 60.0; }
    public (int w, int h) Resolution { get => (128, 128); }

    private int menuSelected;

    private record SettingDescriptor(
        string DisplayName,
        Func<OptionsFile, string> GetDisplay,
        Action<OptionsFile, int>? OnChange);

    private static readonly List<SettingDescriptor> Settings =
    [
        new("sound_on",
            f => f.Gen_Sound_On.ToString().ToLower(),
            (f, dir) => { f.Gen_Sound_On = !f.Gen_Sound_On; OptionsFile.JsonWrite(f); Mute(); }),

        new("music_vol",
            f => f.Gen_Music_Vol.ToString(),
            (f, dir) =>
            {
                f.Gen_Music_Vol = dir < 0
                    ? Math.Max(0, f.Gen_Music_Vol - 10)
                    : Math.Min(100, f.Gen_Music_Vol + 10);
                OptionsFile.JsonWrite(f);
            }),

        new("sfx_vol",
            f => f.Gen_Sfx_Vol.ToString(),
            (f, dir) =>
            {
                f.Gen_Sfx_Vol = dir < 0
                    ? Math.Max(0, f.Gen_Sfx_Vol - 10)
                    : Math.Min(100, f.Gen_Sfx_Vol + 10);
                OptionsFile.JsonWrite(f);
            }),

        new("fullscreen",
            f => f.Gen_Fullscreen.ToString().ToLower(),
            (f, dir) =>
            {
                f.Gen_Fullscreen = !f.Gen_Fullscreen;
                OptionsFile.JsonWrite(f);
                GameRendering.Current.ApplyDisplaySettings(f.Gen_Fullscreen, f.Gen_Window_Width, f.Gen_Window_Height);
                UpdateViewport();
            }),

        new("window_width",
            f => f.Gen_Window_Width.ToString(),
            (f, dir) =>
            {
                f.Gen_Window_Width = dir < 0
                    ? Math.Max(128, f.Gen_Window_Width - 128)
                    : Math.Min(16384, f.Gen_Window_Width + 128);
                OptionsFile.JsonWrite(f);
                GameRendering.Current.ApplyDisplaySettings(f.Gen_Fullscreen, f.Gen_Window_Width, f.Gen_Window_Height);
            }),

        new("window_height",
            f => f.Gen_Window_Height.ToString(),
            (f, dir) =>
            {
                f.Gen_Window_Height = dir < 0
                    ? Math.Max(128, f.Gen_Window_Height - 128)
                    : Math.Min(16384, f.Gen_Window_Height + 128);
                OptionsFile.JsonWrite(f);
                GameRendering.Current.ApplyDisplaySettings(f.Gen_Fullscreen, f.Gen_Window_Width, f.Gen_Window_Height);
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

        if (Btnp(0) && setting.OnChange != null)
        {
            setting.OnChange(optionsFile, -1);
        }
        if (Btnp(1) && setting.OnChange != null)
        {
            setting.OnChange(optionsFile, 1);
        }

        if (Btnp(2)) { menuSelected -= 1; }
        if (Btnp(3)) { menuSelected += 1; }

        if (menuSelected < 0) { ScheduleScene(() => new GeneralOptionsTitle()); return; }
        menuSelected = GeneralFunctions.Loop(menuSelected, Settings.Count);
    }

    public void Draw()
    {
        Cls();

        GameRendering.Current.Draw("OptionsBackground5", new Vector2(0, 0), Color.White, CellWidth, CellHeight);

        var optionsFile = OptionsFile.Current;
        int x = 15;
        int y = 43;
        int step = 8;

        if (menuSelected > -1)
        {
            Vector2 position5 = new((x - 4) * CellWidth, (menuSelected * step + y) * CellHeight);
            GameRendering.Current.Draw("Arrow", position5, Colors[6], CellWidth, CellHeight, flipX: true);
        }

        GameRendering.Current.Draw("Checker", new Vector2(x * CellWidth, (y - 5) * CellHeight), Color.White, CellWidth, CellHeight);

        foreach (var setting in Settings)
        {
            Print($"{setting.DisplayName} : {setting.GetDisplay(optionsFile)}", x + 2, y, 6);
            y += step;
        }

        GameRendering.Current.Draw("Checker", new Vector2(x * CellWidth, (y + 1) * CellHeight), Color.White, CellWidth, CellHeight, flipY: true);
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
