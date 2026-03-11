using System.Text.RegularExpressions;
using CSharpCraft.Pico8;
using FixMath;
using Microsoft.Xna.Framework.Input;
using NativeFileDialogs.Net;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CSharpCraft.Pcraft;

public partial class LoadSeed : SpeedrunBase
{
    public override string SceneName => "load seed";

    [GeneratedRegex(@"\D")]
    private static partial Regex DigitOnly();

    private int yStart = 1;
    private int menuX = 8;
    private bool introActive = false;

    private string fileName = "no seed loaded";
    private int[]? loadedSeed = null;
    private string seedInput = "";
    private KeyboardState prevState;

    private bool collectStats = true;

    private int[]? ImageToByteArray(string imagePath)
    {
        try
        {
            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"File not found: {imagePath}");
                return null;
            }

            loadedSeed = new int[128 * 64];

            using (Image<Rgba32> image = Image.Load<Rgba32>(imagePath))
            {
                if (image.Width != 128 || image.Height != 64)
                {
                    Console.WriteLine($"Invalid image dimensions. Expected 128x64, got {image.Width}x{image.Height}");
                    return null;
                }

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
                                if (col == p8.Colors[i])
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
        catch (UnknownImageFormatException ex)
        {
            Console.WriteLine($"Unsupported image format: {ex.Message}");
            return null;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File not found: {ex.FileName}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image: {ex.Message}");
            return null;
        }
    }

    private void OpenFileDialog()
    {
        string path = Path.Combine($"{AppContext.BaseDirectory}Seeds");
        var result = Nfd.OpenDialog(out path, null);

        if (result == NfdStatus.Ok)
        {
            loadedSeed = ImageToByteArray(path);
            if (loadedSeed is null) { return; }

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
            Random e = new(rngSeed);
            rngSeed = Math.Abs(e.Next(0, int.MaxValue));
            seedInput = rngSeed.ToString();
        }
    }

    public override void Init(Pico8Functions pico8)
    {
        TextInputEXT.StartTextInput();
        TextInputEXT.TextInput += OnTextInput;
        menuX = 8;
        base.Init(pico8);
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
        if (string.IsNullOrEmpty(seedInput))
        {
            rngSeed = 0;
            return;
        }
        else
        {
            seedInput = DigitOnly().Replace(seedInput, "");
            rngSeed = Convert.ToInt32(seedInput);
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
        TextInputEXT.TextInput -= OnTextInput;
        base.Dispose();
    }
}
