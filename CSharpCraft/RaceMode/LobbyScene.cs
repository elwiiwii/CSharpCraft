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
    public class LobbyScene(MainRace mainRace) : IScene, IDisposable
    {
#nullable enable
        private Menu roomMenu = new();
        private Menu actionsMenu = new();
        private List<Item> actionsItems = new();
        private Menu rulesMenu = new();
        private List<Item> rulesItems = new();
        private string roomName;
        private string roomPassword;
#nullable disable

        public string SceneName { get => "1"; }
        private Pico8Functions p8;

        public void Init(Pico8Functions pico8)
        {
            p8 = pico8;

            roomName = "????";
            roomPassword = "????";
            roomMenu = new Menu { Name = $"room name-{roomName}", Items = null, Xpos = 20, Ypos = 5, Width = 88, Height = 74 };
            actionsMenu = new Menu { Name = "actions", Items = actionsItems, Xpos = 5, Ypos = 82, Width = 53, Height = 41, Active = true };
            rulesMenu = new Menu { Name = "rules", Items = rulesItems, Xpos = 62, Ypos = 82, Width = 61, Height = 41, Active = false };
        }

        public async void Update()
        {
            bool allReady = true;
            foreach (KeyValuePair<int, RoomUser> player in mainRace.playerDictionary)
            {
                if (!player.Value.Ready) { allReady = false; }
            }

            actionsItems.Clear();
            actionsItems.Add(new Item { Name = mainRace.myself.Ready ? "unready" : "ready", Active = mainRace.myself.Role == "Player", Method = PlayerReady });
            actionsItems.Add(new Item { Name = "start match", Active = mainRace.myself.Host && allReady, Method = StartMatch });
            actionsItems.Add(new Item { Name = "leave room", Active = true, Method = LeaveRoom });
            actionsItems.Add(new Item { Name = "change role", Active = true, Method = ChangeRole });
            actionsItems.Add(new Item { Name = "change host", Active = mainRace.myself.Host, Method = ChangeHost });
            actionsItems.Add(new Item { Name = "seeding", Active = mainRace.myself.Host, Method = Seeding });
            actionsItems.Add(new Item { Name = "settings", Active = true, Method = Settings });
            actionsItems.Add(new Item { Name = "password", Active = true, Method = Password });

            rulesItems.Clear();
            rulesItems.Add(new Item { Name = "best of:5", Active = mainRace.myself.Host });
            rulesItems.Add(new Item { Name = "mode:any%", Active = mainRace.myself.Host });
            rulesItems.Add(new Item { Name = "finishers:1", Active = mainRace.myself.Host });
            rulesItems.Add(new Item { Name = "unbans:on", Active = mainRace.myself.Host });
            rulesItems.Add(new Item { Name = "adv:0-0", Active = mainRace.myself.Host });

            if (p8.Btnp(0)) { actionsMenu.Active = true; rulesMenu.Active = false; }
            if (p8.Btnp(1)) { rulesMenu.Active = true; actionsMenu.Active = false; }

            if (actionsMenu.Active)
            {
                if (p8.Btnp(2)) { actionsMenu.Sel -= 1; }
                if (p8.Btnp(3)) { actionsMenu.Sel += 1; }
                actionsMenu.Sel = actionsMenu.Sel < 0 ? 0 : actionsMenu.Sel >= actionsMenu.Items.Count ? actionsMenu.Items.Count - 1 : actionsMenu.Sel;

                if (p8.Btnp(5) && actionsMenu.Items[actionsMenu.Sel].Active) { await actionsMenu.Items[actionsMenu.Sel].Method(); }
            }
            else if (rulesMenu.Active)
            {
                if (p8.Btnp(2)) { rulesMenu.Sel -= 1; }
                if (p8.Btnp(3)) { rulesMenu.Sel += 1; }
                rulesMenu.Sel = rulesMenu.Sel < 0 ? 0 : rulesMenu.Sel >= rulesMenu.Items.Count ? rulesMenu.Items.Count - 1 : rulesMenu.Sel;

                if (p8.Btnp(5) && rulesMenu.Items[rulesMenu.Sel].Active) { await rulesMenu.Items[rulesMenu.Sel].Method(); }
            }


            if (p8.Btnp(4)) { p8.LoadCart(new PickBanScene(mainRace)); }

            //if (p8.Btnp(5) && mainRace.myself.Role == "Player")
            //{
            //    await PlayerReady();
            //}
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

            int hx = x + (width - name.Length * 4) / 2;
            p8.Rectfill(hx, y + 1, hx + name.Length * 4, y + 7, 13);
            p8.Print(name, hx + 1, y + 2, 7);
        }

        private void Selector(int x, int y, int width)
        {
            p8.Rectfill(x, y, x + width - 1, y + 6, 13);
            p8.Spr(68, x - 3, y);
            p8.Spr(68, x + width - 5, y, 1, 1, true);
        }

        private void List(Menu menu, int x, int y, int width, int height, int displayed)
        {
            Panel(menu.Name, x, y, width, height);

            int tlist = menu.Items.Count;
            if (tlist < 1)
            {
                return;
            }

            int sel = menu.Sel;
            if (menu.Off > Math.Max(0, sel - 2)) { menu.Off = Math.Max(0, sel - 2); }
            if (menu.Off < Math.Min(tlist, sel + 2) - displayed) { menu.Off = Math.Min(tlist, sel + 2) - displayed; }

            sel -= menu.Off;

            int debut = menu.Off + 1;
            int fin = Math.Min(menu.Off + displayed, tlist);

            int offset = 0;
            int viewportWidth = p8.graphicsDevice.Viewport.Width;
            int viewportHeight = p8.graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);

            if (menu.Sel > tlist - 3)
            {
                offset = 4;
                p8.batch.Draw(p8.textureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * cellW, (y + 10) * cellH), null, p8.colors[13], 0, Vector2.Zero, size, SpriteEffects.FlipVertically, 0);
            }
            else if (menu.Sel > 1)
            {
                offset = 2;
                p8.batch.Draw(p8.textureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * cellW, (y + 9) * cellH), null, p8.colors[13], 0, Vector2.Zero, size, SpriteEffects.FlipVertically, 0);
                p8.batch.Draw(p8.textureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * cellW, (y + height - 6) * cellH), null, p8.colors[13], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }
            else
            {
                offset = 0;
                p8.batch.Draw(p8.textureDictionary["Arrow5"], new Vector2((x + (width / 2) - 2) * cellW, (y + height - 7) * cellH), null, p8.colors[13], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }

            int sely = y + offset + 4 + (sel + 1) * 7;
            if (menu.Active) { Selector(x, sely, width); }

            //p8.Rectfill(x + 1, sely, x + sx - 3, sely + 6, 13);
            //p8.Rectfill(menu.Xpos, sely, menu.Xpos + menu.Width - 1, sely + 6, 13);
            //p8.Spr(68, menu.Xpos - 3, sely);
            //p8.Spr(68, menu.Xpos + menu.Width - 5, sely, 1, 1, true);

            x += 5;
            y += 12;

            for (int i = debut - 1; i < fin; i++)
            {
                Item it = menu.Items[i];
                int py = y + offset + (i - menu.Off) * 7;
                p8.Print(it.Name, x, py, it.Active ? 7 : 0);
            }



            //p8.Spr(68, x - 8, sely);
            //p8.Spr(68, x + sx - 10, sely, 1, 1, true);
        }

        public void Draw()
        {
            p8.Cls();

            // Get the size of the viewport
            int viewportWidth = p8.graphicsDevice.Viewport.Width;
            int viewportHeight = p8.graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);
            Vector2 halfSize = new(cellW / 2f, cellH / 2f);

            //batch.Draw(textureDictionary["LobbyBackground"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            //string roomName = "room name-????";
            //p8.Rectfill(64 - roomName.Length * 2, 6, 64 + roomName.Length * 2, 6 + 6, 13);
            //Printc(roomName, 65, 7, 7);
            //Printc("password-????", 65, 17, 7);

            //DrawMenu(actionsMenu, 1, true);
            //DrawMenu(rulesMenu, 1, false);

            Panel($"room name-{roomName}", roomMenu.Xpos, roomMenu.Ypos, roomMenu.Width, roomMenu.Height);
            Selector(roomMenu.Xpos, roomMenu.Ypos + 11, roomMenu.Width);
            Printc($"password-{roomPassword}", 65, roomMenu.Ypos + 12, 7);
            List(actionsMenu, actionsMenu.Xpos, actionsMenu.Ypos, actionsMenu.Width, actionsMenu.Height, 3);
            List(rulesMenu, rulesMenu.Xpos, rulesMenu.Ypos, rulesMenu.Width, rulesMenu.Height, 3);

            //string[] actionsList = ["ready", "start game", "leave room"];
            //DrawMenu(actionsMenu);
            //string actions = "actions";
            //p8.Rectfill(17, 83, 17 + actions.Length * 4, 83 + 6, 13);
            //p8.Print(actions, 18, 84, 7);

            //string[] rulesList = ["best of:5", "mode:any%", "finishers:1"];
            //DrawMenu("rules", rulesList, 62, 82, 61, 41);
            //string rules = "rules";
            //p8.Rectfill(82, 83, 82 + rules.Length * 4, 83 + 6, 13);
            //p8.Print(rules, 83, 84, 7);

            int i = 0;
            foreach (RoomUser player in mainRace.playerDictionary.Values)
            {
                p8.batch.Draw(p8.textureDictionary[$"{player.Role}Icon"], new Vector2(25 * cellW, (26 + i * 7) * cellH), null, Color.White, 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
                p8.Print(player.Name, 36, 26 + i * 7, 7);
                if (player.Role == "Player" && player.Ready)
                {
                    p8.batch.Draw(p8.textureDictionary["Tick"], new Vector2((37 + player.Name.Length * 4) * cellW, (26 + i * 7) * cellH), null, p8.colors[6], 0, Vector2.Zero, size, SpriteEffects.None, 0);
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

        private async Task Password()
        {
            throw new NotImplementedException();
        }

        private async Task Settings()
        {
            throw new NotImplementedException();
        }

        private async Task Seeding()
        {
            throw new NotImplementedException();
        }

        private async Task ChangeHost()
        {
            throw new NotImplementedException();
        }

        private async Task ChangeRole()
        {
            throw new NotImplementedException();
        }

        private async Task LeaveRoom()
        {
            throw new NotImplementedException();
        }

        private async Task StartMatch()
        {
            mainRace.service.StartMatch(new StartMatchRequest { Name = mainRace.myself.Name });
        }

        private async Task PlayerReady()
        {
            mainRace.myself.Ready = mainRace.service.PlayerReady(new PlayerReadyRequest { Name = mainRace.myself.Name }).Ready;
        }

        public string SpriteData => @"
00000000ffffffffffffffffffffffffffffffff44fff44ffff44fff020121000004200002031000fff55fffffff555ff5555fff000000000001000000101000
00000000ffffffffffffffffffffffffffff444fff4ffff4ff4fffff310310200303102041420000ff56655ffff56665f56665ff000100000011100001000100
00000000fff4fffff4ffffffff4fffff4444fffffff444ff44fff44f205200024002001030310410f566665ffff566655666665f001110000110110010000100
00000000ffffffffffffff4ffffffffffffffff4ff4fff44fff44ff415340401340100402020030256666665f551566515666665000100000011100000000000
00000000ffffffffffffffffffffff4fff44444fffffffffffffffff424243032300403410140201566666655665155115666665000000000001000001100100
00000000ffffffffff4fffffffffffffffffffffff44fff444fff44f313132021240302300034104156655515666511ff1566565000000000000000000010000
00000000ffff4ffffffffffffffffffff4444fff44ff444fff444fff002021404130201204023003f155511f56651ffff1565151000000000000000000000000
00000000fffffffffffffffffffffffffffff444ffffffffffffffff001010000020100003012040ff111fff1551ffffff151f1f000000000000000000000000
fffff11ffffffffff11fffff3353333333333333ff1111ffff1111ffff1111ff6666666666666666fffff44444ffff444444ffffddddddddddddddddffffffff
fff115511fffff1115511fff3515333333333353f155551111555511115555df6666666666666666ff44444444444444444444ffddddddddddddddddfffff111
ff15533551fff155533551ff5153333333333515155555555555555555555dd166666dddddd66666f4444444444444444444444fddddddddddddddddfff11666
f15333333511153333333b1f515333333353515315556555555665555556ddd1666dddddddddd66644441111144444411444444fddddddddddddddddff166666
f15333335155515333333b1f351533333515515315556666666666666666ddd1666dddddddddd6664411ddddd111111dd144444fddddddddddddddddf116dddd
f15333351533351533333b1f33533533351535151555566666666666666dddd1666dddddddddd66641dddddddddddddddd144444dddddd1111ddddddf16ddddd
1533333353333353333333b13333515bb1533515f155566666666666666ddd1f6666ddd11ddd666641ddddddddddddddddd11444ddddd144441dddddf1dddddd
1533333333333333335333b1333335b115333353f155566666666666666ddd1f6666dd1001dd666641ddddddddddddddddddd114ddddd14ff41dddddf1dd5555
f15335333333333335153b1f333333b115533333f155566666666666666ddd1f666655100155666641dddddddddddddddddddd14ddddd144441dddddf1d55555
f1535153333333333351b1ff33333515511535331555566666666d66666dddd1666655111155666641dddddddddddddddddddd14dddd14444441ddddf1551111
ff15153333333333333b1fff35335153355331531555666665d666666666ddd16666555555556666441dddddddddddddddddd14fdddd14444441ddddf1511111
fff1533333333533333b1fff515535333333551515556666666666666666ddd16666655555566666f41dddddddddddddddddd14fddd1444114441dddf1111000
fff1533333335153333b1fff351153333333351515556666666666666666ddd16666666666666666f41dddddddddddddddddd14fdddd111dd111ddddf1110000
fff15333333335333351b1ff33551533333351531555566666665666666dddd16666666666666666441dddddddddddddddddd14fddddddddddddddddff110000
ff1515333333333335153b1f3335153333333533f15556666666d666666ddd1f666666666666666641dddddddddddddddddddd14ddddddddddddddddfff11111
f15351533333333333533b1f3333533333333333f155566666666666666ddd1f666666666666666641dddddddddddddddddddd14ddddddddddddddddffffffff
f15335333333333333333b1f3333333333533333f155566666666666666ddd1f66666666666d666641ddddddddddddddddddd114dddddddddddddddd00000000
1533333335333335333333b13333333335153333f1556666666666666666dd1f6666666665566666441dddddddddddddddd11444dddddddddddddddd00000000
1533333351533351533333b1335333533353333315556dddddd66dddddd6ddd166655666d6666666f441dddddddddddddd14444fdddddddddddddddd00000000
f1533333351bbb1533333b1f35153515333335531555ddddddddddddddddddd166d66d6666666666ff441dddddddddddd144444fdddddddddddddddd00000000
f153333333b111b333333b1f5153335335335115155dddddddddddddddddddd166666666666666d6fff4411ddd1111ddd1444fffdddddddddddddddd00000000
ff1bbb33bb1fff1b33bbb1ff353353335153355315ddddddddddddddddddddd16666666666666556fff44441114444111444ffffdddddddddddddddd00000000
fff111bb11fffff1bb111fff3335153335153333fddddd1111dddd1111dddd1f55666666666d5666ffff444444ffff444444ffffdddddddddddddddd00000000
ffffff11ffffffff11ffffff3333533333533333ff1111ffff1111ffff1111ff66d6666d66666666ffffffffffffffffffffffffdddddddddddddddd00000000
00000000222020000011111111111100001dd000000282000000770001400000000004100012022001000010000000000011a861000000000000000010101010
0000000024224200011dddddddddd11001d1110000282820000777700124006006004210012e12ee141111410000000011e1bec1009009000000000151515151
000000022422420011d1111111111d111d11111002828282007777770012441441442100122e11e1124444210000000016e1bec100400400000000115a585651
00020024244342001d111111111111d11d1111102828282807777775140122522522104112ee112241222214001100001111325100444400000001d15b5e5c51
00022024334344201d111111111111d11d1111108282828277777750124111611611142112eee1212444444202ff1000999999990040040000011dcd5b5e5c51
00024244434434201d111111111111d101d1110008282820577775000124441441444210222eee114222222422ff1000541111450024420000161ded5b5e5c51
00224334443434201d111111111111d1001dd00002828200057750000012225225222100222222102411114222220000541111450020020001676ded53525151
02344433344444201d111111111111d100000000002020000055000014111151151111411221110002444420222000005411114500222200156f6d8d55555551
02334444434444201d111111111111d11010101006111600417710211241116116111421222222220d1dd1d006666660444444440020020015666ddd52222251
24434334443344421d111111111111d1010101010061600017777142012444144144421023333332d515515d15666dd549999994002222001555555525555521
23444434444444201d111111111111d1101010100623260077771442001222522522210023333332511111150155ddd549999994001111001999999999999991
02344333444443201d111111111111d101010101623432607774142100141151151141002222222211a9e9110015dd5044444444001001001944444444444491
00234444334432001d111111111111d1101010106333336017114421001241611614210055555555119e8a110001d50055599555001111001999999999999991
012333344333221011d1111111111d11010101016233326001444210000124144142100051111115111111110001500054455445001001001544501010154451
0112222222221110011dddddddddd110101010100623260024422100000012222221000051111115156556510054210054444445001001001544510101054451
00011111111110000011111111111100010101010066600012211000000001111110000051111115011111100542121055555555000000000155101010105510
0020000000000000000000000000001100000000000011110111100000001f100222222222222220000001111100000000000000000000000000000000000000
024202000000200000002200000001410000111000001441144441000001fff10233333333333320001111565111100000066666666600000011111111111100
02442420000220000002341000001410000144410001444101111410001ffff4023333333333332001ddd1d1d1ddd10006666666677666660144444444444410
0244344422242020000244410001410000023441000234110002314101ffff41023333333333332001ddd15651ddd100166666666666666d0144999999994410
2434434442444242002324410014100000232141002321000023214119fff410023333333333332001ddd1d1d1ddd1001666666666666dd10149999999999410
24434344344444420232441002410000023201410232000002320141019f4100023333333333332001ddd15651ddd100156666666666dd100149999999999410
24434444434443202320110023200000232000102320000023200141001910000233333333333320011111d1d111110015566666666dd1000149999999999410
234444434433432032000000320000003200000032000000020000100001000002333333333333201dddd15651dddd100155666666dd10000149999999999410
023444434343322000555000000005000000400005555d5000022200000012200222222222222220155551555155551001556666ddd100000144999999994410
02324443432220000511150000505b50001242205000d6d50123432000012342055555555555555015111111111115101155556dddd400000144444444444410
00202344320000005111115005b5b735012242e2500d676d1234343200123432055555555555555015191a181a1915101445555ddd2410000155559999555510
0000023444200000511111155b73535012282efe5000d6d50123434201234321051000000000015015121812181215101412555dd21441000154445445444510
0000002433200000511111150535b5001288efe250000d051234332112343210051101010101015015115111115115100101255d210144100154445555444510
000012333211000005111115005b735001288e825000000512332232123321000510101010101150155161555161551000001242100014100154444444444510
00012222222210000055115000053500001288205000000501221121012210000511010101010150115515555515511000000141000001000155555555555510
00001111111100000000550000005000000122000555555000110010001100000510101010101150011111111111110000000111000000000011111111111100
".Replace("\n", "").Replace("\r", "");

        public string FlagData => @"";
        public string MapData => @"";
        public Dictionary<string, List<(List<(string name, bool loop)> tracks, int group)>> Music => new();
        public void Dispose()
        {

        }

    }
}
