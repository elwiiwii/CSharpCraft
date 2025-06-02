using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace RaceServer.Services;

public class GameServer(ILogger<GameServer> logger, Room room) : GameService.GameServiceBase
{
    private readonly List<IServerStreamWriter<RoomStreamResponse>> clients = [];
    private readonly Room room = room;
    private readonly ILogger<GameServer> logger = logger;

    public override async Task RoomStream(RoomStreamRequest request, IServerStreamWriter<RoomStreamResponse> responseStream, ServerCallContext context)
    {
        logger.LogInformation($"User {request.Name} joining room stream");
        logger.LogInformation($"Current users in room before adding: {string.Join(", ", room.Users.Select(u => u.Name))}");
        
        try
        {
            clients.Add(responseStream);

            User newUser = new() { Name = request.Name, Role = request.Role, Host = room.Users.Count == 0, Ready = request.Role != "Player" };
            room.AddPlayer(newUser);
            logger.LogInformation($"Added user {request.Name} to room. Total users: {room.Users.Count}");
            logger.LogInformation($"Current users in room after adding: {string.Join(", ", room.Users.Select(u => u.Name))}");

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
            logger.LogError(ex, $"Error in RoomStream for user {request.Name}");
            throw;
        }
        finally
        {
            logger.LogInformation($"User {request.Name} leaving room stream");
            logger.LogInformation($"Current users in room before removing: {string.Join(", ", room.Users.Select(u => u.Name))}");
            room.RemovePlayer(request.Name);
            clients.Remove(responseStream);
            logger.LogInformation($"Removed user {request.Name} from room. Total users: {room.Users.Count}");
            logger.LogInformation($"Current users in room after removing: {string.Join(", ", room.Users.Select(u => u.Name))}");

            // Notify remaining clients
            var notification = new RoomStreamResponse
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
                try
                {
                    await client.WriteAsync(notification);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error notifying clients about user {request.Name} leaving");
                }
            }
        }
    }

    public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
    {
        //User newUser = new() { Name = request.Name, Role = request.Role, Host = room.Users.Count == 0 ? true : false, Ready = request.Role == "Player" ? false : true };
        //room.AddPlayer(newUser);
        //
        //RoomStreamResponse notification = new()
        //{
        //    JoinRoomNotification = new JoinRoomNotification { }
        //};
        //foreach (User user in room.Users)
        //{
        //    RoomUser roomUser = new()
        //    {
        //        Name = user.Name,
        //        Role = user.Role,
        //        Host = user.Host,
        //        Ready = user.Ready
        //    };
        //    notification.JoinRoomNotification.Users.Add(roomUser);
        //}
        //
        //foreach (IServerStreamWriter<RoomStreamResponse> client in clients)
        //{
        //    await client.WriteAsync(notification);
        //}

        return new JoinRoomResponse
        {
            Name = request.Name,
            Role = request.Role,
            Host = room.Users.Count < 2, //todo this is jank
            Ready = request.Role != "Player"
        };
    }

    public override async Task<PlayerReadyResponse> PlayerReady(PlayerReadyRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Name cannot be null or empty"));
        }

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
        if (myself == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User {request.Name} not found in room"));
        }

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
        
        if (h == null || l == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Could not find players with seeds 1 and 2"));
        }
        
        room.CurrentMatch = room.NewDuelMatch(h, l);

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
}