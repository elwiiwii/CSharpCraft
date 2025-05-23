using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Pcraft;

public abstract class DeluxeBase : IScene, IDisposable
{
    public abstract string SceneName { get; }
    public double Fps { get => 30.0; }

    private static Pico8Functions? p8;

#nullable enable
    private F32 clx;
    private F32 cly;
    private List<Entity> invent = [];
    private Entity? curItem;
    private Entity? curMenu;
    private int switchLevel;
    private bool canSwitchLevel;
    private Entity menuInvent;
    private Entity menuMap;
    private bool bossDead;
    private F32 baseRand;
#nullable disable

    private readonly string version = "";

    private bool lb4 = false;
    private bool lb5 = false;
    private bool block5 = false;
    private F32 time = F32.Zero;
    private int frame = 0;
    private int toogleMenu;
    private static readonly List<Material> allitems = [];

    private readonly int enstepWait = 0;
    private readonly int enstepWalk = 1;
    private readonly int enstepChase = 2;
    private readonly int enstepPatrol = 3;

    private static readonly string[] pwrNames = ["wood", "stone", "iron", "gold", "gem"];
    private static readonly int[][] pwrPal = [[2, 2, 4, 4], [5, 2, 4, 13], [13, 5, 13, 6], [9, 2, 9, 10], [13, 2, 14, 12]];
    
    private static readonly Material haxe = Item("haxe", 98);
    private static readonly Material sword = Item("sword", 99);
    private static readonly Material scythe = Item("scythe", 100);
    private static readonly Material shovel = Item("shovel", 101);
    private static readonly Material pick = Item("pick", 102);
    
    private static readonly int[] pstone = [0, 1, 5, 13];
    private static readonly int[] piron = [1, 5, 13, 6];
    private static readonly int[] pgold = [1, 9, 10, 7];
    
    private static readonly Material wood = Item("wood", 103);
    private static readonly Material sand = Item("sand", 114, [15]);
    private static readonly Material seed = Item("seed", 115);
    private static readonly Material wheat = Item("wheat", 118, [4, 9, 10, 9]);
    private static readonly Material apple = Item("apple", 116);
    
    private static readonly Material glass = Item("glass", 117);
    private static readonly Material stone = Item("stone", 118, pstone);
    private static readonly Material iron = Item("iron", 118, piron);
    private static readonly Material gold = Item("gold", 118, pgold);
    private static readonly Material gem = Item("gem", 118, [1, 2, 14, 12]);
    
    private static readonly Material disk = Item("savedisk", 10);
    private static readonly Material scroll = Item("map", 11);
    private static readonly Material artpart = Item("artifact piece", 63);
    private static readonly Material artifact = Item("talisman", 12);
    private static readonly Material fabric = Item("fabric", 69);
    private static readonly Material sail = Item("sail", 70);
    private static readonly Material glue = Item("glue", 85, [1, 13, 12, 7]);
    private static readonly Material boat = Item("boat", 86);
    private static readonly Material ichor = Item("ichor", 114, [11]);
    private static readonly Material potion = Item("potion", 85, [1, 2, 8, 14]);
    
    private static readonly Material ironbar = Item("iron bar", 119, piron);
    private static readonly Material goldbar = Item("gold bar", 119, pgold);
    private static readonly Material bread = Item("bread", 119, [1, 4, 15, 7]);
    private static readonly Material bossgem = Item("ruby", 0);
    
    private static readonly Material workbench = BigSpr(104, Item("workbench", 89, [1, 4, 9], true));
    private static readonly Material stonebench = BigSpr(104, Item("stonebench", 89, [1, 6, 13], true));
    private static readonly Material furnace = BigSpr(106, Item("furnace", 90, null, true));
    private static readonly Material anvil = BigSpr(108, Item("anvil", 91, null, true));
    private static readonly Material factory = BigSpr(71, Item("factory", 74, null, true));
    private static readonly Material chem = BigSpr(78, Item("chem lab", 76, null, true));
    private static readonly Material chest = BigSpr(110, Item("chest", 92));
    
    private static readonly Material inventary = Item("inventory", 89);
    private static readonly Material pickuptool = Item("pickup tool", 73);
    
    private static readonly Material etext = Item("text", 103);
    private static readonly Material player = Item(null, 1);
    private static readonly Material zombi = Item(null, 2);
    
    private static readonly Ground grwater = new() { Id = 0, Gr = 0 };
    private static readonly Ground grsand = new() { Id = 1, Gr = 1 };
    private static readonly Ground grgrass = new() { Id = 2, Gr = 2 };
    private static readonly Ground grrock = new() { Id = 3, Gr = 3, Mat = stone, Tile = grsand, Life = 15 };
    private static readonly Ground grtree = new() { Id = 4, Gr = 2, Mat = wood, Tile = grgrass, Life = 8, IsTree = true, Pal = [1, 5, 3, 11] };
    private static readonly Ground grfarm = new() { Id = 5, Gr = 1 };
    private static readonly Ground grwheat = new() { Id = 6, Gr = 1 };
    private static readonly Ground grplant = new() { Id = 7, Gr = 2 };
    private static readonly Ground griron = new() { Id = 8, Gr = 1, Mat = iron, Tile = grsand, Life = 45, IsTree = true, Pal = [1, 1, 13, 6] };
    private static readonly Ground grgold = new() { Id = 9, Gr = 1, Mat = gold, Tile = grsand, Life = 80, IsTree = true, Pal = [1, 2, 9, 10] };
    private static readonly Ground grgem = new() { Id = 10, Gr = 1, Mat = gem, Tile = grsand, Life = 160, IsTree = true, Pal = [1, 2, 14, 12] };
    private static readonly Ground grhole = new() { Id = 11, Gr = 1 };
    private static readonly Ground grartsand = new() { Id = 12, Gr = 1 };
    private static readonly Ground grartgrass = new() { Id = 13, Gr = 2 };
    private static readonly Ground grholeboss = new() { Id = 14, Gr = 1 };

    private Ground lastGround = grsand;

    private F32[][] level;
    private readonly F32[][] Rndwat = new F32[16][];
    private List<Entity> enemies = [];
    private F32[] data;
    private int levelsx;
    private int levelsy;
    private F32 plx;
    private F32 ply;
    private bool levelUnder;
    private List<Entity> entities = [];
    private int holex;
    private Level currentLevel;
    private int levelx;
    private int levely;
    private int holey;
    private F32 prot;
    private F32 lrot;
    private F32 panim;
    private F32 pstam;
    private F32 lstam;
    private F32 plife;
    private F32 llife;
    private F32 banim;
    private int stamCost;
    private F32 pow;
    private F32 coffx;
    private F32 coffy;
    private Level cave;
    private Level island;
    private List<Entity> furnaceRecipe = [];
    private List<Entity> workbenchRecipe = [];
    private List<Entity> stonebenchRecipe = [];
    private List<Entity> anvilRecipe = [];
    private List<Entity> factoryRecipe = [];
    private List<Entity> chemRecipe = [];
    private double savePx;
    private double savePy;
    private List<Entity> chests = [];
    private F32 gameState;
    private bool permaDeath;
    private List<Entity> nearEnemies = [];
    private F32 cmx;
    private F32 cmy;
    readonly int[] typeCount = new int[11];
    private readonly Ground[] grounds = { grwater, grsand, grgrass, grrock, grtree, grfarm, grwheat, grplant, griron, grgold, grgem, grhole, grartsand, grartgrass, grholeboss };

    private readonly Entity mainMenu = Cmenu(inventary, null, 128, "by nusan", "2016");
    private readonly Entity introMenu = Cmenu(inventary, null, 136, "a storm leaved you", "on a deserted island");
    private readonly Entity deathMenu = Cmenu(inventary, null, 128, "you died", "alone ...");
    private readonly Entity winMenu = Cmenu(inventary, null, 136, "you successfully escaped", "from the island");

    static DeluxeBase()
    {
        apple.GiveLife = 20;

        potion.GiveLife = 100;

        bread.GiveLife = 40;
    }

    private static Material Item(string n, int s, int[] p = null, bool bc = false)
    {
        Material it = new() { Name = n, Spr = s, Pal = p, BeCraft = bc };
        allitems.Add(it);
        return it;
    }

    private static Entity Instc(Material it, int? c = null, List<Entity> l = null)
    {
        return new() { Type = it, Count = c, List = l };
    }

    private static Entity SetPower(int? v, Entity i)
    {
        i.Power = v;
        return i;
    }

    private static Entity Entity(Material it, F32 xx, F32 yy, F32 vxx, F32 vyy)
    {
        return new() { Type = it, X = xx, Y = yy, Vx = vxx, Vy = vyy };
    }

    private static Entity Rentity(Material it, F32 xx, F32 yy)
    {
        return Entity(it, xx, yy, p8.Rnd(3) - F32.FromDouble(1.5), p8.Rnd(3) - F32.FromDouble(1.5));
    }

    private static Entity SetText(string t, int c, F32 time, Entity e)
    {
        e.Text = t;
        e.Timer = time;
        e.C = c;
        return e;
    }

    private static Material BigSpr(int spr, Material ent)
    {
        ent.BigSpr = spr;
        ent.Drop = true;
        return ent;
    }

    private static Entity Recipe(Entity m, List<Entity> require)
    {
        return new() { Type = m.Type, Power = m.Power, Count = m.Count, Req = require, List = m.List };
    }

    private bool CanCraft(Entity req)
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

    private void Craft(Entity req)
    {
        foreach (Entity e in req.Req)
        {
            RemInList(invent, e);
        }
        AddItemInList(invent, SetPower(req.Power, Instc(req.Type, req.Count, req.List)), -1);
    }

    private static void SetPal(int[] l)
    {
        for (int i = 0; i < l.Length; i++)
        {
            p8.Pal(i + 1, l[i]);
        }
    }

    private static Entity Cmenu(Material t, List<Entity> l = null, int? s = null, string te1 = null, string te2 = null)
    {
        return new() { List = l, Type = t, Sel = 0, Off = 0, Spr = s, Text = te1, Text2 = te2 };
    }

    private static int HowMany(List<Entity> list, Entity it)
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

    private static Entity IsInList(List<Entity> list, Entity it)
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

    private static void RemInList(List<Entity> list, Entity elem)
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

