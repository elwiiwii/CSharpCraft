using AccountService;
using Grpc.Net.Client;
using System.Text.Json;
using Grpc.Core;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using System.Security.Cryptography;

namespace CSharpCraft.Competitive;

public static class AccountHandler
{
    private static string? _userId;
    private static string? _permanentToken;
    private static readonly string _clientId = GetPersistentClientId();
    private static string TOKEN_FILE => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FirebaseTest2",
        $"auth_tokens_{_clientId}.json"
    );
    private static readonly byte[] _encryptionKey = GetOrCreateEncryptionKey();
    private static readonly byte[] _iv = new byte[16];
    private static GrpcChannel? _channel;
    private static AccountService.AccountService.AccountServiceClient? _client;
    private const string SERVER_URL = "https://localhost:5072";
    private static CancellationTokenSource? _tokenCheckCts;
    private static Task? _tokenCheckTask;
    public static bool _isLoggedIn;
    private const int TOKEN_CHECK_INTERVAL_MS = 2000; // 2 seconds

    static AccountHandler()
    {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        try
        {
            StopTokenCheck();
            _channel?.Dispose();
            _tokenCheckCts?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during process exit cleanup: {ex.Message}");
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            Console.WriteLine($"Unhandled exception: {e.ExceptionObject}");
            StopTokenCheck();
            _channel?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during unhandled exception cleanup: {ex.Message}");
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        try
        {
            Console.WriteLine($"Unobserved task exception: {e.Exception}");
            StopTokenCheck();
            _channel?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during unobserved task exception cleanup: {ex.Message}");
        }
    }

    public static async Task Shutdown()
    {
        try
        {
            if (_isLoggedIn && _permanentToken is not null)
            {
                await ForceLogout();
            }

            StopTokenCheck();
            _channel?.Dispose();
            _tokenCheckCts?.Dispose();

            _channel = null;
            _client = null;
            _isLoggedIn = false;
            _permanentToken = null;
            _userId = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during shutdown: {ex.Message}");
        }
    }

    class AuthTokens
    {
        public string? UserId { get; set; }
        public string? PermanentToken { get; set; }
    }

    private static string GetPersistentClientId()
    {
        try
        {
            var clientIdFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_id.txt");
            if (File.Exists(clientIdFile))
            {
                return File.ReadAllText(clientIdFile).Trim();
            }

            var newClientId = Guid.NewGuid().ToString();

            var directory = Path.GetDirectoryName(clientIdFile);
            if (directory is not null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(clientIdFile, newClientId);
            return newClientId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not create persistent client ID: {ex.Message}");
            return Guid.NewGuid().ToString();
        }
    }

    private static void LoadTokens()
    {
        try
        {
            if (File.Exists(TOKEN_FILE))
            {
                var json = File.ReadAllText(TOKEN_FILE);
                var tokens = JsonSerializer.Deserialize<AuthTokens>(json);
                if (tokens is not null)
                {
                    _userId = tokens.UserId;
                    _permanentToken = tokens.PermanentToken;
                }
            }
            else if (File.Exists(TOKEN_FILE + ".bak"))
            {
                Console.WriteLine($"Warning: Token file not found. Attempting to load backup.");
                var json = File.ReadAllText(TOKEN_FILE + ".bak");
                var tokens = JsonSerializer.Deserialize<AuthTokens>(json);
                if (tokens is not null)
                {
                    _userId = tokens.UserId;
                    _permanentToken = tokens.PermanentToken;
                    SaveTokens();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not load tokens: {ex.Message}");
            throw;
        }
    }

    private static void SaveTokens()
    {
        try
        {
            if (_userId is null || _permanentToken is null)
            {
                Console.WriteLine("Warning: Cannot save tokens, one or more are null.");
                return;
            }

            var tokens = new AuthTokens
            {
                UserId = _userId,
                PermanentToken = _permanentToken
            };
            var json = JsonSerializer.Serialize(tokens);

            var directory = Path.GetDirectoryName(TOKEN_FILE);
            if (directory is not null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var tempFile = TOKEN_FILE + ".tmp";
            File.WriteAllText(tempFile, json);

            if (File.Exists(TOKEN_FILE))
            {
                File.Delete(TOKEN_FILE + ".bak");
            }
            File.Move(tempFile, TOKEN_FILE);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not save tokens: {ex.Message}");
            throw;
        }
    }

    private static string Encrypt(string plainText)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.IV = _iv;
            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var srEncrypt = new StreamWriter(csEncrypt);
            {
                srEncrypt.Write(plainText);
            }
            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not encrypt token: {ex.Message}");
            throw;
        }
    }

    private static string Decrypt(string encryptedText)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.IV = _iv;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedText));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not decrypt token: {ex.Message}");
            throw;
        }
    }

    private static byte[] GetOrCreateEncryptionKey()
    {
        var keyFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CSharpCraft",
            $"encryption_key_{_clientId}.bin"
        );

        try
        {
            if (File.Exists(keyFile))
            {
                return File.ReadAllBytes(keyFile);
            }

            var key = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            var directory = Path.GetDirectoryName(keyFile);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(keyFile, key);
            return key;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not create encryption key: {ex.Message}");
            throw;
        }
    }

    public static async Task ConnectToServer()
    {
        try
        {
            Console.WriteLine($"Attempting to connect to server at {SERVER_URL}...");
            
            var channelOptions = new GrpcChannelOptions
            {
                HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                    KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests,
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    ConnectTimeout = TimeSpan.FromSeconds(10),
                    PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                    MaxConnectionsPerServer = 1,
                    SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                    {
                        EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13,
                        RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            Console.WriteLine($"SSL Certificate validation: {sslPolicyErrors}");
                            if (sslPolicyErrors != System.Net.Security.SslPolicyErrors.None && certificate != null)
                            {
                                Console.WriteLine($"Certificate details:");
                                Console.WriteLine($"  Subject: {certificate.Subject}");
                                Console.WriteLine($"  Issuer: {certificate.Issuer}");
                                Console.WriteLine($"  Valid from: {certificate.GetEffectiveDateString()}");
                                Console.WriteLine($"  Valid to: {certificate.GetExpirationDateString()}");
                            }
                            return true;
                        }
                    }
                }
            };

            Console.WriteLine("Creating gRPC channel...");
            _channel = GrpcChannel.ForAddress(SERVER_URL, channelOptions);
            Console.WriteLine("Creating gRPC client...");
            _client = new AccountService.AccountService.AccountServiceClient(_channel);

            try
            {
                Console.WriteLine("Attempting to establish connection...");
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                try
                {
                    await _channel.ConnectAsync(cts.Token);
                    Console.WriteLine("Successfully connected to server.");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Connection attempt timed out after 15 seconds.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection attempt failed:");
                Console.WriteLine($"  Error Type: {ex.GetType().Name}");
                Console.WriteLine($"  Error Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"  Inner Error Type: {ex.InnerException.GetType().Name}");
                    Console.WriteLine($"  Inner Error Message: {ex.InnerException.Message}");
                }
                if (ex is RpcException rpcEx)
                {
                    Console.WriteLine($"  gRPC Status Code: {rpcEx.Status.StatusCode}");
                    Console.WriteLine($"  gRPC Status Detail: {rpcEx.Status.Detail}");
                }
                throw new Exception("Failed to connect to server. Please ensure the server is running.", ex);
            }

            if (_permanentToken is not null && _userId is not null)
            {
                try
                {
                    var response = await _client.CheckTokenAsync(new CheckTokenRequest
                    {
                        UserId = _userId,
                        Token = _permanentToken
                    });

                    if (response.IsValid)
                    {
                        _isLoggedIn = true;
                        await StartTokenCheck();
                    }
                    else
                    {
                        await ForceLogout();
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
                {
                    Console.WriteLine("Server is unavailable. Please try again later.");
                    await ForceLogout();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to server: {ex.Message}");
            await DisconnectFromServer();
        }
    }

    private static async Task DisconnectFromServer()
    {
        try
        {
            StopTokenCheck();

            if (_channel is not null)
            {
                try
                {
                    _channel.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing channel: {ex.Message}");
                }
                _channel = null;
            }
            _client = null;
            _isLoggedIn = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during disconnect: {ex.Message}");
        }
    }

    private static async Task StartTokenCheck()
    {
        StopTokenCheck();

        if (_client is null || _permanentToken is null || _userId is null)
        {
            return;
        }

        _tokenCheckCts = new CancellationTokenSource();
        _tokenCheckTask = Task.Run(async () =>
        {
            try
            {
                while (!_tokenCheckCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (_client is null || _permanentToken is null || _userId is null)
                        {
                            return;
                        }

                        var response = await _client.CheckTokenAsync(new CheckTokenRequest
                        {
                            UserId = _userId,
                            Token = _permanentToken
                        });

                        if (!response.IsValid)
                        {
                            Console.WriteLine("Permanent token is invalid. Logging out...");
                            await ForceLogout();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking token: {ex.Message}");
                        await ForceLogout();
                        return;
                    }

                    try
                    {
                        await Task.Delay(TOKEN_CHECK_INTERVAL_MS, _tokenCheckCts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token check failed: {ex.Message}");
                await ForceLogout();
            }
        }, _tokenCheckCts.Token);
    }

    private static void StopTokenCheck()
    {
        if (_tokenCheckCts is not null)
        {
            try
            {
                _tokenCheckCts.Cancel();
                _tokenCheckCts.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping token check: {ex.Message}");
            }
            finally
            {
                _tokenCheckCts = null;
                _tokenCheckTask = null;
            }
        }
    }

    private static async Task ForceLogout()
    {
        try
        {
            if (_client is not null && !string.IsNullOrEmpty(_permanentToken))
            {
                try
                {
                    await _client.LogoutAsync(new LogoutRequest
                    {
                        PermanentToken = _permanentToken
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during logout request: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during logout: {ex.Message}");
        }
        finally
        {
            StopTokenCheck();
            _permanentToken = null;
            _userId = null;
            _isLoggedIn = false;
            ClearTokens();
        }
    }

    private static void ClearTokens()
    {
        _permanentToken = null;
        _userId = null;
        try
        {
            if (File.Exists(TOKEN_FILE))
            {
                var backupFile = TOKEN_FILE + ".bak";
                if (File.Exists(backupFile))
                {
                    try
                    {
                        File.Delete(backupFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting backup token file: {ex.Message}");
                    }
                }
                try
                {
                    File.Move(TOKEN_FILE, backupFile);
                    File.Delete(TOKEN_FILE);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error moving token file: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing tokens: {ex.Message}");
        }
    }

    public static async Task<VerifyEmailBeforeRegistrationResponse> VerifyEmailBeforeRegistration(string email)
    {
        if (_client is null)
        {
            await ConnectToServer();
            if (_client is null)
            {
                return new VerifyEmailBeforeRegistrationResponse
                {
                    Success = false,
                    Message = "Not connected to server."
                };
            }
        }

        try
        {
            return await _client.VerifyEmailBeforeRegistrationAsync(new VerifyEmailBeforeRegistrationRequest
            {
                Email = email
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying email: {ex.Message}");
            return new VerifyEmailBeforeRegistrationResponse
            {
                Success = false,
                Message = $"Error verifying email: {ex.Message}"
            };
        }
    }

    public static async Task<RegisterResponse> Register(string email, string username, string password, string verificationCode)
    {
        if (_client == null)
        {
            await ConnectToServer();
            if (_client == null)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Not connected to server."
                };
            }
        }

        try
        {
            var response = await _client.RegisterAsync(new RegisterRequest
            {
                Email = email,
                Username = username,
                Password = password,
                VerificationCode = verificationCode
            });

            if (response.Success)
            {
                _userId = response.UserId;
                _isLoggedIn = true;
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering user: {ex.Message}");
            return new RegisterResponse
            {
                Success = false,
                Message = $"Error registering user: {ex.Message}"
            };
        }
    }

    public static async Task<LoginResponse> Login(string email, string password)
    {
        if (_client == null)
        {
            await ConnectToServer();
            if (_client == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Not connected to server."
                };
            }
        }

        try
        {
            var response = await _client.LoginAsync(new LoginRequest
            {
                Email = email,
                Password = password,
            });

            if (response.Success)
            {
                if (response.RequiresTwoFactor)
                {
                    return response;
                }
                else
                {
                    _userId = response.UserId;
                    _permanentToken = response.PermanentToken;
                    _isLoggedIn = true;
                    SaveTokens();
                    await StartTokenCheck();
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                Message = $"Error during login: {ex.Message}"
            };
        }
    }

    public static async Task<Verify2FAResponse> Verify2FA(string email, string verificationCode)
    {
        if (_client is null)
        {
            await ConnectToServer();
            if (_client is null)
            {
                return new Verify2FAResponse
                {
                    Success = false,
                    Message = "Not connected to server."
                };
            }
        }

        try
        {
            var response = await _client.Verify2FAAsync(new Verify2FARequest
            {
                Email = email,
                VerificationCode = verificationCode
            });

            if (response.Success)
            {
                _permanentToken = response.PermanentToken;
                _isLoggedIn = true;
                SaveTokens();
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error verifying 2FA: {ex.Message}");
            return new Verify2FAResponse
            {
                Success = false,
                Message = $"Error verifying 2FA: {ex.Message}"
            };
        }
    }
}
