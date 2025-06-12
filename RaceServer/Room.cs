using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using RaceServer.Services;


namespace RaceServer;

public class Room
{
    public string Name { get; }
    public string? Password { get; set; } = null;
    private List<RoomUser> users = [];
    public Match? CurrentMatch = null;
    private readonly ILogger<GameServer> logger;

    public IReadOnlyList<RoomUser> Users => users;

    public Room(string name, string? password, ILogger<GameServer> logger)
    {
        Name = name;
        Password = password;
        CurrentMatch = new Match(users);
        this.logger = logger;
    }

    public void AddPlayer(RoomUser user)
    {
        if (users.FirstOrDefault(p => p.Host) is not null && user.Host) user.Host = false;
        users.Add(user);
        if (CurrentMatch is null) CurrentMatch = new Match(users);
        CurrentMatch.UpdateAll(logger, users);
    }

    public void RemovePlayer(string name)
    {
        RoomUser user = users.FirstOrDefault(p => p.Name == name);

        if (user is not null)
        {
            users.Remove(user);
            if (user.Host) users[0].Host = true;
        }
        if (CurrentMatch is null) CurrentMatch = new Match(users);
        CurrentMatch.UpdateAll(logger, users);
    }

    public void TogglePlayerReady(string name)
    {
        RoomUser player = users.FirstOrDefault(p => p.Name == name);
        if (player is not null && player.Role == Role.Player)
            player.Ready = !player.Ready;
    }

    public void AssignSeedingTemp()
    {
        int seed = 1;
        foreach (RoomUser user in users)
        {
            if (user.Role == Role.Player)
            {
                user.Seed = seed;
                seed++;
            }
        }
    }

    public bool AllPlayersReady()
    {
        return users.All(p => p.Ready);
    }

    public void StartMatch(ILogger logger)
    {
        if (CurrentMatch is null)
        {
            logger.LogError("Cannot start match: CurrentMatch is null.");
            return;
        }
        if (users.Count < 2)
        {
            logger.LogError("Cannot start match: Not enough players.");
            return;
        }
        if (!AllPlayersReady())
        {
            logger.LogError("Cannot start match: Not all players are ready.");
            return;
        }
    }
}

public class Match
{
    public MatchStatus Status { get; private set; }
    public List<RoomUser> Players { get; private set; } = [];
    public int Finishers { get; private set; }
    public int BestOf { get; private set; }
    public int CurrentGame { get; private set; }
    public (int h, int l) Advantage { get; private set; }
    public (int h, int l) Score { get; private set; }
    public List<GameReport> GameReports { get; private set; } = [];
    public Category Category { get; private set; }
    public List<SeedType> SeedTypes { get; private set; } = [];
    public bool PicksOn { get; private set; }
    public bool BansOn { get; private set; }
    public bool UnbansOn { get; private set; }
    private static readonly List<SeedType> defaultSeeds = [
        new() { IsSurface = true, Type = 1, Status = SeedStatus.Available },
        new() { IsSurface = true, Type = 2, Status = SeedStatus.Available },
        new() { IsSurface = true, Type = 3, Status = SeedStatus.Available },
        new() { IsSurface = true, Type = 4, Status = SeedStatus.Available },
        new() { IsSurface = true, Type = 5, Status = SeedStatus.Available },
        new() { IsSurface = false, Type = 1, Status = SeedStatus.Available },
        new() { IsSurface = false, Type = 2, Status = SeedStatus.Available },
        new() { IsSurface = false, Type = 3, Status = SeedStatus.Available },
        new() { IsSurface = false, Type = 4, Status = SeedStatus.Available },
        new() { IsSurface = false, Type = 5, Status = SeedStatus.Available },
        ];

