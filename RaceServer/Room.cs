using System.Collections.Generic;
using System.Linq;

namespace RaceServer
{
    public class Room(string name)
    {
        public string Name { get; } = name;
        private readonly List<User> users = new();

        public IReadOnlyList<User> Users => users;

        public int bestOf { get; set; } = 5;
        public string category { get; set; } = "any%";
        public int finishers { get; set; } = 1;
        public bool unbans { get; set; } = true;
        public int seedType { get; set; } = 0;
        public (int, int) advantage { get; set; } = (0, 0);

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
    }

    public class User
    {
        public string Name { get; set; }
        public string Role { get; set; } = "Player";
        public bool Host { get; set; }
        public bool Ready { get; set; } = true;
    }
}
