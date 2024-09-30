using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        clients.Add(responseStream);
        //var newUser = new User { Name = request.Name, Role = request.Role, Host = room.Users.Count == 0 ? true : false, Ready = request.Role == "Player" ? false : true };
        //room.AddPlayer(newUser);
        //
        //var joinMessage = new RoomStreamResponse
        //{
        //    Message = $"{request.Name} has joined the room.",
        //};
        //
        //var users = new List<RoomUser>();
        //foreach (var user in room.Users)
        //{
        //    var roomUser = new RoomUser
        //    {
        //        Name = user.Name,
        //        Role = user.Role,
        //        Host = user.Host,
        //        Ready = user.Ready
        //    };
        //    joinMessage.Users.Add(roomUser);
        //}
        //
        //foreach (var client in clients)
        //{
        //    await client.WriteAsync(joinMessage);
        //}

        // Keep the stream open
        while (!context.CancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000);
        }

        //todo remove player or spectator depending on request.Type == Player or Spectator
        room.RemovePlayer(request.Name);

        // Remove the client when the stream is closed
        clients.Remove(responseStream);

        // notify the player has left the room

        var notification = new RoomStreamResponse
        {
            JoinRoomNotification = new JoinRoomNotification { }
        };
        foreach (var user in room.Users)
        {
            var roomUser = new RoomUser
            {
                Name = user.Name,
                Role = user.Role,
                Host = user.Host,
                Ready = user.Ready
            };
            notification.JoinRoomNotification.Users.Add(roomUser);
        }

        //List<string> userNames = new List<string>();
        //foreach (var user in room.Users)
        //{
        //    userNames.Add(user.Name);
        //}
        //
        //joinMessage = new RoomStreamResponse
        //{
        //    Message = $"{request.Name} has left the room.",
        //};
        //users = new List<RoomUser>();
        //foreach (var user in room.Users)
        //{
        //    var roomUser = new RoomUser
        //    {
        //        Name = user.Name,
        //        Role = user.Role,
        //        Host = user.Host
        //    };
        //    joinMessage.Users.Add(roomUser);
        //}

        foreach (var client in clients)
        {
            await client.WriteAsync(notification);
        }
    }

    public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
    {
        var newUser = new User { Name = request.Name, Role = request.Role, Host = room.Users.Count == 0 ? true : false, Ready = request.Role == "Player" ? false : true };
        room.AddPlayer(newUser);

        var notification = new RoomStreamResponse
        {
            JoinRoomNotification = new JoinRoomNotification { }
        };
        foreach (var user in room.Users)
        {
            var roomUser = new RoomUser
            {
                Name = user.Name,
                Role = user.Role,
                Host = user.Host,
                Ready = user.Ready
            };
            notification.JoinRoomNotification.Users.Add(roomUser);
        }

        foreach (var client in clients)
        {
            await client.WriteAsync(notification);
        }

        return new JoinRoomResponse
        {
            Name = newUser.Name,
            Role = newUser.Role,
            Host = newUser.Host,
            Ready = newUser.Ready
        };
    }

    public override async Task<PlayerReadyResponse> PlayerReady(PlayerReadyRequest request, ServerCallContext context)
    {
        room.TogglePlayerReady(request.Name);

        var notification = new RoomStreamResponse
        {
            PlayerReadyNotification = new PlayerReadyNotification { }
        };
        foreach (var user in room.Users)
        {
            var roomUser = new RoomUser
            {
                Name = user.Name,
                Role = user.Role,
                Host = user.Host,
                Ready = user.Ready
            };
            notification.PlayerReadyNotification.Users.Add(roomUser);
        }

        foreach (var client in clients)
        {
            await client.WriteAsync(notification);
        }

        var myself = room.users.FirstOrDefault(p => p.Name == request.Name);
        return new PlayerReadyResponse
        {
            Ready = myself.Ready
        };
    }

}