using System.Reflection;
using System.Threading.Channels;
using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Pcraft;

public abstract class PcraftBase : IScene, IDisposable
{
    public virtual string SceneName => "PcraftBase";
    public virtual double Fps => 30.0;
    public static (int w, int h) Resolution => (128, 128);

    protected Pico8Functions? p8;

    protected List<Entity>? anvilRecipe;

    protected F32 banim;

    protected bool canSwitchLevel = false;
    protected Level? cave;
    protected List<Entity>? chemRecipe;
    protected F32 clx;
    protected F32 cly;
    protected F32 cmx;
    protected F32 cmy;
    protected F32 coffx;
    protected F32 coffy;
    protected Entity? curItem;
    protected Entity? curMenu;
    protected Level? currentLevel;

    protected F32[] data = new F32[8192];

    protected List<Entity> enemies = [];
    protected List<Entity> entities = [];

    protected List<Entity>? factoryRecipe;

    protected List<Entity>? furnaceRecipe;

    protected int holex;
    protected int holey;

    protected List<Entity> invent = [];
    protected Level? island;

    protected F32[][]? level;
    protected int levelsx;
    protected int levelsy;
    protected int levelx;
    protected int levely;
    protected bool levelUnder = false;
    protected F32 llife;
    protected F32 lrot;
    protected F32 lstam;

    protected Entity? menuInvent;

    protected List<Entity>? nearEnemies;

    protected F32 panim;

    protected F32 plife;
    protected F32 plx;
    protected F32 ply;
    protected F32 prot;
    protected F32 pstam;

    protected F32[][] Rndwat = new F32[16][];

    protected int stamCost;
    protected List<Entity>? stonebenchRecipe;
    protected bool switchLevel = false;

    protected F32 time;
    protected int toogleMenu;
    protected int[] typeCount = new int[11];

    protected List<Entity>? workbenchRecipe;

    //p.craft
    //by nusan

    protected bool lb4 = false;
    protected bool lb5 = false;
    protected bool block5 = false;

    protected readonly int enstep_Wait = 0;
    protected readonly int enstep_Walk = 1;
    protected readonly int enstep_Chase = 2;
    protected readonly int enstep_Patrol = 3;

    protected static readonly string[] pwrNames = ["wood", "stone", "iron", "gold", "gem"];
    protected static readonly int[][] pwrPal = [[2, 2, 4, 4], [5, 2, 4, 13], [13, 5, 13, 6], [9, 2, 9, 10], [13, 2, 14, 12]];

    protected static readonly Material haxe = Item("haxe", 98);
    protected static readonly Material sword = Item("sword", 99);
    protected static readonly Material scythe = Item("scythe", 100);
    protected static readonly Material shovel = Item("shovel", 101);
    protected static readonly Material pick = Item("pick", 102);

    protected static readonly int[] pstone = [0, 1, 5, 13];
    protected static readonly int[] piron = [1, 5, 13, 6];
    protected static readonly int[] pgold = [1, 9, 10, 7];

    protected static readonly Material wood = Item("wood", 103);
    protected static readonly Material sand = Item("sand", 114, [15]);
    protected static readonly Material seed = Item("seed", 115);
    protected static readonly Material wheat = Item("wheat", 118, [4, 9, 10, 9]);
    protected static readonly Material apple = Item("apple", 116);

    protected static readonly Material glass = Item("glass", 117);
    protected static readonly Material stone = Item("stone", 118, pstone);
    protected static readonly Material iron = Item("iron", 118, piron);
    protected static readonly Material gold = Item("gold", 118, pgold);
    protected static readonly Material gem = Item("gem", 118, [1, 2, 14, 12]);

    protected static readonly Material fabric = Item("fabric", 69);
    protected static readonly Material sail = Item("sail", 70);
    protected static readonly Material glue = Item("glue", 85, [1, 13, 12, 7]);
    protected static readonly Material boat = Item("boat", 86);
    protected static readonly Material ichor = Item("ichor", 114, [11]);
    protected static readonly Material potion = Item("potion", 85, [1, 2, 8, 14]);

    protected static readonly Material ironbar = Item("iron bar", 119, piron);
    protected static readonly Material goldbar = Item("gold bar", 119, pgold);
    protected static readonly Material bread = Item("bread", 119, [1, 4, 15, 7]);

    protected static readonly Material workbench = BigSpr(104, Item("workbench", 89, [1, 4, 9], true));
    protected static readonly Material stonebench = BigSpr(104, Item("stonebench", 89, [1, 6, 13], true));
    protected static readonly Material furnace = BigSpr(106, Item("furnace", 90, null, true));
    protected static readonly Material anvil = BigSpr(108, Item("anvil", 91, null, true));
    protected static readonly Material factory = BigSpr(71, Item("factory", 74, null, true));
    protected static readonly Material chem = BigSpr(78, Item("chem lab", 76, null, true));
    protected static readonly Material chest = BigSpr(110, Item("chest", 92));

    protected static readonly Material inventary = Item("inventory", 89);
    protected static readonly Material pickuptool = Item("pickup tool", 73);

    protected static readonly Material etext = Item("text", 103);
    protected static readonly Material player = Item(null, 1);
    protected static readonly Material zombi = Item(null, 2);

