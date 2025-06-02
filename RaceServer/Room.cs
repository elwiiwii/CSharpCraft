using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace RaceServer;

public class Room(string name)
{
    public string Name { get; } = name;
    private readonly List<User> users = new();
    public DuelMatch CurrentMatch = new();

    public IReadOnlyList<User> Users => users;

    public void AddPlayer(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        Console.WriteLine($"[Room {Name}] Adding player {user.Name}. Current users: {string.Join(", ", users.Select(u => u.Name))}");
        users.Add(user);
        Console.WriteLine($"[Room {Name}] After adding {user.Name}. Current users: {string.Join(", ", users.Select(u => u.Name))}");
    }

    public void RemovePlayer(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        }

        Console.WriteLine($"[Room {Name}] Removing player {name}. Current users: {string.Join(", ", users.Select(u => u.Name))}");
        User user = users.FirstOrDefault(p => p.Name == name);

        if (user is not null)
        {
            users.Remove(user);
            Console.WriteLine($"[Room {Name}] After removing {name}. Current users: {string.Join(", ", users.Select(u => u.Name))}");
        }
        else
        {
            Console.WriteLine($"[Room {Name}] Player {name} not found in users list");
        }
    }

    public void TogglePlayerReady(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Name cannot be null or empty", nameof(name));
        }

        Console.WriteLine($"[Room {Name}] Toggling ready for {name}. Current users: {string.Join(", ", users.Select(u => u.Name))}");
        User player = users.FirstOrDefault(p => p.Name == name);
        if (player is not null && player.Role == "Player")
        {
            player.Ready = !player.Ready;
            Console.WriteLine($"[Room {Name}] {name} ready state set to {player.Ready}");
        }
        else
        {
            Console.WriteLine($"[Room {Name}] Player {name} not found or not a player");
        }
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

    public DuelMatch NewDuelMatch(User higherSeed, User lowerSeed)
    {
        if (higherSeed == null)
        {
            throw new ArgumentNullException(nameof(higherSeed));
        }
        if (lowerSeed == null)
        {
            throw new ArgumentNullException(nameof(lowerSeed));
        }
        return new DuelMatch { HigherSeed = higherSeed, LowerSeed = lowerSeed };
    }
}

public class User
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "Player";
    public bool Host { get; set; }
    public bool Ready { get; set; } = true;
    public int? Seed { get; set; } = null;
}

public class DuelMatch
{
    public User HigherSeed { get; set; }
    public User LowerSeed { get; set; }
    public SeedTypes SeedTypes { get; set; } = new();
    public int CurrentGame { get; set; } = 1;
    public int BestOf { get; set; } = 5;
    public string Category { get; set; } = "any%";
    public int Finishers { get; set; } = 1;
    public bool Unbans { get; set; } = true;
    public (int, int) Advantage { get; set; } = (0, 0);
}

public class GroupMatch
{
    public string Category { get; set; } = "any%";
    public int Finishers { get; set; } = 1;
}

public class SeedTypes
{
    public string Type1Status { get; set; } = "UNBANNED";
    public string Type2Status { get; set; } = "UNBANNED";
    public string Type3Status { get; set; } = "UNBANNED";
    public string Type4Status { get; set; } = "UNBANNED";
    public string Type5Status { get; set; } = "UNBANNED";
    public string Type6Status { get; set; } = "UNBANNED";
    public string Type7Status { get; set; } = "UNBANNED";
}

public class GameReport
{
    public string Player1Status { get; set; }
    public string Player2Status { get; set; }
    public double FinishTime { get; set; }
}