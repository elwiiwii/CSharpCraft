
namespace CSharpCraft
{

    public class Pcraft(Pico8Functions p8) : IGameMode
    {

        public string GameModeName { get => "pcraft"; }

        private readonly Pico8Functions p8 = p8;

#nullable enable

        private List<Entity>? anvilRecipe;
    
        private double banim;
    
        private bool canSwitchLevel = false;
        private Level? cave;
        private List<Entity>? chemRecipe;
        private double clx;
        private double cly;
        private double cmx;
        private double cmy;
        private double coffx;
        private double coffy;
        private Entity? curItem;
        private Entity? curMenu;
        private Level? currentLevel;
    
        private double[] data = new double[8192];
    
        private List<Entity> enemies = [];
        private List<Entity> entities = [];
    
        private List<Entity>? factoryRecipe;
    
        private List<Entity>? furnaceRecipe;
    
        private int holex;
        private int holey;
    
        private List<Entity> invent = [];
        private Level? island;
    
        private double[][]? level;
        private int levelsx;
        private int levelsy;
        private int levelx;
        private int levely;
        private bool levelUnder = false;
        private double llife;
        private double lrot;
        private double lstam;
    
        private Entity? menuInvent;
    
        private List<Entity>? nearEnemies;
    
        private double panim;
    
        private double plife;
        private double plx;
        private double ply;
        private double prot;
        private double pstam;
    
        private double[][] Rndwat = new double[16][];
    
        private int stamCost;
        private List<Entity>? stonebenchRecipe;
        private bool switchLevel = false;
    
        private double time;
        private int toogleMenu;
        readonly int[] typeCount = new int[11];
    
        private List<Entity>? workbenchRecipe;

#nullable disable

        //p.craft
        //by nusan

        private bool lb4 = false;
        private bool lb5 = false;
        private bool block5 = false;
    
        private readonly int enstep_Wait = 0;
        private readonly int enstep_Walk = 1;
        private readonly int enstep_Chase = 2;
        private readonly int enstep_Patrol = 3;
    
        static readonly string[] pwrNames = ["wood", "stone", "iron", "gold", "gem"];
        static readonly int[][] pwrPal = [[2, 2, 4, 4], [5, 2, 4, 13], [13, 5, 13, 6], [9, 2, 9, 10], [13, 2, 14, 12]];
    
        static readonly Material haxe = Item("haxe", 98);
        static readonly Material sword = Item("sword", 99);
        static readonly Material scythe = Item("scythe", 100);
        static readonly Material shovel = Item("shovel", 101);
        static readonly Material pick = Item("pick", 102);
    
        static readonly int[] pstone = [0, 1, 5, 13];
        static readonly int[] piron = [1, 5, 13, 6];
        static readonly int[] pgold = [1, 9, 10, 7];
    
        static readonly Material wood = Item("wood", 103);
        static readonly Material sand = Item("sand", 114, [15]);
        static readonly Material seed = Item("seed", 115);
        static readonly Material wheat = Item("wheat", 118, [4, 9, 10, 9]);
        static readonly Material apple = Item("apple", 116);
    
        static readonly Material glass = Item("glass", 117);
        static readonly Material stone = Item("stone", 118, pstone);
        static readonly Material iron = Item("iron", 118, piron);
        static readonly Material gold = Item("gold", 118, pgold);
        static readonly Material gem = Item("gem", 118, [1, 2, 14, 12]);
    
        static readonly Material fabric = Item("fabric", 69);
        static readonly Material sail = Item("sail", 70);
        static readonly Material glue = Item("glue", 85, [1, 13, 12, 7]);
        static readonly Material boat = Item("boat", 86);
        static readonly Material ichor = Item("ichor", 114, [11]);
        static readonly Material potion = Item("potion", 85, [1, 2, 8, 14]);
    
        static readonly Material ironbar = Item("iron bar", 119, piron);
        static readonly Material goldbar = Item("gold bar", 119, pgold);
        static readonly Material bread = Item("bread", 119, [1, 4, 15, 7]);
    
        static readonly Material workbench = BigSpr(104, Item("workbench", 89, [1, 4, 9], true));
        static readonly Material stonebench = BigSpr(104, Item("stonebench", 89, [1, 6, 13], true));
        static readonly Material furnace = BigSpr(106, Item("furnace", 90, null, true));
        static readonly Material anvil = BigSpr(108, Item("anvil", 91, null, true));
        static readonly Material factory = BigSpr(71, Item("factory", 74, null, true));
        static readonly Material chem = BigSpr(78, Item("chem lab", 76, null, true));
        static readonly Material chest = BigSpr(110, Item("chest", 92));
    
        static readonly Material inventary = Item("inventory", 89);
        static readonly Material pickuptool = Item("pickup tool", 73);
    
        static readonly Material etext = Item("text", 103);
        static readonly Material player = Item(null, 1);
        static readonly Material zombi = Item(null, 2);
    
        static readonly Ground grwater = new() { Id = 0, Gr = 0 };
        static readonly Ground grsand = new() { Id = 1, Gr = 1 };
        static readonly Ground grgrass = new() { Id = 2, Gr = 2 };
        static readonly Ground grrock = new() { Id = 3, Gr = 3, Mat = stone, Tile = grsand, Life = 15 };
        static readonly Ground grtree = new() { Id = 4, Gr = 2, Mat = wood, Tile = grgrass, Life = 8, IsTree = true, Pal = [1, 5, 3, 11] };
        static readonly Ground grfarm = new() { Id = 5, Gr = 1 };
        static readonly Ground grwheat = new() { Id = 6, Gr = 1 };
        static readonly Ground grplant = new() { Id = 7, Gr = 2 };
        static readonly Ground griron = new() { Id = 8, Gr = 1, Mat = iron, Tile = grsand, Life = 45, IsTree = true, Pal = [1, 1, 13, 6] };
        static readonly Ground grgold = new() { Id = 9, Gr = 1, Mat = gold, Tile = grsand, Life = 80, IsTree = true, Pal = [1, 2, 9, 10] };
        static readonly Ground grgem = new() { Id = 10, Gr = 1, Mat = gem, Tile = grsand, Life = 160, IsTree = true, Pal = [1, 2, 14, 12] };
        static readonly Ground grhole = new() { Id = 11, Gr = 1 };
    
