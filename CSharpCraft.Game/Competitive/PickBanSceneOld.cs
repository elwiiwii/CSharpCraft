using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;
using CSharpCraft.Pico8;
using static CSharpCraft.Pico8.Pico8;
using CSharpCraft.OptionsMenu;
using FixMath;

namespace CSharpCraft.Competitive;

public class PickBanSceneOld() : IScene, IDisposable
{
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

    public string SceneName { get => "2"; }
    public double Fps { get => 60.0; }
    public (int w, int h) Resolution { get => (128, 128); }

    public void Init()
    {

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
        if (Btnp(0)) { selectedType -= 1; }
        if (Btnp(1)) { selectedType += 1; }
        if (Btnp(2) && selectedType > 4) { selectedType -= 4; }
        if (Btnp(3) && selectedType < 5) { selectedType += 4; }
        if (selectedType < 1) { selectedType = 1; }
        else if (selectedType > 7) { selectedType = 7; }
        seedTypes[selectedType].Selected = true;

        SeedType turnType = turns[turn];
        SeedType selectedSeedType = seedTypes[selectedType];
        SeedType gameType = gameSeeds[gameCount - 1];

        int unbannedCount = 0;
        foreach (SeedType seedType in seedTypes)
        {
            if (seedType.Status == "UNBANNED") { unbannedCount++; }
        }
        if (unbannedCount == 0)
        {
            foreach (SeedType seedType in seedTypes)
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
            if (Btnp(5) && !selectedSeedType.Unavailable)
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
                if (Btnp(5) && !selectedSeedType.Unavailable && selectedSeedType.Status == "UNBANNED")
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
                if (Btnp(5) && selectedSeedType.Status == "UNBANNED")
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
        int color = 2;
        bool animationCheck = animated && seedType.Status == "UNBANNED" && !seedType.Unavailable;

        GameRendering.Current.Draw(animationCheck ? seedType.Name + Math.Floor(animationTimer % 4) : seedType.Name, new Vector2((seedType.Xpos + 2) * CellWidth, (seedType.Ypos + 2) * CellHeight), Color.White, CellWidth / 4f, CellHeight / 4f);
        
        if (seedType.Status == "BANNED")
        {
            color = 1;
            GameRendering.Current.Draw("SeedSelector", new Vector2(seedType.Xpos * CellWidth, seedType.Ypos * CellHeight), seedType.Selected ? Colors[14] : Colors[color], CellWidth, CellHeight);
            GameRendering.Current.Draw("SeedCross", new Vector2((seedType.Xpos + 2) * CellWidth, (seedType.Ypos + 2) * CellHeight), seedType.Selected ? Colors[14] : Colors[1], CellWidth, CellHeight);
        }
        else
        {
            GameRendering.Current.Draw("SeedSelector", new Vector2(seedType.Xpos * CellWidth, seedType.Ypos * CellHeight), seedType.Selected ? Colors[14] : Colors[color], CellWidth, CellHeight);
        }
        if (seedType.Status == "PLAYED" || seedType.Unavailable)
        {
            color = 1;
            GameRendering.Current.Draw("SeedSelector", new Vector2(seedType.Xpos * CellWidth, seedType.Ypos * CellHeight), seedType.Selected ? Colors[14] : Colors[color], CellWidth, CellHeight);
            GameRendering.Current.Draw("SeedGreyOut", new Vector2(seedType.Xpos * CellWidth, seedType.Ypos * CellHeight), Color.White, CellWidth, CellHeight);
        }
    }

    private void DrawInitialSeedSelection(SeedType seedType, int order)
    {
        GameRendering.Current.Draw(seedType.Name, new Vector2((seedType.Xpos + 2) * CellWidth, (seedType.Ypos + 2) * CellHeight), Color.White, CellWidth / 4f, CellHeight / 4f);

        int color = 2;
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

        GameRendering.Current.Draw("SeedSelectorArrow", new Vector2((seedType.Xpos - 9) * CellWidth, seedType.Ypos * CellHeight), Colors[color], CellWidth, CellHeight);
    }

    private void DrawSeedSelection(SeedType seedType, int order)
    {
        GameRendering.Current.Draw(seedType.Name, new Vector2((seedType.Xpos + 2) * CellWidth, (seedType.Ypos + 2) * CellHeight), Color.White, CellWidth / 4f, CellHeight / 4f);

        int color = 2;
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

        GameRendering.Current.Draw("SeedSelector", new Vector2(seedType.Xpos * CellWidth, seedType.Ypos * CellHeight), Colors[color], CellWidth, CellHeight);
    }

    private void Printc(string t, int x, int y, int c)
    {
        Print(t, x - t.Length * 2, y, c);
    }

    private void PrintR(string t, int x, int y, int c)
    {
        Print(t, x + 2 - t.Length * 4, y, c);
    }

    public void Draw()
    {
        Cls();

        GameRendering.Current.Draw("SmallNameBanner", new Vector2(0 * CellWidth, 4 * CellHeight), Color.White, CellWidth, CellHeight);
        GameRendering.Current.Draw("SmallNameBanner", new Vector2(73 * CellWidth, 4 * CellHeight), Color.White, CellWidth, CellHeight, flipX: true);
        Circfill(F32.FromInt(47) - player1Score.ToString().Length, F32.FromInt(9), 3, 1);
        Circfill(F32.FromInt(47) + player1Score.ToString().Length, F32.FromInt(9), 3, 1);
        Rectfill(47 - 1 - player1Score.ToString().Length, 6, 47 + 1 + player1Score.ToString().Length, 12, 1);
        Printc($"{player1Score}", 48, 7, 7);
        PrintR(player1Name, 40, 7, 7);

        Circfill(F32.FromInt(80) - player2Score.ToString().Length, F32.FromInt(9), 3, 1);
        Circfill(F32.FromInt(80) + player2Score.ToString().Length, F32.FromInt(9), 3, 1);
        Rectfill(80 - 1 - player2Score.ToString().Length, 6, 80 + 1 + player2Score.ToString().Length, 12, 1);
        Printc($"{player2Score}", 81, 7, 7);
        Print(player2Name, 87, 7, 7);

        GameRendering.Current.Draw("Game", new Vector2(56 * CellWidth, 6 * CellHeight), Colors[7], CellWidth / 2f, CellHeight / 2f);
        GameRendering.Current.Draw($"{gameCount}", new Vector2(62 * CellWidth, 12 * CellHeight), Colors[7], CellWidth / 2f, CellHeight / 2f);

        string s1 = $"{player1Name}'s turn";
        string s2 = $"[{KeyNames.keyNames[OptionsFile.Current.Kbm_Menu.Bind1]}] for random action";
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
            Print("ban", 4, 97, 6);
            Print("1", 19, 97, 6);
            Print("ban", 38, 97, 6);
            Print("2", 53, 97, 6);
            Print("ban", 72, 97, 6);
            Print("3", 87, 97, 6);
            Print("pick", 107, 97, 6);

            DrawInitialSeedSelection(turns[0], 0);
            DrawInitialSeedSelection(turns[1], 1);
            DrawInitialSeedSelection(turns[2], 2);
            DrawInitialSeedSelection(turns[3], 3);
        }
    }
    public string SpriteImage => "";
    public string SpriteData => @"";
    public string FlagData => @"";
    public (int x, int y) MapDimensions => (0, 0);
    public string MapData => @"";
    public Dictionary<string, List<SongInst>> Music => new();
    public Dictionary<string, Dictionary<int, string>> Sfx => new();
    public void Dispose()
    {

    }

}
