using RaceServer.Services;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using RaceServer;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5072, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        listenOptions.UseHttps(httpsOptions =>
        {
            httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
        });
    });
});

// Initialize Firebase Admin SDK
var credentialsPath = Path.Combine(Environment.CurrentDirectory, "firebase-credentials.json");
if (!File.Exists(credentialsPath))
{
    throw new FileNotFoundException("Firebase credentials file not found. Please place firebase-credentials.json in the RaceServer directory.", credentialsPath);
}

var credentialsJson = File.ReadAllText(credentialsPath);
var projectId = System.Text.Json.JsonDocument.Parse(credentialsJson)
    .RootElement.GetProperty("project_id").GetString()
    ?? throw new InvalidOperationException("Project ID not found in Firebase credentials.");

if (FirebaseApp.DefaultInstance is null)
{
    var googleCredential = GoogleCredential.FromFile(credentialsPath);
    FirebaseApp.Create(new AppOptions
    {
        Credential = googleCredential
    });
}

// Initialize Firestore
var firestoreCredential = GoogleCredential.FromFile(credentialsPath);
var firestoreDb = new FirestoreDbBuilder
{
    ProjectId = projectId,
    Credential = firestoreCredential
}.Build();

// Initialize Azure Key Vault (if needed)
string? keyVaultUrl = null;
try
{
    keyVaultUrl = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URL");
    if (string.IsNullOrEmpty(keyVaultUrl))
    {
        Console.WriteLine("Warning: AZURE_KEY_VAULT_URL environment variable is not set. Some features may not work.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Error reading AZURE_KEY_VAULT_URL: {ex.Message}");
}

// Register services
builder.Services.AddGrpc();
builder.Services.AddSingleton<FirebaseAuth>(provider => FirebaseAuth.DefaultInstance);
builder.Services.AddSingleton<FirestoreDb>(provider => firestoreDb);
builder.Services.AddSingleton<TwoFactorService>();
builder.Services.AddSingleton<IKeyVaultService, KeyVaultService>();
builder.Services.AddSingleton<Room>(provider => new Room("TestRoom"));

// Configure Email Service
builder.Services.AddSingleton<EmailService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<EmailService>>();
    var keyVaultService = sp.GetRequiredService<IKeyVaultService>();

    return new EmailService(
        logger,
        firestoreDb,
        builder.Configuration["Email:SmtpServer"] ?? "smtp.gmail.com",
        int.Parse(builder.Configuration["Email:SmtpPort"] ?? "587"),
        keyVaultService.GetSecretAsync(builder.Configuration["Email:SmtpEmailSecretName"] ?? "EmailServiceUsername").Result,
        keyVaultService.GetSecretAsync("EmailServiceKey").Result
    );
});

builder.Services.AddSingleton<TwoFactorService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<TwoFactorService>>();
    var emailService = sp.GetRequiredService<EmailService>();
    return new TwoFactorService(firestoreDb, logger, emailService);
});

var app = builder.Build();

app.MapGrpcService<AccountServiceImpl>();
app.MapGrpcService<GameServer>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

//WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
//builder.Services.AddGrpc();
//builder.Services.AddSingleton(typeof(GameServer));
//
//WebApplication app = builder.Build();
//app.MapGrpcService<GameServer>();
//
//app.Run();