    protected static readonly Ground grwater = new() { Id = 0, Gr = 0 };
    protected static readonly Ground grsand = new() { Id = 1, Gr = 1 };
    protected static readonly Ground grgrass = new() { Id = 2, Gr = 2 };
    protected static readonly Ground grrock = new() { Id = 3, Gr = 3, Mat = stone, Tile = grsand, Life = 15 };
    protected static readonly Ground grtree = new() { Id = 4, Gr = 2, Mat = wood, Tile = grgrass, Life = 8, IsTree = true, Pal = [1, 5, 3, 11] };
    protected static readonly Ground grfarm = new() { Id = 5, Gr = 1 };
    protected static readonly Ground grwheat = new() { Id = 6, Gr = 1 };
    protected static readonly Ground grplant = new() { Id = 7, Gr = 2 };
    protected static readonly Ground griron = new() { Id = 8, Gr = 1, Mat = iron, Tile = grsand, Life = 45, IsTree = true, Pal = [1, 1, 13, 6] };
    protected static readonly Ground grgold = new() { Id = 9, Gr = 1, Mat = gold, Tile = grsand, Life = 80, IsTree = true, Pal = [1, 2, 9, 10] };
    protected static readonly Ground grgem = new() { Id = 10, Gr = 1, Mat = gem, Tile = grsand, Life = 160, IsTree = true, Pal = [1, 2, 14, 12] };
    protected static readonly Ground grhole = new() { Id = 11, Gr = 1 };

    protected Ground lastGround = grsand;

    protected readonly Ground[] grounds = { grwater, grsand, grgrass, grrock, grtree, grfarm, grwheat, grplant, griron, grgold, grgem, grhole };

    protected Entity mainMenu = Cmenu(inventary, null, 128, "by nusan", "2016");
    protected Entity introMenu = Cmenu(inventary, null, 136, "a storm leaved you", "on a deserted island");
    protected Entity deathMenu = Cmenu(inventary, null, 128, "you died", "alone ...");
    protected Entity winMenu = Cmenu(inventary, null, 136, "you successfully escaped", "from the island");

    static PcraftBase()
    {
        apple.GiveLife = 20;

        potion.GiveLife = 100;

        bread.GiveLife = 40;
    }

    protected static Material Item(string n, int s, int[] p = null, bool bc = false)
    {
        return new() { Name = n, Spr = s, Pal = p, BeCraft = bc };
    }

    protected virtual Entity Inst(Material it)
    {
        return new() { Type = it };
    }

    protected virtual Entity Instc(Material it, int? c = null, List<Entity> l = null)
    {
        return new() { Type = it, Count = c, List = l };
    }

    protected virtual Entity SetPower(int? v, Entity i)
    {
        i.Power = v;
        return i;
    }

    protected virtual Entity Entity(Material it, F32 xx, F32 yy, F32 vxx, F32 vyy)
    {
        return new() { Type = it, X = xx, Y = yy, Vx = vxx, Vy = vyy };
    }

    protected virtual Entity Rentity(Material it, F32 xx, F32 yy)
    {
        return Entity(it, xx, yy, p8.Rnd(3) - F32.FromDouble(1.5), p8.Rnd(3) - F32.FromDouble(1.5));
    }

    protected virtual Entity SetText(string t, int c, F32 time, Entity e)
    {
        e.Text = t;
        e.Timer = time;
        e.C = c;
        return e;
    }

    protected static Material BigSpr(int spr, Material ent)
    {
        ent.BigSpr = spr;
        ent.Drop = true;
        return ent;
    }

    protected virtual Entity Recipe(Entity m, List<Entity> require)
    {
        return new() { Type = m.Type, Power = m.Power, Count = m.Count, Req = require, List = m.List };
    }

    protected virtual bool CanCraft(Entity req)
    {
        bool can = true;
        foreach (Entity e in req.Req)
        {
            if (HowMany(invent, e) < e.Count)
            {
                can = false;
                break;
            }
        }
        return can;
    }

    protected virtual void Craft(Entity req)
    {
        foreach (Entity e in req.Req)
        {
            RemInList(invent, e);
        }
        AddItemInList(invent, SetPower(req.Power, Instc(req.Type, req.Count, req.List)), -1);
    }

    protected virtual void SetPal(int[] l)
    {
        for (int i = 0; i < l.Length; i++)
        {
            p8.Pal(i + 1, l[i]);
        }
    }

    protected static Entity Cmenu(Material t, List<Entity> l = null, int? s = null, string te1 = null, string te2 = null)
    {
        return new() { List = l, Type = t, Sel = 0, Off = 0, Spr = s, Text = te1, Text2 = te2 };
    }

    protected virtual int HowMany(List<Entity> list, Entity it)
    {
        int count = 0;
        foreach (Entity e in list)
        {
            if (e.Type != it.Type)
            {
                continue;
            }
            if (it.Power is null || it.Power == e.Power)
            {
                if (e.Count is not null)
                {
                    count += (int)e.Count;
                }
                else
                {
                    count += 1;
                }
            }
        }
        return count;
    }

    protected virtual Entity IsInList(List<Entity> list, Entity it)
    {
        foreach (Entity e in list)
        {
            if (e.Type != it.Type)
            {
                continue;
            }
            if (it.Power is null || it.Power == e.Power)
            {
                return e;
            }
        }
        return new();
    }

    protected virtual void RemInList(List<Entity> list, Entity elem)
    {
        Entity it = IsInList(list, elem);
        if (it is null)
        {
            return;
        }
        if (it.Count is not null)
        {
            it.Count -= elem.Count;
            if (it.Count <= 0)
            {
                p8.Del(list, it);
            }
        }
        else
        {
            p8.Del(list, it);
        }
    }

    protected virtual void AddItemInList(List<Entity> list, Entity it, int p)
    {
        Entity it2 = IsInList(list, it);
        if (it2 is null || it2.Count is null)
        {
            AddPlace(list, it, p);
        }
        else
        {
            it2.Count += it.Count;
        }
    }