    public Match(List<RoomUser> players, Match? match = null)
    {
        Status = MatchStatus.MatchWaiting;
        players = players.Where(x => x.Role == Role.Player).ToList();
        Players = players;
        Finishers = match?.Finishers ?? 1;
        BestOf = match?.BestOf ?? (players.Count == 2 ? 5 : 1);
        CurrentGame = 1;
        Advantage = match?.Advantage ?? (0, 0);
        Score = players.Count == 2 ? match?.Advantage ?? (0, 0) : (0, 0);
        GameReports = [];
        GameReports.Add(new());
        Category = match?.Category ?? Category.AnyVanilla;
        SeedTypes = match?.SeedTypes ?? defaultSeeds;
        PicksOn = match?.PicksOn ?? true;
        BansOn = match?.BansOn ?? false;
        UnbansOn = match?.UnbansOn ?? false;
    }

    public MatchState ToMatchState()
    {
        var matchState = new MatchState
        {
            Status = Status,
            Finishers = Finishers,
            BestOf = BestOf,
            CurrentGame = CurrentGame,
            AdvantageH = Advantage.h,
            AdvantageL = Advantage.l,
            ScoreH = Score.h,
            ScoreL = Score.l,
            Category = Category,
            PicksOn = PicksOn,
            BansOn = BansOn,
            UnbansOn = UnbansOn
        };

        foreach (RoomUser player in Players)
        {
            matchState.Players.Add(new RoomUser
            {
                Name = player.Name,
                Role = player.Role,
                Host = player.Host,
                Ready = player.Ready,
                Seed = player.Seed
            });
        }

        foreach (GameReport report in GameReports)
        {
            matchState.GameReports.Add(new GameReport
            {
                Time = report.Time,
                Percentage = report.Percentage,
                SurfaceType = report.SurfaceType,
                SurfacePicker = report.SurfacePicker,
                CaveType = report.CaveType,
                CavePicker = report.CavePicker,
                GameEnd = report.GameEnd
            });
        }

        foreach (SeedType seed in SeedTypes)
        {
            matchState.SeedTypes.Add(new SeedType
            {
                IsSurface = seed.IsSurface,
                Type = seed.Type,
                Status = seed.Status
            });
        }

        return matchState;
    }

    public void UpdateAll(ILogger logger, List<RoomUser> players)
    {
        UpdatePlayers(logger, players);
        UpdateState(logger, MatchStatus.MatchWaiting);
        UpdateFinishers(logger, Finishers);
        UpdateBestOf(logger, players, BestOf);
        UpdateCurrentGame(logger, CurrentGame);
        UpdateAdvantage(logger, players, Advantage);
        UpdateScore(logger, players, Score);
        UpdateGameReports(logger, GameReports);
        UpdateCategory(logger, Category);
        UpdateSeedTypes(logger, SeedTypes);
        UpdatePicksOn(logger, PicksOn);
        UpdateBansOn(logger, BansOn);
        UpdateUnbansOn(logger, UnbansOn);
    }

    public void UpdateState(ILogger logger, MatchStatus status)
    {
        Status = status;
    }

    public void UpdatePlayers(ILogger logger, List<RoomUser> players)
    {
        Players = players.Where(x => x.Role == Role.Player).ToList();
    }

    public void UpdateFinishers(ILogger logger, int finishers)
    {
        if (finishers < 1)
        {
            logger.LogError("Cannot start match: Not enough players.");
            Finishers = 1;
            return;
        }
        if (finishers > Players.Count)
        {
            logger.LogError("Cannot set finishers: More finishers than players.");
            Finishers = Players.Count;
            return;
        }
        Finishers = finishers;
    }

    public void UpdateBestOf(ILogger logger, List<RoomUser> players, int bestOf)
    {
        UpdatePlayers(logger, players);
        if (Players.Count > 2)
        {
            BestOf = 1;
            return;
        }
        if (bestOf < 1)
        {
            logger.LogError("Best of must be at least 1. Setting to 1.");
            BestOf = 1;
            return;
        }
        BestOf = bestOf;
    }

