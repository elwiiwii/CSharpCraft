using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Pcraft;

public class HoloCraft : PcraftBase
{
    public override string SceneName => "holocraft";
    private F32 phunger;
    private F32 lhunger;
    private F32 manim;
    private F32 hungercost = F32.FromDouble(0.05);
    protected new static readonly string[] pwrNames = ["wood", "stone", "iron", "gold", "gem", "ruby"];
    protected new static readonly int[][] pwrPal = [[2, 2, 4, 4], [5, 2, 4, 13], [13, 5, 13, 6], [9, 2, 9, 10], [13, 2, 14, 12], [13, 1, 13, 12]];
    protected static readonly Material ruby = Item("ruby", 118, [1, 2, 14, 12]);
    private F32 phealthregen = F32.FromDouble(0.5);
    private F32 stamwost = F32.FromDouble(0.75);

    protected override void ResetLevel()
    {
        p8.Reload();
        p8.Memcpy(0x1000, 0x2000, 0x1000);

        phunger = F32.FromInt(100);
        lhunger = phunger;

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

        cave = CreateLevel(64, 0, 32, 32, true);
        island = CreateLevel(0, 0, 64, 64, false);

        Entity tmpworkbench = Entity(workbench, plx, ply, F32.Zero, F32.Zero);
        tmpworkbench.HasCol = true;
        tmpworkbench.List = workbenchRecipe;

        p8.Add(invent, tmpworkbench);
        p8.Add(invent, Inst(pickuptool));
    }

    public override void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        p8.Music(0, 10000);

        furnaceRecipe = [];
        workbenchRecipe = [];
        stonebenchRecipe = [];
        anvilRecipe = [];
        factoryRecipe = [];
        chemRecipe = [];

        p8.Add(factoryRecipe, Recipe(Instc(sail, 1), [Instc(fabric, 3), Instc(glue, 1)]));
        p8.Add(factoryRecipe, Recipe(Instc(boat), [Instc(wood, 30), Instc(ironbar, 8), Instc(glue, 5), Instc(sail, 4)]));

        p8.Add(chemRecipe, Recipe(Instc(glue, 1), [Instc(glass, 1), Instc(ichor, 3)]));
        p8.Add(chemRecipe, Recipe(Instc(potion, 1), [Instc(glass, 1), Instc(ichor, 1)]));

        p8.Add(furnaceRecipe, Recipe(Instc(ironbar, 1), [Instc(iron, 3)]));
        p8.Add(furnaceRecipe, Recipe(Instc(goldbar, 1), [Instc(gold, 3)]));
        p8.Add(furnaceRecipe, Recipe(Instc(glass, 1), [Instc(sand, 3)]));
        p8.Add(furnaceRecipe, Recipe(Instc(bread, 1), [Instc(wheat, 5)]));

        Material[] tooltypes = [haxe, pick, sword, shovel, scythe];
        int[] quant = [5, 5, 7, 5, 7];
        int[] pows = [1, 2, 3, 4, 5, 6];
        Material[] materials = [wood, stone, ironbar, goldbar, gem, ruby];
        int[] mult = [1, 1, 1, 1, 3, 4];
        List<Entity>[] crafter = [workbenchRecipe, stonebenchRecipe, anvilRecipe, anvilRecipe, anvilRecipe, workbenchRecipe];
        for (int j = 0; j < materials.Length; j++)
        {
            for (int i = 0; i < tooltypes.Length; i++)
            {
                var ingredients = new List<Entity> {
                    Instc(materials[j], quant[i] * mult[j])
                };
                if (materials[j] != wood)
                {
                    ingredients.Add(Instc(wood, 3));
                }
                p8.Add(
                    crafter[j],
                    Recipe(
                    SetPower(pows[j], Instc(tooltypes[i])),
                    ingredients
                    )
                );
            }
        }

        p8.Add(workbenchRecipe, Recipe(Instc(workbench, null, workbenchRecipe), [Instc(wood, 15)]));
        p8.Add(workbenchRecipe, Recipe(Instc(stonebench, null, stonebenchRecipe), [Instc(stone, 15)]));
        p8.Add(workbenchRecipe, Recipe(Instc(factory, null, factoryRecipe), [Instc(wood, 15), Instc(stone, 15)]));
        p8.Add(workbenchRecipe, Recipe(Instc(chem, null, chemRecipe), [Instc(wood, 10), Instc(glass, 3), Instc(gem, 10)]));
        p8.Add(workbenchRecipe, Recipe(Instc(chest), [Instc(wood, 15), Instc(stone, 10)]));

        p8.Add(stonebenchRecipe, Recipe(Instc(anvil, null, anvilRecipe), [Instc(iron, 25), Instc(wood, 10), Instc(stone, 25)]));
        p8.Add(stonebenchRecipe, Recipe(Instc(furnace, null, furnaceRecipe), [Instc(wood, 10), Instc(stone, 15)]));