    private static void AddItemInList(List<Entity> list, Entity it, int p)
    {
        Entity it2 = IsInList(list, it);
        if (it2 is null || it2.Count is null)
        {
            AddPlace(list, it, p);
        }
        else
        {
            it2.Count = Math.Min((int)it2.Count + (int)it.Count, 254);
        }
    }
    private static void AddPlace(List<Entity> l, Entity e, int p)
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

    private bool IsIn(Entity e, int size)
    {
        return e.X > clx - size && e.X < clx + size && e.Y > cly - size && e.Y < cly + size;
    }

    private static F32 GetLen(F32 x, F32 y)
    {
        x *= F32.FromDouble(0.1);
        y *= F32.FromDouble(0.1);
        return F32.Sqrt(x * x + y * y + F32.FromDouble(0.001)) * 10;
    }

    private static F32 GetRot(F32 dx, F32 dy)
    {
        return dy >= 0 ? (dx + 3) * F32.FromDouble(0.25) : (1 - dx) * F32.FromDouble(0.25);
    }

    private void FillEne(Level l)
    {
        l.Ene = [Entity(player, F32.Zero, F32.Zero, F32.Zero, F32.Zero)];
        enemies = l.Ene;
        for (F32 i = F32.Zero; i <= levelsx - 1; i++)
        {
            for (F32 j = F32.Zero; j <= levelsy - 1; j++)
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

    private Level CreateLevel(int xx, int yy, int sizex, int sizey, bool isUnderground, bool makeNew)
    {
        Level l = new() { X = xx, Y = yy, Sx = sizex, Sy = sizey, IsUnder = isUnderground, Ent = [], Ene = [], Dat = new F32[8192] };
        SetLevel(l);
        levelUnder = isUnderground;
        CreateMap(makeNew);
        FillEne(l);
        l.Stx = F32.FromInt((holex - levelx) * 16 + 8);
        l.Sty = F32.FromInt((holey - levely) * 16 + 8);
        return l;
    }

    private void SetLevel(Level l)
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

    private void ResetLevel(bool makeNew)
    {
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
        frame = 0;

        toogleMenu = 0;
        invent = [];
        curItem = null;
        switchLevel = 0;
        canSwitchLevel = false;
        menuInvent = Cmenu(inventary, invent);
        menuMap = Cmenu(scroll, []);
        bossDead = false;
        baseRand = p8.Rnd(F32.MaxValue.Double);

        for (int i = 0; i <= 15; i++)
        {
            Rndwat[i] = new F32[16];
            for (int j = 0; j <= 15; j++)
            {
                Rndwat[i][j] = p8.Rnd(100);
            }
        }

        cave = CreateLevel(64, 0, 32, 32, true, makeNew);
        island = CreateLevel(0, 0, 64, 64, false, makeNew);

        if (makeNew)
        {
            Entity tmpworkbench = Entity(workbench, plx, ply, F32.Zero, F32.Zero);
            tmpworkbench.List = workbenchRecipe;
            MultipleAdd(invent, [tmpworkbench, Instc(pickuptool), Instc(disk)]);
        }
        else
        {
            bool loadFromBoss = Pico8Functions.Dget(18) == 489;
            if (loadFromBoss) { p8.Reload(0x1000, 0x1000, 0x2000, $"pcraft2_boss{version}.p8"); }
            LoadGame();
            if (loadFromBoss) { p8.Reload(); }
        }
    }

    private static int GetIndex(Material it)
    {
        for (int i = 0; i < allitems.Count; i++)
        {
            if (it == allitems[i])
            {
                return i;
            }
        }
        return -1;
    }

    private void SaveAdd(int v)
    {
        p8.Mset(savePx, savePy, v);
        savePx += 1;
        if (savePx >= 128)
        {
            savePx = 96;
            savePy += 1;
        }
    }

    private int SaveGet()
    {
        int v = p8.Mget(savePx, savePy);
        savePx += 1;
        if (savePx >= 128)
        {
            savePx = 96;
            savePy += 1;
        }
        return v;
    }

    private void SaveNewLine()
    {
        savePy += 1;
        savePx = 96;
    }

    private void SaveLevel(Level lev, List<Entity> chests)
    {
        SaveNewLine();
        int entcount = 0;
        for (int i = 0; i < lev.Ent.Count; i++)
        {
            Entity it = lev.Ent[i];
            if (it.Type == chest || it.Type.BeCraft)
            {
                entcount += 1;
            }
        }
        SaveAdd(entcount);
        for (int i = 0; i < lev.Ene.Count; i++)
        {
            Entity it = lev.Ene[i];
            if (it.Type == chest || it.Type.BeCraft)
            {
                SaveAdd(GetIndex(it.Type));
                SaveAdd(F32.FloorToInt(it.X / 16 - F32.Half));
                SaveAdd(F32.FloorToInt(it.Y / 16 - F32.Half));
                if (it.Type == chest)
                {
                    p8.Add(chests, it);
                }
            }
        }
    }

    private void LoadLevel(Level lev, List<Entity> chests)
    {
        SaveNewLine();
        int entcount = SaveGet();
        for (int i = 0; i < entcount; i++)
        {
            Material it = allitems[SaveGet()];
            int npx = SaveGet();
            int npy = SaveGet();
            if (it is not null)
            {
                Entity newEnt = Entity(it, F32.FromInt(npx * 16 + 8), F32.FromInt(npy * 16 + 8), F32.Zero, F32.Zero);
                if (it.Sl is not null)
                {
                    newEnt.List = it.Sl;
                }
                else if (it == chest)
                {
                    p8.Add(chests, newEnt);
                    newEnt.List = [];
                }
                newEnt.HasCol = true;
                p8.Add(lev.Ent, newEnt);
            }
        }
    }

    private void SaveGame()
    {
        Pico8Functions.Dset(1, plx.Double);
        Pico8Functions.Dset(2, ply.Double);
        Pico8Functions.Dset(3, currentLevel == island ? 0 : 1);
        Pico8Functions.Dset(4, 1); // is saved
        Pico8Functions.Dset(5, plife.Double);
        Pico8Functions.Dset(14, bossDead ? 789 : 0);
        Pico8Functions.Dset(18, 0);

        (savePx, savePy) = (96, 0);

        List<Entity> chests = [];

        // player inventory
        SaveAdd(invent.Count);
        foreach (Entity e in invent)
        {
            SaveAdd(GetIndex(e.Type));
            SaveAdd(e.Power is not null ? (int)e.Power : 255);
            SaveAdd(e.Count is not null ? (int)e.Count : 255);
            if (e.Type == chest)
            {
                p8.Add(chests, e);
            }
        }

        // map entities
        SaveLevel(island, chests);
        SaveLevel(cave, chests);

        // chest content
        SaveNewLine();
        SaveAdd(chests.Count);
        foreach (Entity e in chests)
        {
            SaveAdd(e.List.Count);
            foreach (Entity eChest in e.List)
            {
                SaveAdd(GetIndex(eChest.Type));
                SaveAdd(eChest.Power is not null ? (int)eChest.Power : 255);
                SaveAdd(eChest.Count is not null ? (int)eChest.Count : 255);
            }
        }

        Pico8Functions.Cstore();
    }

    private void LoadGame()
    {
        if (Pico8Functions.Dget(3) > 0)
        {
            SetLevel(cave);
            p8.Music(2);
        }

        plx = Pico8Functions.Dget(1);
        ply = Pico8Functions.Dget(2);
        plife = Pico8Functions.Dget(5);
        savePx = 96;
        savePy = 0;
        chests = [];
        bossDead = Pico8Functions.Dget(14) == 789;

        // player inventory
        int invc = SaveGet();
        for (int i = 0; i < invc; i++)
        {
            Material it = allitems[SaveGet()];
            int pow = SaveGet();
            int c = SaveGet();
            if (it is not null)
            {
                Entity newIt = Instc(it, c != 255 ? c : null);
                if (pow < 10) { newIt = SetPower(pow, newIt); }
                if (it.Sl is not null)
                {
                    newIt.List = it.Sl;
                }
                if (it == chest)
                {
                    newIt.List = [];
                    p8.Add(chests, newIt);
                }
                AddItemInList(invent, newIt, -1);
            }
        }

        // map entities
        LoadLevel(island, chests);
        LoadLevel(cave, chests);

        // chest content
        SaveNewLine();
        int chestcount = SaveGet();
        for (int i = 0; i < chestcount; i++)
        {
            Entity it = chests[i];
            int itemcount = SaveGet();
            for (int j = 0; j < itemcount; j++)
            {
                Material chestit = allitems[SaveGet()];
                int pow = SaveGet();
                int c = SaveGet();
                if (chestit is not null)
                {
                    Entity newIt = Instc(chestit, c != 255 ? c : null);
                    if (pow < 10) { newIt = SetPower(pow, newIt); }
                    if (chestit.Sl is not null)
                    {
                        newIt.List = chestit.Sl;
                    }
                    AddItemInList(it.List, newIt, -1);
                }
            }
        }
        curMenu = null;
        curItem = invent[0];
    }

    private static void MultipleAdd(List<Entity> l, List<Entity> vals)
    {
        for (int i = 0; i < vals.Count; i++)
        {
            p8.Add(l, vals[i]);
        }
    }

    public virtual void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        Pico8Functions.CartData($"nusan_pcraft{version}");

        gameState = Pico8Functions.Dget(0);
        permaDeath = Pico8Functions.Dget(19) > 0;

        void NewGame()
        {
            Pico8Functions.Dset(0, 0);
            ResetLevel(true);
        }
        p8.Menuitem(1, () => "new game", () => NewGame());
        if (!permaDeath)
        {
            void LoadGame()
            {
                if (Pico8Functions.Dget(4) == 1)
                {
                    p8.Reload();
                    ResetLevel(false);
                }
            }
            p8.Menuitem(2, () => "load game", () => LoadGame());
        }
        p8.Menuitem(3, () => "save game", () => SaveGame());

        p8.Music(1);

        furnaceRecipe = [];
        workbenchRecipe = [];
        stonebenchRecipe = [];
        anvilRecipe = [];
        factoryRecipe = [];
        chemRecipe = [];

        MultipleAdd(factoryRecipe, [Recipe(Instc(sail, 1), [Instc(fabric, 3), Instc(glue, 1)]), Recipe(Instc(boat), [Instc(wood, 30), Instc(ironbar, 8), Instc(glue, 5), Instc(sail, 4)]), Recipe(Instc(artifact, 1), [Instc(artpart, 4), Instc(glue, 2)])]);

        MultipleAdd(chemRecipe, [Recipe(Instc(glue, 1), [Instc(glass, 1), Instc(ichor, 3)]), Recipe(Instc(potion, 1), [Instc(glass, 1), Instc(ichor, 1)])]);

        MultipleAdd(furnaceRecipe, [Recipe(Instc(ironbar, 1), [Instc(iron, 3)]), Recipe(Instc(goldbar, 1), [Instc(gold, 3)]), Recipe(Instc(glass, 1), [Instc(sand, 3)]), Recipe(Instc(bread, 1), [Instc(wheat, 5)])]);

        Material[] toolTypes = [haxe, pick, sword, shovel, scythe];
        int[] quant = [5, 5, 7, 7, 7];
        Material[] materials = [wood, stone, ironbar, goldbar, gem];
        List<Entity>[] crafter = [workbenchRecipe, stonebenchRecipe, anvilRecipe, anvilRecipe, anvilRecipe];
        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < toolTypes.Length; i++)
            {
                p8.Add(crafter[j], Recipe(SetPower(j, Instc(toolTypes[i])), [Instc(materials[j], quant[i] * (j > 4 ? 3 : 1))]));
            }
        }

