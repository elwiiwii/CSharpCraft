using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using RaceServer;
using System.Threading.Channels;
using System.Collections.Concurrent;

namespace CSharpCraft
{
    public class MainRace(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice) : IGameMode
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        private static AsyncServerStreamingCall<JoinRoomResponse> RoomJoiningStream;
        //TODO channel is disposable
        private static GrpcChannel channel;
        private static GameService.GameServiceClient service;

        private static ConcurrentString userName = new();
        private static ConcurrentString joinMessage = new();
        private static ConcurrentDictionary<int, string> playerDictionary = new();

        private static bool joinedRoom;
        private KeyboardState prevState;

        public string GameModeName { get => "race"; }

        public async void Init()
        {
            channel = GrpcChannel.ForAddress("https://localhost:5072");
            service = new GameService.GameServiceClient(channel);
            prevState = Keyboard.GetState();
        }

        public async void Update()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.LeftShift) && state.IsKeyDown(Keys.Q))
            {
                CancellationTokenSource.Cancel(); // Cancel the listening task
                RoomJoiningStream?.Dispose(); // Dispose of the stream
            }
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => {
                CancellationTokenSource.Cancel(); // Cancel the listening task
                RoomJoiningStream?.Dispose(); // Dispose of the stream
            };

            foreach (var key in state.GetPressedKeys())
            {
                if (!joinedRoom && state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Enter))
                {
                    joinedRoom = true;
                    await JoinRoom();
                    break;
                }
                else if (key == Keys.Back && userName.Value.Length > 0)
                {
                    userName.Value = userName.Value.Substring(0, userName.Value.Length - 1);
                }
                else if (key.ToString().Length == 1 && userName.Value.Length < 15)
                {
                    var keyMatch = false;
                    foreach (var prevKey in prevState.GetPressedKeys())
                    {
                        if (key == prevKey) { keyMatch = true; break; }
                    }
                    if (!keyMatch) { userName.Value += key.ToString().ToLower(); }
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
                batch.Draw(textureDictionary["SelectorHalf"], new Vector2(31 * cellW, 59 * cellH), null, p8.colors[7], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["SelectorHalf"], new Vector2(73 * cellW, 59 * cellH), null, p8.colors[7], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
                p8.Rectfill(54, 59, 72, 67, 7);
                p8.Rectfill(54, 60, 72, 66, 0);
                Printc("enter your name", 64, 53, 7);
                Printc(userName.Value, 64, 61, 13);

                batch.Draw(textureDictionary["SmallSelector"], new Vector2(24 * cellW, 69 * cellH), null, p8.colors[6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                p8.Print("back", 27, 71, 6);

                batch.Draw(textureDictionary["SelectorHalf"], new Vector2(47 * cellW, 69 * cellH), null, p8.colors[6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["SelectorHalf"], new Vector2(57 * cellW, 69 * cellH), null, p8.colors[6], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
                p8.Print("join as", 50, 71, 6);

                batch.Draw(textureDictionary["SmallSelector"], new Vector2(82 * cellW, 69 * cellH), null, p8.colors[6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["SpectatorIcon"], new Vector2(85 * cellW, 71 * cellH), null, Color.White, 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["Arrow"], new Vector2(95 * cellW, 75 * cellH), null, p8.colors[6], -1.57f, Vector2.Zero, size, SpriteEffects.None, 0);
            }
            else
            {
                Printc("players in room", 64, 5, 8);
                int i = 0;
                foreach (var player in playerDictionary.Values)
                {
                    p8.Print(player, 34, 13 + (i * 6), 8);
                    i++;
                }
            }
            
        }

        private void Printc(string t, int x, int y, int c)
        {
            p8.Print(t, x - t.Length * 2, y, c);
        }

        private static async Task JoinRoom()
        {
            //Console.WriteLine("Join as (1) Player or (2) Spectator?");
            //var role = Console.ReadKey().KeyChar == '1' ? "Player" : "Spectator";

            RoomJoiningStream = service.JoinRoom(new JoinRoomRequest { UserName = userName.Value });

            // Run the listening logic in a separate task

            _ = Task.Run(ReadRoomJoiningStream, CancellationTokenSource.Token);
        }

        private static async Task ReadRoomJoiningStream()
        {
            await foreach (var response in RoomJoiningStream.ResponseStream.ReadAllAsync(CancellationTokenSource.Token))
            {
                // response.Message needs to be written to a ConcurrentString (see ConcurrentString.cs), which the draw method can draw from
                joinMessage.Value = response.Message; // roomJoiningMessage is displayed later in 'draw'
                //Console.WriteLine(response.Message);

                // this would be in 'draw'
                //Console.WriteLine("Players in the room:");

                // this would write to a ConcurrentDictionary of players, which the draw method can draw from
                playerDictionary.Clear();
                var dummyIndex = 1;
                foreach (var player in response.Players)
                {
                    Console.WriteLine(player);
                    playerDictionary.TryAdd(dummyIndex, player);
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
