using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Drawing;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using RaceServer;
using System.Threading.Channels;
using System.Collections.Concurrent;
using System.Security.Principal;
using CSharpCraft.Pico8;

namespace CSharpCraft.RaceMode
{
    public class MainRace(List<IGameMode> raceScenes) : IGameMode
    {
        public string GameModeName { get => "race"; }

        public CancellationTokenSource CancellationTokenSource = new();
        public AsyncServerStreamingCall<RoomStreamResponse> RoomStream;
        public ConcurrentDictionary<int, RoomUser> playerDictionary = new();
        public RoomUser myself = new();

        public int currentScene;

        public void Init()
        {
            currentScene = 0;
            raceScenes[currentScene].Init();
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.LeftControl) && state.IsKeyDown(Keys.Q))
            {
                CancellationTokenSource.Cancel(); // Cancel the listening task
                RoomJoiningStream?.Dispose(); // Dispose of the stream
            }
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                CancellationTokenSource.Cancel(); // Cancel the listening task
                RoomJoiningStream?.Dispose(); // Dispose of the stream
            };

            currentScene = int.Parse(raceScenes[currentScene].GameModeName);
            raceScenes[currentScene].Update();
        }

        public void Draw()
        {
            raceScenes[currentScene].Draw();
        }

    }

}
