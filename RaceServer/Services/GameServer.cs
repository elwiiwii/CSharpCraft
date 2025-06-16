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

    private readonly Room room;
    private readonly ILogger<GameServer> logger;
    private readonly object lockObject = new();

    public GameServer(ILogger<GameServer> logger)
    {
        this.logger = logger;
        room = new("TestRoom", null, this.logger);
    }

    public override async Task RoomStream(RoomStreamRequest request, IServerStreamWriter<RoomStreamResponse> responseStream, ServerCallContext context)
    {
        //todo add as player or spectator depending on what's in the request
        //todo modify return message depending on whether the user is a player or a spectator
        try
        {
            clients.Add(responseStream);

            RoomUser newUser = new() { Name = request.Name, Role = request.Role, Host = room.Users.Count == 0, Ready = request.Role != Role.Player };
            room.AddPlayer(newUser);

            RoomStreamResponse notification = new()
            {
                JoinRoomNotification = new JoinRoomNotification()
            };
            foreach (RoomUser user in room.Users)
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
            foreach (RoomUser user in room.Users)
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
            foreach (RoomUser user in room.Users)
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
            Host = room.Users.Where(p => p.Host).Count() == 0,
            Ready = request.Role != Role.Player
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
            foreach (RoomUser user in room.Users)
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
        foreach (RoomUser user in room.Users)
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

        RoomUser myself = room.Users.FirstOrDefault(p => p.Name == request.Name);
        return new PlayerReadyResponse
        {
            Ready = myself.Ready
        };
    }

    public override async Task<StartMatchResponse> StartMatch(StartMatchRequest request, ServerCallContext context)
    {
        room.AssignSeedingTemp();
        RoomUser h = room.Users.FirstOrDefault(p => p.Seed == 1);
        RoomUser l = room.Users.FirstOrDefault(p => p.Seed == 2);
        room.CurrentMatch.GameReports[room.CurrentMatch.GameReports.Count - 1].WorldSeed = new Random().Next(0, int.MaxValue);
        room.CurrentMatch.GameReports[room.CurrentMatch.GameReports.Count - 1].RngSeed = new Random().Next(0, int.MaxValue);

        RoomStreamResponse notification = new()
        {
            StartMatchNotification = new StartMatchNotification { MatchState = room.CurrentMatch.ToMatchState() }
        };

        foreach (IServerStreamWriter<RoomStreamResponse> client in clients)
        {
            await client.WriteAsync(notification);
        }

        return new StartMatchResponse
        {

        };
    }

    public override async Task<TogglePicksResponse> TogglePicks(TogglePicksRequest request, ServerCallContext context)
    {
        room.CurrentMatch.UpdatePicksOn(logger, !room.CurrentMatch.PicksOn);

        RoomStreamResponse notification = new()
        {
            TogglePicksNotification = new()
            {
                MatchState = room.CurrentMatch.ToMatchState()
            }
        };

        foreach (IServerStreamWriter<RoomStreamResponse> client in clients)
        {
            await client.WriteAsync(notification);
        }

        return new TogglePicksResponse
        {
            Success = true
        };
    }
}