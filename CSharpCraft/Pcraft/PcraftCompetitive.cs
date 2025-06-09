using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Pcraft;

public class PcraftCompetitive : SpeedrunBase
{
    public override string SceneName => "comp_pcraft";

    public override void Init(Pico8Functions pico8)
    {
        base.Init(pico8);
    }

    private async Task ResetLevelAsync(CancellationToken ct)
    {
        runtimer = 0;
        frameTimer = F32.Zero;
        timer = "0:00.00";

        zombiesKilled = 0;
        zombiesDroppedCount = (0, 0);
        barFull = new int[7];
        menuTime = new int[8];
        ladderResets = new int[2];
        missedHits = new int[8];
        wastedHits = new int[9];
        pickupAction = false;
        placeAction = false;

        zReset = 0;

        int incr = 0;
        WatRng = new Random(rngSeed + incr);
        incr++;
        pSpawnRng = stdPlayerSpawn ? new Random(rngSeed + incr) : null;
        incr++;
        zSpawnRng = stdZombieSpawns ? new Random(rngSeed + incr) : null;
        incr++;
        zMoveRng = stdZombieMovement ? new Random(rngSeed + incr) : null;
        incr++;
        foreach (var key in dropsDict.Keys)
        {
            dropsDict[key] = stdDrops ? new Random(rngSeed + incr) : null;
            incr++;
        }
        foreach (var key in spreadDict.Keys)
        {
            spreadDict[key] = stdSpread ? new Random(rngSeed + incr) : null;
            incr++;
        }
        foreach (var key in damageDict.Keys)
        {
            damageDict[key] = [];
            for (int i = 0; i < pwrNames.Length; i++)
            {
                damageDict[key].Add(stdDamage ? new Random(rngSeed + incr) : null);
                incr++;
            }
        }
        zombieDamage = stdDamage ? new Random(rngSeed + incr) : null;
        incr++;
        sandTimer = stdSpread ? new Random(rngSeed + incr) : null;
        incr++;
        wheatTimer = stdSpread ? new Random(rngSeed + incr) : null;

        foreach (Ground ground in grounds)
        {
            ground.MinedCount = 0;
            ground.DroppedCount = (0, 0);
        }

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
                Rndwat[i][j] = p8.Rnd(100, WatRng);
            }
        }

        cave = await SeedFilter.CreateLevelAsync(64, 0, 32, 32, true);
        island = await SeedFilter.CreateLevelAsync(0, 0, 64, 64, false);

        Entity tmpworkbench = Entity(workbench, plx, ply, F32.Zero, F32.Zero);
        tmpworkbench.HasCol = true;
        tmpworkbench.List = workbenchRecipe;

        p8.Add(invent, tmpworkbench);
        p8.Add(invent, Inst(pickuptool));
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

            if (curMenu == menuInvent)
            {
                menuTime[0]++;
            }
            else if (curMenu.Type == workbench)
            {
                menuTime[1]++;
            }
            else if (curMenu.Type == stonebench)
            {
                menuTime[2]++;
            }
            else if (curMenu.Type == furnace)
            {
                menuTime[3]++;
            }
            else if (curMenu.Type == anvil)
            {
                menuTime[4]++;
            }
            else if (curMenu.Type == chem)
            {
                menuTime[5]++;
            }
            else if (curMenu.Type == factory)
            {
                menuTime[6]++;
            }
            else if (curMenu.Type == chest)
            {
                menuTime[7]++;
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
                        exitMenu = true;
                    }
                }
            }

            if (p8.Btnp(4) && !lb4)
            {
                curMenu = null;
                p8.Sfx(17, 3);
                exitMenu = true;
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
            if (zReset == 0) { ladderResets[0]++; } else { ladderResets[1]++; }
            if (currentLevel == cave)
            {
                if (zReset == 1)
                {
                    zSpawnRng = stdZombieSpawns ? new Random(rngSeed - 1) : null;
                    zMoveRng = stdZombieSpawns ? new Random(rngSeed - 10) : null;
                    zReset = 2;
                }
                SetLevel(island);
            }
            else
            {
                if (zReset == 1 || zReset == 2)
                {
                    zSpawnRng = stdZombieSpawns ? new Random(rngSeed - 2) : null;
                    zMoveRng = stdZombieSpawns ? new Random(rngSeed - 20) : null;
                    zReset = 3;
                }
                SetLevel(cave);
            }
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
                placeAction = true;
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
        exitMenu = false;

        if (banim > 0)
        {
            banim -= 1;
        }

        if (pstam < 100)
        {
            pstam = F32.Min(F32.FromInt(100), pstam + 1);
        }
        if (pstam >= 100 && runtimer == 1)
        {
            if (curItem is not null)
            {
                if (curItem.Power is null)
                {
                    barFull[0]++;
                }
                else if (curItem.Power is not null && curItem.Power == 1)
                {
                    barFull[1]++;
                }
                else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == haxe)
                {
                    barFull[2]++;
                }
                else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == pick)
                {
                    barFull[3]++;
                }
                else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == sword)
                {
                    barFull[4]++;
                }
                else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == shovel)
                {
                    barFull[5]++;
                }
                else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == scythe)
                {
                    barFull[6]++;
                }
            }
            else
            {
                barFull[0]++;
            }
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
            deathMenu = Cmenu(inventary, null, 128, "you died!", timer);
            curMenu = deathMenu;
            runtimer = 0;
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

    public override void Dispose()
    {
        base.Dispose();
        
    }
}
