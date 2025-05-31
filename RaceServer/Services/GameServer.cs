using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace RaceServer.Services;

public class GameServer : GameService.GameServiceBase
{
    private readonly Dictionary<string, IServerStreamWriter<RoomStreamResponse>> _clients = new();
    private readonly Room _room;
    private readonly ILogger<GameServer> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public GameServer(ILogger<GameServer> logger, ILoggerFactory loggerFactory, Room room)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _room = room;
    }

    public override async Task<ConnectToRoomResponse> ConnectToRoom(ConnectToRoomRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"User {request.Username} attempting to connect to room as {request.Role}");
        _logger.LogInformation($"Current room users before adding: {string.Join(", ", _room.Users.Select(u => u.Username))}");

        var user = new RoomPlayer
        {
            Username = request.Username,
            Role = request.Role
        };

        var (success, message) = _room.AddUser(user);
        if (!success)
        {
            _logger.LogWarning($"Failed to add user {request.Username} to room: {message}");
            return new ConnectToRoomResponse
            {
                Success = false,
                Message = message
            };
        }

        _logger.LogInformation($"Successfully added user {request.Username} to room");
        _logger.LogInformation($"Current room users after adding: {string.Join(", ", _room.Users.Select(u => u.Username))}");

        // Notify all clients about the new room state
        _logger.LogInformation("Broadcasting room state to all clients");
        await BroadcastRoomState();

        return new ConnectToRoomResponse
        {
            Success = true,
            Message = "Connected to room successfully",
            IsHost = user.IsHost,
            IsReady = user.IsReady
        };
    }

    public override async Task<DisconnectFromRoomResponse> DisconnectFromRoom(DisconnectFromRoomRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"User {request.Username} disconnecting from room");

        var (success, message) = _room.RemoveUser(request.Username);
        if (!success)
        {
            return new DisconnectFromRoomResponse
            {
                Success = false,
                Message = message
            };
        }

        // Remove client from stream
        if (_clients.ContainsKey(request.Username))
        {
            _clients.Remove(request.Username);
        }

        // Notify remaining clients about the room state
        await BroadcastRoomState();

        return new DisconnectFromRoomResponse
        {
            Success = true,
            Message = "Disconnected from room successfully"
        };
    }

    public override async Task RoomStream(RoomStreamRequest request, IServerStreamWriter<RoomStreamResponse> responseStream, ServerCallContext context)
    {
        _logger.LogInformation($"User {request.Username} starting room stream");

        if (!_clients.ContainsKey(request.Username))
        {
            _clients.Add(request.Username, responseStream);
        }

        try
        {
            // Send initial room state
            var response = new RoomStreamResponse
            {
                RoomState = new RoomStateNotification
                {
                    AllReady = _room.AllPlayersReady()
                }
            };

            _logger.LogInformation($"Current room users: {string.Join(", ", _room.Users.Select(u => u.Username))}");
            foreach (var user in _room.Users)
            {
                _logger.LogInformation($"Adding user to room state: {user.Username} (Role: {user.Role}, IsHost: {user.IsHost}, IsReady: {user.IsReady})");
                response.RoomState.Users.Add(new RoomUser
                {
                    Username = user.Username,
                    Role = user.Role,
                    IsHost = user.IsHost,
                    IsReady = user.IsReady,
                    Seed = user.Seed ?? 0
                });
            }

            _logger.LogInformation($"Sending room state with {response.RoomState.Users.Count} users");
            await responseStream.WriteAsync(response);

            // Keep stream alive until client disconnects
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in room stream for user {request.Username}: {ex.Message}");
        }
        finally
        {
            if (_clients.ContainsKey(request.Username))
            {
                _clients.Remove(request.Username);
            }
        }
    }

    public override async Task<SetReadyResponse> SetReady(SetReadyRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"User {request.Username} setting ready status to {request.Ready}");

        var (success, message) = _room.SetUserReady(request.Username, request.Ready);
        if (!success)
        {
            return new SetReadyResponse
            {
                Success = false,
                Message = message,
                Ready = false
            };
        }

        _logger.LogInformation($"_clients.Count = {_clients.Count}");
        // Notify all clients about the ready status change
        await BroadcastRoomState();

        return new SetReadyResponse
        {
            Success = true,
            Message = "Ready status updated successfully",
            Ready = request.Ready
        };
    }

    public override async Task<StartMatchResponse> StartMatch(StartMatchRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"User {request.Username} attempting to start match");

        var (success, message) = _room.StartMatch();
        if (!success)
        {
            return new StartMatchResponse
            {
                Success = false,
                Message = message
            };
        }

        // Notify all clients that the match has started
        var response = new RoomStreamResponse
        {
            StartMatch = new StartMatchNotification
            {
                MatchStarted = true
            }
        };

        foreach (var user in _room.Users)
        {
            response.StartMatch.Users.Add(new RoomUser
            {
                Username = user.Username,
                Role = user.Role,
                IsHost = user.IsHost,
                IsReady = user.IsReady,
                Seed = user.Seed ?? 0
            });
        }

        await BroadcastToAll(response);

        return new StartMatchResponse
        {
            Success = true,
            Message = "Match started successfully"
        };
    }

    private async Task BroadcastRoomState()
    {
        _logger.LogInformation("Preparing to broadcast room state");
        _logger.LogInformation($"Current room users: {string.Join(", ", _room.Users.Select(u => u.Username))}");

        var response = new RoomStreamResponse
        {
            RoomState = new RoomStateNotification
            {
                AllReady = _room.AllPlayersReady()
            }
        };

        foreach (var user in _room.Users)
        {
            _logger.LogInformation($"Adding user to room state: {user.Username} (Role: {user.Role}, IsHost: {user.IsHost}, IsReady: {user.IsReady})");
            response.RoomState.Users.Add(new RoomUser
            {
                Username = user.Username,
                Role = user.Role,
                IsHost = user.IsHost,
                IsReady = user.IsReady,
                Seed = user.Seed ?? 0
            });
        }

        _logger.LogInformation($"_clients.Count = {_clients.Count}");
        _logger.LogInformation($"Room state prepared with {response.RoomState.Users.Count} users");
        await BroadcastToAll(response);
        _logger.LogInformation("Room state broadcast complete");
    }

    private async Task BroadcastToAll(RoomStreamResponse response)
    {
        _logger.LogInformation($"Broadcasting to {_clients.Count} clients");
        foreach (var client in _clients.Values)
        {
            try
            {
                await client.WriteAsync(response);
                _logger.LogInformation("Successfully wrote to client");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error broadcasting to client: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}