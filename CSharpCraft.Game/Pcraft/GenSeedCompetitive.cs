using System;
using CSharpCraft.Competitive;
using CSharpCraft.Pico8;
using FixMath;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace CSharpCraft.Pcraft;

public class GenSeedCompetitive : SpeedrunBase
{
    public override string SceneName => "seed_gen";

    private List<DensityCheck> densityChecks = [];
    private List<DensityComparison> densityComparisons = [];

    private Task ResetLevelTask;
    private CancellationTokenSource cts = new();
    private int worldSeed;

    private int surfaceIndex;
    private int caveIndex;

    public override void Init(Pico8Functions pico8)
    {
        base.Init(pico8);
        if (RoomHandler._myself.Generator)
        {
            worldSeed = RoomHandler._curMatch.GameReports[^1].WorldSeed;
            Console.WriteLine(worldSeed);
            densityChecks.AddRange(RankedFilters.RankedSurfaceChecks[RoomHandler._curMatch.GameReports[^1].SurfaceType - 1]);
            densityChecks.AddRange(RankedFilters.RankedCaveChecks[RoomHandler._curMatch.GameReports[^1].CaveType - 1]);
            densityComparisons.AddRange(RankedFilters.RankedSurfaceComps[RoomHandler._curMatch.GameReports[^1].SurfaceType - 1]);
            densityComparisons.AddRange(RankedFilters.RankedCaveComps[RoomHandler._curMatch.GameReports[^1].CaveType - 1]);
            cts = new();
            ResetLevelTask = Task.Run(() => ResetLevelAsync(cts.Token));
        }
    }

    private async Task ResetLevelAsync(CancellationToken ct)
    {
        await CreateLevelAsync(64, 0, 32, 32, true, ct);
        await CreateLevelAsync(0, 0, 64, 64, false, ct);
        Console.WriteLine("Seed generation complete.");
        RoomHandler.SendSeed(worldSeed, surfaceIndex, caveIndex);
    }

    private async Task CreateLevelAsync(int xx, int yy, int sizex, int sizey, bool isUnderground, CancellationToken ct)
    {
        Level l = new() { X = xx, Y = yy, Sx = sizex, Sy = sizey, IsUnder = isUnderground, Ent = [], Ene = [], Dat = new F32[8192] };
        SetLevel(l);
        levelUnder = isUnderground;
        await CreateMapAsync(ct);
    }

    private async Task CreateMapAsync(CancellationToken ct)
    {
        bool needmap = true;

        while (needmap)
        {
            needmap = false;

            if (levelUnder)
            {
                List<DensityCheck> caveDensityChecks = densityChecks.Where(check => check.IsCave).ToList();
                List<DensityComparison> caveDensityComparisons = densityComparisons.Where(check => check.IsCave).ToList();
                (level, typeCount, caveIndex) = await SeedFilter.CreateMapStepCheck(caveDensityChecks, caveDensityComparisons, levelsx, levelsy, 3, 8, 1, 9, 10, Noise, worldSeed, ct, false);

                if (typeCount[8] < 30) { needmap = true; }
                //if (typeCount[9] < 20) { needmap = true; }
                if (typeCount[10] < 15) { needmap = true; }
                //if (needmap) caveFailCount++;
            }
            else
            {
                List<DensityCheck> surfaceDensityChecks = densityChecks.Where(check => !check.IsCave).ToList();
                List<DensityComparison> surfaceDensityComparisons = densityComparisons.Where(check => !check.IsCave).ToList();
                (level, typeCount, surfaceIndex) = await SeedFilter.CreateMapStepCheck(surfaceDensityChecks, surfaceDensityComparisons, levelsx, levelsy, 0, 1, 2, 3, 4, Noise, worldSeed, ct, false);

                if (typeCount[3] < 30) { needmap = true; }
                if (typeCount[4] < 30) { needmap = true; }
                //if (needmap) surfaceFailCount++;
            }

            if (!needmap)
            {
                plx = F32.Neg1;
                ply = F32.Neg1;

                List<(int x, int y)> spawnableTiles = [];

                for (int i = -4; i <= 4; i++)
                {
                    if (i == 0) { continue; }
                    for (int j = -4; j <= 4; j++)
                    {
                        if (j == 0) { continue; }
                        int depx = levelsx / 2 + i;
                        int depy = levelsy / 2 + j;
                        F32 c = level[depx][depy];

                        if (c == 1 || c == 2)
                        {
                            spawnableTiles.Add((depx, depy));
                        }
                    }
                }

                if (spawnableTiles.Count > 0)
                {
                    int indx = F32.FloorToInt(p8.Rnd(spawnableTiles.Count, pSpawnRng));
                    (int x, int y) tile = spawnableTiles[indx];

                    plx = F32.FromInt(tile.x * 16 + 8);
                    ply = F32.FromInt(tile.y * 16 + 8);
                }

                if (plx < 0)
                {
                    needmap = true;
                    //surfaceFailCount++;
                }
            }
        }

        for (int i = 0; i < levelsx; i++)
        {
            for (int j = 0; j < levelsy; j++)
            {
                p8.Mset(i + levelx, j + levely, level[i][j].Double);
            }
        }

        holex = levelsx / 2 + levelx;
        holey = levelsy / 2 + levely;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                p8.Mset(holex + i, holey + j, levelUnder ? 1 : 3);
            }
        }

        p8.Mset(holex, holey, 11);

        clx = plx;
        cly = ply;

        cmx = plx;
        cmy = ply;
    }

    public override void Update()
    {
        
    }

    public override void Draw()
    {
        p8.Cls(17);
        Shared.Printc(p8, $"generating = {RoomHandler._myself.Generator}", 64, 61, 15);
    }

    public override void Dispose()
    {
        base.Dispose();
        cts?.Cancel();
        cts?.Dispose();
    }
}
