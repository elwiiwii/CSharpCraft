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

namespace CSharpCraft.RaceMode;

public class LobbyScene(string joinRole) : PcraftBase
{
#nullable enable
    private Menu roomMenu = new();
    private Menu actionsMenu = new();
    private List<Item> actionsItems = new();
    private Menu rulesMenu = new();
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
    private int cameraStep = 0;
    private int rndEnemy;

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
                p8.LoadCart(new LoginScene(new CompetitiveScene()));
                return;
            }

            // Connect to room and wait for initial room state
            bool connected = await RoomHandler.JoinRoom(AccountHandler._myself.Username, joinRole);
            if (!connected)
            {
                Console.WriteLine("Failed to connect to room");
                p8.LoadCart(new LoginScene(new CompetitiveScene()));
                return;
            }

            roomName = "test room";
            roomPassword = "????";
            roomMenu = new Menu { Name = $"room name-{roomName}", Items = null, Xpos = 20, Ypos = 5, Width = 88, Height = 74 };
            actionsMenu = new Menu { Name = "actions", Items = actionsItems, Xpos = 5, Ypos = 82, Width = 53, Height = 41, Active = true };
            rulesMenu = new Menu { Name = "rules", Items = rulesItems, Xpos = 62, Ypos = 82, Width = 61, Height = 41, Active = false };
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
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            p8.LoadCart(new LoginScene(this));
        }
        finally
        {
            isInitializing = false;
        }
    }

    public override async void Update()
    {
        if (!isInitialized || isInitializing) return;

        if (RoomHandler._myself == null)
        {
            p8.LoadCart(new PrivateScene(new CompetitiveScene()));
            return;
        }

        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        cursorX = mouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        cursorY = mouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

        if (mouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue)
        {
            listSel = Math.Max(0, listSel - 1);
        }
        else if (mouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue)
        {
            listSel = Math.Min(Math.Max(0, RoomHandler._playerDictionary.Count - 7), listSel + 1);
        }

        bool allReady = true;
        foreach (KeyValuePair<int, RoomUser> player in RoomHandler._playerDictionary)
        {
            if (!player.Value.Ready) { allReady = false; }
        }

        actionsItems.Clear();
        actionsItems.Add(new Item { Name = RoomHandler._myself.Ready ? "unready" : "ready", Active = RoomHandler._myself.Role == "Player", Method = RoomHandler.PlayerReady });
        actionsItems.Add(new Item { Name = "start match", Active = RoomHandler._myself.Host && allReady, Method = RoomHandler.StartMatch });
        actionsItems.Add(new Item { Name = "leave room", Active = true, Method = RoomHandler.LeaveRoom });
        actionsItems.Add(new Item { Name = "change role", Active = true, Method = RoomHandler.ChangeRole });
        actionsItems.Add(new Item { Name = "change host", Active = RoomHandler._myself.Host, Method = RoomHandler.ChangeHost });
        actionsItems.Add(new Item { Name = "seeding", Active = RoomHandler._myself.Host, Method = RoomHandler.Seeding });
        actionsItems.Add(new Item { Name = "settings", Active = true, Method = RoomHandler.Settings });
        actionsItems.Add(new Item { Name = "password", Active = RoomHandler._myself.Host, Method = RoomHandler.Password });

        rulesItems.Clear();
        rulesItems.Add(new Item { Name = "best of:5", Active = RoomHandler._myself.Host });
        rulesItems.Add(new Item { Name = "mode:any%", Active = RoomHandler._myself.Host });
        rulesItems.Add(new Item { Name = "finishers:1", Active = RoomHandler._myself.Host });
        rulesItems.Add(new Item { Name = "unbans:on", Active = RoomHandler._myself.Host });
        rulesItems.Add(new Item { Name = "adv:0-0", Active = RoomHandler._myself.Host });

        if (p8.Btnp(0)) { actionsMenu.Active = true; rulesMenu.Active = false; }
        if (p8.Btnp(1)) { rulesMenu.Active = true; actionsMenu.Active = false; }

        if (actionsMenu.Active)
        {
            if (p8.Btnp(2)) { actionsMenu.Sel -= 1; }
            if (p8.Btnp(3)) { actionsMenu.Sel += 1; }
            actionsMenu.Sel = actionsMenu.Sel < 0 ? 0 : actionsMenu.Sel >= actionsMenu.Items.Count ? actionsMenu.Items.Count - 1 : actionsMenu.Sel;

            if (p8.Btnp(5) && actionsMenu.Items[actionsMenu.Sel].Active) { await actionsMenu.Items[actionsMenu.Sel].Method(); }
        }
        else if (rulesMenu.Active)
        {
            if (p8.Btnp(2)) { rulesMenu.Sel -= 1; }
            if (p8.Btnp(3)) { rulesMenu.Sel += 1; }
            rulesMenu.Sel = rulesMenu.Sel < 0 ? 0 : rulesMenu.Sel >= rulesMenu.Items.Count ? rulesMenu.Items.Count - 1 : rulesMenu.Sel;

            if (p8.Btnp(5) && rulesMenu.Items[rulesMenu.Sel].Active) { await rulesMenu.Items[rulesMenu.Sel].Method(); }
        }


        if (p8.Btnp(4)) { p8.LoadCart(new PickBanScene()); }

        //if (p8.Btnp(5) && RoomHandler.myself.Role == "Player")
        //{
        //    await PlayerReady();
        //}

        if (cameraStep % 2 == 0)
        {
            if (cameraStep % (15 * 60) == 0) rndEnemy = F32.FloorToInt(p8.Rnd(enemies.Count));
            nearEnemies = [];
            UpEnemies(F32.Neg1, F32.Neg1);
            clx = enemies[rndEnemy].X;
            cly = enemies[rndEnemy].Y;
        }

        cameraStep++;
        time += F32.FromDouble(1.0 / 60.0);

        prevKeyboardState = keyboardState;
        prevMouseState = mouseState;
    }

    private void Printc(string t, int x, int y, int c)
    {
        p8.Print(t, x - t.Length * 2, y, c);
    }

    private void DrawMenu(Menu menu, int sel, bool active)
    {
        p8.Rectfill(menu.Xpos + (menu.Width - menu.Name.Length * 4) / 2, menu.Ypos + 1, menu.Xpos - 1 + menu.Width - (menu.Width - menu.Name.Length * 4) / 2, menu.Ypos + 7, 13);
        p8.Print(menu.Name, menu.Xpos + 1 + (menu.Width - menu.Name.Length * 4) / 2, menu.Ypos + 2, 7);

        if (active)
        {
            p8.Rectfill(menu.Xpos, menu.Ypos + 10, menu.Xpos + menu.Width - 1, menu.Ypos + 16, 13);
            p8.Spr(68, menu.Xpos - 3, menu.Ypos + 10);
            p8.Spr(68, menu.Xpos + menu.Width - 5, menu.Ypos + 10, 1, 1, true);
        }

        for (int i = sel; i <= sel + 2; i++)
        {
            p8.Print(menu.Items[i].Name, menu.Xpos + 5, menu.Ypos + 11 + i * 7, menu.Items[i].Active ? 7 : 0);
        }
    }

    private void Panel(string name, int x, int y, int width, int height)
    {
        p8.Rectfill(x + 8, y + 8, x + width - 9, y + height - 9, 1);
        p8.Spr(66, x, y);
        p8.Spr(67, x + width - 8, y);
        p8.Spr(82, x, y + height - 8);
        p8.Spr(83, x + width - 8, y + height - 8);
        p8.Sspr(24, 32, 4, 8, x + 8, y, width - 16, 8);
        p8.Sspr(24, 40, 4, 8, x + 8, y + height - 8, width - 16, 8);
        p8.Sspr(16, 36, 8, 4, x, y + 8, 8, height - 16);
        p8.Sspr(24, 36, 8, 4, x + width - 8, y + 8, 8, height - 16);

        int hx = x + (width - name.Length * 4) / 2;
        p8.Rectfill(hx, y + 1, hx + name.Length * 4, y + 7, 13);
        p8.Print(name, hx + 1, y + 2, 7);
    }

    private void Selector(int x, int y, int width)
    {
        p8.Rectfill(x, y, x + width - 1, y + 6, 13);
        p8.Spr(68, x - 3, y);
        p8.Spr(68, x + width - 5, y, 1, 1, true);
    }

    private void List(Menu menu, int x, int y, int width, int height, int displayed)
    {
        Panel(menu.Name, x, y, width, height);

        int tlist = menu.Items.Count;
        if (tlist < 1)
        {
            return;
        }

        int sel = menu.Sel;
        if (menu.Off > Math.Max(0, sel - 2)) { menu.Off = Math.Max(0, sel - 2); }
        if (menu.Off < Math.Min(tlist, sel + 2) - displayed) { menu.Off = Math.Min(tlist, sel + 2) - displayed; }

        sel -= menu.Off;

        int debut = menu.Off + 1;
        int fin = Math.Min(menu.Off + displayed, tlist);

        int offset = 0;

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);

        if (menu.Sel > tlist - 3)
        {
            offset = 4;
            p8.Batch.Draw(p8.TextureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * p8.Cell.Width, (y + 10) * p8.Cell.Height), null, p8.Colors[13], 0, Vector2.Zero, size, SpriteEffects.FlipVertically, 0);
        }
        else if (menu.Sel > 1)
        {
            offset = 2;
            p8.Batch.Draw(p8.TextureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * p8.Cell.Width, (y + 9) * p8.Cell.Height), null, p8.Colors[13], 0, Vector2.Zero, size, SpriteEffects.FlipVertically, 0);
            p8.Batch.Draw(p8.TextureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * p8.Cell.Width, (y + height - 6) * p8.Cell.Height), null, p8.Colors[13], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }
        else
        {
            offset = 0;
            p8.Batch.Draw(p8.TextureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * p8.Cell.Width, (y + height - 7) * p8.Cell.Height), null, p8.Colors[13], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }

        int sely = y + offset + 4 + (sel + 1) * 7;
        if (menu.Active) { Selector(x, sely, width); }

        //p8.Rectfill(x + 1, sely, x + sx - 3, sely + 6, 13);
        //p8.Rectfill(menu.Xpos, sely, menu.Xpos + menu.Width - 1, sely + 6, 13);
        //p8.Spr(68, menu.Xpos - 3, sely);
        //p8.Spr(68, menu.Xpos + menu.Width - 5, sely, 1, 1, true);

        x += 5;
        y += 12;

        for (int i = debut - 1; i < fin; i++)
        {
            Item it = menu.Items[i];
            int py = y + offset + (i - menu.Off) * 7;
            p8.Print(it.Name, x, py, it.Active ? 7 : 0);
        }



        //p8.Spr(68, x - 8, sely);
        //p8.Spr(68, x + sx - 10, sely, 1, 1, true);
    }

    private void DrawList(int sel)
    {
        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        Vector2 halfSize = new(p8.Cell.Width / 2f, p8.Cell.Height / 2f);

        int menuWidth = 16;
        if (roomName.Length > menuWidth) { menuWidth = Math.Min(roomName.Length, 26); }
        foreach (var player in RoomHandler._playerDictionary.Values)
        {
            if (player.Name.Length + 10 > menuWidth) { menuWidth = Math.Min(player.Name.Length, 16) + 10; }
        }
        int x = 63 - menuWidth * 2 - 9;
        int y = 5;
        p8.Batch.Draw(p8.TextureDictionary["LobbyPlayerListContainer"], new Vector2(x * p8.Cell.Width, y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary["LobbyPlayerListContainer"], new Vector2((64 - x) * p8.Cell.Width, y * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
        p8.Rectfill(63 - roomName.Length * 2 - 1, y + 1, 63 + roomName.Length * 2 + 1, y + 7, 13);
        Shared.Printc(p8, roomName, 64, y + 2, 7);
        Shared.Printc(p8, $"password-{roomPassword}", 64, y + 12, 7);

        int i = 0;
        foreach (RoomUser player in RoomHandler._playerDictionary.Values)
        {
            if (i >= sel && i < sel + 7)
            {
                p8.Batch.Draw(p8.TextureDictionary[$"{player.Role}Icon"], new Vector2((x + 8) * p8.Cell.Width, (y + 21 + i * 8) * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
                p8.Print(player.Name, x + 19, y + 21 + i * 8, 7);
                if (player.Ready)
                {
                    p8.Batch.Draw(p8.TextureDictionary["Tick"], new Vector2((x + 20 + player.Name.Length * 4) * p8.Cell.Width, (y + 21 + i * 8) * p8.Cell.Height), null, p8.Colors[6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                }
                if (player.Host)
                {
                    p8.Print("[", 127 - x - 31, 26 + i * 8, 5);
                    p8.Print("host", 127 - x - 28, 26 + i * 8, 5);
                    p8.Print("]", 127 - x - 13, 26 + i * 8, 5);
                }
                //p8.Print("0", 84, 85, 13);
                //p8.Print("0", 89, 118, 13);
            }
            i++;
        }
        i = Math.Max(7, i - 7);
        int scrollBarX = 127 - x - 8;
        int scrollBarY = y + 19;
        p8.Rectfill(scrollBarX, scrollBarY, scrollBarX + 2, scrollBarY + 51, 13);
        p8.Pset(F32.FromInt(scrollBarX) + 2, F32.FromInt(scrollBarY) + 51, 1);
        double range = 48.0 / Math.Max(7, RoomHandler._playerDictionary.Count);
        p8.Rectfill(scrollBarX + 1, y + 20 + sel * range, scrollBarX + 1, y + 20 + (sel + 7) * range, 6);
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

        DrawList(listSel);
        p8.Batch.Draw(p8.TextureDictionary["LobbySettingsContainer"], new Vector2(5 * p8.Cell.Width, 83 * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary["LobbySettingsContainer"], new Vector2(20 * p8.Cell.Width, 83 * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
        p8.Batch.Draw(p8.TextureDictionary["LobbySettingsContainer"], new Vector2(64 * p8.Cell.Width, 83 * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        p8.Batch.Draw(p8.TextureDictionary["LobbySettingsContainer"], new Vector2(83 * p8.Cell.Width, 83 * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);

        //string roomName = "room name-????";
        //p8.Rectfill(64 - roomName.Length * 2, 6, 64 + roomName.Length * 2, 6 + 6, 13);
        //Printc(roomName, 65, 7, 7);
        //Printc("password-????", 65, 17, 7);

        //DrawMenu(actionsMenu, 1, true);
        //DrawMenu(rulesMenu, 1, false);

        ///Panel($"room name-{roomName}", roomMenu.Xpos, roomMenu.Ypos, roomMenu.Width, roomMenu.Height);
        ///Selector(roomMenu.Xpos, roomMenu.Ypos + 11, roomMenu.Width);
        ///Printc($"password-{roomPassword}", 65, roomMenu.Ypos + 12, 7);
        ///List(actionsMenu, actionsMenu.Xpos, actionsMenu.Ypos, actionsMenu.Width, actionsMenu.Height, 3);
        ///List(rulesMenu, rulesMenu.Xpos, rulesMenu.Ypos, rulesMenu.Width, rulesMenu.Height, 3);

        //string[] actionsList = ["ready", "start game", "leave room"];
        //DrawMenu(actionsMenu);
        //string actions = "actions";
        //p8.Rectfill(17, 83, 17 + actions.Length * 4, 83 + 6, 13);
        //p8.Print(actions, 18, 84, 7);

        //string[] rulesList = ["best of:5", "mode:any%", "finishers:1"];
        //DrawMenu("rules", rulesList, 62, 82, 61, 41);
        //string rules = "rules";
        //p8.Rectfill(82, 83, 82 + rules.Length * 4, 83 + 6, 13);
        //p8.Print(rules, 83, 84, 7);

        ///int i = 0;
        ///foreach (RoomUser player in RoomHandler._playerDictionary.Values)
        ///{
        ///    p8.Batch.Draw(p8.TextureDictionary[$"{player.Role}Icon"], new Vector2(25 * p8.Cell.Width, (26 + i * 7) * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
        ///    p8.Print(player.Name, 36, 26 + i * 7, 7);
        ///    if (player.Role == "Player" && player.Ready)
        ///    {
        ///        p8.Batch.Draw(p8.TextureDictionary["Tick"], new Vector2((37 + player.Name.Length * 4) * p8.Cell.Width, (26 + i * 7) * p8.Cell.Height), null, p8.Colors[6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        ///    }
        ///    if (player.Host)
        ///    {
        ///        p8.Print("[", 81, 26 + i * 7, 5);
        ///        p8.Print("host", 84, 26 + i * 7, 5);
        ///        p8.Print("]", 99, 26 + i * 7, 5);
        ///    }
        ///    //p8.Print("0", 84, 85, 13);
        ///    //p8.Print("0", 89, 118, 13);
        ///    i++;
        ///}
        ///
        Shared.DrawCursor(p8, cursorX, cursorY);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
