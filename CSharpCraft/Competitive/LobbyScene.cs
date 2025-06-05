using System.Data;
using CSharpCraft.Competitive;
using CSharpCraft.Pcraft;
using CSharpCraft.Pico8;
using FixMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RaceServer;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Competitive;

public class LobbyScene(string joinRole) : PcraftBase
{
#nullable enable
    private List<Item> actionsItems = new();
    private List<Item> rulesItems = new();
    private string roomName;
    private string roomPassword;
#nullable disable

    public override string SceneName { get => "1"; }
    public override double Fps { get => 60.0; }
    private KeyboardState prevKeyboardState;
    private MouseState prevMouseState;
    private float cursorX;
    private float cursorY;
    private bool isInitializing;
    private bool isInitialized;
    private int listSel;
    private int actionsSel;
    private int rulesSel;
    private PlayerList playerList;
    private RoomSettings actionsSettings;
    private RoomSettings rulesSettings;

    public override async void Init(Pico8Functions pico8)
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

            // Connect to room and wait for initial room state
            bool connected = await RoomHandler.JoinRoom(AccountHandler._myself.Username, joinRole);
            if (!connected)
            {
                Console.WriteLine("Failed to connect to room");
                p8.ScheduleScene(() => new LoginScene(new CompetitiveScene()));
                return;
            }

            roomName = "test room";
            roomPassword = "????";

            playerList = new(p8, roomName, roomPassword, 5);
            actionsSettings = new(p8, (5, 83), "actions", actionsItems);
            rulesSettings = new(p8, (64, 83), "rules", rulesItems);

            prevKeyboardState = Keyboard.GetState();
            prevMouseState = Mouse.GetState();
            cursorX = prevMouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
            cursorY = prevMouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);
            base.Init(p8);
            ResetLevel();

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

    public override async void Update()
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

        bool allReady = true;
        foreach (KeyValuePair<int, RoomUser> player in RoomHandler._playerDictionary)
        {
            if (!player.Value.Ready) { allReady = false; }
        }

        actionsItems.Clear();
        actionsItems.Add(new Item(RoomHandler._myself.Ready ? "unready" : "ready", RoomHandler._myself.Role == "Player", RoomHandler.PlayerReady));
        actionsItems.Add(new Item("start match", RoomHandler._myself.Host && allReady, RoomHandler.StartMatch));
        actionsItems.Add(new Item("leave room", true, RoomHandler.LeaveRoom));
        actionsItems.Add(new Item("change role", true, RoomHandler.ChangeRole));
        actionsItems.Add(new Item("change host", RoomHandler._myself.Host, RoomHandler.ChangeHost));
        actionsItems.Add(new Item("seeding", RoomHandler._myself.Host, RoomHandler.Seeding));
        actionsItems.Add(new Item("settings", true, RoomHandler.Settings));
        actionsItems.Add(new Item("password", RoomHandler._myself.Host, RoomHandler.Password));

        rulesItems.Clear();
        rulesItems.Add(new Item("best of:5", RoomHandler._myself.Host));
        rulesItems.Add(new Item("mode:any%", RoomHandler._myself.Host));
        rulesItems.Add(new Item("finishers:1", RoomHandler._myself.Host));
        rulesItems.Add(new Item("unbans:on", RoomHandler._myself.Host));
        rulesItems.Add(new Item("adv:0-0", RoomHandler._myself.Host));

        playerList.Update(mouseState, prevMouseState);
        actionsSettings.Update(mouseState, prevMouseState);
        rulesSettings.Update(mouseState, prevMouseState);

        ///if (p8.Btnp(0)) { actionsMenu.Active = true; rulesMenu.Active = false; }
        ///if (p8.Btnp(1)) { rulesMenu.Active = true; actionsMenu.Active = false; }
        ///
        ///if (actionsMenu.Active)
        ///{
        ///    if (p8.Btnp(2)) { actionsMenu.Sel -= 1; }
        ///    if (p8.Btnp(3)) { actionsMenu.Sel += 1; }
        ///    actionsMenu.Sel = actionsMenu.Sel < 0 ? 0 : actionsMenu.Sel >= actionsMenu.Items.Count ? actionsMenu.Items.Count - 1 : actionsMenu.Sel;
        ///
        ///    if (p8.Btnp(5) && actionsMenu.Items[actionsMenu.Sel].Active) { await actionsMenu.Items[actionsMenu.Sel].Method(); }
        ///}
        ///else if (rulesMenu.Active)
        ///{
        ///    if (p8.Btnp(2)) { rulesMenu.Sel -= 1; }
        ///    if (p8.Btnp(3)) { rulesMenu.Sel += 1; }
        ///    rulesMenu.Sel = rulesMenu.Sel < 0 ? 0 : rulesMenu.Sel >= rulesMenu.Items.Count ? rulesMenu.Items.Count - 1 : rulesMenu.Sel;
        ///
        ///    if (p8.Btnp(5) && rulesMenu.Items[rulesMenu.Sel].Active) { await rulesMenu.Items[rulesMenu.Sel].Method(); }
        ///}


        if (p8.Btnp(4)) { p8.ScheduleScene(() => new PickBanScene()); }

        //if (p8.Btnp(5) && RoomHandler.myself.Role == "Player")
        //{
        //    await PlayerReady();
        //}

        prevKeyboardState = keyboardState;
        prevMouseState = mouseState;
    }

    public override void Draw()
    {
        p8.Cls();

        if (!isInitialized || isInitializing) return;

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        Vector2 halfSize = new(p8.Cell.Width / 2f, p8.Cell.Height / 2f);

        p8.Camera(clx - 64, cly - 64);
        DrawBack();
        p8.Camera();

        playerList.Draw();
        actionsSettings.Draw();
        rulesSettings.Draw();

        Shared.DrawCursor(p8, cursorX, cursorY);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
