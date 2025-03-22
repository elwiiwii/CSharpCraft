﻿using CSharpCraft.Pico8;
using FixMath;
using Force.DeepCloner;
using Google.Protobuf.Reflection;

namespace CSharpCraft.Pcraft
{
    public class FilterTest : PcraftBase
    {
        public override string SceneName => "filter";

        private int runtimer = 0;
        private F32 frameTimer = F32.Zero;
        private string timer = "0:00.00";

        private bool found = false;
        private object lockObj = new();
        private List<Button> buttonRow1 = [];
        private List<Button> buttonRow2 = [];
        private List<Button> buttonRow3 = [];
        List<DensityCheck> densityChecks = [];
        List<DensityComparison> densityComparisons = [];

        Random random = new();

        private class DensityCheck
        {
            public (int Lb, int Ub) Radius { get; set; } = (1, 2);
            public List<int> Tiles { get; set; } = [];
            public (double Lb, double Ub) Density { get; set; } = (0, 100);
            public int Count { get; set; } = 0;
        }

        private class DensityComparison
        {
            public (int Lb, int Ub) Radius1 { get; set; } = (1, 2);
            public List<int> Tiles1 { get; set; } = [];
            public int Count1 { get; set; } = 0;
            public (int Lb, int Ub) Radius2 { get; set; } = (1, 2);
            public List<int> Tiles2 { get; set; } = [];
            public int Count2 { get; set; } = 0;
            public int Mag { get; set; } = 100;
            public string Opr { get; set; } = "=";
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
                double area = ((check.Radius.Ub + check.Radius.Ub * check.Radius.Ub) - (check.Radius.Lb + check.Radius.Lb * check.Radius.Lb)) * 4;
                if (check.Count / area < check.Density.Lb / 100.0 || check.Count / area > check.Density.Ub / 100.0)
                {
                    return true;
                }
                else
                {

                }
            }

            foreach (var check in densityComparisons)
            {
                double area1 = ((check.Radius1.Ub + check.Radius1.Ub * check.Radius1.Ub) - (check.Radius1.Lb + check.Radius1.Lb * check.Radius1.Lb)) * 4;
                double area2 = ((check.Radius2.Ub + check.Radius2.Ub * check.Radius2.Ub) - (check.Radius2.Lb + check.Radius2.Lb * check.Radius2.Lb)) * 4;
                double density1 = check.Count1 / area1;
                double density2 = check.Count2 / area2;
                switch (check.Opr)
                {
                    case ">":
                        if (density1 - density2 < check.Mag / 100.0)
                        {
                            return true;
                        }
                        return false;
                    case "=":
                        if (Math.Abs(density1 - density2) > check.Mag / 2.0 / 100.0)
                        {
                            return true;
                        }
                        return false;
                    case "<":
                        if (density1 - density2 > -check.Mag / 100.0)
                        {
                            return true;
                        }
                        return false;
                    default:
                        return true;
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

        private void CreateMapStepCheck(List<DensityCheck> densityChecks, List<DensityComparison> densityComparisons, int sx, int sy, int a, int b, int c, int d, int e)
        {
            Parallel.For(0, int.MaxValue, (index, state) =>
            {
                if (found)
                {
                    return;
                }

                bool needmap = true;
                List<DensityCheck> densityChecksClone = DeepClonerExtensions.DeepClone(densityChecks);
                List<DensityComparison> densityComparisonsClone = DeepClonerExtensions.DeepClone(densityComparisons);

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
                        }
                    }
                }
            });

            found = false;
        }

