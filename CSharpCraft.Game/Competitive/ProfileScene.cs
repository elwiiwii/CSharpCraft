using AccountService;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Competitive;

public class ProfileScene(IScene prevScene, string username) : IScene
{
    public string SceneName { get => "profile"; }
    public double Fps { get => 60.0; }
    public (int w, int h) Resolution { get => (128, 128); }
    private Pico8Functions p8;
    private Icon back;

    private Icon? curIcon;
    private float cursorX;
    private float cursorY;
    private MouseState prevState;

    private GetUserResponse user;

    public async void Init(Pico8Functions pico8)
    {
        p8 = pico8;
        back = new() { StartPos = (120, 3), EndPos = (125, 10), Label = "back", ShadowTexture = "BackShadow", IconTexture = "BackIcon", Scene = prevScene };

        user = await AccountHandler.GetUserByUsername(username);
        curIcon = null;
        prevState = Mouse.GetState();
        cursorX = prevState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        cursorY = prevState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);
    }

    public void Update()
    {
        MouseState state = Mouse.GetState();
        cursorX = state.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        cursorY = state.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

        curIcon = Shared.UpdateIcon(p8, [back], cursorX, cursorY);

        if (state.LeftButton == ButtonState.Pressed && prevState.LeftButton == ButtonState.Released && curIcon is not null && curIcon.Scene is not null) { p8.ScheduleScene(() => curIcon.Scene); }
        prevState = state;
    }

    public void Draw()
    {
        p8.Batch.GraphicsDevice.Clear(Color.Black);

        p8.Rectfill(0, 0, 127, 127, 17);

        if (user is not null && user.Success)
        {
            p8.Rectfill(3, 3, 34, 34, Pico8Utils.HexToColor(user.BackgroundColor));
            for (int i = 0; i < user.HexCodes.Count; i++)
            {
                p8.Pal(p8.Colors[i + 1], Pico8Utils.HexToColor(user.HexCodes[i]));
            }
            int lastRowIndex = user.ProfilePicture % (p8.TextureDictionary["PfpIcons"].Width / 32);
            p8.Spr(user.ProfilePicture * 16 - lastRowIndex * 12, 3, 3, 4, 4);
            p8.Pal();
            p8.Rect(3, 3, 34, 34, Pico8Utils.HexToColor(user.OutlineColor));

            p8.PrintBig(user.Username, 41, 6, Pico8Utils.HexToColor(user.ShadowColor));
            p8.PrintBig(user.Username, 40, 5, Pico8Utils.HexToColor(user.NameColor));


        }
        else if (user is not null && !user.Success)
        {
            Shared.Printc(p8, "user not found", 64, 62, 15);
        }
        else
        {
            Shared.Printc(p8, "loading...", 64, 62, 15);
        }

        Shared.DrawIcons(p8, [back], cursorX, cursorY);

        Shared.DrawCursor(p8, cursorX, cursorY);
    }
    public string SpriteImage => "PfpIcons";
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
