using Microsoft.Xna.Framework.Input;
using Grpc.Core;
using Grpc.Net.Client;
using RaceServer;
using System.Collections.Concurrent;
using CSharpCraft.Pico8;

namespace CSharpCraft.RaceMode;

public static class RoomHandler
{
    private static readonly GrpcChannel _channel;
    private static readonly GameService.GameServiceClient _service;
    private static CancellationTokenSource? _cancellationTokenSource;
    private static AsyncServerStreamingCall<RoomStreamResponse>? _roomStream;
    public static readonly ConcurrentDictionary<int, RoomUser> _playerDictionary = new();
    public static RoomUser? _myself;
    private static int _currentScene;

    static RoomHandler()
    {
        _channel = GrpcChannel.ForAddress("https://localhost:5072");
        _service = new GameService.GameServiceClient(_channel);
    }

    public static async Task<bool> ConnectToRoom(string username, string role)
    {
        try
        {
            Console.WriteLine($"Attempting to connect to room as {username} with role {role}");
            var response = await _service.ConnectToRoomAsync(new ConnectToRoomRequest
            {
                Username = username,
                Role = role
            });

            if (!response.Success)
            {
                Console.WriteLine($"Failed to connect to room: {response.Message}");
                return false;
            }

            Console.WriteLine($"Successfully connected to room. IsHost: {response.IsHost}, IsReady: {response.IsReady}");
            _myself = new RoomUser
            {
                Username = username,
                Role = role,
                IsHost = response.IsHost,
                IsReady = response.IsReady
            };

            // Start the room stream
            await StartRoomStream(username);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to room: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return false;
        }
    }

    public static async Task DisconnectFromRoom()
    {
        if (_myself == null) return;

        Console.WriteLine("Stopping room stream");
        StopRoomStream();

        try
        {
            var response = await _service.DisconnectFromRoomAsync(new DisconnectFromRoomRequest
            {
                Username = _myself.Username
            });

            if (!response.Success)
            {
                Console.WriteLine($"Failed to disconnect from room: {response.Message}");
                return;
            }

            Console.WriteLine("Successfully disconnected from room");
            _playerDictionary.Clear();
            _myself = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disconnecting from room: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    private static async Task StartRoomStream(string username)
    {
        Console.WriteLine($"Starting room stream for {username}");
        StopRoomStream();

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Console.WriteLine("Created new cancellation token source");
            
            _roomStream = _service.RoomStream(new RoomStreamRequest
            {
                Username = username
            });
            Console.WriteLine("Room stream established");

            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine("Starting to read from room stream");
                    await foreach (var response in _roomStream.ResponseStream.ReadAllAsync(_cancellationTokenSource.Token))
                    {
                        Console.WriteLine($"Received message type: {response.MessageCase}");
                        switch (response.MessageCase)
                        {
                            case RoomStreamResponse.MessageOneofCase.RoomState:
                                HandleRoomState(response.RoomState);
                                break;
                            case RoomStreamResponse.MessageOneofCase.PlayerReady:
                                HandlePlayerReady(response.PlayerReady);
                                break;
                            case RoomStreamResponse.MessageOneofCase.StartMatch:
                                HandleStartMatch(response.StartMatch);
                                break;
                            case RoomStreamResponse.MessageOneofCase.UpdateSeeds:
                                HandleUpdateSeeds(response.UpdateSeeds);
                                break;
                            case RoomStreamResponse.MessageOneofCase.EndGame:
                                HandleEndGame(response.EndGame);
                                break;
                            case RoomStreamResponse.MessageOneofCase.EndMatch:
                                HandleEndMatch(response.EndMatch);
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
                    Console.WriteLine("Room stream cancelled normally");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in room stream: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting room stream: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }

    public static void StopRoomStream()
    {
        Console.WriteLine("Stopping room stream");
        if (_cancellationTokenSource != null)
        {
            Console.WriteLine("Cancelling token source");
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
        if (_roomStream != null)
        {
            Console.WriteLine("Disposing room stream");
            _roomStream.Dispose();
            _roomStream = null;
        }
    }

    public static async Task<bool> SetReady()
    {
        if (_myself is null) return false;

        try
        {
            var response = await _service.SetReadyAsync(new SetReadyRequest
            {
                Username = _myself.Username,
                Ready = !_myself.IsReady
            });

            if (!response.Success)
            {
                Console.WriteLine($"Failed to set ready status: {response.Message}");
                return false;
            }

            _myself.IsReady = response.Ready;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting ready status: {ex.Message}");
            return false;
        }
    }

    public static async Task<bool> StartMatch()
    {
        if (_myself is null) return false;

        try
        {
            var response = await _service.StartMatchAsync(new StartMatchRequest
            {
                Username = _myself.Username
            });

            if (!response.Success)
            {
                Console.WriteLine($"Failed to start match: {response.Message}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting match: {ex.Message}");
            return false;
        }
    }

    private static void HandleRoomState(RoomStateNotification notification)
    {
        Console.WriteLine($"Handling room state with {notification.Users.Count} users");
        _playerDictionary.Clear();
        int index = 1;
        foreach (var user in notification.Users)
        {
            Console.WriteLine($"Adding user to dictionary: {user.Username} (Role: {user.Role}, IsHost: {user.IsHost}, IsReady: {user.IsReady})");
            _playerDictionary.TryAdd(index++, user);
        }
        Console.WriteLine($"Player dictionary now contains {_playerDictionary.Count} users");
    }

    private static void HandlePlayerReady(PlayerReadyNotification notification)
    {
        _playerDictionary.Clear();
        int index = 1;
        foreach (var user in notification.Users)
        {
            _playerDictionary.TryAdd(index++, user);
        }
    }

    private static void HandleStartMatch(StartMatchNotification notification)
    {
        //if (notification.MatchStarted && _p8 is not null)
        //{
        //    switch (_myself?.Role)
        //    {
        //        case "Player":
        //            _p8.LoadCart(new PickBanScene());
        //            break;
        //        case "Spectator":
        //            _p8.LoadCart(new PickBanScene()); // TODO: Create spectator version
        //            break;
        //    }
        //}
    }

    private static void HandleUpdateSeeds(UpdateSeedsNotification notification)
    {
        _playerDictionary.Clear();
        int index = 1;
        foreach (var user in notification.Users)
        {
            _playerDictionary.TryAdd(index++, user);
        }
    }

    private static void HandleEndGame(EndGameNotification notification)
    {
        // TODO: Implement end game handling
    }

    private static void HandleEndMatch(EndMatchNotification notification)
    {
        // TODO: Implement end match handling
    }

    internal static async Task ChangeRole()
    {
        throw new NotImplementedException();
    }

    internal static async Task ChangeHost()
    {
        throw new NotImplementedException();
    }

    internal static async Task Seeding()
    {
        throw new NotImplementedException();
    }

    internal static async Task Settings()
    {
        throw new NotImplementedException();
    }

    internal static async Task Password()
    {
        throw new NotImplementedException();
    }
}
