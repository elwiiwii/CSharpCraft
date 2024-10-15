using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;

namespace RaceServer
{
    public class Room(string name)
    {
        public string Name { get; } = name;
        public readonly List<User> users = new();

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

        public bool AllPlayersReady()
        {
            return users.All(p => p.Ready);
        }
    }

    public class User
    {
        public string Name { get; set; }
        public string Role { get; set; } = "Player";
        public bool Host { get; set; }
        public bool Ready { get; set; } = true;
    }

    public class DuelMatch
    {
        public int currentGame { get; set; } = 1;
        public int bestOf { get; set; } = 5;
        public string category { get; set; } = "any%";
        public int finishers { get; set; } = 1;
        public bool unbans { get; set; } = true;
        public (int, int) advantage { get; set; } = (0, 0);
    }

    public class GroupMatch
    {
        public string category { get; set; } = "any%";
        public int finishers { get; set; } = 1;
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
