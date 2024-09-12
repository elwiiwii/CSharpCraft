using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RaceServer.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
builder.Services.AddSingleton(typeof(GameServer));

var app = builder.Build();
app.MapGrpcService<GameServer>();

app.Run();
