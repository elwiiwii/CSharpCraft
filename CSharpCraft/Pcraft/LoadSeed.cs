using System.Reflection.Metadata.Ecma335;
using CSharpCraft.Pico8;
using CSharpCraft.RaceMode;
using FixMath;
using Microsoft.Xna.Framework.Input;
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
        private int yStart = 1;
        private int menuX = 8;
        private bool introActive = false;

        private string fileName = "no seed loaded";
        private int[]? loadedSeed = null;
        private int rngSeed = 0;
        private string seedInput = "";
        private KeyboardState prevState;

        private bool collectStats = true;
        private bool stdPlayerSpawn = true;
        private bool stdZombieSpawns = true;
        private bool stdZombieMovement = true;
        private bool stdDrops = true;
        private bool stdSpread = true;
        private bool stdDamage = true;

        private bool exitMenu = false;
        private int zombiesKilled = 0;
        private (int ichor, int fabric) zombiesDroppedCount = (0, 0);
        private int[] barFull = new int[7];
        private int[] menuTime = new int[8];
        private int[] ladderResets = new int[2];
        private int[] missedHits = new int[8];
        private int[] wastedHits = new int[9];
        private bool pickupAction = false;
        private bool placeAction = false;

        private new DataItem[] data = new DataItem[8192];

        private int zReset = 0;

        private Random? sandTimer = null;
        private Random? wheatTimer = null;
        private Random? WatRng = null;

        private Random? pSpawnRng = null;
        private Random? zSpawnRng = null;
        private Random? zMoveRng = null;

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

                fileName = path.Split("\\").Last();
                string tempName = fileName;
                string extension = Path.GetExtension(tempName);
                tempName = tempName.Replace(extension, "");
                char[] rngSeedChar = tempName.ToCharArray();
                rngSeed = rngSeedChar[0] + 1;
                for (int i = 1; i < rngSeedChar.Length; i++)
                {
                    rngSeed += rngSeed + (rngSeedChar[i] + 1) * (i + 1);
                }
                seedInput = rngSeed.ToString();
            }
        }

        protected override void Craft(Entity req)
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
            for (F32 i = F32.Zero; i < levelsx; i++)
            {
                for (F32 j = F32.Zero; j < levelsy; j++)
                {
                    Ground c = GetDirectGr(i, j);
                    F32 r = p8.Rnd(100, zSpawnRng);
                    F32 ex = i * 16 + 8;
                    F32 ey = j * 16 + 8;
                    F32 dist = F32.Max(F32.Abs(ex - plx), F32.Abs(ey - ply));
                    int? newRngSeed = zMoveRng is not null ? zMoveRng.Next() : null;
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
                        newe.PosRnd = newRngSeed is not null ? new Random((int)newRngSeed) : null;
                        newe.TimRnd = newRngSeed is not null ? new Random((int)newRngSeed + 1) : null;
                        p8.Add(l.Ene, newe);
                    }
                }
            }
        }

        protected override Level CreateLevel(int xx, int yy, int sizex, int sizey, bool IsUnderground)
        {
            Level l = new Level { X = xx, Y = yy, Sx = sizex, Sy = sizey, IsUnder = IsUnderground, Ent = [], Ene = [], DatIt = Enumerable.Range(0, 8192).Select(_ => new DataItem()).ToArray() };
            SetLevel(l);
            levelUnder = IsUnderground;
            CreateMap();
            FillEne(l);
            l.Stx = F32.FromInt((holex - levelx) * 16 + 8);
            l.Sty = F32.FromInt((holey - levely) * 16 + 8);
            return l;
        }

        protected override void SetLevel(Level l)
        {
            currentLevel = l;
            levelx = l.X;
            levely = l.Y;
            levelsx = l.Sx;
            levelsy = l.Sy;
            levelUnder = l.IsUnder;
            entities = l.Ent;
            enemies = l.Ene;
            data = l.DatIt;
            plx = l.Stx;
            ply = l.Sty;
        }

        protected override void ResetLevel()
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
            TextInputEXT.StartTextInput();
            TextInputEXT.TextInput += OnTextInput;
            menuX = 8;
            base.Init(pico8);
        }

        protected new DataItem DirGetData(F32 i, F32 j, F32 @default)
        {
            int g = F32.FloorToInt(i + j * levelsx);
            if (data[g - 1].Val == 0)
            {
                data[g - 1].Val = @default;
            }
            return data[g - 1];
        }

        protected new DataItem GetData(F32 x, F32 y, int @default)
        {
            (int i, int j) = GetMcoord(x, y);
            if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
            {
                return new DataItem { Val = F32.FromInt(@default) };
            }
            return DirGetData(F32.FromInt(i), F32.FromInt(j), F32.FromInt(@default));
        }

        protected void SetDataItem(F32 x, F32 y, DataItem v)
        {
            (int i, int j) = GetMcoord(x, y);
            if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
            {
                return;
            }
            data[i + j * levelsx - 1] = v;
        }

        protected override void Cleardata(F32 x, F32 y)
        {
            (int i, int j) = GetMcoord(x, y);
            if (i < 0 || j < 0 || i > levelsx - 1 || j > levelsy - 1)
            {
                return;
            }
            data[i + j * levelsx - 1] = new DataItem();
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

        protected override void UpGround()
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
                        DataItem d = DirGetData(i, j, F32.Zero);
                        if (time > d.Val)
                        {
                            p8.Mset(i.Double + levelx, j.Double, grsand.Id);
                        }
                    }
                }
            }
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
                        e.Dx = p8.Rnd(2, e.PosRnd) - 1;
                        e.Dy = p8.Rnd(2, e.PosRnd) - 1;
                        e.Dtim = 30 + p8.Rnd(60, e.TimRnd);
                    }
                    else if (e.Step == enstep_Walk)
                    {
                        e.Step = enstep_Wait;
                        e.Dx = F32.Zero;
                        e.Dy = F32.Zero;
                        e.Dtim = 30 + p8.Rnd(60, e.TimRnd);
                    }
                    else // chase
                    {
                        e.Dtim = 10 + p8.Rnd(60, e.TimRnd);
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
                            e.Dtim = 30 + p8.Rnd(60, e.TimRnd);
                        }
                    }
                    else
                    {
                        if (distp < 40)
                        {
                            e.Step = enstep_Chase;
                            e.Dtim = 10 + p8.Rnd(60, e.TimRnd);
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
                if (!(curItem.Type == haxe && hit == grtree && nearEnemies.Count <= 0) &&
                    !(curItem.Type == pick && hit != grtree && (hit.IsTree || hit == grrock) && nearEnemies.Count <= 0) &&
                    !(curItem.Type == sword && nearEnemies.Count > 0) &&
                    !(curItem.Type == shovel && hit == grsand && nearEnemies.Count <= 0) &&
                    !(curItem.Type == scythe && (hit == grgrass || hit == grplant || hit == grwheat) && nearEnemies.Count <= 0) &&
                    !(hit == grtree && !hasAxe && nearEnemies.Count <= 0) &&
                    !(curItem.Type == seed && hit == grfarm && nearEnemies.Count <= 0) &&
                    !(curItem.Type == sand && hit == grwater && nearEnemies.Count <= 0) &&
                    !(curItem.Type.GiveLife is not null && nearEnemies.Count <= 0 && hit != grtree && hit != grrock && !hit.IsTree) &&
                    !(pickupAction) &&
                    !(placeAction))
                {
                    if (exitMenu)
                    {
                        missedHits[7]++;
                    }
                    else if (curItem is not null)
                    {
                        if (curItem.Power is null)
                        {
                            missedHits[0]++;
                        }
                        else if (curItem.Power is not null && curItem.Power == 1)
                        {
                            missedHits[1]++;
                        }
                        else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == haxe)
                        {
                            missedHits[2]++;
                        }
                        else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == pick)
                        {
                            missedHits[3]++;
                        }
                        else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == sword)
                        {
                            missedHits[4]++;
                        }
                        else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == shovel)
                        {
                            missedHits[5]++;
                        }
                        else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == scythe)
                        {
                            missedHits[6]++;
                        }
                    }
                    else
                    {
                        missedHits[0]++;
                    }
                }
            }
            else if ((hit == grtree && !hasAxe && nearEnemies.Count <= 0) ||
                pickupAction ||
                placeAction)
            {

            }
            else
            {
                if (exitMenu)
                {
                    missedHits[7]++;
                }
                else if (curItem is not null)
                {
                    if (curItem.Power is null)
                    {
                        missedHits[0]++;
                    }
                    else if (curItem.Power is not null && curItem.Power == 1)
                    {
                        missedHits[1]++;
                    }
                    else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == haxe)
                    {
                        missedHits[2]++;
                    }
                    else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == pick)
                    {
                        missedHits[3]++;
                    }
                    else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == sword)
                    {
                        missedHits[4]++;
                    }
                    else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == shovel)
                    {
                        missedHits[5]++;
                    }
                    else if (curItem.Power is not null && curItem.Power > 1 && curItem.Type == scythe)
                    {
                        missedHits[6]++;
                    }
                }
                else
                {
                    missedHits[0]++;
                }
            }
        }

        private void WastedHitsCheck(F32 hitx, F32 hity, Ground hit, bool hasAxe)
        {
            if (curItem is not null)
            {
                if (curItem.Type == haxe && hit == grtree && nearEnemies.Count <= 0)
                {
                    if (curItem.Power is not null && curItem.Power == 1) { wastedHits[1]++; }
                    else if (curItem.Power is not null && curItem.Power > 1) { wastedHits[2]++; }
                }
                else if (curItem.Type == pick && hit != grtree && (hit.IsTree || hit == grrock) && nearEnemies.Count <= 0)
                {
                    if (curItem.Power is not null && curItem.Power == 1) { wastedHits[1]++; }
                    else if (curItem.Power is not null && curItem.Power > 1) { wastedHits[3]++; }
                }
                else if (hit == grtree && !hasAxe && nearEnemies.Count <= 0)
                {
                    wastedHits[0]++;
                }
                else if (curItem.Type == seed && hit == grfarm && nearEnemies.Count <= 0)
                {
                    wastedHits[4]++;
                }
                else if (curItem.Type == sand && hit == grwater && nearEnemies.Count <= 0)
                {
                    wastedHits[5]++;
                }
                else if (curItem.Type.GiveLife is not null && nearEnemies.Count <= 0 && hit != grtree && hit != grrock && !hit.IsTree)
                {
                    wastedHits[6]++;
                }
                else if (pickupAction)
                {
                    wastedHits[7]++;
                }
                else if (placeAction)
                {
                    wastedHits[8]++;
                }
            }
            else if (hit == grtree && !hasAxe && nearEnemies.Count <= 0)
            {
                wastedHits[0]++;
            }
            else if (placeAction)
            {
                wastedHits[8]++;
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
            WastedHitsCheck(hitx, hity, hit, hasAxe);
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
                        zombiesKilled++;
                        p8.Del(enemies, e);
                        int dropCount = F32.FloorToInt(p8.Rnd(3, dropsDict[ichor]));
                        zombiesDroppedCount.ichor += dropCount;
                        AddItem(ichor, dropCount, e.X, e.Y);
                        dropCount = F32.FloorToInt(p8.Rnd(3, dropsDict[fabric]));
                        zombiesDroppedCount.fabric += dropCount;
                        AddItem(fabric, dropCount, e.X, e.Y);
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

                DataItem d = GetData(hitx, hity, hit.Life);
                if (curItem is not null && curItem.Type == haxe && hit == grtree && nearEnemies.Count <= 0)
                {
                    if (curItem.Power is not null && curItem.Power == 1) { d.Hits[1]++; }
                    else if (curItem.Power is not null && curItem.Power > 1) { d.Hits[2]++; }
                }
                else if (curItem is not null && curItem.Type == pick && hit != grtree && (hit.IsTree || hit == grrock) && nearEnemies.Count <= 0)
                {
                    if (curItem.Power is not null && curItem.Power == 1) { d.Hits[1]++; }
                    else if (curItem.Power is not null && curItem.Power > 1) { d.Hits[3]++; }
                }
                else if (hit == grtree && !hasAxe && nearEnemies.Count <= 0)
                {
                    d.Hits[0]++;
                }

                if (d.Val - pow <= 0)
                {
                    if (curItem is not null && curItem.Type == haxe && hit == grtree && nearEnemies.Count <= 0)
                    {
                        if (curItem.Power is not null && curItem.Power == 1) { wastedHits[1] -= d.Hits[1]; }
                        else if (curItem.Power is not null && curItem.Power > 1) { wastedHits[2] -= d.Hits[2]; }
                    }
                    else if (curItem is not null && curItem.Type == pick && hit != grtree && (hit.IsTree || hit == grrock) && nearEnemies.Count <= 0)
                    {
                        if (curItem.Power is not null && curItem.Power == 1) { wastedHits[1] -= d.Hits[1]; }
                        else if (curItem.Power is not null && curItem.Power > 1) { wastedHits[3] -= d.Hits[3]; }
                    }
                    else if (hit == grtree && !hasAxe && nearEnemies.Count <= 0)
                    {
                        wastedHits[0] -= d.Hits[0];
                    }
                    hit.MinedCount++;
                    SetGr(hitx, hity, hit.Tile);
                    Cleardata(hitx, hity);
                    int dropCount = F32.FloorToInt(p8.Rnd(3, dropsDict[hit.Mat]) + 2);
                    hit.DroppedCount = (hit.DroppedCount.a + dropCount, hit.DroppedCount.b);
                    AddItem(hit.Mat, dropCount, hitx, hity);
                    if (hit == grtree && p8.Rnd(1, dropsDict[apple]) > F32.FromDouble(0.7))
                    {
                        hit.DroppedCount = (hit.DroppedCount.a, hit.DroppedCount.b + 1);
                        AddItem(apple, 1, hitx, hity);
                    }
                }
                else
                {
                    d.Val -= pow;
                    SetDataItem(hitx, hity, d);
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
                        hit.MinedCount++;
                        SetGr(hitx, hity, grsand);
                        if (p8.Rnd(1, dropsDict[seed]) > F32.FromDouble(0.4)) { hit.DroppedCount = (hit.DroppedCount.a + 1, hit.DroppedCount.b); AddItem(seed, 1, hitx, hity); }
                        break;
                    case (Ground, Material) gm when gm == (grsand, shovel):
                        hit.MinedCount++;
                        if (curItem.Power > 3)
                        {
                            hit.DroppedCount = (hit.DroppedCount.a + 2, hit.DroppedCount.b);
                            SetGr(hitx, hity, grwater);
                            AddItem(sand, 2, hitx, hity);
                        }
                        else
                        {
                            SetGr(hitx, hity, grfarm);
                            SetDataItem(hitx, hity, new DataItem { Val = time + 15 + p8.Rnd(5, sandTimer) });
                            int sDropCount = F32.FloorToInt(p8.Rnd(2, dropsDict[sand]));
                            hit.DroppedCount = (hit.DroppedCount.a + sDropCount, hit.DroppedCount.b);
                            AddItem(sand, sDropCount, hitx, hity);
                        }
                        break;
                    case (Ground, Material) gm when gm == (grwater, sand):
                        SetGr(hitx, hity, grsand);
                        RemInList(invent, Instc(sand, 1));
                        break;
                    case (Ground, Material) gm when gm == (grwater, boat):
                        p8.Reload();
                        p8.Memcpy(0x1000, 0x2000, 0x1000);
                        winMenu = Cmenu(inventary, null, 136, "you escaped!", timer);
                        curMenu = winMenu;
                        runtimer = 0;
                        p8.Music(3);
                        break;
                    case (Ground, Material) gm when gm == (grfarm, seed):
                        SetGr(hitx, hity, grwheat);
                        SetDataItem(hitx, hity, new DataItem { Val = time + 15 + p8.Rnd(5, wheatTimer) });
                        RemInList(invent, Instc(seed, 1));
                        break;
                    case (Ground, Material) gm when gm == (grwheat, scythe):
                        hit.MinedCount++;
                        SetGr(hitx, hity, grsand);
                        F32 d = F32.Max(F32.Zero, F32.Min(F32.FromInt(4), 4 - (GetData(hitx, hity, 0).Val - time)));
                        int dropCount = F32.FloorToInt(d / 2 + p8.Rnd((d / 2).Double, dropsDict[wheat]));
                        hit.DroppedCount = (hit.DroppedCount.a + dropCount, hit.DroppedCount.b);
                        AddItem(wheat, F32.FloorToInt(d / 2 + p8.Rnd((d / 2).Double, dropsDict[wheat])), hitx, hity);
                        hit.DroppedCount = (hit.DroppedCount.a, hit.DroppedCount.b + 1);
                        AddItem(seed, 1, hitx, hity);
                        break;
                    default:
                        break;
                }
            }
        }

        private void OnTextInput(char c)
        {
            if (curMenu == introMenu && menuX == 0 && char.IsDigit(c) && seedInput.Length < 10)
            {
                string potential = string.IsNullOrEmpty(seedInput) ? c.ToString() : seedInput + c;
                if (long.TryParse(potential, out long test) && test < int.MaxValue)
                {
                    seedInput = potential;
                    UpdateRngSeed();
                }

            }
        }

        private void UpdateRngSeed()
        {
            rngSeed = string.IsNullOrEmpty(seedInput) ? 0 : Convert.ToInt32(seedInput);
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
                    else if (((p8.Btnp(4) && !lb4) || introActive) && loadedSeed is not null)
                    {
                        if (curMenu == mainMenu)
                        {
                            curMenu = introMenu;
                            introActive = true;
                        }
                        else if (curMenu == introMenu)
                        {
                            if (menuX == 0)
                            {
                                KeyboardState state = Keyboard.GetState();
                                if (state.GetPressedKeys().Length > 0)
                                {
                                    if (state.IsKeyDown(Keys.Back) && prevState.IsKeyUp(Keys.Back) && seedInput.Length > 0)
                                    {
                                        seedInput = seedInput.Remove(seedInput.Length - 1);
                                        UpdateRngSeed();
                                    }
                                }
                                prevState = state;
                            }
                            else if (p8.Btnp(4))
                            {
                                if (menuX == 8)
                                {
                                    introActive = false;
                                    ResetLevel();
                                    curMenu = null;
                                    p8.Music(1);
                                }
                                else
                                {
                                    switch (menuX)
                                    {
                                        case 1:
                                            collectStats = !collectStats;
                                            break;
                                        case 2:
                                            stdPlayerSpawn = !stdPlayerSpawn;
                                            break;
                                        case 3:
                                            stdZombieSpawns = !stdZombieSpawns;
                                            break;
                                        case 4:
                                            stdZombieMovement = !stdZombieMovement;
                                            break;
                                        case 5:
                                            stdDrops = !stdDrops;
                                            break;
                                        case 6:
                                            stdSpread = !stdSpread;
                                            break;
                                        case 7:
                                            stdDamage = !stdDamage;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }

                            if (p8.Btnp(2)) { menuX = Math.Max(menuX - 1, 0); }
                            else if (p8.Btnp(3)) { menuX = Math.Min(menuX + 1, 8); }
                        }
                        else
                        {
                            ResetLevel();
                            curMenu = null;
                            p8.Music(1);
                        }
                    }
                    else if (p8.Btnp(2)) { yStart = 1; }
                    else if (p8.Btnp(3)) { yStart = -40; }
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
                int indx = F32.FloorToInt(p8.Rnd(spawnableTiles.Count, pSpawnRng));
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

        protected override void DrawBack()
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
                        F32 d = DirGetData(i, j, F32.Zero).Val - time;
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

        private int DrawStats(int xpos, int ypos, string title, int[] data, int[] sprites, int[][] pals, (int a, int b)[]? data2 = null)
        {
            int xstart = xpos;
            p8.Print($"{title}", xpos, ypos, 7);
            xpos += title.Length * 4 + 3;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > 0)
                {
                    if (xpos > 127 - data[i].ToString().Length * 4 - (data2 is not null ? $"/{data2[i].a}{(data2[i].b > 0 ? $",{data2[i].b}" : "")}".Length * 4 : 0) - 10) { xpos = xstart; ypos += 9; }
                    p8.Pal();
                    SetPal(pals[i]);
                    p8.Spr(sprites[i], xpos, ypos - 2);
                    xpos += 9;
                    p8.Print($"{data[i]}{(data2 is not null ? $"/{data2[i].a}{(data2[i].b > 0 ? $",{data2[i].b}" : "")}" : "")}", xpos, ypos, 7);
                    xpos += data[i].ToString().Length * 4 + (data2 is not null ? $"/{data2[i].a}{(data2[i].b > 0 ? $",{data2[i].b}" : "")}".Length * 4 : 0) + 3;
                }
            }
            return ypos;
        }

        public override void Draw()
        {
            if (curMenu is not null && curMenu.Spr is not null)
            {
                p8.Camera();
                p8.Palt(0, false);
                p8.Rectfill(0, 0, 128, 46, 12);
                p8.Rectfill(0, 46, 128, 128, 1);
                if (!collectStats || (curMenu == mainMenu || curMenu == introMenu))
                {
                    p8.Spr((int)curMenu.Spr, 32, 14, 8, 8);
                }
                if (curMenu == introMenu)
                {
                    int y = 3;
                    string s1 = "rng seed ";
                    string s2 = $"{rngSeed}";
                    p8.Rectfill(64 - (s1 + s2).Length * 2 + s1.Length * 4 - 2, y, 64 + (s1 + s2).Length * 2, y + 8, menuX == 0 ? 7 : 6);
                    p8.Rectfill(64 - (s1 + s2).Length * 2 + s1.Length * 4 - 1, y + 1, 64 + (s1 + s2).Length * 2 - 1, y + 7, 12);
                    Printc(s1 + s2, 64, y + 2, menuX == 0 ? 7 : 6);

                    Printc($"collect stats: {collectStats.ToString().ToLower()}", 64, 67, menuX == 1 ? 7 : 6);
                    Printc($"player spawn: {(stdPlayerSpawn ? "std" : "rnd")}", 64, 74, menuX == 2 ? 7 : 6);
                    Printc($"zombie spawns: {(stdZombieSpawns ? "std" : "rnd")}", 64, 81, menuX == 3 ? 7 : 6);
                    Printc($"zombie movement: {(stdZombieMovement ? "std" : "rnd")}", 64, 88, menuX == 4 ? 7 : 6);
                    Printc($"drops: {(stdDrops ? "std" : "rnd")}", 64, 95, menuX == 5 ? 7 : 6);
                    Printc($"item spreads: {(stdSpread ? "std" : "rnd")}", 64, 102, menuX == 6 ? 7 : 6);
                    Printc($"damage: {(stdDamage ? "std" : "rnd")}", 64, 109, menuX == 7 ? 7 : 6);

                    Printc($"load seed", 64, 121, menuX == 8 ? 7 : 6);
                }
                else if (!collectStats || curMenu == mainMenu)
                {
                    Printc(curMenu.Text, 64, 80, 6);
                    Printc(curMenu.Text2, 64, 90, 6);
                }
                if (curMenu == introMenu)
                {
                    int y = 3;
                    string s1 = "rng seed ";
                    string s2 = $"{rngSeed}";
                    p8.Rectfill(64 - (s1 + s2).Length * 2 + s1.Length * 4 - 2, y, 64 + (s1 + s2).Length * 2, y + 8, menuX == 0 ? 7 : 6);
                    p8.Rectfill(64 - (s1 + s2).Length * 2 + s1.Length * 4 - 1, y + 1, 64 + (s1 + s2).Length * 2 - 1, y + 7, 12);
                    Printc(s1 + s2, 64, y + 2, menuX == 0 ? 7 : 6);

                    Printc($"collect stats: {collectStats.ToString().ToLower()}", 64, 67, menuX == 1 ? 7 : 6);
                    Printc($"player spawn: {(stdPlayerSpawn ? "std" : "rnd")}", 64, 74, menuX == 2 ? 7 : 6);
                    Printc($"zombie spawns: {(stdZombieSpawns ? "std" : "rnd")}", 64, 81, menuX == 3 ? 7 : 6);
                    Printc($"zombie movement: {(stdZombieMovement ? "std" : "rnd")}", 64, 88, menuX == 4 ? 7 : 6);
                    Printc($"drops: {(stdDrops ? "std" : "rnd")}", 64, 95, menuX == 5 ? 7 : 6);
                    Printc($"item spreads: {(stdSpread ? "std" : "rnd")}", 64, 102, menuX == 6 ? 7 : 6);
                    Printc($"damage: {(stdDamage ? "std" : "rnd")}", 64, 109, menuX == 7 ? 7 : 6);

                    Printc($"load seed", 64, 121, menuX == 8 ? 7 : 6);
                }
                if (collectStats && (curMenu == winMenu || curMenu == deathMenu))
                {
                    int ypos = yStart;
                    //file name
                    string s = $"file name: {fileName}";
                    IEnumerable<string> chunks = s.Chunk(127/4).Select(c => new string(c));
                    foreach (string chunk in chunks)
                    {
                        p8.Print(chunk, 1, ypos, 7);
                        ypos += 6;
                    }
                    ypos ++;
                    //rng seed
                    p8.Print($"rng seed: {rngSeed}", 1, ypos, 7);
                    ypos += 7;
                    //standardisation toggles
                    p8.Print($"Pspawn={(stdPlayerSpawn ? "s" : "r")} Zspawn={(stdZombieSpawns ? "s" : "r")} Zmove={(stdZombieMovement ? "s" : "r")}", 1, ypos, 7);
                    ypos += 7;
                    p8.Print($"drops={(stdDrops ? "s" : "r")} spread={(stdSpread ? "s" : "r")} dmg={(stdDamage ? "s" : "r")}", 1, ypos, 7);
                    ypos += 7;
                    //time
                    p8.Print($"{curMenu.Text} {curMenu.Text2}", 1, ypos, 7);
                    ypos += 8;
                    //missed hits
                    ypos = DrawStats(1, ypos, "missed hits:", missedHits, [75, 102, 98, 102, 99, 101, 100, 89], [[], pwrPal[0], pwrPal[1], pwrPal[1], pwrPal[1], pwrPal[1], pwrPal[1], workbench.Pal]);
                    ypos += 10;
                    //wasted hits
                    ypos = DrawStats(1, ypos, "wasted hits:", wastedHits, [75, 102, 98, 102, 115, 114, 116, 73, 89], [[], pwrPal[0], pwrPal[1], pwrPal[1], [], [15], [], [], workbench.Pal]);
                    ypos += 10;
                    //mined counts
                    int[] mCounts = new int[grounds.Length + 1];
                    for (int i = 0; i < grounds.Length + 1; i++)
                    {
                        if (i == 0) { mCounts[i] = zombiesKilled; }
                        else { mCounts[i] = grounds[i - 1].MinedCount; }
                    }
                    ypos = DrawStats(1, ypos, "mined/drops:", mCounts, [75, 43, 2, 52, 56, 96, 5, 8, 9, 96, 96, 96, 47], [[1,8,3,1,5,6,7,8,9,10,11,12,13,14,3], [], [], [], [], grtree.Pal, [], [3, 3, 3, 9], [1, 3, 3, 3], griron.Pal, grgold.Pal, grgem.Pal, []], [zombiesDroppedCount, grwater.DroppedCount, grsand.DroppedCount, grgrass.DroppedCount, grrock.DroppedCount, grtree.DroppedCount, grfarm.DroppedCount, grwheat.DroppedCount, grplant.DroppedCount, griron.DroppedCount, grgold.DroppedCount, grgem.DroppedCount, grhole.DroppedCount]);
                    ypos += 10;
                    //bar full
                    ypos = DrawStats(1, ypos, "bar full:", barFull, [75, 102, 98, 102, 99, 101, 100], [[], pwrPal[0], pwrPal[1], pwrPal[1], pwrPal[1], pwrPal[1], pwrPal[1]]);
                    ypos += 10;
                    //menu time
                    ypos = DrawStats(1, ypos, "menu time:", menuTime, [66, 89, 89, 90, 91, 76, 74, 92], [[], workbench.Pal, stonebench.Pal, [], [], [], [], []]);
                    ypos += 10;
                    //ladder resets
                    ypos = DrawStats(1, ypos, "ladder resets:", ladderResets, [75, 99], [[], pwrPal[1]]);
                    ypos += 10;
                    Printc("press button 1", 64, Math.Max(ypos, 112), F32.FloorToInt(6 + time % 2));
                }
                if (curMenu == mainMenu)
                {
                    Printc("btn 0 to change seed", 64, 108, F32.FloorToInt(6 + time % 2));
                    Printc(loadedSeed is not null ? "btn 1 to play" : "", 64, 116, F32.FloorToInt(6 + time % 2));
                }
                else if (!collectStats && curMenu != introMenu)
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

        public override void Dispose()
        {
            TextInputEXT.StopTextInput();
            base.Dispose();
        }
    }
}
