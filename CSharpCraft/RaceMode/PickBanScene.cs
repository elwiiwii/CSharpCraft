﻿using Microsoft.Xna.Framework;
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
using CSharpCraft.OptionsMenu;

namespace CSharpCraft.RaceMode
{
    public class PickBanScene(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, List<IGameMode> raceScenes, MainRace mainRace, TitleScreen titleScreen, KeyboardOptionsFile keyboardOptionsFile) : IGameMode
    {
#nullable enable
        private float animationTimer;
        private int gameCount;
        private int turn;
        private string player1Name;
        private string player2Name;
        private int player1Score;
        private int player2Score;
        private int selectedType;

        private SeedType seedType1;
        private SeedType seedType2;
        private SeedType seedType3;
        private SeedType seedType4;
        private SeedType seedType5;
        private SeedType seedType6;
        private SeedType seedType7;
        private List<SeedType> seedTypes = new();
        private List<SeedType> turns = new();
        private List<SeedType> gameSeeds = new();
#nullable disable

        public string GameModeName { get => "2"; }

        public void Init()
        {
            mainRace.currentScene = 2;
            animationTimer = 0;
            gameCount = 1;
            turn = 0;
            player1Name = "holoknight";
            player2Name = "holoknight";
            player1Score = 0;
            player2Score = 0;
            selectedType = 1;

            seedTypes.Clear();
            seedTypes.Add(new SeedType { Name = "SeedType0", Description = "", Status = "", Xpos = 0, Ypos = 0 });
            seedTypes.Add(new SeedType { Name = "SeedType1", Description = "ow stone, open cave", Status = "UNBANNED", Xpos = 17, Ypos = 38 });
            seedTypes.Add(new SeedType { Name = "SeedType2", Description = "ow stone, closed cave", Status = "UNBANNED", Xpos = 41, Ypos = 38 });
            seedTypes.Add(new SeedType { Name = "SeedType3", Description = "no ow stone, open cave", Status = "UNBANNED", Xpos = 65, Ypos = 38 });
            seedTypes.Add(new SeedType { Name = "SeedType4", Description = "no ow stone, closed cave", Status = "UNBANNED", Xpos = 89, Ypos = 38 });
            seedTypes.Add(new SeedType { Name = "SeedType5", Description = "weak tool type", Status = "UNBANNED", Xpos = 29, Ypos = 62 });
            seedTypes.Add(new SeedType { Name = "SeedType6", Description = "balanced seed", Status = "UNBANNED", Xpos = 53, Ypos = 62 });
            seedTypes.Add(new SeedType { Name = "SeedType7", Description = "random seed", Status = "UNBANNED", Xpos = 77, Ypos = 62 });

            turns.Clear();
            turns.Add(new SeedType { Name = "SeedType0", Description = "ban 1", Status = "BANNED", Xpos = 2, Ypos = 103, });
            turns.Add(new SeedType { Name = "SeedType0", Description = "ban 2", Status = "BANNED", Xpos = 36, Ypos = 103, });
            turns.Add(new SeedType { Name = "SeedType0", Description = "ban 3", Status = "BANNED", Xpos = 70, Ypos = 103, });
            turns.Add(new SeedType { Name = "SeedType0", Description = "pick", Status = "PLAYED", Xpos = 104, Ypos = 103, });

            gameSeeds.Clear();
            gameSeeds.Add(new SeedType { Name = "SeedType0", Description = "game 1", Status = "PLAYED", Xpos = 3, Ypos = 103, });
            gameSeeds.Add(new SeedType { Name = "SeedType0", Description = "game 2", Status = "PLAYED", Xpos = 28, Ypos = 103, });
            gameSeeds.Add(new SeedType { Name = "SeedType0", Description = "game 3", Status = "PLAYED", Xpos = 53, Ypos = 103, });
            gameSeeds.Add(new SeedType { Name = "SeedType0", Description = "game 4", Status = "PLAYED", Xpos = 78, Ypos = 103, });
            gameSeeds.Add(new SeedType { Name = "SeedType0", Description = "game 5", Status = "PLAYED", Xpos = 103, Ypos = 103, });
        }

