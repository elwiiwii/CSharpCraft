using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RaceServer.Services;
using System.Text;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

var jwtSecret = builder.Configuration["JWT_SECRET_KEY"]
             ?? throw new Exception("JWT_SECRET_KEY not configured!");
var keyBytes = Convert.FromBase64String(jwtSecret);
var securityKey = new SymmetricSecurityKey(keyBytes);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddGrpc();
builder.Services.AddSingleton(typeof(GameServer));
builder.Services.AddSingleton(securityKey);

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<GameServer>();

app.Run();