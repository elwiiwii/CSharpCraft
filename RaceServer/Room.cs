using Grpc.Net.Client;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;

namespace RaceServer
{
    public class Room(string name)
    {
        public string Name { get; } = name;
        public readonly List<User> users = new();
        public DuelMatch CurrentMatch = new();

        public IReadOnlyList<User> Users => users;

        public void AddPlayer(User user)
        {
            users.Add(user);
        }

        public void RemovePlayer(string name)
        {
            var user = users.FirstOrDefault(p => p.Name == name);

            if (user != null)
            {
                users.Remove(user);
            }
        }
        
        public void TogglePlayerReady(string name)
        {
            var player = users.FirstOrDefault(p => p.Name == name);
            if (player is not null && player.Role == "Player")
                player.Ready = !player.Ready;
        }

        public void AssignSeedingTemp()
        {
            int seed = 1;
            foreach (var user in users)
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
            return new DuelMatch{ HigherSeed = higherSeed, LowerSeed = lowerSeed };
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

}
