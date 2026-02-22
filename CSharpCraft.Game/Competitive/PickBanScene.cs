using CSharpCraft.Pcraft;
using CSharpCraft.Pico8;
using static CSharpCraft.Pico8.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RaceServer;

namespace CSharpCraft.Competitive;

public class PickBanScene : IScene, IDisposable
{
    public string SceneName { get => "1"; }
    public double Fps { get => 60.0; }
    public (int w, int h) Resolution { get => (192, 128); }

    private KeyboardState prevKeyboardState;
    private MouseState prevMouseState;
    private float cursorX;
    private float cursorY;
    private bool isInitializing;
    private bool isInitialized;
    private RoomUser higherSeed;
    private RoomUser lowerSeed;
    private (int h, int l) score;
    private int bestOf;
    private int gameCount;
    private (int h, int l) advantage;
    private List<Game> games = [];
    private int sel;
    private List<SeedTypeUI> seedTypes = [];
    private SeedPickButton gentlemenButton;

    private class Game
    {
        public string? Time { get; set; } = null;
        public double? Percentage { get; set; } = null;
        public int? SurfaceType { get; set; } = null;
        public int? SurfacePicker { get; set; } = null;
        public int? CaveType { get; set; } = null;
        public int? CavePicker { get; set; } = null;
        public bool HigherWin { get; set; } = false;
        public bool LowerWin { get; set; } = false;
    }

