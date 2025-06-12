using System.Collections.Concurrent;
using CSharpCraft.Pcraft;
using CSharpCraft.Pico8;
using Grpc.Core;
using Grpc.Net.Client;
using RaceServer;

namespace CSharpCraft.Competitive;

public static class RoomHandler
{
    private static readonly GrpcChannel _channel;
    private static readonly GameService.GameServiceClient _service;
    private static CancellationTokenSource? _cancellationTokenSource;
    private static AsyncServerStreamingCall<RoomStreamResponse>? _roomStream;
    public static readonly ConcurrentDictionary<int, RoomUser> _playerDictionary = new();
    public static RoomUser? _myself;
    public static Pico8Functions? p8;

    public static MatchState? _curMatch;

    static RoomHandler()
    {
        _channel = GrpcChannel.ForAddress("https://localhost:5072");
        _service = new GameService.GameServiceClient(_channel);
        _cancellationTokenSource = new CancellationTokenSource();
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        try
        {
            LeaveRoom();
            _channel?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during process exit cleanup: {ex.Message}");
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            LeaveRoom();
            _channel?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during unhandled exception cleanup: {ex.Message}");
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            LeaveRoom();
            _channel?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during unobserved task exception cleanup: {ex.Message}");
        }
    }

    public static void Shutdown()
    {
        try
        {
            LeaveRoom();
            _channel?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during shutdown cleanup: {ex.Message}");
        }
    }

    public static async Task<bool> JoinRoom(string name, Role role)
    {
        try
        {
            _roomStream = _service.RoomStream(new RoomStreamRequest { Name = name, Role = role });
            _ = Task.Run(async () => 
            {
                try 
                {
                    await ReadRoomStream();
                }
                catch (OperationCanceledException)
                {
                    // Expected when leaving room, no need to log
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in room stream: {ex.Message}");
                }
            }, _cancellationTokenSource.Token);
            JoinRoomResponse response = _service.JoinRoom(new JoinRoomRequest { Name = name, Role = role });
            _myself = new RoomUser { Name = response.Name, Role = response.Role, Host = response.Host, Ready = response.Ready };
            return true;
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"Error joining room: {ex.Status.Detail}");
            return false;
        }
    }

    private static async Task ReadRoomStream()
    {
        try
        {
            await foreach (RoomStreamResponse response in _roomStream.ResponseStream.ReadAllAsync(_cancellationTokenSource.Token))
            {
                switch (response.MessageCase)
                {
                    case RoomStreamResponse.MessageOneofCase.JoinRoomNotification:
                        HandleJoinRoomNotification(response.JoinRoomNotification);
                        break;
                    case RoomStreamResponse.MessageOneofCase.LeaveRoomNotification:
                        HandleLeaveRoomNotification(response.LeaveRoomNotification);
                        break;
                    case RoomStreamResponse.MessageOneofCase.PlayerReadyNotification:
                        HandlePlayerReadyNotification(response.PlayerReadyNotification);
                        break;
                    case RoomStreamResponse.MessageOneofCase.StartMatchNotification:
                        HandleStartMatchNotification(response.StartMatchNotification);
                        break;
                    case RoomStreamResponse.MessageOneofCase.SendSeedNotification:
                        HandleSendSeedNotification(response.SendSeedNotification);
                        break;
                    case RoomStreamResponse.MessageOneofCase.UpdateSeedsNotification:
                        HandleUpdateSeedsNotification(response.UpdateSeedsNotification);
                        break;
                    case RoomStreamResponse.MessageOneofCase.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when leaving room, no need to log
            throw; // Re-throw to be handled by the caller
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in room stream: {ex.Message}");
            throw; // Re-throw to be handled by the caller
        }
    }

    private static void HandleSendSeedNotification(SendSeedNotification sendSeedNotification)
    {
        SeedFilter.Cts?.Cancel();
        SeedFilter.Cts = new();
        _curMatch = sendSeedNotification.MatchState;
    }

    private static void HandleLeaveRoomNotification(LeaveRoomNotification notification)
    {
        UpdatePlayerDictionary(notification.Users);
        UpdateMyself(notification.Users);
    }

    private static void HandleUpdateSeedsNotification(UpdateSeedsNotification updateSeedsNotification)
    {
        throw new NotImplementedException();
    }

    private static void HandleStartMatchNotification(StartMatchNotification startMatchNotification)
    {
        _curMatch = startMatchNotification.MatchState;

        if (startMatchNotification.MatchState.PicksOn || startMatchNotification.MatchState.BansOn)
        {
            switch (_myself.Role)
            {
                case Role.Player:
                    p8.ScheduleScene(() => new PickBanScene());
                    break;
                case Role.Spectator:
                    p8.ScheduleScene(() => new PickBanScene()); //should be spectator version when i make the spectator scene
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (_myself.Role)
            {
                case Role.Player:
                    p8.ScheduleScene(() => new PcraftCompetitive());
                    break;
                case Role.Spectator:
                    //should be spectator version when i make the spectator scene
                    break;
                default:
                    break;
            }
        }
    }

    private static void UpdateMyself(IEnumerable<RoomUser> users)
    {
        _myself = users.FirstOrDefault(x => x.Name == _myself.Name);
        if (_myself is null) users.FirstOrDefault(x => x.Name == AccountHandler._myself.Username);
        if (_myself is null) LeaveRoom();
    }

    private static void UpdatePlayerDictionary(IEnumerable<RoomUser> users)
    {
        _playerDictionary.Clear();
        int dummyIndex = 1;
        foreach (RoomUser player in users)
        {
            _playerDictionary.TryAdd(dummyIndex, player);
            dummyIndex++;
        }
    }

    private static void HandleJoinRoomNotification(JoinRoomNotification notification)
    {
        UpdatePlayerDictionary(notification.Users);
    }

    private static void HandlePlayerReadyNotification(PlayerReadyNotification notification)
    {
        UpdatePlayerDictionary(notification.Users);
        UpdateMyself(notification.Users);
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
        try
        {
            // First cancel the room stream to prevent any new messages
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _roomStream = null;

            // Then try to leave the room if we have a valid _myself
            if (_myself is null)
            {
                if (AccountHandler._myself is not null) _service.LeaveRoom(new LeaveRoomRequest { Name = AccountHandler._myself.Username });
            }
            else
            {
                _service.LeaveRoom(new LeaveRoomRequest { Name = _myself.Name });
            }
        }
        catch
        {
            Console.WriteLine("Error leaving room, _myself may be null or not initialized properly.");
        }
        finally
        {
            // Clear state first
            _myself = null;
            _playerDictionary.Clear();

            // Schedule the scene change for the next frame to avoid texture disposal issues
            if (p8 is not null)
            {
                p8.ScheduleScene(() => new PrivateScene(new CompetitiveScene()));
            }
        }
    }

    public static async Task StartMatch()
    {
        _service.StartMatch(new StartMatchRequest { Name = _myself.Name });
    }

    public static async Task PlayerReady()
    {
        if (_myself is null)
        {
            Console.WriteLine("_myself is null when trying to call PlayerReady");
            return;
        }
        Console.WriteLine($"Calling PlayerReady for {_myself.Name}, current ready state: {_myself.Ready}");
        _myself.Ready = _service.PlayerReady(new PlayerReadyRequest { Name = _myself.Name }).Ready;
        Console.WriteLine($"PlayerReady response received, new ready state: {_myself.Ready}");
    }
}
