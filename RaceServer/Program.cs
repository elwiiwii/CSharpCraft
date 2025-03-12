using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RaceServer.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
builder.Services.AddSingleton(typeof(GameServer));

WebApplication app = builder.Build();
app.MapGrpcService<GameServer>();

app.Run();
