using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;


namespace RaceServer;

public class Room
{
    public string Name { get; }
    public string? Password { get; set; } = null;
    private readonly List<User> users = [];
    public Match? CurrentMatch = null;

    public IReadOnlyList<User> Users => users;

    public Room(string name)
    {
        Name = name;
        CurrentMatch = NewMatch();
    }

    public void AddPlayer(User user)
    {
        users.Add(user);
    }

    public void RemovePlayer(string name)
    {
        User user = users.FirstOrDefault(p => p.Name == name);

        if (user is not null)
        {
            users.Remove(user);
        }
    }

    public void TogglePlayerReady(string name)
    {
        User player = users.FirstOrDefault(p => p.Name == name);
        if (player is not null && player.Role == "Player")
            player.Ready = !player.Ready;
    }

    public void AssignSeedingTemp()
    {
        int seed = 1;
        foreach (User user in users)
        {
            if (user.Role == "Player")
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

    public Match NewMatch()
    {
        return new();
    }
}

public class User
{
    public string Name { get; set; }
    public string Role { get; set; } = "Player";
    public bool Host { get; set; }
    public bool Ready { get; set; } = true;
    public int? Seed { get; set; } = null;
}

public class Match
{
    public List<User> Players { get; set; } = [];
    public int Finishers { get; set; }
    public int BestOf { get; set; }
    public int CurrentGame { get; set; }
    public (int h, int l) Advantage { get; set; }
    public (int h, int l) Score { get; set; }
    public List<GameReport> GameReports { get; set; } = [];
    public string? Category { get; set; }
    public List<SeedType> SeedTypes { get; set; } = [];
    public bool BansOn { get; set; }
    public bool UnbansOn { get; set; }

    //public Match(List<User> players, int finishers = 1, int bestOf = 5, (int h, int l)? advantage = null, string category = "any%", List<SeedType>? seedTypes = null, bool bansOn = false, bool unbansOn = false)
    //{
    //    Players = players;
    //    Finishers = Math.Max(1, Math.Min(players.Count, finishers));
    //    BestOf = players.Count == 2 ? bestOf: 1;
    //    CurrentGame = 1;
    //    Advantage = advantage ?? (0, 0);
    //    Score = players.Count == 2 ? advantage ?? (0, 0) : (0, 0);
    //    GameReports = [];
    //    GameReports.Add(new());
    //    Category = category;
    //    SeedTypes = seedTypes ?? [];
    //    BansOn = bansOn;
    //    UnbansOn = unbansOn;
    //}
}

public class SeedType(bool isSurface, int type, bool isBanned, bool isAvailable)
{
    public bool IsSurface { get; set; } = isSurface;
    public int Type { get; set; } = type;
    public bool IsBanned { get; set; } = isBanned;
    public bool IsAvailable { get; set; } = isAvailable;
}

public class GameReport
{
    public string? Time { get; set; } = null;
    public double? Percentage { get; set; } = null;
    public int? SurfaceType { get; set; } = null;
    public int? SurfacePicker { get; set; } = null;
    public int? CaveType { get; set; } = null;
    public int? CavePicker { get; set; } = null;
    public bool HigherWin { get; set; } = false;
    public bool LowerWin { get; set; } = false;
}