        MultipleAdd(workbenchRecipe, [Recipe(Instc(scroll), [Instc(wood, 15), Instc(fabric, 5), Instc(ichor, 3)]), Recipe(Instc(workbench, null, workbenchRecipe), [Instc(wood, 15)]), Recipe(Instc(stonebench, null, stonebenchRecipe), [Instc(stone, 15)]), Recipe(Instc(factory, null, factoryRecipe), [Instc(wood, 15), Instc(stone, 15)]), Recipe(Instc(chem, null, chemRecipe), [Instc(wood, 10), Instc(glass, 3), Instc(gem, 10)]), Recipe(Instc(chest), [Instc(wood, 15), Instc(stone, 10)])]);


        MultipleAdd(stonebenchRecipe, [Recipe(Instc(anvil, null, anvilRecipe), [Instc(iron, 25), Instc(wood, 10), Instc(stone, 25)]), Recipe(Instc(furnace, null, furnaceRecipe), [Instc(wood, 10), Instc(stone, 15)])]);

        factory.Sl = factoryRecipe;
        chem.Sl = chemRecipe;
        furnace.Sl = furnaceRecipe;
        anvil.Sl = anvilRecipe;
        workbench.Sl = workbenchRecipe;
        stonebench.Sl = stonebenchRecipe;

        ResetLevel(gameState == 3);
    }

    private static (int x, int y) GetMcoord(F32 x, F32 y)
    {
        return (F32.FloorToInt(x / 16), F32.FloorToInt(y / 16));
    }

    private bool IsFree(F32 x, F32 y, Entity e = null)
    {
        Ground gr = GetGr(x, y);
        return !(gr.IsTree || gr == grrock);
    }

    private bool IsFreeEnem(F32 x, F32 y, Entity e = null)
    {
        Ground gr = GetGr(x, y);
        return !(gr.IsTree || gr == grrock || gr == grwater);
    }

    private Ground GetGr(F32 x, F32 y)
    {
        (int i, int j) = GetMcoord(x, y);
        return GetDirectGr(F32.FromInt(i), F32.FromInt(j));
    }

    private Ground GetDirectGr(F32 i, F32 j)
    {
        if (i < 0 || j < 0 || i >= levelsx || j >= levelsy) { return grounds[0]; }
        return grounds[p8.Mget(i.Double + levelx, j.Double)];
    }

    private void SetGr(F32 x, F32 y, Ground v)
    {
        (int i, int j) = GetMcoord(x, y);
        if (i < 0 || j < 0 || i >= levelsx || j >= levelsy) { return; }
        p8.Mset(i + levelx, j, v.Id);
    }

    private F32 DirGetData(F32 i, F32 j, F32 @default)
    {
        int g = F32.FloorToInt(i + j * levelsx);
        if (data[g - 1] == 0)
        {
            data[g - 1] = @default;
        }
        return data[g - 1];
    }

    private F32 GetData(F32 x, F32 y, int @default)
    {
        (int i, int j) = GetMcoord(x, y);
        if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
        {
            return F32.FromInt(@default);
        }
        return DirGetData(F32.FromInt(i), F32.FromInt(j), F32.FromInt(@default));
    }

    private void SetData(F32 x, F32 y, F32 v)
    {
        (int i, int j) = GetMcoord(x, y);
        if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
        {
            return;
        }
        data[i + j * levelsx - 1] = v;
    }

    private void Cleardata(F32 x, F32 y)
    {
        (int i, int j) = GetMcoord(x, y);
        if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
        {
            return;
        }
        data[i + j * levelsx - 1] = F32.Zero;
    }

    private static int Loop(int sel, List<Entity> l)
    {
        int lp = l.Count;
        return (sel % lp + lp) % lp;
    }

    private bool EntColFree(F32 x, F32 y, Entity e)
    {
        return F32.Max(F32.Abs(e.X - x), F32.Abs(e.Y - y)) > 8;
    }

    private static (F32 dx, F32 dy) ReflectCol(F32 x, F32 y, F32 dx, F32 dy, Func<F32, F32, Entity, bool> checkfun, F32 dp, Entity e = null)
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

    private void AddItem(Material mat, int count, F32 hitx, F32 hity)
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

    private void UpGround()
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

    private static F32 UpRot(F32 grot, F32 rot)
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

    private (F32 dx, F32 dy, bool canAct) UpEntity(F32 dx, F32 dy, bool canAct)
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

    private void UpEnemies(F32 ebx, F32 eby)
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
                if (e.Step == enstepWait || e.Step == enstepPatrol)
                {
                    e.Step = enstepWalk;
                    e.Dx = p8.Rnd(2) - 1;
                    e.Dy = p8.Rnd(2) - 1;
                    e.Dtim = 30 + p8.Rnd(60);
                }
                else if (e.Step == enstepWalk)
                {
                    e.Step = enstepWait;
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
                if (e.Step == enstepChase)
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
                        e.Step = enstepPatrol;
                        e.Dtim = 30 + p8.Rnd(60);
                    }
                }
                else
                {
                    if (distp < 40)
                    {
                        e.Step = enstepChase;
                        e.Dtim = 10 + p8.Rnd(60);
                    }
                }
                e.Dtim -= 1;
            }

            F32 dl = mspeed / GetLen(e.Dx, e.Dy);
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

    private void UpHit(F32 hitx, F32 hity, Ground hit)
    {
        bool lifeItem = curItem is not null && curItem.Type.GiveLife is not null;
        if (nearEnemies.Count > 0 && !lifeItem)
        {
            p8.Sfx(19, 3);
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
        else if (hit.Mat is not null && !lifeItem)
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
            if (curItem.Type == scroll)
            {
                curMenu = menuMap;
            }
            switch (hit, curItem.Type)
            {
                case (Ground, Material) gm when gm == (grgrass, scythe):
                    SetGr(hitx, hity, grsand);
                    if (p8.Rnd(1) > F32.FromDouble(0.4)) { AddItem(seed, 1, hitx, hity); }
                    break;
                case (Ground, Material) gm when gm == (grartgrass, scythe):
                    SetGr(hitx, hity, grartsand);
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
                case (Ground, Material) gm when gm == (grartsand, shovel):
                    SetGr(hitx, hity, grsand);
                    Entity newChest = Entity(chest, hitx, hity, F32.Zero, F32.Zero);
                    newChest.List = [Instc(artpart, 1)];
                    newChest.HasCol = true;
                    p8.Add(entities, newChest);
                    p8.Sfx(39, 3);
                    break;
                case (Ground, Material) gm when gm == (grwater, sand):
                    SetGr(hitx, hity, grsand);
                    RemInList(invent, Instc(sand, 1));
                    break;
                case (Ground, Material) gm when gm == (grwater, boat):
                    Pico8Functions.Dset(0, bossDead ? 333 : 999);
                    if (permaDeath) { Pico8Functions.Dset(4, 0); }
                    Pico8Functions.Load($"pcraft2{version}");
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
        frame += 1;

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
                        ResetLevel(true);
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
                    else if (curMenu.List[curMenu.Sel].Type == disk)
                    {
                        SaveGame();
                        curMenu = null;
                        block5 = true;
                        p8.Sfx(16, 3);
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

        if (switchLevel > 0)
        {
            if (switchLevel == 2)
            {
                if (!bossDead)
                {
                    SaveGame();
                    Pico8Functions.Load($"pcraft2_boss{version}");
                }
            }
            else
            {
                if (currentLevel == cave) { SetLevel(island); p8.Music(1); }
                else { SetLevel(cave); p8.Music(2); }
                FillEne(currentLevel);
                switchLevel = 0;
                canSwitchLevel = false;
            }
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
        if (playHit == grhole || playHit == grholeboss)
        {
            if (canSwitchLevel)
            {
                switchLevel = playHit == grhole ? 1 : 2;
            }
        }
        else
        {
            canSwitchLevel = true;
        }

        F32 dx = F32.Zero;
        F32 dy = F32.Zero;
        bool canAct = true;

        if (p8.Btn(0)) dx -= 1;
        if (p8.Btn(1)) dx += 1;
        if (p8.Btn(2)) dy -= 1;
        if (p8.Btn(3)) dy += 1;

        F32 dl = 1 / GetLen(dx, dy);

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
                pow = F32.One;
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
            Pico8Functions.Dset(0, 666);
            if (permaDeath) { Pico8Functions.Dset(4, 0); }
            Pico8Functions.Load($"pcraft2{version}");
            p8.Reload();
            p8.Memcpy(0x1000, 0x2000, 0x1000);
            curMenu = deathMenu;
            p8.Music(4);
        }
    }

    private static (int mx, int my) Mirror(F32 rot)
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

    private void Dplayer(F32 x, F32 y, F32 rot, F32 anim, F32 subanim, bool isplayer)
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

    private static F32[][] Noise(int sx, int sy, F32 startscale, F32 scalemod, int featstep)
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

            for (int i = 0; i <= sx - 1; i += step)
            {
                for (int j = 0; j <= sy - 1; j += step)
                {
                    F32 c1 = n[i][j];
                    F32 c2 = n[i + step][j];
                    F32 c3 = n[i][j + step];
                    n[i + step / 2][j] = (c1 + c2) * F32.Half + (p8.Rnd(1) - F32.Half) * cscal;
                    n[i][j + step / 2] = (c1 + c3) * F32.Half + (p8.Rnd(1) - F32.Half) * cscal;
                }
            }

            for (int i = 0; i <= sx - 1; i += step)
            {
                for (int j = 0; j <= sy - 1; j += step)
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

    private F32[][] CreateMapStep(int sx, int sy, int a, int b, int c, int d, int e)
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

                int id = a == 3 ? (coast < F32.FromDouble(-1.3) ? 0 : a) : a;
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

    public (F32, F32) GetRandPos()
    {
        F32 npx = F32.Neg1;
        F32 npy = F32.Neg1;
        for (int i = 0; i <= 500; i++)
        {
            int depx = F32.FloorToInt(levelsx / 8 + p8.Rnd(levelsx * 6 / 8));
            int depy = F32.FloorToInt(levelsy / 8 + p8.Rnd(levelsy * 6 / 8));
            F32 c = level[depx][depy];
            if (c == 1 || c == 2)
            {
                npx = F32.FromInt(depx * 16 + 8);
                npy = F32.FromInt(depy * 16 + 8);
                break;
            }
        }
        return (npx, npy);
    }

    private void CreateMap(bool makeNew)
    {
        bool needmap = makeNew;

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

            if (makeNew)
            {
                holex = levelsx / 2;
                holey = levelsy / 2;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        level[holex + i][holey + j] = levelUnder ? F32.One : F32.FromInt(3);
                    }
                }
                level[holex][holey] = F32.FromInt(11);
                holex += levelx;
                holey += levely;
            }

            if (!needmap)
            {
                (plx, ply) = GetRandPos();
                if (plx < 0)
                {
                    needmap = true;
                }
                plx = plx * 16 + 8;
                ply = ply * 16 + 8;
                if (levelUnder)
                {
                    int bpx = F32.FloorToInt(p8.Rnd(levelsx - 2)) + 1;
                    int bpy = p8.Rnd(1) > F32.Half ? 1 : levelsx - 2;
                    if (p8.Rnd(1) > F32.Half) { (bpx, bpy) = (bpy, bpx); }
                    for (int i = bpx - 1; i <= bpx + 1; i++)
                    {
                        for (int j = bpy - 1; j <= bpy + 1; j++)
                        {
                            level[i][j] = F32.FromInt(10);
                        }
                    }
                    level[bpx][bpy] = F32.FromInt(14);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        (F32 ax, F32 ay) = GetRandPos();
                        (int ix, int iy) = (F32.FloorToInt(ax), F32.FloorToInt(ay));
                        level[ix][iy] = level[ix][iy] + 11;
                        if (ix < 0)
                        {
                            needmap = true;
                        }
                    }
                }
            }
        }

        if (makeNew)
        {
            for (int i = 0; i <= levelsx - 1; i++)
            {
                for (int j = 0; j <= levelsy - 1; j++)
                {
                    p8.Mset(i + levelx, j + levely, level[i][j].Double);
                }
            }

            if (!levelUnder)
            {
                Pico8Functions.Dset(62, plx.Double);
                Pico8Functions.Dset(63, ply.Double);
            }
        }
        else
        {
            plx = Pico8Functions.Dget(1);
            ply = Pico8Functions.Dget(2);
            holex = levelsx / 2 + levelx;
            holey = levelsy / 2 + levely;
        }

        clx = plx;
        cly = ply;
        cmx = plx;
        cmy = ply;
    }

    private bool Comp(F32 i, F32 j, Ground gr)
    {
        Ground gr2 = GetDirectGr(i, j);
        return gr is not null && gr2 is not null && gr.Gr == gr2.Gr;
    }

    private F32 WatVal(F32 i, F32 j)
    {
        return Rndwat[F32.FloorToInt(F32.Abs(i * 2) % 16)][F32.FloorToInt(F32.Abs(j * 2) % 16)];
    }

    private void WatAnim(F32 i, F32 j)
    {
        F32 a = (time * F32.FromDouble(0.6) + WatVal(i, j) / 100) % 1 * 19;
        if (a > 16) { p8.Spr(13 + a.Double - 16, i.Double * 16, j.Double * 16); }
    }

    private F32 RndCenter(F32 i, F32 j)
    {
        return (F32.Floor(WatVal(i, j) / 34) + 18) % 20;
    }

    private int RndSand(F32 i, F32 j)
    {
        return F32.FloorToInt(WatVal(i, j) / 34) + 1;
    }

    private int RndTree(F32 i, F32 j)
    {
        return F32.FloorToInt(WatVal(i, j) / 51) * 32;
    }

    private static void Spr4(F32 i, F32 j, F32 gi, F32 gj, int a, int b, int c, int d, int off, Func<F32, F32, int> f)
    {
        p8.Spr(f(i, j + off) + a, gi.Double, (gj + 2 * off).Double);
        p8.Spr(f(i + F32.Half, j + off) + b, gi.Double + 8, (gj + 2 * off).Double);
        p8.Spr(f(i, j + F32.Half + off) + c, gi.Double, (gj + 8 + 2 * off).Double);
        p8.Spr(f(i + F32.Half, j + F32.Half + off) + d, gi.Double + 8, (gj + 8 + 2 * off).Double);
    }

    private void DrawBack()
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

                if (gr == grhole || (gr == grholeboss && !bossDead))
                {
                    p8.Pal();
                    if (gr == grholeboss || !levelUnder)
                    {
                        p8.Palt(0, false);
                        if (gr == grholeboss) { p8.Pal(15, 5); }
                        p8.Spr(31, gi.Double, gj.Double, 1, 2);
                        p8.Spr(31, gi.Double + 8, gj.Double, 1, 2, true);
                    }
                    p8.Palt();
                    p8.Spr(77, gi.Double + 4, gj.Double, 1, 2);
                }
            }
        }
    }

    private static void Panel(string name, int x, int y, int sx, int sy)
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

    private static void ItemName(int x, int y, Entity it, int col)
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

    private void List(Entity menu, int x, int y, int sx, int sy, int my)
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

    private void RequireList(Entity recip, int x, int y, int sx, int sy)
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

    public static void Printb(string t, double x, double y, int c)
    {
        p8.Print(t, x + 1, y, 1);
        p8.Print(t, x - 1, y, 1);
        p8.Print(t, x, y + 1, 1);
        p8.Print(t, x, y - 1, 1);
        p8.Print(t, x, y, c);
    }

    private static void Printc(string t, int x, int y, int c)
    {
        p8.Print(t, x - t.Length * 2, y, c);
    }

    private void Dent()
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

    private static void Sorty(List<Entity> t)
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

    private void Denemies()
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

    private static void Dbar(int px, int py, F32 v, F32 m, int c, int c2)
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
        if (curMenu == menuMap)
        {
            p8.Rectfill(13, 43, 78, 108, 4);
            p8.Rectfill(14, 44, 77, 107, 9);
            int[] pp = [15, 7, 6, 7, 15, 2, 2, 0, 0, 0, 14, 4, 4, 4];
            F32 dx = levelx * F32.FromDouble(0.25);
            for (int i = 0; i <= levelsx - 1; i++)
            {
                for (int j = 0; j <= levelsy - 1; j++)
                {
                    var c = p8.Mget(i + levelx, j);
                    if (c > 0)
                    {
                        p8.Pset(14 + i + dx, 44 + j + dx, pp[c]);
                    }
                }    
            }
            if (Math.Floor(frame / 8.0) % 2 == 1) { p8.Pset(14 + plx / 16 + dx, 44 + ply / 16 + dx, 8); }
        }
    }

    public string SpriteImage => "";

    public string SpriteData => @"
