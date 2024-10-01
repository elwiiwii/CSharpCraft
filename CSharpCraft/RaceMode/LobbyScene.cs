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
using CSharpCraft.Pcraft;
using System.Drawing;

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
            actionsItems.Add(new Item { Name = mainRace.myself.Ready ? "unready" : "ready", Active = mainRace.myself.Role == "Player" });
            actionsItems.Add(new Item { Name = "start game", Active = mainRace.myself.Host });
            actionsItems.Add(new Item { Name = "leave room", Active = true });
            actionsItems.Add(new Item { Name = "change role", Active = true });
            actionsItems.Add(new Item { Name = "change host", Active = mainRace.myself.Host });
            actionsItems.Add(new Item { Name = "seeding", Active = mainRace.myself.Host });
            actionsItems.Add(new Item { Name = "settings", Active = true });
            actionsItems.Add(new Item { Name = "password", Active = true });

            rulesItems.Clear();
            rulesItems.Add(new Item { Name = "best of:5", Active = mainRace.myself.Host });
            rulesItems.Add(new Item { Name = "mode:any%", Active = mainRace.myself.Host });
            rulesItems.Add(new Item { Name = "finishers:1", Active = mainRace.myself.Host });
            rulesItems.Add(new Item { Name = "unbans:on", Active = mainRace.myself.Host });
            rulesItems.Add(new Item { Name = "adv:0-0", Active = mainRace.myself.Host });

            if (p8.Btnp(2)) { actionsMenu.Sel -= 1; }
            if (p8.Btnp(3)) { actionsMenu.Sel += 1; }

            if (p8.Btnp(5) && mainRace.myself.Role == "Player")
            {
                await PlayerReady();
            }
        }

        private void Printc(string t, int x, int y, int c)
        {
            p8.Print(t, x - t.Length * 2, y, c);
        }

        private void DrawMenu(Menu menu, int sel, bool active)
        {
            p8.Rectfill(menu.Xpos + (menu.Width - menu.Name.Length * 4) / 2, menu.Ypos + 1, menu.Xpos - 1 + menu.Width - (menu.Width - menu.Name.Length * 4) / 2, menu.Ypos + 7, 13);
            p8.Print(menu.Name, menu.Xpos + 1 + (menu.Width - menu.Name.Length * 4) / 2, menu.Ypos + 2, 7);

            if (active)
            {
                p8.Rectfill(menu.Xpos, menu.Ypos + 10, menu.Xpos + menu.Width - 1, menu.Ypos + 16, 13);
                p8.Spr(68, menu.Xpos - 3, menu.Ypos + 10);
                p8.Spr(68, menu.Xpos + menu.Width - 5, menu.Ypos + 10, 1, 1, true);
            }

            var offset = 0;
            

            for (int i = sel; i <= sel + 2; i++)
            {
                p8.Print(menu.Items[i].Name, menu.Xpos + 5, menu.Ypos + 11 + i * 7, menu.Items[i].Active ? 7 : 0);
            }
        }

        private void Panel(string name, int x, int y, int width, int height)
        {
            p8.Rectfill(x + 8, y + 8, x + width - 9, y + height - 9, 1);
            p8.Spr(66, x, y);
            p8.Spr(67, x + width - 8, y);
            p8.Spr(82, x, y + height - 8);
            p8.Spr(83, x + width - 8, y + height - 8);
            p8.Sspr(24, 32, 4, 8, x + 8, y, width - 16, 8);
            p8.Sspr(24, 40, 4, 8, x + 8, y + height - 8, width - 16, 8);
            p8.Sspr(16, 36, 8, 4, x, y + 8, 8, height - 16);
            p8.Sspr(24, 36, 8, 4, x + width - 8, y + 8, 8, height - 16);

            var hx = x + (width - name.Length * 4) / 2;
            p8.Rectfill(hx, y + 1, hx + name.Length * 4, y + 7, 13);
            p8.Print(name, hx + 1, y + 2, 7);
        }

        private void List(Menu menu, int x, int y, int width, int height, int displayed)
        {
            Panel(menu.Name, x, y, width, height);

            var tlist = menu.Items.Count;
            if (tlist < 1)
            {
                return;
            }

            var sel = menu.Sel;
            if (menu.Off > Math.Max(0, sel - 2)) { menu.Off = Math.Max(0, sel - 2); }
            if (menu.Off < Math.Min(tlist, sel + 2) - displayed) { menu.Off = Math.Min(tlist, sel + 2) - displayed; }

            sel -= menu.Off;

            var debut = menu.Off + 1;
            var fin = Math.Min(menu.Off + displayed, tlist);

            var offset = 0;
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);

            if (menu.Sel > tlist - 3)
            {
                offset = 4;
                batch.Draw(textureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * cellW, (y + 10) * cellH), null, p8.colors[13], 0, Vector2.Zero, size, SpriteEffects.FlipVertically, 0);
            }
            else if (menu.Sel > 1)
            {
                offset = 2;
                batch.Draw(textureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * cellW, (y + 9) * cellH), null, p8.colors[13], 0, Vector2.Zero, size, SpriteEffects.FlipVertically, 0);
                batch.Draw(textureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * cellW, (y + height - 6) * cellH), null, p8.colors[13], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }
            else
            {
                offset = 0;
                batch.Draw(textureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * cellW, (y + height - 7) * cellH), null, p8.colors[13], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }

            var sely = y + offset + 4 + (sel + 1) * 7;
            //p8.Rectfill(x + 1, sely, x + sx - 3, sely + 6, 13);
            p8.Rectfill(menu.Xpos, sely, menu.Xpos + menu.Width - 1, sely + 6, 13);
            p8.Spr(68, menu.Xpos - 3, sely);
            p8.Spr(68, menu.Xpos + menu.Width - 5, sely, 1, 1, true);

            x += 5;
            y += 12;

            for (int i = debut - 1; i < fin; i++)
            {
                var it = menu.Items[i];
                var py = y + offset + (i - menu.Off) * 7;
                p8.Print(it.Name, x, py, it.Active ? 7 : 0);
            }



            //p8.Spr(68, x - 8, sely);
            //p8.Spr(68, x + sx - 10, sely, 1, 1, true);
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

            //DrawMenu(actionsMenu, 1, true);
            //DrawMenu(rulesMenu, 1, false);
            List(actionsMenu, actionsMenu.Xpos, actionsMenu.Ypos, actionsMenu.Width, actionsMenu.Height, 3);
            List(rulesMenu, rulesMenu.Xpos, rulesMenu.Ypos, rulesMenu.Width, rulesMenu.Height, 3);

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
                    batch.Draw(textureDictionary["Tick"], new Vector2((37 + player.Name.Length * 4) * cellW, (26 + i * 7) * cellH), null, p8.colors[6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
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
