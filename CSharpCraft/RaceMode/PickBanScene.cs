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
    public class PickBanScene(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, List<IGameMode> raceScenes, MainRace mainRace, TitleScreen titleScreen) : IGameMode
    {
#nullable enable
        private int gameCount;
        private string player1Name;
        private string player2Name;
        private int player1Score;
        private int player2Score;
        private int selectedType;
#nullable disable

        public string GameModeName { get => "2"; }

        public void Init()
        {
            mainRace.currentScene = 2;
            gameCount = 1;
            player1Name = "holoknight";
            player2Name = "holoknight";
            player1Score = 0;
            player2Score = 0;
            selectedType = 1;
        }

        public async void Update()
        {
            if (p8.Btnp(0) && selectedType > 4) { selectedType -= 4; }
            if (p8.Btnp(1) && selectedType < 5) { selectedType += 4; }
            if (p8.Btnp(2)) { selectedType -= 1; }
            if (p8.Btnp(3)) { selectedType += 1; }
            if (selectedType < 1) { selectedType = 1; }
            if (selectedType > 7) { selectedType = 7; }
        }

        private void Printc(string t, int x, int y, int c)
        {
            p8.Print(t, x - t.Length * 2, y, c);
        }

        private void PrintR(string t, int x, int y, int c)
        {
            p8.Print(t, x + 2 - t.Length * 4, y, c);
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
            Vector2 halfSize = new(cellW / 2f, cellH / 2f);
            Vector2 quarterSize = new(cellW / 4f, cellH / 4f);

            batch.Draw(textureDictionary["SmallNameBanner"], new Vector2(0 * cellW, 4 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SmallNameBanner"], new Vector2(73 * cellW, 4 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
            p8.Circfill(47 - player1Score.ToString().Length, 9, 3, 1);
            p8.Circfill(47 + player1Score.ToString().Length, 9, 3, 1);
            p8.Rectfill(47 - 1 - player1Score.ToString().Length, 6, 47 + 1 + player1Score.ToString().Length, 12, 1);
            Printc($"{player1Score}", 48, 7, 7);
            PrintR(player1Name, 40, 7, 7);

            p8.Circfill(80 - player2Score.ToString().Length, 9, 3, 1);
            p8.Circfill(80 + player2Score.ToString().Length, 9, 3, 1);
            p8.Rectfill(80 - 1 - player2Score.ToString().Length, 6, 80 + 1 + player2Score.ToString().Length, 12, 1);
            Printc($"{player2Score}", 81, 7, 7);
            p8.Print(player2Name, 87, 7, 7);

            batch.Draw(textureDictionary["Game"], new Vector2(56 * cellW, 6 * cellH), null, p8.colors[7], 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);
            batch.Draw(textureDictionary[$"{gameCount}"], new Vector2(62 * cellW, 12 * cellH), null, p8.colors[7], 0, Vector2.Zero, halfSize, SpriteEffects.None, 0);

            batch.Draw(textureDictionary["SeedSelector"], new Vector2(17 * cellW, 38 * cellH), null, p8.colors[1], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedSelector"], new Vector2(41 * cellW, 38 * cellH), null, p8.colors[2], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedSelector"], new Vector2(65 * cellW, 38 * cellH), null, p8.colors[2], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedSelector"], new Vector2(89 * cellW, 38 * cellH), null, p8.colors[1], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedSelector"], new Vector2(29 * cellW, 62 * cellH), null, p8.colors[2], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedSelector"], new Vector2(53 * cellW, 62 * cellH), null, p8.colors[2], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedSelector"], new Vector2(77 * cellW, 62 * cellH), null, p8.colors[2], 0, Vector2.Zero, size, SpriteEffects.None, 0);

            batch.Draw(textureDictionary["SeedType1"], new Vector2((17 + 2) * cellW, (38 + 2) * cellH), null, Color.White, 0, Vector2.Zero, quarterSize, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedType2"], new Vector2((41 + 2) * cellW, (38 + 2) * cellH), null, Color.White, 0, Vector2.Zero, quarterSize, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedType3"], new Vector2((65 + 2) * cellW, (38 + 2) * cellH), null, Color.White, 0, Vector2.Zero, quarterSize, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedType4"], new Vector2((89 + 2) * cellW, (38 + 2) * cellH), null, Color.White, 0, Vector2.Zero, quarterSize, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedType51"], new Vector2((29 + 2) * cellW, (62 + 2) * cellH), null, Color.White, 0, Vector2.Zero, quarterSize, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedType6"], new Vector2((53 + 2) * cellW, (62 + 2) * cellH), null, Color.White, 0, Vector2.Zero, quarterSize, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedType7"], new Vector2((77 + 2) * cellW, (62 + 2) * cellH), null, Color.White, 0, Vector2.Zero, quarterSize, SpriteEffects.None, 0);

            batch.Draw(textureDictionary["SeedGreyOut"], new Vector2(17 * cellW, 38 * cellH), null, p8.colors[2], 0, Vector2.Zero, size, SpriteEffects.None, 0);

            batch.Draw(textureDictionary["SeedSelectorArrow"], new Vector2(-7 * cellW, 103 * cellH), null, p8.colors[14], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedSelectorArrow"], new Vector2(27 * cellW, 103 * cellH), null, p8.colors[2], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedSelectorArrow"], new Vector2(61 * cellW, 103 * cellH), null, p8.colors[2], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["SeedSelectorArrow"], new Vector2(95 * cellW, 103 * cellH), null, p8.colors[2], 0, Vector2.Zero, size, SpriteEffects.None, 0);

            p8.Print("ban", 4, 97, 6);
            p8.Print("1", 19, 97, 6);
            p8.Print("ban", 38, 97, 6);
            p8.Print("2", 53, 97, 6);
            p8.Print("ban", 72, 97, 6);
            p8.Print("3", 87, 97, 6);
            p8.Print("pick", 107, 97, 6);

        }


    }

}
