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
using System.Security.Principal;
using CSharpCraft.Pico8;

namespace CSharpCraft.RaceMode
{
    public class MainRace(List<IGameMode> raceScenes) : IGameMode
    {
        public string GameModeName { get => "race"; }

        //TODO channel is disposable
        public GrpcChannel channel;
        public GameService.GameServiceClient service;

        public CancellationTokenSource cancellationTokenSource = new();
        public AsyncServerStreamingCall<RoomStreamResponse> roomStream;
        public ConcurrentDictionary<int, RoomUser> playerDictionary = new();
        public RoomUser myself = new();

        public int currentScene;

        public void Init()
        {
            channel = GrpcChannel.ForAddress("https://localhost:5072");
            service = new GameService.GameServiceClient(channel);

            currentScene = 0;
            raceScenes[currentScene].Init();
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Q))
            {
                cancellationTokenSource.Cancel(); // Cancel the listening task
                roomStream?.Dispose(); // Dispose of the stream
            }
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                cancellationTokenSource.Cancel(); // Cancel the listening task
                roomStream?.Dispose(); // Dispose of the stream
            };

            currentScene = int.Parse(raceScenes[currentScene].GameModeName);
            raceScenes[currentScene].Update();
        }

        public void Draw()
        {
            raceScenes[currentScene].Draw();
        }


        public async Task ReadRoomStream()
        {
            await foreach (var response in roomStream.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
            {
                switch (response.MessageCase)
                {
                    case RoomStreamResponse.MessageOneofCase.JoinRoomNotification:
                        HandleJoinRoomNotification(response.JoinRoomNotification);
                        break;
                    case RoomStreamResponse.MessageOneofCase.PlayerReadyNotification:
                        HandlePlayerReadyNotification(response.PlayerReadyNotification);
                        break;
                    case RoomStreamResponse.MessageOneofCase.StartMatchNotification:
                        HandleStartMatchNotification(response.StartMatchNotification);
                        break;
                    case RoomStreamResponse.MessageOneofCase.UpdateSeedsNotification:
                        HandleUpdateSeedsNotification(response.UpdateSeedsNotification);
                        break;
                    case RoomStreamResponse.MessageOneofCase.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                // response.Message needs to be written to a ConcurrentString (see ConcurrentString.cs), which the draw method can draw from
                //joinMessage.Value = response.Message; // roomJoiningMessage is displayed later in 'draw'
                //Console.WriteLine(response.Message);

                // this would be in 'draw'
                //Console.WriteLine("Players in the room:");


                //mainRace.myself = response.Myself;

                // this would write to a ConcurrentDictionary of players, which the draw method can draw from
                //mainRace.playerDictionary.Clear();
                //var dummyIndex = 1;
                //foreach (var player in response.Users)
                //{
                //    //Console.WriteLine(player);
                //    mainRace.playerDictionary.TryAdd(dummyIndex, player);
                //    dummyIndex++;
                //}

                //Console.WriteLine("Spectators in the room:");
                //foreach (var spectator in response.Spectators)
                //{
                //    Console.WriteLine(spectator);
                //}
            }
        }

        private void HandleUpdateSeedsNotification(UpdateSeedsNotification updateSeedsNotification)
        {
            throw new NotImplementedException();
        }

        private void HandleStartMatchNotification(StartMatchNotification startMatchNotification)
        {
            if (startMatchNotification.MatchStarted)
            {
                currentScene = myself.Role == "Player" ? 2 : 2; //should be 2:3 when i make the spectator scene
                raceScenes[currentScene].Init();
            }
        }

        private void HandleJoinRoomNotification(JoinRoomNotification notification)
        {
            playerDictionary.Clear();
            var dummyIndex = 1;
            foreach (var player in notification.Users)
            {
                //Console.WriteLine(player);
                playerDictionary.TryAdd(dummyIndex, player);
                dummyIndex++;
            }
        }

        private void HandlePlayerReadyNotification(PlayerReadyNotification notification)
        {
            playerDictionary.Clear();
            var dummyIndex = 1;
            foreach (var player in notification.Users)
            {
                //Console.WriteLine(player);
                playerDictionary.TryAdd(dummyIndex, player);
                dummyIndex++;
            }
        }

    }

}