        private Ground lastGround = grsand;
    
        private readonly Ground[] grounds = { grwater, grsand, grgrass, grrock, grtree, grfarm, grwheat, grplant, griron, grgold, grgem, grhole };
    
        private Entity mainMenu = Cmenu(inventary, null, 128, "by nusan", "2016");
        private Entity introMenu = Cmenu(inventary, null, 136, "a storm leaved you", "on a deserted island");
        private Entity deathMenu = Cmenu(inventary, null, 128, "you died", "alone ...");
        private Entity winMenu = Cmenu(inventary, null, 136, "you successfully escaped", "from the island");
    
        static Pcraft()
        {
            apple.GiveLife = 20;
    
            potion.GiveLife = 100;
    
            bread.GiveLife = 40;
        }
    
        private static Material Item(string n, int s, int[] p = null, bool bc = false)
        {
            return new() { Name = n, Spr = s, Pal = p, BeCraft = bc };
        }
    
        private Entity Inst(Material it)
        {
            return new() { Type = it };
        }
    
        private Entity Instc(Material it, int? c = null, List<Entity> l = null)
        {
            return new() { Type = it, Count = c, List = l };
        }
    
        private Entity SetPower(int? v, Entity i)
        {
            i.Power = v;
            return i;
        }
    
        private Entity Entity(Material it, double xx, double yy, double vxx, double vyy)
        {
            return new() { Type = it, X = xx, Y = yy, Vx = vxx, Vy = vyy };
        }
    
        private Entity Rentity(Material it, double xx, double yy)
        {
            return Entity(it, xx, yy, p8.Rnd(3) - 1.5, p8.Rnd(3) - 1.5);
        }
    
        private Entity SetText(string t, int c, int time, Entity e)
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
    
        private Entity Recipe(Entity m, List<Entity> require)
        {
            return new() { Type = m.Type, Power = m.Power, Count = m.Count, Req = require, List = m.List };
        }
    
        private bool CanCraft(Entity req)
        {
            var can = true;
            for (int i = 0; i < req.Req.Count; i++)
            {
                if (HowMany(invent, req.Req[i]) < req.Req[i].Count)
                {
                    can = false;
                    break;
                }
            }
            return can;
        }
    
        private void Craft(Entity req)
        {
            for (int i = 0; i < req.Req.Count; i++)
            {
                RemInList(invent, req.Req[i]);
            }
            AddItemInList(invent, SetPower(req.Power, Instc(req.Type, req.Count, req.List)), -1);
        }
    
        private void SetPal(int[] l)
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
    
