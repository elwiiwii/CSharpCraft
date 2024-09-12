using System.Collections.Generic;
using System.Linq;

namespace RaceServer
{
    public class Room(string name)
    {
        public string Name { get; } = name;
        private readonly List<Player> players = new();
        private readonly List<Spectator> spectators = new();

        public IReadOnlyList<Player> Players => players;
        public IReadOnlyList<Spectator> Spectators => spectators;

        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public void AddSpectator(Spectator spectator)
        {
            spectators.Add(spectator);
        }

        public void RemovePlayer(string name)
        {
            var player = players.FirstOrDefault(p => p.Name == name);

            if (player != null)
            {
                players.Remove(player);
            }
        }
    }

    public class Player
    {
        public string Name { get; init; }
    }

    public class Spectator
    {
        public string Name { get; init; }
    }
}
