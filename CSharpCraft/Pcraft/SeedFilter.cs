using System.Reflection;
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

public class DensityCheck
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

public class DensityComparison
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

public static class SeedFilter
{
    private static bool found = false;
    private static readonly Lock lockObj = new();
    private static readonly List<DensityCheck> densityChecks = [];
    private static readonly List<DensityComparison> densityComparisons = [];

    private static readonly Random random = new();

    public static CancellationTokenSource Cts = new();

    public static readonly Dictionary<string, int> TileNum = new()
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

    private static int MinRadius(List<DensityCheck> densityChecks, List<DensityComparison> densityComparisons, int lvlSize)
    {
        int mval = lvlSize / 2;
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

    public static async Task<(F32[][], int[])> CreateMapStepCheck(List<DensityCheck> densityChecks, List<DensityComparison> densityComparisons, int sx, int sy, int a, int b, int c, int d, int e, Func<int, int, F32, F32, int, F32[][]> noise, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<(F32[][], int[])>();
        F32[][] resultMap = null;
        int[] resultTypeCount = null;

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

                    F32[][] cur = noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.2), sx);
                    F32[][] cur2 = noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.4), 8);
                    F32[][] cur3 = noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.3), 8);
                    F32[][] cur4 = noise(sx, sy, F32.FromDouble(0.8), F32.FromDouble(1.1), 4);

                    int maxRadius = Math.Min(sx / 2 - 1, Math.Max(1, MaxRadius(densityChecksClone, densityComparisonsClone)));
                    int minRadius = Math.Min(sx / 2 - 1, Math.Max(1, MinRadius(densityChecksClone, densityComparisonsClone, sx)));
                    int center = sx / 2;
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
                                resultMap = cur;
                                resultTypeCount = temp_typeCount;
                                state.Stop();
                                tcs.TrySetResult((resultMap, resultTypeCount));
                            }
                        }
                    }
                });
            }
            finally
            {
                if (!tcs.Task.IsCompleted)
                {
                    tcs.TrySetResult((null, null));
                }
            }
        }, ct);

        var result = await tcs.Task;
        found = false;
        return result;
    }
}
