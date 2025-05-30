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
    private static readonly CancellationTokenSource _cancellationTokenSource = new();
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
            return false;
        }
    }

    public static async Task DisconnectFromRoom()
    {
        if (_myself is null) return;

        try
        {
            var response = await _service.DisconnectFromRoomAsync(new DisconnectFromRoomRequest
            {
                Username = _myself.Username
            });

            if (!response.Success)
            {
                Console.WriteLine($"Failed to disconnect from room: {response.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disconnecting from room: {ex.Message}");
        }
        finally
        {
            StopRoomStream();
            _myself = null;
            _playerDictionary.Clear();
        }
    }

    private static async Task StartRoomStream(string username)
    {
        StopRoomStream();

        try
        {
            _roomStream = _service.RoomStream(new RoomStreamRequest
            {
                Username = username
            });

            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var response in _roomStream.ResponseStream.ReadAllAsync(_cancellationTokenSource.Token))
                    {
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
                    // Normal cancellation
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in room stream: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting room stream: {ex.Message}");
            throw;
        }
    }

    public static void StopRoomStream()
    {
        _cancellationTokenSource.Cancel();
        _roomStream?.Dispose();
        _roomStream = null;
    }

    public static async Task<bool> SetReady(bool ready)
    {
        if (_myself is null) return false;

        try
        {
            var response = await _service.SetReadyAsync(new SetReadyRequest
            {
                Username = _myself.Username,
                Ready = ready
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
        _playerDictionary.Clear();
        int index = 1;
        foreach (var user in notification.Users)
        {
            _playerDictionary.TryAdd(index++, user);
        }
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

    internal static async Task SetReady()
    {
        throw new NotImplementedException();
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