    public void UpdateCurrentGame(ILogger logger, int curGame)
    {
        if (curGame < 1)
        {
            logger.LogError("Current game must be at least 1. Setting to 1.");
            CurrentGame = 1;
            return;
        }
        if (curGame > BestOf)
        {
            logger.LogError("Current game cannot be greater than Best of. Ending match.");
            // todo end match
            return;
        }
        CurrentGame = curGame;
    }

    public void UpdateAdvantage(ILogger logger, List<RoomUser> players, (int h, int l) adv)
    {
        UpdatePlayers(logger, players);
        if (Players.Count != 2)
        {
            logger.LogError("Advantage can only be set for 2 players. Setting to (0, 0).");
            Advantage = (0, 0);
            return;
        }
        if (adv.h < 0 || adv.l < 0)
        {
            logger.LogError("Advantage cannot be negative. Setting to (0, 0).");
            Advantage = (0, 0);
            return;
        }
        if (adv.h > Math.Round(BestOf / 2.0, MidpointRounding.AwayFromZero) || adv.l > Math.Round(BestOf / 2.0, MidpointRounding.AwayFromZero))
        {
            logger.LogError("Advantage cannot be greater than Best of. Setting to (0, 0).");
            Advantage = (0, 0);
            return;
        }
        Advantage = adv;
        Score = adv;
    }

    public void UpdateScore(ILogger logger, List<RoomUser> players, (int h, int l) score)
    {
        UpdatePlayers(logger, players);
        if (Players.Count != 2)
        {
            logger.LogError("Score can only be set for 2 players. Setting to (0, 0).");
            Score = (0, 0);
            return;
        }
        if (score.h < 0 || score.l < 0)
        {
            logger.LogError("Score cannot be negative. Setting to (0, 0).");
            Score = (0, 0);
            return;
        }
        if (score.h >= Math.Round(BestOf / 2.0, MidpointRounding.AwayFromZero) || score.l >= Math.Round(BestOf / 2.0, MidpointRounding.AwayFromZero))
        {
            // todo end match
            return;
        }
        Score = score;
    }

    public void SetSeed(ILogger logger, bool isSurface, int[] seed)
    {
        if (isSurface)
        {
            GameReports.LastOrDefault()?.SurfaceSeed.Clear();
            foreach (var tile in seed)
            {
                GameReports.LastOrDefault()?.SurfaceSeed.Add(tile);
            }
        }
        else
        {
            GameReports.LastOrDefault()?.CaveSeed.Clear();
            foreach (var tile in seed)
            {
                GameReports.LastOrDefault()?.CaveSeed.Add(tile);
            }
        }
    }

    public void UpdateGameReports(ILogger logger, List<GameReport> games)
    {
        GameReports = games;
    }

    public void UpdateCategory(ILogger logger, Category cat)
    {
        Category = cat;
    }

    public void UpdateSeedTypes(ILogger logger, List<SeedType> types)
    {
        SeedTypes = types;
    }

    public void UpdatePicksOn(ILogger logger, bool state)
    {
        PicksOn = state;
    }

    public void UpdateBansOn(ILogger logger, bool state)
    {
        BansOn = state;
    }

    public void UpdateUnbansOn(ILogger logger, bool state)
    {
        UnbansOn = BansOn && state;
    }
}

//public class SeedType(bool isSurface, int type, SeedStatus status = SeedStatus.Available)
//{
//    public bool IsSurface { get; set; } = isSurface;
//    public int Type { get; set; } = type;
//    public SeedStatus Status { get; set; } = status;
//}
//
//public class GameReport
//{
//    public string? Time { get; set; } = null;
//    public double? Percentage { get; set; } = null;
//    public int? SurfaceType { get; set; } = null;
//    public int? SurfacePicker { get; set; } = null;
//    public int? CaveType { get; set; } = null;
//    public int? CavePicker { get; set; } = null;
//    public GameStatus GameEnd { get; set; } = GameStatus.GameWaiting;
//}