using Microsoft.Xna.Framework.Input;
using Grpc.Core;
using Grpc.Net.Client;
using RaceServer;
using System.Collections.Concurrent;
using CSharpCraft.Pico8;

namespace CSharpCraft.RaceMode;

public static class RoomHandler
{
    public GrpcChannel channel;
    public GameService.GameServiceClient service;

    public CancellationTokenSource cancellationTokenSource = new();
    public AsyncServerStreamingCall<RoomStreamResponse> roomStream;
    public ConcurrentDictionary<int, RoomUser> playerDictionary = new();
    public RoomUser myself = new();

    public int currentScene;

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        channel = GrpcChannel.ForAddress("https://localhost:5072");
        service = new GameService.GameServiceClient(channel);

        p8.LoadCart(new JoinRoomScene(this));
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
    }

    public void Draw()
    {

    }


    public async Task ReadRoomStream()
    {
        await foreach (RoomStreamResponse response in roomStream.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
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
            //int dummyIndex = 1;
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
            switch (myself.Role)
            {
                case "Player":
                    p8.LoadCart(new PickBanScene(this));
                    break;
                case "Spectator":
                    p8.LoadCart(new PickBanScene(this)); //should be spectator version when i make the spectator scene
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleJoinRoomNotification(JoinRoomNotification notification)
    {
        playerDictionary.Clear();
        int dummyIndex = 1;
        foreach (RoomUser player in notification.Users)
        {
            //Console.WriteLine(player);
            playerDictionary.TryAdd(dummyIndex, player);
            dummyIndex++;
        }
    }

    private void HandlePlayerReadyNotification(PlayerReadyNotification notification)
    {
        playerDictionary.Clear();
        int dummyIndex = 1;
        foreach (RoomUser player in notification.Users)
        {
            //Console.WriteLine(player);
            playerDictionary.TryAdd(dummyIndex, player);
            dummyIndex++;
        }
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
        cancellationTokenSource.Cancel(); // Cancel the listening task
        roomStream?.Dispose(); // Dispose of the stream
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            cancellationTokenSource.Cancel(); // Cancel the listening task
            roomStream?.Dispose(); // Dispose of the stream
        };
    }

}