    protected virtual void AddPlace(List<Entity> l, Entity e, int p)
    {
        if (p < l.Count - 1 && p >= 0)
        {
            l.Insert(p, e);
        }
        else
        {
            p8.Add(l, e);
        }
    }

    protected virtual bool IsIn(Entity e, int size)
    {
        return e.X > clx - size && e.X < clx + size && e.Y > cly - size && e.Y < cly + size;
    }

    protected virtual F32 GetInvLen(F32 x, F32 y)
    {
        return 1 / GetLen(x, y);
    }

    protected virtual F32 GetLen(F32 x, F32 y)
    {
        return F32.Sqrt(x * x + y * y + F32.FromDouble(0.001));
    }

    protected virtual F32 GetRot(F32 dx, F32 dy)
    {
        return dy >= 0 ? (dx + 3) * F32.FromDouble(0.25) : (1 - dx) * F32.FromDouble(0.25);
    }

    protected virtual void FillEne(Level l)
    {
        l.Ene = [Entity(player, F32.Zero, F32.Zero, F32.Zero, F32.Zero)];
        enemies = l.Ene;
        for (F32 i = F32.Zero; i < levelsx; i++)
        {
            for (F32 j = F32.Zero; j < levelsy; j++)
            {
                Ground c = GetDirectGr(i, j);
                F32 r = p8.Rnd(100);
                F32 ex = i * 16 + 8;
                F32 ey = j * 16 + 8;
                F32 dist = F32.Max(F32.Abs(ex - plx), F32.Abs(ey - ply));
                if (r < 3 && c != grwater && c != grrock && !c.IsTree && dist > 50)
                {
                    Entity newe = Entity(zombi, ex, ey, F32.Zero, F32.Zero);
                    newe.Life = F32.FromInt(10);
                    newe.Prot = F32.Zero;
                    newe.Lrot = F32.Zero;
                    newe.Panim = F32.Zero;
                    newe.Banim = F32.Zero;
                    newe.Dtim = F32.Zero;
                    newe.Step = 0;
                    newe.Ox = F32.Zero;
                    newe.Oy = F32.Zero;
                    p8.Add(l.Ene, newe);
                }
            }
        }
    }

    protected virtual Level CreateLevel(int xx, int yy, int sizex, int sizey, bool IsUnderground)
    {
        Level l = new() { X = xx, Y = yy, Sx = sizex, Sy = sizey, IsUnder = IsUnderground, Ent = [], Ene = [], Dat = new F32[8192] };
        SetLevel(l);
        levelUnder = IsUnderground;
        CreateMap();
        FillEne(l);
        l.Stx = F32.FromInt((holex - levelx) * 16 + 8);
        l.Sty = F32.FromInt((holey - levely) * 16 + 8);
        return l;
    }

    protected virtual void SetLevel(Level l)
    {
        currentLevel = l;
        levelx = l.X;
        levely = l.Y;
        levelsx = l.Sx;
        levelsy = l.Sy;
        levelUnder = l.IsUnder;
        entities = l.Ent;
        enemies = l.Ene;
        data = l.Dat;
        plx = l.Stx;
        ply = l.Sty;
    }

    protected virtual void ResetLevel()
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

        cave = CreateLevel(64, 0, 32, 32, true);
        island = CreateLevel(0, 0, 64, 64, false);

        Entity tmpworkbench = Entity(workbench, plx, ply, F32.Zero, F32.Zero);
        tmpworkbench.HasCol = true;
        tmpworkbench.List = workbenchRecipe;

