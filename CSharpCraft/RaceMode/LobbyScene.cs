using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using Grpc.Core;
using Grpc.Net.Client;
using RaceServer;
using System.Collections.Concurrent;
using CSharpCraft.Pico8;
using System.Xml.Linq;
using System.Data;
using System;

namespace CSharpCraft.RaceMode
{
    public class LobbyScene(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, List<IGameMode> raceScenes, MainRace mainRace, TitleScreen titleScreen) : IGameMode
    {
#nullable enable

#nullable disable

        public string GameModeName { get => "1"; }

        public void Init()
        {

        }

        public async void Update()
        {
            
        }

        private void Printc(string t, int x, int y, int c)
        {
            p8.Print(t, x - t.Length * 2, y, c);
        }

        private void DrawMenu(string title, string[] list, int xpos, int ypos, int width, int height)
        {
            p8.Rectfill(xpos + (width - title.Length * 4) / 2, ypos + 1, xpos - 1 + width - (width - title.Length * 4) / 2, ypos + 7, 13);
            p8.Print(title, xpos + 1 + (width - title.Length * 4) / 2, ypos + 2, 7);
        }

        public void Draw()
        {
            p8.Cls();

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);
            Vector2 halfSize = new(cellW / 2, cellH / 2);

            batch.Draw(textureDictionary["LobbyBackground"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            var roomName = "room name-????";
            p8.Rectfill(64 - roomName.Length * 2, 6, 64 + roomName.Length * 2, 6 + 6, 13);
            Printc(roomName, 65, 7, 7);
            Printc("password-????", 65, 17, 7);

            string[] placeholder = [];
            DrawMenu("actions", placeholder, 5, 82, 53, 41);
            //var actions = "actions";
            //p8.Rectfill(17, 83, 17 + actions.Length * 4, 83 + 6, 13);
            //p8.Print(actions, 18, 84, 7);

            DrawMenu("rules", placeholder, 62, 82, 61, 41);
            //var rules = "rules";
            //p8.Rectfill(82, 83, 82 + rules.Length * 4, 83 + 6, 13);
            //p8.Print(rules, 83, 84, 7);

            int i = 0;
            foreach (var player in mainRace.playerDictionary.Values)
            {
                batch.Draw(textureDictionary[$"{player.Role}Icon"], new Vector2(25 * cellW, (26 + i * 7) * cellH), null, Color.White, 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
                p8.Print(player.Name, 36, 26 + i * 7, 7);
                if (player.Host)
                {
                    p8.Print("[", 81, 26 + i * 7, 5);
                    p8.Print("host", 84, 26 + i * 7, 5);
                    p8.Print("]", 99, 26 + i * 7, 5);
                }
                //p8.Print("0", 84, 85, 13);
                //p8.Print("0", 89, 118, 13);
                i++;
            }
        }

    }

}
