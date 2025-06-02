using System.Collections.Concurrent;
using System.Data;
using System.Threading;
using CSharpCraft.Pico8;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Xna.Framework.Input;
using RaceServer;

namespace CSharpCraft.RaceMode;

public static class RoomHandler
{
    private static readonly GrpcChannel _channel;
    private static readonly GameService.GameServiceClient _service;
    private static CancellationTokenSource? _cancellationTokenSource;
    private static AsyncServerStreamingCall<RoomStreamResponse>? _roomStream;
    public static readonly ConcurrentDictionary<int, RoomUser> _playerDictionary = new();
    public static RoomUser? _myself;
    public static Pico8Functions? p8;

    static RoomHandler()
    {
        _channel = GrpcChannel.ForAddress("https://localhost:5072");
        _service = new GameService.GameServiceClient(_channel);
    }

    public static async Task<bool> JoinRoom(string name, string role)
    {
        try
        {
            JoinRoomResponse response = _service.JoinRoom(new JoinRoomRequest { Name = name, Role = role });
            _myself = new RoomUser { Name = response.Name, Role = response.Role, Host = response.Host };
            return true;
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"Error joining room: {ex.Status.Detail}");
            return false;
        }
    }

    public static async Task ReadRoomStream()
    {
        await foreach (RoomStreamResponse response in _roomStream.ResponseStream.ReadAllAsync(_cancellationTokenSource.Token))
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

    private static void HandleUpdateSeedsNotification(UpdateSeedsNotification updateSeedsNotification)
    {
        throw new NotImplementedException();
    }

    private static void HandleStartMatchNotification(StartMatchNotification startMatchNotification)
    {
        if (startMatchNotification.MatchStarted)
        {
            switch (_myself.Role)
            {
                case "Player":
                    p8.LoadCart(new PickBanScene());
                    break;
                case "Spectator":
                    p8.LoadCart(new PickBanScene()); //should be spectator version when i make the spectator scene
                    break;
                default:
                    break;
            }
        }
    }

    private static void HandleJoinRoomNotification(JoinRoomNotification notification)
    {
        _playerDictionary.Clear();
        int dummyIndex = 1;
        foreach (RoomUser player in notification.Users)
        {
            //Console.WriteLine(player);
            _playerDictionary.TryAdd(dummyIndex, player);
            dummyIndex++;
        }
    }

    private static void HandlePlayerReadyNotification(PlayerReadyNotification notification)
    {
        _playerDictionary.Clear();
        int dummyIndex = 1;
        foreach (RoomUser player in notification.Users)
        {
            //Console.WriteLine(player);
            _playerDictionary.TryAdd(dummyIndex, player);
            dummyIndex++;
        }
    }

    public static async Task Password()
    {
        throw new NotImplementedException();
    }

    public static async Task Settings()
    {
        throw new NotImplementedException();
    }

    public static async Task Seeding()
    {
        throw new NotImplementedException();
    }

    public static async Task ChangeHost()
    {
        throw new NotImplementedException();
    }

    public static async Task ChangeRole()
    {
        throw new NotImplementedException();
    }

    public static async Task LeaveRoom()
    {
        throw new NotImplementedException();
    }

    public static async Task StartMatch()
    {
        _service.StartMatch(new StartMatchRequest { Name = _myself.Name });
    }

    public static async Task PlayerReady()
    {
        _myself.Ready = _service.PlayerReady(new PlayerReadyRequest { Name = _myself.Name }).Ready;
    }
}
