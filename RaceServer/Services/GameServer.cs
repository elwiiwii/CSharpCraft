using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using RaceServer;

namespace RaceServer.Services;

public class GameServer : GameService.GameServiceBase
{
    private readonly List<IServerStreamWriter<JoinRoomResponse>> clients = new();

    private readonly Room room = new("TestRoom");

    public override async Task JoinRoom(JoinRoomRequest request, IServerStreamWriter<JoinRoomResponse> responseStream, ServerCallContext context)
    {
        //todo add as player or spectator depending on what's in the request
        //todo modify return message depending on whether the user is a player or a spectator

        clients.Add(responseStream);
        var newUser = new User { Name = request.Name, Role = request.Role, Host = room.Users.Count == 0 ? true : false };
        room.AddPlayer(newUser);

        var joinMessage = new JoinRoomResponse
        {
            Message = $"{request.Name} has joined the room.",
            Myself = { Name = request.Name, Role = request.Role, Host = room.Users.Count == 0 ? true : false }
        };

        var users = new List<RoomUser>();
        foreach (var user in room.Users)
        {
            var roomUser = new RoomUser
            {
                Name = user.Name,
                Role = user.Role,
                Host = user.Host
            };
            joinMessage.Users.Add(roomUser);
        }

        foreach (var client in clients)
        {
            await client.WriteAsync(joinMessage);
        }

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
        List<string> userNames = new List<string>();
        foreach (var user in room.Users)
        {
            userNames.Add(user.Name);
        }

        joinMessage = new JoinRoomResponse
        {
            Message = $"{request.Name} has left the room.",
        };
        users = new List<RoomUser>();
        foreach (var user in room.Users) 
        {
            var roomUser = new RoomUser
            {
                Name = user.Name,
                Role = user.Role,
                Host = user.Host
            };
            joinMessage.Users.Add(roomUser);
        }

        foreach (var client in clients)
        {
            await client.WriteAsync(joinMessage);
        }
    }
}