using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Grpc.Core;
using Grpc.Net.Client;
using RaceServer;
using System.Collections.Concurrent;
using CSharpCraft.Pico8;
using System.Collections.Generic;

namespace CSharpCraft.RaceMode
{
    public class JoinRoomScene(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, List<IGameMode> raceScenes, MainRace mainRace, TitleScreen titleScreen) : IGameMode
    {
#nullable enable
        //TODO channel is disposable
        private static GrpcChannel channel;
        private static GameService.GameServiceClient service;

        private static ConcurrentString userName = new();
        private static ConcurrentString role = new();
        //private static ConcurrentString joinMessage = new();
        
        private static bool joinedRoom;
        private KeyboardState prevState;
        private int menuState;
        private string prompt;
#nullable disable

        public string GameModeName { get => "0"; }

        public void Init()
        {
            mainRace.CancellationTokenSource = new();
            channel = GrpcChannel.ForAddress("https://localhost:5072");
            service = new GameService.GameServiceClient(channel);
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
            if (key == Keys.Back && @string.Value.Length > 0)
            {
                @string.Value = @string.Value.Substring(0, @string.Value.Length - 1);
            }
            else if ((KeyNames.keyNames[key.ToString()].Length == 1 || key == Keys.Space) && @string.Value.Length < 10)
            {
                var keyMatch = false;
                foreach (var prevKey in prevState.GetPressedKeys())
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
                        foreach (var key in state.GetPressedKeys())
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
                            await JoinRoom();
                            mainRace.currentScene = 1;
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
                            mainRace.CancellationTokenSource.Cancel(); // Cancel the listening task
                            mainRace.RoomJoiningStream?.Dispose(); // Dispose of the stream
                            titleScreen.currentGameMode = 0;
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

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);
            Vector2 halfSize = new(cellW / 2, cellH / 2);

            if (!joinedRoom)
            {
                batch.Draw(textureDictionary["SelectorHalf"], new Vector2(21 * cellW, 59 * cellH), null, p8.colors[menuState == 0 ? 7 : 6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["SelectorHalf"], new Vector2(63 * cellW, 59 * cellH), null, p8.colors[menuState == 0 ? 7 : 6], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
                p8.Rectfill(44, 59, 62, 67, menuState == 0 ? 7 : 6);
                p8.Rectfill(44, 60, 62, 66, 0);
                Printc(prompt, 64, 52, 7);
                Printc(userName.Value, 54, 61, 13);

                batch.Draw(textureDictionary["SmallSelector"], new Vector2(42 * cellW, 70 * cellH), null, p8.colors[menuState == 3 ? 7 : 6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                p8.Print("back", 45, 72, menuState == 3 ? 7 : 6);

                batch.Draw(textureDictionary["SmallSelector"], new Vector2(65 * cellW, 70 * cellH), null, p8.colors[menuState == 2 ? 7 : 6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                p8.Print("join", 68, 72, menuState == 2 ? 7 : 6);

                batch.Draw(textureDictionary["SmallSelector"], new Vector2(88 * cellW, 59 * cellH), null, p8.colors[menuState == 1 ? 7 : 6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary[$"{role.Value}Icon"], new Vector2(91 * cellW, 61 * cellH), null, Color.White, 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["Arrow"], new Vector2(101 * cellW, 65 * cellH), null, p8.colors[menuState == 1 ? 7 : 6], -1.57f, Vector2.Zero, size, SpriteEffects.None, 0);
            }
            else
            {
                Printc("players in room", 64, 5, 8);
                int i = 0;
                foreach (var player in mainRace.playerDictionary.Values)
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
            //Console.WriteLine("Join as (1) Player or (2) Spectator?");
            //var role = Console.ReadKey().KeyChar == '1' ? "Player" : "Spectator";

            mainRace.RoomJoiningStream = service.JoinRoom(new JoinRoomRequest { Name = userName.Value, Role = role.Value });

            // Run the listening logic in a separate task

            _ = Task.Run(ReadRoomJoiningStream, mainRace.CancellationTokenSource.Token);
        }

        private async Task ReadRoomJoiningStream()
        {
            await foreach (var response in mainRace.RoomJoiningStream.ResponseStream.ReadAllAsync(mainRace.CancellationTokenSource.Token))
            {
                // response.Message needs to be written to a ConcurrentString (see ConcurrentString.cs), which the draw method can draw from
                //joinMessage.Value = response.Message; // roomJoiningMessage is displayed later in 'draw'
                //Console.WriteLine(response.Message);

                // this would be in 'draw'
                //Console.WriteLine("Players in the room:");

                // this would write to a ConcurrentDictionary of players, which the draw method can draw from
                mainRace.playerDictionary.Clear();
                var dummyIndex = 1;
                foreach (var player in response.Users)
                {
                    //Console.WriteLine(player);
                    mainRace.playerDictionary.TryAdd(dummyIndex, player);
                    dummyIndex++;
                }

                //Console.WriteLine("Spectators in the room:");
                //foreach (var spectator in response.Spectators)
                //{
                //    Console.WriteLine(spectator);
                //}
            }
        }

        private static async Task StartGame(GameService.GameServiceClient client)
        {
            throw new NotImplementedException();
        }

        private static async Task QuitGame(GameService.GameServiceClient client)
        {
            throw new NotImplementedException();
        }

    }

}
