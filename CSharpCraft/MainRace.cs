using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;
using System.Threading.Tasks;
using Grpc.Net.Client;
using RaceServer;

namespace CSharpCraft
{
    public class MainRace(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice) : IGameMode
    {

        public string GameModeName { get => "race"; }

        private List<PlayerInfo> playerList;
        private string ping;
        //private Player playerInfo = new();

        public void Init()
        {
            var playerInfo = new PlayerInfo { Name = "client", Type = "player" };

            // The port number must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5072");
            var client = new RoomHandler.RoomHandlerClient(channel);
            var reply = client.JoinRoom(
                              new JoinRequest { PlayerInfo = playerInfo });
            playerList = reply.PlayerList.ToList();
            //Console.WriteLine("Greeting: " + reply.Message);
            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();
        }

        public void Update()
        {
            if (p8.Btnp(5))
            {
                using var channel = GrpcChannel.ForAddress("http://localhost:5072");
                var client = new StreamTest.StreamTestClient(channel);
                var reply = client.SendMessage(
                                  new ClientToServerMessage { Text = "buh" });
                ping = reply.Text;
            }
        }

        public void Draw()
        {
            p8.Cls();
            //p8.Print(playerList, 1, 1, 8);
            //int i = 0;
            //foreach (var player in playerList)
            //{
            //    p8.Print(player.Name, 1, 1 + (i*6), 8);
            //    i++;
            //}
            p8.Print(ping, 1, 1, 8);
        }

    }

}
