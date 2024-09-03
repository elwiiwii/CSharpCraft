using Grpc.Core;
using RaceServer;

namespace RaceServer.Services
{
    public class RoomHandlerService : RoomHandler.RoomHandlerBase
    {
        private readonly ILogger<RoomHandlerService> _logger;
        public RoomHandlerService(ILogger<RoomHandlerService> logger)
        {
            _logger = logger;
        }

        public override Task<JoinResponse> JoinRoom(JoinRequest request, ServerCallContext context)
        {
            return Task.FromResult(new JoinResponse
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
