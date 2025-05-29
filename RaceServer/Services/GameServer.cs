using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using AccountService;

namespace RaceServer.Services;

public class GameServer : GameService.GameServiceBase
{
    private readonly List<IServerStreamWriter<RoomStreamResponse>> clients = new();
    private readonly Dictionary<string, IServerStreamWriter<RoomStreamResponse>> clientStreams = new();
    private readonly Room room = new("TestRoom");
    private readonly ILogger<GameServer> logger;
    private readonly AccountService.AccountService.AccountServiceClient _accountClient;

    public GameServer(ILogger<GameServer> logger, AccountService.AccountService.AccountServiceClient accountClient)
    {
        this.logger = logger;
        this._accountClient = accountClient;
    }

    private async Task<bool> ValidateToken(string userId, string token)
    {
        try
        {
            var response = await _accountClient.CheckTokenAsync(new CheckTokenRequest
            {
                UserId = userId,
                Token = token
            });
            return response.IsValid;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error validating token: {ex.Message}");
            return false;
        }
    }

    public override async Task RoomStream(RoomStreamRequest request, IServerStreamWriter<RoomStreamResponse> responseStream, ServerCallContext context)
    {
        if (!await ValidateToken(request.UserId, request.PermanentToken))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid token"));
        }

        clients.Add(responseStream);
        clientStreams[request.UserId] = responseStream;

        try
        {
            var userResponse = await _accountClient.GetUserByUsernameAsync(new GetUserByUsernameRequest
            {
                Username = request.UserId // This should be changed to use GetUserById when implemented
            });

            if (!userResponse.Success)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
            }

            User newUser = new()
            {
                UserId = request.UserId,
                Username = userResponse.Username,
                Role = request.Role,
                Host = room.Users.Count == 0,
                Ready = request.Role == "Player" ? false : true,
                ProfilePicture = userResponse.ProfilePicture,
                NameColor = userResponse.NameColor,
                ShadowColor = userResponse.ShadowColor,
                OutlineColor = userResponse.OutlineColor,
                BackgroundColor = userResponse.BackgroundColor,
                HexCodes = userResponse.HexCodes.ToList()
            };

            room.AddPlayer(newUser);

            var notification = new RoomStreamResponse
            {
                JoinRoomNotification = new JoinRoomNotification()
            };

