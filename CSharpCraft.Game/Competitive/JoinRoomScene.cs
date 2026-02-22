using System;
using System.Runtime.CompilerServices;
using CSharpCraft.Competitive;
using CSharpCraft.Pico8;
using static CSharpCraft.Pico8.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RaceServer;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Competitive;

public class JoinRoomScene() : IScene, IDisposable
{
    public string SceneName { get => "0"; }
    public double Fps { get => 60.0; }
    public (int w, int h) Resolution { get => (128, 128); }

    private static Role role;

    private static bool joinedRoom;
    private string prompt;

    private float cursorX;
    private float cursorY;
    private KeyboardState prevKeyboardState;
    private MouseState prevMouseState;

    private Button joinAs;
    private Button roleBtn;

    private bool isInitializing;
    private bool isInitialized;

    public async void Init()
    {
        if (isInitializing) return;
        isInitializing = true;

        try
        {

            await AccountHandler.ConnectToServer();
            if (!AccountHandler._isLoggedIn)
            {
                ScheduleScene(() => new LoginScene(this));
                return;
            }

            role = Role.Player;
            joinedRoom = false;
            prompt = "";
            joinAs = new((34, 59), "join as", true);
            roleBtn = new((75, 59), "  ", true);

            prevKeyboardState = Keyboard.GetState();
            prevMouseState = Mouse.GetState();
            (cursorX, cursorY) = GameRendering.Current.GetCursorPosition(prevMouseState.X, prevMouseState.Y);

            isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing CompetitiveScene: {ex.Message}");
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

        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        (cursorX, cursorY) = GameRendering.Current.GetCursorPosition(mouseState.X, mouseState.Y);

        if (!joinedRoom)
        {
            joinAs.Update(cursorX, cursorY);
            roleBtn.Update(cursorX, cursorY);

            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton != ButtonState.Pressed)
            {
                if (joinAs.IsHovered)
                {
                    if (string.IsNullOrEmpty(AccountHandler._myself.Username))
                    {
                        prompt = "Please log in first";
                        return;
                    }

                    joinedRoom = true;
                    ScheduleScene(() => new LobbyScene(role));
                }
                else if (roleBtn.IsHovered)
                {
                    role = role == Role.Player ? Role.Spectator : Role.Player;
                }
            }
        }

        prevKeyboardState = keyboardState;
        prevMouseState = mouseState;
    }

    public void Draw()
    {
        GameRendering.Current.ClearDevice(Color.Black);

        Rectfill(0, 0, 127, 127, 17);

        if (!isInitialized || isInitializing) { Shared.Printc("loading...", 64, 61, 15); return; }

        if (!joinedRoom)
        {
            joinAs.Draw();
            roleBtn.Draw();
            GameRendering.Current.Draw($"{role}Icon", new Vector2(80.25f * CellWidth, (role == Role.Player ? 61 : 60.75f) * CellHeight), Color.White, CellWidth / 2f, CellHeight / 2f);
            
            if (!string.IsNullOrEmpty(prompt))
            {
                Print(prompt, 64 - prompt.Length * 2, 70, 8);
            }
        }
        else
        {
            Printc("players in room", 64, 5, 8);
            int i = 0;
            foreach (RoomUser player in RoomHandler._playerDictionary.Values)
            {
                Print(player.Name, 34, 13 + i * 6, 8);
                i++;
            }
        }

        Shared.DrawCursor(cursorX, cursorY);
    }

    private void Printc(string t, int x, int y, int c)
    {
        Print(t, x - t.Length * 2, y, c);
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
