using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Pcraft
{

    public abstract class PcraftBase : IScene, IDisposable
    {
        public abstract string SceneName { get; }

        public Pico8Functions? p8;

#nullable enable

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

#nullable disable

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
            Level l = new Level { X = xx, Y = yy, Sx = sizex, Sy = sizey, IsUnder = IsUnderground, Ent = [], Ene = [], Dat = new F32[8192] };
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

            return p8.Mod(p8.Mod(F32.Lerp(rot, grot, F32.FromDouble(0.4)), 1) + 1, 1);
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
                            e.Banim = p8.Mod(e.Banim, 8);
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

            F32 blade = p8.Mod(rot + F32.FromDouble(0.25), 1);
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

                (int my2, int mx2) = Mirror(p8.Mod(rot + F32.FromDouble(0.75), 1));
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

        public virtual string SpriteImage => "";

        public virtual string SpriteData => @"
00000000ffffffffffffffffffffffffffffffff44fff44ffff44fff020121000004200002031000fff55fffffff555ff5555fff000000000001000000101000
00000000ffffffffffffffffffffffffffff444fff4ffff4ff4fffff310310200303102041420000ff56655ffff56665f56665ff000100000011100001000100
00000000fff4fffff4ffffffff4fffff4444fffffff444ff44fff44f205200024002001030310410f566665ffff566655666665f001110000110110010000100
00000000ffffffffffffff4ffffffffffffffff4ff4fff44fff44ff415340401340100402020030256666665f551566515666665000100000011100000000000
00000000ffffffffffffffffffffff4fff44444fffffffffffffffff424243032300403410140201566666655665155115666665000000000001000001100100
00000000ffffffffff4fffffffffffffffffffffff44fff444fff44f313132021240302300034104156655515666511ff1566565000000000000000000010000
00000000ffff4ffffffffffffffffffff4444fff44ff444fff444fff002021404130201204023003f155511f56651ffff1565151000000000000000000000000
00000000fffffffffffffffffffffffffffff444ffffffffffffffff001010000020100003012040ff111fff1551ffffff151f1f000000000000000000000000
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
1533333335333335333333b13333333335153333f1556666666666666666dd1f6666666665566666441dddddddddddddddd11444dddddddddddddddd00000000
1533333351533351533333b1335333533353333315556dddddd66dddddd6ddd166655666d6666666f441dddddddddddddd14444fdddddddddddddddd00000000
f1533333351bbb1533333b1f35153515333335531555ddddddddddddddddddd166d66d6666666666ff441dddddddddddd144444fdddddddddddddddd00000000
f153333333b111b333333b1f5153335335335115155dddddddddddddddddddd166666666666666d6fff4411ddd1111ddd1444fffdddddddddddddddd00000000
ff1bbb33bb1fff1b33bbb1ff353353335153355315ddddddddddddddddddddd16666666666666556fff44441114444111444ffffdddddddddddddddd00000000
fff111bb11fffff1bb111fff3335153335153333fddddd1111dddd1111dddd1f55666666666d5666ffff444444ffff444444ffffdddddddddddddddd00000000
ffffff11ffffffff11ffffff3333533333533333ff1111ffff1111ffff1111ff66d6666d66666666ffffffffffffffffffffffffdddddddddddddddd00000000
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
024202000000200000002200000001410000111000001441144441000001fff10233333333333320001111565111100000066666666600000011111111111100
02442420000220000002341000001410000144410001444101111410001ffff4023333333333332001ddd1d1d1ddd10006666666677666660144444444444410
0244344422242020000244410001410000023441000234110002314101ffff41023333333333332001ddd15651ddd100166666666666666d0144999999994410
2434434442444242002324410014100000232141002321000023214119fff410023333333333332001ddd1d1d1ddd1001666666666666dd10149999999999410
24434344344444420232441002410000023201410232000002320141019f4100023333333333332001ddd15651ddd100156666666666dd100149999999999410
24434444434443202320110023200000232000102320000023200141001910000233333333333320011111d1d111110015566666666dd1000149999999999410
234444434433432032000000320000003200000032000000020000100001000002333333333333201dddd15651dddd100155666666dd10000149999999999410
023444434343322000555000000005000000400005555d5000022200000012200222222222222220155551555155551001556666ddd100000144999999994410
02324443432220000511150000505b50001242205000d6d50123432000012342055555555555555015111111111115101155556dddd400000144444444444410
00202344320000005111115005b5b735012242e2500d676d1234343200123432055555555555555015191a181a1915101445555ddd2410000155559999555510
0000023444200000511111155b73535012282efe5000d6d50123434201234321051000000000015015121812181215101412555dd21441000154445445444510
0000002433200000511111150535b5001288efe250000d051234332112343210051101010101015015115111115115100101255d210144100154445555444510
000012333211000005111115005b735001288e825000000512332232123321000510101010101150155161555161551000001242100014100154444444444510
00012222222210000055115000053500001288205000000501221121012210000511010101010150115515555515511000000141000001000155555555555510
00001111111100000000550000005000000122000555555000110010001100000510101010101150011111111111110000000111000000000011111111111100
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccdddddddddccccccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccdddd222222222dddcccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccdd2222111111111222dccccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccd2211111000000001112ddccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccd2110000000000000000122dccdddccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccd21000000000000ddddddd112dd222ddccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccd2100000000000dd22222222212d11122dcccccccccccccccccccc
ccccccccccccccccccccccccc777cccccccccccccccccccccccccccccccccccccccccccccd2100000000000d22111111111012d01112dccccccccccccccccccc
ccccccccccccccccccccc777777777cccccccccccccccccccccccccccccccccccccccccccd110000000000d2111000000000012d0012ddcccccccccccccccccc
cccccccccccccccccc7777777777777cccccccccccccccccccccccccccccccccccccccccccdd111000000d211000000000000012d00122dccccccccccccccccc
cccccccccccc77777777777777777777777cccccccccccccccccccccccccccccccccccccccccddd10000000000000000000000012d00dd2dddcccccccccccccc
cccccccc7777777777777777777777777777cccccccccccccccccccccccccccccccccccccccccccd111100000000000000000000120d221222dddccccccccccc
ccccccc7777777cccccccccccccccccccccccccccccccccccccccccccccccccccccccccddcccccdddddd1000000000000000000001d2210111222ddccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccd22dcccd222222d10000000000000000000d211000001112ddcccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccc7cccccccccccccccccccd2112ddd21111112d100000000000000000d210000000000122dccccccc
cccccccccccccccccccccccccccccccccccccccccccc7777777cccccccccccccccccd21001222100000012d10000000000000001d1000000000000112dcccccc
cccccccccccccccccccccbccccccccccccccccc7777777777777777ccccccccccccd2100001110000000012d1000000000000000100000000000000012ddcccc
ccccccccccccccccccbbb3bccccccccccccc77777777777777777777777cccccccd210000000000000000011000000000000000000000000000000000122dccc
ccccccccccccccccccb333bccbbccccccc77777777777777777777777777ccccccd1100000000000000000000000000000000000000000000000000000112dcc
cccccccccccccbbbbc333332cb3bdccccccccccccccccccccccccccccccccccccccd1000000000000000000100000001000000000000000000000000000012dc
c7777ccccccccb333cc3cb34233b5dddcccccccbbbbcccccccccccccccccccccccccd100110000000000001d1000001d100000000000000000000000000001dc
7777777cbbbbc33334bbb34b4235bb55dccccccb333bccccccccccccccccccccccccd110011000000000001d10000001d10000000010000000010000000001dc
c77777cb333bbc332433334bb42b33b55ddcccb333cccccccccccccccccccccccccccdd111110000000001d1d1000001d100000001d11111001d1000000001dc
ccccccbb33333b222b3332233b223335555dbbb242bbbbccccccccccbbbbcccccccccccdddd1100000011d111d11111d1d110000011ddddd11d10000100001dc
ccccbbb3333333c22b33422433b4bb3b5b55b3324b3333bccddddccb33bbbbcccccccccccccd1111111dd1711dddddd1d1dd11111dd111d1dd10000001001dcc
ccccbb3324333342b33444443344b33bb3b553344b333ccdd5555dccc3333bccccccccccccccddddddd1d1711d1d1711d1d1ddddd11d1d1711d110000011dccc
ccccb33342433422b33222444444b33333bb5bbb444cccd5555555dccc2423bccccccccccccc111d1111d171d11d171d11d1d1d11d1d1d171d1dd111111dcccc
cccccc3c2bb4424b33344422bbb2343b4bb3bb33442dcd555555555dc24423bccccccccccccc1d1d1d11d171d11d171d11d1d1d1d11d1d171d111ddddddccccc
cccccdddbb3242522222222b33342423bb3333324425dd5555555555d242ccbccccccccccccc1d1d1d1d1171d1d1171d1d11d1d1d1d11d171d11d1d1d1cccccc
ccccd555d2444252225dd55b32325224b33b52444425d55555555555d242cc3ccccccccccccc1d1d1d1d1711d1d1171d1d1d11d1d1d1d1171d11d1d1d10ccccc
cccf5555f224425555ddddbb244252425243b524242f5555555555555d42ccccccccccccccccc1d11d1d1711d1d1711d1d1d11d1d1d1d1711d1d11d1d10ccccc
dffffffffff2255555d55d535224224224423b52242555555555ffffff2fffddccccccccccccc1d1d11d171d11d1711d1d1d1d11d1d1d1711d1d1d11d10ccccc
1dffffffffffff5ffd5555dfff224f2f242ff3f2f2ffff55555ffffffffffffd11111111111101d1d1d1171d1d1171d11d1d1d1d11d1d171d11d1d11d1011111
11dddddddffffffff555555ffff2fffff2fffffffffffffffffffdddddddddd11111111111101d11d1d1711d1d1171d1d11d1d1d1d11d171d11d1d1d11011111
111111111ddddfffffffffffffffffffffffffffffffffffffffd111111111111111111110001d1d11d1711d1d1711d1d1d11d1d1d1d1171d1d11d1d11001111
1111111111111ddffffffffffffffffffffdddddfffffffddddd1111111111111111111110001d1d1d1171d11d1711d1d1d11d1d1d1d1711d1d1d11d11000111
111111111111111dddddffffffffffffffd11111ddddddd11111111111111111111111111001d11d1d1171d1d1171d11d1d1d11d1d1d1711d1d1100100000111
11111111111111111111dddddddddddddd111111111111111111111111111111111111111001d1d11d1711d1d1171d11d1d1d1d11d1d1711d1d1000000000111
1111111111111111111111111111111111111111111111111111111111111111111111110001d1d1d11711d1d1171d1d11d1d1d1d11d171d1110000000011111
111111111111111111111111111111111111111111111111111111111111111111111110000011d1d1171d11d1711d1d1d11d1d001d1171d1000000001111111
1111111111111111111111111111111111111111111111111111111111111111111111100000001011711d1d1171d11d1d1d10000000011d1000111111111111
1122221111111111222211122221111111222222111111222222211222222221111111100000000000101d1d1171d11101010000000000010011111111111111
12777721111111127777212777721111127777772111127777777227777777721111111000000000000001010171d00000000000000000000111111111111111
27cccc7211111127cccc227cccc7211127cccccc721127ccccccc22cccccccc21111111111000000000000000010100000000000000000001111111111111111
2cc22cc72111127ccccc22cccccc72127cccccccc7212cccccccc22cccccccc21111111111110000000000000000000000000000000000011111111111111111
2cc212cc721127cccccc22cc222cc722ccc2222ccc212ccc2222211222cc22211111111111111000000000000000000000000000000000111111111111111111
2cc2112cc2112cccc22212cc2112cc22cc211112cc212cc21111111112cc21111111111111111000000000000000000000000000000001111111111111111111
2cc2112cc2112ccc211112cc2112cc22cc211112cc212cc21111111112cc21111111111111111000000000000000000000000000000011111111111111111111
2ff2112ff2112ff2111112ff2112ff22ff211112ff212ff21111111112ff21111111111111111111110000000000000000000000000111111111111111111111
2ff2112ff2112ff2111112ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111100000000000000001111111111111111111111111111
2ff2127ff2112ff2111112ff2127ff22ff211112ff212ff21111111112ff21111111111111111111111111100000000000011111111111111111111111111111
2ff227fff2112ff2111112ff227fff22ff222222ff212ff22221111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff77fff21112ff2111112ff77fff212ff777777ff212ff77772111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ffffff211112ff2111112ffffff2112ffffffffff212ffffff2111112ff21111111111111111111111111111111111111111111111111111111111111111111
2fffff2111112ff2111112ffffff2112ffffffffff212ffffff2111112ff21111111111111111111111111111111111111111111111111111111111111111111
2fff221111112ff2111112ff22ff7212ff222222ff212ff22221111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff2111111112ff7211112ff222ff722ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff2111112112fff722212ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff2111127212ffff77722ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff211127f7212ffffff22ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
2ff21112fff2112fffff22ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
122111112f211112ffff22ff2112ff22ff211112ff212ff21111111112ff21111111111111111111111111111111111111111111111111111111111111111111
11111111121111112222112211112211221111112211122111111111112211111111111111111111111111111111111111111111111111111111111111111111
11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
".Replace("\n", "").Replace("\r", "");

        public virtual string FlagData => @"";

        public virtual string MapImage => "";

        public virtual string MapData => @"
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccfccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc4fcccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccfc44ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccfcc4cccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc4fc4cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc77cccccccccc4fcccccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc77777777777777777777cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc7c777777777777777777777777776666cccccc7c7777cc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc77777777777777777777777777776756cccc77777777c7cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc7c777767767777776776777777777766c5cccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc7c677766667767776676777777776756cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc77cccccccccccccccccccccccccccccc776677667577666656777777777766c5cccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccc777777777777cccccccccccccccccccccccccc675677567c676666c5776777776766c5cccccccccccccccccccccccccc7777cccccccccccccccccccccc777777777777777777cccccccccccccccccccccccccc67c5675677666656c4776677776666c5cccccccccccccccc
cccccccc77777777cccccccccccccccc777777777777777777777777c7cccccccccccc7c7777c7cc56cc67c5676665f5c4776677666656cccccccccccccccccccccc7c777777777777cccccccccccccccccccccccccc77c7cccccccccccccccccccc7c77777777cccccc66c556555c4fcc676677666656cccccccccccccccccc
cccccccccccccccc7cc7cccccccccccccccccccccccccccccccccccccccccccccc7c777777777777c7cc56ccc5cccc4fcc6c56776666c5ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc5ccccccfc44cc5c56776656cccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccfccccccccccccccfcc4ccccc57c66c5cccc7777c7cccccccccccccccccccccccccccccccccccccccccccc66666666666666c6ccccccccccccccccccccccccccf2ccccccccccccccfcc4cccccc5c55cc777777777777c7cccccc
cccccccccccccccccccccccccccccccc666677777777776666cccccccccccccccccccccccccc42cfccccccccccccfcc4cccccccccccccccccc77777777c7cccccccccccccccccccccccccccccccccc6c66666677777766666666cccccccccccccccccccccccc42f4ccccccccccccfcc4cc44c4cccccccccccccccccccccccccc
cccccccccccccccccccccccccccccc6656556566777766666666c6cccccccccccccccccccccc4244cfcccccccccc4f242cff44cccccccccccccccccccccccccccccccccccccccccccccccccccccc6c561111116666666666666666cccccccccccccccccccccc2244f4cccccccccc4ff2f2ff4fcccccccccccccccccccccccccc
cccccccccccccccccccccccccccc56151111116666665655556566c6cccccccccccccccccccc224444cfcccccccc4f2cf2ff2fcccccccccccccccccccccccccccccccccc777777cccccccccccc6c5511111161666656111111556566cccccccccccccccccccc224244f4ccccccfc442c22ff22c2cccccccccc3c33cccccccccc
cc7c7777777777cccccccccccc6c1511000060666615111111515565cccccccccccccccccccc2242444fcfccccfc442c222222c2cccccccc3cbcb33bc3cccccccc77777777777777cccccccccc561501000066665611111111515555c5cccccccccccccccccc2222f442f4ccccf7142122222272c7ccccccb3b335b43b3cc3cc
7c777777777777777777cccccc555501606665665611111111515555c5cccccccccccccccccc22222f4444ffffff141122222f7777c7ccc3b3433b553bbb33cccccccccccc7c7777c7cccccccc655665565551665511010000515555c5cccccccccccccccccc2cf24244444f4444f71f111122777777373b4bb3345434b4c3cc
cccccccccccccccccccccccccc65666615115166551100000055555555cccccccccc7777777727242244f44244747c44ffff7177777747343443b335453544cccccccccccccccccccccccccccc55666615115166561500000055555555cccccccc7777777777412222422f747744c77747442f2212d1ff4f4555544555f4ffdf
1111111111111111111111111155555511155166565555005055555555111111111111717711112222f222777777cdcc774424222211ddfdffffffffffdfdd1d11111111111111111111111151555555551551656666555555555555551511111111111111111122222f77c7cc77d7cdcc777777771111d1dddddddddd1d1111
11111111111111111111111175755755551555656666565555555555551511111111111111717727f272c7cccc7c77d7cdcccccccc771711171111111111111111111111111111111111111177755677555555555566555555555555557577111111111171c7cccc2c72cccccccccc7cd7cccccc77cc7c777ccc171111111111
111111111111117717111111d6666d7655555555555555555555555555c7cc1711111171c7cccccccc77ccccccccccccccccccccccccccccccdc111111111111111111111111657777111111dddddd5675575555555555555555555555c7cc17111111c7ccccdddd7dc7ccdcddddddcccccccccccccc7cccd7dd111111111111
1111111111116555551117117117dddd66575555555555555555757777cccc7c111171ccccdc1d11c7ccdc7d7777cccdcc7777c7cccc7cccdd11111111111111111111111111555515617711617711dd6d557755555555557577c7cccccccccc1111ccdcccdd1171ccdc7d77cccccccc77c7ccccccccccdd1d11111111111111
1111117577515555c5656677517617cddc6d765555557775c7cccccccccccccc11d1cccddd1d1171ccdd77ccccccccccccccccc7ccccdc1d111111111111111111117177555c6656cc656676577567d1cddc57557577ccc7ccccccccccccccdc11d1dcdc1d111111dd71c7cccccccccccccc77ccccdcdd111111111111111111
1111775655cc7667cc55666657657715d1cc7d75c7ccccccccccccccccdddd1d1111dddc1111111171c7cccccccccccccc7cccccdcdd1111111111111111111111716755c55c6667d55d656676557615d1ccccc7ccccccccccccccdcdd1111111111111d1111117177ccccccccccdcdd7dc7ccccdd1111111111111111111111
11665655d55d6577565155667657665611cdccccccccccdcddcccc7c11111111111111111111117777ccccccdcdd1d1171ccccdc1d11111111111111111111111166555511516576565155656617655611d1ddcdccccdc1d11ddcdcc171111111111111111117177ccccccccdd11111171cccc1d111111111111111111111111
71555575115155766715556566666566151111d1cccc1d111111d1dd7c17111111111111111171c7ccccccdd11111111c7ccdc1d111111111111111111111111c77757c577115566671651556666656615111111cddd111111111111cddc111111111111111177c7ccccdd111111111171ccdc11111111111111111111111111
cccc77c7cc1d5565765651556666656615111111d111111111111111d11d111111111111111177ccccdd11111117111171ccdc11111111111111111111111111cdcccccccc1d515576665155656655557577171111111111111111111111111111111111117177ccdc1d117177dc1111c7ccdc11111111111111111111111111
cdcc7cd7dd1111556666115555665c55c5cc7c1d111111111111111111111111111111111171c7dc1d1111c7ccdc1111c7ccdd11111111111111111111111111cdcccc7711111155666615555566c577c7cccc1d111111111111111111111111111111111171dd1d111171cccc1d1111c7cc1d11111111111111111111111111
d1cdcccc17111151656616515566d5cdccccdc111111111111111111111111111111111111111d111111c7ccdc111111d1cd1d1111111111111111111111111111d1cccc1d71777765665657555575d1ccdd1d1111111111111111111111111111111111111111111171cccc1d11111111d11d11111111111111111111111111
1111dddd11c7cccc5566565c5575c777d71d111111111111111111111111111111111111111111111171ccdc11111111111111111111111111111111111111111111111177cccccc7c55c5cc7777cccccc1c111111111111111111111111111111111111111111111171ccdc1111111111111111111111111111111111111111
11111171cccc77cccc77c7cccccccccccc1d111111111111111111111111111111111111111111111171cc1d111111111111111111111111111111111111111111111171cc7ccccccccccccccccccccccc1d111111111111111111111111111111111111111111111171cc1d1111111111111111111111111111111111111111
111111c1cccdccdcdd77c7ccccccdcdddd11111111111111111111111111111111111111111111111111dd111111111111111111111111111111111111111111111111d1dcd1cc1d7dccccccccdd1d11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
111111111dd1dc11d1ddccccdd1111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111d1111d1dcdd11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
111111111111111111111d111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccddddddddcdccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccdddd22222222d2ddcccccccccccccccccccccccccccccccc
ccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccdd2222111111112122cdccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccdc221111010000001011d2cdcccccccccccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc2d11000000000000000021d2ccddcdccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccdc12000000000000dddddd1d21dd22d2cdcccccccccccccccccccc
cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc2d010000000000dd2222222212d21121d2cccccccccccccccccccccccccccccccccccccccccccc7c77ccccccccccccccccccccccccccccccccccccccccccccdc120000000000d0221111111101210d1121cdcccccccccccccccccc
cccccccccccccccccccc7c77777777ccccccccccccccccccccccccccccccccccccccccccdc1100000000002d11010000000010d20021ddcccccccccccccccccccccccccccccccccccc777777777777c7ccccccccccccccccccccccccccccccccccccccccccdd11010000d01201000000000000210d1022cdcccccccccccccccc
cccccccccccc7777777777777777777777c7ccccccccccccccccccccccccccccccccccccccccdd1d000000000000000000000010d200ddd2ddcccccccccccccccccccccc7777777777777777777777777777ccccccccccccccccccccccccccccccccccccccccccdc11110000000000000000000021d0222122ddcdcccccccccc
cccccc7c777777ccccccccccccccccccccccccccccccccccccccccccccccccccccccccdccdccccdddddd01000000000000000000102d12101122d2cdcccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc2dd2ccdc2222221d000000000000000000d0120100001121ddcccccccc
cccccccccccccccccccccccccccccccccccccccccccccccc7cccccccccccccccccccdc1221dd2d111111d20100000000000000002d01000000001022cdcccccccccccccccccccccccccccccccccccccccccccccccccc777777c7cccccccccccccccc2d01102212000000211d00000000000000101d00000000000011d2cccccc
ccccccccccccccccccccbccccccccccccccccc7c77777777777777c7ccccccccccdc120000110100000010d20100000000000000010000000000000021ddccccccccccccccccccccccbb3bcbcccccccccccc7777777777777777777777c7cccccc2d01000000000000000011000000000000000000000000000000001022cdcc
cccccccccccccccccc3b33cbbccbcccccc77777777777777777777777777cccccc1d01000000000000000000000000000000000000000000000000000011d2ccccccccccccccbcbbcb333323bcb3cdccccccccccccccccccccccccccccccccccccdc0100000000000000001000000010000000000000000000000000000021cd
7c77c7ccccccbc33c33cbc4332b3d5ddccccccbcbbcbcccccccccccccccccccccccc1d0011000000000000d1010000d1010000000000000000000000000010cd777777c7bbbb3c3343bb3bb42453bb55cdccccbc33b3cccccccccccccccccccccccc1d0110010000000000d1010000101d0000000001000000100000000010cd
7c7777bc33b3cb33423333b44bb2335bd5cdcc3b33ccccccccccccccccccccccccccdc1d111100000000101d1d0000101d000000101d111100d10100000010cdccccccbb3333b322b2332332b322335355d5bb2b24bbbbccccccccccbbbbccccccccccdcdd1d01000010d111d11111d1d111000010d1dddd111d0000010010cd
ccccbb3b3333332cb2332442334bbbb3b5553b23b43333cbdcddcdbc33bbbbccccccccccccdc111111d11d17d1dddd1d1ddd1111d11d111ddd0100001000d1ccccccbb33423333243b43444433443bb33b5b3543b433c3dc5d55d5cc3c33b3ccccccccccccccdddddd1d1d17d1d171111d1ddddd1dd1d171111d01000011cdcc
cccc3b33243443223b23224444443b3333bbb5bb44c4cc5d555555cdcc4232cbcccccccccccc11d111111d171dd171d1111d1d1dd1d1d171d1d11d1111d1ccccccccccc3b24b24b433434422bb2b43b3b43bbb3344d2dc55555555d52c4432cbccccccccccccd1d1d1111d171dd171d1111d1d1d1dd1d171d111d1ddddcdcccc
ccccdcddbb232425222222b233434232bb3333234452dd55555555552d24cccbccccccccccccd1d1d1d111171d1d71d1d1111d1d1d1dd171d1111d1d1dcccccccccc5d552d44242522d55db5232325423bb3254444525d55555555552d24ccc3ccccccccccccd1d1d1d171111d1d71d1d1d1111d1d1d1d71d1111d1d1dc0cccc
ccfc55552f42245555ddddbb4224252425345b4242f2555555555555d524cccccccccccccccc1c1dd1d171111d1d17d1d1d1111d1d1d1d17d1d1111d1dc0ccccfdffffffff2f5255555dd535254222244224b325425255555555fffffff2ffddcccccccccccc1c1d1dd171d1111d17d1d1d1d1111d1d1d17d1d1d1111dc0cccc
d1fffffffffffff5df5555fdff22f4f242f23f2f2fffff5555f5ffffffffffdf111111111111101d1d1d71d1d111171dd1d1d1d1111d1d171dd1d1111d10111111ddddddfdffffff5f5555f5ff2fffff2fffffffffffffffffffdfdddddddd1d111111111101d1111d1d17d1d111171d1dd1d1d1d1111d171dd1d1d111101111
11111111d1ddfdffffffffffffffffffffffffffffffffffffff1d1111111111111111110100d1d1111d17d1d171111d1d1dd1d1d1d111171d1dd1d111001111111111111111d1fdffffffffffffffffffdfddddffffffdfdddd111111111111111111110100d1d1d111171dd171111d1d1dd1d1d1d171111d1d1dd111001011
11111111111111d1ddddffffffffffffff1d1111dddddd1d11111111111111111111111101101dd1d111171d1d71d1111d1d1dd1d1d171111d1d01100000101111111111111111111111dddddddddddddd1111111111111111111111111111111111111101101d1dd171111d1d71d1111d1d1d1dd1d171111d1d000000001011
11111111111111111111111111111111111111111111111111111111111111111111111100101d1d1d71111d1d71d1d1111d1d1d1dd171d111010000001011111111111111111111111111111111111111111111111111111111111111111111111111010000111d1d71d1111d17d1d1d1111d0d101d71d10100000010111111
111111111111111111111101111111111111111111111111111111111111111111111101000000011117d1d111171dd1d1d10100000010d10100111111111111112222111111111122221121221211111122222211111122222212212222221211111101000000000001d1d111171d1110100000000000100011111111111111
217777121111112177771272772711112177777712112177777727727777772711111101000000000000101010170d000000000000000000101111111111111172cccc2711111172cccc22c7cc7c121172cccccc271172cccccc2cc2cccccc2c1111111111000000000000000001010000000000000000001111111111111111
c22cc27c121121c7cccc22cccccc2721c7cccccc7c12c2cccccc2cc2cccccc2c1111111111110000000000000000000000000000000000101111111111111111c22c21cc271172cccccc22cc22c27c22cc2c22c2cc12c2cc2222122122cc22121111111111110100000000000000000000000000000000111111111111111111
c22c11c22c11c2cc2c2221cc1221cc22cc121121cc12c22c1111111121cc12111111111111110100000000000000000000000000000010111111111111111111c22c11c22c11c2cc121121cc1221cc22cc121121cc12c22c1111111121cc12111111111111110100000000000000000000000000000011111111111111111111
f22f11f22f11f22f111121ff1221ff22ff121121ff12f22f1111111121ff12111111111111111111110000000000000000000000001011111111111111111111f22f11f22f11f22f111121ff1221ff22ff121121ff12f22f1111111121ff12111111111111111111111100000000000000001111111111111111111111111111
f22f21f72f11f22f111121ff1272ff22ff121121ff12f22f1111111121ff12111111111111111111111111010000000000101111111111111111111111111111f22f72ff2f11f22f111121ff22f7ff22ff222222ff12f22f2212111121ff12111111111111111111111111111111111111111111111111111111111111111111
f27ff7ff1211f22f111121ff77ff2f21ff777777ff12f27f7727111121ff12111111111111111111111111111111111111111111111111111111111111111111f2ffff2f1111f22f111121ffffff1221ffffffffff12f2ffff2f111121ff12111111111111111111111111111111111111111111111111111111111111111111
f2ffff121111f22f111121ffffff1221ffffffffff12f2ffff2f111121ff12111111111111111111111111111111111111111111111111111111111111111111f2ff22111111f22f111121ff22ff2721ff222222ff12f22f2212111121ff12111111111111111111111111111111111111111111111111111111111111111111
f22f11111111f27f121121ff22f27f22ff121121ff12f22f1111111121ff12111111111111111111111111111111111111111111111111111111111111111111f22f11112111f2ff272221ff1221ff22ff121121ff12f22f1111111121ff12111111111111111111111111111111111111111111111111111111111111111111
f22f11117212f2ff7f7722ff1221ff22ff121121ff12f22f1111111121ff12111111111111111111111111111111111111111111111111111111111111111111f22f1121f72721ffffff22ff1221ff22ff121121ff12f22f1111111121ff12111111111111111111111111111111111111111111111111111111111111111111
f22f1121ff2f11f2ffff22ff1221ff22ff121121ff12f22f1111111121ff1211111111111111111111111111111111111111111111111111111111111111111121121111f2121121ffff22ff1221ff22ff121121ff12f22f1111111121ff12111111111111111111111111111111111111111111111111111111111111111111
1111111121111111222211221111221122111111221121121111111111221111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111
".Replace("\n", "").Replace("\r", "");

        public virtual Dictionary<string, List<SongInst>> Music => new()
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
                new([("pcraft_new_cave", true)], 3)]
            },
            { "pog edition", [
                new([("pcraft_pe_title_0", false), ("pcraft_pe_title_1", true)], 0),
                new([("pcraft_pe_surface_0", false), ("pcraft_pe_surface_1", true)], 1),
                new([("pcraft_pe_cave_0", false), ("pcraft_pe_cave_1", true)], 2),
                new([("pcraft_pe_win", false)], 3),
                new([("pcraft_pe_death", true)], 4)]
            }
        };

        public virtual Dictionary<string, Dictionary<int, string>> Sfx => new()
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

        public virtual void Dispose()
        {

        }

    }
}
