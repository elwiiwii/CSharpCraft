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

        private string greeting;

        public void Init()
        {
            // The port number must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5072");
            var client = new RoomHandler.RoomHandlerClient(channel);
            var reply = client.JoinRoom(
                              new JoinRequest { Name = "Client" });
            greeting = reply.Message;
            //Console.WriteLine("Greeting: " + reply.Message);
            //Console.WriteLine("Press any key to exit...");
            //Console.ReadKey();
        }

        public void Update()
        {
            
        }

        public void Draw()
        {
            p8.Cls();
            p8.Print(greeting, 1, 1, 8);
        }

    }

}
