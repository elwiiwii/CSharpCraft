using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace RaceServer.Services;

public class GameServer : GameService.GameServiceBase
{
    private readonly List<IServerStreamWriter<RoomStreamResponse>> clients = new();

    private readonly Room room = new("TestRoom");
    private readonly ILogger<GameServer> logger;

    public GameServer(ILogger<GameServer> logger)
    {
        this.logger = logger;
    }

    public override async Task RoomStream(RoomStreamRequest request, IServerStreamWriter<RoomStreamResponse> responseStream, ServerCallContext context)
    {
        //todo add as player or spectator depending on what's in the request
        //todo modify return message depending on whether the user is a player or a spectator
        try
        {
            clients.Add(responseStream);

            User newUser = new() { Name = request.Name, Role = request.Role, Host = room.Users.Count == 0, Ready = request.Role != "Player" };
            room.AddPlayer(newUser);

            RoomStreamResponse notification = new()
            {
                JoinRoomNotification = new JoinRoomNotification()
            };
            foreach (User user in room.Users)
            {
                RoomUser roomUser = new()
                {
                    Name = user.Name,
                    Role = user.Role,
                    Host = user.Host,
                    Ready = user.Ready
                };
                notification.JoinRoomNotification.Users.Add(roomUser);
            }

            foreach (IServerStreamWriter<RoomStreamResponse> client in clients)
            {
                await client.WriteAsync(notification);
            }

            // Keep the stream open
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in RoomStream for user {Name}", request.Name);

            //todo remove player or spectator depending on request.Type == Player or Spectator
            room.RemovePlayer(request.Name);

            // Remove the client when the stream is closed
            clients.Remove(responseStream);

            // notify the player has left the room

            RoomStreamResponse notification = new()
            {
                JoinRoomNotification = new JoinRoomNotification()
            };
            foreach (User user in room.Users)
            {
                RoomUser roomUser = new()
                {
                    Name = user.Name,
                    Role = user.Role,
                    Host = user.Host,
                    Ready = user.Ready
                };
                notification.JoinRoomNotification.Users.Add(roomUser);
            }

            foreach (IServerStreamWriter<RoomStreamResponse> client in clients)
            {
                await client.WriteAsync(notification);
            }
        }
        finally
        {
            //todo remove player or spectator depending on request.Type == Player or Spectator
            room.RemovePlayer(request.Name);

            // Remove the client when the stream is closed
            clients.Remove(responseStream);

            // notify the player has left the room

            RoomStreamResponse notification = new()
            {
                JoinRoomNotification = new JoinRoomNotification()
            };
            foreach (User user in room.Users)
            {
                RoomUser roomUser = new()
                {
                    Name = user.Name,
                    Role = user.Role,
                    Host = user.Host,
                    Ready = user.Ready
                };
                notification.JoinRoomNotification.Users.Add(roomUser);
            }

            foreach (IServerStreamWriter<RoomStreamResponse> client in clients)
            {
                await client.WriteAsync(notification);
            }
        }
    }

    public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
    {
        return new JoinRoomResponse
        {
            Name = request.Name,
            Role = request.Role,
            Host = room.Users.Count < 2 ? true : false, //todo this is jank
            Ready = request.Role == "Player" ? false : true
        };
    }

    public override async Task<LeaveRoomResponse> LeaveRoom(LeaveRoomRequest request, ServerCallContext context)
    {
        try
        {
            // First notify all clients about the room change
            RoomStreamResponse notification = new()
            {
                LeaveRoomNotification = new LeaveRoomNotification()
            };
            foreach (User user in room.Users)
            {
                RoomUser roomUser = new()
                {
                    Name = user.Name,
                    Role = user.Role,
                    Host = user.Host,
                    Ready = user.Ready
                };
                notification.LeaveRoomNotification.Users.Add(roomUser);
            }

            // Send notifications to all clients except the one leaving
            foreach (IServerStreamWriter<RoomStreamResponse> client in clients)
            {
                try
                {
                    await client.WriteAsync(notification);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to notify client about room change");
                }
            }

            // Then remove the player
            room.RemovePlayer(request.Name);

            return new LeaveRoomResponse
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in LeaveRoom for user {Name}", request.Name);
            return new LeaveRoomResponse
            {
                Success = false
            };
        }
    }

    public override async Task<PlayerReadyResponse> PlayerReady(PlayerReadyRequest request, ServerCallContext context)
    {
        room.TogglePlayerReady(request.Name);

        RoomStreamResponse notification = new()
        {
            PlayerReadyNotification = new PlayerReadyNotification()
        };
        foreach (User user in room.Users)
        {
            RoomUser roomUser = new()
            {
                Name = user.Name,
                Role = user.Role,
                Host = user.Host,
                Ready = user.Ready
            };
            notification.PlayerReadyNotification.Users.Add(roomUser);
        }

        foreach (IServerStreamWriter<RoomStreamResponse> client in clients)
        {
            await client.WriteAsync(notification);
        }

        User myself = room.Users.FirstOrDefault(p => p.Name == request.Name);
        return new PlayerReadyResponse
        {
            Ready = myself.Ready
        };
    }

    public override async Task<StartMatchResponse> StartMatch(StartMatchRequest request, ServerCallContext context)
    {
        room.AssignSeedingTemp();
        User h = room.Users.FirstOrDefault(p => p.Seed == 1);
        User l = room.Users.FirstOrDefault(p => p.Seed == 2);

        RoomStreamResponse notification = new()
        {
            StartMatchNotification = new StartMatchNotification { MatchStarted = room.AllPlayersReady() }
        };

        foreach (IServerStreamWriter<RoomStreamResponse> client in clients)
        {
            await client.WriteAsync(notification);
        }

        return new StartMatchResponse
        {

        };
    }

}