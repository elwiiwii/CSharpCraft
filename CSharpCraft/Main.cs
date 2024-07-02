using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using CSharpCraft;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CSharpCraft
{
    public class Material
    {
        public string? Name { get; set; }
        public int? Spr { get; set; }
        public int[]? Pal { get; set; }
        public bool? Becraft { get; set; }
        public int Bigspr { get; set; }
        public bool Drop { get; set; }
    }

    public class Ground
    {
        public double Id { get; set; }
        public double Gr { get; set; }
        public Material? Mat { get; set; }
        public Ground? Tile { get; set; }
        public double Life { get; set; }
        public bool Istree { get; set; }
        public int[]? Pal { get; set; }
    }

    public class Level
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Sx { get; set; }
        public double Sy { get; set; }
        public bool Isunder { get; set; }
        public double Stx { get; set; }
        public double Sty { get; set; }
        public List<Entity> Ent { get; set; }
        public List<Entity> Ene { get; set; }
        public double[] Dat { get; set; }
    }

    public class Entity
    {
        public Material? Type { get; set; }
        public int? Count { get; set; }
        public List<Entity>? List { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }
        public string? Text { get; set; }
        public string? Text2 { get; set; }
        public double Timer { get; set; }
        public double C { get; set; }
        public int Power { get; set; }
        public Entity[]? Req { get; set; }
        public double Sel { get; set; }
        public double Off { get; set; }
        public int Spr { get; set; }
        public double Life { get; set; }
        public double Prot { get; set; }
        public double Lrot { get; set; }
        public double Panim { get; set; }
        public double Banim { get; set; }
        public double Dtim { get; set; }
        public double Step { get; set; }
        public double Ox { get; set; }
        public double Oy { get; set; }
        public bool Hascol { get; set; }
        public Material Giveitem { get; set; }
    }

    class FNAGame : Game
    {
        [STAThread]
        static void Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            using FNAGame g = new();
            g.Run();
        }

#nullable enable
        private SpriteBatch batch;
        private Texture2D pixel;
        private Texture2D SpriteSheet1;
        private Pico8Functions pico8Functions;
        private readonly GraphicsDeviceManager graphics;
        private double frameRate = 0.0;
        private int frameCounter = 0;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        private bool lb4 = false;
        private bool lb5 = false;
        private bool block5 = false;

        private double time;

        private double plx;
        private double ply;
        private double prot;
        private double lrot;
        private double panim;
        private double banim;
        private double pstam;
        private double lstam;
        private double plife;
        private double llife;

        private Level currentlevel;

        private bool levelunder = false;
        private double levelsx;
        private double levelsy;
        private double levelx;
        private double levely;
        private double[] data = [8192];
        private double holex;
        private double holey;
        private double clx;
        private double cly;
        private double cmx;
        private double cmy;

        private double[][] level;
        readonly int[] typecount = new int[11];

        private readonly Level currentLevel;

        private int[][] Rndwat = new int[16][];

        private double coffx;
        private double coffy;

        private bool switchlevel = false;
        private bool canswitchlevel = false;

        private Level cave;
        private Level island;

        private double stamcost;

        private int[] nearenemies;

        private List<Entity> entities = new();

        private int enstep_wait = 0;
        private int enstep_walk = 1;
        private int enstep_chase = 2;
        private int enstep_patrol = 3;

        private List<Entity> invent = new();

        private List<Entity> enemies = new();

        List<Entity> furnacerecipe;
        List<Entity> workbenchrecipe;
        List<Entity> stonebenchrecipe;
        List<Entity> anvilrecipe;
        List<Entity> factoryrecipe;
        List<Entity> chemrecipe;

        List<Entity> Ent;
        List<Entity> Ene;
        private double[] Dat;

        private Entity curmenu;

        private int tooglemenu;
        private Entity curitem;
        private Entity menuinvent;

