using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RaceServer;

namespace CSharpCraft.Competitive;

public class PickBanScene : IScene, IDisposable
{
    public string SceneName { get => "1"; }
    public double Fps { get => 60.0; }
    public (int w, int h) Resolution { get => (192, 128); }
    private Pico8Functions p8;

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

    private class Game
    {
        public string Time { get; set; }
        public int SurfaceType { get; set; }
        public int CaveType { get; set; }
        public bool HigherWin { get; set; }
        public bool LowerWin { get; set; }
    }

    public async void Init(Pico8Functions pico8)
    {
        if (isInitializing) return;
        isInitializing = true;

        try
        {
            p8 = pico8;

            await AccountHandler.ConnectToServer();
            if (!AccountHandler._isLoggedIn)
            {
                p8.ScheduleScene(() => new LoginScene(new CompetitiveScene()));
                return;
            }

            prevKeyboardState = Keyboard.GetState();
            prevMouseState = Mouse.GetState();
            cursorX = prevMouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
            cursorY = prevMouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

            higherSeed = new RoomUser
            {
                Name = "__higher__seed__",
                Role = "Player",
                Host = false,
                Ready = true,
                Seed = 1
            };

            lowerSeed = new RoomUser
            {
                Name = "__lower___seed__",
                Role = "Player",
                Host = false,
                Ready = true,
                Seed = 2
            };

            score = (2, 2);
            bestOf = 7;
            gameCount = 5;
            advantage = (0, 0);

            games.Add(new Game
            {
                Time = "4:00.00",
                SurfaceType = 0,
                CaveType = 0,
                HigherWin = true
            });

            games.Add(new Game
            {
                Time = "4:00.00",
                SurfaceType = 1,
                CaveType = 1,
                LowerWin = true
            });

            games.Add(new Game
            {
                Time = "4:00.00",
                SurfaceType = 2,
                CaveType = 2,
                HigherWin = true
            });

            games.Add(new Game
            {
                Time = "4:00.00",
                SurfaceType = 3,
                CaveType = 3,
                LowerWin = true
            });

            isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing CompetitiveScene: {ex.Message}");
            if (ex.InnerException is not null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            p8.ScheduleScene(() => new LoginScene(this));
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
            p8.ScheduleScene(() => new PrivateScene(new CompetitiveScene()));
            return;
        }

        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        cursorX = mouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        cursorY = mouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

        prevKeyboardState = keyboardState;
        prevMouseState = mouseState;
    }

    public void Draw()
    {
        p8.Cls();

        if (!isInitialized || isInitializing) return;

        p8.Rectfill(0, 0, 191, 127, 17);

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        Vector2 halfSize = new(p8.Cell.Width / 2f, p8.Cell.Height / 2f);

        for (int i = 0; i < 5; i ++)
        {
            p8.Print($"game {gameCount - i}", 24, 6 + (gameCount - i - 1) * 20, 7);
            if (gameCount - i - 1 < games.Count)
            {
                p8.Print(games[gameCount - i - 1].Time, 24, 13 + (gameCount - i - 1) * 20, 7);
                if (games[gameCount - i - 1].HigherWin)
                {
                    Shared.Printc(p8, higherSeed.Name, 93, 6 + (gameCount - i - 1) * 20, 24);
                }
                else if (games[gameCount - i - 1].LowerWin)
                {
                    Shared.Printc(p8, lowerSeed.Name, 93, 6 + (gameCount - i - 1) * 20, 28);
                }
                else
                {
                    Shared.Printc(p8, "tied", 93, 6 + (gameCount - i - 1) * 20, 7);
                }
                Shared.Printc(p8, "victory", 93, 13 + (gameCount - i - 1) * 20, 7);
            }
            else
            {

            }
        }

        p8.Rectfill(133, 0, 135, 127, 13);
        p8.Rectfill(0, 104, 132, 104, 13);
        p8.Rectfill(0, 105, 134, 127, 17);
        p8.Rectfill(67, 105, 67, 113, 13);

        Shared.Printc(p8, higherSeed.Name, 34, 107, 24);
        p8.Print($"seed #{higherSeed.Seed}", 2, 114, 6);
        Shared.Printc(p8, $"{score.h}-{score.l}", 67, 114, 7);

        Shared.Printc(p8, lowerSeed.Name, 102, 107, 28);
        Shared.Printr(p8, $"seed #{lowerSeed.Seed}", 133, 114, 6);
        Shared.Printc(p8, $"stats  coming  soon", 67, 121, 5);

        Shared.DrawCursor(p8, cursorX, cursorY);
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