        curMenu = mainMenu;
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
                    AddItem(ichor, F32.FloorToInt(p8.Rnd(2)) + 1, e.X, e.Y);
                    AddItem(fabric, F32.FloorToInt(p8.Rnd(2)) + 1, e.X, e.Y);
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
                    pow = (int)curItem.Power + (int)curItem.Power * 2 + p8.Rnd((int)curItem.Power * (int)curItem.Power - ((int)curItem.Power - 1));
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
                phunger = F32.Min(F32.FromInt(100), phunger + (int)curItem.Type.GiveLife);
                RemInList(invent, Instc(curItem.Type, 1));
                p8.Sfx(21, 3);
            }
            switch (hit, curItem.Type)
            {
                case (Ground, Material) gm when gm == (grgrass, scythe):
                    SetGr(hitx, hity, grsand);
                    if (p8.Rnd(1) > F32.FromDouble(0.0)) { AddItem(seed, 1, hitx, hity); }
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
                    curMenu = winMenu;
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
        lhunger += F32.Max(F32.Neg1, F32.Min(F32.One, phunger - lhunger));
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

        if (manim > 0)
        {
            manim -= 1;
        }
        
        if (pstam < 100)
        {
            pstam = F32.Min(F32.FromInt(100), pstam + stamwost);
        }
        
        if (phunger <= 100)
        {
            phunger = F32.Min(F32.FromInt(100), phunger - hungercost);
        }
        if (phunger <= 0)
        {
            plife = F32.Min(F32.FromInt(100), plife - 1);
        }
        
        if (phunger > 50)
        {
            if (plife < 100)
            {
                plife = F32.Min(F32.FromInt(100), plife + phealthregen);
                phunger = F32.Min(F32.FromInt(100), phunger - hungercost);
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
            curMenu = deathMenu;
            p8.Music(4);
        }
    }

    protected override void Dplayer(F32 x, F32 y, F32 rot, F32 anim, F32 subanim, bool isplayer)
    {
        F32 cr = p8.Cos(rot);
        F32 sr = p8.Sin(rot);
        F32 cv = -sr;
        F32 sv = cr;

        x = F32.Floor(x);
        y = F32.Floor(y - 4);

        F32 lan = p8.Sin(anim * 2) * F32.FromDouble(1.5);

        Ground bel = GetGr(x, y);
        if (bel == grwater)
        {
            y += 4;
            p8.Circ(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, 3, 6);
            p8.Circ(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, 3, 6);

            F32 anc = 3 + time * 3 % 1 * 3;
            p8.Circ(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, anc.Double, 6);
            p8.Circ(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, anc.Double, 6);
        }
        else
        {
            p8.Circfill(x + cv * 2 - cr * lan, y + 3 + sv * 2 - sr * lan, 3, 1);
            p8.Circfill(x - cv * 2 + cr * lan, y + 3 - sv * 2 + sr * lan, 3, 1);
        }

        F32 blade = Pico8Functions.Mod(rot + F32.FromDouble(0.25), 1);
        if (subanim > 0)
        {
            blade = blade - F32.FromDouble(0.3) + subanim * F32.FromDouble(0.04);
        }
        F32 bcr = p8.Cos(blade);
        F32 bsr = p8.Sin(blade);

        (int mx, int my) = Mirror(blade);

        int weap = 75;

        if (isplayer && curItem is not null)
        {
            p8.Pal();
            weap = curItem.Type.Spr;
            if (curItem.Power is not null)
            {
                SetPal(pwrPal[(int)curItem.Power - 1]);
            }
            if (curItem.Type is not null && curItem.Type.Pal is not null)
            {
                SetPal(curItem.Type.Pal);
            }
        }

        p8.Spr(weap, (x + bcr * 4 - cr * lan - mx * 8 + 1).Double, (y + bsr * 4 - sr * lan + my * 8 - 7).Double, 1, 1, mx == 1, my == 1);

        if (isplayer) { p8.Pal(); }

        if (bel != grwater)
        {
            p8.Circfill(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, 3, 2);
            p8.Circfill(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, 3, 2);

            (int my2, int mx2) = Mirror(Pico8Functions.Mod(rot + F32.FromDouble(0.75), 1));
            p8.Spr(75, (x + cv * 4 + cr * lan - 8 + mx2 * 8 + 1).Double, (y + sv * 4 + sr * lan + my2 * 8 - 7).Double, 1, 1, mx2 == 0, my2 == 1);
        }

        p8.Circfill(x + cr, y + sr - 2, 4, 2);
        p8.Circfill(x + cr, y + sr, 4, 2);
        p8.Circfill(x + cr * F32.FromDouble(1.5), y + sr * F32.FromDouble(1.5) - 2, 2.5, 15);
        p8.Circfill(x - cr, y - sr - 3, 3, 4);
    }

    protected override void ItemName(int x, int y, Entity it, int col)
    {
        Material ty = it.Type;
        p8.Pal();
        int px = x;
        if (it.Power is not null)
        {
            string pwn = pwrNames[(int)it.Power - 1];
            p8.Print(pwn, x + 10, y, col);
            px += pwn.Length * 4 + 4;
            SetPal(pwrPal[(int)it.Power - 1]);
        }
        if (ty.Pal is not null) { SetPal(ty.Pal); }
        p8.Spr(ty.Spr, x, y - 2);
        p8.Pal();
        p8.Print(ty.Name, px + 10, y, col);
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
        Dbar(4, 14, F32.Max(F32.Zero, phunger), lhunger % 1, 14, 1);

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