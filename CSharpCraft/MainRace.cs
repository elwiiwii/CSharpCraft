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

        public string GameModeName { get => "race"; }

        public async void Init()
        {
            channel = GrpcChannel.ForAddress("https://localhost:5072");
            service = new GameService.GameServiceClient(channel);
        }

        public async void Update()
        {
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Q))
            {
                CancellationTokenSource.Cancel(); // Cancel the listening task
                RoomJoiningStream?.Dispose(); // Dispose of the stream
            }

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
                else if (key != Keys.Back)
                {
                    userName.Value += key.ToString();
                }
            }


        }

        public void Draw()
        {
            p8.Cls();
            if (!joinedRoom)
            {
                p8.Print("enter your name", 1, 1, 8);
                p8.Print(userName.Value, 1, 6, 8);
            }
            else
            {
                p8.Print("players in room", 1, 1, 8);
                int i = 0;
                foreach (var player in playerDictionary.Values)
                {
                    p8.Print(player, 1, 6 + (i * 6), 8);
                    i++;
                }
            }
            
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
