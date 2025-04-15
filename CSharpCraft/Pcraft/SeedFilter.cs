using System;
using CSharpCraft.Pico8;
using FixMath;
using Force.DeepCloner;
using Microsoft.Xna.Framework.Input;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CSharpCraft.Pcraft
{
    public class SeedFilter : PcraftBase, IDisposable
    {
        public override string SceneName => "seed filter";

        private SavedMap curMap = new();
        private SavedMap prevMap = new();

        private bool found = false;
        private object lockObj = new();
        private List<Button> buttonRow1 = [];
        private List<Button> buttonRow2 = [];
        private List<Button> buttonRow3 = [];
        private List<Button> buttonRow4 = [];
        private List<Button> buttonRow5 = [];
        List<DensityCheck> densityChecks = [];
        List<DensityComparison> densityComparisons = [];
        List<List<Button>> buttonRows = [];

        Random random = new();

        private int menuY = 0;
        private int menuX = 0;
        private int tileIndex = 0;

        private Task ResetLevelTask;

        MouseState prevState = new();
        int camoffy;

        CancellationTokenSource cts = new();

        private class SavedMap
        {
            public int[] Map { get; set; } = new int[8192];
            public bool Saved { get; set; } = false;
        }

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

        private Dictionary<string, int> TileNum = new()
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

        private class Button
        {
            public string Text { get; set; }
            public (int X, int Y) Pos { get; set; }
            public int OutCol { get; set; }
            public int MidCol { get; set; }
            public int TextCol { get; set; }
            public Action Function { get; set; }
        }

        private bool Filter(List<DensityCheck> densityChecks, List<DensityComparison> densityComparisons)
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

        private void ComparisonCount(List<DensityComparison> densityComparisons, int i, int j, int curtile)
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

        private void DensityCount(List<DensityCheck> densityChecks, int i, int j, int curtile)
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

        private int MaxRadius(List<DensityCheck> densityChecks, List<DensityComparison> densityComparisons)
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
                                densityChecks[i].IncrementTryCount();
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
            Level l = new Level { X = xx, Y = yy, Sx = sizex, Sy = sizey, IsUnder = IsUnderground, Ent = [], Ene = [], Dat = new F32[8192] };
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

        private async Task ResetLevelAsync(CancellationToken ct)
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

        public override void Init(Pico8Functions pico8)
        {
            base.Init(pico8);
            camoffy = 0;
            menuY = 0;
            menuX = 0;
            tileIndex = 0;
            buttonRow1 = [];
            buttonRow2 = [];
            buttonRow3 = [];
            buttonRow4 = [];
            buttonRow5 = [];
            AddButtons();
            buttonRows = [buttonRow1, buttonRow2, buttonRow3, buttonRow4, buttonRow5];
            cts = new();
        }

        private void AddButtons()
        {
            void Generate()
            {
                foreach (var check in densityChecks)
                {
                    check.FailCount = 0;
                    check.TryCount = 0;
                }
                foreach (var check in densityComparisons)
                {
                    check.FailCount = 0;
                    check.TryCount = 0;
                }
                cts = new();
                ResetLevelTask = Task.Run(() => ResetLevelAsync(cts.Token));
                curMenu = introMenu;
            }
            buttonRow1.Add(new() { Text = "generate", Pos = (5, 5), OutCol = 7, MidCol = 2, TextCol = 7, Function = () => Generate() });

            //void Save()
            //{
            //
            //}
            //buttonRow1.Add(new() { Text = "save", Pos = (69, 5), OutCol = 7, MidCol = 2, TextCol = 7, Function = () => Save() });
            //
            //void Load()
            //{
            //
            //}
            //buttonRow1.Add(new() { Text = "load", Pos = (89, 5), OutCol = 7, MidCol = 2, TextCol = 7, Function = () => Load() });

            void NewSurfaceDensity()
            {
                densityChecks.Add(new() { IsCave = false });
            }
            buttonRow2.Add(new() { Text = "new surface density", Pos = (5, 15), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => NewSurfaceDensity() });

            void ClearSurfaceDensity()
            {
                densityChecks.RemoveAll(check => !check.IsCave);
            }
            buttonRow2.Add(new() { Text = "clear", Pos = (85, 15), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => ClearSurfaceDensity() });

            void NewSurfaceComp()
            {
                densityComparisons.Add(new() { IsCave = false });
            }
            buttonRow3.Add(new() { Text = "new surface comp", Pos = (5, 25), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => NewSurfaceComp() });

            void ClearSurfaceComp()
            {
                densityComparisons.RemoveAll(check => !check.IsCave);
            }
            buttonRow3.Add(new() { Text = "clear", Pos = (73, 25), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => ClearSurfaceComp() });

            void NewCaveDensity()
            {
                densityChecks.Add(new() { IsCave = true });
            }
            buttonRow4.Add(new() { Text = "new cave density", Pos = (5, 35), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => NewCaveDensity() });

            void ClearCaveDensity()
            {
                densityChecks.RemoveAll(check => check.IsCave);
            }
            buttonRow4.Add(new() { Text = "clear", Pos = (73, 35), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => ClearCaveDensity() });

            void NewCaveComp()
            {
                densityComparisons.Add(new() { IsCave = true });
            }
            buttonRow5.Add(new() { Text = "new cave comp", Pos = (5, 45), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => NewCaveComp() });

            void ClearCaveComp()
            {
                densityComparisons.RemoveAll(check => check.IsCave);
            }
            buttonRow5.Add(new() { Text = "clear", Pos = (61, 45), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => ClearCaveComp() });
        }

        public override void Update()
        {
            if (curMenu is not null)
            {
                if (curMenu.Spr is not null && curMenu == mainMenu)
                {
                    if (menuY > buttonRows.Count + densityChecks.Count + densityComparisons.Count * 2 - 1) { menuY = buttonRows.Count + densityChecks.Count + densityComparisons.Count * 2 - 1; }
                    if (menuY < buttonRows.Count)
                    {
                        if (menuX > buttonRows[menuY].Count - 1) { menuX = buttonRows[menuY].Count - 1; }
                        buttonRows[menuY][menuX].OutCol = 7;
                    }

                    if (menuY < buttonRows.Count)
                    {
                        if (p8.Btnp(0)) { menuX = Math.Max(0, menuX - 1); }
                        if (p8.Btnp(1)) { menuX = Math.Min(buttonRows[menuY].Count - 1, menuX + 1); }
                        if (p8.Btnp(2)) { menuY = Math.Max(0, menuY - 1); menuX = 0; }
                        if (p8.Btnp(3)) { menuY += 1; menuX = 0; }
                        if (menuY > buttonRows.Count - 1 && densityChecks.Count + densityComparisons.Count == 0) { menuY = buttonRows.Count - 1; }
                        if (menuY < buttonRows.Count)
                        {
                            if (menuX > buttonRows[menuY].Count - 1) { menuX = buttonRows[menuY].Count - 1; }
                            buttonRows[menuY][menuX].OutCol = 9;
                            if (p8.Btnp(4)) { buttonRows[menuY][menuX].Function(); }
                        }
                    }
                    else if (menuY > buttonRows.Count - 1)
                    {
                        if ((p8.Btnp(4) || p8.Btnp(5)) && menuY - buttonRows.Count < densityChecks.Count && menuX == 5) { densityChecks.RemoveAt(menuY - buttonRows.Count); return; }
                        if ((p8.Btnp(4) || p8.Btnp(5)) && (menuY - buttonRows.Count - densityChecks.Count) / 2 < densityComparisons.Count && (menuY - buttonRows.Count - densityChecks.Count) % 2 == 0 && menuX == 6) { densityComparisons.RemoveAt((menuY - buttonRows.Count - densityChecks.Count) / 2); return; }
                        if ((p8.Btnp(4) || p8.Btnp(5)) && (menuY - buttonRows.Count - densityChecks.Count) / 2 < densityComparisons.Count && (menuY - buttonRows.Count - densityChecks.Count) % 2 == 1 && menuX == 2) { densityComparisons.RemoveAt((menuY - buttonRows.Count - densityChecks.Count) / 2); return; }
                        
                        if (p8.Btn(4) || p8.Btn(5))
                        {
                            if (menuY - buttonRows.Count < densityChecks.Count)
                            {
                                DensityCheckUpdate();
                            }
                            else
                            {
                                DensityCompUpdate();
                            }
                        }
                        else
                        {
                            if (p8.Btnp(0))
                            {
                                menuX = Math.Max(menuX - 1, 0);
                                tileIndex = 0;
                            }
                            if (p8.Btnp(1))
                            {
                                if (menuY - buttonRows.Count < densityChecks.Count)
                                {
                                    menuX = Math.Min(menuX + 1, 5);
                                }
                                else if ((menuY - buttonRows.Count - densityChecks.Count) / 2 < densityComparisons.Count)
                                {
                                    menuX = Math.Min(menuX + 1, (menuY - buttonRows.Count - densityChecks.Count) % 2 == 0 ? 6 : 2);
                                }
                                tileIndex = 0;
                            }
                            if (p8.Btnp(2))
                            {
                                if (((menuY - buttonRows.Count < densityChecks.Count && menuX == 2) ||
                                    menuY - buttonRows.Count >= densityChecks.Count && (menuX == 2 || menuX == 5)) && tileIndex > 0)
                                {
                                    tileIndex -= 1;
                                }
                                else
                                {
                                    tileIndex = 0;
                                    menuY -= 1;
                                    menuX = 0;
                                }
                            }
                            if (p8.Btnp(3))
                            {
                                int group = (menuY - buttonRows.Count - densityChecks.Count) / 2;
                                if (menuY - buttonRows.Count < densityChecks.Count && menuX == 2 && tileIndex < densityChecks[menuY - buttonRows.Count].Tiles.Count)
                                {
                                    tileIndex += 1;
                                }
                                else if (menuY - buttonRows.Count >= densityChecks.Count && menuX == 2 && tileIndex < densityComparisons[group].Tiles1.Count)
                                {
                                    tileIndex += 1;
                                }
                                else if (menuY - buttonRows.Count >= densityChecks.Count && menuX == 5 && tileIndex < densityComparisons[group].Tiles2.Count)
                                {
                                    tileIndex += 1;
                                }
                                else
                                {
                                    tileIndex = 0;
                                    menuY = Math.Min(buttonRows.Count + densityChecks.Count + densityComparisons.Count * 2 - 1, menuY + 1);
                                    menuX = 0;
                                }
                            }
                        }
                    }

                    MouseState mouseState = Mouse.GetState();
                    camoffy += (prevState.ScrollWheelValue - mouseState.ScrollWheelValue) / 10;
                    int heightTotal = 0;
                    int yIndex = 0;
                    foreach (var row in buttonRows)
                    {
                        if (yIndex == menuY && (p8.Btnp(2) || p8.Btnp(3))) { camoffy = 0; }
                        heightTotal += 10;
                        yIndex++;
                    }
                    foreach (var check in densityChecks)
                    {
                        if (yIndex == menuY && (p8.Btnp(2) || p8.Btnp(3))) { camoffy = heightTotal; }
                        heightTotal += 27;
                        heightTotal += check.Tiles.Count * 6;
                        yIndex++;
                    }
                    foreach (var check in densityComparisons)
                    {
                        if ((yIndex == menuY || yIndex + 1 == menuY) && (p8.Btnp(2) || p8.Btnp(3))) { camoffy = heightTotal; }
                        heightTotal += 47;
                        heightTotal += Math.Max(check.Tiles1.Count, check.Tiles2.Count) * 6;
                        yIndex += 2;
                    }
                    camoffy = Math.Min(Math.Max(5 + heightTotal - 128, 0), Math.Max(camoffy, 0));
                    prevState = mouseState;
                    return;
                }
                else if (curMenu.Spr is not null && curMenu == introMenu)
                {
                    if (ResetLevelTask.Status == TaskStatus.RanToCompletion)
                    {
                        if (p8.Btnp(4) && !lb4)
                        {
                            curMenu = null;
                            p8.Music(1);

                            prevMap = curMap.DeepClone();
                            curMap = new() { Map = p8._map, Saved = false };

                            void SaveSeed(SavedMap map)
                            {
                                using (var image = new Image<Rgba32>(128, 64))
                                {
                                    for (int x = 0; x < 128; x++)
                                    {
                                        for (int y = 0; y < 64; y++)
                                        {
                                            var col = p8.colors[map.Map[x + y * 128] % 16];
                                            image[x, y] = new Rgba32(col.R, col.G, col.B, col.A);
                                        }
                                    }

                                    string path = Path.Combine($"{AppContext.BaseDirectory}Seeds", $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.png");
                                    try 
                                    {
                                        image.SaveAsPng(path);
                                    }
                                    catch (DirectoryNotFoundException)
                                    {
                                        Directory.CreateDirectory(Path.Combine($"{AppContext.BaseDirectory}Seeds"));
                                        image.SaveAsPng(path);
                                    }
                                }
                                map.Saved = true;
                            }
                            if (!curMap.Map.All(x => x == 0)) { p8.Menuitem(1, () => $"{(curMap.Saved == false ? "save cur seed" : "cur saved")}", () => SaveSeed(curMap)); }
                            if (!prevMap.Map.All(x => x == 0)) { p8.Menuitem(2, () => $"{(prevMap.Saved == false ? "save prev seed" : "prev saved")}", () => SaveSeed(prevMap)); }
                        }
                        lb4 = p8.Btn(4);
                    }
                    return;
                }

                Entity intMenu = curMenu;
                Entity othMenu = menuInvent;
                if (curMenu.Type == chest)
                {
                    if (p8.Btnp(0)) { toogleMenu -= 1; p8.Sfx(18, 3); }
                    if (p8.Btnp(1)) { toogleMenu += 1; p8.Sfx(18, 3); }
                    toogleMenu = (toogleMenu % 2 + 2) % 2;
                    if (toogleMenu == 1)
                    {
                        intMenu = menuInvent;
                        othMenu = curMenu;
                    }
                }

                if (intMenu.List.Count > 0)
                {
                    if (p8.Btnp(2)) { intMenu.Sel -= 1; p8.Sfx(18, 3); }
                    if (p8.Btnp(3)) { intMenu.Sel += 1; p8.Sfx(18, 3); }

                    intMenu.Sel = Loop(intMenu.Sel, intMenu.List);

                    if (p8.Btnp(5) && !lb5)
                    {
                        if (curMenu.Type == chest)
                        {
                            p8.Sfx(16, 3);
                            Entity el = intMenu.List[intMenu.Sel];
                            p8.Del(intMenu.List, el);
                            AddItemInList(othMenu.List, el, othMenu.Sel);
                            if (intMenu.List.Count > 0 && intMenu.Sel > intMenu.List.Count - 1) { intMenu.Sel -= 1; }
                            if (intMenu == menuInvent && curItem == el)
                            {
                                curItem = null;
                            }
                        }
                        else if (curMenu.Type.BeCraft)
                        {
                            if (curMenu.Sel >= 0 && curMenu.Sel < intMenu.List.Count)
                            {
                                Entity rec = curMenu.List[curMenu.Sel];
                                if (CanCraft(rec))
                                {
                                    Craft(rec);
                                    p8.Sfx(16, 3);
                                }
                                else
                                {
                                    p8.Sfx(17, 3);
                                }
                            }
                        }
                        else
                        {
                            curItem = curMenu.List[curMenu.Sel];
                            p8.Del(curMenu.List, curItem);
                            AddItemInList(curMenu.List, curItem, 0);
                            curMenu.Sel = 0;
                            curMenu = null;
                            block5 = true;
                            p8.Sfx(16, 3);
                        }
                    }
                }

                if (p8.Btnp(4) && !lb4)
                {
                    curMenu = null;
                    p8.Sfx(17, 3);
                }
                lb4 = p8.Btn(4);
                lb5 = p8.Btn(5);
                return;
            }

            if (switchLevel)
            {
                if (currentLevel == cave) { SetLevel(island); }
                else { SetLevel(cave); }
                plx = currentLevel.Stx;
                ply = currentLevel.Sty;
                FillEne(currentLevel);
                switchLevel = false;
                canSwitchLevel = false;
                p8.Music(currentLevel == cave ? 2 : 1);
            }

            if (curItem is not null)
            {
                if (HowMany(invent, curItem) <= 0) { curItem = null; }
            }

            UpGround();

            Ground playHit = GetGr(plx, ply);
            if (playHit != lastGround && playHit == grwater) { p8.Sfx(11, 3); }
            lastGround = playHit;
            int s = playHit == grwater || pstam <= 0 ? 1 : 2;
            if (playHit == grhole)
            {
                switchLevel = switchLevel || canSwitchLevel;
            }
            else
            {
                canSwitchLevel = true;
            }

            F32 dx = F32.Zero;
            F32 dy = F32.Zero;

            if (p8.Btn(0)) dx -= 1;
            if (p8.Btn(1)) dx += 1;
            if (p8.Btn(2)) dy -= 1;
            if (p8.Btn(3)) dy += 1;

            F32 dl = GetInvLen(dx, dy);

            dx *= dl;
            dy *= dl;

            if (F32.Abs(dx) > 0 || F32.Abs(dy) > 0)
            {
                lrot = GetRot(dx, dy);
                panim += F32.FromDouble(1.0 / 33.0);
            }
            else
            {
                panim = F32.Zero;
            }

            dx *= s;
            dy *= s;

            (dx, dy) = ReflectCol(plx, ply, dx, dy, IsFree, F32.Zero);

            bool canAct = true;
            (dx, dy, canAct) = UpEntity(dx, dy, canAct);

            nearEnemies = [];

            F32 ebx = p8.Cos(prot);
            F32 eby = p8.Sin(prot);
            UpEnemies(ebx, eby);

            (dx, dy) = ReflectCol(plx, ply, dx, dy, IsFree, F32.Zero);

            plx += dx;
            ply += dy;

            prot = UpRot(lrot, prot);

            llife += F32.Max(F32.Neg1, F32.Min(F32.One, plife - llife));
            lstam += F32.Max(F32.Neg1, F32.Min(F32.One, pstam - lstam));

            if (p8.Btn(5) && !block5 && canAct)
            {
                F32 bx = p8.Cos(prot);
                F32 by = p8.Sin(prot);
                F32 hitx = plx + bx * 8;
                F32 hity = ply + by * 8;
                Ground hit = GetGr(hitx, hity);

                if (!lb5 && curItem is not null && curItem.Type.Drop && (hit == grsand || hit == grgrass))
                {
                    if (curItem.List is null) { curItem.List = []; }
                    curItem.HasCol = true;

                    curItem.X = F32.Floor(hitx / 16) * 16 + 8;
                    curItem.Y = F32.Floor(hity / 16) * 16 + 8;
                    curItem.Vx = F32.Zero;
                    curItem.Vy = F32.Zero;
                    p8.Add(entities, curItem);
                    RemInList(invent, curItem);
                    canAct = false;
                }
                if (banim == 0 && pstam > 0 && canAct)
                {
                    banim = F32.FromInt(8);
                    stamCost = 20;
                    UpHit(hitx, hity, hit);
                    pstam -= stamCost;
                }
            }

            if (banim > 0)
            {
                banim -= 1;
            }

            if (pstam < 100)
            {
                pstam = F32.Min(F32.FromInt(100), pstam + 1);
            }

            int m = 16;
            F32 msp = F32.FromInt(4);

            if (F32.Abs(cmx - plx) > m)
            {
                coffx += dx * F32.FromDouble(0.4);
            }
            if (F32.Abs(cmy - ply) > m)
            {
                coffy += dy * F32.FromDouble(0.4);
            }

            cmx = F32.Max(plx - m, cmx);
            cmx = F32.Min(plx + m, cmx);
            cmy = F32.Max(ply - m, cmy);
            cmy = F32.Min(ply + m, cmy);

            coffx *= F32.FromDouble(0.9);
            coffy *= F32.FromDouble(0.9);
            coffx = F32.Min(msp, F32.Max(-msp, coffx));
            coffy = F32.Min(msp, F32.Max(-msp, coffy));

            clx += coffx;
            cly += coffy;

            clx = F32.Max(cmx - m, clx);
            clx = F32.Min(cmx + m, clx);
            cly = F32.Max(cmy - m, cly);
            cly = F32.Min(cmy + m, cly);

            if (p8.Btnp(4) && !lb4)
            {
                curMenu = menuInvent;
                p8.Sfx(13, 3);
            }

            lb4 = p8.Btn(4);
            lb5 = p8.Btn(5);
            if (!p8.Btn(5))
            {
                block5 = false;
            }

            time += F32.FromDouble(1.0 / 30.0);

            if (plife <= 0)
            {
                p8.Reload();
                p8.Memcpy(0x1000, 0x2000, 0x1000);
                curMenu = deathMenu;
                p8.Music(4);
            }
        }

        private void DensityCheckUpdate()
        {
            switch (menuX)
            {
                case 0:
                    var temp0 = densityChecks[menuY - buttonRows.Count].Radius;
                    if (p8.Btnp(0) || p8.Btn(3)) { temp0.Lb -= temp0.Lb > 1 ? 1 : 0; }
                    else if (p8.Btnp(1) || p8.Btn(2)) { temp0.Lb += temp0.Lb < temp0.Ub - 1 ? 1 : 0; }
                    densityChecks[menuY - buttonRows.Count].Radius = temp0;
                    break;
                case 1:
                    var temp1 = densityChecks[menuY - buttonRows.Count].Radius;
                    if (p8.Btnp(0) || p8.Btn(3)) { temp1.Ub -= temp1.Ub > temp1.Lb + 1 ? 1 : 0; }
                    else if (p8.Btnp(1) || p8.Btn(2)) { temp1.Ub += temp1.Ub <= (densityChecks[menuY - buttonRows.Count].IsCave ? 16 : 32) ? 1 : 0; }
                    densityChecks[menuY - buttonRows.Count].Radius = temp1;
                    break;
                case 2:
                    var temp2 = densityChecks[menuY - buttonRows.Count].Tiles;
                    if (p8.Btnp(0) || p8.Btn(3))
                    {
                        if (tileIndex >= temp2.Count)
                        {
                            densityChecks[menuY - buttonRows.Count].Tiles.Add(densityChecks[menuY - buttonRows.Count].IsCave ? 10 : 4);
                        }
                        else
                        {
                            temp2[tileIndex] -= 1;
                            if (densityChecks[menuY - buttonRows.Count].IsCave)
                            {
                                if (temp2[tileIndex] == 7) { temp2[tileIndex] = 3; }
                                if (temp2[tileIndex] == 2) { temp2[tileIndex] = 1; }
                            }
                            if (temp2[tileIndex] < 0) { temp2.RemoveAt(tileIndex); }
                            densityChecks[menuY - buttonRows.Count].Tiles = temp2;
                        }
                    }
                    else if (p8.Btnp(1) || p8.Btn(2))
                    {
                        if (tileIndex >= temp2.Count)
                        {
                            densityChecks[menuY - buttonRows.Count].Tiles.Add(0);
                        }
                        else
                        {
                            temp2[tileIndex] += 1;
                            if (densityChecks[menuY - buttonRows.Count].IsCave)
                            {
                                if (temp2[tileIndex] == 4) { temp2[tileIndex] = 8; }
                                if (temp2[tileIndex] == 2) { temp2[tileIndex] = 3; }
                            }
                            if (temp2[tileIndex] > (densityChecks[menuY - buttonRows.Count].IsCave ? 10 : 4)) { temp2.RemoveAt(tileIndex); }
                            densityChecks[menuY - buttonRows.Count].Tiles = temp2;
                        }
                    }
                    break;
                case 3:
                    var temp3 = densityChecks[menuY - buttonRows.Count].Density;
                    if (p8.Btnp(0) || p8.Btn(3)) { temp3.Lb -= temp3.Lb > 0 ? temp3.Lb <= 1 ? 0.1 : 1 : 0; }
                    else if (p8.Btnp(1) || p8.Btn(2)) { temp3.Lb += temp3.Lb < 100 ? temp3.Lb < 1 ? 0.1 : 1 : 0; }
                    temp3.Lb = Math.Round(temp3.Lb, 1);
                    densityChecks[menuY - buttonRows.Count].Density = temp3;
                    break;
                case 4:
                    var temp4 = densityChecks[menuY - buttonRows.Count].Density;
                    if (p8.Btnp(0) || p8.Btn(3)) { temp4.Ub -= temp4.Ub > 0 ? temp4.Ub <= 1 ? 0.1 : 1 : 0; }
                    else if (p8.Btnp(1) || p8.Btn(2)) { temp4.Ub += temp4.Ub < 100 ? temp4.Ub < 1 ? 0.1 : 1 : 0; }
                    temp4.Ub = Math.Round(temp4.Ub, 1);
                    densityChecks[menuY - buttonRows.Count].Density = temp4;
                    break;
                default:
                    break;
            }
        }

        private void DensityCompUpdate()
        {
            int group = (menuY - buttonRows.Count - densityChecks.Count) / 2;
            switch (menuX)
            {
                case 0:
                    if ((menuY - buttonRows.Count - densityChecks.Count) % 2 == 0)
                    {
                        var temp0 = densityComparisons[group].Radius1;
                        if (p8.Btnp(0) || p8.Btn(3)) { temp0.Lb -= temp0.Lb > 1 ? 1 : 0; }
                        else if (p8.Btnp(1) || p8.Btn(2)) { temp0.Lb += temp0.Lb < temp0.Ub - 1 ? 1 : 0; }
                        densityComparisons[group].Radius1 = temp0;
                    }
                    else
                    {
                        var temp0 = densityComparisons[group].Mag;
                        if (p8.Btnp(0) || p8.Btn(3)) { temp0 -= temp0 > 0 ? 1 : 0; }
                        else if (p8.Btnp(1) || p8.Btn(2)) { temp0 += temp0 < 100 ? 1 : 0; }
                        densityComparisons[group].Mag = temp0;
                    }
                    break;
                case 1:
                    if ((menuY - buttonRows.Count - densityChecks.Count) % 2 == 0)
                    {
                        var temp1 = densityComparisons[group].Radius1;
                        if (p8.Btnp(0) || p8.Btn(3)) { temp1.Ub -= temp1.Ub > temp1.Lb + 1 ? 1 : 0; }
                        else if (p8.Btnp(1) || p8.Btn(2)) { temp1.Ub += temp1.Ub <= (densityComparisons[group].IsCave ? 16 : 32) ? 1 : 0; }
                        densityComparisons[group].Radius1 = temp1;
                    }
                    else
                    {
                        if (p8.Btnp(0) || p8.Btn(3))
                        {
                            if (densityComparisons[group].Opr == "<") { densityComparisons[group].Opr = "="; }
                            else if (densityComparisons[group].Opr == "=") { densityComparisons[group].Opr = ">"; }
                        }
                        else if (p8.Btnp(1) || p8.Btn(2))
                        {
                            if (densityComparisons[group].Opr == ">") { densityComparisons[group].Opr = "="; }
                            else if (densityComparisons[group].Opr == "=") { densityComparisons[group].Opr = "<"; }
                        }
                    }
                    break;
                case 2:
                    var temp2 = densityComparisons[group].Tiles1;
                    if (p8.Btnp(0) || p8.Btn(3))
                    {
                        if (tileIndex >= temp2.Count)
                        {
                            densityComparisons[group].Tiles1.Add(densityComparisons[group].IsCave ? 10 : 4);
                        }
                        else
                        {
                            temp2[tileIndex] -= 1;
                            if (densityComparisons[group].IsCave)
                            {
                                if (temp2[tileIndex] == 7) { temp2[tileIndex] = 3; }
                                if (temp2[tileIndex] == 2) { temp2[tileIndex] = 1; }
                            }
                            if (temp2[tileIndex] < 0) { temp2.RemoveAt(tileIndex); }
                            densityComparisons[group].Tiles1 = temp2;
                        }
                    }
                    else if (p8.Btnp(1) || p8.Btn(2))
                    {
                        if (tileIndex >= temp2.Count)
                        {
                            densityComparisons[group].Tiles1.Add(0);
                        }
                        else
                        {
                            temp2[tileIndex] += 1;
                            if (densityComparisons[group].IsCave)
                            {
                                if (temp2[tileIndex] == 4) { temp2[tileIndex] = 8; }
                                if (temp2[tileIndex] == 2) { temp2[tileIndex] = 3; }
                            }
                            if (temp2[tileIndex] > (densityComparisons[group].IsCave ? 10 : 4)) { temp2.RemoveAt(tileIndex); }
                            densityComparisons[group].Tiles1 = temp2;
                        }
                    }
                    break;
                case 3:
                    var temp3 = densityComparisons[group].Radius2;
                    if (p8.Btnp(0) || p8.Btn(3)) { temp3.Lb -= temp3.Lb > 1 ? 1 : 0; }
                    else if (p8.Btnp(1) || p8.Btn(2)) { temp3.Lb += temp3.Lb < temp3.Ub - 1 ? 1 : 0; }
                    densityComparisons[group].Radius2 = temp3;
                    break;
                case 4:
                    var temp4 = densityComparisons[group].Radius2;
                    if (p8.Btnp(0) || p8.Btn(3)) { temp4.Ub -= temp4.Ub > temp4.Lb ? 1 : 0; }
                    else if (p8.Btnp(1) || p8.Btn(2)) { temp4.Ub += temp4.Ub <= (densityComparisons[group].IsCave ? 16 : 32) ? 1 : 0; }
                    densityComparisons[group].Radius2 = temp4;
                    break;
                case 5:
                    var temp5 = densityComparisons[group].Tiles2;
                    int index2 = 0;
                    if (p8.Btnp(0) || p8.Btn(3))
                    {
                        if (index2 >= temp5.Count)
                        {
                            densityComparisons[group].Tiles2.Add(densityComparisons[group].IsCave ? 10 : 4);
                        }
                        else
                        {
                            temp5[index2] -= 1;
                            if (densityComparisons[group].IsCave)
                            {
                                if (temp5[index2] == 7) { temp5[index2] = 3; }
                                if (temp5[index2] == 2) { temp5[index2] = 1; }
                            }
                            if (temp5[index2] < 0) { temp5.RemoveAt(index2); }
                            densityComparisons[group].Tiles2 = temp5;
                        }
                    }
                    else if (p8.Btnp(1) || p8.Btn(2))
                    {
                        if (index2 >= temp5.Count)
                        {
                            densityComparisons[group].Tiles2.Add(0);
                        }
                        else
                        {
                            temp5[index2] += 1;
                            if (densityComparisons[group].IsCave)
                            {
                                if (temp5[index2] == 4) { temp5[index2] = 8; }
                                if (temp5[index2] == 2) { temp5[index2] = 3; }
                            }
                            if (temp5[index2] > (densityComparisons[group].IsCave ? 10 : 4)) { temp5.RemoveAt(index2); }
                            densityComparisons[group].Tiles2 = temp5;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void DrawDensityCheck(int index, int x, int y)
        {
            DensityCheck check = densityChecks[index];
            int count = check.Tiles.Count * 6;
            bool selected = menuY == buttonRows.Count + index;

            p8.Rectfill(x, y + 5, x + 92, y + 25 + count, 7);
            p8.Rectfill(x + 1, y + 6, x + 85, y + 24 + count, 1);

            p8.Rectfill(x + 2, y + 1, x + 4 + (check.IsCave ? 16 : 28), y + 4, 7);
            p8.Rectfill(x + 3, y, x + 3 + (check.IsCave ? 16 : 28), y, 7);
            p8.Rectfill(x + 3, y + 1, x + 3 + (check.IsCave ? 16 : 28), y + 5, 1);
            p8.Print(check.IsCave ? "CAVE" : "SURFACE", x + 4, y + 1, 7);

            p8.Rectfill(x + 87, y + 6, x + 91, y + 24 + count, selected && menuX == 5 ? 8 : 2);
            p8.Print("x", x + 88, y + 13 + count / 2, 7);

            p8.Rectfill(x + 2, y + 7, x + 28, y + 15, 7);
            p8.Rectfill(x + 3, y + 8, x + 27, y + 14, 13);
            p8.Print("radius", x + 4, y + 9, 7);
            Printc($"{check.Radius.Lb} {(check.Radius.Ub < 10 ? " " : "  ")}", x + 16, y + 18, selected && menuX == 0 ? 9 : 7);
            Printc($"{(check.Radius.Lb < 10 ? " " : "  ")}-{(check.Radius.Ub < 10 ? " " : "  ")}", x + 16, y + 18, 7);
            Printc($"{(check.Radius.Lb < 10 ? " " : "  ")} {check.Radius.Ub}", x + 16, y + 18, selected && menuX == 1 ? 9 : 7);

            p8.Rectfill(x + 30, y + 7, x + 52, y + 15, 7);
            p8.Rectfill(x + 31, y + 8, x + 51, y + 14, 13);
            p8.Print("tiles", x + 32, y + 9, 7);
            int i = 0;
            foreach (var tile in check.Tiles)
            {
                Printc(TileNum.FirstOrDefault(val => val.Value == tile).Key, x + 42, y + 18 + i * 6, selected && i == tileIndex && menuX == 2 ? 9 : 7);
                i++;
            }
            Printc("add", x + 42, y + 18 + i * 6, selected && i == tileIndex && menuX == 2 ? 9 : 13);

            p8.Rectfill(x + 54, y + 7, x + 84, y + 15, 7);
            p8.Rectfill(x + 55, y + 8, x + 83, y + 14, 13);
            p8.Print("density", x + 56, y + 9, 7);
            Printc($"{check.Density.Lb} {new string(' ', check.Density.Ub.ToString().Length)}", x + 70, y + 18, selected && menuX == 3 ? 9 : 7);
            Printc($"{new string(' ', check.Density.Lb.ToString().Length)}-{new string(' ', check.Density.Ub.ToString().Length)}", x + 70, y + 18, 7);
            Printc($"{new string(' ', check.Density.Lb.ToString().Length)} {check.Density.Ub}", x + 70, y + 18, selected && menuX == 4 ? 9 : 7);
        }

        private void DrawDensityComparison(int index, int x, int y)
        {
            DensityComparison check = densityComparisons[index / 2];
            int count = Math.Max(check.Tiles1.Count, check.Tiles2.Count) * 6;
            bool selected1 = menuY == buttonRows.Count + densityChecks.Count + index;
            bool selected2 = menuY == buttonRows.Count + densityChecks.Count + index + 1;
            
            p8.Rectfill(x, y + 5, x + 114, y + 45 + count, 7);
            p8.Rectfill(x + 1, y + 6, x + 53, y + 24 + count, 1);
            p8.Rectfill(x + 55, y + 6, x + 107, y + 24 + count, 1);
            p8.Rectfill(x + 1, y + 26 + count, x + 107, y + 44 + count, 1);
            p8.Rectfill(x + 20, y + 26 + count, x + 88, y + 40 + count, 7);
            p8.Rectfill(x + 21, y + 26 + count, x + 87, y + 39 + count, 1);
            p8.Rectfill(x + 37, y + 40 + count, x + 71, y + 40 + count, 1);
            p8.Rectfill(x + 34, y + 38 + count, x + 34, y + 42 + count, 7);
            p8.Rectfill(x + 35, y + 39 + count, x + 35, y + 41 + count, 7);
            p8.Rectfill(x + 74, y + 38 + count, x + 74, y + 42 + count, 7);
            p8.Rectfill(x + 73, y + 39 + count, x + 73, y + 41 + count, 7);

            p8.Rectfill(x + 2, y + 1, x + 4 + (check.IsCave ? 16 : 28), y + 4, 7);
            p8.Rectfill(x + 3, y, x + 3 + (check.IsCave ? 16 : 28), y, 7);
            p8.Rectfill(x + 3, y + 1, x + 3 + (check.IsCave ? 16 : 28), y + 5, 1);
            p8.Print(check.IsCave ? "CAVE" : "SURFACE", x + 4, y + 1, 7);

            p8.Rectfill(x + 109, y + 6, x + 113, y + 44 + count, (selected1 && menuX == 6 || selected2 && menuX == 2) ? 8 : 2);
            p8.Print("x", x + 110, y + 23 + count / 2, 7);

            p8.Rectfill(x + 2, y + 7, x + 28, y + 15, 7);
            p8.Rectfill(x + 3, y + 8, x + 27, y + 14, 13);
            p8.Print("radius", x + 4, y + 9, 7);
            Printc($"{check.Radius1.Lb} {(check.Radius1.Ub < 10 ? " " : "  ")}", x + 16, y + 18, selected1 && menuX == 0 ? 9 : 7);
            Printc($"{(check.Radius1.Lb < 10 ? " " : "  ")}-{(check.Radius1.Ub < 10 ? " " : "  ")}", x + 16, y + 18, 7);
            Printc($"{(check.Radius1.Lb < 10 ? " " : "  ")} {check.Radius1.Ub}", x + 16, y + 18, selected1 && menuX == 1 ? 9 : 7);

            p8.Rectfill(x + 30, y + 7, x + 52, y + 15, 7);
            p8.Rectfill(x + 31, y + 8, x + 51, y + 14, 13);
            p8.Print("tiles", x + 32, y + 9, 7);
            int i = 0;
            foreach (var tile in check.Tiles1)
            {
                Printc(TileNum.FirstOrDefault(val => val.Value == tile).Key, x + 42, y + 18 + i * 6, selected1 && i == tileIndex && menuX == 2 ? 9 : 7);
                i++;
            }
            Printc("add", x + 42, y + 18 + i * 6, selected1 && i == tileIndex && menuX == 2 ? 9 : 13);

            p8.Rectfill(x + 56, y + 7, x + 82, y + 15, 7);
            p8.Rectfill(x + 57, y + 8, x + 81, y + 14, 13);
            p8.Print("radius", x + 58, y + 9, 7);
            Printc($"{check.Radius2.Lb} {(check.Radius2.Ub < 10 ? " " : "  ")}", x + 70, y + 18, selected1 && menuX == 3 ? 9 : 7);
            Printc($"{(check.Radius2.Lb < 10 ? " " : "  ")}-{(check.Radius2.Ub < 10 ? " " : "  ")}", x + 70, y + 18, 7);
            Printc($"{(check.Radius2.Lb < 10 ? " " : "  ")} {check.Radius2.Ub}", x + 70, y + 18, selected1 && menuX == 4 ? 9 : 7);

            p8.Rectfill(x + 84, y + 7, x + 106, y + 15, 7);
            p8.Rectfill(x + 85, y + 8, x + 105, y + 14, 13);
            p8.Print("tiles", x + 86, y + 9, 7);
            i = 0;
            foreach (var tile in check.Tiles2)
            {
                Printc(TileNum.FirstOrDefault(x => x.Value == tile).Key, x + 96, y + 18 + i * 6, selected1 && i == tileIndex && menuX == 5 ? 9 : 7);
                i++;
            }
            Printc("add", x + 96, y + 18 + i * 6, selected1 && i == tileIndex && menuX == 5 ? 9 : 13);

            p8.Rectfill(x + 39, y + 27 + count, x + 53, y + 35 + count, 7);
            p8.Rectfill(x + 40, y + 28 + count, x + 52, y + 34 + count, 13);
            p8.Print("mag", x + 41, y + 29 + count, 7);
            Printc($"{check.Mag}%", x + 47, y + 38 + count, selected2 && menuX == 0 ? 9 : 7);

            p8.Rectfill(x + 55, y + 27 + count, x + 69, y + 35 + count, 7);
            p8.Rectfill(x + 56, y + 28 + count, x + 68, y + 34 + count, 13);
            p8.Print("opr", x + 57, y + 29 + count, 7);
            p8.Print(check.Opr, x + 61, y + 38 + count, selected2 && menuX == 1 ? 9 : 7);
        }

        public override void Draw()
        {
            if (curMenu is not null && curMenu.Spr is not null)
            {
                if (curMenu == mainMenu)
                {
                    p8.Cls(12);
                    p8.Camera(F32.Zero, F32.FromInt(camoffy));
                    p8.Palt(0, false);
                    foreach (var row in buttonRows)
                    {
                        foreach (var button in row)
                        {
                            p8.Rectfill(button.Pos.X, button.Pos.Y, button.Pos.X + button.Text.Length * 4 + 2, button.Pos.Y + 8, button.OutCol);
                            p8.Rectfill(button.Pos.X + 1, button.Pos.Y + 1, button.Pos.X + button.Text.Length * 4 + 1, button.Pos.Y + 7, button.MidCol);
                            p8.Print(button.Text, button.Pos.X + 2, button.Pos.Y + 2, button.TextCol);
                        }
                    }

                    int y = 55;
                    for (int i = 0; i < densityChecks.Count; i++)
                    {
                        DrawDensityCheck(i, 5, y);
                        y += 26 + densityChecks[i].Tiles.Count * 6 + 1;

                    }

                    for (int i = 0; i < densityComparisons.Count; i++)
                    {
                        DrawDensityComparison(i * 2, 5, y);
                        y += 46 + Math.Max(densityComparisons[i].Tiles1.Count, densityComparisons[i].Tiles2.Count) * 6 + 1;
                    }

                    p8.Rectfill(124, camoffy, 127, camoffy + 128, 1);
                    p8.Rectfill(125, camoffy + ((double)camoffy / y) * 128 + 1, 126, camoffy + ((double)camoffy / y) * 128 + Math.Min(128, 128.0 / (y / 128.0)) - 2, 13);

                    return;
                }
                else if (curMenu == introMenu)
                {
                    p8.Cls(1);
                    p8.Camera();
                    p8.Palt(0, false);

                    int ypos = 2;
                    List<DensityCheck> caveChecks = [];
                    foreach (var check in densityChecks)
                    {
                        if (check.IsCave) { caveChecks.Add(check); }
                    }
                    List<DensityComparison> caveComps = [];
                    foreach (var check in densityComparisons)
                    {
                        if (check.IsCave) { caveComps.Add(check); }
                    }
                    List<DensityCheck> surfaceChecks = [];
                    foreach (var check in densityChecks)
                    {
                        if (!check.IsCave) { surfaceChecks.Add(check); }
                    }
                    List<DensityComparison> surfaceComps = [];
                    foreach (var check in densityComparisons)
                    {
                        if (!check.IsCave) { surfaceComps.Add(check); }
                    }
                    //
                    p8.Print($"cave gen failed - {Math.Max(caveChecks.Count > 0 ? caveChecks.Max(x => x.TryCount) : 0, caveComps.Count > 0 ? caveComps.Max(x => x.TryCount) : 0)}", 2, ypos, 7);
                    ypos += 7;
                    int i = 0;
                    foreach (var check in caveChecks)
                    {
                        p8.Print($"check {i} - {check.FailCount}/{check.TryCount} - {(check.FailCount > 0 ? Math.Round((double)check.FailCount / check.TryCount * 100, 2) : 0)}%", 2, ypos, 7);
                        ypos += 7;
                        i++;
                    }
                    i = 0;
                    foreach (var check in caveComps)
                    {
                        p8.Print($"comp {i} - {check.FailCount}/{check.TryCount} - {(check.FailCount > 0 ? Math.Round((double)check.FailCount / check.TryCount * 100, 2) : 0)}%", 2, ypos, 7);
                        ypos += 7;
                        i++;
                    }
                    ypos += 7;
                    //
                    p8.Print($"surface gen failed - {Math.Max(surfaceChecks.Count > 0 ? surfaceChecks.Max(x => x.TryCount) : 0, surfaceComps.Count > 0 ? surfaceComps.Max(x => x.TryCount) : 0)}", 2, ypos, 7);
                    ypos += 7;
                    i = 0;
                    foreach (var check in surfaceChecks)
                    {
                        p8.Print($"check {i} - {check.FailCount}/{check.TryCount} - {(check.FailCount > 0 ? Math.Round((double)check.FailCount / check.TryCount * 100, 2) : 0)}%", 2, ypos, 7);
                        ypos += 7;
                        i++;
                    }
                    i = 0;
                    foreach (var check in surfaceComps)
                    {
                        p8.Print($"comp {i} - {check.FailCount}/{check.TryCount} - {(check.FailCount > 0 ? Math.Round((double)check.FailCount / check.TryCount * 100, 2) : 0)}%", 2, ypos, 7);
                        ypos += 7;
                        i++;
                    }
                    //
                    if (ResetLevelTask.Status == TaskStatus.RanToCompletion)
                    {
                        Printc("press button 1", 64, 112, F32.FloorToInt(6 + time % 2));
                        time += F32.FromDouble(0.1);
                    }
                    return;
                } 
                p8.Camera();
                p8.Palt(0, false);
                p8.Rectfill(0, 0, 128, 46, 12);
                p8.Rectfill(0, 46, 128, 128, 1);
                p8.Spr((int)curMenu.Spr, 32, 14, 8, 8);
                Printc(curMenu.Text, 64, 80, 6);
                Printc(curMenu.Text2, 64, 90, 6);
                Printc("press button 1", 64, 112, F32.FloorToInt(6 + time % 2));
                time += F32.FromDouble(0.1);
                return;
            }

            p8.Cls();

            p8.Camera(clx - 64, cly - 64);

            DrawBack();

            Dent();

            Denemies();

            p8.Camera();
            Dbar(4, 4, plife, llife, 8, 2);
            Dbar(4, 9, F32.Max(F32.Zero, pstam), lstam, 11, 3);

            if (curItem is not null)
            {
                int ix = 35;
                int iy = 3;
                ItemName(ix + 1, iy + 3, curItem, 7);
                if (curItem.Count is not null)
                {
                    string c = $"{curItem.Count}";
                    p8.Print(c, ix + 88 - 16, iy + 3, 7);
                }
            }

            if (curMenu is null)
            {
                return;
            }
            p8.Camera();
            if (curMenu.Type == chest)
            {
                if (toogleMenu == 0)
                {
                    List(menuInvent, 87, 24, 84, 96, 10);
                    List(curMenu, 4, 24, 84, 96, 10);
                }
                else
                {
                    List(curMenu, -44, 24, 84, 96, 10);
                    List(menuInvent, 39, 24, 84, 96, 10);
                }
            }
            else if (curMenu.Type.BeCraft == true)
            {
                if (curMenu.Sel >= 0 && curMenu.Sel < curMenu.List.Count)
                {
                    Entity curgoal = curMenu.List[curMenu.Sel];
                    Panel("have", 71, 50, 52, 30);
                    p8.Print($"{HowMany(invent, curgoal)}", 91, 65, 7);
                    RequireList(curgoal, 4, 79, 104, 50);
                }
                List(curMenu, 4, 16, 68, 64, 6);
            }
            else
            {
                List(curMenu, 4, 24, 84, 96, 10);
            }
        }

        public override void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }
    }
}