        p8.Add(invent, tmpworkbench);
        p8.Add(invent, Inst(pickuptool));
    }

    public virtual void Init(Pico8Functions pico8)
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
        int[] quant = [5, 5, 7, 7, 7];
        int[] pows = [1, 2, 3, 4, 5];
        Material[] materials = [wood, stone, ironbar, goldbar, gem];
        int[] mult = [1, 1, 1, 1, 3];
        List<Entity>[] crafter = [workbenchRecipe, stonebenchRecipe, anvilRecipe, anvilRecipe, anvilRecipe];
        for (int j = 0; j < pows.Length; j++)
        {
            for (int i = 0; i < tooltypes.Length; i++)
            {
                p8.Add(crafter[j], Recipe(SetPower(pows[j], Instc(tooltypes[i])), [Instc(materials[j], quant[i] * mult[j])]));
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

    protected virtual (int x, int y) GetMcoord(F32 x, F32 y)
    {
        return (F32.FloorToInt(x / 16), F32.FloorToInt(y / 16));
    }

    protected virtual bool IsFree(F32 x, F32 y, Entity e = null)
    {
        Ground gr = GetGr(x, y);
        return !(gr.IsTree || gr == grrock);
    }

    protected virtual bool IsFreeEnem(F32 x, F32 y, Entity e = null)
    {
        Ground gr = GetGr(x, y);
        return !(gr.IsTree || gr == grrock || gr == grwater);
    }

    protected virtual Ground GetGr(F32 x, F32 y)
    {
        (int i, int j) = GetMcoord(x, y);
        return GetDirectGr(F32.FromInt(i), F32.FromInt(j));
    }

    protected virtual Ground GetDirectGr(F32 i, F32 j)
    {
        if (i < 0 || j < 0 || i >= levelsx || j >= levelsy) { return grounds[0]; }
        return grounds[p8.Mget(i.Double + levelx, j.Double)];
    }

    protected virtual void SetGr(F32 x, F32 y, Ground v)
    {
        (int i, int j) = GetMcoord(x, y);
        if (i < 0 || j < 0 || i >= levelsx || j >= levelsy) { return; }
        p8.Mset(i + levelx, j, v.Id);
    }

    protected virtual F32 DirGetData(F32 i, F32 j, F32 @default)
    {
        int g = F32.FloorToInt(i + j * levelsx);
        if (data[g - 1] == 0)
        {
            data[g - 1] = @default;
        }
        return data[g - 1];
    }

    protected virtual F32 GetData(F32 x, F32 y, int @default)
    {
        (int i, int j) = GetMcoord(x, y);
        if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
        {
            return F32.FromInt(@default);
        }
        return DirGetData(F32.FromInt(i), F32.FromInt(j), F32.FromInt(@default));
    }

    protected virtual void SetData(F32 x, F32 y, F32 v)
    {
        (int i, int j) = GetMcoord(x, y);
        if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
        {
            return;
        }
        data[i + j * levelsx - 1] = v;
    }

    protected virtual void Cleardata(F32 x, F32 y)
    {
        (int i, int j) = GetMcoord(x, y);
        if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
        {
            return;
        }
        data[i + j * levelsx - 1] = F32.Zero;
    }

    protected virtual int Loop(int sel, List<Entity> l)
    {
        int lp = l.Count;
        return (sel % lp + lp) % lp;
    }

    protected virtual bool EntColFree(F32 x, F32 y, Entity e)
    {
        return F32.Max(F32.Abs(e.X - x), F32.Abs(e.Y - y)) > 8;
    }

    protected virtual (F32 dx, F32 dy) ReflectCol(F32 x, F32 y, F32 dx, F32 dy, Func<F32, F32, Entity, bool> checkfun, F32 dp, Entity e = null)
    {
        F32 newx = x + dx;
        F32 newy = y + dy;

        bool ccur = checkfun(x, y, e);
        bool ctotal = checkfun(newx, newy, e);
        bool chor = checkfun(newx, y, e);
        bool cver = checkfun(x, newy, e);

        if (!ccur)
        {
            return (dx, dy);
        }
        if (chor || cver)
        {
            if (!ctotal)
            {
                if (chor)
                {
                    dy = -dy * dp;
                }
                else
                {
                    dx = -dx * dp;
                }
            }
        }
        else
        {
            dx = -dx * dp;
            dy = -dy * dp;
        }

        return (dx, dy);
    }

    protected virtual void AddItem(Material mat, int count, F32 hitx, F32 hity)
    {
        for (int i = 0; i < count; i++)
        {
            Entity gi = Rentity(mat, F32.Floor(hitx / 16) * 16 + p8.Rnd(14) + 1, F32.Floor(hity / 16) * 16 + p8.Rnd(14) + 1);
            gi.GiveItem = mat;
            gi.HasCol = true;
            gi.Timer = 110 + p8.Rnd(20);
            p8.Add(entities, gi);
        }
    }

    protected virtual void UpGround()
    {
        F32 ci = F32.Floor((clx - 64) / 16);
        F32 cj = F32.Floor((cly - 64) / 16);
        for (F32 i = ci; i < ci + 8; i++)
        {
            for (F32 j = cj; j < cj + 8; j++)
            {
                Ground gr = GetDirectGr(i, j);
                if (gr == grfarm)
                {
                    F32 d = DirGetData(i, j, F32.Zero);
                    if (time > d)
                    {
                        p8.Mset(i.Double + levelx, j.Double, grsand.Id);
                    }
                }
            }
        }
    }

    protected virtual F32 UpRot(F32 grot, F32 rot)
    {
        if (F32.Abs(rot - grot) > F32.Half)
        {
            if (rot > grot)
            {
                grot += 1;
            }
            else
            {
                grot -= 1;
            }
        }

        return Pico8Functions.Mod(Pico8Functions.Mod(F32.Lerp(rot, grot, F32.FromDouble(0.4)), 1) + 1, 1);
    }

    protected virtual (F32 dx, F32 dy, bool canAct) UpEntity(F32 dx, F32 dy, bool canAct)
    {
        int fin = entities.Count;
        for (int i = fin - 1; i >= 0; i--)
        {
            Entity e = entities[i];
            if (e.HasCol)
            {
                (e.Vx, e.Vy) = ReflectCol(e.X, e.Y, e.Vx, e.Vy, IsFree, F32.FromDouble(0.9));
            }
            e.X += e.Vx;
            e.Y += e.Vy;
            e.Vx *= F32.FromDouble(0.95);
            e.Vy *= F32.FromDouble(0.95);

            if (e.Timer is not null && e.Timer < 1)
            {
                p8.Del(entities, e);
                continue;
            }

            if (e.Timer is not null) { e.Timer -= 1; }

            F32 dist = F32.Max(F32.Abs(e.X - plx), F32.Abs(e.Y - ply));
            if (e.GiveItem is not null)
            {
                if (dist < 5 && (e.Timer is null || e.Timer < 115))
                {
                    Entity newIt = Instc(e.GiveItem, 1);
                    AddItemInList(invent, newIt, -1);
                    p8.Del(entities, e);
                    p8.Add(entities, SetText(HowMany(invent, newIt).ToString(), 11, F32.FromInt(20), Entity(etext, e.X, e.Y - 5, F32.Zero, F32.Neg1)));
                    p8.Sfx(18, 3);
                }
                continue;
            }

            if (e.HasCol)
            {
                (dx, dy) = ReflectCol(plx, ply, dx, dy, EntColFree, F32.Zero, e);
            }
            if (dist < 12 && p8.Btn(5) && !block5 && !lb5)
            {
                if (curItem is not null && curItem.Type == pickuptool)
                {
                    if (e.Type == chest || e.Type.BeCraft)
                    {
                        AddItemInList(invent, e, -1);
                        curItem = e;
                        p8.Del(entities, e);
                    }
                    canAct = false;
                    continue;
                }

                if (e.Type == chest || e.Type.BeCraft)
                {
                    toogleMenu = 0;
                    curMenu = Cmenu(e.Type, e.List);
                    p8.Sfx(13, 3);
                }
                canAct = false;
            }
        }
        return (dx, dy, canAct);
    }

    protected virtual void UpEnemies(F32 ebx, F32 eby)
    {
        foreach (Entity e in enemies)
        {
            if (!IsIn(e, 100))
            {
                continue;
            }
            if (e.Type == player)
            {
                e.X = plx;
                e.Y = ply;
                continue;
            }

            F32 distp = GetLen(e.X - plx, e.Y - ply);
            F32 mspeed = F32.FromDouble(0.8);

            F32 disten = GetLen(e.X - plx - ebx * 8, e.Y - ply - eby * 8);
            if (disten < 10)
            {
                p8.Add(nearEnemies, e);
            }
            if (distp < 8)
            {
                e.Ox += F32.Max(F32.FromDouble(-0.4), F32.Min(F32.FromDouble(0.4), e.X - plx));
                e.Oy += F32.Max(F32.FromDouble(-0.4), F32.Min(F32.FromDouble(0.4), e.Y - ply));
            }

            if (e.Dtim <= 0)
            {
                if (e.Step == enstep_Wait || e.Step == enstep_Patrol)
                {
                    e.Step = enstep_Walk;
                    e.Dx = p8.Rnd(2) - 1;
                    e.Dy = p8.Rnd(2) - 1;
                    e.Dtim = 30 + p8.Rnd(60);
                }
                else if (e.Step == enstep_Walk)
                {
                    e.Step = enstep_Wait;
                    e.Dx = F32.Zero;
                    e.Dy = F32.Zero;
                    e.Dtim = 30 + p8.Rnd(60);
                }
                else // chase
                {
                    e.Dtim = 10 + p8.Rnd(60);
                }
            }
            else
            {
                if (e.Step == enstep_Chase)
                {
                    if (distp > 10)
                    {
                        e.Dx += plx - e.X;
                        e.Dy += ply - e.Y;
                        e.Banim = F32.Zero;
                    }
                    else
                    {
                        e.Dx = F32.Zero;
                        e.Dy = F32.Zero;
                        e.Banim -= 1;
                        e.Banim = Pico8Functions.Mod(e.Banim, 8);
                        int pow = 10;
                        if (e.Banim == 4)
                        {
                            plife -= pow;
                            p8.Add(entities, SetText(pow.ToString(), 8, F32.FromInt(20), Entity(etext, plx, ply - 10, F32.Zero, F32.Neg1)));
                            p8.Sfx(14 + p8.Rnd(2).Double, 3);
                        }
                        plife = F32.Max(F32.Zero, plife);
                    }
                    mspeed = F32.FromDouble(1.4);
                    if (distp > 70)
                    {
                        e.Step = enstep_Patrol;
                        e.Dtim = 30 + p8.Rnd(60);
                    }
                }
                else
                {
                    if (distp < 40)
                    {
                        e.Step = enstep_Chase;
                        e.Dtim = 10 + p8.Rnd(60);
                    }
                }
                e.Dtim -= 1;
            }

            F32 dl = mspeed * GetInvLen(e.Dx, e.Dy);
            e.Dx *= dl;
            e.Dy *= dl;

            F32 fx = e.Dx + e.Ox;
            F32 fy = e.Dy + e.Oy;
            (fx, fy) = ReflectCol(e.X, e.Y, fx, fy, IsFreeEnem, F32.Zero);

            if (F32.Abs(e.Dx) > 0 || F32.Abs(e.Dy) > 0)
            {
                e.Lrot = GetRot(e.Dx, e.Dy);
                e.Panim += F32.FromDouble(1.0 / 33.0);
            }
            else
            {
                e.Panim = F32.Zero;
            }

            e.X += fx;
            e.Y += fy;

            e.Ox *= F32.FromDouble(0.9);
            e.Oy *= F32.FromDouble(0.9);

            e.Prot = UpRot(e.Lrot, e.Prot);
        }
    }

    protected virtual void UpHit(F32 hitx, F32 hity, Ground hit)
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

    public virtual void Update()
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

    protected virtual (int mx, int my) Mirror(F32 rot)
    {
        switch (rot)
        {
            case F32 r when r < F32.FromDouble(0.125):
                return (0, 1);
            case F32 r when r < F32.FromDouble(0.325):
                return (0, 0);
            case F32 r when r < F32.FromDouble(0.625):
                return (1, 0);
            case F32 r when r < F32.FromDouble(0.825):
                return (1, 1);
            default:
                return (0, 1);

        }
    }

    protected virtual void Dplayer(F32 x, F32 y, F32 rot, F32 anim, F32 subanim, bool isplayer)
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

    protected virtual F32[][] Noise(int sx, int sy, F32 startscale, F32 scalemod, int featstep)
    {
        F32[][] n = new F32[sx + 1][];

        for (int i = 0; i <= sx; i++)
        {
            n[i] = new F32[sy + 1];
            for (int j = 0; j <= sy; j++)
            {
                n[i][j] = F32.Half;
            }
        }

        int step = sx;
        F32 scale = startscale;

        while (step > 1)
        {
            F32 cscal = scale;
            if (step == featstep) { cscal = F32.One; }

            for (int i = 0; i < sx; i += step)
            {
                for (int j = 0; j < sy; j += step)
                {
                    F32 c1 = n[i][j];
                    F32 c2 = n[i + step][j];
                    F32 c3 = n[i][j + step];
                    n[i + step / 2][j] = (c1 + c2) * F32.Half + (p8.Rnd(1) - F32.Half) * cscal;
                    n[i][j + step / 2] = (c1 + c3) * F32.Half + (p8.Rnd(1) - F32.Half) * cscal;
                }
            }

            for (int i = 0; i < sx; i += step)
            {
                for (int j = 0; j < sy; j += step)
                {
                    F32 c1 = n[i][j];
                    F32 c2 = n[i + step][j];
                    F32 c3 = n[i][j + step];
                    F32 c4 = n[i + step][j + step];
                    n[i + step / 2][j + step / 2] = (c1 + c2 + c3 + c4) * F32.FromDouble(0.25) + (p8.Rnd(1) - F32.Half) * cscal;
                }
            }

            step /= 2;
            scale *= scalemod;
        }

        return n;
    }

    protected virtual F32[][] CreateMapStep(int sx, int sy, int a, int b, int c, int d, int e)
    {
        F32[][] cur = Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.2), sx);
        F32[][] cur2 = Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.4), 8);
        F32[][] cur3 = Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.3), 8);
        F32[][] cur4 = Noise(sx, sy, F32.FromDouble(0.8), F32.FromDouble(1.1), 4);

        for (int i = 0; i < 11; i++)
        {
            typeCount[i] = 0;
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

                typeCount[id] += 1;

                cur[i][j] = F32.FromInt(id);
            }
        }

        return cur;
    }

    protected virtual void CreateMap()
    {
        bool needmap = true;

        while (needmap)
        {
            needmap = false;

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
            }

            if (!needmap)
            {
                plx = F32.Neg1;
                ply = F32.Neg1;

                for (int i = 0; i <= 500; i++)
                {
                    int depx = F32.FloorToInt(levelsx / 8 + p8.Rnd(levelsx * 6 / 8));
                    int depy = F32.FloorToInt(levelsy / 8 + p8.Rnd(levelsy * 6 / 8));
                    F32 c = level[depx][depy];

                    if (c == 1 || c == 2)
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

    protected virtual bool Comp(F32 i, F32 j, Ground gr)
    {
        Ground gr2 = GetDirectGr(i, j);
        return gr is not null && gr2 is not null && gr.Gr == gr2.Gr;
    }

    protected virtual F32 WatVal(F32 i, F32 j)
    {
        return Rndwat[F32.FloorToInt(F32.Abs(i * 2) % 16)][F32.FloorToInt(F32.Abs(j * 2) % 16)];
    }

    protected virtual void WatAnim(F32 i, F32 j)
    {
        F32 a = (time * F32.FromDouble(0.6) + WatVal(i, j) / 100) % 1 * 19;
        if (a > 16) { p8.Spr(13 + a.Double - 16, i.Double * 16, j.Double * 16); }
    }

    protected virtual F32 RndCenter(F32 i, F32 j)
    {
        return (F32.Floor(WatVal(i, j) / 34) + 18) % 20;
    }

    protected virtual int RndSand(F32 i, F32 j)
    {
        return F32.FloorToInt(WatVal(i, j) / 34) + 1;
    }

    protected virtual int RndTree(F32 i, F32 j)
    {
        return F32.FloorToInt(WatVal(i, j) / 51) * 32;
    }

    protected virtual void Spr4(F32 i, F32 j, F32 gi, F32 gj, int a, int b, int c, int d, int off, Func<F32, F32, int> f)
    {
        p8.Spr(f(i, j + off) + a, gi.Double, (gj + 2 * off).Double);
        p8.Spr(f(i + F32.Half, j + off) + b, gi.Double + 8, (gj + 2 * off).Double);
        p8.Spr(f(i, j + F32.Half + off) + c, gi.Double, (gj + 8 + 2 * off).Double);
        p8.Spr(f(i + F32.Half, j + F32.Half + off) + d, gi.Double + 8, (gj + 8 + 2 * off).Double);
    }

    protected virtual void DrawBack()
    {
        F32 ci = F32.Floor((clx - 64) / 16);
        F32 cj = F32.Floor((cly - 64) / 16);

        for (F32 i = ci; i <= ci + 8; i++)
        {
            for (F32 j = cj; j <= cj + 8; j++)
            {
                Ground gr = GetDirectGr(i, j);

                int gi = F32.FloorToInt(i - ci) * 2 + 64;
                int gj = F32.FloorToInt(j - cj) * 2 + 32;

                if (gr is not null && gr.Gr == 1) // sand
                {
                    int sv = 0;
                    if (gr == grfarm || gr == grwheat) { sv = 3; }
                    p8.Mset(gi, gj, RndSand(i, j) + sv);
                    p8.Mset(gi + 1, gj, RndSand(i + F32.Half, j) + sv);
                    p8.Mset(gi, gj + 1, RndSand(i, j + F32.Half) + sv);
                    p8.Mset(gi + 1, gj + 1, RndSand(i + F32.Half, j + F32.Half) + sv);
                }
                else
                {
                    bool u = Comp(i, j - 1, gr);
                    bool d = Comp(i, j + 1, gr);
                    bool l = Comp(i - 1, j, gr);
                    bool r = Comp(i + 1, j, gr);

                    int b = gr == grrock ? 21 : gr == grwater ? 26 : 16;

                    p8.Mset(gi, gj, b + (l ? u ? Comp(i - 1, j - 1, gr) ? 17 + RndCenter(i, j).Double : 20 : 1 : u ? 16 : 0));
                    p8.Mset(gi + 1, gj, b + (r ? u ? Comp(i + 1, j - 1, gr) ? 17 + RndCenter(i + F32.Half, j).Double : 19 : 1 : u ? 18 : 2));
                    p8.Mset(gi, gj + 1, b + (l ? d ? Comp(i - 1, j + 1, gr) ? 17 + RndCenter(i, j + F32.Half).Double : 4 : 33 : d ? 16 : 32));
                    p8.Mset(gi + 1, gj + 1, b + (r ? d ? Comp(i + 1, j + 1, gr) ? 17 + RndCenter(i + F32.Half, j + F32.Half).Double : 3 : 33 : d ? 18 : 34));

                }
            }
        }

        p8.Pal();
        if (levelUnder)
        {
            p8.Pal(15, 5);
            p8.Pal(4, 1);
        }
        p8.Map(64, 32, ci.Double * 16, cj.Double * 16, 18, 18);

        for (F32 i = ci - 1; i <= ci + 8; i++)
        {
            for (F32 j = cj - 1; j <= cj + 8; j++)
            {
                Ground gr = GetDirectGr(i, j);
                if (gr is null)
                {
                    continue;
                }

                F32 gi = i * 16;
                F32 gj = j * 16;

                p8.Pal();

                if (gr == grwater)
                {
                    WatAnim(i, j);
                    WatAnim(i + F32.Half, j);
                    WatAnim(i, j + F32.Half);
                    WatAnim(i + F32.Half, j + F32.Half);
                }

                if (gr == grwheat)
                {
                    F32 d = DirGetData(i, j, F32.Zero) - time;
                    for (int pp = 2; pp <= 4; pp++)
                    {
                        p8.Pal(pp, 3);
                        if (d > 10 - pp * 2) { p8.Palt(pp, true); }
                    }
                    if (d < 0) { p8.Pal(4, 9); }
                    Spr4(i, j, gi, gj, 6, 6, 6, 6, 0, RndSand);
                }

                if (gr.IsTree)
                {
                    SetPal(gr.Pal);

                    Spr4(i, j, gi, gj, 64, 65, 80, 81, 0, RndTree);
                }

                if (gr == grhole)
                {
                    p8.Pal();
                    if (!levelUnder)
                    {
                        p8.Palt(0, false);
                        p8.Spr(31, gi.Double, gj.Double, 1, 2);
                        p8.Spr(31, gi.Double + 8, gj.Double, 1, 2, true);
                    }
                    p8.Palt();
                    p8.Spr(77, gi.Double + 4, gj.Double, 1, 2);
                }
            }
        }
    }

    protected virtual void Panel(string name, int x, int y, int sx, int sy)
    {
        p8.Rectfill(x + 8, y + 8, x + sx - 9, y + sy - 9, 1);
        p8.Spr(66, x, y);
        p8.Spr(67, x + sx - 8, y);
        p8.Spr(82, x, y + sy - 8);
        p8.Spr(83, x + sx - 8, y + sy - 8);
        p8.Sspr(24, 32, 4, 8, x + 8, y, sx - 16, 8);
        p8.Sspr(24, 40, 4, 8, x + 8, y + sy - 8, sx - 16, 8);
        p8.Sspr(16, 36, 8, 4, x, y + 8, 8, sy - 16);
        p8.Sspr(24, 36, 8, 4, x + sx - 8, y + 8, 8, sy - 16);

        int hx = x + (sx - name.Length * 4) / 2;
        p8.Rectfill(hx, y + 1, hx + name.Length * 4, y + 7, 13);
        p8.Print(name, hx + 1, y + 2, 7);
    }

    protected virtual void ItemName(int x, int y, Entity it, int col)
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

    protected virtual void List(Entity menu, int x, int y, int sx, int sy, int my)
    {
        Panel(menu.Type.Name, x, y, sx, sy);

        int tlist = menu.List.Count;
        if (tlist < 1)
        {
            return;
        }

        int sel = menu.Sel;
        if (menu.Off > Math.Max(0, sel - 4)) { menu.Off = Math.Max(0, sel - 4); }
        if (menu.Off < Math.Min(tlist, sel + 3) - my) { menu.Off = Math.Min(tlist, sel + 3) - my; }

        sel -= menu.Off;

        int debut = menu.Off + 1;
        int fin = Math.Min(menu.Off + my, tlist);

        int sely = y + 3 + (sel + 1) * 8;
        p8.Rectfill(x + 1, sely, x + sx - 3, sely + 6, 13);

        x += 5;
        y += 12;

        for (int i = debut - 1; i < fin; i++)
        {
            Entity it = menu.List[i];
            int py = y + (i - menu.Off) * 8;
            int col = 7;
            if (it.Req is not null && !CanCraft(it))
            {
                col = 0;
            }

            ItemName(x, py, it, col);

            if (it.Count is not null)
            {
                string c = $"{it.Count}";
                p8.Print(c, x + sx - c.Length * 4 - 10, py, col);
            }
        }

        p8.Spr(68, x - 8, sely);
        p8.Spr(68, x + sx - 10, sely, 1, 1, true);
    }

    protected virtual void RequireList(Entity recip, int x, int y, int sx, int sy)
    {
        Panel("require", x, y, sx, sy);
        int tlist = recip.Req.Count;
        if (tlist < 1)
        {
            return;
        }

        x += 5;
        y += 12;

        for (int i = 0; i < tlist; i++)
        {
            Entity it = recip.Req[i];
            int py = y + i * 8;
            ItemName(x, py, it, 7);

            if (it.Count is not null)
            {
                int h = HowMany(invent, it);
                string c = $"{h}/{it.Count}";
                p8.Print(c, x + sx - c.Length * 4 - 10, py, h < it.Count ? 8 : 7);
            }
        }
    }

    protected virtual void Printb(string t, double x, double y, int c)
    {
        p8.Print(t, x + 1, y, 1);
        p8.Print(t, x - 1, y, 1);
        p8.Print(t, x, y + 1, 1);
        p8.Print(t, x, y - 1, 1);
        p8.Print(t, x, y, c);
    }

    protected virtual void Printc(string t, int x, int y, int c)
    {
        p8.Print(t, x - t.Length * 2, y, c);
    }

    protected virtual void Dent()
    {
        foreach (Entity e in entities)
        {
            p8.Pal();
            if (e.Type.Pal is not null) { SetPal(e.Type.Pal); }
            if (e.Type.BigSpr is not null)
            {
                p8.Spr((int)e.Type.BigSpr, e.X.Double - 8, e.Y.Double - 8, 2, 2);
                continue;
            }

            if (e.Type == etext)
            {
                Printb(e.Text, e.X.Double - 2, e.Y.Double - 4, e.C);
                continue;
            }

            if (e.Timer is not null && e.Timer < 45 && e.Timer % 4 > 2)
            {
                for (int i = 0; i <= 15; i++)
                {
                    p8.Palt(i, true);
                }
            }
            p8.Spr(e.Type.Spr, e.X.Double - 4, e.Y.Double - 4);
        }
    }

    protected virtual void Sorty(List<Entity> t)
    {
        int tv = t.Count - 1;
        for (int i = 0; i < tv; i++)
        {
            Entity t1 = t[i];
            Entity t2 = t[i + 1];
            if (t1.Y > t2.Y)
            {
                t[i] = t2;
                t[i + 1] = t1;
            }
        }
    }

    protected virtual void Denemies()
    {
        Sorty(enemies);

        foreach (Entity e in enemies)
        {
            if (e.Type == player)
            {
                p8.Pal();
                Dplayer(plx, ply, prot, panim, banim, true);
            }
            else
            {
                if (IsIn(e, 72))
                {
                    p8.Pal();
                    p8.Pal(15, 3);
                    p8.Pal(4, 1);
                    p8.Pal(2, 8);
                    p8.Pal(1, 1);

                    Dplayer(e.X, e.Y, e.Prot, e.Panim, e.Banim, false);
                }
            }
        }
    }

    protected virtual void Dbar(int px, int py, F32 v, F32 m, int c, int c2)
    {
        p8.Pal();
        F32 pe = px + v * F32.FromDouble(0.3);
        F32 pe2 = px + m * F32.FromDouble(0.3);
        p8.Rectfill(px - 1, py - 1, px + 30, py + 4, 0);
        p8.Rectfill(px, py, pe.Double, py + 3, c2);
        p8.Rectfill(px, py, F32.Max(F32.FromInt(px), pe - 1).Double, py + 2, c);
        if (m > v) { p8.Rectfill(pe.Double + 1, py, pe2.Double, py + 3, 10); }
    }

    public virtual void Draw()
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

    public virtual string? SpritesPath => "Sprite_PcraftBase";

    public virtual string? FlagData => null;

    public virtual string? MapPath => "Map_PcraftBase";

    public virtual List<Soundtrack> Music =>
    [
        new(name: "original", 
        tracks: [
            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false), new(filename: "pcraft_og_cave_1", loop: true)], channel: 0),
            new Track(parts: [new(filename: "pcraft_og_surface", loop: true)], channel: 1),
            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false), new(filename: "pcraft_og_cave_1", loop: true)], channel: 2),
            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false), new(filename: "pcraft_og_cave_1", loop: true)], channel: 3),
            new Track(parts: [new(filename: "pcraft_og_cave_0", loop: false), new(filename: "pcraft_og_cave_1", loop: true)], channel: 4)]
        ),
        new(name: "new!", 
        tracks: [
            new Track(parts: [new(filename: "pcraft_new_title", loop: true)], channel: 0),
            new Track(parts: [new(filename: "pcraft_new_surface", loop: true)], channel: 1),
            new Track(parts: [new(filename: "pcraft_new_cave", loop: true)], channel: 1),
            new Track(parts: [new(filename: "pcraft_new_title", loop: false), new(filename: "pcraft_new_title", loop: true)], channel: 2),
            new Track(parts: [new(filename: "pcraft_new_death", loop: true)], channel: 3)]
        ),
        new(name: "pog edition", 
        tracks: [
            new Track(parts: [new(filename: "pcraft_pe_title_0", loop: false), new(filename: "pcraft_pe_title_1", loop: true)], channel: 0),
            new Track(parts: [new(filename: "pcraft_pe_surface_0", loop: false), new(filename: "pcraft_pe_surface_1", loop: true)], channel: 1),
            new Track(parts: [new(filename: "pcraft_pe_cave_0", loop: false), new(filename: "pcraft_pe_cave_1", loop: true)], channel: 2),
            new Track(parts: [new(filename: "pcraft_pe_win", loop: false)], channel: 3),
            new Track(parts: [new(filename: "pcraft_pe_death", loop: true)], channel: 4)]
        )
    ];

    public virtual Dictionary<string, string> Sfx => new()
    {
        { "original", "pcraft_og_" },
        { "soft", "pcraft_soft_" },
        { "pog edition", "pcraft_pe_" }
    };

    public virtual void Dispose()
    {

    }
}