00aaaa00ffffffffffffffffffffffffffffffff44fff44ffff44fff020121000004200002031000166666610099944000111100000000000001000000101000
0a2888a0ffffffffffffffffffffffffffff444fff4ffff4ff4fffff31031020030310204142000016eeee610995594401aaaa10000100000011100001000100
a288878afff4fffff4ffffffff4fffff4444fffffff444ff44fff44f20520002400200103031041016eeee61995995941a9999a1001110000110110010000100
a288888affffffffffffff4ffffffffffffffff4ff4fff44fff44ff415340401340100402020030216eeee619595559919899891000100000011100000000000
9a2888a9ffffffffffffffffffffff4fff44444fffffffffffffffff42424303230040341014020116eeee619595995919922991000000000001000001100100
09a22a90ffffffffff4fffffffffffffffffffffff44fff444fff44f313132021240302300034104166666614959959912922921000000000000000000010000
009aa900ffff4ffffffffffffffffffff4444fff44ff444fff444fff002021404130201204023003167777614495599001211210000000000000000000000000
00099000fffffffffffffffffffffffffffff444ffffffffffffffff001010000020100003012040166666100449990000100100000000000000000000000000
fffff11ffffffffff11fffff3353333333333333ff1111ffff1111ffff1111ff6666666666666666fffff44444ffff444444ffffddddddddddddddddffffffff
fff115511fffff1115511fff3515333333333353f155551111555511115555df6666666666666666ff44444444444444444444ffddddddddddddddddfffff111
ff15533551fff155533551ff5153333333333515155555555555555555555dd166666dddddd66666f4444444444444444444444fddddddddddddddddfff11666
f15333333511153333333b1f515333333353515315556555555665555556ddd1666dddddddddd66644441111144444411444444fddddddddddddddddff166666
f15333335155515333333b1f351533333515515315556666666666666666ddd1666dddddddddd6664411ddddd111111dd144444fddddddddddddddddf116dddd
f15333351533351533333b1f33533533351535151555566666666666666dddd1666dddddddddd66641dddddddddddddddd144444dddddd1111ddddddf16ddddd
1533333353333353333333b13333515bb1533515f155566666666666666ddd1f6666ddd11ddd666641ddddddddddddddddd11444ddddd144441dddddf1dddddd
1533333333333333335333b1333335b115333353f155566666666666666ddd1f6666dd1001dd666641ddddddddddddddddddd114ddddd14ff41dddddf1dd5555
f15335333333333335153b1f333333b115533333f155566666666666666ddd1f666655100155666641dddddddddddddddddddd14ddddd144441dddddf1d55555
f1535153333333333351b1ff33333515511535331555566666666d66666dddd1666655111155666641dddddddddddddddddddd14dddd14444441ddddf1551111
ff15153333333333333b1fff35335153355331531555666665d666666666ddd16666555555556666441dddddddddddddddddd14fdddd14444441ddddf1511111
fff1533333333533333b1fff515535333333551515556666666666666666ddd16666655555566666f41dddddddddddddddddd14fddd1444114441dddf1111000
fff1533333335153333b1fff351153333333351515556666666666666666ddd16666666666666666f41dddddddddddddddddd14fdddd111dd111ddddf1110000
fff15333333335333351b1ff33551533333351531555566666665666666dddd16666666666666666441dddddddddddddddddd14fddddddddddddddddff110000
ff1515333333333335153b1f3335153333333533f15556666666d666666ddd1f666666666666666641dddddddddddddddddddd14ddddddddddddddddfff11111
f15351533333333333533b1f3333533333333333f155566666666666666ddd1f666666666666666641dddddddddddddddddddd14ddddddddddddddddffffffff
f15335333333333333333b1f3333333333533333f155566666666666666ddd1f66666666666d666641ddddddddddddddddddd114dddddddddddddddd00000000
1533333335333335333333b13333333335153333f1556666666666666666dd1f6666666665566666441dddddddddddddddd11444dddddddddddddddd01111000
1533333351533351533333b1335333533353333315556dddddd66dddddd6ddd166655666d6666666f441dddddddddddddd14444fdddddddddddddddd01aaa100
f1533333351bbb1533333b1f35153515333335531555ddddddddddddddddddd166d66d6666666666ff441dddddddddddd144444fdddddddddddddddd12999a10
f153333333b111b333333b1f5153335335335115155dddddddddddddddddddd166666666666666d6fff4411ddd1111ddd1444fffdddddddddddddddd02299210
ff1bbb33bb1fff1b33bbb1ff353353335153355315ddddddddddddddddddddd16666666666666556fff44441114444111444ffffdddddddddddddddd00129210
fff111bb11fffff1bb111fff3335153335153333fddddd1111dddd1111dddd1f55666666666d5666ffff444444ffff444444ffffdddddddddddddddd00212100
ffffff11ffffffff11ffffff3333533333533333ff1111ffff1111ffff1111ff66d6666d66666666ffffffffffffffffffffffffdddddddddddddddd00001000
00000000222020000011111111111100001dd000000282000000770001400000000004100012022001000010000000000011a861000000000000000010101010
0000000024224200011dddddddddd11001d1110000282820000777700124006006004210012e12ee141111410000000011e1bec1009009000000000151515151
000000022422420011d1111111111d111d11111002828282007777770012441441442100122e11e1124444210000000016e1bec100400400000000115a585651
00020024244342001d111111111111d11d1111102828282807777775140122522522104112ee112241222214001100001111325100444400000001d15b5e5c51
00022024334344201d111111111111d11d1111108282828277777750124111611611142112eee1212444444202ff1000999999990040040000011dcd5b5e5c51
00024244434434201d111111111111d101d1110008282820577775000124441441444210222eee114222222422ff1000541111450024420000161ded5b5e5c51
00224334443434201d111111111111d1001dd00002828200057750000012225225222100222222102411114222220000541111450020020001676ded53525151
02344433344444201d111111111111d100000000002020000055000014111151151111411221110002444420222000005411114500222200156f6d8d55555551
02334444434444201d111111111111d11010101006111600417710211241116116111421222222220d1dd1d006666660444444440020020015666ddd52222251
24434334443344421d111111111111d1010101010061600017777142012444144144421023333332d515515d15666dd549999994002222001555555525555521
23444434444444201d111111111111d1101010100623260077771442001222522522210023333332511111150155ddd549999994001111001999999999999991
02344333444443201d111111111111d101010101623432607774142100141151151141002222222211a9e9110015dd5044444444001001001944444444444491
00234444334432001d111111111111d1101010106333336017114421001241611614210055555555119e8a110001d50055599555001111001999999999999991
012333344333221011d1111111111d11010101016233326001444210000124144142100051111115111111110001500054455445001001001544501010154451
0112222222221110011dddddddddd110101010100623260024422100000012222221000051111115156556510054210054444445001001001544510101054451
00011111111110000011111111111100010101010066600012211000000001111110000051111115011111100542121055555555000000000155101010105510
0020000000000000000000000000001100000000000011110111100000001f100222222222222220000001111100000000000000000000000000000000000000
024202000000200000002200000001410000111000001441144441000001fff10233333333333320001111565111100000666666666666000011111111111100
02442420000220000002341000001410000144410001444101111410001ffff4023333333333332001ddd1d1d1ddd100066666666776666d0144444444444410
0244344422242020000244410001410000023441000234110002314101ffff41023333333333332001ddd15651ddd10066666666666666dd0144999999994410
2434434442444242002324410014100000232141002321000023214119fff410023333333333332001ddd1d1d1ddd1001666666666666dd10149999999999410
24434344344444420232441002410000023201410232000002320141019f4100023333333333332001ddd15651ddd100156666666666dd100149999999999410
24434444434443202320110023200000232000102320000023200141001910000233333333333320011111d1d111110015566666666dd1000149999999999410
234444434433432032000000320000003200000032000000020000100001000002333333333333201dddd15651dddd101555666666dd10000149999999999410
023444434343322000555000000005000000400005555d50000222000000122002222222222222201555515551555510015556666dd100000144999999994410
02324443432220000511150000505b50001242205000d6d501234320000123420555555555555550151111111111151001555566ddd400000144444444444410
00202344320000005111115005b5b735012242e2500d676d1234343200123432055555555555555015191a181a1915101245555ddd2410000155559999555510
0000023444200000511111155b73535012282efe5000d6d50123434201234321051000000000015015121812181215101412555dd21441000154445445444510
0000002433200000511111150535b5001288efe250000d051234332112343210051101010101015015115111115115100101255d210144100154445555444510
000012333211000005111115005b735001288e825000000512332232123321000510101010101150155161555161551000001242100014100154444444444510
00012222222210000055115000053500001288205000000501221121012210000511010101010150115515555515511000000141000001000155555555555510
00001111111100000000550000005000000122000555555000110010001100000510101010101150011111111111110000000111000000000011111111111100
0000000000001020204040303030302020204020202020204040100040000010b030001020100010204010000000000000000000100000000000000000000000
1020529393626272a1b1e2d3e3e3e3b2d3e3101010a0a0a0308010101010a0a0a00000000000000000000000000030303030801010a010803030303030303030
00000000000000102040203030302040202020402020204020201000100000303030100000001020204010000000000000000000000000000000000000000000
1030526293628372a2d3b2b2b2d3d3d3d3b210a0a0a010a08080101010101010100000000000000000000000000030303030101010a0a0a03030308080a01030
00000000000000001020203030304020202020202020402030202000100000001010202020202040204010001000000010000000000000000000000000000000
3010528383939372a2d3b2e3d3e3e3d3b2d3901010a010a0103090101010101010000000000000000000000000003030303010a0a0a010a03030801010a01080
00000000000000000000003030302040404020202020202030202000101000001020202020202020202010003010000010402020401010101000000000000000
3030526262836272a3b3e1d3d3d3e3b2b2b29010a010101080801010a01010a0100000000000000000000000000030303030901010a010a03030101010a0a0a0
00000000000000000000000000000010204020202040203030302000101000000020404020204040202000303010000020202020202020202000000000000000
10105283838393826171a2b2d3b2b2b2d3d330801010101030801010101010101000000000000000000000000000303030309010a0101010303010a0a0a010a0
00000000000000000000000000000010202020402020202030202010000000000010402020402020202010003000000010404020202040100000000000000000
10105293938163636373a3b3b3b3e1b2d3d330303080a030301010a0101010101000000000000000000000000000303030303080101010103030901010a010a0
0000000000000000000000000000c010204020202020202040204020101010000010202020202020202010000000000010204020204020000000000000000000
2030529393723020201010302020a2d3e3b2303030303030801010a01010b01010000000000000000000000000003030303030303080a03030309010a0101010
00000000000000000000000000001020202020402020204020202040402040100010402020402020101010101010100000102020202010000000000000000000
3020528362721020301020103010a2e3b2e330303030303080101010101010101000000000000000000000000000303030303030303030303030308010101010
00000000000010101000000000101040202040101010404020204020402040100010204020201000000010202040100000002020202000000000000000000000
5161929383721030301020202030a2d3e3b230808080303080a0101010101010100000000000000000000000000030303030303030303030303030303080a030
00000000100000000000000000001010202010100010101020404020404010100000000000000000001010102010000000000010200000000000000000000000
5363636363731030102020101020a2e3d3b290101080303080a09090101080803000000000000000000000000000303030303080808030303030303030303030
00000000100000000000000000000010201010000000001010101010101000000000001010000000101010100000000010000000000000000000000000000000
1020202030101010101010102020a2e3d3e390101010803080909090108030303000100000000000000000000000303030309010108030303030303030303030
00000010200000000000000000000000101000000000000000000000000000000000104040400000201010000000101020100000000000000000000000000000
2020302020102030303010202030a2d3e3e330308080303030909090108080303010400000000000000000000000303030309010101080303030308080803030
00001010200000000000000000000000100000001000000000000010101000000010202020200000202000003030204020204010000000000000000000000000
5161616161616171a1b1b1b1b1b1e2d3e3b230303030303030109090101010808000000000000000000000000000303030303030808030303030901010803030
00000000100000000000000000000000100000001000000000000000000000000000104040100000201000000030102020101000000000000000000000000000
5363919393629372a2e3d3d3d3e3b2e3e3e330303030303030309080a010a0108000000000000000000000000000303030303030303030303030901010108030
00000000000000000000000000000010100000001000000000000000000000000000000000000000000000000000001010000000000000000000000000000000
2010526293626272a2b2e3b2d3e3b2e3e3d38080803030303030308010a010101020201000000000000000000000303030303030303030303030303080803030
00000000000000000000000010101010100000001000000000000000000000000000000010000000000000000000000000000000000000000000000000000000
1010536391936272a3b3e1b2b2e3d3b2d3d330303030303030303030801010a0a020202000000000000000000000303030308080803030303030303030303030
00000000000000000000104030202020100000001000000000000000000000000000102020200000000000000000000000000000000000000000000000000000
10201020526262826171a2d3e3e3e3b2d3e330303030803030303000000000000040402000000000000000000000303030303030303030303030303030303030
00000000000000000010202040201010000000000000000000000000000000000000001040100000000000000000000000000000000000000000000000000000
10302030536391626272a2b2b2d3d3d3d3b230303080803030303000000000000020201000000000000000000000303030303030303080303030808080303030
00000000000000000010202020201000000000000000000000000000000000000000000010000000000000000000000010000000000000000000000000000000
10100000000000000000000000003030303080808080103030303000000000000020101000000000000000000000303030303030308080303030303030303030
00000000000000001040202020200000000000000000000000000000000000000000000000000000000000000000001020000000c00000000000000000000000
00000000000000000000000000003030303030303030303030303000000000000010000000000000000000000000303030308080808010303030303030308030
00000000000000003020202040400000200000001000000000000000000000001000000000000000000000101000001020100000300000000000000000000000
00000000000000000000000000003030303030303030303030303000000000000000000000000000002010100000000000000000000030303030303030808030
00000000000000001010202040100000000000000000000000000000000000000000000000000000000000000000000010000000100000000000000000000000
00000000000000000000000000003030303030303030303030303000000000000000000000000000001000000000000000000000000030303030808080801030
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000c00000000000000000000000
0000000000000000000000000000b1b1b1b1b1c1201020300212430000000000000000000000000000000000000000000000003030303030303080a030203002
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100000000000000000000000
0000100000000000000000000000e3b2b2b2e3c23020202002333300000000000000000000000000000000000000000000000030303030303030303030202002
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000100000000000000000000000e3b2b2e3e3d2b1c1101002124300000000000000000000000000000000000000000000000030303030303030303030101002
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000100000000000000000000000d3d1b3b3e1d3d3c2203002311300000000000000000000000000000000000000000000000030303030308080803030203002
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000100000000000000000000000e3c21030a2b2e3d2b1c102223000000000000000000000000010000000000000000000000030303030901010803030b1c102
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000100000000000000000000000d3c21030a2e3e3d1b3c302223000000000000000000000001040000000000000000000000030303030901010108030b3c302
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000e3d2b1b1e2e3d3c2011142321100100000000000000000000000000000000000000000000030303030303080803030011142
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000e1b2b2d3e3d1b3c3023312331210400000000000000000000000000000000000000000000030303030303030303030023312
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
0000000000000000000000000000a2d3b2e3b2c20111423312331200000000000000000000002020100000000000000000000030303030303030303030423312
00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000
11111111111111111111111111111111111111111111111111111100000000000000000000002020200000000000000000000030303030808080303030111111
".Replace("\n", "").Replace("\r", "");

    public string FlagData => @"";

    public (int x, int y) MapDimensions => (0, 0);

    public string MapData => @"
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%<%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%a%9%9%9%9%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%<#g%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%<#o%9%9%9%9%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%a#o%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$?%9%9%9%9%9%a%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$?$?$?$?$?$?$?$?$?$?%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%4$?$?$?$?$?$?$?$?$?$?$?$?$?$.$.%9%9%9%4$?$?%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$?$?$?$?$?$?$?$?$?$?$?$?$?$?$>$-%9%9$?$?$?$?$D%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%4$?$?$>$/$?$?$?$>$/$?$?$?$?$?$.$$%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%4$>$?$.$.$?$>$?$.$/$?$?$?$?$>$-%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$?%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$?$.$?$.#z$?$.$.$-$?$?$?$?$?$.$$%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$?$?$?$?$?$?%9%9%9%9%9%9%9%9%9%9%9%9%9$>$-$?$-%4$>$.$.$$$?$>$?$?$>$.$$%9%9%9%9%9%9%9%9%9%9%9%9%9$?$?%9%9%9%9%9%9%9%9%9%9%9$?$?$?$?$?$?$?$?$?%9%9%9%9%9%9%9%9%9%9%9%9%9$>$$$>$-$?$.$.$-#o$?$.$?$?$.$.$$%9%9%9%9%9%9%9%9
%9%9%9%9$?$?$?$?%9%9%9%9%9%9%9%9$?$?$?$?$?$?$?$?$?$?$?$?$D%9%9%9%9%9%9%4$?$?$D%9$-%9$>$$$>$.#y$'#o$?$.$?$.$.$-%9%9%9%9%9%9%9%9%9%9%9%4$?$?$?$?$?$?%9%9%9%9%9%9%9%9%9%9%9%9%9$?$D%9%9%9%9%9%9%9%9%9%9%4$?$?$?$?%9%9%9$.$$$-#x%2%a%9$>$.$?$.$.$-%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%4$D%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%4$?$?$?$?$?$?$D%9$-%9$$%9%9%a%9%3$-$?$.$.$$%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$$%9%9%9%<#g%9%2$-$?$.$-%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%i%9%9%9%9%9%9%9%<#o%9%9$$%4$.$$%9%9$?$?$D%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$.$.$.$.$.$.$.$4%9%9%9%9%9%9%9%9%9%9%9%9%9#R%9%9%9%9%9%9%9%<#o%9%9%9%2#x%9$?$?$?$?$?$?$D%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$.$.$?$?$?$?$?$.$.%9%9%9%9%9%9%9%9%9%9%9%9%9#G%i%9%9%9%9%9%9%<#o%9%9%9%9%9%9%9%9%9$?$?$?$?$D%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%3$.$.$.$?$?$?$.$.$.$.%9%9%9%9%9%9%9%9%9%9%9%9#G#r%9%9%9%9%9%9%<#o%9#g#o%9%9%9%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$.$-#x#y$.$?$?$.$.$.$.$4%9%9%9%9%9%9%9%9%9%9%9#G#g%i%9%9%9%9%9%a#e%/%l#g%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%3$-#4#4#4$.$.$.$.$.$.$.$.%9%9%9%9%9%9%9%9%9%9%9#E#g#r%9%9%9%9%9%a#R#R%l%a%9%9%9%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9$-#t#4#4#4$.$.$.$-#x#x#y$.$4%9%9%9%9%9%9%9%9%9%9#E#g#g%i%9%9%9%9%a%/#R%l%_%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$?$?$?%9%9%9%9%9%9%3#x#4#4#4#9$.$.$-#4#4#4#x#y$.%9%9%9%9%9%9%9%9%9%9#E#G#g#r%9%9%9%<#g%/#E%l#E#O%9%9%9%9%9%0#V%9%9%9%9%9
%9%4$?$?$?$?$?%9%9%9%9%9%9%3#t#4#####)$.$.#t#4#4#4#8#x#y%9%9%9%9%9%9%9%9%9%9#E#G#g%a%i%9%9%<#g%/#E#E#E#O%9%9%9%9%0%8#^${#_%9%9%9%9$?$?$?$?$?$?$?%9%9%9%9%9$-#t#3####$.$.$-#4#4#4#4#8#x#x$$%9%9%9%9%9%9%9%9%9#E#E#r#G#r%9%9$G#d#5#E#E#E#J$D%9%9%9#^#^#v#n${%0#_%9
%4$?$?$?$?$?$?$?$?$?%9%9%9#x#x#3#)$.#y$.$-#4#4#4#4#8#x#x$$%9%9%9%9%9%9%9%9%9#E#E%_#g#g%l%l%l#d#4#E#E%_$?$?$D%9#_#^#W${#x${%(#V%9%9%9%9%9%9%4$?$?$D%9%9%9%9#y$-#y$-#x#8$.#x#4#3#####8#x#x$$%9%9%9%9%9%9%9%9%9%/#R#G#g#g%a#g#g$G%^#4#4#E$?$?$?$;${$|#^#f#h#f#n#_%9
%9%9%9%9%9%9%9%9%9%9%9%9%9#y$.$.#t#4#8$.#x#4#######x#x#x#x%9%9%9%9%9$?$?$?$?$:#e#E#g#r#G#g#j%4#g%l%l#:$?$?$?$<#f#f#W#^#v#w#v#g%9%9%9%9%9%9%9%9%9%9%9%9%9%9#x$.$.#t#4#8$.$-#t#######x#x#x#x%9%9%9%9$?$?$?$?$?#7#E#E#G%_#j$?#g$D$?$<#g%_#E#D#@%l%a#w#x#h#w#x#r%l%j
#4#4#4#4#4#4#4#4#4#4#4#4#4#x#x#x#4#t#8$.$-#x#x###(#x#x#x#x#4#4#4#4#4#4#:$?#4#4#E#E#R#E$?$?$?%I%9$?#g#e#E#E#4%J%L%l%l%l%l%l%j%J%>#4#4#4#4#4#4#4#4#4#4#4#4#8#x#x#x#x#t#8#y$.$.#x#x#x#x#x#x#x#t#4#4#4#4#4#4#4#4#4#E#E%_$?$D%9$?$E%I%9$?$?$?$?#4#4#@%J%J%J%J%J%>#4#4
#4#4#4#4#4#4#4#4#4#4#4#4#z#z$=#x#x#t#x#y$.$.$-#x#x#x#x#x#x#t#4#4#4#4#4#4#4#:$?$:#R#J$D%9%9%4$?$E%I%9%9%9%9$?$9#4$9#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4$?#z$-$?#x#x#x#x#x$.#x#x#x#x#x#x#x#z$?#4#4#4#4#4#:$D%9%9%/#J%9%9%9%9%9%4$E%9%9%9$?%9%4$?%4%9$9#4#4#4#4#4
#4#4#4#4#4#4#4$?$9#4#4#4$5$.%C$/#x#x#x#x#x#x#x#x#x#x#x#x#x$D%9$9#4#4#4#:$D%9%9%9%9$?%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%:#4#4#4#4#4#4#4#4#4#4#4#4#y$?$?#4#4#4%J%J%J$-#z$=#x#x#x#x#x#x#x#x#x#x#x$D%9$9#4#4#4$D%9%9%J%J%D$D%9%:%J%J%J%9%9%9%9%9%9%9%4%9$E%J#4#4#4#4#4#4
#4#4#4#4#4#4#y#x#x#4$9#4#:$9%J%J$.$=#x#x#x#x#x#x#x#x#z$?$?%9%9%4#4#4#:%9%9%:%>#4$D%9%:%D$?$?%9%I%9$?$?$D%9%9%4%9%J#4#4#4#4#4#4#4#4#4#4#4#4#4#x#x#t#9$?#4#9$?#4%J%C#x$?#x#x#x#x#x#z$?$D%9%9%9%9%9#4#4%9%:%9%J#4#:%9%:%D$?%9%9%9%9$?$D%9%9%9%9%9%J%>#4#4#4#4#4#4#4
#4#4#4#z$?#8#x#x$$#y$.$?#8$/$9%I%:%C$/#x#x#x$?#z$D%9%9%9%9%9%9%9#4#@%9%I%J%>#4#:%9%J$?%9%9%9%9%9%9%9%9$D%9%9%:%>#4#4#4#4#4#4#4#4#4#4#:$?#x%2$.$-%9#y$.$/$=#z$>#@%I%:$=#x#z$?%9$D%9%9%9%9%9%9%9%:#4#@%:%:%>#4#4#4%J#:$D%9%9%9%9%9%9%9$?%9%9%:%J#4#4#4#4#4#4#4#4#4
#4#4$?$-#x%9$/$>%9#x$.$.$=#y$?#t#@%9%D#z$D%9%9%9%9%9%9%9%9%J%J%>#4#4%J%:#4#4#4#4#:$D%9%9%9%9%9%9%9%4%9%9%:%J#4#4#4#4#4#4#4#4#4#4#4#:$>#x$$%2$.$>$%%B#y$.$/#x$/#t#@%9%9$D%9%9%9%9%9%9%9%:%J#4#4#4#4#4#4%>#4#4#4#:$?%9%9%9%9%9%:%J%D$D%9%9%J#4#4#4#4#4#4#4#4#4#4#4
#4$.$-#x$%%B#y$?$-#8#x$.$/$=$.$-#4%I%9%9%9%9%9%:%J%9%9%4#4#4#4#4#4#4#4#4#4#4#4$?$?%9%9%9%:%J%>#4#:%9%9%:%>#4#4#4#4#4#4#4#4#4#4#4#4$.#x#x#4#8#y$/$-#8#x#y$.$9#y$-#4#@%J%I%9%9%:%>#4%J%I%9$9#4#4#4#4#4#4#4#4#4#:$?%9%9%9%9%J#4#4#4#:%9%9%>#4#4#4#4#4#4#4#4#4#4#4#4
#:#x#x#z#4#8#x$/$>#t#x#y$.$.#y$.#t#4#4#@%9%9%>#4#4#4#@%J%4$9#4#4#4#4#4#4#4#4#:$D%9%9%9%J#4#4#4#4$D%9%:%>#4#4#4#4#4#4#4#4#4#4#4#4$D$?$=$$$?#4#x$.$>$)#8#x$.$.#y$.#t#4#4#4%I%J#4#4#4#4#4#4%I%:#4#4#4#4#4#4#4#4$?$D%9%9%J#4#4#4#4#4#:%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4
%9%9$?$D%9%>#x#y$/$-#8#x$.$.#y$.#t#4#4#4#@#4#4#4#4#4#4#4#@%>#4#4#4#4#4#4#4#4$?%9%9%J#4#4#4$9#4#4#:%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4%I%9%9%9%9%>#8#x$/$.#8#x#y$.#x#x#z$?$9#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:$?%9%:%>#4#:$?%:#4#4$D%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4
%I%9%4$E%J#4#4#x$.$.#4#x#x$.%2#x$$%9%4%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:$D%:%>#4#4$D%9%:#4#4$D%9%J#4#4#4#4#4#4#4#4#4#4#4#4#4%I%9%9$?#4#4#4#x$.$.#t#x#x$.$$$?$D%9%9%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:%J%>#4#4#:%9%9%>#4#4$D%9%>#4#4#4#4#4#4#4#4#4#4#4#4#4
#@%I%9%9$9#4#4#8#y$.$)#8#x$.$%%I%9%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4%>#4#4#4$D%9%:#4#4#4#@%I%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#@%9%9%>#:$?$?#y$.$-$=#x#x#z#@%9%J%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:%9%9%>#4#4#4#4#@%>#4#4#4#4#4#4#4#4#4#4#4#4#4
#4#4%J%J#4$D%9%9#x$.$-%2#x#z$D$?$E%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4$?%9%9%9%4#x$$%9$?$?%9%9%9%.#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4
#4#4#4#:%9%9$?%9%9$?$D%9%9%9%9%9%9%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:%9%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:%9%4%9%9%9%9%9%9%9%9%9%9%9%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:%9%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4
#4#4#4#?%9%I%9%:%J$?$D%9%9%9%:%J%J#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4%J#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#@%:#@%9%>%D%9%9%9%9%J%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4
#4#4#4#4%>#@%:#4#@%J%9%9%J#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4%>#4#4#@%:%J#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4
#4#4#4#4#4#4#4#4#4#4%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%<%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%a%9%9%9%9%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%<#g%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%<#o%9%9%9%9%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%(%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%a#o%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9#^%(%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$?%9%9%9%9%9%a%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%0%(%i%9%9%9%9%9%9%4$D%9%9%9%9%9%9%9%9%9%9%9%9$?$?$?$?$?$?$?$?$?$?%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%0#^%,%9%9%9%9%9$?$?$?$?$D%9%9%9%9%9%9%9%9%9%4$?$?$?$?$?$?$?$?$?$?$?$?$?$.$.%9%9%9%4$?$?%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9#V%(%i%9%9%9$?$?$?$?$?$?%9%9%9%9%9%9%9%9%9$?$?$?$?$?$?$?$?$?$?$?$?$?$?$>$-%9%9$?$?$?$?$D%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%(%(%)%0%(%)%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%4$?$?$>$/$?$?$?$>$/$?$?$?$?$?$.$$%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%(%(%9%0#^%(%`#^%)%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%4$>$?$.$.$?$>$?$.$/$?$?$?$?$>$-%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%8#V#^%(%9#v%(%(%l%h%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$?$.$?$.#z$?$.$.$-$?$?$?$?$?$.$$%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9#x#x#V%(%)%2#V%(%(%,%)%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$>$-$?$-%4$>$.$.$$$?$>$?$?$>$.$$%9%9%9%9%9%9%9%9%9$?$?$D%9%9%9%9%9%9%9%9%9%9%9#v#^%(%2#V#^%(%(%h%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9$>$$$>$-$?$.$.$-#o$?$.$?$?$.$.$$%9%9%9%9%9%9%9%9
%4$?$?$?$?$D%9%9%9%9%9%9%9%9%9%2#V%($}#v#V%(${%(%(%9%(%h%9%9%9%9%9%9%9%4$?$?$D%9$-%9$>$$$>$.#y$'#o$?$.$?$.$.$-%9%9%9%9%9%9%9%9%9$?$?$?$?$?$?%9%9%9%9%9%9%9%9%8$}#v#^%(#x#V#^${#^%,%(%(%,%h%)%9%9%9%9%4$?$?$?$?%9%9%9$.$$$-#x%2%a%9$>$.$?$.$.$-%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%8${#V#x#V#V#X#v#V#V#^%`%,%(#V#b%(%9%9%9%4$?$?$?$?$?$?$D%9$-%9$$%9%9%a%9%3$-$?$.$.$$%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9#X#x#x#x#x#v#X#x#V#V#^%,#^%`#V#V#b%)%9%9%9%9%9%9%9%9%9%9%9$$%9%9%9%<#g%9%2$-$?$.$-%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%0#x%9%9%9%9#x#V#x#v#V#V%(#^%,#_%9#V%(%9%9%9%9%9%9%9%i%6$t$b%6$t%9%9%<#o%9%9$$%4$.$$%9%9$?$?$D%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9#x#x#x#x#V${#V%(%i%9%9%0%9%9%9%9%9%9%9#R$`$B$P$`$A%9%9%<#o%9%9%9%2#x%9$?$?$?$?$?$?$D%9%9%9
%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%2#x#x#x$$#v$}#V#^%)%9%9%9%9%9%9%9%9%9%9#G%e$Q$b$P$`$t%9%<#o%9%9%9%9%9%9%9%9%9$?$?$?$?$D%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9#E#G#h#x$$#v%)#v#^%)%9%9%9%9%9%9%9%9%9%9#G#r$`$O$b$P$b%9%<#o%9#g#o%9%9%9%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9%9%4$?$?$?$D%9%9%9#G#G#g#x%9#^#_%2#V%)%9%9%9%9%9%9%9%9%9%9#G#g%e$P$b$P$`$t%a#e%/%l#g%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%.#@%J%J%J%D$D%9%/#E#g#l#l%9#V%9%9#v%)%9%9%9%9%9%9%9%9%9%9#E#g#r$Q$O$a$O$d%a#R#R%l%a%9%9%9%9%9%9%9%9%9%9%9%9%9
%9%9%9%9%9%9%9%9%9#@%J%J%J%J%J%D%9%/#E#g$\$d%9%9%9%9%0%9%9%9%9%9%9%9%9%9%9%9#E#g#g%e$a$`$a$t%a%/#R%l%_%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%4$D%9%9%9%.%J%J%J%J%J%J%J$D#E#e#g#l%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9#E#G#g#r$P$A$O$g#g%/#E%l#E#O%9%9%9%9%9%0#V%9%9%9%9%9
%9%9%4$?$?$?%9%9#@%J%J%J%J%J%J%J%D#E#G#l#l%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9#E#G#g%a%e$Q$Q$W#g%/#E#E#E#O%9%9%9%9%0%8#^${#_%9%9%9%9%4$?$?$?$?$D%9#@#4%J%J%J#4#4%J%J#E#G#g$d%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9#E#E#r#G#r$`$`$G#d#5#E#E#E#J$D%9%9%9#^#^#v#n${%0#_%9
%9%9%9%9$?$?$?%.#@#$#@%J%>#4#4%J%J#G#G#l%9%9%9%9%9%9%9%9%4$?$?%9%9%9%9%9%9%9#E#E%_#g#g%l%l%l#d#4#E#E%_$?$?$D%9#_#^#W${#x${%(#V%9%9%9%9%9%9%9%9%.#@###@%J%=#4#@%J%?#E#l#l%9%9%9%9%9%4$?$?$?$?$?$D%9%9%9%9%9%9%/#R#G#g#g%a#g#g$G%^#4#4#E$?$?$?$;${$|#^#f#h#f#n#_%9
%9%9%9%9%9%9%9%.#@###P%J%=#D%J%J%?#E#g$d%9%9%9%9$?$?$?$?$?$?$?$?%9%9$?$?$?$?$:#e#E#g#r#G#g#j%4#g%l%l#:$?$?$?$<#f#f#W#^#v#w#v#g%9%9%9%9%9%9%9%9%.#@%=#@%J%=#0%J%J#E#E#g$_$D%9%9%9%9%9%9%9%9%9%9%9%9$?$?$?$?$?#7#E#E#G%_#j$?#g$D$?$<#g%_#E#D#@%l%a#w#x#h#w#x#r%l%j
#4#4#4#4#4#4#4#3#@%J%J%J%J%J%J%J#E#e$\$e%D#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:$?#4#4#E#E#R#E$?$?$?%I%9$?#g#e#E#E#4%J%L%l%l%l%l%l%j%J%>#4#4#4#4#4#4#4#3#4%J%>%J%J%J%J%J#E#G#l%J%J$?#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#E#E%_$?$D%9$?$E%I%9$?$?$?$?#4#4#@%J%J%J%J%J%>#4#4
#4#4#4#4#4#4#4#3#$#@%>#@%J%J%J%?#G#G$e%J%J%J$9#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:$?$:#R#J$D%9%9%4$?$E%I%9%9%9%9$?$9#4$9#4#4#4#4#4#4#4#4#4#4#4#4#4#4#3###4#4#@#4%J%J#5#E#g$e%J%J%J%D$9#4#4#4#4#4#4#4#4#4#4#4#4#:$D%9%9%/#J%9%9%9%9%9%4$E%9%9%9$?%9%4$?%4%9$9#4#4#4#4#4
#4#4#4#4#<$a$a$X###$#4#4#4#@%>#5#E#G$e%J%J%J%J%D$9#4#4#4#4#4#4#4#4#4#4#:$D%9%9%9%9$?%9%9%9%9%9%9%9%9%9%9%9%9%9%9%9%:#4#4#4#4#4#4#4#4#4#4$g%l%l%l#####3#$#3#4#4#%#e#L#@%J%J%J%J$?%D#<$a$a#4#4#4#4#4#4#4$D%9%9%J%J%D$D%9%:%J%J%J%9%9%9%9%9%9%9%4%9$E%J#4#4#4#4#4#4
#4#4#4#<%l%l%l%l%]#####$#3#####E#G#L#$#@%J%J%D$E%J#2%l%l$Y#4#4#4#4#4#:%9%9%:%>#4$D%9%:%D$?$?%9%I%9$?$?$D%9%9%4%9%J#4#4#4#4#4#4#4#4#@%F$g%l%l%l%l#g#############E#E#l###4#4%J$?%J%J%=%l%l%f$Y#4#4#4#4%9%:%9%J#4#:%9%:%D$?%9%9%9%9$?$D%9%9%9%9%9%J%>#4#4#4#4#4#4#4
#4%J$g%l%a#g%l%a#g#c#########%#e#E#l#######:$E%J%J%=#r#g%l#l#4#4#4#@%9%I%J%>#4#:%9%J$?%9%9%9%9%9%9%9%9$D%9%9%:%>#4#4#4#4#4#4#4#4#@%A%l%l#r%l#r%l#g#g#########E#G#e#l#####*$?%J%J%J%=#g%l%l#g%J%>#4#@%:%:%>#4#4#4%J#:$D%9%9%9%9%9%9%9$?%9%9%:%J#4#4#4#4#4#4#4#4#4
#@%A%l%l%l%l%a%l%a#g#g#g#e#E#E#E#G#l##$?$E%J%J%J%J#'#r%l%a#g%J#@#4#4%J%:#4#4#4#4#:$D%9%9%9%9%9%9%9%4%9%9%:%J#4#4#4#4#4#4#4#4#4#4#@%A#r#g%l%l%l%a#r%l%a#g#E#E#E#E#E#l#*%J%J%J%J%J%=#g%l%l%a#p%>#@#4#4#4%>#4#4#4#:$?%9%9%9%9%9%:%J%D$D%9%9%J#4#4#4#4#4#4#4#4#4#4#4
#@%A#g%l#g%l%l%l%l#g#r#g#g#e#E#E#E#g#d##%J%J%=###'#r%l%a#g%J%>#@#4#4#4#4#4#4#4$?$?%9%9%9%:%J%>#4#:%9%9%:%>#4#4#4#4#4#4#4#4#4#4#4#@%A#g#r%l%l%a#g%l%l#g#g#g#g#g#E#E#r#g#g#####'#g#g%l%a#g%J%J%J%>#4#4#4#4#4#4#:$?%9%9%9%9%J#4#4#4#:%9%9%>#4#4#4#4#4#4#4#4#4#4#4#4
#@%J#g#g%l%a#r%l%l#g#g#g#g#g#g#g#g%l%l#g#g#g#r%l%l%l#p%J%J#4%>%>#4#4#4#4#4#4#:$D%9%9%9%J#4#4#4#4$D%9%:%>#4#4#4#4#4#4#4#4#4#4#4#4#@%J#g#g#g%l%l%l#g#g#g#g#g#r%l%l%l%l%l%l#r%l%l%l%a#g%J%J%J%J%>#@#4#4#4#4#4#4$?$D%9%9%J#4#4#4#4#4#:%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4
#4%J%J#g#g#g#g#g#g#g#p%J%J#g#g#g#g#r%l%l%a%l#g#g#g%J%>#4%J#4#@#4#4#4#4#4#4#4$?%9%9%J#4#4#4$9#4#4#:%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4#4%J%J%J#g#g#g%J%A#p%J#4%J%J%J%J%A#g#g#g#g#g#g#p%J%J%>#@%>#4#@#4#4#4#4#4#4#:$?%9%:%>#4#:$?%:#4#4$D%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4
#@#4%J%J%J%J%J%J%J%J%>#4#@%J%J%J%J%A#g#g#g#g#p%J%J%J%J%J#4#4%>#4#4#4#4#4#4#:$D%:%>#4#4$D%9%:#4#4$D%9%J#4#4#4#4#4#4#4#4#4#4#4#4#4#@#4#4%J%J%J%>#4#@%J%>%>#4%J%J%J%J%J#g#g#g#p%J%J%J#@%J%>#4#4%>#4#4#4#4#4#4#:%J%>#4#4#:%9%9%>#4#4$D%9%>#4#4#4#4#4#4#4#4#4#4#4#4#4
#@#4#4#@#4#@%>#4#4%J#4#@#4%J%J%J%J%J%A#g%J%J%>#4#4#4#@%>#4#4#4#4#4#4#4#4#4#4%>#4#4#4$D%9%:#4#4#4#@%I%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4%>#4#@#4#4%J#4#4#4#4#4%J%J%>#4#4%J%J%J%J%J#4#4#4#4%J#@#4#4#4#4#4#4#4#4#4#4#4#4#4#:%9%9%>#4#4#4#4#@%>#4#4#4#4#4#4#4#4#4#4#4#4#4
#4#4#4%>#4#@#@%J%>#4#4#@#4#4%J#4#4#4%>#4#4#@%>#@#4#@%>#4%>#4#4#4#4#4#4#4#4#4#4#4#4#:%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4%>#4#4#4%J#4#@#4#4#4#4#@%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:%9%:#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4
#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#@%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:%9%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4%J#4#4#4#4%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#:%9%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4
#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#@%J#4#@%J#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4%J#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4%J%>#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4
#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4
#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4#4
".Replace("\n", "").Replace("\r", "");

    public Dictionary<string, List<SongInst>> Music => new()
    {
        { "original", [
            new([("pcraft_og_cave_0", false), ("pcraft_og_cave_1", true)], 0),
            new([("pcraft_og_surface", true)], 1),
            new([("pcraft_og_cave_0", false), ("pcraft_og_cave_1", true)], 2),
            new([("pcraft_og_cave_0", false), ("pcraft_og_cave_1", true)], 3),
            new([("pcraft_og_cave_0", false), ("pcraft_og_cave_1", true)], 4)]
        },
        { "new!", [
            new([("pcraft_new_title", true)], 0),
            new([("pcraft_new_surface", true)], 1),
            new([("pcraft_new_cave", true)], 1),
            new([("pcraft_new_title", false), ("pcraft_new_title", true)], 2),
            new([("pcraft_new_death", true)], 3)]
        },
        { "pog edition", [
            new([("pcraft_pe_title_0", false), ("pcraft_pe_title_1", true)], 0),
            new([("pcraft_pe_surface_0", false), ("pcraft_pe_surface_1", true)], 1),
            new([("pcraft_pe_cave_0", false), ("pcraft_pe_cave_1", true)], 2),
            new([("pcraft_pe_win", false)], 3),
            new([("pcraft_pe_death", true)], 4)]
        }
    };

    public Dictionary<string, Dictionary<int, string>> Sfx => new()
    {
        { "original", new() {
            { 11, "pcraft_og_11" },
            { 12, "pcraft_og_12" },
            { 13, "pcraft_og_13" },
            { 14, "pcraft_og_14" },
            { 15, "pcraft_og_15" },
            { 16, "pcraft_og_16" },
            { 17, "pcraft_og_17" },
            { 18, "pcraft_og_18" },
            { 19, "pcraft_og_19" },
            { 20, "pcraft_og_20" },
            { 21, "pcraft_og_21" }}
        },
        { "soft", new() {
            { 11, "pcraft_soft_11" },
            { 12, "pcraft_soft_12" },
            { 13, "pcraft_soft_13" },
            { 14, "pcraft_soft_14" },
            { 15, "pcraft_soft_15" },
            { 16, "pcraft_soft_16" },
            { 17, "pcraft_soft_17" },
            { 18, "pcraft_soft_18" },
            { 19, "pcraft_soft_19" },
            { 20, "pcraft_soft_20" },
            { 21, "pcraft_soft_21" }}
        },
        { "pog edition", new() {
            { 11, "pcraft_pe_11" },
            { 12, "pcraft_pe_12" },
            { 13, "pcraft_pe_13" },
            { 14, "pcraft_pe_14" },
            { 15, "pcraft_pe_15" },
            { 16, "pcraft_pe_16" },
            { 17, "pcraft_pe_17" },
            { 18, "pcraft_pe_18" },
            { 19, "pcraft_pe_19" },
            { 20, "pcraft_pe_20" },
            { 21, "pcraft_pe_21" }}
        },
    };

    public void Dispose()
    {

    }

}