        protected override void CreateMap()
        {
            bool needmap = true;

            while (needmap)
            {
                needmap = false;
                
                if (levelUnder)
                {
                    CreateMapStepCheck(densityChecks, densityComparisons, levelsx, levelsy, 3, 8, 1, 9, 10);

                    if (typeCount[8] < 30) { needmap = true; }
                    if (typeCount[9] < 20) { needmap = true; }
                    if (typeCount[10] < 15) { needmap = true; }
                }
                else
                {
                    CreateMapStepCheck(densityChecks, densityComparisons, levelsx, levelsy, 0, 1, 2, 3, 4);

                    if (typeCount[3] < 30) { needmap = true; }
                    if (typeCount[4] < 30) { needmap = true; }

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

        protected override void ResetLevel()
        {
            runtimer = 0;
            frameTimer = F32.Zero;
            timer = "0:00.00";
            base.ResetLevel();
        }

        public override void Init(Pico8Functions pico8)
        {
            base.Init(pico8);
            AddButtons();
            densityChecks.Add(new DensityCheck() { Radius = (1, 6), Tiles = [TileNum["water"]], Density = (0, 1) });
            densityChecks.Add(new DensityCheck() { Radius = (1, 5), Tiles = [TileNum["sand"]], Density = (17, 100) });
            densityChecks.Add(new DensityCheck() { Radius = (1, 6), Tiles = [TileNum["tree"], TileNum["water"]], Density = (30, 40) });
            densityComparisons.Add(new DensityComparison() { Tiles1 = [TileNum["water"], TileNum["water"]], Tiles2 = [TileNum["water"]] });
            }

        private void AddButtons()
        {
            void Generate()
            {
                ResetLevel();
                curMenu = null;
                p8.Music(1);
            }
            buttonRow1.Add(new() { Text = "generate", Pos = (6, 9), OutCol = 7, MidCol = 2, TextCol = 7, Function = () => Generate() });

            void Save()
            {

            }
            buttonRow1.Add(new() { Text = "save", Pos = (62, 9), OutCol = 7, MidCol = 2, TextCol = 7, Function = () => Save() });

            void Load()
            {

            }
            buttonRow1.Add(new() { Text = "load", Pos = (82, 9), OutCol = 7, MidCol = 2, TextCol = 7, Function = () => Load() });

            void NewDensityCheck()
            {
                densityChecks.Add(new());
            }
            buttonRow2.Add(new() { Text = "new density check", Pos = (6, 19), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => NewDensityCheck() });

            void ClearDensityChecks()
            {
                densityChecks.Clear();
            }
            buttonRow2.Add(new() { Text = "clear", Pos = (78, 19), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => ClearDensityChecks() });

            void NewRelativeCheck()
            {
                densityComparisons.Add(new());
            }
            buttonRow3.Add(new() { Text = "new relative check", Pos = (6, 29), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => NewRelativeCheck() });

            void ClearRelativeChecks()
            {
                densityComparisons.Clear();
            }
            buttonRow3.Add(new() { Text = "clear", Pos = (82, 29), OutCol = 7, MidCol = 1, TextCol = 6, Function = () => ClearRelativeChecks() });
        }

        protected override void UpHit(F32 hitx, F32 hity, Ground hit)
        {
            if (nearEnemies.Count > 0)
            {
                p8.Sfx(19, 3);
                F32 pow = F32.One;
                if (curItem is not null && curItem.Type == sword)
                {
                    pow = 1 + (int)curItem.Power + p8.Rnd((int)curItem.Power * (int)curItem.Power);
                    stamCost = Math.Max(0, 20 - (int)curItem.Power * 2);
                    pow = F32.Floor(pow);
                    p8.Sfx(14 + p8.Rnd(2).Double, 3);
                }
                foreach (Entity e in nearEnemies)
                {
                    e.Life -= pow / nearEnemies.Count;
                    F32 push = (pow - 1) * F32.Half;
                    e.Ox += F32.Max(-push, F32.Min(push, e.X - plx));
                    e.Oy += F32.Max(-push, F32.Min(push, e.Y - ply));
                    if (e.Life <= 0)
                    {
                        p8.Del(enemies, e);
                        AddItem(ichor, F32.FloorToInt(p8.Rnd(3)), e.X, e.Y);
                        AddItem(fabric, F32.FloorToInt(p8.Rnd(3)), e.X, e.Y);
                    }
                    p8.Add(entities, SetText(pow.ToString(), 9, F32.FromInt(20), Entity(etext, e.X, e.Y - 10, F32.Zero, F32.Neg1)));
                }
            }
            else if (hit.Mat is not null)
            {
                p8.Sfx(15, 3);
                F32 pow = F32.One;
                if (curItem is not null)
                {
                    if (hit == grtree)
                    {
                        if (curItem.Type == haxe)
                        {
                            pow = 1 + (int)curItem.Power + p8.Rnd((int)curItem.Power * (int)curItem.Power);
                            stamCost = Math.Max(0, 20 - (int)curItem.Power * 2);
                            p8.Sfx(12, 3);
                        }
                    }
                    else if ((hit == grrock || hit.IsTree) && curItem.Type == pick)
                    {
                        pow = 1 + (int)curItem.Power * 2 + p8.Rnd((int)curItem.Power * (int)curItem.Power);
                        stamCost = Math.Max(0, 20 - (int)curItem.Power * 2);
                        p8.Sfx(12, 3);
                    }
                }
                pow = F32.Floor(pow);

                F32 d = GetData(hitx, hity, hit.Life);
                if (d - pow <= 0)
                {
                    SetGr(hitx, hity, hit.Tile);
                    Cleardata(hitx, hity);
                    AddItem(hit.Mat, F32.FloorToInt(p8.Rnd(3) + 2), hitx, hity);
                    if (hit == grtree && p8.Rnd(1) > F32.FromDouble(0.7))
                    {
                        AddItem(apple, 1, hitx, hity);
                    }
                }
                else
                {
                    SetData(hitx, hity, d - pow);
                }
                p8.Add(entities, SetText(pow.ToString(), 10, F32.FromInt(20), Entity(etext, hitx, hity, F32.Zero, F32.Neg1)));
            }
            else
            {
                p8.Sfx(19, 3);
                if (curItem is null)
                {
                    return;
                }
                if (curItem.Power is not null)
                {
                    stamCost = Math.Max(0, 20 - (int)curItem.Power * 2);
                }
                if (curItem.Type.GiveLife is not null)
                {
                    plife = F32.Min(F32.FromInt(100), plife + (int)curItem.Type.GiveLife);
                    RemInList(invent, Instc(curItem.Type, 1));
                    p8.Sfx(21, 3);
                }
                switch (hit, curItem.Type)
                {
                    case (Ground, Material) gm when gm == (grgrass, scythe):
                        SetGr(hitx, hity, grsand);
                        if (p8.Rnd(1) > F32.FromDouble(0.4)) { AddItem(seed, 1, hitx, hity); }
                        break;
                    case (Ground, Material) gm when gm == (grsand, shovel):
                        if (curItem.Power > 3)
                        {
                            SetGr(hitx, hity, grwater);
                            AddItem(sand, 2, hitx, hity);
                        }
                        else
                        {
                            SetGr(hitx, hity, grfarm);
                            SetData(hitx, hity, time + 15 + p8.Rnd(5));
                            AddItem(sand, F32.FloorToInt(p8.Rnd(2)), hitx, hity);
                        }
                        break;
                    case (Ground, Material) gm when gm == (grwater, sand):
                        SetGr(hitx, hity, grsand);
                        RemInList(invent, Instc(sand, 1));
                        break;
                    case (Ground, Material) gm when gm == (grwater, boat):
                        p8.Reload();
                        p8.Memcpy(0x1000, 0x2000, 0x1000);
                        curMenu = Cmenu(inventary, null, 136, "final time:", timer);
                        runtimer = 0;
                        p8.Music(3);
                        break;
                    case (Ground, Material) gm when gm == (grfarm, seed):
                        SetGr(hitx, hity, grwheat);
                        SetData(hitx, hity, time + 15 + p8.Rnd(5));
                        RemInList(invent, Instc(seed, 1));
                        break;
                    case (Ground, Material) gm when gm == (grwheat, scythe):
                        SetGr(hitx, hity, grsand);
                        F32 d = F32.Max(F32.Zero, F32.Min(F32.FromInt(4), 4 - (GetData(hitx, hity, 0) - time)));
                        AddItem(wheat, F32.FloorToInt(d / 2 + p8.Rnd((d / 2).Double)), hitx, hity);
                        AddItem(seed, 1, hitx, hity);
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Update()
        {
            if (runtimer == 1)
            {
                frameTimer += F32.FromRaw(1);
            }
            timer = $"{F32.FloorToInt(frameTimer / F32.FromRaw(1800))}:{$"{100.001 + (frameTimer % F32.FromRaw(1800) / F32.FromRaw(30)).Double}".Substring(1, 5)}";

            if (curMenu is not null)
            {
                if (curMenu.Spr is not null)
                {
                    if (p8.Btnp(4) && !lb4)
                    {
                        if (curMenu == mainMenu)
                        {
                            curMenu = introMenu;
                        }
                        else
                        {
                            ResetLevel();
                            curMenu = null;
                            p8.Music(1);
                        }
                    }
                    lb4 = p8.Btn(4);
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

            for (int i = 0; i <= 5; i++)
            {
                if (p8.Btnp(i))
                {
                    runtimer = 1;
                }
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
                curMenu = Cmenu(inventary, null, 128, "final time:", timer);
                runtimer = 0;
                p8.Music(4);
            }
        }

        private void DrawDensityCheck(DensityCheck check, int x, int y)
        {
            int count = check.Tiles.Count * 6;
            p8.Rectfill(x, y, x + 92, y + 20 + count, 7);
            p8.Rectfill(x + 1, y + 1, x + 85, y + 19 + count, 1);

            p8.Rectfill(x + 87, y + 1, x + 91, y + 19 + count, 2);
            p8.Print("x", x + 88, y + 8 + count / 2, 7);

            p8.Rectfill(x + 2, y + 2, x + 28, y + 10, 7);
            p8.Rectfill(x + 3, y + 3, x + 27, y + 9, 13);
            p8.Print("radius", x + 4, y + 4, 7);
            Printc($"{check.Radius.Lb}-{check.Radius.Ub}", x + 16, y + 13, 7);

            p8.Rectfill(x + 30, y + 2, x + 52, y + 10, 7);
            p8.Rectfill(x + 31, y + 3, x + 51, y + 9, 13);
            p8.Print("tiles", x + 32, y + 4, 7);
            int i = 0;
            foreach (var tile in check.Tiles)
            {
                Printc(TileNum.FirstOrDefault(x => x.Value == tile).Key, x + 42, y + 13 + i * 6, 7);
                i++;
            }
            Printc("add", x + 42, y + 13 + i * 6, 13);

            p8.Rectfill(x + 54, y + 2, x + 84, y + 10, 7);
            p8.Rectfill(x + 55, y + 3, x + 83, y + 9, 13);
            p8.Print("density", x + 56, y + 4, 7);
            Printc($"{check.Density.Lb}-{check.Density.Ub}", x + 70, y + 13, 7);
        }

        private void DrawDensityComparison(DensityComparison check, int x, int y)
        {
            x = 6;
            y = 73;
            int count = Math.Max(check.Tiles1.Count, check.Tiles1.Count) * 6;
            p8.Rectfill(x, y, x + 114, y + 40 + count, 7);
            p8.Rectfill(x + 1, y + 1, x + 53, y + 19 + count, 1);
            p8.Rectfill(x + 55, y + 1, x + 107, y + 19 + count, 1);
            p8.Rectfill(x + 1, y + 21 + count, x + 107, y + 39 + count, 1);
            p8.Rectfill(x + 20, y + 21 + count, x + 88, y + 35 + count, 7);
            p8.Rectfill(x + 21, y + 21 + count, x + 87, y + 34 + count, 1);
            p8.Rectfill(43, 108 + count, 77, 108 + count, 1);
            p8.Rectfill(40, 106 + count, 40, 110 + count, 7);
            p8.Rectfill(41, 107 + count, 41, 109 + count, 7);
            p8.Rectfill(80, 106 + count, 80, 110 + count, 7);
            p8.Rectfill(79, 107 + count, 79, 109 + count, 7);

            p8.Rectfill(115, 74, 119, 112 + count, 2);
            p8.Print("x", 116, 91 + count / 2, 7);

            p8.Rectfill(8, 75, 34, 83, 7);
            p8.Rectfill(9, 76, 33, 82, 13);
            p8.Print("radius", 10, 77, 7);
            Printc($"{check.Radius1.Lb}-{check.Radius1.Ub}", 22, 86, 7);

            p8.Rectfill(36, 75, 58, 83, 7);
            p8.Rectfill(37, 76, 57, 82, 13);
            p8.Print("tiles", 38, 77, 7);
            int i = 0;
            foreach (var tile in check.Tiles1)
            {
                Printc(TileNum.FirstOrDefault(x => x.Value == tile).Key, 48, 86 + i * 6, 7);
                i++;
            }
            Printc("add", 48, 86 + i * 6, 13);

            p8.Rectfill(62, 75, 88, 83, 7);
            p8.Rectfill(63, 76, 87, 82, 13);
            p8.Print("radius", 64, 77, 7);
            Printc($"{check.Radius2.Lb}-{check.Radius2.Ub}", 76, 86, 7);

            p8.Rectfill(90, 75, 112, 83, 7);
            p8.Rectfill(91, 76, 111, 82, 13);
            p8.Print("tiles", 92, 77, 7);
            i = 0;
            foreach (var tile in check.Tiles2)
            {
                Printc(TileNum.FirstOrDefault(x => x.Value == tile).Key, 102, 86 + i * 6, 7);
                i++;
            }
            Printc("add", 102, 86 + i * 6, 13);

            p8.Rectfill(45, 95 + count, 59, 103 + count, 7);
            p8.Rectfill(46, 96 + count, 58, 102 + count, 13);
            p8.Print("mag", 47, 97 + count, 7);
            Printc($"{check.Mag}%", 53, 106 + count, 7);

            p8.Rectfill(61, 95 + count, 75, 103 + count, 7);
            p8.Rectfill(62, 96 + count, 74, 102 + count, 13);
            p8.Print("opr", 63, 97 + count, 7);
            p8.Print(check.Opr, 67, 106 + count, 7);
        }

        public override void Draw()
        {
            if (curMenu is not null && curMenu.Spr is not null)
            {
                if (curMenu == mainMenu)
                {
                    p8.Cls(12);
                    p8.Camera();
                    p8.Palt(0, false);
                    List<List<Button>> rows = [buttonRow1, buttonRow2, buttonRow3];
                    foreach (var row in rows)
                    {
                        foreach (var button in row)
                        {
                            p8.Rectfill(button.Pos.X, button.Pos.Y, button.Pos.X + button.Text.Length * 4 + 2, button.Pos.Y + 8, button.OutCol);
                            p8.Rectfill(button.Pos.X + 1, button.Pos.Y + 1, button.Pos.X + button.Text.Length * 4 + 1, button.Pos.Y + 7, button.MidCol);
                            p8.Print(button.Text, button.Pos.X + 2, button.Pos.Y + 2, button.TextCol);
                        }
                    }

                    foreach (var check in densityChecks)
                    {
                        DrawDensityCheck(check, 6, 39);
                    }

                    foreach (var check in densityComparisons)
                    {
                        DrawDensityComparison(check, 6, 73);
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

            Printb(timer, 124 - timer.Length * 4, curMenu is not null ? 41 : 118, 7);

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
    }
}
