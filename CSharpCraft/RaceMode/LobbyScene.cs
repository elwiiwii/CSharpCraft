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
        private Menu actionsMenu = new();
        private List<Item> actionsItems = new();
        private Menu rulesMenu = new();
        private List<Item> rulesItems = new();
#nullable disable

        public string GameModeName { get => "1"; }

        public void Init()
        {
            mainRace.currentScene = 1;
            actionsMenu = new Menu { Name = "actions", Items = actionsItems, Xpos = 5, Ypos = 82, Width = 53, Height = 41 };
            rulesMenu = new Menu { Name = "rules", Items = rulesItems, Xpos = 62, Ypos = 82, Width = 61, Height = 41 };
        }

        public async void Update()
        {
            actionsItems.Clear();
            actionsItems.Add(new Item { Name = "ready", Active = !mainRace.myself.Ready });
            actionsItems.Add(new Item { Name = "start game", Active = mainRace.myself.Host });
            actionsItems.Add(new Item { Name = "leave room", Active = true });

            rulesItems.Clear();
            rulesItems.Add(new Item { Name = "best of:5", Active = mainRace.myself.Host });
            rulesItems.Add(new Item { Name = "mode:any%", Active = mainRace.myself.Host });
            rulesItems.Add(new Item { Name = "finishers:1", Active = mainRace.myself.Host });

            if (p8.Btnp(5))
            {
                await PlayerReady();
            }
        }

        private void Printc(string t, int x, int y, int c)
        {
            p8.Print(t, x - t.Length * 2, y, c);
        }

        private void DrawMenu(Menu menu)
        {
            p8.Rectfill(menu.Xpos + (menu.Width - menu.Name.Length * 4) / 2, menu.Ypos + 1, menu.Xpos - 1 + menu.Width - (menu.Width - menu.Name.Length * 4) / 2, menu.Ypos + 7, 13);
            p8.Print(menu.Name, menu.Xpos + 1 + (menu.Width - menu.Name.Length * 4) / 2, menu.Ypos + 2, 7);

            int i = 0;
            foreach (var item in menu.Items)
            {
                p8.Print(item.Name, menu.Xpos + 5, menu.Ypos + 11 + i * 7, item.Active ? 7 : 0);
                i++;
            }
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

            DrawMenu(actionsMenu);
            DrawMenu(rulesMenu);

            //string[] actionsList = ["ready", "start game", "leave room"];
            //DrawMenu(actionsMenu);
            //var actions = "actions";
            //p8.Rectfill(17, 83, 17 + actions.Length * 4, 83 + 6, 13);
            //p8.Print(actions, 18, 84, 7);

            //string[] rulesList = ["best of:5", "mode:any%", "finishers:1"];
            //DrawMenu("rules", rulesList, 62, 82, 61, 41);
            //var rules = "rules";
            //p8.Rectfill(82, 83, 82 + rules.Length * 4, 83 + 6, 13);
            //p8.Print(rules, 83, 84, 7);

            int i = 0;
            foreach (var player in mainRace.playerDictionary.Values)
            {
                batch.Draw(textureDictionary[$"{player.Role}Icon"], new Vector2(25 * cellW, (26 + i * 7) * cellH), null, Color.White, 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
                p8.Print(player.Name, 36, 26 + i * 7, 7);
                if (player.Role == "Player" && player.Ready)
                {
                    batch.Draw(textureDictionary["Tick"], new Vector2((36 + player.Name.Length * 4) * cellW, (26 + i * 7) * cellH), null, p8.colors[6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                }
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

        private async Task PlayerReady()
        {
            mainRace.myself.Ready = mainRace.service.PlayerReady(new PlayerReadyRequest { Name = mainRace.myself.Name }).Ready;
        }

    }

}