    public async void Init()
    {
        if (isInitializing) return;
        isInitializing = true;

        try
        {

            await AccountHandler.ConnectToServer();
            if (!AccountHandler._isLoggedIn)
            {
                ScheduleScene(() => new LoginScene(new CompetitiveScene()));
                return;
            }

            prevKeyboardState = Keyboard.GetState();
            prevMouseState = Mouse.GetState();
            var cursor = GameRendering.Current.GetCursorPosition(prevMouseState.X, prevMouseState.Y);
            cursorX = cursor.X;
            cursorY = cursor.Y;

            for (int i = 0; i < 5; i++)
            {
                seedTypes.Add(new((141, 2 + i * 23), true, i + 1, false));
                seedTypes.Add(new((166, 2 + i * 23), false, i + 1, false));
            }

            gentlemenButton = new((144, 117), "gentlemen");

            higherSeed = new RoomUser
            {
                Name = "__higher__seed__",
                Role = Role.Player,
                Host = false,
                Ready = true,
                Seed = 1
            };

            lowerSeed = new RoomUser
            {
                Name = "__lower___seed__",
                Role = Role.Player,
                Host = false,
                Ready = true,
                Seed = 2
            };

            score = (3, 3);
            bestOf = 7;
            gameCount = 7;
            advantage = (0, 0);

            games.Add(new Game
            {
                Time = "4:00.00",
                Percentage = 80,
                SurfaceType = 1,
                SurfacePicker = 0,
                CaveType = 1,
                CavePicker = 1,
                HigherWin = true
            });

            games.Add(new Game
            {
                Time = "4:00.00",
                Percentage = 80,
                SurfaceType = 2,
                SurfacePicker = 1,
                CaveType = 2,
                CavePicker = 0,
                LowerWin = true
            });

            games.Add(new Game
            {
                Time = "4:00.00",
                Percentage = 80,
                SurfaceType = 3,
                SurfacePicker = 1,
                CaveType = 3,
                CavePicker = 0,
                HigherWin = true
            });

            games.Add(new Game
            {
                Time = "4:00.00",
                Percentage = 80,
                SurfaceType = 3,
                SurfacePicker = 0,
                CaveType = 3,
                CavePicker = 1,
                LowerWin = true
            });
            
            games.Add(new Game
            {
                Time = "4:00.00",
                Percentage = 80,
                SurfaceType = 4,
                SurfacePicker = 1,
                CaveType = 4,
                CavePicker = 0,
                HigherWin = true
            });

            games.Add(new Game
            {
                Time = "4:00.00",
                Percentage = 80,
                SurfaceType = 5,
                SurfacePicker = 0,
                CaveType = 5,
                CavePicker = 1,
                LowerWin = true
            });

            games.Add(new Game
            {
                CaveType = 4,
                CavePicker = 1,
            });

            sel = Math.Max(0, gameCount - 5);

            isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing CompetitiveScene: {ex.Message}");
            if (ex.InnerException is not null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            ScheduleScene(() => new LoginScene(this));
        }
        finally
        {
            isInitializing = false;
        }
    }

    public async void Update()
    {
        if (!isInitialized || isInitializing) return;

        if (RoomHandler._myself is null)
        {
            ScheduleScene(() => new PrivateScene(new CompetitiveScene()));
            return;
        }

        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        var cursorUpdate = GameRendering.Current.GetCursorPosition(mouseState.X, mouseState.Y);
        cursorX = cursorUpdate.X;
        cursorY = cursorUpdate.Y;

        //if (cursorX > 0 * CellWidth && cursorX < 135 * CellWidth && cursorY > 0 * CellHeight && cursorY < 103 * CellHeight)
        
        if (mouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue)
        {
            sel = Math.Max(0, sel - 1);
        }
        else if (mouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue)
        {
            sel = Math.Min(Math.Max(0, gameCount - 5), sel + 1);
        }
        
        foreach (SeedTypeUI seedType in seedTypes)
        {
            seedType.Update(mouseState, prevMouseState);
        }
        gentlemenButton.Update(cursorX, cursorY);

        prevKeyboardState = keyboardState;
        prevMouseState = mouseState;
    }

    public void Draw()
    {
        GameRendering.Current.ClearDevice(Color.Black);

        Rectfill(0, 0, 191, 127, 17);

        if (!isInitialized || isInitializing) { Shared.Printc("loading...", 96, 61, 15); return; }

        for (int i = 0; i < 5; i ++)
        {
            if (sel + i < Math.Min(gameCount, games.Count))
            {
                if (games[sel + i].SurfaceType is not null && games[sel + i].SurfaceType > 0 && games[sel + i].SurfaceType <= 5)
                {
                    GameRendering.Current.Draw($"Surface{games[sel + i].SurfaceType}Test", new Vector2(3 * CellWidth, (3 + i * 20) * CellHeight), new Rectangle(0, 0, 10, 18), Color.White, CellWidth, CellHeight);
                }
                if (games[sel + i].CaveType is not null && games[sel + i].CaveType > 0 && games[sel + i].CaveType <= 5)
                {
                    GameRendering.Current.Draw($"Cave{games[sel + i].CaveType}Test", new Vector2(13 * CellWidth, (3 + i * 20) * CellHeight), new Rectangle(10, 0, 8, 18), Color.White, CellWidth, CellHeight);
                }

                int col;
                if (games[sel + i].CavePicker is null) col = 7;
                else col = games[sel + i].CavePicker == 0 ? 24 : 28;
                GameRendering.Current.Draw("SeedPickIndicator", new Vector2(11 * CellWidth, (3 + i * 20) * CellHeight), Colors[col], CellWidth, CellHeight, flipX: true);
                if (games[sel + i].SurfacePicker is null) col = 7;
                else col = games[sel + i].SurfacePicker == 0 ? 24 : 28;
                GameRendering.Current.Draw("SeedPickIndicator", new Vector2(3 * CellWidth, (3 + i * 20) * CellHeight), Colors[col], CellWidth, CellHeight);

                Print($"game {sel + i + 1}", 24, 6 + i * 20, 7);
                if (games[sel + i].Time is not null) Print(games[sel + i].Time, 24, 13 + i * 20, 7);
                if (games[sel + i].HigherWin && !games[sel + i].LowerWin)
                {
                    Shared.Printc(higherSeed.Name, 93, 6 + i * 20, 24);
                    if (games[sel + i].Percentage is not null) Shared.Printr($"{games[sel + i].Percentage}%", 124, 13 + i * 20, 28);
                    Print("victory", 79, 13 + i * 20, 7);
                }
                else if (!games[sel + i].HigherWin && games[sel + i].LowerWin)
                {
                    Shared.Printc(lowerSeed.Name, 93, 6 + i * 20, 28);
                    if (games[sel + i].Percentage is not null) Shared.Printr($"{games[sel + i].Percentage}%", 124, 13 + i * 20, 24);
                    Print("victory", 79, 13 + i * 20, 7);
                }
                else if (games[sel + i].HigherWin && games[sel + i].LowerWin)
                {
                    Shared.Printc("tied", 93, 6 + i * 20, 7);
                    Shared.Printc("game", 93, 13 + i * 20, 7);
                }
                else
                {
                    Print("picking...", 79, 13 + i * 20, 7);
                }
            }
        }

        foreach (SeedTypeUI seedType in seedTypes)
        {
            seedType.Draw();
        }
        gentlemenButton.Draw();

        Rectfill(133, 0, 135, 127, 13);
        Rectfill(0, 104, 132, 104, 13);
        Rectfill(0, 105, 134, 127, 17);
        Rectfill(67, 105, 67, 113, 13);
        double range = 102.0 / Math.Max(5, gameCount);
        Rectfill(134, 1 + sel * range, 134, 1 + (sel + 5) * range, 6);

        Shared.Printc(higherSeed.Name, 34, 107, 24);
        Print($"seed #{higherSeed.Seed}", 2, 114, 6);
        Shared.Printc($"{score.h}-{score.l}", 67, 114, 7);

        Shared.Printc(lowerSeed.Name, 102, 107, 28);
        Shared.Printr($"seed #{lowerSeed.Seed}", 133, 114, 6);
        Shared.Printc($"best of {bestOf}", 67, 121, 5);

        Shared.DrawCursor(cursorX, cursorY);
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