            foreach (User user in room.Users)
            {
                notification.JoinRoomNotification.Users.Add(new RoomUser
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = user.Role,
                    Host = user.Host,
                    Ready = user.Ready,
                    Seed = user.Seed ?? 0,
                    ProfilePicture = user.ProfilePicture,
                    NameColor = user.NameColor,
                    ShadowColor = user.ShadowColor,
                    OutlineColor = user.OutlineColor,
                    BackgroundColor = user.BackgroundColor
                });
                notification.JoinRoomNotification.Users[^1].HexCodes.AddRange(user.HexCodes);
            }

            notification.JoinRoomNotification.AllReady = room.AllPlayersReady();

            foreach (var client in clients)
            {
                await client.WriteAsync(notification);
            }

            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
        }
        finally
        {
            room.RemovePlayer(request.UserId);
            clients.Remove(responseStream);
            clientStreams.Remove(request.UserId);

            var leaveNotification = new RoomStreamResponse
            {
                LeaveRoomNotification = new LeaveRoomNotification
                {
                    Username = request.UserId // This should be the username, not the ID
                }
            };

            foreach (User user in room.Users)
            {
                leaveNotification.LeaveRoomNotification.Users.Add(new RoomUser
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = user.Role,
                    Host = user.Host,
                    Ready = user.Ready,
                    Seed = user.Seed ?? 0,
                    ProfilePicture = user.ProfilePicture,
                    NameColor = user.NameColor,
                    ShadowColor = user.ShadowColor,
                    OutlineColor = user.OutlineColor,
                    BackgroundColor = user.BackgroundColor
                });
                leaveNotification.LeaveRoomNotification.Users[^1].HexCodes.AddRange(user.HexCodes);
            }

            foreach (var client in clients)
            {
                await client.WriteAsync(leaveNotification);
            }
        }
    }

    public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
    {
        if (!await ValidateToken(request.UserId, request.PermanentToken))
        {
            return new JoinRoomResponse
            {
                Success = false,
                Message = "Invalid token"
            };
        }

        var userResponse = await _accountClient.GetUserByUsernameAsync(new GetUserByUsernameRequest
        {
            Username = request.UserId // This should be changed to use GetUserById when implemented
        });

        if (!userResponse.Success)
        {
            return new JoinRoomResponse
            {
                Success = false,
                Message = "User not found"
            };
        }

        return new JoinRoomResponse
        {
            Success = true,
            Message = "Successfully joined room",
            User = new RoomUser
            {
                UserId = request.UserId,
                Username = userResponse.Username,
                Role = request.Role,
                Host = room.Users.Count == 0,
                Ready = request.Role == "Player" ? false : true,
                ProfilePicture = userResponse.ProfilePicture,
                NameColor = userResponse.NameColor,
                ShadowColor = userResponse.ShadowColor,
                OutlineColor = userResponse.OutlineColor,
                BackgroundColor = userResponse.BackgroundColor
            }
        };
    }

    public override async Task<PlayerReadyResponse> PlayerReady(PlayerReadyRequest request, ServerCallContext context)
    {
        if (!await ValidateToken(request.UserId, request.PermanentToken))
        {
            return new PlayerReadyResponse
            {
                Success = false,
                Message = "Invalid token",
                Ready = false
            };
        }

        room.TogglePlayerReady(request.UserId);

        var notification = new RoomStreamResponse
        {
            PlayerReadyNotification = new PlayerReadyNotification()
        };

        foreach (User user in room.Users)
        {
            notification.PlayerReadyNotification.Users.Add(new RoomUser
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role,
                Host = user.Host,
                Ready = user.Ready,
                Seed = user.Seed ?? 0,
                ProfilePicture = user.ProfilePicture,
                NameColor = user.NameColor,
                ShadowColor = user.ShadowColor,
                OutlineColor = user.OutlineColor,
                BackgroundColor = user.BackgroundColor
            });
            notification.PlayerReadyNotification.Users[^1].HexCodes.AddRange(user.HexCodes);
        }

        notification.PlayerReadyNotification.AllReady = room.AllPlayersReady();

        foreach (var client in clients)
        {
            await client.WriteAsync(notification);
        }

        var myself = room.Users.FirstOrDefault(p => p.UserId == request.UserId);
        return new PlayerReadyResponse
        {
            Success = true,
            Message = "Successfully toggled ready status",
            Ready = myself?.Ready ?? false
        };
    }

    public override async Task<StartMatchResponse> StartMatch(StartMatchRequest request, ServerCallContext context)
    {
        if (!await ValidateToken(request.UserId, request.PermanentToken))
        {
            return new StartMatchResponse
            {
                Success = false,
                Message = "Invalid token"
            };
        }

        room.AssignSeedingTemp();
        User higherSeed = room.Users.FirstOrDefault(p => p.Seed == 1);
        User lowerSeed = room.Users.FirstOrDefault(p => p.Seed == 2);
        room.CurrentMatch = room.NewDuelMatch(higherSeed, lowerSeed);

        var notification = new RoomStreamResponse
        {
            StartMatchNotification = new StartMatchNotification
            {
                MatchStarted = room.AllPlayersReady()
            }
        };

        foreach (User user in room.Users.Where(u => u.Role == "Player"))
        {
            notification.StartMatchNotification.Players.Add(new RoomUser
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role,
                Host = user.Host,
                Ready = user.Ready,
                Seed = user.Seed ?? 0,
                ProfilePicture = user.ProfilePicture,
                NameColor = user.NameColor,
                ShadowColor = user.ShadowColor,
                OutlineColor = user.OutlineColor,
                BackgroundColor = user.BackgroundColor
            });
            notification.StartMatchNotification.Players[^1].HexCodes.AddRange(user.HexCodes);
        }

        foreach (var client in clients)
        {
            await client.WriteAsync(notification);
        }

        return new StartMatchResponse
        {
            Success = true,
            Message = "Match started successfully"
        };
    }
}