using Grpc.Core;
using RaceServer;

namespace RaceServer.Services
{
    public class StreamTestService : StreamTest.StreamTestBase
    {
        private readonly ILogger<StreamTestService> _logger;

        public StreamTestService(ILogger<StreamTestService> logger)
        {
            _logger = logger;
        }

        public override async Task SendMessage(
            IAsyncStreamReader<ClientToServerMessage> requestStream,
            IServerStreamWriter<ServerToClientMessage> responseStream,
            ServerCallContext context)
        {
            var clientToServerTask = ClientToServerPingHandlingAsync(requestStream, context);
            var serverToClientTask = ServerToClientPingHandlingAsync(responseStream, context);

            await Task.WhenAll(clientToServerTask, serverToClientTask);
        }

        private static async Task ServerToClientPingHandlingAsync(IServerStreamWriter<ServerToClientMessage> responseStream, ServerCallContext context)
        {
            var pingCount = 0;
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(new ServerToClientMessage
                {
                    Text = $"Server said hi {++pingCount}"
                });
                await Task.Delay(1000);
            }
        }

        private async Task ClientToServerPingHandlingAsync(IAsyncStreamReader<ClientToServerMessage> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext() && !context.CancellationToken.IsCancellationRequested)
            {
                var message = requestStream.Current;
                _logger.LogInformation("The client said: {Message}", message.Text);
            }
        }
    }
}