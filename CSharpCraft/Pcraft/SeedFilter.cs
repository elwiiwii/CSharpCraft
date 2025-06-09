using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using CSharpCraft.Pico8;
using FixMath;
using Force.DeepCloner;
using Microsoft.Xna.Framework.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;

namespace CSharpCraft.Pcraft;

public class SeedFilter
{
    private bool found = false;
    private F32[][] level;
    private int[] typeCount;
    private readonly Lock lockObj = new();
    private readonly List<DensityCheck> densityChecks = [];
    private readonly List<DensityComparison> densityComparisons = [];

    private readonly Random random = new();

    private Task ResetLevelTask;

    private CancellationTokenSource cts = new();
    private int levelsx;
    private bool levelUnder;
    private int holex;
    private int levelx;
    private int holey;
    private int levely;
    private int levelsy;
    private F32 plx;
    private F32 ply;

    private class DensityCheck
    {
        public bool IsCave { get; set; } = false;
        public (int Lb, int Ub) Radius { get; set; } = (1, 2);
        public List<int> Tiles { get; set; } = [];
        public (double Lb, double Ub) Density { get; set; } = (0, 100);
        public int Count { get; set; } = 0;

        private int _failCount = 0;
        public int FailCount
        {
            get => _failCount;
            set => _failCount = value;
        }
        public void IncrementFailCount()
        {
            Interlocked.Increment(ref _failCount);
        }

        private int _tryCount = 0;
        public int TryCount
        {
            get => _tryCount;
            set => _tryCount = value;
        }
        public void IncrementTryCount()
        {
            Interlocked.Increment(ref _tryCount);
        }

        public DensityCheck Clone()
        {
            return new DensityCheck
            {
                IsCave = this.IsCave,
                Radius = this.Radius,
                Tiles = new(this.Tiles),
                Density = this.Density,
                Count = 0,
                FailCount = 0,
                TryCount = 0
            };
        }
    }

    private class DensityComparison
    {
        public bool IsCave { get; set; } = false;
        public (int Lb, int Ub) Radius1 { get; set; } = (1, 2);
        public List<int> Tiles1 { get; set; } = [];
        public int Count1 { get; set; } = 0;
        public (int Lb, int Ub) Radius2 { get; set; } = (1, 2);
        public List<int> Tiles2 { get; set; } = [];
        public int Count2 { get; set; } = 0;
        public int Mag { get; set; } = 100;
        public string Opr { get; set; } = "=";

        private int _failCount = 0;
        public int FailCount
        {
            get => _failCount;
            set => _failCount = value;
        }
        public void IncrementFailCount()
        {
            Interlocked.Increment(ref _failCount);
        }

        private int _tryCount = 0;
        public int TryCount
        {
            get => _tryCount;
            set => _tryCount = value;
        }
        public void IncrementTryCount()
        {
            Interlocked.Increment(ref _tryCount);
        }

        public DensityComparison Clone()
        {
            return new DensityComparison
            {
                IsCave = this.IsCave,
                Radius1 = this.Radius1,
                Tiles1 = new(this.Tiles1),
                Count1 = 0,
                Radius2 = this.Radius2,
                Tiles2 = new(this.Tiles2),
                Count2 = 0,
                Mag = this.Mag,
                Opr = this.Opr,
                FailCount = 0,
                TryCount = 0
            };
        }
    }

    private readonly Dictionary<string, int> TileNum = new()
    {
        { "water", 0 },
        { "sand", 1 },
        { "grass", 2 },
        { "stone", 3 },
        { "tree", 4 },
        { "iron", 8 },
        { "gold", 9 },
        { "gem", 10 },
    };

