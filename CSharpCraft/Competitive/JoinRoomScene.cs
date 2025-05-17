using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using RaceServer;
using CSharpCraft.Pico8;

namespace CSharpCraft.RaceMode;

public class JoinRoomScene(MainRace mainRace) : IScene, IDisposable
{
#nullable enable
    private static ConcurrentString userName = new();
    private static ConcurrentString role = new();
    //private static ConcurrentString joinMessage = new();
    
    private static bool joinedRoom;
    private KeyboardState prevState;
    private int menuState;
    private string prompt;
#nullable disable

    public string SceneName { get => "0"; }
    private Pico8Functions p8;

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        mainRace.cancellationTokenSource = new();
        userName = new();
        role = new();
        role.Value = "Player";
        //joinMessage = new();
        mainRace.playerDictionary = new();
        joinedRoom = false;
        prevState = Keyboard.GetState();
        menuState = 0;
        prompt = "";
    }

    private void TypingHandling(Keys key, ConcurrentString @string)
    {
        if (key == Keys.Back && !prevState.IsKeyDown(Keys.Back) && @string.Value.Length > 0)
        {
            @string.Value = @string.Value.Substring(0, @string.Value.Length - 1);
        }
        else if ((KeyNames.keyNames[key.ToString()].Length == 1 || key == Keys.Space) && @string.Value.Length < 10)
        {
            bool keyMatch = false;
            foreach (Keys prevKey in prevState.GetPressedKeys())
            {
                if (key == prevKey) { keyMatch = true; break; }
            }
            if (!keyMatch)
            {
                if (key != Keys.Space)
                {
                    @string.Value += KeyNames.keyNames[key.ToString()];
                }
                else
                {
                    @string.Value = @string.Value.PadRight(@string.Value.Length + 1);
                }
            }
        }
    }

    public async void Update()
    {
        KeyboardState state = Keyboard.GetState();

        if (!joinedRoom)
        {
            switch (menuState)
            {
                case 0:
                    prompt = "enter your name";
                    foreach (Keys key in state.GetPressedKeys())
                    {
                        TypingHandling(key, userName);
                    }
                    if (state.IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter) && userName.Value.Length > 0)
                    {
                        menuState = 1;
                    }

                    break;
                case 1:
                    prompt = "choose your role";
                    if (p8.Btnp(2) || p8.Btnp(3))
                    {
                        role.Value = role.Value == "Player" ? "Spectator" : "Player";
                    }
                    else if (state.IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter))
                    {
                        menuState = 2;
                    }
                    break;
                case 2:
                    prompt = "join the room";
                    if (p8.Btnp(0))
                    {
                        menuState = 3;
                    }
                    else if (state.IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter))
                    {
                        joinedRoom = true;
                        await RoomStream();
                        await JoinRoom();
                        p8.LoadCart(new LobbyScene(mainRace));
                    }
                    break;
                case 3:
                    prompt = "return to menu";
                    if (p8.Btnp(1))
                    {
                        menuState = 2;
                    }
                    else if (state.IsKeyDown(Keys.Enter) && !prevState.IsKeyDown(Keys.Enter))
                    {
                        mainRace.cancellationTokenSource.Cancel(); // Cancel the listening task
                        mainRace.roomStream?.Dispose(); // Dispose of the stream
                        p8.LoadCart(new TitleScreen(false));
                        return;
                    }
                    break;
            }
        }

        prevState = state;
    }

    public void Draw()
    {
        p8.Cls();

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);
        Vector2 halfSize = new(p8.Cell.Width / 2f, p8.Cell.Height / 2f);

        if (!joinedRoom)
        {
            p8.Batch.Draw(p8.TextureDictionary["SelectorHalf"], new Vector2(21 * p8.Cell.Width, 59 * p8.Cell.Height), null, p8.Colors[menuState == 0 ? 7 : 6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["SelectorHalf"], new Vector2(63 * p8.Cell.Width, 59 * p8.Cell.Height), null, p8.Colors[menuState == 0 ? 7 : 6], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
            p8.Rectfill(44, 59, 62, 67, menuState == 0 ? 7 : 6);
            p8.Rectfill(44, 60, 62, 66, 0);
            Printc(prompt, 64, 52, 7);
            Printc(userName.Value, 54, 61, 13);

            p8.Batch.Draw(p8.TextureDictionary["SmallSelector"], new Vector2(42 * p8.Cell.Width, 70 * p8.Cell.Height), null, p8.Colors[menuState == 3 ? 7 : 6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Print("back", 45, 72, menuState == 3 ? 7 : 6);

            p8.Batch.Draw(p8.TextureDictionary["SmallSelector"], new Vector2(65 * p8.Cell.Width, 70 * p8.Cell.Height), null, p8.Colors[menuState == 2 ? 7 : 6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Print("join", 68, 72, menuState == 2 ? 7 : 6);

            p8.Batch.Draw(p8.TextureDictionary["SmallSelector"], new Vector2(88 * p8.Cell.Width, 59 * p8.Cell.Height), null, p8.Colors[menuState == 1 ? 7 : 6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary[$"{role.Value}Icon"], new Vector2(91 * p8.Cell.Width, 61 * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["Arrow"], new Vector2(101 * p8.Cell.Width, 65 * p8.Cell.Height), null, p8.Colors[menuState == 1 ? 7 : 6], -1.57f, Vector2.Zero, size, SpriteEffects.None, 0);
        }
        else
        {
            Printc("players in room", 64, 5, 8);
            int i = 0;
            foreach (RoomUser player in mainRace.playerDictionary.Values)
            {
                p8.Print(player.Name, 34, 13 + i * 6, 8);
                i++;
            }
        }

    }

    private void Printc(string t, int x, int y, int c)
    {
        p8.Print(t, x - t.Length * 2, y, c);
    }

    private async Task JoinRoom()
    {
        JoinRoomResponse response = mainRace.service.JoinRoom(new JoinRoomRequest { Name = userName.Value, Role = role.Value });
        mainRace.myself = new RoomUser { Name = response.Name, Role = response.Role, Host = response.Host };
    }

    private async Task RoomStream()
    {
        //Console.WriteLine("Join as (1) Player or (2) Spectator?");
        //string role = Console.ReadKey().KeyChar == '1' ? "Player" : "Spectator";

        mainRace.roomStream = mainRace.service.RoomStream(new RoomStreamRequest { Name = userName.Value, Role = role.Value });

        // Run the listening logic in a separate task

        _ = Task.Run(mainRace.ReadRoomStream, mainRace.cancellationTokenSource.Token);
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