        public async void Update()
        {
            seedTypes[selectedType].Selected = false;
            if (p8.Btnp(0)) { selectedType -= 1; }
            if (p8.Btnp(1)) { selectedType += 1; }
            if (p8.Btnp(2) && selectedType > 4) { selectedType -= 4; }
            if (p8.Btnp(3) && selectedType < 5) { selectedType += 4; }
            if (selectedType < 1) { selectedType = 1; }
            else if (selectedType > 7) { selectedType = 7; }
            seedTypes[selectedType].Selected = true;

            var turnType = turns[turn];
            var selectedSeedType = seedTypes[selectedType];
            var gameType = gameSeeds[gameCount - 1];

            var unbannedCount = 0;
            foreach (var seedType in seedTypes)
            {
                if (seedType.Status == "UNBANNED") { unbannedCount++; }
            }
            if (unbannedCount == 0)
            {
                foreach (var seedType in seedTypes)
                {
                    if (seedType.Name != "SeedType0")
                    {
                        seedType.Status = "UNBANNED";
                    }
                }
            }

            if (gameCount > 1)
            {
                seedTypes[5].Unavailable = false;
                seedTypes[6].Unavailable = false;
                seedTypes[7].Unavailable = false;
                if (p8.Btnp(5) && !selectedSeedType.Unavailable)
                {
                    if (selectedSeedType.Status == "UNBANNED")
                    {
                        gameType.Name = $"SeedType{selectedType}";
                        selectedSeedType.Status = gameType.Status;
                        if (gameCount < 5)
                        {
                            gameCount++; //testing
                        }
                    }
                    else if (selectedSeedType.Status == "BANNED")
                    {
                        selectedSeedType.Status = "UNBANNED";
                    }
                }
            }
            else if (gameCount == 1)
            {
                if (turn == 3)
                {
                    seedTypes[5].Unavailable = true;
                    seedTypes[6].Unavailable = true;
                    seedTypes[7].Unavailable = true;
                    if (p8.Btnp(5) && !selectedSeedType.Unavailable && selectedSeedType.Status == "UNBANNED")
                    {
                        turnType.Name = $"SeedType{selectedType}";
                        selectedSeedType.Status = turnType.Status;
                        gameType.Name = $"SeedType{selectedType}";
                        selectedSeedType.Status = gameType.Status;
                        gameCount++; //testing
                    }
                }
                else
                {
                    if (p8.Btnp(5) && selectedSeedType.Status == "UNBANNED")
                    {
                        turnType.Name = $"SeedType{selectedType}";
                        seedTypes[selectedType].Status = turnType.Status;
                        turn++;
                    }
                }
            }

            animationTimer += 0.025f;
        }

        private void DrawSeedType(SeedType seedType, bool animated)
        {
            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);
            Vector2 quarterSize = new(cellW / 4f, cellH / 4f);

            var color = 2;
            var animationCheck = animated && seedType.Status == "UNBANNED" && !seedType.Unavailable;

            batch.Draw(textureDictionary[animationCheck ? seedType.Name + Math.Floor(animationTimer % 4) : seedType.Name], new Vector2((seedType.Xpos + 2) * cellW, (seedType.Ypos + 2) * cellH), null, Color.White, 0, Vector2.Zero, quarterSize, SpriteEffects.None, 0);
            
