using Grpc.Core;
using RaceServer;

namespace RaceServer.Services
{
    public class RoomHandlerService : RoomHandler.RoomHandlerBase
    {
        private Room room;
        private string playerListString;
        private readonly ILogger<RoomHandlerService> _logger;
        public RoomHandlerService(ILogger<RoomHandlerService> logger)
        {
            _logger = logger;
            room = new Room();
        }

        public override Task<JoinResponse> JoinRoom(JoinRequest request, ServerCallContext context)
        {
            var player = new Player { Name = request.PlayerInfo.Name, Type = request.PlayerInfo.Type };
            room.PlayerList.Add(player);
            var response = new JoinResponse();

            foreach (var p in room.PlayerList)
            {
                response.PlayerList.Add(new PlayerInfo { Name = p.Name, Type = p.Type });
            }
            return Task.FromResult(response);
        }
    }
}
