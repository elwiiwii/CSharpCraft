using CSharpCraft.Pico8;
using FixMath;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CSharpCraft.Pcraft
{
    public class FilterTest : PcraftBase
    {
        public override string SceneName => "filter";

        private int runtimer = 0;
        private F32 frameTimer = F32.Zero;
        private string timer = "0:00.00";

        private class DensityCheck
        {
            public (int Lb, int Ub) Radius { get; set; }
            public List<int> Tiles { get; set; } = [];
            public (double Lb, double Ub) Density { get; set; }
            public int Count { get; set; }
        }

        private class DensityComparison
        {
            public (int Lb, int Ub) Radius1 { get; set; }
            public List<int> Tiles1 { get; set; } = [];
            public int Count1 { get; set; }
            public (int Lb, int Ub) Radius2 { get; set; }
            public List<int> Tiles2 { get; set; } = [];
            public int Count2 { get; set; }
            public int Mag { get; set; }
            public string Opr { get; set; } = "";
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

        private bool Filter(List<DensityCheck> densityChecks, List<DensityComparison> densityComparisons)
        {
            int radius = Math.Min(levelsx/2-1, MaxRadius(densityChecks, densityComparisons));
            int center = levelsx/2;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int curtile = F32.FloorToInt(level[center + i][center + j]);
                    if (curtile == 0)
                    {

                    }
                    DensityCount(densityChecks, i, j, curtile);
                    ComparisonCount(densityComparisons, i, j, curtile);
                }
            }

            foreach (var check in densityChecks)
            {
                double area = ((check.Radius.Ub + check.Radius.Ub * check.Radius.Ub) - (check.Radius.Lb + check.Radius.Lb * check.Radius.Lb)) * 4;
                if (check.Count / area < check.Density.Lb / 100.0 || check.Count / area > check.Density.Ub / 100.0)
                {
                    return true;
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
                        break;
                    case "=":
                        if (Math.Abs(density1 - density2) > check.Mag / 2.0 / 100.0)
                        {
                            return true;
                        }
                        break;
                    case "<":
                        if (density1 - density2 > -check.Mag / 100.0)
                        {
                            return true;
                        }
                        break;
                    default:
                        return true;
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

        protected override void CreateMap()
        {
            bool needmap = true;

            while (needmap)
            {
                needmap = false;
                holex = levelsx / 2 + levelx;
                holey = levelsy / 2 + levely;

                if (levelUnder)
                {
                    level = CreateMapStep(levelsx, levelsy, 3, 8, 1, 9, 10);

                    if (typeCount[8] < 30) { needmap = true; }
                    if (typeCount[9] < 20) { needmap = true; }
                    if (typeCount[10] < 15) { needmap = true; }
                }
                else
                {
                    level = CreateMapStep(levelsx, levelsy, 0, 1, 2, 3, 4);

                    if (typeCount[3] < 30) { needmap = true; }
                    if (typeCount[4] < 30) { needmap = true; }
                    List<DensityCheck> densityChecks = [];
                    densityChecks.Add(new DensityCheck() { Radius = (0, 8), Tiles = [TileNum["water"]], Density = (50, 100) });
                    List<DensityComparison> densityComparisons = [];
                    //densityComparisons.Add(new DensityComparison() { Radius1 = (0, 8), Tiles1 = [TileNum["tree"]], Radius2 = (0, 8), Tiles2 = [TileNum["stone"]], Mag = 40, Opr = ">" });
                    needmap = Filter(densityChecks, densityComparisons);
                }

                if (!needmap)
                {
                    plx = F32.Neg1;
                    ply = F32.Neg1;

                    for (int i = 0; i <= 500; i++)
                    {
                        int depx = F32.FloorToInt(levelsx / 2 - 4 + p8.Rnd(9));
                        int depy = F32.FloorToInt(levelsy / 2 - 4 + p8.Rnd(9));
                        F32 c = level[depx][depy];

                        if ((c == 1 || c == 2) && !(depx == holex && depy == holey))
                        {
                            plx = F32.FromInt(depx * 16 + 8);
                            ply = F32.FromInt(depy * 16 + 8);
                            break;
                        }
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
                curMenu = deathMenu;
                p8.Music(4);
            }
        }

        public override void Draw()
        {
            if (curMenu is not null && curMenu.Spr is not null)
            {
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