#nullable disable

        static readonly string[] pwrnames = ["wood", "stone", "iron", "gold", "gem"];
        static readonly int[][] pwrpal = [[2, 2, 4, 4], [5, 2, 4, 13], [13, 5, 13, 6], [9, 2, 9, 10], [13, 2, 14, 12]];

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
        //apple.Givelife = 20;
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
        //potion.Givelife = 100

        static readonly Material ironbar = Item("iron bar", 119, piron);
        static readonly Material goldbar = Item("gold bar", 119, pgold);
        static readonly Material bread = Item("bread", 119, [1, 4, 15, 7]);
        //bread.Givelife = 40

        static readonly Material workbench = Bigspr(104, Item("workbench", 89, [1, 4, 9], true));
        static readonly Material stonebench = Bigspr(104, Item("stonebench", 89, [1, 6, 13], true));
        static readonly Material furnace = Bigspr(106, Item("furnace", 90, null, true));
        static readonly Material anvil = Bigspr(108, Item("anvil", 91, null, true));
        static readonly Material factory = Bigspr(71, Item("factory", 74, null, true));
        static readonly Material chem = Bigspr(78, Item("chem lab", 76, null, true));
        static readonly Material chest = Bigspr(110, Item("chest", 92));

        static readonly Material inventary = Item("inventory", 89);
        static readonly Material pickuptool = Item("pickup tool", 73);

        static readonly Material etext = Item("text", 103);
        static readonly Material player = Item(null, 1);
        static readonly Material zombi = Item(null, 2);

        static readonly Ground grwater = new() { Id = 0, Gr = 0 };
        static readonly Ground grsand = new() { Id = 1, Gr = 1 };
        static readonly Ground grgrass = new() { Id = 2, Gr = 2 };
        static readonly Ground grrock = new() { Id = 3, Gr = 3, Mat = stone, Tile = grsand, Life = 15 };
        static readonly Ground grtree = new() { Id = 4, Gr = 2, Mat = wood, Tile = grgrass, Life = 8, Istree = true, Pal = [1, 5, 3, 11] };
        static readonly Ground grfarm = new() { Id = 5, Gr = 1 };
        static readonly Ground grwheat = new() { Id = 6, Gr = 1 };
        static readonly Ground grplant = new() { Id = 7, Gr = 2 };
        static readonly Ground griron = new() { Id = 8, Gr = 1, Mat = iron, Tile = grsand, Life = 45, Istree = true, Pal = [1, 1, 13, 6] };
        static readonly Ground grgold = new() { Id = 9, Gr = 1, Mat = gold, Tile = grsand, Life = 80, Istree = true, Pal = [1, 2, 9, 10] };
        static readonly Ground grgem = new() { Id = 10, Gr = 1, Mat = gem, Tile = grsand, Life = 160, Istree = true, Pal = [1, 2, 14, 12] };
        static readonly Ground grhole = new() { Id = 11, Gr = 1 };

        private Ground lastground = grsand;

        private readonly Ground[] grounds = { grwater, grsand, grgrass, grrock, grtree, grfarm, grwheat, grplant, griron, grgold, grgem, grhole };

        private Entity mainmenu = Cmenu(inventary, null, 128, "by nusan", "2016");
        private Entity intromenu = Cmenu(inventary, null, 136, "a storm leaved you", "on a deserted island");
        private Entity deathmenu = Cmenu(inventary, null, 128, "you died", "alone ...");
        private Entity winmenu = Cmenu(inventary, null, 136, "you successfully escaped", "from the island");

        private FNAGame()
        {
            graphics = new GraphicsDeviceManager(this);

            // Allow the user to resize the window
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

            // All graphics loaded will be in a "Graphics" folder
            Content.RootDirectory = "Graphics";

            graphics.PreferredBackBufferWidth = 512;
            graphics.PreferredBackBufferHeight = 512;
            graphics.IsFullScreen = false;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30.0);
            graphics.SynchronizeWithVerticalRetrace = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            UpdateViewport();

            Array.Copy(pico8Functions.colors, pico8Functions.resetColors, pico8Functions.colors.Length);
            Array.Copy(pico8Functions.colors, pico8Functions.sprColors, pico8Functions.colors.Length);

            pico8Functions.Palt();

            furnacerecipe = [];
            workbenchrecipe = [];
            stonebenchrecipe = [];
            anvilrecipe = [];
            factoryrecipe = [];
            chemrecipe = [];

            pico8Functions.Add(factoryrecipe, Recipe(Instc(sail, 1), [Instc(fabric, 3), Instc(glue, 1)]));
            pico8Functions.Add(factoryrecipe, Recipe(Instc(boat), [Instc(wood, 30), Instc(ironbar, 8), Instc(glue, 5), Instc(sail, 4)]));

            pico8Functions.Add(chemrecipe, Recipe(Instc(glue, 1), [Instc(glass, 1), Instc(ichor, 3)]));
            pico8Functions.Add(chemrecipe, Recipe(Instc(potion, 1), [Instc(glass, 1), Instc(ichor, 1)]));

            pico8Functions.Add(furnacerecipe, Recipe(Instc(ironbar, 1), [Instc(iron, 3)]));
            pico8Functions.Add(furnacerecipe, Recipe(Instc(goldbar, 1), [Instc(gold, 3)]));
            pico8Functions.Add(furnacerecipe, Recipe(Instc(glass, 1), [Instc(sand, 3)]));
            pico8Functions.Add(furnacerecipe, Recipe(Instc(bread, 1), [Instc(wheat, 5)]));

            Material[] tooltypes = [haxe, pick, sword, shovel, scythe];
            int[] quant = [5, 5, 7, 7, 7];
            int[] pows = [1, 2, 3, 4, 5];
            Material[] materials = [wood, stone, ironbar, goldbar, gem];
            int[] mult = [1, 1, 1, 1, 3];
            List<Entity>[] crafter = [workbenchrecipe, stonebenchrecipe, anvilrecipe, anvilrecipe, anvilrecipe];
            for (int j = 0; j < pows.Length; j++)
            {
                for (int i = 0; i < tooltypes.Length; i++)
                {
                    pico8Functions.Add(crafter[j], Recipe(Setpower(pows[j], Instc(tooltypes[i])), [Instc(materials[j], quant[i] * mult[j])]));
                }
            }

            pico8Functions.Add(workbenchrecipe, Recipe(Instc(workbench, null, workbenchrecipe), [Instc(wood, 15)]));
            pico8Functions.Add(workbenchrecipe, Recipe(Instc(stonebench, null, stonebenchrecipe), [Instc(stone, 15)]));
            pico8Functions.Add(workbenchrecipe, Recipe(Instc(factory, null, factoryrecipe), [Instc(wood, 15), Instc(stone, 15)]));
            pico8Functions.Add(workbenchrecipe, Recipe(Instc(chem, null, chemrecipe), [Instc(wood, 10), Instc(glass, 3), Instc(gem, 10)]));
            pico8Functions.Add(workbenchrecipe, Recipe(Instc(chest), [Instc(wood, 15), Instc(stone, 10)]));

            pico8Functions.Add(stonebenchrecipe, Recipe(Instc(anvil, null, anvilrecipe), [Instc(iron, 25), Instc(wood, 10), Instc(stone, 25)]));
            pico8Functions.Add(stonebenchrecipe, Recipe(Instc(furnace, null, furnacerecipe), [Instc(wood, 10), Instc(stone, 15)]));

            //curmenu = mainmenu;

            Resetlevel();
        }

        private static Material Item(string n, int s, int[] p = null, bool? bc = null)
        {
            return new() { Name = n, Spr = s, Pal = p, Becraft = bc };
        }

        private Entity Inst(Material it)
        {
            return new() { Type = it };
        }

        private Entity Instc(Material it, int? c = null, List<Entity> l = null)
        {
            return new() { Type = it, Count = c, List = l }; 
        }

        private Entity Setpower(int v, Entity i)
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
            return Entity(it, xx, yy, new Random().Next(3) - 1.5, new Random().Next(3) - 1.5);
        }

        private Entity Settext(string t, double c, double time, Entity e)
        {
            e.Text = t;
            e.Timer = time;
            e.C = c;
            return e;
        }

        private static Material Bigspr(int spr, Material ent)
        {
            ent.Bigspr = spr;
            ent.Drop = true;
            return ent;
        }

        private Entity Recipe(Entity m, Entity[] require)
        {
            return new() { Type = m.Type, Power = m.Power, Count = m.Count, Req = require, List = m.List };
        }

        private bool Cancraft(Entity req)
        {
            var can = true;
            for (int i = 0; i < req.Req.Length; i++)
            {
                if (Howmany(invent, req.Req[i]) < req.Req[i].Count)
                {
                    can = false;
                    break;
                }
            }
            return can;
        }

        private void Craft(Entity req)
        {
            for (int i = 0; i < req.Req.Length; i++)
            {
                Reminlist(invent, req.Req[i]);
            }
            Additeminlist(invent, Setpower(req.Power, Instc(req.Type, req.Count, req.List)), 0);
        }

        private void Setpal(int[] l)
        {
            for (int i = 0; i < l.Length; i++)
            {
                pico8Functions.Pal(i + 1, l[i]);
            }
        }

        private static Entity Cmenu(Material t, List<Entity>? l, int? s = null, string te1 = null, string te2 = null)
        {
            return new() { List = l, Type = t, Sel = 1, Off = 0, Spr = (int)s, Text = te1, Text2 = te2 };
        }

        private int Howmany(List<Entity> list, Entity it)
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

        private Entity Isinlist(List<Entity> list, Entity it)
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

        private void Reminlist(List<Entity> list, Entity elem)
        {
            var it = Isinlist(list, elem);
            if (it == null)
            {
                return;
            }
            if (it.Count != null)
            {
                it.Count -= elem.Count;
                if (it.Count <= 0)
                {
                    pico8Functions.Del(list, it);
                }
            }
            else
            {
                pico8Functions.Del(list, it);
            }
        }

        private void Additeminlist(List<Entity> list, Entity it, int p)
        {
            var it2 = Isinlist(list, it);
            if (it2 == null || it2.Count == null)
            {
                Addplace(list, it, p);
            }
            else
            {
                it2.Count += it.Count;
            }
        }

        private void Addplace(List<Entity> l, Entity e, int p)
        {
            if (p < l.Count && p > 0)
            {
                for (int i = l.Count; i > p; i--)
                {
                    l[i + 1] = l[i];
                }
                l[p] = e;
            }
            else
            {
                pico8Functions.Add(l, e);
            }
        }

        private bool Isin(Entity e, int size)
        {
            return e.X > clx - size && e.X < clx + size && e.Y > cly - size && e.Y < cly + size;
        }

        private double Lerp(double a, double b, double alpha)
        {
            return a * (1.0 - alpha) + b * alpha;
        }

        private double Getinvlen(double x, double y)
        {
            return 1 / Getlen(x, y);
        }

        private double Getlen(double x, double y)
        {
            return Math.Sqrt(x * x + y * y + 0.001);
        }

        private double Getrot(double dx, double dy)
        {
            return dy >= 0 ? (dx + 3) * 0.25 : (1 - dx) * 0.25;
        }

        private double Normgetrot(double dx, double dy)
        {
            var l = 1 / Math.Sqrt(dx * dx + dy * dy + 0.001);
            return Getrot(dx * l, dy * l);
        }

        private void Fillene(Level l)
        {
            l.Ene = [Entity(player, 0, 0, 0, 0)];
            enemies = l.Ene;
            for (int i = 0; i <= levelsx - 1; i++)
            {
                for (int j = 0; j <= levelsy - 1; j++)
                {
                    var c = Getdirectgr(i, j);
                    var r = new Random().Next(100);
                    var ex = i * 16 + 8;
                    var ey = j * 16 + 8;
                    var dist = Math.Max(Math.Abs(ex - plx), Math.Abs(ey - ply));
                    if (r < 3 && c != grwater && c != grrock && !c.Istree && dist > 50)
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
                        pico8Functions.Add(l.Ene, newe);
                    }
                }
            }
        }

        private Level Createlevel(int xx, int yy, int sizex, int sizey, bool isUnderground)
        {
            var l = new Level {X = xx, Y = yy, Sx = sizex, Sy = sizey, Isunder = isUnderground, Ent = [], Ene = [], Dat = new double[8192]};
            Setlevel(l);
            levelunder = isUnderground;
            Createmap();
            Fillene(l);
            l.Stx = (holex - levelx) * 16 + 8;
            l.Sty = (holey - levely) * 16 + 8;
            return l;
        }

        private void Setlevel(Level l)
        {
            currentlevel = l;
            levelx = l.X;
            levely = l.Y;
            levelsx = l.Sx;
            levelsy = l.Sy;
            levelunder = l.Isunder;
            entities = l.Ent;
            enemies = l.Ene;
            data = l.Dat;
            plx = l.Stx;
            ply = l.Sty;
        }

        private void Resetlevel()
        {
            prot = 0.0;
            lrot = 0.0;

            panim = 0.0;

            pstam = 100;
            lstam = pstam;
            plife = 100;
            llife = plife;

            banim = 0.0;

            coffx = 0;
            coffy = 0;

            time = 0;

            tooglemenu = 0;
            invent = [];
            curitem = null;
            switchlevel = false;
            canswitchlevel = false;
            //menuinvent = Cmenu(inventary, invent);

            for (int i = 0; i <= 15; i++)
            {
                Rndwat[i] = new int[16];
                for (int j = 0; j <= 15; j++)
                {
                    Rndwat[i][j] = new Random().Next(100);
                }
            }

            cave = Createlevel(64, 0, 32, 32, true); // cave
            island = Createlevel(0, 0, 64, 64, false); // island

            var tmpworkbench = Entity(workbench, plx, ply, 0, 0);
            tmpworkbench.Hascol = true;
            tmpworkbench.List = workbenchrecipe;

            pico8Functions.Add(invent, tmpworkbench);
            pico8Functions.Add(invent, Inst(pickuptool));
        }

        private (int, int) Getmcoord(double x, double y)
        {
            return ((int)Math.Floor(x / 16), (int)Math.Floor(y / 16));
        }

        private bool Isfree(double x, double y, Entity? e = null)
        {
            var gr = Getgr(x, y);
            return !(gr.Istree || gr == grrock);
        }

        private bool Isfreeenem(double x, double y)
        {
            var gr = Getgr(x, y);
            return !(gr.Istree || gr == grrock || gr == grwater);
        }

        private bool Iscool(double x, double y)
        {
            return !Isfree(x, y);
        }

        private Ground Getgr(double x, double y)
        {
            var (i, j) = Getmcoord(x, y);
            return Getdirectgr(i, j);
        }

        private Ground Getdirectgr(double i, double j)
        {
            if (i < 0 || j < 0 || i >= levelsx || j >= levelsy) { return grounds[0]; }
            //Console.WriteLine(pico8Functions.Mget(i + levelx, j));
            return grounds[pico8Functions.Mget(i + levelx, j)];
        }

        private void Setgr(double x, double y, Ground v)
        {
            var (i, j) = Getmcoord(x, y);
            if (i < 0 || j < 0 || i >= levelsx || j >= levelsy) { return; }
            pico8Functions.Mset(i + levelx, j, v.Id);
        }

        private double Dirgetdata(double i, double j, double @default)
        {
            int iFlr = (int)Math.Floor(i);
            int jFlr = (int)Math.Floor(j);
            int levelsxFlr = (int)Math.Floor(levelsx);

            int g = iFlr + jFlr * levelsxFlr;
            if (data[g] == 0)
            {
                data[g] = @default;
            }
            Console.WriteLine(data[g]);
            return data[g];
        }

        private void Dirsetdata(double i, double j, double v)
        {
            data[(int)(i + j * levelsx)] = v;
        }

        private double Getdata(double x, double y, double @default)
        {
            var (i, j) = Getmcoord(x, y);
            if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
            {
                return @default;
            }
            return Dirgetdata(i, j, @default);
        }

        private void Setdata(double x, double y, double v)
        {
            var (i, j) = Getmcoord(x, y);
            if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
            {
                return;
            }
            Dirsetdata(i, j, v);
        }

        private void Cleardata(double x, double y)
        {
            var (i, j) = Getmcoord(x,y);
            if (i < 0 || j < 0 || i > levelsx -1 || j > levelsy - 1)
            {
                return;
            }
            data[(int)(i + j * levelsx)] = 0; // original code has null
        }

        private double Loop(double sel, List<Entity> l)
        {
            var lp = l.Count;
            return (sel - 1) % lp % lp + 1;
        }

        private bool Entcolfree(double x, double y, Entity e)
        {
            return Math.Max(Math.Abs(e.X - x), Math.Abs(e.Y - y)) > 8;
        }

        private (double, double) Reflectcol(double x, double y, double dx, double dy, Func<double,double,Entity?,bool> checkfun, double dp, Entity? e = null)
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

        private void Additem(Material mat, int count, double hitx, double hity)
        {
            for (int i = 0; i < count; i++)
            {
                var gi = Rentity(mat, Math.Floor(hitx / 16) * 16 + new Random().Next(14) + 1, Math.Floor(hity / 16) * 16 + new Random().Next(14) + 1);
                gi.Giveitem = mat;
                gi.Hascol = true;
                gi.Timer = 110 + new Random().Next(20);
                pico8Functions.Add(entities, gi);
            }
        }

        public void Upground()
        {
            var ci = (int)Math.Floor((clx - 64) / 16);
            var cj = (int)Math.Floor((cly - 64) / 16);
            for (int i = ci; i < ci + 8; i++)
            {
                for (int j = cj; j < cj + 8; j++)
                {
                    var gr = Getdirectgr(i, j);
                    if (gr == grfarm)
                    {
                        var d = Dirgetdata(i, j, 0);
                        if (time > d)
                        {
                            pico8Functions.Mset(i + levelx, j, grsand.Id);
                        }
                    }
                }
            }
        }

        private double Uprot(double grot, double rot)
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
            rot = -rot * 2 * Math.PI;

            var cr = Math.Cos(rot);
            var sr = Math.Sin(rot);
            var cv = -sr;
            var sv = cr;

            x = Math.Floor(x);
            y = Math.Floor(y - 4);

            var lan = Math.Sin(anim * 2) * 1.5;
            var bel = Getgr(x, y);

            if (bel == grwater)
            {
                y += 4;
                pico8Functions.Circ(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, 3, 6);
                pico8Functions.Circ(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, 3, 6);

                var anc = 3 + time * 3 % 1 * 3;
                pico8Functions.Circ(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, anc, 6);
                pico8Functions.Circ(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, anc, 6);
            }
            else
            {
                pico8Functions.Circfill(x + cv * 2 - cr * lan, y + 3 + sv * 2 - sr * lan, 3, 1);
                pico8Functions.Circfill(x - cv * 2 + cr * lan, y + 3 - sv * 2 + sr * lan, 3, 1);
            }

            var blade = (rot + 0.25) % 1;

            if (subanim > 0)
            {
                blade = blade - 0.3 + subanim * 0.04;
            }

            var bcr = Math.Cos(blade);
            var bsr = Math.Sin(blade);

            (int mx, int my) = Mirror(blade);

            var weap = 75;

            if (isplayer && curitem != null)
            {
                pico8Functions.Pal();
                weap = (int)curitem.Type.Spr;
                if (curitem.Power != null)
                {
                    Setpal(pwrpal[curitem.Power]);
                }
                if (curitem.Type != null && curitem.Type.Pal != null)
                {
                    Setpal(curitem.Type.Pal);
                }
            }

            pico8Functions.Spr(weap, x + bcr * 4 - cr * lan - mx * 8 + 1, y + bsr * 4 - sr * lan + my * 8 - 7 - 8, 1, 1, mx == 1, my == 1);

            if (isplayer) { pico8Functions.Pal(); }

            if (bel != grwater)
            {
                pico8Functions.Circfill(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, 3, 2);
                pico8Functions.Circfill(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, 3, 2);

                (int mx2, int my2) = Mirror((rot + 0.75) % 1);
                pico8Functions.Spr(75, x + cv * 4 + cr * lan - 8 + mx2 * 8 + 1, y + sv * 4 + sr * lan + my2 * 8 - 7 - 8, 1, 1, mx2 == 0, my2 == 1);
            }

            pico8Functions.Circfill(x + cr, y + sr - 2, 4, 2);
            pico8Functions.Circfill(x + cr, y + sr, 4, 2);
            pico8Functions.Circfill(x + cr * 1.5, y + sr * 1.5 - 2, 2.5, 15);
            pico8Functions.Circfill(x - cr, y - sr - 3, 3, 4);

        }

        private double[][] Noise(double sx, double sy, double startscale, double scalemod, double featstep)
        {
            int sxFlr = (int)Math.Floor(sx);
            int syFlr = (int)Math.Floor(sy);
            int featstepFlr = (int)Math.Floor(featstep);

            double[][] n = new double[sxFlr + 1][];

            for (int i = 0; i <= sxFlr; i++)
            {
                n[i] = new double[syFlr + 1];
                for (int j = 0; j <= syFlr; j++)
                {
                    n[i][j] = 0.5;
                }
            }

            var step = sxFlr;
            var scale = startscale;

            while (step > 1)
            {
                var cscal = scale;
                if (step == featstepFlr) { cscal = 1; }

                for (int i = 0; i <= sxFlr - 1; i += step)
                {
                    for (int j = 0; j <= syFlr - 1; j += step)
                    {
                        var c1 = n[i][j];
                        var c2 = n[i + step][j];
                        var c3 = n[i][j + step];
                        n[i + step / 2][j] = (c1 + c2) * 0.5 + (new Random().NextDouble() - 0.5) * cscal;
                        n[i][j + step / 2] = (c1 + c3) * 0.5 + (new Random().NextDouble() - 0.5) * cscal;
                    }
                }

                for (int i = 0; i <= sxFlr - 1; i += step)
                {
                    for (int j = 0; j <= syFlr - 1; j += step)
                    {
                        var c1 = n[i][j];
                        var c2 = n[i + step][j];
                        var c3 = n[i][j + step];
                        var c4 = n[i + step][j + step];
                        n[i + step / 2][j + step / 2] = (c1 + c2 + c3 + c4) * 0.25 + (new Random().NextDouble() - 0.5) * cscal;
                    }
                }

                step /= 2;
                scale *= scalemod;
            }

            return n;
        }

        private double[][] Createmapstep(double sx, double sy, double a, double b, double c, double d, double e)
        {
            int sxFlr = (int)Math.Floor(sx);
            int syFlr = (int)Math.Floor(sy);

            var cur = Noise(sxFlr, syFlr, 0.9, 0.2, sxFlr);
            var cur2 = Noise(sxFlr, syFlr, 0.9, 0.4, 8);
            var cur3 = Noise(sxFlr, syFlr, 0.9, 0.3, 8);
            var cur4 = Noise(sxFlr, syFlr, 0.8, 1.1, 4);

            for (int i = 0; i < 11; i++)
            {
                typecount[i] = 0;
            }

            for (int i = 0; i <= sxFlr; i++)
            {
                for (int j = 0; j <= syFlr; j++)
                {
                    var v = Math.Abs(cur[i][j] - cur2[i][j]);
                    var v2 = Math.Abs(cur[i][j] - cur3[i][j]);
                    var v3 = Math.Abs(cur[i][j] - cur4[i][j]);
                    var dist = Math.Max(Math.Abs((double)i / sxFlr - 0.5) * 2, Math.Abs((double)j / syFlr - 0.5) * 2);
                    dist = dist * dist * dist * dist;
                    //Math.Pow(dist, 5);
                    var coast = v * 4 - dist * 4;

                    var id = a;
                    if (coast > 0.3) { id = b; } // sand
                    if (coast > 0.6) { id = c; } // grass
                    if (coast > 0.3 && v2 > 0.5) { id = d; } // stone
                    if (id == c && v3 > 0.5) { id = e; } // tree

                    typecount[(int)id] += 1;

                    cur[i][j] = id;
                }
            }

            return cur;
        }

        private void Createmap()
        {
            var needmap = true;

            while (needmap)
            {
                needmap = false;

                if (levelunder)
                {
                    level = Createmapstep(levelsx, levelsy, 3, 8, 1, 9, 10);

                    if (typecount[8] < 30) { needmap = true; }
                    if (typecount[9] < 20) { needmap = true; }
                    if (typecount[10] < 15) { needmap = true; }
                }
                else
                {
                    level = Createmapstep(levelsx, levelsy, 0, 1, 2, 3, 4);

                    if (typecount[3] < 30) { needmap = true; }
                    if (typecount[4] < 30) { needmap = true; }
                }

                if (!needmap)
                {
                    plx = -1;
                    ply = -1;

                    for (int i = 0; i <= 500; i++)
                    {
                        var depx = (int)Math.Floor(levelsx / 8 + new Random().NextDouble() * levelsx * 6 / 8);
                        var depy = (int)Math.Floor(levelsy / 8 + new Random().NextDouble() * levelsy * 6 / 8);
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
                    pico8Functions.Mset(i + levelx, j + levely, level[i][j]);
                }
            }

            holex = levelsx / 2 + levelx;
            holey = levelsy / 2 + levely;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    pico8Functions.Mset(holex + i, holey + j, levelunder ? 1 : 3);
                }
            }

            pico8Functions.Mset(holex, holey, 11);

            clx = plx;
            cly = ply;

            cmx = plx;
            cmy = ply;
        }

        private bool Comp(double i, double j, Ground gr)
        {
            var gr2 = Getdirectgr(i, j);
            return gr != null && gr2 != null && gr.Gr == gr2.Gr;
        }

        private int Watval(double i, double j)
        {
            return Rndwat[(int)Math.Floor(Math.Abs(i) * 2 % 16)][(int)Math.Floor(Math.Abs(j) * 2 % 16)];
        }

        private void Watanim(double i, double j)
        {
            var a = ((time * 0.6 + (double)Watval(i, j) / 100) % 1) * 19;
            if (a > 16) { pico8Functions.Spr(13 + a - 16, i * 16, j * 16); }
        }

        private double Rndcenter(double i, double j)
        {
            return (double)((int)Math.Floor((double)Watval(i, j) / 34) + 18) % 20;
        }

        private int Rndsand(double i, double j)
        {
            return (int)Math.Floor((double)Watval(i, j) / 34) + 1;
        }

        private int Rndtree(double i, double j)
        {
            return (int)Math.Floor((double)Watval(i, j) / 51) * 32;
        }

        private void Spr4(double i, double j, double gi, double gj, double a, double b, double c, double d, double off, Func<double,double,int> f)
        {
            pico8Functions.Spr(f(i, j + off) + a, gi, gj + 2 * off);
            pico8Functions.Spr(f(i + 0.5, j + off) + b, gi + 8, gj + 2 * off);
            pico8Functions.Spr(f(i, j + 0.5 + off) + c, gi, gj + 8 + 2 * off);
            pico8Functions.Spr(f(i + 0.5, j + 0.5 + off) + d, gi + 8, gj + 8 + 2 * off);
        }
        
        private void Drawback()
        {
            var ci = (int)Math.Floor((clx - 64) / 16);
            var cj = (int)Math.Floor((cly - 64) / 16);

            for (int i = ci; i <= ci + 8; i++)
            {
                for (int j = cj; j <= cj + 8; j++)
                {
                    var gr = Getdirectgr(i, j);

                    var gi = (i - ci) * 2 + 64;
                    var gj = (j - cj) * 2 + 32;

                    if (gr != null && gr.Gr == 1) // sand
                    {
                        var sv = 0;
                        if (gr == grfarm || gr == grwheat) { sv = 3; }
                        pico8Functions.Mset(gi, gj, Rndsand(i, j) + sv);
                        pico8Functions.Mset(gi + 1, gj, Rndsand(i + 0.5, j) + sv);
                        pico8Functions.Mset(gi, gj + 1, Rndsand(i, j + 0.5) + sv);
                        pico8Functions.Mset(gi + 1, gj + 1, Rndsand(i + 0.5, j + 0.5) + sv);
                    }
                    else
                    {
                        var u = Comp(i, j - 1, gr);
                        var d = Comp(i, j + 1, gr);
                        var l = Comp(i - 1, j, gr);
                        var r = Comp(i + 1, j, gr);

                        var b = gr == grrock ? 21 : gr == grwater ? 26 : 16;

                        pico8Functions.Mset(gi, gj, b + (l ? (u ? (Comp(i - 1, j - 1, gr) ? 17 + Rndcenter(i, j) : 20) : 1) : (u ? 16 : 0)));
                        pico8Functions.Mset(gi + 1, gj, b + (r ? (u ? (Comp(i + 1, j - 1, gr) ? 17 + Rndcenter(i + 0.5, j) : 19) : 1) : (u ? 18 : 2)));
                        pico8Functions.Mset(gi, gj + 1, b + (l ? (d ? (Comp(i - 1, j + 1, gr) ? 17 + Rndcenter(i, j + 0.5) : 4) : 33) : (d ? 16 : 32)));
                        pico8Functions.Mset(gi + 1, gj + 1, b + (r ? (d ? (Comp(i + 1, j + 1, gr) ? 17 + Rndcenter(i + 0.5, j + 0.5) : 3) : 33) : (d ? 18 : 34)));

                    }
                }
            }

            pico8Functions.Pal();

            if (levelunder)
            {
                pico8Functions.Pal(15, 5);
                pico8Functions.Pal(4, 1);
            }

            pico8Functions.Map(64, 32, ci * 16, cj * 16, 18, 18);
            
            for (int i = ci - 1; i <= ci + 8; i++)
            {
                for (int j = cj - 1; j <= cj + 8; j++)
                {
                    var gr = Getdirectgr(i, j);

                    if (gr != null)
                    {
                        var gi = i * 16;
                        var gj = j * 16;

                        pico8Functions.Pal();

                        if (gr == grwater)
                        {
                            Watanim(i, j);
                            Watanim(i + 0.5, j);
                            Watanim(i, j + 0.5);
                            Watanim(i + 0.5, j + 0.5);
                        }

                        if (gr == grwheat)
                        {
                            var d = Dirgetdata(i, j, 0) - time;
                            for (int pp = 2; pp <= 4; pp++)
                            {
                                pico8Functions.Pal(pp, 3);
                                if (d > (10 - pp * 2)) { pico8Functions.Palt(pp, true); }
                            }
                            if (d < 0) { pico8Functions.Pal(4, 9); }
                            Spr4(i, j, gi, gj, 6, 6, 6, 6, 0, Rndsand);
                        }

                        if (gr.Istree)
                        {
                            Setpal(gr.Pal);

                            Spr4(i, j, gi, gj, 64, 65, 80, 81, 0, Rndtree);
                        }

                        if (gr == grhole)
                        {
                            pico8Functions.Pal();
                            if (!levelunder)
                            {
                                pico8Functions.Palt(0, false);
                                pico8Functions.Spr(31, gi, gj, 1, 2);
                                pico8Functions.Spr(31, gi + 8, gj, 1, 2, true); // changed +8 to +16 because of a temporary spr change
                            }
                            pico8Functions.Palt();
                            pico8Functions.Spr(77, gi + 4, gj, 1, 2);
                        }
                    }
                }
            }
        }

        private void Panel(string name, double x, double y, double sx, double sy)
        {
            pico8Functions.Rectfill(x + 8, y + 8, x + sx - 9, y + sy - 9, 1);
            pico8Functions.Spr(66, x, y);
            pico8Functions.Spr(67, x + sx - 8, y);
            pico8Functions.Spr(82, x, y + sy - 8);
            pico8Functions.Spr(83, x + sx - 8, y + sy - 8);
            pico8Functions.Sspr(24, 32, 4, 8, x + 8, y, sx - 16, 8);
            pico8Functions.Sspr(24, 40, 4, 8, x + 8, y + sy - 8, sx - 16, 8);
            pico8Functions.Sspr(16, 36, 8, 4, x, y + 8, 8, sy - 16);
            pico8Functions.Sspr(24, 36, 8, 4, x + sx - 8, y + 8, 8, sy - 16);

            var hx = x + (sx - name.Length * 4) / 2;
            pico8Functions.Rectfill(hx, y + 1, hx + name.Length * 4, y + 7, 13);
            pico8Functions.Print(name, hx + 1, y + 2, 7);
        }

        private void Itemname(double x, double y, Entity it, int col)
        {
            var ty = it.Type;
            pico8Functions.Pal();
            var px = x;
            if (it.Power != null)
            {
                var pwn = pwrnames[it.Power];
                pico8Functions.Print(pwn, x + 10, y, col);
                px += pwn.Length * 4 + 4;
                Setpal(pwrpal[it.Power]);
            }
            if (ty.Pal != null) { Setpal(ty.Pal); }
            pico8Functions.Spr((double)ty.Spr, x, y - 2);
            pico8Functions.Pal();
            pico8Functions.Print(ty.Name, px + 10, y, col);
        }

        private void List(Entity menu, double x, double y, double sx, double sy, double my)
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

            var debut = (int)menu.Off + 1;
            var fin = Math.Min(menu.Off + my, tlist);

            var sely = y + 3 + sel * 8;
            pico8Functions.Rectfill(x + 1, sely, x + sx - 3, sely + 6, 13);

            x += 5;
            y += 12;

            for (int i = debut; i < fin; i++)
            {
                var it = menu.List[i];
                var py = y + (i - 1 - menu.Off) * 8;
                var col = 7;
                if ((it.Req != null) !& Cancraft(it))
                {
                    col = 0;
                }

                Itemname(x, py, it, col);

                if (it.Count != null)
                {
                    var c = $"{it.Count}";
                    pico8Functions.Print(c, x + sx - c.Length * 4 - 10, py, col);
                }
            }

            pico8Functions.Spr(68, x - 8, sely);
            pico8Functions.Spr(68, x + sx - 10, sely, 1, 1, true);
        }

        private void Requirelist(Entity recip, double x, double y, double sx, double sy)
        {
            Panel("require", x, y, sx, sy);
            var tlist = recip.Req.Length;
            if (tlist < 1)
            {
                return;
            }

            x += 5;
            y += 12;

            for (int i = 0; i < tlist; i++)
            {
                var it = recip.Req[i];
                var py = y + (i + 1) * 8;
                Itemname(x, py, it, 7);

                if (it.Count != null)
                {
                    var h = Howmany(invent, it);
                    var c = $"{h}/{it.Count}";
                    pico8Functions.Print(c, x + sx - c.Length * 4 - 10, py, h < it.Count ? 8 : 7);
                }
            }
        }

        public void Printb(string t, double x, double y, double c)
        {
            pico8Functions.Print(t, x + 1, y, 1);
            pico8Functions.Print(t, x - 1, y, 1);
            pico8Functions.Print(t, x, y + 1, 1);
            pico8Functions.Print(t, x, y - 1, 1);
            pico8Functions.Print(t, x, y, c);
        }

        private void Printc(string t, double x, double y, double c)
        {
            pico8Functions.Print(t, x - (t.Length * 2), y, c);
        }

        private void Dent()
        {
            for (int i = 0; i < entities.Count(); i++)
            {
                var e = entities[i];
                pico8Functions.Pal();
                if (e.Type.Pal != null) { Setpal(e.Type.Pal); }
                if (0 != 0) { }
                if (e.Type.Bigspr != null)
                {
                    pico8Functions.Spr(e.Type.Bigspr, e.X - 8, e.Y - 8, 2, 2);
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
                                pico8Functions.Palt(j, true);
                            }
                        }
                        pico8Functions.Spr((double)e.Type.Spr, e.X - 4, e.Y - 4);
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
                    pico8Functions.Pal();
                    Dplayer(plx, ply, prot, panim, banim, true);
                }
                else
                {
                    if (Isin(e, 72))
                    {
                        pico8Functions.Pal();
                        pico8Functions.Pal(15, 3);
                        pico8Functions.Pal(4, 1);
                        pico8Functions.Pal(2, 8);
                        pico8Functions.Pal(1, 1);

                        Dplayer(e.X, e.Y, e.Prot, e.Panim, e.Banim, false);
                    }
                }
            }
        }

        private void Dbar(double px, double py, double v, double m, double c, double c2)
        {
            pico8Functions.Pal();
            var pe = px + v * 0.299988;
            var pe2 = px + m * 0.299988;
            pico8Functions.Rectfill(px - 1, py - 1, px + 30, py + 4, 0);
            pico8Functions.Rectfill(px, py, pe, py + 3, c2);
            pico8Functions.Rectfill(px, py, Math.Max(px, pe - 1), py + 2, c);
            if (m > v) { pico8Functions.Rectfill(pe + 1, py, pe2, py + 3, 10); }
        }

        protected override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                frameRate = frameCounter / elapsedTime.TotalSeconds;
                elapsedTime = TimeSpan.Zero;
                frameCounter = 0;
                Console.WriteLine("FPS: " + frameRate); // Print the frame rate to the console
            }

            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Tab)) Resetlevel();
            //if (state.IsKeyDown(Keys.Tab)) Createlevel(0, 0, 32, 32, true);

            if (state.IsKeyDown(Keys.Q)) switchlevel = true;

            if (curmenu != null)
            {
                if (curmenu.Spr != null)
                {
                    if (state.IsKeyDown(Keys.Z))
                    {
                        if (curmenu == mainmenu)
                        {
                            curmenu = intromenu;
                        }
                        else
                        {
                            Resetlevel();
                            curmenu = null;
                        }
                    }
                    lb4 = state.IsKeyDown(Keys.Z);
                    return;
                }
                else
                {
                    var intmenu = curmenu;
                    var othmenu = menuinvent;
                    if (curmenu.Type == chest)
                    {
                        if (state.IsKeyDown(Keys.A)) { tooglemenu -= 1; }
                        if (state.IsKeyDown(Keys.D)) { tooglemenu += 1; }
                        tooglemenu = (tooglemenu % 2 + 2) & 2;
                        if (tooglemenu == 1)
                        {
                            intmenu = menuinvent;
                            othmenu = curmenu;
                        }
                    }

                    if (intmenu.List.Count > 0)
                    {
                        if (state.IsKeyDown(Keys.W)) { intmenu.Sel -= 1; }
                        if (state.IsKeyDown(Keys.S)) { intmenu.Sel += 1; }

                        intmenu.Sel = Loop(intmenu.Sel, intmenu.List);

                        if (state.IsKeyDown(Keys.X))
                        {
                            if (curmenu.Type == chest)
                            {
                                var el = intmenu.List[(int)intmenu.Sel];
                                pico8Functions.Del(intmenu.List, el);
                                Additeminlist(othmenu.List, el, (int)othmenu.Sel);
                                if (intmenu.List.Count > 0 && intmenu.Sel > intmenu.List.Count) { intmenu.Sel -= 1; }
                                if (intmenu == menuinvent && curitem == el)
                                {
                                    curitem = null;
                                }
                            }
                            else if ((bool)curmenu.Type.Becraft)
                            {
                                if (curmenu.Sel > 0 && curmenu.Sel <= intmenu.List.Count)
                                {
                                    var rec = curmenu.List[(int)curmenu.Sel];
                                    if (Cancraft(rec))
                                    {
                                        Craft(rec);
                                    }
                                    else
                                    {

                                    }
                                }
                            }
                            else
                            {
                                curitem = curmenu.List[(int)curmenu.Sel];
                                pico8Functions.Del(curmenu.List, curitem);
                                Additeminlist(curmenu.List, curitem, 1);
                                curmenu.Sel = 1;
                                curmenu = null;
                                block5 = true;
                            }
                        }
                    }
                }
                if (state.IsKeyDown(Keys.Z)! & lb4)
                {
                    curmenu = null;
                }
                lb4 = state.IsKeyDown(Keys.Z);
                lb5 = state.IsKeyDown(Keys.X);
                return;
            }

            if (switchlevel)
            {
                if (currentlevel == cave) { Setlevel(island); }
                else { Setlevel(cave); }
                plx = currentlevel.Stx;
                ply = currentlevel.Sty;
                Fillene(currentlevel);
                switchlevel = false;
                canswitchlevel = false;
            }

            if (curitem != null)
            {
                if (Howmany(invent, curitem) <= 0) { curitem = null; }
            }

            var playhit = Getgr(plx, ply);
            if (playhit != lastground && playhit == grwater) {  }
            lastground = playhit;
            var s = (playhit == grwater || pstam == 0) ? 1 : 2;
            if (playhit == grhole)
            {
                switchlevel = switchlevel || canswitchlevel;
            }
            else
            {
                canswitchlevel = true;
            }

            double dx = 0.0;
            double dy = 0.0;

            if (state.IsKeyDown(Keys.A)) dx -= 1.0;
            if (state.IsKeyDown(Keys.D)) dx += 1.0;
            if (state.IsKeyDown(Keys.W)) dy -= 1.0;
            if (state.IsKeyDown(Keys.S)) dy += 1.0;

            double dl = Getinvlen(dx, dy);
            
            dx *= dl;
            dy *= dl;
            
            if (Math.Abs(dx) > 0 || Math.Abs(dy) > 0)
            {
                lrot = Getrot(dx, dy);
                panim += 1.0 / 5.5;
            }
            else
            {
                panim = 0;
            }
            
            dx *= s;
            dy *= s;
            
            (dx, dy) = Reflectcol(plx, ply, dx, dy, Isfree, 0);

            var canact = true;

            var fin = entities.Count();
            for (int i = fin; i > 0; i--)
            {
                var e = entities[i - 1];
                if (e.Hascol)
                {
                    (e.Vx, e.Vy) = Reflectcol(e.X, e.Y, e.Vx, e.Vy, Isfree, 0.9);
                }
                e.X += e.Vx;
                e.Y += e.Vy;
                e.Vx *= 0.95;
                e.Vy *= 0.95;

                if (e.Timer != null && e.Timer < 1)
                {
                    pico8Functions.Del(entities, e);
                }
                else
                {
                    if (e.Timer != null) { e.Timer -= 1; }

                    var dist = Math.Max(Math.Abs(e.X - plx), Math.Abs(e.Y - ply));
                    if (e.Giveitem != null)
                    {
                        if (dist < 5)
                        {
                            if (e.Timer == null || e.Timer < 115)
                            {
                                var newit = Instc(e.Giveitem, 1);
                                Additeminlist(invent, newit, -1);
                                pico8Functions.Del(entities, e);
                                pico8Functions.Add(entities, Settext($"{Howmany(invent, newit)}", 11, 20, Entity(etext, e.X, e.Y - 5, 0, -1)));
                            }
                        }
                    }
                    else
                    {
                        if (e.Hascol)
                        {
                            (dx, dy) = Reflectcol(plx, ply, dx, dy, Entcolfree, 0, e);
                        }
                    }
                }
            }

            (dx, dy) = Reflectcol(plx, ply, dx, dy, Isfree, 0);

            plx += dx;
            ply += dy;
            
            prot = Uprot(lrot, prot);

            llife += Math.Max(-1, Math.Min(1, plife - llife));
            lstam += Math.Max(-1, Math.Min(1, pstam - lstam));

            if (state.IsKeyDown(Keys.X) && !block5 && canact)
            {
                var pxrot = -prot * 2 * Math.PI;
                var bx = Math.Cos(pxrot);
                var by = Math.Sin(pxrot);
                var hitx = plx + bx * 8;
                var hity = ply + by * 8;
                var hit = Getgr(hitx, hity);

                if (banim == 0 && pstam > 0 && canact)
                {
                    banim = 8;
                    stamcost = 20;
                    if (0 != 0)
                    {

                    }
                    else if (hit.Mat != null)
                    {
                        var pow = 1.0;

                        pow = Math.Floor(pow);

                        var d = Getdata(hitx, hity, hit.Life);
                        if (d - pow <= 0)
                        {
                            Setgr(hitx, hity, hit.Tile);
                            Cleardata(hitx, hity);
                        }
                        else
                        {
                            Setdata(hitx, hity, d - pow);
                        }
                        pico8Functions.Add(entities, Settext(pow.ToString(), 10, 20, Entity(etext, hitx, hity, 0, -1)));
                        Console.WriteLine(pow.ToString());
                    }
                    else
                    {

                    }
                    pstam -= stamcost;
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

            lb4 = state.IsKeyDown(Keys.Z);
            lb5 = state.IsKeyDown(Keys.X);
            if (!state.IsKeyDown(Keys.X))
            {
                block5 = false;
            }

            time += 1.0 / 30.0;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            frameCounter++;

            GraphicsDevice.Clear(Color.Black);

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);

            // Get the size of the viewport
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellWidth = viewportWidth / 128;
            int cellHeight = viewportHeight / 128;

            if (curmenu != null && curmenu.Spr != null)
            {
                pico8Functions.Camera();
                pico8Functions.Palt(0, false);
                pico8Functions.Rectfill(0, 0, 128, 46, 12);
                pico8Functions.Rectfill(0, 46, 128, 128, 1);
                pico8Functions.Spr(curmenu.Spr, 32, 14, 8, 8);
                Printc(curmenu.Text, 64, 80, 6);
                Printc(curmenu.Text2, 64, 90, 6);
                Printc("press button 1", 64, 112, 6 + time % 2);
                time += 0.1;
                return;
            }

            pico8Functions.Camera(clx - 64, cly - 64);

            Drawback();

            Dent();

            //Dplayer(plx, ply, prot, panim, banim, true);

            Denemies();

            pico8Functions.Camera();

            Dbar(4, 4, plife, llife, 8, 2);
            Dbar(4, 9, Math.Max(0, pstam), lstam, 11, 3);

            if (curitem != null)
            {
                var ix = 35;
                var iy = 3;
                Itemname(ix + 1, iy + 3, curitem, 7);
                if (curitem.Count != null)
                {
                    var c = $"{curitem.Count}";
                    pico8Functions.Print(c, ix + 88 - 16, iy + 3, 7);
                }
            }
            if (curmenu != null)
            {
                pico8Functions.Camera();
                if (curmenu.Type == chest)
                {
                    if (tooglemenu == 0)
                    {
                        List(menuinvent, 87, 24, 84, 96, 10);
                        List(curmenu, 4, 24, 84, 96, 10);
                    }
                    else
                    {
                        List(curmenu, -44, 24, 84, 96, 10);
                        List(menuinvent, 39, 24, 84, 96, 10);
                    }
                }
                else if (curmenu.Type.Becraft != null)
                {
                    if (curmenu.Sel >= 1 && curmenu.Sel <= curmenu.List.Count)
                    {
                        var curgoal = curmenu.List[(int)curmenu.Sel];
                        Panel("have", 71, 50, 52, 30);
                        pico8Functions.Print($"{Howmany(invent, curgoal)}", 91, 65, 7);
                        Requirelist(curgoal, 4, 79, 104, 50);
                    }
                    List(curmenu, 4, 16, 68, 64, 6);
                }
                else
                {
                    List(curmenu, 4, 24, 84, 96, 10);
                }
            }

            /*
            pico8Functions.Rectfill(31 + 50, 31 + 16, 65 + 50, 65 + 16, 8);
            pico8Functions.Rectfill(32 + 50, 32 + 16, 64 + 50, 64 + 16, 0);
            for (int i = 0; i <= 31; i++)
            {
                for (int j = 0; j <= 31; j++)
                {
                    var c = pico8Functions.Mget(i + 64, j);
                    pico8Functions.Pset(i + 32 + 50, j + 32 + 16, c);
                }
            }

            pico8Functions.Rectfill(31 - 20, 31, 97 - 20, 97, 8);
            pico8Functions.Rectfill(32 - 20, 32, 96 - 20, 96, 0);
            for (int i = 0; i <= 63; i++)
            {
                for (int j = 0; j <= 63; j++)
                {
                    var c = pico8Functions.Mget(i, j);
                    pico8Functions.Pset(i + 32 - 20, j + 32, c);
                }
            }
            */

            //pico8Functions.mset(1, 1, 8);
            //var ec = pico8Functions.mget(1, 1);
            //pico8Functions.pset(1, 1, ec);

            // Draw the grid
            /*for (int i = 0; i <= 128; i++)
            {
                // Draw vertical lines
                batch.DrawLine(pixel, new Vector2(i * cellWidth, 0), new Vector2(i * cellWidth, viewportHeight), Color.White, 1);
                // Draw horizontal lines
                batch.DrawLine(pixel, new Vector2(0, i * cellHeight), new Vector2(viewportWidth, i * cellHeight), Color.White, 1);
            }*/

            batch.End();

            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            // Create the batch...
            batch = new SpriteBatch(GraphicsDevice);

            // Create a 1x1 white pixel texture for drawing lines
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            // ... then load a texture from ./Graphics/FNATexture.png
            SpriteSheet1 = Content.Load<Texture2D>("SpriteSheet1");

            pico8Functions = new Pico8Functions(pixel, batch, GraphicsDevice);

        }

        protected override void UnloadContent()
        {
            batch.Dispose();
            SpriteSheet1.Dispose();
            pixel.Dispose();
            pico8Functions.Dispose();
        }

        private void UpdateViewport()
        {
            // Calculate the size of the largest square that fits in the client area
            int maxSize = Math.Min(Window.ClientBounds.Width, Window.ClientBounds.Height);

            // Round the size down to the nearest multiple of 128
            int size = (maxSize / 128) * 128;

            // Calculate the exact center of the client area
            double centerX = Window.ClientBounds.Width / 2.0f;
            double centerY = Window.ClientBounds.Height / 2.0f;

            // Calculate the top left corner of the square so that its center aligns with the client area's center
            int left = (int)Math.Round(centerX - size / 2.0f);
            int top = (int)Math.Round(centerY - size / 2.0f);

            // Set the viewport to the square area
            GraphicsDevice.Viewport = new Viewport(left, top, size, size);
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            UpdateViewport();
        }

    }
}