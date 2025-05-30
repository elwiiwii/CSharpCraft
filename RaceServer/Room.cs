using System.Collections.Generic;
using System.Linq;

namespace RaceServer
{
    public class Room
    {
        public string Name { get; }
        private readonly List<RoomPlayer> _users = new();
        private DuelMatch? _currentMatch;

        public IReadOnlyList<RoomPlayer> Users => _users.AsReadOnly();
        public DuelMatch? CurrentMatch => _currentMatch;

        public Room(string name)
        {
            Name = name;
        }

        public (bool success, string message) AddUser(RoomPlayer user)
        {
            if (_users.Any(u => u.Username == user.Username))
            {
                return (false, "User already in room");
            }

            if (user.Role == "Player" && _users.Count(u => u.Role == "Player") >= 2)
            {
                return (false, "Room is full");
            }

            user.IsHost = _users.Count == 0;
            _users.Add(user);
            return (true, "User added successfully");
        }

        public (bool success, string message) RemoveUser(string username)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            if (user is null)
            {
                return (false, "User not found in room");
            }

            _users.Remove(user);

            // If the host left, assign a new host
            if (user.IsHost && _users.Any())
            {
                _users[0].IsHost = true;
            }

            return (true, "User removed successfully");
        }

        public (bool success, string message) SetUserReady(string username, bool ready)
        {
            var user = _users.FirstOrDefault(u => u.Username == username);
            if (user is null)
            {
                return (false, "User not found in room");
            }

            if (user.Role != "Player")
            {
                return (false, "Only players can set ready status");
            }

            user.IsReady = ready;
            return (true, "Ready status updated successfully");
        }

        public bool AllPlayersReady()
        {
            return _users.Where(u => u.Role == "Player").All(u => u.IsReady);
        }

        public (bool success, string message) StartMatch()
        {
            if (!AllPlayersReady())
            {
                return (false, "Not all players are ready");
            }

            var players = _users.Where(u => u.Role == "Player").ToList();
            if (players.Count != 2)
            {
                return (false, "Need exactly 2 players to start a match");
            }

            // Assign seeds
            players[0].Seed = 1;
            players[1].Seed = 2;

            _currentMatch = new DuelMatch
            {
                HigherSeed = players[0],
                LowerSeed = players[1]
            };

            return (true, "Match started successfully");
        }

        public (bool success, string message) EndMatch(string winner)
        {
            if (_currentMatch is null)
            {
                return (false, "No active match");
            }

            _currentMatch = null;
            foreach (var user in _users.Where(u => u.Role == "Player"))
            {
                user.IsReady = false;
                user.Seed = null;
            }

            return (true, "Match ended successfully");
        }
    }

    public class RoomPlayer
    {
        public string Username { get; set; }
        public string Role { get; set; } = "Player";
        public bool IsHost { get; set; }
        public bool IsReady { get; set; }
        public int? Seed { get; set; }
    }

    public class DuelMatch
    {
        public RoomPlayer HigherSeed { get; set; }
        public RoomPlayer LowerSeed { get; set; }
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