            if (seedType.Status == "BANNED")
            {
                color = 1;
                batch.Draw(textureDictionary["SeedSelector"], new Vector2(seedType.Xpos * cellW, seedType.Ypos * cellH), null, seedType.Selected ? p8.colors[14] : p8.colors[color], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["SeedCross"], new Vector2((seedType.Xpos + 2) * cellW, (seedType.Ypos + 2) * cellH), null, seedType.Selected ? p8.colors[14] : p8.colors[1], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }
            else
            {
                batch.Draw(textureDictionary["SeedSelector"], new Vector2(seedType.Xpos * cellW, seedType.Ypos * cellH), null, seedType.Selected ? p8.colors[14] : p8.colors[color], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }
            if (seedType.Status == "PLAYED" || seedType.Unavailable)
            {
                color = 1;
                batch.Draw(textureDictionary["SeedSelector"], new Vector2(seedType.Xpos * cellW, seedType.Ypos * cellH), null, seedType.Selected ? p8.colors[14] : p8.colors[color], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["SeedGreyOut"], new Vector2(seedType.Xpos * cellW, seedType.Ypos * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }
        }

        private void DrawInitialSeedSelection(SeedType seedType, int order)
        {
            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);
            Vector2 quarterSize = new(cellW / 4f, cellH / 4f);

            batch.Draw(textureDictionary[seedType.Name], new Vector2((seedType.Xpos + 2) * cellW, (seedType.Ypos + 2) * cellH), null, Color.White, 0, Vector2.Zero, quarterSize, SpriteEffects.None, 0);

            var color = 2;
            if (order > turn)
            {
                color = 2;
            }
            else if (order < turn)
            {
                color = 1;
            }
            else
            {
                color = 14;
            }

            batch.Draw(textureDictionary["SeedSelectorArrow"], new Vector2((seedType.Xpos - 9) * cellW, seedType.Ypos * cellH), null, p8.colors[color], 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }

        private void DrawSeedSelection(SeedType seedType, int order)
        {
            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);
            Vector2 quarterSize = new(cellW / 4f, cellH / 4f);

            batch.Draw(textureDictionary[seedType.Name], new Vector2((seedType.Xpos + 2) * cellW, (seedType.Ypos + 2) * cellH), null, Color.White, 0, Vector2.Zero, quarterSize, SpriteEffects.None, 0);

            var color = 2;
            if (order > gameCount)
            {
                color = 2;
            }
            else if (order < gameCount)
            {
                color = 1;
                Printc("lost", seedType.Xpos + 12, seedType.Ypos - 6, 6);
            }
            else
            {
                color = 14;
            }

            batch.Draw(textureDictionary["SeedSelector"], new Vector2(seedType.Xpos * cellW, seedType.Ypos * cellH), null, p8.colors[color], 0, Vector2.Zero, size, SpriteEffects.None, 0);
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

            var s1 = $"{player1Name}'s turn";
            var s2 = $"[{KeyNames.keyNames[keyboardOptionsFile.Menu.Bind1]}] for random action";
            //Printc(Math.Floor(animationTimer) % 2 == 0 ? s1 : s2, 64, 29, 7);
            Printc(s1, 64, 29, 7);

            DrawSeedType(seedTypes[1], false);
            DrawSeedType(seedTypes[2], false);
            DrawSeedType(seedTypes[3], false);
            DrawSeedType(seedTypes[4], false);
            DrawSeedType(seedTypes[5], true);
            DrawSeedType(seedTypes[6], false);
            DrawSeedType(seedTypes[7], false);

            if (gameCount > 1)
            {
                DrawSeedSelection(gameSeeds[0], 1);
                DrawSeedSelection(gameSeeds[1], 2);
                DrawSeedSelection(gameSeeds[2], 3);
                DrawSeedSelection(gameSeeds[3], 4);
                DrawSeedSelection(gameSeeds[4], 5);
            }
            else if (gameCount == 1)
            {
                p8.Print("ban", 4, 97, 6);
                p8.Print("1", 19, 97, 6);
                p8.Print("ban", 38, 97, 6);
                p8.Print("2", 53, 97, 6);
                p8.Print("ban", 72, 97, 6);
                p8.Print("3", 87, 97, 6);
                p8.Print("pick", 107, 97, 6);

                DrawInitialSeedSelection(turns[0], 0);
                DrawInitialSeedSelection(turns[1], 1);
                DrawInitialSeedSelection(turns[2], 2);
                DrawInitialSeedSelection(turns[3], 3);
            }
            

        }


    }

}