    private static bool Filter(List<DensityCheck> densityChecks, List<DensityComparison> densityComparisons)
    {
        foreach (var check in densityChecks)
        {
            check.IncrementTryCount();
            double area = ((check.Radius.Ub + check.Radius.Ub * check.Radius.Ub) - (check.Radius.Lb + check.Radius.Lb * check.Radius.Lb)) * 4;
            if (check.Count / area < check.Density.Lb / 100.0 || check.Count / area > check.Density.Ub / 100.0)
            {
                check.IncrementFailCount();
                return true;
            }
        }

        foreach (var check in densityComparisons)
        {
            check.IncrementTryCount();
            double area1 = ((check.Radius1.Ub + check.Radius1.Ub * check.Radius1.Ub) - (check.Radius1.Lb + check.Radius1.Lb * check.Radius1.Lb)) * 4;
            double area2 = ((check.Radius2.Ub + check.Radius2.Ub * check.Radius2.Ub) - (check.Radius2.Lb + check.Radius2.Lb * check.Radius2.Lb)) * 4;
            double density1 = check.Count1 / area1;
            double density2 = check.Count2 / area2;
            switch (check.Opr)
            {
                case ">":
                    if (density1 - density2 < check.Mag / 100.0)
                    {
                        check.IncrementFailCount();
                        return true;
                    }
                    return false;
                case "=":
                    if (Math.Abs(density1 - density2) > check.Mag / 2.0 / 100.0)
                    {
                        check.IncrementFailCount();
                        return true;
                    }
                    return false;
                case "<":
                    if (density1 - density2 > -check.Mag / 100.0)
                    {
                        check.IncrementFailCount();
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        return false;
    }

    private static void ComparisonCount(List<DensityComparison> densityComparisons, int i, int j, int curtile)
    {
        foreach (var check in densityComparisons)
        {
            foreach (var tile in check.Tiles1)
            {
                if (tile == curtile && Math.Abs(i) >= check.Radius1.Lb && Math.Abs(i) <= check.Radius1.Ub && Math.Abs(j) >= check.Radius1.Lb && Math.Abs(j) <= check.Radius1.Ub)
                {
                    check.Count1++;
                }
            }
        }
        foreach (var check in densityComparisons)
        {
            foreach (var tile in check.Tiles2)
            {
                if (tile == curtile && Math.Abs(i) >= check.Radius2.Lb && Math.Abs(i) <= check.Radius2.Ub && Math.Abs(j) >= check.Radius2.Lb && Math.Abs(j) <= check.Radius2.Ub)
                {
                    check.Count2++;
                }
            }
        }
    }

    private static void DensityCount(List<DensityCheck> densityChecks, int i, int j, int curtile)
    {
        foreach (var check in densityChecks)
        {
            foreach (var tile in check.Tiles)
            {
                if (tile == curtile && Math.Abs(i) >= check.Radius.Lb && Math.Abs(i) <= check.Radius.Ub && Math.Abs(j) >= check.Radius.Lb && Math.Abs(j) <= check.Radius.Ub)
                {
                    check.Count++;
                }
            }
        }
    }

    private static int MaxRadius(List<DensityCheck> densityChecks, List<DensityComparison> densityComparisons)
    {
        int mval = 0;
        foreach (var check in densityChecks)
        {
            if (check.Radius.Ub > mval)
            {
                mval = check.Radius.Ub;
            }
        }
        foreach (var check in densityComparisons)
        {
            if (check.Radius1.Ub > mval)
            {
                mval = check.Radius1.Ub;
            }
            if (check.Radius2.Ub > mval)
            {
                mval = check.Radius2.Ub;
            }
        }
        return mval;
    }

    private int MinRadius(List<DensityCheck> densityChecks, List<DensityComparison> densityComparisons)
    {
        int mval = levelsx / 2;
        foreach (var check in densityChecks)
        {
            if (check.Radius.Lb < mval)
            {
                mval = check.Radius.Lb;
            }
        }
        foreach (var check in densityComparisons)
        {
            if (check.Radius1.Lb < mval)
            {
                mval = check.Radius1.Lb;
            }
            if (check.Radius2.Lb < mval)
            {
                mval = check.Radius2.Lb;
            }
        }
        return mval;
    }

    private async Task CreateMapStepCheck(List<DensityCheck> densityChecks, List<DensityComparison> densityComparisons, int sx, int sy, int a, int b, int c, int d, int e, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>();

        await Task.Run(async () =>
        {
            try
            {
                Parallel.For(0, int.MaxValue, (index, state) =>
                {
                    if (ct.IsCancellationRequested || found)
                    {
                        state.Stop();
                        return;
                    }

                    bool needmap = true;
                    //List<DensityCheck> densityChecksClone = DeepClonerExtensions.DeepClone(densityChecks);
                    //List<DensityComparison> densityComparisonsClone = DeepClonerExtensions.DeepClone(densityComparisons);

                    List<DensityCheck> densityChecksClone = densityChecks.Select(c => c.Clone()).ToList();
                    List<DensityComparison> densityComparisonsClone = densityComparisons.Select(c => c.Clone()).ToList();

                    F32[][] cur = Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.2), sx);
                    F32[][] cur2 = Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.4), 8);
                    F32[][] cur3 = Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.3), 8);
                    F32[][] cur4 = Noise(sx, sy, F32.FromDouble(0.8), F32.FromDouble(1.1), 4);

                    int maxRadius = Math.Min(levelsx / 2 - 1, Math.Max(1, MaxRadius(densityChecksClone, densityComparisonsClone)));
                    int minRadius = Math.Min(levelsx / 2 - 1, Math.Max(1, MinRadius(densityChecksClone, densityComparisonsClone)));
                    int center = levelsx / 2;
                    int[] temp_typeCount = new int[11];

                    for (int i = 0; i < 11; i++)
                    {
                        temp_typeCount[i] = 0;
                    }

                    for (int i = 0; i <= sx; i++)
                    {
                        for (int j = 0; j <= sy; j++)
                        {
                            F32 v = F32.Abs(cur[i][j] - cur2[i][j]);
                            F32 v2 = F32.Abs(cur[i][j] - cur3[i][j]);
                            F32 v3 = F32.Abs(cur[i][j] - cur4[i][j]);
                            F32 dist = F32.Max(F32.Abs(F32.FromDouble((double)i / sx - 0.5)) * 2, F32.Abs(F32.FromDouble((double)j / sy - 0.5)) * 2);
                            dist = dist * dist * dist * dist;
                            F32 coast = v * 4 - dist * 4;

                            int id = a;
                            if (coast > F32.FromDouble(0.3)) { id = b; } // sand
                            if (coast > F32.FromDouble(0.6)) { id = c; } // grass
                            if (coast > F32.FromDouble(0.3) && v2 > F32.Half) { id = d; } // stone
                            if (id == c && v3 > F32.Half) { id = e; } // tree

                            temp_typeCount[id]++;

                            cur[i][j] = F32.FromInt(id);

                            if ((densityChecksClone.Count == 0 && densityComparisonsClone.Count == 0) ||
                                i < center - maxRadius ||
                                i > center + maxRadius ||
                                j < center - maxRadius ||
                                j > center + maxRadius) { continue; }
                            //if ((i >= center - minRadius && i <= center + minRadius) || (j >= center - minRadius && j <= center + minRadius)) { continue; }
                            int curtile = F32.FloorToInt(cur[i][j]);
                            DensityCount(densityChecksClone, i - center, j - center, curtile);
                            ComparisonCount(densityComparisonsClone, i - center, j - center, curtile);
                        }
                    }

                    needmap = Filter(densityChecksClone, densityComparisonsClone);

                    for (int i = 0; i < densityChecks.Count; i++)
                    {
                        if (densityChecksClone[i].TryCount == 1)
                        {
                            densityChecks[i].IncrementTryCount();
                        }
                        if (densityChecksClone[i].FailCount == 1)
                        {
                            densityChecks[i].IncrementFailCount();
                            break;
                        }
                    }
                    for (int i = 0; i < densityComparisons.Count; i++)
                    {
                        if (densityComparisonsClone[i].TryCount == 1)
                        {
                            densityComparisons[i].IncrementTryCount();
                        }
                        if (densityComparisonsClone[i].FailCount == 1)
                        {
                            densityComparisons[i].IncrementFailCount();
                            break;
                        }
                    }

                    if (!needmap)
                    {
                        lock (lockObj)
                        {
                            if (!found)
                            {
                                found = true;
                                level = cur;
                                typeCount = temp_typeCount;
                                state.Stop();
                                tcs.TrySetResult(true);
                            }
                        }
                    }
                });
            }
            finally
            {
                if (!tcs.Task.IsCompleted) { tcs.TrySetResult(false); }
            }
        }, ct);

        await tcs.Task;
        found = false;
    }

    private async Task<Level> CreateLevelAsync(int xx, int yy, int sizex, int sizey, bool IsUnderground, CancellationToken ct)
    {
        Level l = new() { X = xx, Y = yy, Sx = sizex, Sy = sizey, IsUnder = IsUnderground, Ent = [], Ene = [], Dat = new F32[8192] };
        SetLevel(l);
        levelUnder = IsUnderground;
        await CreateMapAsync(ct);
        FillEne(l);
        l.Stx = F32.FromInt((holex - levelx) * 16 + 8);
        l.Sty = F32.FromInt((holey - levely) * 16 + 8);
        return l;
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
                await CreateMapStepCheck(caveDensityChecks, caveDensityComparisons, levelsx, levelsy, 3, 8, 1, 9, 10, ct);

                if (typeCount[8] < 30) { needmap = true; }
                if (typeCount[9] < 20) { needmap = true; }
                if (typeCount[10] < 15) { needmap = true; }
                //if (needmap) caveFailCount++;
            }
            else
            {
                List<DensityCheck> surfaceDensityChecks = densityChecks.Where(check => !check.IsCave).ToList();
                List<DensityComparison> surfaceDensityComparisons = densityComparisons.Where(check => !check.IsCave).ToList();
                await CreateMapStepCheck(surfaceDensityChecks, surfaceDensityComparisons, levelsx, levelsy, 0, 1, 2, 3, 4, ct);

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
                    int indx = random.Next(spawnableTiles.Count);
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

    private async Task ResetLevelAsync(CancellationToken ct) // mayber remove from this file
    {
        p8.Reload();
        p8.Memcpy(0x1000, 0x2000, 0x1000);

        prot = F32.Zero;
        lrot = F32.Zero;

        panim = F32.Zero;

        pstam = F32.FromInt(100);
        lstam = pstam;
        plife = F32.FromInt(100);
        llife = plife;

        banim = F32.Zero;

        coffx = F32.Zero;
        coffy = F32.Zero;

        time = F32.Zero;

        toogleMenu = 0;
        invent = [];
        curItem = null;
        switchLevel = false;
        canSwitchLevel = false;
        menuInvent = Cmenu(inventary, invent);

        for (int i = 0; i <= 15; i++)
        {
            Rndwat[i] = new F32[16];
            for (int j = 0; j <= 15; j++)
            {
                Rndwat[i][j] = p8.Rnd(100);
            }
        }

        cave = await CreateLevelAsync(64, 0, 32, 32, true, ct);
        island = await CreateLevelAsync(0, 0, 64, 64, false, ct);

        Entity tmpworkbench = Entity(workbench, plx, ply, F32.Zero, F32.Zero);
        tmpworkbench.HasCol = true;
        tmpworkbench.List = workbenchRecipe;

        p8.Add(invent, tmpworkbench);
        p8.Add(invent, Inst(pickuptool));
    }
}
