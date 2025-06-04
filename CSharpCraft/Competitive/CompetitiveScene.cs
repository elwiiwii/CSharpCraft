using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Competitive;

public class CompetitiveScene : IScene
{
    public string SceneName { get => "competitive"; }
    public double Fps { get => 60.0; }
    private Pico8Functions p8;
    private Icon back;
    private Icon ranked;
    private Icon speedrun;
    private Icon unranked;
    private Icon @private;
    private Icon replays;
    private Icon statistics;
    private Icon search;
    private Icon profile;
    private Icon settings;
    private float labelLength;
    private bool isInitialized = false;
    private bool isInitializing = false;

    private Icon[] icons;
    private Icon? curIcon;
    private float cursorX;
    private float cursorY;
    private MouseState prevState;

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

            back = new() { StartPos = (120, 3), EndPos = (125, 10), Label = "back", ShadowTexture = "BackShadow", IconTexture = "BackIcon", Scene = p8.TitleScreen };
            ranked = new() { StartPos = (30, 32), EndPos = (61, 63), Label = "ranked", ShadowTexture = "ModeShadow", IconTexture = "RankedIcon", Scene = new RankedScene(this) };
            speedrun = new() { StartPos = (66, 32), EndPos = (97, 63), Label = "speedrun", ShadowTexture = "ModeShadow", IconTexture = "SpeedrunIcon", Scene = new SpeedrunScene(this) };
            unranked = new() { StartPos = (30, 68), EndPos = (61, 99), Label = "unranked", ShadowTexture = "ModeShadow", IconTexture = "UnrankedIcon", Scene = new UnrankedScene(this) };
            @private = new() { StartPos = (66, 68), EndPos = (97, 99), Label = "private", ShadowTexture = "ModeShadow", IconTexture = "PrivateIcon", Scene = new PrivateScene(this) };
            replays = new() { StartPos = (111, 46), EndPos = (125, 59), Label = "replays", ShadowTexture = "ReplaysShadow", IconTexture = "ReplaysIcon", Scene = new ReplaysScene(this) };
            statistics = new() { StartPos = (111, 63), EndPos = (125, 75), Label = "statistics", ShadowTexture = "StatisticsShadow", IconTexture = "StatisticsIcon", Scene = new StatisticsScene(this) };
            search = new() { StartPos = (112, 78), EndPos = (124, 90), Label = "search", ShadowTexture = "SearchShadow", IconTexture = "SearchIcon", Scene = new SearchScene(this) };
            profile = new() { StartPos = (112, 93), EndPos = (124, 108), Label = "profile", ShadowTexture = "ProfileShadow", IconTexture = "ProfileIcon", Scene = new ProfileScene(this, AccountHandler._myself.Username) };
            settings = new() { StartPos = (111, 111), EndPos = (125, 125), Label = "settings", ShadowTexture = "SettingsShadow", IconTexture = "SettingsIcon", Scene = new SettingsScene(this) };
            icons = [back, ranked, speedrun, unranked, @private, replays, statistics, search, profile, settings];

            labelLength = 0;
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

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);

        p8.Batch.Draw(p8.TextureDictionary["CompetitiveBackground"], new(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        Shared.DrawIcons(p8, icons, cursorX, cursorY);

        if (curIcon is not null) { labelLength = curIcon.Label.Length * 4; }
        else { labelLength = Math.Max(labelLength - labelLength / 4, 0); }
        p8.Batch.Draw(p8.TextureDictionary["12pxHighlightCenter"], new((63 - labelLength / 2) * p8.Cell.Width, 108 * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, new Vector2(p8.Cell.Width * (labelLength + 1), p8.Cell.Height), SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary["12pxHighlightEdge"], new((63 - 5 - labelLength / 2 - 1) * p8.Cell.Width, 108 * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary["12pxHighlightEdge"], new((63 + labelLength / 2 + 1) * p8.Cell.Width, 108 * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
        if (curIcon is not null) { Shared.Printcb(p8, curIcon.Label, 63, 111, 15, 1); }

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
        AccountHandler.DisconnectFromServer();
    }

}
