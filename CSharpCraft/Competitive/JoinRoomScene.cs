using System;
using System.Runtime.CompilerServices;
using CSharpCraft.Competitive;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RaceServer;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.RaceMode;

public class JoinRoomScene() : IScene, IDisposable
{
#nullable enable
    private static ConcurrentString role = new();
    
    private static bool joinedRoom;
    private string prompt;

    private float cursorX;
    private float cursorY;
    private KeyboardState prevKeyboardState;
    private MouseState prevMouseState;

    private Button joinAs = new((34, 59), "join as", true);
    private Button roleBtn = new((75, 59), "  ", true);

#nullable disable

    public string SceneName { get => "0"; }
    public double Fps { get => 60.0; }
    private Pico8Functions p8;
    private bool isInitializing;
    private bool isInitialized;

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

            role = new();
            role.Value = "Player";
            joinedRoom = false;
            prompt = "";
            prevKeyboardState = Keyboard.GetState();
            prevMouseState = Mouse.GetState();
            cursorX = prevMouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
            cursorY = prevMouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

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

    public async void Update()
    {
        if (!isInitialized || isInitializing) return;

        KeyboardState keyboardState = Keyboard.GetState();
        MouseState mouseState = Mouse.GetState();
        cursorX = mouseState.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        cursorY = mouseState.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

        if (!joinedRoom)
        {
            joinAs.Update(p8, cursorX, cursorY);
            roleBtn.Update(p8, cursorX, cursorY);

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
                    p8.LoadCart(new LobbyScene(role.Value));
                }
                else if (roleBtn.IsHovered)
                {
                    role.Value = role.Value == "Player" ? "Spectator" : "Player";
                }
            }
        }

        prevKeyboardState = keyboardState;
        prevMouseState = mouseState;
    }

    public void Draw()
    {
        p8.Batch.GraphicsDevice.Clear(Color.Black);
        p8.Rectfill(0, 0, 127, 127, 17);

        if (!isInitialized || isInitializing) return;

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        Vector2 halfSize = new(p8.Cell.Width / 2f, p8.Cell.Height / 2f);

        if (!joinedRoom)
        {
            joinAs.Draw(p8);
            roleBtn.Draw(p8);
            p8.Batch.Draw(p8.TextureDictionary[$"{role.Value}Icon"], new Vector2(80.25f * p8.Cell.Width, (role.Value == "Player" ? 61 : 60.75f) * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
            
            if (!string.IsNullOrEmpty(prompt))
            {
                p8.Print(prompt, 64 - prompt.Length * 2, 70, 8);
            }
        }
        else
        {
            Printc("players in room", 64, 5, 8);
            int i = 0;
            foreach (RoomUser player in RoomHandler._playerDictionary.Values)
            {
                p8.Print(player.Name, 34, 13 + i * 6, 8);
                i++;
            }
        }

        Shared.DrawCursor(p8, cursorX, cursorY);
    }

    private void Printc(string t, int x, int y, int c)
    {
        p8.Print(t, x - t.Length * 2, y, c);
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
