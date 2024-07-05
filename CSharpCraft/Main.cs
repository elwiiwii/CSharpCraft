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
using System.Runtime.CompilerServices;
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

    public class Entity
    {
        public double Banim { get; set; }
        public double C { get; set; }
        public int? Count { get; set; }
        public double Dtim { get; set; }
        public double Dx { get; set; }
        public double Dy { get; set; }
        public Material? Giveitem { get; set; }
        public bool Hascol { get; set; }
        public double Life { get; set; }
        public List<Entity>? List { get; set; }
        public double Lrot { get; set; }
        public double Off { get; set; }
        public double Ox { get; set; }
        public double Oy { get; set; }
        public double Panim { get; set; }
        public int? Power { get; set; }
        public double Prot { get; set; }
        public Entity[]? Req { get; set; }
        public double Sel { get; set; }
        public int? Spr { get; set; }
        public double Step { get; set; }
        public string? Text { get; set; }
        public string? Text2 { get; set; }
        public double? Timer { get; set; }
        public Material? Type { get; set; }
        public double Vx { get; set; }
        public double Vy { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }


    public class Ground
    {
        public double Gr { get; set; }
        public double Id { get; set; }
        public bool Istree { get; set; }
        public double Life { get; set; }
        public Material? Mat { get; set; }
        public int[]? Pal { get; set; }
        public Ground? Tile { get; set; }
    }


    public class Level
    {
        public double[]? Dat { get; set; }
        public List<Entity>? Ene { get; set; }
        public List<Entity>? Ent { get; set; }
        public bool Isunder { get; set; }
        public double Stx { get; set; }
        public double Sty { get; set; }
        public double Sx { get; set; }
        public double Sy { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }


    public class Material
    {
        public bool? Becraft { get; set; }
        public int? Bigspr { get; set; }
        public bool Drop { get; set; }
        public int? Givelife { get; set; }
        public string? Name { get; set; }
        public int[]? Pal { get; set; }
        public int Spr { get; set; }
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


        private List<Entity> anvilrecipe;

        private double banim;
        private SpriteBatch batch;

        private bool canswitchlevel = false;
        private Level cave;
        private List<Entity> chemrecipe;
        private double clx;
        private double cly;
        private double cmx;
        private double cmy;
        private double coffx;
        private double coffy;
        private Entity curitem;
        private Entity curmenu;
        private Level currentlevel;
        private readonly Level currentLevel;

        private double[] Dat;
        private double[] data = new double[8192];

        private TimeSpan elapsedTime = TimeSpan.Zero;
        private List<Entity> Ene;
        private List<Entity> enemies = [];
        private List<Entity> Ent;
        private List<Entity> entities = [];

        private List<Entity> factoryrecipe;
        private int frameCounter = 0;
        private double frameRate = 0.0;
        private List<Entity> furnacerecipe;

        private readonly GraphicsDeviceManager graphics;

        private double holex;
        private double holey;

        private List<Entity> invent = [];
        private Level island;

        private double[][] level;
        private double levelsx;
        private double levelsy;
        private double levelx;
        private double levely;
        private bool levelunder = false;
        private double llife;
        private double lrot;
        private double lstam;

        private Entity menuinvent;

        private List<Entity> nearenemies;

        private double panim;
        private Pico8Functions p8;
        private Texture2D pixel;
        private double plife;
        private double plx;
        private double ply;
        private double prot;
        private double pstam;

        private int[][] Rndwat = new int[16][];

        private Texture2D SpriteSheet1;
        private double stamcost;
        private List<Entity> stonebenchrecipe;
        private bool switchlevel = false;

        private double time;
        private int tooglemenu;
        readonly int[] typecount = new int[11];

        private List<Entity> workbenchrecipe;


#nullable disable


        private bool lb4 = false;
        private bool lb5 = false;
        private bool block5 = false;

        private int enstep_wait = 0;
        private int enstep_walk = 1;
        private int enstep_chase = 2;
        private int enstep_patrol = 3;

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

        static FNAGame()
        {
            apple.Givelife = 20;
        }

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

            Array.Copy(p8.colors, p8.resetColors, p8.colors.Length);
            Array.Copy(p8.colors, p8.sprColors, p8.colors.Length);

            p8.Palt();

            furnacerecipe = [];
            workbenchrecipe = [];
            stonebenchrecipe = [];
            anvilrecipe = [];
            factoryrecipe = [];
            chemrecipe = [];

            p8.Add(factoryrecipe, Recipe(Instc(sail, 1), [Instc(fabric, 3), Instc(glue, 1)]));
            p8.Add(factoryrecipe, Recipe(Instc(boat), [Instc(wood, 30), Instc(ironbar, 8), Instc(glue, 5), Instc(sail, 4)]));

            p8.Add(chemrecipe, Recipe(Instc(glue, 1), [Instc(glass, 1), Instc(ichor, 3)]));
            p8.Add(chemrecipe, Recipe(Instc(potion, 1), [Instc(glass, 1), Instc(ichor, 1)]));

            p8.Add(furnacerecipe, Recipe(Instc(ironbar, 1), [Instc(iron, 3)]));
            p8.Add(furnacerecipe, Recipe(Instc(goldbar, 1), [Instc(gold, 3)]));
            p8.Add(furnacerecipe, Recipe(Instc(glass, 1), [Instc(sand, 3)]));
            p8.Add(furnacerecipe, Recipe(Instc(bread, 1), [Instc(wheat, 5)]));

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
                    p8.Add(crafter[j], Recipe(Setpower(pows[j], Instc(tooltypes[i])), [Instc(materials[j], quant[i] * mult[j])]));
                }
            }

            p8.Add(workbenchrecipe, Recipe(Instc(workbench, null, workbenchrecipe), [Instc(wood, 15)]));
            p8.Add(workbenchrecipe, Recipe(Instc(stonebench, null, stonebenchrecipe), [Instc(stone, 15)]));
            p8.Add(workbenchrecipe, Recipe(Instc(factory, null, factoryrecipe), [Instc(wood, 15), Instc(stone, 15)]));
            p8.Add(workbenchrecipe, Recipe(Instc(chem, null, chemrecipe), [Instc(wood, 10), Instc(glass, 3), Instc(gem, 10)]));
            p8.Add(workbenchrecipe, Recipe(Instc(chest), [Instc(wood, 15), Instc(stone, 10)]));

            p8.Add(stonebenchrecipe, Recipe(Instc(anvil, null, anvilrecipe), [Instc(iron, 25), Instc(wood, 10), Instc(stone, 25)]));
            p8.Add(stonebenchrecipe, Recipe(Instc(furnace, null, furnacerecipe), [Instc(wood, 10), Instc(stone, 15)]));

            curmenu = mainmenu;
        }


        private void Additem(Material mat, double count, double hitx, double hity)
        {
            var countFlr = (int)Math.Floor(count);

            for (int i = 0; i < countFlr; i++)
            {
                var gi = Rentity(mat, Math.Floor(hitx / 16) * 16 + new Random().Next(14) + 1, Math.Floor(hity / 16) * 16 + new Random().Next(14) + 1);
                gi.Giveitem = mat;
                gi.Hascol = true;
                gi.Timer = 110 + new Random().Next(20);
                p8.Add(entities, gi);
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
            //l.Insert(p, e);

            if (p < l.Count - 1 && p >= 0)
            {
                for (int i = l.Count - 1; i >= p; i--)
                {
                    l[i] = l[i - 1];
                }
                l[p] = e;
            }
            else
            {
                p8.Add(l, e);
            }
        }


        private static Material Bigspr(int spr, Material ent)
        {
            ent.Bigspr = spr;
            ent.Drop = true;
            return ent;
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


        private void Cleardata(double x, double y)
        {
            var (i, j) = Getmcoord(x, y);
            if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
            {
                return;
            }
            data[(int)(i + j * levelsx)] = 0; // original code has null
        }


        private static Entity Cmenu(Material t, List<Entity> l = null, int? s = null, string te1 = null, string te2 = null)
        {
            return new() { List = l, Type = t, Sel = 1, Off = 0, Spr = s, Text = te1, Text2 = te2 };
        }


        private bool Comp(double i, double j, Ground gr)
        {
            var gr2 = Getdirectgr(i, j);
            return gr != null && gr2 != null && gr.Gr == gr2.Gr;
        }


        private void Craft(Entity req)
        {
            for (int i = 0; i < req.Req.Length; i++)
            {
                Reminlist(invent, req.Req[i]);
            }
            Additeminlist(invent, Setpower((int)req.Power, Instc(req.Type, req.Count, req.List)), 0);
        }


        private Level Createlevel(int xx, int yy, int sizex, int sizey, bool isUnderground)
        {
            var l = new Level { X = xx, Y = yy, Sx = sizex, Sy = sizey, Isunder = isUnderground, Ent = [], Ene = [], Dat = new double[8192] };
            Setlevel(l);
            levelunder = isUnderground;
            Createmap();
            Fillene(l);
            l.Stx = (holex - levelx) * 16 + 8;
            l.Sty = (holey - levely) * 16 + 8;
            return l;
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
                    p8.Mset(i + levelx, j + levely, level[i][j]);
                }
            }

            holex = levelsx / 2 + levelx;
            holey = levelsy / 2 + levely;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    p8.Mset(holex + i, holey + j, levelunder ? 1 : 3);
                }
            }

            p8.Mset(holex, holey, 11);

            clx = plx;
            cly = ply;

            cmx = plx;
            cmy = ply;
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


        private void Dbar(double px, double py, double v, double m, double c, double c2)
        {
            p8.Pal();
            var pe = px + v * 0.299988;
            var pe2 = px + m * 0.299988;
            p8.Rectfill(px - 1, py - 1, px + 30, py + 4, 0);
            p8.Rectfill(px, py, pe, py + 3, c2);
            p8.Rectfill(px, py, Math.Max(px, pe - 1), py + 2, c);
            if (m > v) { p8.Rectfill(pe + 1, py, pe2, py + 3, 10); }
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
                    if (Isin(e, 72))
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


        private void Dent()
        {
            for (int i = 0; i < entities.Count(); i++)
            {
                var e = entities[i];
                p8.Pal();
                if (e.Type.Pal != null) { Setpal(e.Type.Pal); }
                if (e.Type.Bigspr != null)
                {
                    p8.Spr((int)e.Type.Bigspr, e.X - 8, e.Y - 8, 2, 2);
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
                p8.Circ(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, 3, 6);
                p8.Circ(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, 3, 6);

                var anc = 3 + time * 3 % 1 * 3;
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

            var bcr = Math.Cos(blade);
            var bsr = Math.Sin(blade);

            (int mx, int my) = Mirror(blade);

            var weap = 75;

            if (isplayer && curitem != null)
            {
                p8.Pal();
                weap = (int)curitem.Type.Spr;
                if (curitem.Power != null)
                {
                    Setpal(pwrpal[(int)curitem.Power]);
                }
                if (curitem.Type != null && curitem.Type.Pal != null)
                {
                    Setpal(curitem.Type.Pal);
                }
            }

            p8.Spr(weap, x + bcr * 4 - cr * lan - mx * 8 + 1, y + bsr * 4 - sr * lan + my * 8 - 7 - 8, 1, 1, mx == 1, my == 1);

            if (isplayer) { p8.Pal(); }

            if (bel != grwater)
            {
                p8.Circfill(x + cv * 3 + cr * lan, y + sv * 3 + sr * lan, 3, 2);
                p8.Circfill(x - cv * 3 - cr * lan, y - sv * 3 - sr * lan, 3, 2);

                (int mx2, int my2) = Mirror((rot + 0.75) % 1);
                p8.Spr(75, x + cv * 4 + cr * lan - 8 + mx2 * 8 + 1, y + sv * 4 + sr * lan + my2 * 8 - 7 - 8, 1, 1, mx2 == 0, my2 == 1);
            }

            p8.Circfill(x + cr, y + sr - 2, 4, 2);
            p8.Circfill(x + cr, y + sr, 4, 2);
            p8.Circfill(x + cr * 1.5, y + sr * 1.5 - 2, 2.5, 15);
            p8.Circfill(x - cr, y - sr - 3, 3, 4);

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
                        p8.Mset(gi, gj, Rndsand(i, j) + sv);
                        p8.Mset(gi + 1, gj, Rndsand(i + 0.5, j) + sv);
                        p8.Mset(gi, gj + 1, Rndsand(i, j + 0.5) + sv);
                        p8.Mset(gi + 1, gj + 1, Rndsand(i + 0.5, j + 0.5) + sv);
                    }
                    else
                    {
                        var u = Comp(i, j - 1, gr);
                        var d = Comp(i, j + 1, gr);
                        var l = Comp(i - 1, j, gr);
                        var r = Comp(i + 1, j, gr);

                        var b = gr == grrock ? 21 : gr == grwater ? 26 : 16;

                        p8.Mset(gi, gj, b + (l ? (u ? (Comp(i - 1, j - 1, gr) ? 17 + Rndcenter(i, j) : 20) : 1) : (u ? 16 : 0)));
                        p8.Mset(gi + 1, gj, b + (r ? (u ? (Comp(i + 1, j - 1, gr) ? 17 + Rndcenter(i + 0.5, j) : 19) : 1) : (u ? 18 : 2)));
                        p8.Mset(gi, gj + 1, b + (l ? (d ? (Comp(i - 1, j + 1, gr) ? 17 + Rndcenter(i, j + 0.5) : 4) : 33) : (d ? 16 : 32)));
                        p8.Mset(gi + 1, gj + 1, b + (r ? (d ? (Comp(i + 1, j + 1, gr) ? 17 + Rndcenter(i + 0.5, j + 0.5) : 3) : 33) : (d ? 18 : 34)));

                    }
                }
            }

            p8.Pal();

            if (levelunder)
            {
                p8.Pal(15, 5);
                p8.Pal(4, 1);
            }

            p8.Map(64, 32, ci * 16, cj * 16, 18, 18);

            for (int i = ci - 1; i <= ci + 8; i++)
            {
                for (int j = cj - 1; j <= cj + 8; j++)
                {
                    var gr = Getdirectgr(i, j);

                    if (gr != null)
                    {
                        var gi = i * 16;
                        var gj = j * 16;

                        p8.Pal();

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
                                p8.Pal(pp, 3);
                                if (d > (10 - pp * 2)) { p8.Palt(pp, true); }
                            }
                            if (d < 0) { p8.Pal(4, 9); }
                            Spr4(i, j, gi, gj, 6, 6, 6, 6, 0, Rndsand);
                        }

                        if (gr.Istree == true)
                        {
                            Setpal(gr.Pal);

                            Spr4(i, j, gi, gj, 64, 65, 80, 81, 0, Rndtree);
                        }

                        if (gr == grhole)
                        {
                            p8.Pal();
                            if (!levelunder)
                            {
                                p8.Palt(0, false);
                                p8.Spr(31, gi, gj, 1, 2);
                                p8.Spr(31, gi + 8, gj, 1, 2, true); // changed +8 to +16 because of a temporary spr change
                            }
                            p8.Palt();
                            p8.Spr(77, gi + 4, gj, 1, 2);
                        }
                    }
                }
            }
        }


        private bool Entcolfree(double x, double y, Entity e)
        {
            return Math.Max(Math.Abs(e.X - x), Math.Abs(e.Y - y)) > 8;
        }


        private Entity Entity(Material it, double xx, double yy, double vxx, double vyy)
        {
            return new() { Type = it, X = xx, Y = yy, Vx = vxx, Vy = vyy };
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
                        p8.Add(l.Ene, newe);
                    }
                }
            }
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


        private Ground Getdirectgr(double i, double j)
        {
            if (i < 0 || j < 0 || i >= levelsx || j >= levelsy) { return grounds[0]; }
            return grounds[p8.Mget(i + levelx, j)];
        }


        private Ground Getgr(double x, double y)
        {
            var (i, j) = Getmcoord(x, y);
            return Getdirectgr(i, j);
        }


        private double Getinvlen(double x, double y)
        {
            return 1 / Getlen(x, y);
        }


        private double Getlen(double x, double y)
        {
            return Math.Sqrt(x * x + y * y + 0.001);
        }


        private (int, int) Getmcoord(double x, double y)
        {
            return ((int)Math.Floor(x / 16), (int)Math.Floor(y / 16));
        }


        private double Getrot(double dx, double dy)
        {
            return dy >= 0 ? (dx + 3) * 0.25 : (1 - dx) * 0.25;
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


        private Entity Inst(Material it)
        {
            return new() { Type = it };
        }


        private Entity Instc(Material it, int? c = null, List<Entity> l = null)
        {
            return new() { Type = it, Count = c, List = l };
        }


        private bool Isfree(double x, double y, Entity e = null)
        {
            var gr = Getgr(x, y);
            return !(gr.Istree || gr == grrock);
        }


        private bool Isfreeenem(double x, double y, Entity e = null)
        {
            var gr = Getgr(x, y);
            return !(gr.Istree || gr == grrock || gr == grwater);
        }


        private bool Isin(Entity e, int size)
        {
            return e.X > clx - size && e.X < clx + size && e.Y > cly - size && e.Y < cly + size;
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


        private static Material Item(string n, int s, int[] p = null, bool? bc = null)
        {
            return new() { Name = n, Spr = s, Pal = p, Becraft = bc };
        }


        private void Itemname(double x, double y, Entity it, int col)
        {
            var ty = it.Type;
            p8.Pal();
            var px = x;
            if (it.Power != null)
            {
                var pwn = pwrnames[(int)it.Power];
                p8.Print(pwn, x + 10, y, col);
                px += pwn.Length * 4 + 4;
                Setpal(pwrpal[(int)it.Power]);
            }
            if (ty.Pal != null) { Setpal(ty.Pal); }
            p8.Spr((double)ty.Spr, x, y - 2);
            p8.Pal();
            p8.Print(ty.Name, px + 10, y, col);
        }


        private double Lerp(double a, double b, double alpha)
        {
            return a * (1.0 - alpha) + b * alpha;
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
            p8.Rectfill(x + 1, sely, x + sx - 3, sely + 6, 13);

            x += 5;
            y += 12;

            for (int i = debut; i < fin; i++)
            {
                var it = menu.List[i];
                var py = y + (i - 1 - menu.Off) * 8;
                var col = 7;
                if ((it.Req != null) && !Cancraft(it))
                {
                    col = 0;
                }

                Itemname(x, py, it, col);

                if (it.Count != null)
                {
                    var c = $"{it.Count}";
                    p8.Print(c, x + sx - c.Length * 4 - 10, py, col);
                }
            }

            p8.Spr(68, x - 8, sely);
            p8.Spr(68, x + sx - 10, sely, 1, 1, true);
        }


        private double Loop(double sel, List<Entity> l)
        {
            var lp = l.Count;
            return (sel - 1) % lp % lp + 1;
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


        private void Panel(string name, double x, double y, double sx, double sy)
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


        public void Printb(string t, double x, double y, double c)
        {
            p8.Print(t, x + 1, y, 1);
            p8.Print(t, x - 1, y, 1);
            p8.Print(t, x, y + 1, 1);
            p8.Print(t, x, y - 1, 1);
            p8.Print(t, x, y, c);
        }


        private void Printc(string t, double x, double y, double c)
        {
            p8.Print(t, x - (t.Length * 2), y, c);
        }


        private Entity Recipe(Entity m, Entity[] require)
        {
            return new() { Type = m.Type, Power = m.Power, Count = m.Count, Req = require, List = m.List };
        }


        private (double, double) Reflectcol(double x, double y, double dx, double dy, Func<double, double, Entity, bool> checkfun, double dp, Entity e = null)
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
                    p8.Del(list, it);
                }
            }
            else
            {
                p8.Del(list, it);
            }
        }


        private Entity Rentity(Material it, double xx, double yy)
        {
            return Entity(it, xx, yy, new Random().Next(3) - 1.5, new Random().Next(3) - 1.5);
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
                    p8.Print(c, x + sx - c.Length * 4 - 10, py, h < it.Count ? 8 : 7);
                }
            }
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
            menuinvent = Cmenu(inventary, invent);

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

            p8.Add(invent, tmpworkbench);
            p8.Add(invent, Inst(pickuptool));
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


        private void Setdata(double x, double y, double v)
        {
            var (i, j) = Getmcoord(x, y);
            if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
            {
                return;
            }
            Dirsetdata(i, j, v);
        }


        private void Setgr(double x, double y, Ground v)
        {
            var (i, j) = Getmcoord(x, y);
            if (i < 0 || j < 0 || i >= levelsx || j >= levelsy) { return; }
            p8.Mset(i + levelx, j, v.Id);
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


        private void Setpal(int[] l)
        {
            for (int i = 0; i < l.Length; i++)
            {
                p8.Pal(i + 1, l[i]);
            }
        }


        private Entity Setpower(int v, Entity i)
        {
            i.Power = v;
            return i;
        }


        private Entity Settext(string t, double c, double time, Entity e)
        {
            e.Text = t;
            e.Timer = time;
            e.C = c;
            return e;
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


        private void Spr4(double i, double j, double gi, double gj, double a, double b, double c, double d, double off, Func<double, double, int> f)
        {
            p8.Spr(f(i, j + off) + a, gi, gj + 2 * off);
            p8.Spr(f(i + 0.5, j + off) + b, gi + 8, gj + 2 * off);
            p8.Spr(f(i, j + 0.5 + off) + c, gi, gj + 8 + 2 * off);
            p8.Spr(f(i + 0.5, j + 0.5 + off) + d, gi + 8, gj + 8 + 2 * off);
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
                            p8.Mset(i + levelx, j, grsand.Id);
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


        private void Watanim(double i, double j)
        {
            var a = ((time * 0.6 + (double)Watval(i, j) / 100) % 1) * 19;
            if (a > 16) { p8.Spr(13 + a - 16, i * 16, j * 16); }
        }


        private int Watval(double i, double j)
        {
            return Rndwat[(int)Math.Floor(Math.Abs(i) * 2 % 16)][(int)Math.Floor(Math.Abs(j) * 2 % 16)];
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

            if (state.IsKeyDown(Keys.Q)) switchlevel = true;

            if (curmenu != null)
            {
                if (curmenu.Spr != null)
                {
                    if (p8.Btnp(4) && !lb4)
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
                    lb4 = p8.Btn(4);
                    goto Continue;
                }
                else
                {
                    var intmenu = curmenu;
                    var othmenu = menuinvent;
                    if (curmenu.Type == chest)
                    {
                        if (p8.Btnp(0)) { tooglemenu -= 1; }
                        if (p8.Btnp(1)) { tooglemenu += 1; }
                        tooglemenu = (tooglemenu % 2 + 2) & 2;
                        if (tooglemenu == 1)
                        {
                            intmenu = menuinvent;
                            othmenu = curmenu;
                        }
                    }

                    if (intmenu.List.Count > 0)
                    {
                        if (p8.Btnp(2)) { intmenu.Sel -= 1; }
                        if (p8.Btnp(3)) { intmenu.Sel += 1; }

                        intmenu.Sel = Loop(intmenu.Sel, intmenu.List);

                        if (p8.Btnp(5))
                        {
                            if (curmenu.Type == chest)
                            {
                                var el = intmenu.List[(int)intmenu.Sel];
                                p8.Del(intmenu.List, el);
                                Additeminlist(othmenu.List, el, (int)othmenu.Sel);
                                if (intmenu.List.Count > 0 && intmenu.Sel > intmenu.List.Count) { intmenu.Sel -= 1; }
                                if (intmenu == menuinvent && curitem == el)
                                {
                                    curitem = null;
                                }
                            }
                            else if (curmenu.Type.Becraft == true)
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
                                curitem = curmenu.List[(int)curmenu.Sel - 1];
                                p8.Del(curmenu.List, curitem);
                                Additeminlist(curmenu.List, curitem, 1);
                                curmenu.Sel = 1;
                                curmenu = null;
                                block5 = true;
                            }
                        }
                    }
                }
                if (p8.Btnp(4) && !lb4)
                {
                    curmenu = null;
                }
                lb4 = p8.Btnp(4);
                lb5 = p8.Btnp(5);
                goto Continue;
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

            if (p8.Btn(0)) dx -= 1.0;
            if (p8.Btn(1)) dx += 1.0;
            if (p8.Btn(2)) dy -= 1.0;
            if (p8.Btn(3)) dy += 1.0;

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
                    p8.Del(entities, e);
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
                                p8.Del(entities, e);
                                p8.Add(entities, Settext(Howmany(invent, newit).ToString(), 11, 20, Entity(etext, e.X, e.Y - 5, 0, -1)));
                            }
                        }
                    }
                    else
                    {
                        if (e.Hascol)
                        {
                            (dx, dy) = Reflectcol(plx, ply, dx, dy, Entcolfree, 0, e);
                        }
                        if (dist < 12 && p8.Btn(5) && !block5 && !lb5)
                        {
                            if (curitem != null && curitem.Type == pickuptool)
                            {
                                if (e.Type == chest || e.Type.Becraft == true)
                                {
                                    Additeminlist(invent, e, 0);
                                    curitem = e;
                                    p8.Del(entities, e);
                                }
                                canact = false;
                            }
                            else
                            {
                                if (e.Type == chest || e.Type.Becraft == true)
                                {
                                    tooglemenu = 0;
                                    curmenu = Cmenu(e.Type, e.List);
                                }
                                canact = false;
                            }
                        }
                    }
                }
            }

            nearenemies = [];

            var ebx = Math.Cos(prot);
            var eby = Math.Sin(prot);

            for (int i = 0; i < enemies.Count; i++)
            {
                var e = enemies[i];
                if (Isin(e, 100))
                {
                    if (e.Type == player)
                    {
                        e.X = plx;
                        e.Y = ply;
                    }
                    else
                    {
                        var distp = Getlen(e.X - plx, e.Y - ply);
                        var mspeed = 0.8;
            
                        var disten = Getlen(e.X - plx - ebx * 8, e.Y - ply - eby * 8);
                        if (disten < 10)
                        {
                            p8.Add(nearenemies, e);
                        }
                        if (distp < 8)
                        {
                            e.Ox += Math.Max(-0.4, Math.Min(0.4, e.X - plx));
                            e.Oy += Math.Max(-0.4, Math.Min(0.4, e.Y - ply));
                        }
            
                        if (e.Dtim <= 0)
                        {
                            if (e.Step == enstep_wait || e.Step == enstep_patrol)
                            {
                                e.Step = enstep_walk;
                                e.Dx = new Random().Next(2) - 1;
                                e.Dy = new Random().Next(2) - 1;
                                e.Dtim = 30 + new Random().Next(60);
                            }
                            else if (e.Step == enstep_walk)
                            {
                                e.Step = enstep_wait;
                                e.Dx = 0;
                                e.Dy = 0;
                                e.Dtim = 30 + new Random().Next(60);
                            }
                            else // chase
                            {
                                e.Dtim = 10 + new Random().Next(60);
                            }
                        }
                        else
                        {
                            if (e.Step == enstep_chase)
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
                                    e.Banim = e.Banim % 8;
                                    var pow = 10;
                                    if (e.Banim == 4)
                                    {
                                        plife -= pow;
                                        p8.Add(entities, Settext(pow.ToString(), 8, 20, Entity(etext, plx, ply - 10, 0, -1)));
                                    }
                                    plife = Math.Max(0, plife);
                                }
                                mspeed = 1.4;
                                if (distp > 70)
                                {
                                    e.Step = enstep_chase;
                                    e.Dtim = 10 + new Random().Next(60);
                                }
                            }
                            e.Dtim -= 1;
                        }
            
                        var dl2 = mspeed * Getinvlen(e.Dx, e.Dy);
                        e.Dx *= dl2;
                        e.Dy *= dl2;
            
                        var fx = e.Dx + e.Ox;
                        var fy = e.Dy + e.Oy;
                        (fx, fy) = Reflectcol(e.X, e.Y, fx, fy, Isfreeenem, 0);
            
                        if (Math.Abs(e.Dx) > 0 || Math.Abs(e.Dy) > 0)
                        {
                            e.Lrot = Getrot(e.Dx, e.Dy);
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
            
                        e.Prot = Uprot(e.Lrot, e.Prot);
                    }
                }
            }

            (dx, dy) = Reflectcol(plx, ply, dx, dy, Isfree, 0);

            plx += dx;
            ply += dy;
            
            prot = Uprot(lrot, prot);

            llife += Math.Max(-1, Math.Min(1, plife - llife));
            lstam += Math.Max(-1, Math.Min(1, pstam - lstam));

            if (p8.Btn(5) && !block5 && canact)
            {
                var pxrot = -prot * 2 * Math.PI;
                var bx = Math.Cos(pxrot);
                var by = Math.Sin(pxrot);
                var hitx = plx + bx * 8;
                var hity = ply + by * 8;
                var hit = Getgr(hitx, hity);

                if (!lb5 && curitem != null && curitem.Type.Drop)
                {
                    if (hit == grsand || hit == grgrass)
                    {
                        if (curitem.List != null) { curitem.List = []; }
                        curitem.Hascol = true;
                
                        curitem.X = Math.Floor(hitx / 16) * 16 + 8;
                        curitem.Y = Math.Floor(hity / 16) * 16 + 8;
                        curitem.Vx = 0;
                        curitem.Vy = 0;
                        p8.Add(entities, curitem);
                        Reminlist(invent, curitem);
                        canact = false;
                    }
                }

                if (banim == 0 && pstam > 0 && canact)
                {
                    banim = 8;
                    stamcost = 20;
                    if (nearenemies.Count > 0)
                    {
                        var pow = 1.0;
                        if (curitem != null && curitem.Type == sword)
                        {
                            pow = 1 + (int)curitem.Power + new Random().Next((int)curitem.Power * (int)curitem.Power);
                            stamcost = Math.Max(0, 20 - (int)curitem.Power * 2);
                            pow = Math.Floor(pow);
                        }
                        for (int i = 0; i < nearenemies.Count; i++)
                        {
                            var e = nearenemies[i];
                            e.Life -= pow / nearenemies.Count;
                            var push = (pow - 1) * 0.5;
                            e.Ox += Math.Max(-push, Math.Min(push, e.X - plx));
                            e.Oy += Math.Max(-push, Math.Min(push, e.Y - ply));
                            if (e.Life <= 0)
                            {
                                p8.Del(enemies, e);
                                Additem(ichor, new Random().Next(3), e.X, e.Y);
                                Additem(fabric, new Random().Next(3), e.X, e.Y);
                            }
                            p8.Add(entities, Settext(pow.ToString(), 9, 20, Entity(etext, e.X, e.Y - 10, 0, -1)));
                        }
                    }
                    else if (hit.Mat != null)
                    {
                        var pow = 1.0;
                        if (curitem != null)
                        {
                            if (hit == grtree)
                            {
                                if (curitem.Type == haxe)
                                {
                                    pow = 1 + (int)curitem.Power + new Random().Next((int)curitem.Power * (int)curitem.Power);
                                    stamcost = Math.Max(0, 20 - (int)curitem.Power * 2);
                                }
                            }
                            else if ((hit == grrock || hit.Istree) && curitem.Type == pick)
                            {
                                pow = 1 + (int)curitem.Power * 2 + new Random().Next((int)curitem.Power * (int)curitem.Power);
                                stamcost = Math.Max(0, 20 - (int)curitem.Power * 2);
                            }
                        }
                        pow = Math.Floor(pow);
                    
                        var d = Getdata(hitx, hity, hit.Life);
                        if (d - pow <= 0)
                        {
                            Setgr(hitx, hity, hit.Tile);
                            Cleardata(hitx, hity);
                            Additem(hit.Mat, new Random().Next(3) + 2, hitx, hity);
                            if (hit == grtree && new Random().Next() > 0.7)
                            {
                                Additem(apple, 1, hitx, hity);
                            }
                        }
                        else
                        {
                            Setdata(hitx, hity, d - pow);
                        }
                        p8.Add(entities, Settext(pow.ToString(), 10, 20, Entity(etext, hitx, hity, 0, -1)));
                    }
                    else
                    {
                        if (curitem != null)
                        {
                            if (curitem.Power != null)
                            {
                                stamcost = Math.Max(0, 20 - (int)curitem.Power * 2);
                            }
                            if (curitem.Type.Givelife != null)
                            {
                                plife = Math.Min(100, plife + (int)curitem.Type.Givelife);
                                Reminlist(invent, Instc(curitem.Type, 1));
                            }
                            if (hit == grgrass && curitem.Type == scythe)
                            {
                                Setgr(hitx, hity, grsand);
                                if (new Random().Next() > 0.4) { Additem(seed, 1, hitx, hity); }
                            }
                            if (hit == grsand && curitem.Type == shovel)
                            {
                                if (curitem.Power > 3)
                                {
                                    Setgr(hitx, hity, grwater);
                                    Additem(sand, 2, hitx, hity);
                                }
                                else
                                {
                                    Setgr(hitx, hity, grfarm);
                                    Setdata(hitx, hity, time + 15 + new Random().Next(5));
                                    Additem(sand, new Random().Next(2), hitx, hity);
                                }
                            }
                            if (hit == grwater && curitem.Type == sand)
                            {
                                Setgr(hitx, hity, grsand);
                                Reminlist(invent, Instc(sand, 1));
                            }
                            if (hit == grwater && curitem.Type == boat)
                            {
                                curmenu = winmenu;
                            }
                            if (hit == grfarm && curitem.Type == seed)
                            {
                                Setgr(hitx, hity, grwheat);
                                Setdata(hitx, hity, time + 15 + new Random().Next(5));
                                Reminlist(invent, Instc(seed, 1));
                            }
                            if (hit == grwheat && curitem.Type == scythe)
                            {
                                Setgr(hitx, hity, grsand);
                                var d = Math.Max(0, Math.Min(4, 4 - (Getdata(hitx, hity, 0) - time)));
                                Additem(wheat, d / 2 + new Random().NextDouble() * (d / 2), hitx, hity);
                                Additem(seed, 1, hitx, hity);
                            }
                        }
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

            if (p8.Btnp(4) && !lb4)
            {
                curmenu = menuinvent;
            }

            lb4 = p8.Btn(4);
            lb5 = p8.Btnp(5);
            if (!p8.Btnp(5))
            {
                block5 = false;
            }

            time += 1.0 / 30.0;

            if (plife <= 0)
            {
                curmenu = deathmenu;
            }

            Continue:

            p8.prev0 = state.IsKeyDown(Keys.A);
            p8.prev1 = state.IsKeyDown(Keys.D);
            p8.prev2 = state.IsKeyDown(Keys.W);
            p8.prev3 = state.IsKeyDown(Keys.S);
            p8.prev4 = state.IsKeyDown(Keys.M);
            p8.prev5 = state.IsKeyDown(Keys.N);

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

            p8.Palt();

            if (curmenu != null && curmenu.Spr != null)
            {
                p8.Camera();
                p8.Palt(0, false);
                p8.Rectfill(0, 0, 128, 46, 12);
                p8.Rectfill(0, 46, 128, 128, 1);
                p8.Spr((double)curmenu.Spr, 32, 14, 8, 8);
                Printc(curmenu.Text, 64, 80, 6);
                Printc(curmenu.Text2, 64, 90, 6);
                Printc("press button 1", 64, 112, 6 + time % 2);
                time += 0.1;
                goto Continue;
            }

            p8.Camera(clx - 64, cly - 64);

            Drawback();

            Dent();

            //Dplayer(plx, ply, prot, panim, banim, true);

            Denemies();

            p8.Camera();

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
                    p8.Print(c, ix + 88 - 16, iy + 3, 7);
                }
            }
            if (curmenu != null)
            {
                p8.Camera();
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
                        p8.Print($"{Howmany(invent, curgoal)}", 91, 65, 7);
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
            p8.Rectfill(31 + 50, 31 + 16, 65 + 50, 65 + 16, 8);
            p8.Rectfill(32 + 50, 32 + 16, 64 + 50, 64 + 16, 0);
            for (int i = 0; i <= 31; i++)
            {
                for (int j = 0; j <= 31; j++)
                {
                    var c = p8.Mget(i + 64, j);
                    p8.Pset(i + 32 + 50, j + 32 + 16, c);
                }
            }

            p8.Rectfill(31 - 20, 31, 97 - 20, 97, 8);
            p8.Rectfill(32 - 20, 32, 96 - 20, 96, 0);
            for (int i = 0; i <= 63; i++)
            {
                for (int j = 0; j <= 63; j++)
                {
                    var c = p8.Mget(i, j);
                    p8.Pset(i + 32 - 20, j + 32, c);
                }
            }
            */

            //p8.mset(1, 1, 8);
            //var ec = p8.mget(1, 1);
            //p8.pset(1, 1, ec);

            // Draw the grid
            /*for (int i = 0; i <= 128; i++)
            {
                // Draw vertical lines
                batch.DrawLine(pixel, new Vector2(i * cellWidth, 0), new Vector2(i * cellWidth, viewportHeight), Color.White, 1);
                // Draw horizontal lines
                batch.DrawLine(pixel, new Vector2(0, i * cellHeight), new Vector2(viewportWidth, i * cellHeight), Color.White, 1);
            }*/

            Continue:

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

            p8 = new Pico8Functions(pixel, batch, GraphicsDevice);

        }


        protected override void UnloadContent()
        {
            batch.Dispose();
            SpriteSheet1.Dispose();
            pixel.Dispose();
            p8.Dispose();
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