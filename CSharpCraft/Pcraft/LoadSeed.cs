using CSharpCraft.Pico8;
using CSharpCraft.RaceMode;
using FixMath;
using NativeFileDialogs.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CSharpCraft.Pcraft
{
    public class LoadSeed : PcraftBase
    {
        public override string SceneName => "load seed";

        private int runtimer = 0;
        private F32 frameTimer = F32.Zero;
        private string timer = "0:00.00";

        private int[]? loadedSeed = null;
        private int rngSeed = 0;

        private int ladderResets = 0;
        private int missedHits = 0;
        private int[] wastedHits = new int[pwrNames.Length + 6];
        private bool pickupAction = false;
        private bool placeAction = false;

        private int zReset = 0;

        private List<Random?> zombiePosRng = [];
        private List<Random?> zombieTimer = [];

        private Random? sandTimer = null;
        private Random? wheatTimer = null;

        private Random? spawnRng = null;
        private Random? waterRng = null;

        private Random? zombieDamage = null;
        private Dictionary<Ground, List<Random?>> damageDict = new()
        {
            { grwater, [] },
            { grsand, [] },
            { grgrass, [] },
            { grrock, [] },
            { grtree, [] },
            { grfarm, [] },
            { grwheat, [] },
            { grplant, [] },
            { griron, [] },
            { grgold, [] },
            { grgem, [] },
            { grhole, [] }
        };

        private Dictionary<Material, Random?> dropsDict = new()
        {
            { sand, null },
            { seed, null },
            { wheat, null },
            { stone, null },
            { wood, null },
            { apple, null },
            { iron, null },
            { gold, null },
            { gem, null },
            { ichor, null },
            { fabric, null }
        };

        private Dictionary<Material, Random?> spreadDict = new()
        {
            { haxe, null },
            { sword, null },
            { scythe, null },
            { shovel, null },
            { pick, null },

            { wood, null },
            { sand, null },
            { seed, null },
            { wheat, null },
            { apple, null },

            { glass, null },
            { stone, null },
            { iron, null },
            { gold, null },
            { gem, null },

            { fabric, null },
            { sail, null },
            { glue, null },
            { boat, null },
            { ichor, null },
            { potion, null },

            { ironbar, null },
            { goldbar, null },
            { bread, null },

            { workbench, null },
            { stonebench, null },
            { furnace, null },
            { anvil, null },
            { factory, null },
            { chem, null },
            { chest, null },

            { inventary, null },
            { pickuptool, null },

            { etext, null },
            { player, null },
            { zombi, null }
        };

        public int[] ImageToByteArray(string imagePath)
        {
            loadedSeed = new int[128 * 64];
            using (Image<Rgba32> image = Image.Load<Rgba32>(imagePath))
            {
                image.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < 64; y++)
                    {
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                        for (int x = 0; x < 128; x++)
                        {
                            ref Rgba32 pixel = ref pixelRow[x];
                            Microsoft.Xna.Framework.Color col = new(pixel.R, pixel.G, pixel.B);
                            for (int i = 0; i < 16; i++)
                            {
                                if (col == p8.colors[i])
                                {
                                    loadedSeed[x + y * 128] = i;
                                    break;
                                }
                            }
                        }
                    }
                });
            }
            return loadedSeed;
        }

        private void OpenFileDialog()
        {
            string path = Path.Combine($"{AppContext.BaseDirectory}seeds");
            var result = Nfd.OpenDialog(out path, null);

            if (result == NfdStatus.Ok)
            {
                loadedSeed = ImageToByteArray(path);

                string filename = path.Split("\\").Last();
                string extension = Path.GetExtension(filename);
                filename = filename.Replace(extension, "");
                char[] rngSeedChar = filename.ToCharArray();
                rngSeed = rngSeedChar[0] + 1;
                for (int i = 1; i < rngSeedChar.Length; i++)
                {
                    rngSeed += rngSeed + (rngSeedChar[i] + 1) * (i + 1);
                }
            }
        }

        protected virtual void Craft(Entity req)
        {
            foreach (Entity e in req.Req)
            {
                RemInList(invent, e);
            }
            AddItemInList(invent, SetPower(req.Power, Instc(req.Type, req.Count, req.List)), -1);
            if (req.Type == sword && req.Power == 2 && zReset == 0) { zReset = 1; }
        }

        protected override void FillEne(Level l)
        {
            l.Ene = [Entity(player, F32.Zero, F32.Zero, F32.Zero, F32.Zero)];
            enemies = l.Ene;
            zombiePosRng = [null];
            zombieTimer = [null];
            for (F32 i = F32.Zero; i < levelsx; i++)
            {
                for (F32 j = F32.Zero; j < levelsy; j++)
                {
                    Ground c = GetDirectGr(i, j);
                    F32 r = p8.Rnd(100, spawnRng);
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
                        zombiePosRng.Add(new Random(spawnRng.Next()));
                        zombieTimer.Add(new Random(spawnRng.Next()));
                    }
                }
            }
        }

        protected override void ResetLevel()
        {
            runtimer = 0;
            frameTimer = F32.Zero;
            timer = "0:00.00";

            ladderResets = 0;
            missedHits = 0;
            wastedHits = new int[pwrNames.Length + 6];
            pickupAction = false;
            placeAction = false;

            zReset = 0;zombiePosRng = [];
            zombieTimer = [];

            int incr = 0;
            spawnRng = new Random(rngSeed + incr);
            incr++;
            foreach (var key in dropsDict.Keys)
            {
                dropsDict[key] = new Random(rngSeed + incr);
                incr++;
            }
            foreach (var key in spreadDict.Keys)
            {
                spreadDict[key] = new Random(rngSeed + incr);
                incr++;
            }
            foreach (var key in damageDict.Keys)
            {
                damageDict[key] = [];
                for (int i = 0; i < pwrNames.Length; i++)
                {
                    damageDict[key].Add(new Random(rngSeed + incr));
                    incr++;
                }
            }
            zombieDamage = new Random(rngSeed + incr);
            incr++;
            sandTimer = new Random(rngSeed + incr);
            incr++;
            wheatTimer = new Random(rngSeed + incr);
            incr++;
            waterRng = new Random(rngSeed + incr);

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
                    Rndwat[i][j] = p8.Rnd(100, waterRng);
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

        protected override void AddItem(Material mat, int count, F32 hitx, F32 hity)
        {
            for (int i = 0; i < count; i++)
            {
                Entity gi = Entity(mat, F32.Floor(hitx / 16) * 16 + p8.Rnd(14, spreadDict[mat]) + 1, F32.Floor(hity / 16) * 16 + p8.Rnd(14, spreadDict[mat]) + 1, p8.Rnd(3, spreadDict[mat]) - F32.FromDouble(1.5), p8.Rnd(3, spreadDict[mat]) - F32.FromDouble(1.5));
                gi.GiveItem = mat;
                gi.HasCol = true;
                gi.Timer = 110 + p8.Rnd(20, spreadDict[mat]);
                p8.Add(entities, gi);
            }
        }

        public override void Init(Pico8Functions pico8)
        {
            base.Init(pico8);
        }

        protected override (F32 dx, F32 dy, bool canAct) UpEntity(F32 dx, F32 dy, bool canAct)
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
                            pickupAction = true;
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

        protected override void UpEnemies(F32 ebx, F32 eby)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                Entity e = enemies[i];
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
                        e.Dx = p8.Rnd(2, zombiePosRng[i]) - 1;
                        e.Dy = p8.Rnd(2, zombiePosRng[i]) - 1;
                        e.Dtim = 30 + p8.Rnd(60, zombieTimer[i]);
                    }
                    else if (e.Step == enstep_Walk)
                    {
                        e.Step = enstep_Wait;
                        e.Dx = F32.Zero;
                        e.Dy = F32.Zero;
                        e.Dtim = 30 + p8.Rnd(60, zombieTimer[i]);
                    }
                    else // chase
                    {
                        e.Dtim = 10 + p8.Rnd(60, zombieTimer[i]);
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
                            e.Dtim = 30 + p8.Rnd(60, zombieTimer[i]);
                        }
                    }
                    else
                    {
                        if (distp < 40)
                        {
                            e.Step = enstep_Chase;
                            e.Dtim = 10 + p8.Rnd(60, zombieTimer[i]);
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

        private void MissedHitsCheck(Ground hit, bool hasAxe)
        {
            if (curItem is not null)
            {
                if (!(curItem.Type == haxe && hit == grtree) &&
                    !(curItem.Type == pick && hit != grtree && (hit.IsTree || hit == grrock)) &&
                    !(curItem.Type == sword && nearEnemies.Count > 0) &&
                    !(curItem.Type == shovel && hit == grsand) &&
                    !(curItem.Type == scythe && (hit == grgrass || hit == grplant || hit == grwheat)) &&
                    !(hit == grtree && !hasAxe) &&
                    !(curItem.Type == seed && hit == grfarm) &&
                    !(curItem.Type == sand && hit == grwater) &&
                    !(curItem.Type.GiveLife is not null && nearEnemies.Count <= 0 && hit != grtree && hit != grrock && !hit.IsTree) &&
                    !(pickupAction) &&
                    !(placeAction))
                {
                    missedHits++;
                    Console.WriteLine(missedHits);
                }
            }
            else if ((hit == grtree && !hasAxe) ||
                pickupAction ||
                placeAction)
            {

            }
            else
            {
                missedHits++;
                Console.WriteLine(missedHits);
            }
        }

        private void WastedHitsCheck(Ground hit, bool hasAxe)
        {
            if (curItem is not null)
            {
                if ((curItem.Type == haxe && hit == grtree) ||
                    (curItem.Type == pick && hit != grtree && (hit.IsTree || hit == grrock)) ||
                    (curItem.Type == sword && nearEnemies.Count > 0) ||
                    (curItem.Type == shovel && hit == grsand) ||
                    (curItem.Type == scythe && (hit == grgrass || hit == grplant || hit == grwheat)))
                {
                    wastedHits[(int)curItem.Power]++;
                }
                else if (hit == grtree && !hasAxe)
                {
                    wastedHits[0]++;
                }
                else if (curItem.Type == seed && hit == grfarm)
                {
                    wastedHits[pwrNames.Length + 1]++;
                }
                else if (curItem.Type == sand && hit == grwater)
                {
                    wastedHits[pwrNames.Length + 2]++;
                }
                else if (curItem.Type.GiveLife is not null && nearEnemies.Count <= 0 && hit != grtree && hit != grrock && !hit.IsTree)
                {
                    wastedHits[pwrNames.Length + 3]++;
                }
                else if (pickupAction)
                {
                    wastedHits[pwrNames.Length + 4]++;
                }
                else if (placeAction)
                {
                    wastedHits[pwrNames.Length + 5]++;
                }
            }
        }

        protected override void UpHit(F32 hitx, F32 hity, Ground hit)
        {
            bool hasAxe = false;
            for (int i = 0; i < pwrNames.Length; i++)
            {
                if (HowMany(invent, SetPower(i, Inst(haxe))) > 0) { hasAxe = true; break; }
            }
            MissedHitsCheck(hit, hasAxe);
            WastedHitsCheck(hit, hasAxe);
            pickupAction = false;
            placeAction = false;
            if (nearEnemies.Count > 0)
            {
                p8.Sfx(19, 3);
                F32 pow = F32.One;
                if (curItem is not null && curItem.Type == sword)
                {
                    pow = 1 + (int)curItem.Power + p8.Rnd((int)curItem.Power * (int)curItem.Power, zombieDamage);
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
                        AddItem(ichor, F32.FloorToInt(p8.Rnd(3, dropsDict[ichor])), e.X, e.Y);
                        AddItem(fabric, F32.FloorToInt(p8.Rnd(3, dropsDict[fabric])), e.X, e.Y);
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
                            pow = 1 + (int)curItem.Power + p8.Rnd((int)curItem.Power * (int)curItem.Power, damageDict[hit][(int)curItem.Power - 1]);
                            stamCost = Math.Max(0, 20 - (int)curItem.Power * 2);
                            p8.Sfx(12, 3);
                        }
                    }
                    else if ((hit == grrock || hit.IsTree) && curItem.Type == pick)
                    {
                        pow = 1 + (int)curItem.Power * 2 + p8.Rnd((int)curItem.Power * (int)curItem.Power, damageDict[hit][(int)curItem.Power - 1]);
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
                    AddItem(hit.Mat, F32.FloorToInt(p8.Rnd(3, dropsDict[hit.Mat]) + 2), hitx, hity);
                    if (hit == grtree && p8.Rnd(1, dropsDict[apple]) > F32.FromDouble(0.7))
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
                        if (p8.Rnd(1, dropsDict[seed]) > F32.FromDouble(0.4)) { AddItem(seed, 1, hitx, hity); }
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
                            SetData(hitx, hity, time + 15 + p8.Rnd(5, sandTimer));
                            AddItem(sand, F32.FloorToInt(p8.Rnd(2, dropsDict[sand])), hitx, hity);
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
                        SetData(hitx, hity, time + 15 + p8.Rnd(5, wheatTimer));
                        RemInList(invent, Instc(seed, 1));
                        break;
                    case (Ground, Material) gm when gm == (grwheat, scythe):
                        SetGr(hitx, hity, grsand);
                        F32 d = F32.Max(F32.Zero, F32.Min(F32.FromInt(4), 4 - (GetData(hitx, hity, 0) - time)));
                        AddItem(wheat, F32.FloorToInt(d / 2 + p8.Rnd((d / 2).Double, dropsDict[wheat])), hitx, hity);
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
                    if (p8.Btnp(5) && !lb5)
                    {
                        if (curMenu == mainMenu)
                        {
                            OpenFileDialog();
                        }
                    }
                    else if (p8.Btnp(4) && !lb4 && loadedSeed is not null)
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
                ladderResets++;
                if (currentLevel == cave)
                {
                    if (zReset == 1)
                    {
                        spawnRng = new Random(rngSeed - 1);
                        zReset = 2;
                    }
                    SetLevel(island);
                }
                else
                {
                    if (zReset == 1 || zReset == 2)
                    {
                        spawnRng = new Random(rngSeed - 2);
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

        protected override void CreateMap()
        {
            plx = F32.FromInt((levelsx / 2 + 1) * 16 + 8);
            ply = F32.FromInt((levelsy / 2) * 16 + 8);

            List<(int x, int y)> spawnableTiles = [];

            for (int i = -4; i <= 4; i++)
            {
                if (i == 0) { continue; }
                for (int j = -4; j <= 4; j++)
                {
                    if (j == 0) { continue; }
                    int depx = levelsx / 2 + i;
                    int depy = levelsy / 2 + j;
                    F32 c = F32.FromInt(loadedSeed[depx + depy * 128]);

                    if (c == 1 || c == 2)
                    {
                        spawnableTiles.Add((depx, depy));
                    }
                }
            }

            if (spawnableTiles.Count > 0)
            {
                int indx = F32.FloorToInt(p8.Rnd(spawnableTiles.Count, spawnRng));
                (int x, int y) tile = spawnableTiles[indx];

                plx = F32.FromInt(tile.x * 16 + 8);
                ply = F32.FromInt(tile.y * 16 + 8);
            }

            for (int i = 0; i < levelsx; i++)
            {
                for (int j = 0; j < levelsy; j++)
                {
                    p8.Mset(i + levelx, j + levely, loadedSeed[i + levelx + (j + levely) * 128]);
                    if (loadedSeed[i + levelx + (j + levely) * 128] == 11)
                    {
                        holex = i + levelx;
                        holey = j + levely;
                    }
                }
            }

            clx = plx;
            cly = ply;

            cmx = plx;
            cmy = ply;
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
                if (curMenu == mainMenu)
                {
                    Printc("btn 0 to change seed", 64, 108, F32.FloorToInt(6 + time % 2));
                    Printc(loadedSeed is not null ? "btn 1 to play" : "", 64, 116, F32.FloorToInt(6 + time % 2));
                }
                else
                {
                    Printc("press button 1", 64, 112, F32.FloorToInt(6 + time % 2));
                }
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
