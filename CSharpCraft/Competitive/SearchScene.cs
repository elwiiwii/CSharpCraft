using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Competitive;

public class SearchScene(IScene prevScene) : IScene
{
    public string SceneName { get => "ranked"; }
    public double Fps { get => 60.0; }
    private Pico8Functions p8;
    private Icon back;
    private Icon replays;
    private Icon statistics;
    private Icon search;
    private Icon profile;
    private Icon settings;

    private Icon[] icons;
    private Icon? curIcon;
    private float cursorX;
    private float cursorY;
    private bool isInitialized;
    private MouseState prevState;
    private bool isInitializing;

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
                p8.LoadCart(new LoginScene(this));
                return;
            }

            back = new() { StartPos = (120, 3), EndPos = (125, 10), Label = "back", ShadowTexture = "BackShadow", IconTexture = "BackIcon", Scene = prevScene };
            replays = new() { StartPos = (111, 46), EndPos = (125, 59), Label = "replays", ShadowTexture = "ReplaysShadow", IconTexture = "ReplaysIcon", Scene = new ReplaysScene(prevScene) };
            statistics = new() { StartPos = (111, 63), EndPos = (125, 75), Label = "statistics", ShadowTexture = "StatisticsShadow", IconTexture = "StatisticsIcon", Scene = new StatisticsScene(prevScene) };
            search = new() { StartPos = (112, 78), EndPos = (124, 90), Label = "search", ShadowTexture = "SearchShadow", IconTexture = "SearchIcon" };
            profile = new() { StartPos = (112, 93), EndPos = (124, 108), Label = "profile", ShadowTexture = "ProfileShadow", IconTexture = "ProfileIcon", Scene = new ProfileScene(prevScene, AccountHandler._myself.Username) };
            settings = new() { StartPos = (111, 111), EndPos = (125, 125), Label = "settings", ShadowTexture = "SettingsShadow", IconTexture = "SettingsIcon", Scene = new SettingsScene(prevScene) };
            icons = [back, replays, statistics, search, profile, settings];

            curIcon = null;
            prevState = Mouse.GetState();
            cursorX = prevState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
            cursorY = prevState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

            isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing CompetitiveScene: {ex.Message}");
            p8.LoadCart(new LoginScene(this));
        }
        finally
        {
            isInitializing = false;
        }
    }

    public void Update()
    {
        if (!isInitialized || isInitializing) return;

        MouseState state = Mouse.GetState();
        cursorX = state.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        cursorY = state.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

        curIcon = Shared.UpdateIcon(p8, icons, cursorX, cursorY);

        if (state.LeftButton == ButtonState.Pressed && prevState.LeftButton == ButtonState.Released && curIcon is not null && curIcon.Scene is not null) { p8.ScheduleScene(() => curIcon.Scene); }
        prevState = state;
    }

    public void Draw()
    {
        p8.Batch.GraphicsDevice.Clear(Color.Black);

        if (!isInitialized || isInitializing) return;

        p8.Rectfill(0, 0, 127, 127, 17);

        Shared.DrawIcons(p8, icons, cursorX, cursorY);

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
