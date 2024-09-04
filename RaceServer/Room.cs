namespace RaceServer
{
    public class Room
    {
        public Room()
        {
            PlayerList = new List<Player>();
        }

        public List<Player> PlayerList { get; set; }
    }
}