        private int HowMany(List<Entity> list, Entity it)
        {
            var count = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == it.Type)
                {
                    if (it.Power == null || it.Power == list[i].Power)
                    {
                        if (list[i].Count != null)
                        {
                            count += (int)list[i].Count;
                        }
                        else
                        {
                            count += 1;
                        }
                    }
                }
            }
            return count;
        }
    
        private Entity IsInList(List<Entity> list, Entity it)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == it.Type)
                {
                    if (it.Power == null || it.Power == list[i].Power)
                    {
                        return list[i];
                    }
                }
            }
            return null;
        }
    
        private void RemInList(List<Entity> list, Entity elem)
        {
            var it = IsInList(list, elem);
            if (it == null)
            {
                return;
            }
            if (it.Count != null)
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
    
        private void AddItemInList(List<Entity> list, Entity it, int p)
        {
            var it2 = IsInList(list, it);
            if (it2 == null || it2.Count == null)
            {
                AddPlace(list, it, p);
            }
            else
            {
                it2.Count += it.Count;
            }
        }
    
        private void AddPlace(List<Entity> l, Entity e, int p)
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
            return (e.X > clx - size && e.X < clx + size && e.Y > cly - size && e.Y < cly + size);
        }
    
        private double Lerp(double a, double b, double alpha)
        {
            return a * (1.0 - alpha) + b * alpha;
        }
    
        private double GetInvLen(double x, double y)
        {
            return 1 / GetLen(x, y);
        }
    
        private double GetLen(double x, double y)
        {
            return Math.Sqrt(x * x + y * y + 0.001);
        }
    
        private double GetRot(double dx, double dy)
        {
            return dy >= 0 ? (dx + 3) * 0.25 : (1 - dx) * 0.25;
        }
    
        private void FillEne(Level l)
        {
            l.Ene = [Entity(player, 0, 0, 0, 0)];
            enemies = l.Ene;
            for (int i = 0; i <= levelsx - 1; i++)
            {
                for (int j = 0; j <= levelsy - 1; j++)
                {
                    var c = GetDirectGr(i, j);
                    var r = p8.Rnd(100);
                    var ex = i * 16 + 8;
                    var ey = j * 16 + 8;
                    var dist = Math.Max(Math.Abs(ex - plx), Math.Abs(ey - ply));
                    if (r < 3 && c != grwater && c != grrock && !c.IsTree && dist > 50)
                    {
                        var newe = Entity(zombi, ex, ey, 0, 0);
                        newe.Life = 10;
                        newe.Prot = 0;
                        newe.Lrot = 0;
                        newe.Panim = 0;
                        newe.Banim = 0;
                        newe.Dtim = 0;
                        newe.Step = 0;
                        newe.Ox = 0;
                        newe.Oy = 0;
                        p8.Add(l.Ene, newe);
                    }
                }
            }
        }
    
        private Level CreateLevel(int xx, int yy, int sizex, int sizey, bool IsUnderground)
        {
            var l = new Level { X = xx, Y = yy, Sx = sizex, Sy = sizey, IsUnder = IsUnderground, Ent = [], Ene = [], Dat = new double[8192] };
            SetLevel(l);
            levelUnder = IsUnderground;
            CreateMap();
            FillEne(l);
            l.Stx = (holex - levelx) * 16 + 8;
            l.Sty = (holey - levely) * 16 + 8;
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
    
        private void ResetLevel()
        {
            //p8.Reload();
            //p8.Memcpy(0x1000, 0x2000, 0x1000);

            prot = 0.0;
            lrot = 0.0;
    
            panim = 0.0;
    
            pstam = 100.0;
            lstam = pstam;
            plife = 100.0;
            llife = plife;
    
            banim = 0.0;
    
            coffx = 0.0;
            coffy = 0.0;
    
            time = 0.0;
    
            toogleMenu = 0;
            invent = [];
            curItem = null;
            switchLevel = false;
            canSwitchLevel = false;
            menuInvent = Cmenu(inventary, invent);
    
            for (int i = 0; i <= 15; i++)
            {
                Rndwat[i] = new double[16];
                for (int j = 0; j <= 15; j++)
                {
                    Rndwat[i][j] = p8.Rnd(100);
                }
            }
    
            cave = CreateLevel(64, 0, 32, 32, true);
            island = CreateLevel(0, 0, 64, 64, false);
    
            var tmpworkbench = Entity(workbench, plx, ply, 0, 0);
            tmpworkbench.HasCol = true;
            tmpworkbench.List = workbenchRecipe;
    
            p8.Add(invent, tmpworkbench);
            p8.Add(invent, Inst(pickuptool));
        }
    
        public void Init()
        {
            p8.Music(4, 10000);

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
    
        private (int, int) GetMcoord(double x, double y)
        {
            return ((int)Math.Floor(x / 16), (int)Math.Floor(y / 16));
        }
    
        private bool IsFree(double x, double y, Entity e = null)
        {
            var gr = GetGr(x, y);
            return !(gr.IsTree || gr == grrock);
        }
    
        private bool IsFreeEnem(double x, double y, Entity e = null)
        {
            var gr = GetGr(x, y);
            return !(gr.IsTree || gr == grrock || gr == grwater);
        }
    
        private Ground GetGr(double x, double y)
        {
            var (i, j) = GetMcoord(x, y);
            return GetDirectGr(i, j);
        }
    
        private Ground GetDirectGr(double i, double j)
        {
            if (i < 0 || j < 0 || i >= levelsx || j >= levelsy) { return grounds[0]; }
            return grounds[p8.Mget(i + levelx, j)];
        }
    
        private void SetGr(double x, double y, Ground v)
        {
            var (i, j) = GetMcoord(x, y);
            if (i < 0 || j < 0 || i >= levelsx || j >= levelsy) { return; }
            p8.Mset(i + levelx, j, v.Id);
        }
    
        private double DirGetData(int i, int j, int @default)
        {
            int g = i + j * levelsx;
            if (data[g - 1] == 0)
            {
                data[g - 1] = @default;
            }
            return data[g - 1];
        }
    
        private void DirSetData(int i, int j, double v)
        {
            data[i + j * levelsx - 1] = v;
        }
    
        private double GetData(double x, double y, int @default)
        {
            var (i, j) = GetMcoord(x, y);
            if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
            {
                return @default;
            }
            return DirGetData(i, j, @default);
        }
    
        private void SetData(double x, double y, double v)
        {
            var (i, j) = GetMcoord(x, y);
            if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
            {
                return;
            }
            DirSetData(i, j, v);
        }
    
        private void Cleardata(double x, double y)
        {
            var (i, j) = GetMcoord(x, y);
            if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
            {
                return;
            }
            data[i + j * levelsx - 1] = 0;
        }
    
        private int Loop(int sel, List<Entity> l)
        {
            var lp = l.Count;
            return ((sel % lp) + lp) % lp;
        }
    
        private bool EntColFree(double x, double y, Entity e)
        {
            return Math.Max(Math.Abs(e.X - x), Math.Abs(e.Y - y)) > 8;
        }
    
        private (double, double) ReflectCol(double x, double y, double dx, double dy, Func<double, double, Entity, bool> checkfun, double dp, Entity e = null)
        {
            var newx = x + dx;
            var newy = y + dy;
        
            var ccur = checkfun(x, y, e);
            var ctotal = checkfun(newx, newy, e);
            var chor = checkfun(newx, y, e);
            var cver = checkfun(x, newy, e);
        
            if (ccur)
            {
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
            }
        
            return (dx, dy);
        }
        
        private void AddItem(Material mat, double count, double hitx, double hity)
        {
            var countFlr = (int)Math.Floor(count);
        
            for (int i = 0; i < countFlr; i++)
            {
                var gi = Rentity(mat, Math.Floor(hitx / 16) * 16 + p8.Rnd(14) + 1, Math.Floor(hity / 16) * 16 + p8.Rnd(14) + 1);
                gi.GiveItem = mat;
                gi.HasCol = true;
                gi.Timer = 110 + p8.Rnd(20);
                p8.Add(entities, gi);
            }
        }
        
        private void UpGround()
        {
            var ci = (int)Math.Floor((clx - 64) / 16);
            var cj = (int)Math.Floor((cly - 64) / 16);
            for (int i = ci; i < ci + 8; i++)
            {
                for (int j = cj; j < cj + 8; j++)
                {
                    var gr = GetDirectGr(i, j);
                    if (gr == grfarm)
                    {
                        var d = DirGetData(i, j, 0);
                        if (time > d)
                        {
                            p8.Mset(i + levelx, j, grsand.Id);
                        }
                    }
                }
            }
        }
        
        private double UpRot(double grot, double rot)
        {
            if (Math.Abs(rot - grot) > 0.5)
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
            return (Lerp(rot, grot, 0.4) % 1 + 1) % 1;
        }
        
        public void Update()
        {
            if (curMenu != null)
            {
                if (curMenu.Spr != null)
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
                else
                {
                    var intMenu = curMenu;
                    var othMenu = menuInvent;
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
                                var el = intMenu.List[intMenu.Sel];
                                p8.Del(intMenu.List, el);
                                AddItemInList(othMenu.List, el, othMenu.Sel);
                                if (intMenu.List.Count > 0 && intMenu.Sel > (intMenu.List.Count - 1)) { intMenu.Sel -= 1; }
                                if (intMenu == menuInvent && curItem == el)
                                {
                                    curItem = null;
                                }
                            }
                            else if (curMenu.Type.BeCraft)
                            {
                                if (curMenu.Sel >= 0 && curMenu.Sel < intMenu.List.Count)
                                {
                                    var rec = curMenu.List[curMenu.Sel];
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
                p8.Music(currentLevel == cave ? 4 : 1);
            }
        
            if (curItem != null)
            {
                if (HowMany(invent, curItem) <= 0) { curItem = null; }
            }
        
            UpGround();
        
            var playHit = GetGr(plx, ply);
            if (playHit != lastGround && playHit == grwater) { p8.Sfx(11, 3); }
            lastGround = playHit;
            var s = (playHit == grwater || pstam <= 0) ? 1 : 2;
            if (playHit == grhole)
            {
                switchLevel = switchLevel || canSwitchLevel;
            }
            else
            {
                canSwitchLevel = true;
            }
        
            var dx = 0.0;
            var dy = 0.0;
        
            if (p8.Btn(0)) dx -= 1.0;
            if (p8.Btn(1)) dx += 1.0;
            if (p8.Btn(2)) dy -= 1.0;
            if (p8.Btn(3)) dy += 1.0;
        
            var dl = GetInvLen(dx, dy);
        
            dx *= dl;
            dy *= dl;
        
            if (Math.Abs(dx) > 0 || Math.Abs(dy) > 0)
            {
                lrot = GetRot(dx, dy);
                panim += 1.0 / 33.0;
            }
            else
            {
                panim = 0;
            }
        
            dx *= s;
            dy *= s;
        
            (dx, dy) = ReflectCol(plx, ply, dx, dy, IsFree, 0);
        
            var canAct = true;
        
            var fin = entities.Count;
            for (int i = fin - 1; i >= 0; i--)
            {
                var e = entities[i];
                if (e.HasCol)
                {
                    (e.Vx, e.Vy) = ReflectCol(e.X, e.Y, e.Vx, e.Vy, IsFree, 0.9);
                }
                e.X += e.Vx;
                e.Y += e.Vy;
                e.Vx *= 0.95;
                e.Vy *= 0.95;
        
                if (e.Timer != null && e.Timer < 1)
                {
                    p8.Del(entities, e);
                }
                else
                {
                    if (e.Timer != null) { e.Timer -= 1; }
        
                    var dist = Math.Max(Math.Abs(e.X - plx), Math.Abs(e.Y - ply));
                    if (e.GiveItem != null)
                    {
                        if (dist < 5)
                        {
                            if (e.Timer == null || e.Timer < 115)
                            {
                                var newIt = Instc(e.GiveItem, 1);
                                AddItemInList(invent, newIt, -1);
                                p8.Del(entities, e);
                                p8.Add(entities, SetText(HowMany(invent, newIt).ToString(), 11, 20, Entity(etext, e.X, e.Y - 5, 0, -1)));
                                p8.Sfx(18, 3);
                            }
                        }
                    }
                    else
                    {
                        if (e.HasCol)
                        {
                            (dx, dy) = ReflectCol(plx, ply, dx, dy, EntColFree, 0, e);
                        }
                        if (dist < 12 && p8.Btn(5) && !block5 && !lb5)
                        {
                            if (curItem != null && curItem.Type == pickuptool)
                            {
                                if (e.Type == chest || e.Type.BeCraft)
                                {
                                    AddItemInList(invent, e, -1);
                                    curItem = e;
                                    p8.Del(entities, e);
                                }
                                canAct = false;
                            }
                            else
                            {
                                if (e.Type == chest || e.Type.BeCraft)
                                {
                                    toogleMenu = 0;
                                    curMenu = Cmenu(e.Type, e.List);
                                    p8.Sfx(13, 3);
                                }
                                canAct = false;
                            }
                        }
                    }
                }
            }
        
            nearEnemies = [];
        
            var ebx = p8.Cos(prot);
            var eby = p8.Sin(prot);
        
            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                if (IsIn(e, 100))
                {
                    if (e.Type == player)
                    {
                        e.X = plx;
                        e.Y = ply;
                    }
                    else
                    {
                        var distp = GetLen(e.X - plx, e.Y - ply);
                        var mspeed = 0.8;
        
                        var disten = GetLen(e.X - plx - ebx * 8, e.Y - ply - eby * 8);
                        if (disten < 10)
                        {
                            p8.Add(nearEnemies, e);
                        }
                        if (distp < 8)
                        {
                            e.Ox += Math.Max(-0.4, Math.Min(0.4, e.X - plx));
                            e.Oy += Math.Max(-0.4, Math.Min(0.4, e.Y - ply));
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
                                e.Dx = 0;
                                e.Dy = 0;
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
                                    e.Banim = 0;
                                }
                                else
                                {
                                    e.Dx = 0;
                                    e.Dy = 0;
                                    e.Banim -= 1;
                                    e.Banim = p8.Mod(e.Banim, 8);
                                    var pow = 10;
                                    if (e.Banim == 4)
                                    {
                                        plife -= pow;
                                        p8.Add(entities, SetText(pow.ToString(), 8, 20, Entity(etext, plx, ply - 10, 0, -1)));
                                        p8.Sfx(14 + p8.Rnd(2), 3);
                                    }
                                    plife = Math.Max(0, plife);
                                }
                                mspeed = 1.4;
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
        
                        var dl2 = mspeed * GetInvLen(e.Dx, e.Dy);
                        e.Dx *= dl2;
                        e.Dy *= dl2;
        
                        var fx = e.Dx + e.Ox;
                        var fy = e.Dy + e.Oy;
                        (fx, fy) = ReflectCol(e.X, e.Y, fx, fy, IsFreeEnem, 0);
        
                        if (Math.Abs(e.Dx) > 0 || Math.Abs(e.Dy) > 0)
                        {
                            e.Lrot = GetRot(e.Dx, e.Dy);
                            e.Panim += 1.0 / 33.0;
                        }
                        else
                        {
                            e.Panim = 0;
                        }
        
                        e.X += fx;
                        e.Y += fy;
        
                        e.Ox *= 0.9;
                        e.Oy *= 0.9;
        
                        e.Prot = UpRot(e.Lrot, e.Prot);
                    }
                }
            }
        
            (dx, dy) = ReflectCol(plx, ply, dx, dy, IsFree, 0);
        
            plx += dx;
            ply += dy;
        
            prot = UpRot(lrot, prot);
        
            llife += Math.Max(-1, Math.Min(1, (plife - llife)));
            lstam += Math.Max(-1, Math.Min(1, (pstam - lstam)));
        
            if (p8.Btn(5) && !block5 && canAct)
            {
                var bx = p8.Cos(prot);
                var by = p8.Sin(prot);
                var hitx = plx + bx * 8;
                var hity = ply + by * 8;
                var hit = GetGr(hitx, hity);
        
                if (!lb5 && curItem != null && curItem.Type.Drop)
                {
                    if (hit == grsand || hit == grgrass)
                    {
                        if (curItem.List == null) { curItem.List = []; }
                        curItem.HasCol = true;
        
                        curItem.X = Math.Floor(hitx / 16) * 16 + 8;
                        curItem.Y = Math.Floor(hity / 16) * 16 + 8;
                        curItem.Vx = 0;
                        curItem.Vy = 0;
                        p8.Add(entities, curItem);
                        RemInList(invent, curItem);
                        canAct = false;
                    }
                }
                if (banim == 0 && pstam > 0 && canAct)
                {
                    banim = 8;
                    stamCost = 20;
                    if (nearEnemies.Count > 0)
                    {
                        p8.Sfx(19, 3);
                        var pow = 1.0;
                        if (curItem != null && curItem.Type == sword)
                        {
                            pow = 1 + (int)curItem.Power + p8.Rnd((int)curItem.Power * (int)curItem.Power);
                            stamCost = Math.Max(0, 20 - (int)curItem.Power * 2);
                            pow = Math.Floor(pow);
                            p8.Sfx(14 + p8.Rnd(2), 3);
                        }
                        for (int i = 0; i < nearEnemies.Count; i++)
                        {
                            var e = nearEnemies[i];
                            e.Life -= pow / nearEnemies.Count;
                            var push = (pow - 1) * 0.5;
                            e.Ox += Math.Max(-push, Math.Min(push, e.X - plx));
                            e.Oy += Math.Max(-push, Math.Min(push, e.Y - ply));
                            if (e.Life <= 0)
                            {
                                p8.Del(enemies, e);
                                AddItem(ichor, p8.Rnd(3), e.X, e.Y);
                                AddItem(fabric, p8.Rnd(3), e.X, e.Y);
                            }
                            p8.Add(entities, SetText(pow.ToString(), 9, 20, Entity(etext, e.X, e.Y - 10, 0, -1)));
                        }
                    }
                    else if (hit.Mat != null)
                    {
                        p8.Sfx(15, 3);
                        var pow = 1.0;
                        if (curItem != null)
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
                        pow = Math.Floor(pow);
        
                        var d = GetData(hitx, hity, hit.Life);
                        if (d - pow <= 0)
                        {
                            SetGr(hitx, hity, hit.Tile);
                            Cleardata(hitx, hity);
                            AddItem(hit.Mat, p8.Rnd(3) + 2, hitx, hity);
                            if (hit == grtree && p8.Rnd() > 0.7)
                            {
                                AddItem(apple, 1, hitx, hity);
                            }
                        }
                        else
                        {
                            SetData(hitx, hity, d - pow);
                        }
                        p8.Add(entities, SetText(pow.ToString(), 10, 20, Entity(etext, hitx, hity, 0, -1)));
                    }
                    else
                    {
                        p8.Sfx(19, 3);
                        if (curItem != null)
                        {
                            if (curItem.Power != null)
                            {
                                stamCost = Math.Max(0, 20 - (int)curItem.Power * 2);
                            }
                            if (curItem.Type.GiveLife != null)
                            {
                                plife = Math.Min(100, plife + (int)curItem.Type.GiveLife);
                                RemInList(invent, Instc(curItem.Type, 1));
                                p8.Sfx(21, 3);
                            }
                            if (hit == grgrass && curItem.Type == scythe)
                            {
                                SetGr(hitx, hity, grsand);
                                if (p8.Rnd() > 0.4) { AddItem(seed, 1, hitx, hity); }
                            }
                            if (hit == grsand && curItem.Type == shovel)
                            {
                                if (curItem.Power > 3)
                                {
                                    SetGr(hitx, hity, grwater);
                                    AddItem(sand, 2, hitx, hity);
                                }
                                else
                                {
                                    SetGr(hitx, hity, grfarm);
                                    SetData(hitx, hity, time + 15 + p8.Rnd(5));
                                    AddItem(sand, p8.Rnd(2), hitx, hity);
                                }
                            }
                            if (hit == grwater && curItem.Type == sand)
                            {
                                SetGr(hitx, hity, grsand);
                                RemInList(invent, Instc(sand, 1));
                            }
                            if (hit == grwater && curItem.Type == boat)
                            {
                                //p8.Reload();
                                //p8.Memcpy(0x1000,0x2000,0x1000);
                                curMenu = winMenu;
                                p8.Music(4);
                            }
                            if (hit == grfarm && curItem.Type == seed)
                            {
                                SetGr(hitx, hity, grwheat);
                                SetData(hitx, hity, time + 15 + p8.Rnd(5));
                                RemInList(invent, Instc(seed, 1));
                            }
                            if (hit == grwheat && curItem.Type == scythe)
                            {
                                SetGr(hitx, hity, grsand);
                                var d = Math.Max(0, Math.Min(4, 4 - (GetData(hitx, hity, 0) - time)));
                                AddItem(wheat, d / 2 + p8.Rnd(d / 2), hitx, hity);
                                AddItem(seed, 1, hitx, hity);
                            }
                        }
                    }
                    pstam -= stamCost;
                }
            }
        
            if (banim > 0)
            {
                banim -= 1;
            }
        
            if (pstam < 100)
            {
                pstam = Math.Min(100, pstam + 1);
            }
        
            var m = 16;
            var msp = 4;
        
            if (Math.Abs(cmx - plx) > m)
            {
                coffx += dx * 0.4;
            }
            if (Math.Abs(cmy - ply) > m)
            {
                coffy += dy * 0.4;
            }
        
            cmx = Math.Max(plx - m, cmx);
            cmx = Math.Min(plx + m, cmx);
            cmy = Math.Max(ply - m, cmy);
            cmy = Math.Min(ply + m, cmy);
        
            coffx *= 0.9;
            coffy *= 0.9;
            coffx = Math.Min(msp, Math.Max(-msp, coffx));
            coffy = Math.Min(msp, Math.Max(-msp, coffy));
        
            clx += coffx;
            cly += coffy;
        
            clx = Math.Max(cmx - m, clx);
            clx = Math.Min(cmx + m, clx);
            cly = Math.Max(cmy - m, cly);
            cly = Math.Min(cmy + m, cly);
        
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
        
            time += 1.0 / 30.0;
        
            if (plife <= 0)
            {
                //p8.Reload();
                //p8.Memcpy(0x1000, 0x2000, 0x1000);
                curMenu = deathMenu;
                p8.Music(4);
            }
        }
    
        private (int, int) Mirror(double rot)
        {
            if (rot < 0.125)
            {
                return (0, 1);
            }
            else if (rot < 0.325)
            {
                return (0, 0);
            }
            else if (rot < 0.625)
            {
                return (1, 0);
            }
            else if (rot < 0.825)
            {
                return (1, 1);
            }
            else
            {
                return (0, 1);
            }
        }
        
        private void Dplayer(double x, double y, double rot, double anim, double subanim, bool isplayer)
        {
            var cr = p8.Cos(rot);
            var sr = p8.Sin(rot);
            var cv = -sr;
            var sv = cr;

            x = Math.Floor(x);
            y = Math.Floor(y - 4);

            var lan = p8.Sin(anim * 2) * 1.5;

            var bel = GetGr(x, y);
            if (bel == grwater)
            {
                y += 4;
                p8.Circ(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, 3, 6);
                p8.Circ(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, 3, 6);

                var anc = 3 + ((time * 3) % 1) * 3;
                p8.Circ(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, anc, 6);
                p8.Circ(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, anc, 6);
            }
            else
            {
                p8.Circfill(x + cv * 2 - cr * lan, y + 3 + sv * 2 - sr * lan, 3, 1);
                p8.Circfill(x - cv * 2 + cr * lan, y + 3 - sv * 2 + sr * lan, 3, 1);
            }
            var blade = (rot + 0.25) % 1;
            if (subanim > 0)
            {
                blade = blade - 0.3 + subanim * 0.04;
            }
            var bcr = p8.Cos(blade);
            var bsr = p8.Sin(blade);

            (int mx, int my) = Mirror(blade);

            var weap = 75;

            if (isplayer && curItem != null)
            {
                p8.Pal();
                weap = curItem.Type.Spr;
                if (curItem.Power != null)
                {
                    SetPal(pwrPal[(int)curItem.Power - 1]);
                }
                if (curItem.Type != null && curItem.Type.Pal != null)
                {
                    SetPal(curItem.Type.Pal);
                }
            }

            p8.Spr(weap, x + bcr * 4 - cr * lan - mx * 8 + 1, y + bsr * 4 - sr * lan + my * 8 - 7, 1, 1, mx == 1, my == 1);

            if (isplayer) { p8.Pal(); }

            if (bel != grwater)
            {
                p8.Circfill(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, 3, 2);
                p8.Circfill(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, 3, 2);

                Console.WriteLine(-rot + 0.75);

                (int mx2, int my2) = Mirror(p8.Mod((-rot + 0.75), 1)); // changed from (rot + 0.75) % 1
                p8.Spr(75, x + cv * 4 + cr * lan - 8 + mx2 * 8 + 1, y + sv * 4 + sr * lan + my2 * 8 - 7, 1, 1, mx2 == 0, my2 == 1);
            }

            p8.Circfill(x + cr, y + sr - 2, 4, 2);
            p8.Circfill(x + cr, y + sr, 4, 2);
            p8.Circfill(x + cr * 1.5, y + sr * 1.5 - 2, 2.5, 15);
            p8.Circfill(x - cr, y - sr - 3, 3, 4);

        }
        
        private double[][] Noise(int sx, int sy, double startscale, double scalemod, int featstep)
        {
            var n = new double[sx + 1][];

            for (int i = 0; i <= sx; i++)
            {
                n[i] = new double[sy + 1];
                for (int j = 0; j <= sy; j++)
                {
                    n[i][j] = 0.5;
                }
            }

            var step = sx;
            var scale = startscale;

            while (step > 1)
            {
                var cscal = scale;
                if (step == featstep) { cscal = 1; }

                for (int i = 0; i <= sx - 1; i += step)
                {
                    for (int j = 0; j <= sy - 1; j += step)
                    {
                        var c1 = n[i][j];
                        var c2 = n[i + step][j];
                        var c3 = n[i][j + step];
                        n[i + step / 2][j] = (c1 + c2) * 0.5 + (p8.Rnd() - 0.5) * cscal;
                        n[i][j + step / 2] = (c1 + c3) * 0.5 + (p8.Rnd() - 0.5) * cscal;
                    }
                }

                for (int i = 0; i <= sx - 1; i += step)
                {
                    for (int j = 0; j <= sy - 1; j += step)
                    {
                        var c1 = n[i][j];
                        var c2 = n[i + step][j];
                        var c3 = n[i][j + step];
                        var c4 = n[i + step][j + step];
                        n[i + step / 2][j + step / 2] = (c1 + c2 + c3 + c4) * 0.25 + (p8.Rnd() - 0.5) * cscal;
                    }
                }

                step /= 2;
                scale *= scalemod;
            }

            return n;
        }
    
        private double[][] CreateMapStep(int sx, int sy, int a, int b, int c, int d, int e)
        {
            var cur = Noise(sx, sy, 0.9, 0.2, sx);
            var cur2 = Noise(sx, sy, 0.9, 0.4, 8);
            var cur3 = Noise(sx, sy, 0.9, 0.3, 8);
            var cur4 = Noise(sx, sy, 0.8, 1.1, 4);
    
            for (int i = 0; i < 11; i++)
            {
                typeCount[i] = 0;
            }
    
            for (int i = 0; i <= sx; i++)
            {
                for (int j = 0; j <= sy; j++)
                {
                    var v = Math.Abs(cur[i][j] - cur2[i][j]);
                    var v2 = Math.Abs(cur[i][j] - cur3[i][j]);
                    var v3 = Math.Abs(cur[i][j] - cur4[i][j]);
                    var dist = Math.Max((Math.Abs((double)i / sx - 0.5) * 2), (Math.Abs((double)j / sy - 0.5) * 2));
                    dist = dist * dist * dist * dist;
                    var coast = v * 4 - dist * 4;
    
                    var id = a;
                    if (coast > 0.3) { id = b; } // sand
                    if (coast > 0.6) { id = c; } // grass
                    if (coast > 0.3 && v2 > 0.5) { id = d; } // stone
                    if (id == c && v3 > 0.5) { id = e; } // tree
    
                    typeCount[id] += 1;
    
                    cur[i][j] = id;
                }
            }
    
            return cur;
        }
    
        private void CreateMap()
        {
            var needmap = true;
    
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
                    plx = -1;
                    ply = -1;
    
                    for (int i = 0; i <= 500; i++)
                    {
                        var depx = (int)Math.Floor(levelsx / 8 + p8.Rnd(levelsx * 6 / 8));
                        var depy = (int)Math.Floor(levelsy / 8 + p8.Rnd(levelsy * 6 / 8));
                        var c = level[depx][depy];
    
                        if (c == 1 || c == 2)
                        {
                            plx = depx * 16 + 8;
                            ply = depy * 16 + 8;
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
                    p8.Mset(i + levelx, j + levely, level[i][j]);
                }
            }
    
            holex = levelsx / 2 + levelx;
            holey = levelsy / 2 + levely;
    
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    p8.Mset(holex + i, holey + j, ((levelUnder) ? 1 : 3));
                }
            }
    
            p8.Mset(holex, holey, 11);
    
            clx = plx;
            cly = ply;
    
            cmx = plx;
            cmy = ply;
        }
    
        private bool Comp(int i, int j, Ground gr)
        {
            var gr2 = GetDirectGr(i, j);
            return gr != null && gr2 != null && gr.Gr == gr2.Gr;
        }
    
        private double WatVal(double i, double j)
        {
            return Rndwat[(int)Math.Floor(Math.Abs(i * 2) % 16)][(int)Math.Floor(Math.Abs(j * 2) % 16)];
        }
    
        private void WatAnim(double i, double j)
        {
            var a = ((time * 0.6 + WatVal(i, j) / 100) % 1) * 19;
            if (a > 16) { p8.Spr(13 + a - 16, i * 16, j * 16); }
        }
    
        private double RndCenter(double i, double j)
        {
            return (Math.Floor(WatVal(i, j) / 34) + 18) % 20;
        }
    
        private int RndSand(double i, double j)
        {
            return (int)Math.Floor(WatVal(i, j) / 34) + 1;
        }
    
        private int RndTree(double i, double j)
        {
            return (int)Math.Floor(WatVal(i, j) / 51) * 32;
        }
    
        private void Spr4(int i, int j, int gi, int gj, int a, int b, int c, int d, int off, Func<double, double, int> f)
        {
            p8.Spr(f(i, j + off) + a, gi, gj + 2 * off);
            p8.Spr(f(i + 0.5, j + off) + b, gi + 8, gj + 2 * off);
            p8.Spr(f(i, j + 0.5 + off) + c, gi, gj + 8 + 2 * off);
            p8.Spr(f(i + 0.5, j + 0.5 + off) + d, gi + 8, gj + 8 + 2 * off);
        }
    
        private void DrawBack()
        {
            var ci = (int)Math.Floor((clx - 64) / 16);
            var cj = (int)Math.Floor((cly - 64) / 16);
    
            for (int i = ci; i <= ci + 8; i++)
            {
                for (int j = cj; j <= cj + 8; j++)
                {
                    var gr = GetDirectGr(i, j);
    
                    var gi = (i - ci) * 2 + 64;
                    var gj = (j - cj) * 2 + 32;
    
                    if (gr != null && gr.Gr == 1) // sand
                    {
                        var sv = 0;
                        if (gr == grfarm || gr == grwheat) { sv = 3; }
                        p8.Mset(gi, gj, RndSand(i, j) + sv);
                        p8.Mset(gi + 1, gj, RndSand(i + 0.5, j) + sv);
                        p8.Mset(gi, gj + 1, RndSand(i, j + 0.5) + sv);
                        p8.Mset(gi + 1, gj + 1, RndSand(i + 0.5, j + 0.5) + sv);
                    }
                    else
                    {
                        var u = Comp(i, j - 1, gr);
                        var d = Comp(i, j + 1, gr);
                        var l = Comp(i - 1, j, gr);
                        var r = Comp(i + 1, j, gr);
    
                        var b = gr == grrock ? 21 : gr == grwater ? 26 : 16;
    
                        p8.Mset(gi, gj, b + (l ? (u ? (Comp(i - 1, j - 1, gr) ? 17 + RndCenter(i, j) : 20) : 1) : (u ? 16 : 0)));
                        p8.Mset(gi + 1, gj, b + (r ? (u ? (Comp(i + 1, j - 1, gr) ? 17 + RndCenter(i + 0.5, j) : 19) : 1) : (u ? 18 : 2)));
                        p8.Mset(gi, gj + 1, b + (l ? (d ? (Comp(i - 1, j + 1, gr) ? 17 + RndCenter(i, j + 0.5) : 4) : 33) : (d ? 16 : 32)));
                        p8.Mset(gi + 1, gj + 1, b + (r ? (d ? (Comp(i + 1, j + 1, gr) ? 17 + RndCenter(i + 0.5, j + 0.5) : 3) : 33) : (d ? 18 : 34)));
    
                    }
                }
            }
    
            p8.Pal();
            if (levelUnder)
            {
                p8.Pal(15, 5);
                p8.Pal(4, 1);
            }
            p8.Map(64, 32, ci * 16, cj * 16, 18, 18);
    
            for (int i = ci - 1; i <= ci + 8; i++)
            {
                for (int j = cj - 1; j <= cj + 8; j++)
                {
                    var gr = GetDirectGr(i, j);
                    if (gr != null)
                    {
                        var gi = i * 16;
                        var gj = j * 16;
    
                        p8.Pal();
    
                        if (gr == grwater)
                        {
                            WatAnim(i, j);
                            WatAnim(i + 0.5, j);
                            WatAnim(i, j + 0.5);
                            WatAnim(i + 0.5, j + 0.5);
                        }
    
                        if (gr == grwheat)
                        {
                            var d = DirGetData(i, j, 0) - time;
                            for (int pp = 2; pp <= 4; pp++)
                            {
                                p8.Pal(pp, 3);
                                if (d > (10 - pp * 2)) { p8.Palt(pp, true); }
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
                                p8.Spr(31, gi, gj, 1, 2);
                                p8.Spr(31, gi + 8, gj, 1, 2, true);
                            }
                            p8.Palt();
                            p8.Spr(77, gi + 4, gj, 1, 2);
                        }
                    }
                }
            }
        }
    
        private void Panel(string name, int x, int y, int sx, int sy)
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
    
            var hx = x + (sx - name.Length * 4) / 2;
            p8.Rectfill(hx, y + 1, hx + name.Length * 4, y + 7, 13);
            p8.Print(name, hx + 1, y + 2, 7);
        }
    
        private void ItemName(double x, double y, Entity it, int col)
        {
            var ty = it.Type;
            p8.Pal();
            var px = x;
            if (it.Power != null)
            {
                var pwn = pwrNames[(int)it.Power - 1];
                p8.Print(pwn, x + 10, y, col);
                px += pwn.Length * 4 + 4;
                SetPal(pwrPal[(int)it.Power - 1]);
            }
            if (ty.Pal != null) { SetPal(ty.Pal); }
            p8.Spr(ty.Spr, x, y - 2);
            p8.Pal();
            p8.Print(ty.Name, px + 10, y, col);
        }
    
        private void List(Entity menu, int x, int y, int sx, int sy, int my)
        {
            Panel(menu.Type.Name, x, y, sx, sy);
    
            var tlist = menu.List.Count;
            if (tlist < 1)
            {
                return;
            }
    
            var sel = menu.Sel;
            if (menu.Off > Math.Max(0, sel - 4)) { menu.Off = Math.Max(0, sel - 4); }
            if (menu.Off < Math.Min(tlist, sel + 3) - my) { menu.Off = Math.Min(tlist, sel + 3) - my; }
    
            sel -= menu.Off;
    
            var debut = menu.Off + 1;
            var fin = Math.Min(menu.Off + my, tlist);
    
            var sely = y + 3 + (sel + 1) * 8;
            p8.Rectfill(x + 1, sely, x + sx - 3, sely + 6, 13);
    
            x += 5;
            y += 12;
    
            for (int i = debut - 1; i < fin; i++)
            {
                var it = menu.List[i];
                var py = y + (i - menu.Off) * 8;
                var col = 7;
                if ((it.Req != null) && !CanCraft(it))
                {
                    col = 0;
                }
    
                ItemName(x, py, it, col);
    
                if (it.Count != null)
                {
                    var c = $"{it.Count}";
                    p8.Print(c, x + sx - c.Length * 4 - 10, py, col);
                }
            }
    
            p8.Spr(68, x - 8, sely);
            p8.Spr(68, x + sx - 10, sely, 1, 1, true);
        }
    
        private void RequireList(Entity recip, int x, int y, int sx, int sy)
        {
            Panel("require", x, y, sx, sy);
            var tlist = recip.Req.Count;
            if (tlist < 1)
            {
                return;
            }
    
            x += 5;
            y += 12;
    
            for (int i = 0; i < tlist; i++)
            {
                var it = recip.Req[i];
                var py = y + i * 8;
                ItemName(x, py, it, 7);
    
                if (it.Count != null)
                {
                    var h = HowMany(invent, it);
                    var c = $"{h}/{it.Count}";
                    p8.Print(c, x + sx - c.Length * 4 - 10, py, h < it.Count ? 8 : 7);
                }
            }
        }
    
        public void Printb(string t, double x, double y, int c)
        {
            p8.Print(t, x + 1, y, 1);
            p8.Print(t, x - 1, y, 1);
            p8.Print(t, x, y + 1, 1);
            p8.Print(t, x, y - 1, 1);
            p8.Print(t, x, y, c);
        }
    
        private void Printc(string t, int x, int y, double c)
        {
            p8.Print(t, x - t.Length * 2, y, c);
        }
    
        private void Dent()
        {
            for (int i = 0; i < entities.Count; i++)
            {
                var e = entities[i];
                p8.Pal();
                if (e.Type.Pal != null) { SetPal(e.Type.Pal); }
                if (e.Type.BigSpr != null)
                {
                    p8.Spr((int)e.Type.BigSpr, e.X - 8, e.Y - 8, 2, 2);
                }
                else
                {
                    if (e.Type == etext)
                    {
                        Printb(e.Text, e.X - 2, e.Y - 4, e.C);
                    }
                    else
                    {
                        if (e.Timer != null && e.Timer < 45 && e.Timer % 4 > 2)
                        {
                            for (int j = 0; j <= 15; j++)
                            {
                                p8.Palt(j, true);
                            }
                        }
                        p8.Spr(e.Type.Spr, e.X - 4, e.Y - 4);
                    }
                }
            }
        }
    
        private void Sorty(List<Entity> t)
        {
            var tv = t.Count - 1;
            for (int i = 0; i < tv; i++)
            {
                var t1 = t[i];
                var t2 = t[i + 1];
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
    
            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
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
    
        private void Dbar(int px, int py, double v, double m, int c, int c2)
        {
            p8.Pal();
            var pe = px + v * 0.3;
            var pe2 = px + m * 0.3;
            p8.Rectfill(px - 1, py - 1, px + 30, py + 4, 0);
            p8.Rectfill(px, py, pe - 1, py + 3, c2);
            p8.Rectfill(px, py, Math.Max(px, pe - 2), py + 2, c);
            if (m > v) { p8.Rectfill(pe, py, pe2 - 1, py + 3, 10); }
        }
    
        public void Draw()
        {
            if (curMenu != null && curMenu.Spr != null)
            {
                p8.Camera();
                p8.Palt(0, false);
                p8.Rectfill(0, 0, 128, 46, 12);
                p8.Rectfill(0, 46, 128, 128, 1);
                p8.Spr((int)curMenu.Spr, 32, 14, 8, 8);
                Printc(curMenu.Text, 64, 80, 6);
                Printc(curMenu.Text2, 64, 90, 6);
                Printc("press button 1", 64, 112, 6 + time % 2);
                time += 0.1;
                return;
            }

            p8.Cls();
    
            p8.Camera(clx - 64, cly - 64);
    
            DrawBack();
    
            Dent();
    
            Denemies();
    
            p8.Camera();
            Dbar(4, 4, plife, llife, 8, 2);
            Dbar(4, 9, Math.Max(0, pstam), lstam, 11, 3);
    
            if (curItem != null)
            {
                var ix = 35;
                var iy = 3;
                ItemName(ix + 1, iy + 3, curItem, 7);
                if (curItem.Count != null)
                {
                    var c = $"{curItem.Count}";
                    p8.Print(c, ix + 88 - 16, iy + 3, 7);
                }
            }

            if (curMenu != null)
            {
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
                        var curgoal = curMenu.List[curMenu.Sel];
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
}
