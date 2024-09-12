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
        //todo add as player or user depending on what's in the request
        //todo modify return message depending on whether the user is a player or a spectator

        clients.Add(responseStream);
        room.AddPlayer(new Player { Name = request.UserName });

        var joinMessage = new JoinRoomResponse
        {
            Message = $"{request.UserName} has joined the room.",
            Players = { room.Players.Select(p => p.Name) },
            Spectators = { room.Spectators.Select(s => s.Name) }
        };

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
        room.RemovePlayer(request.UserName);

        // Remove the client when the stream is closed
        clients.Remove(responseStream);

        // notify the player has left the room
        joinMessage = new JoinRoomResponse
        {
            Message = $"{request.UserName} has left the room.",
            Players = { room.Players.Select(p => p.Name) },
            Spectators = { room.Spectators.Select(s => s.Name) }
        };

        foreach (var client in clients)
        {
            await client.WriteAsync(joinMessage);
        }
    }
}