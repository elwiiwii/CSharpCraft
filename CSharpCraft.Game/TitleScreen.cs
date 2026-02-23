using CSharpCraft.Competitive;
using CSharpCraft.Pico8;
using static CSharpCraft.Pico8.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft;

public class TitleScreen(bool animation = false) : IScene, IDisposable
{
    public string SceneName { get => "TitleScreen"; }
    public double Fps { get => 30.0; }
    public (int w, int h) Resolution { get => (128, 128); }
    private readonly string version = "1.1.3";

    private int menuSelected;
    private KeyboardState prevState;
    private int frame;

    public void Init()
    {

        menuSelected = 0;
        prevState = Keyboard.GetState();
        frame = animation ? 0 : 50;
    }

    public void Update()
    {
        KeyboardState state = Keyboard.GetState();

        if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Q) && !prevState.IsKeyDown(Keys.Q))
        {
            try
            {
                AccountHandler.Shutdown();
                RoomHandler.Shutdown();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during AccountHandler shutdown: {ex.Message}");
            }
            Environment.Exit(0);
        }

        if (Btnp(2)) { menuSelected -= 1; }
        if (Btnp(3)) { menuSelected += 1; }

        menuSelected = GeneralFunctions.Loop(menuSelected, Scenes);

        if (frame >= 39 && ((state.IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter)) || Btnp(4) || Btnp(5)))
        {
            ScheduleScene(() => Scenes[menuSelected]);
        }

        prevState = state;
        if (animation) { frame++; }
    }

    public void Draw()
    {
        GameRendering.Current.ClearDevice(Color.Black);

        Vector2 position = new(1 * CellWidth, 1 * CellHeight);

        GameRendering.Current.Draw("CSharpCraftLogo", 1, 1, Color.White);

        if (frame >= 5) { Print($"c# craft {version}", 0, 18, 6); }
        if (frame >= 6) { Print("by nusan-2016 and ellie-2024", 0, 24, 6); }

        if (frame >= 7) { GameRendering.Current.Draw("MusicNote", 3, 36, Colors[13]); }
        if (frame >= 11) { GameRendering.Current.Draw("MusicNote", 11, 38, Colors[13]); }
        if (frame >= 15) { GameRendering.Current.Draw("MusicNote", 19, 36, Colors[13]); }
        if (frame >= 19) { GameRendering.Current.Draw("MusicNote", 27, 34, Colors[13]); }

        if (frame >= 29) { Print("choose a game mode", 0, 50, 6); }

        if (frame >= 39)
        {
            Print(">", 0, 62 + (menuSelected * 6), 7);
            int i = 0;
            foreach (IScene scene in Scenes)
            {
                Print(scene.SceneName, 8, 62 + i, 7);

                i += 6;
            }
        }
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
        // No cleanup needed here as we handle it in Update
    }
}
