using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;

namespace CSharpCraft.PcraftBase.Update;

internal static class PlayerActionUpdater
{
    internal static void Update(
        PlayerEntity player, Level level,
        F32 dx, F32 dy, bool canAct, List<CharacterEntity> nearEnemies)
    {
        // ── Final collision + player advance ─────────────────────────────────
        (dx, dy) = PcraftServices.ReflectCol(
            player.X, player.Y, dx, dy,
            (x, y) => PcraftServices.IsFree(x, y, level),
            F32.Zero);
        player.X += dx;
        player.Y += dy;

        player.Prot = PcraftServices.UpRot(player.Lrot, player.Prot);

        // ── Smooth bars (Llife / Lstam) ───────────────────────────────────────
        player.Llife += F32.Clamp(player.Life  - player.Llife, -F32.One, F32.One);
        player.Lstam += F32.Clamp(player.Stam  - player.Lstam, -F32.One, F32.One);

        // ── Btn(5) action block ───────────────────────────────────────────────
        if (Pico8.Btn(5) && !player.Block5 && canAct)
        {
            var bx       = Pico8.Cos(player.Prot);
            var by       = Pico8.Sin(player.Prot);
            var hitx     = player.X + bx * F32.FromInt(8);
            var hity     = player.Y + by * F32.FromInt(8);
            var hitTile   = PcraftServices.GetTile(hitx, hity, level);
            var stamcost = F32.FromInt(20);

            // Place bench
            if (!player.Lb5 && player.CurItem is not null && player.CurItem.Type is PlaceableItemDef def)
            {
                if (hitTile.Surface is null && (hitTile.Floor == PcraftData.FtSand || hitTile.Floor == PcraftData.FtGrass))
                {
                    var tileX  = F32.Floor(hitx / F32.FromInt(16)) * 16 + 8;
                    var tileY  = F32.Floor(hity / F32.FromInt(16)) * 16 + 8;
                    var placed = new PlacedItemEntity(def, tileX, tileY);
                    level.Ent.Add(placed);
                    PcraftServices.RemInList(player.Invent, player.CurItem);
                    canAct = false;
                }
            }

            if (player.Banim == F32.Zero && player.Stam > F32.Zero && canAct)
            {
                player.Banim = F32.FromInt(8);

                if (nearEnemies.Count > 0)
                {
                    // ── Attack near enemies ───────────────────────────────────
                    Pico8.Sfx(19);
                    var pow = F32.One;
                    if (player.CurItem is not null && player.CurItem.Type == PcraftData.Sword)
                    {
                        var p = player.CurItem is ToolItem swordTool ? swordTool.Power : 0;
                        pow = F32.One + p + Pico8.Rnd(p * p);
                        stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                        pow = F32.FromInt(F32.FloorToInt(pow));
                        Pico8.Sfx(14 + Pico8.Rnd(2).Double);
                    }
                    var count = F32.FromInt(nearEnemies.Count);
                    foreach (var e in nearEnemies)
                    {
                        e.Life -= pow / count;
                        var push = (pow - F32.One) * F32.FromDouble(0.5);
                        e.Ox += F32.Clamp(e.X - player.X, -push, push);
                        e.Oy += F32.Clamp(e.Y - player.Y, -push, push);
                        if (e.Life <= F32.Zero)
                        {
                            level.Ene.Remove(e);
                            PcraftServices.AddItem(PcraftData.Ichor,  F32.FloorToInt(Pico8.Rnd(3)), e.X, e.Y, level.Ent);
                            PcraftServices.AddItem(PcraftData.Fabric, F32.FloorToInt(Pico8.Rnd(3)), e.X, e.Y, level.Ent);
                        }
                        var popup = new TextPopupEntity(pow, 9, e.X, e.Y - F32.FromInt(10), -F32.One);
                        level.Ent.Add(popup);
                    }
                }
                else if (hitTile.Surface is not null)
                {
                    // ── Harvest tile ──────────────────────────────────────────
                    var pow = F32.One;
                    bool toolSoundPlayed = false;
                    if (player.CurItem is not null)
                    {
                        var p = player.CurItem is ToolItem harvestTool ? harvestTool.Power : 0;
                        if (hitTile.Surface is not null)
                        {
                            if (hitTile.Surface == PcraftData.StTree && player.CurItem.Type == PcraftData.Haxe)
                            {
                                pow = F32.One + p + Pico8.Rnd(p * p);
                                stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                                Pico8.Sfx(12);
                                toolSoundPlayed = true;
                            }
                            else if (player.CurItem.Type == PcraftData.Pick)
                            {
                                pow = F32.One + p * 2 + Pico8.Rnd(p * p);
                                stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                                Pico8.Sfx(12);
                                toolSoundPlayed = true;
                            }
                        }
                    }
                    if (!toolSoundPlayed) Pico8.Sfx(15);
                    pow = F32.Floor(pow);
                    var harvestLife = hitTile.HarvestLife ?? F32.FromInt(hitTile.Surface!.Life);
                    if (harvestLife - pow <= F32.Zero)
                    {
                        PcraftServices.SetTile(hitx, hity, new Tile(hitTile.Surface!.UnderlyingFloor), level);
                        PcraftServices.AddItem(hitTile.Surface!.Mat, F32.FloorToInt(Pico8.Rnd(3)) + 2, hitx, hity, level.Ent);
                        if (hitTile.Surface == PcraftData.StTree && Pico8.Rnd(1) > F32.FromDouble(0.7))
                            PcraftServices.AddItem(PcraftData.Apple, 1, hitx, hity, level.Ent);
                    }
                    else
                    {
                        PcraftServices.SetTile(hitx, hity, hitTile with { HarvestLife = harvestLife - pow }, level);
                    }
                    var popup2 = new TextPopupEntity(pow, 10, hitx, hity, -F32.One);
                    level.Ent.Add(popup2);
                }
                else
                {
                    // ── Other item use ────────────────────────────────────────
                    Pico8.Sfx(19);
                    if (player.CurItem is not null)
                    {
                        if (player.CurItem is ToolItem toolForStam)
                            stamcost = F32.Max(F32.Zero, F32.FromInt(20 - toolForStam.Power * 2));

                        if (player.CurItem.Type is HealthItemDef health && health.GiveLife > 0)
                        {
                            player.Life = F32.Min(F32.FromInt(100), player.Life + F32.FromInt(health.GiveLife));
                            PcraftServices.RemInList(player.Invent, new StackableItem(player.CurItem.Type, 1));
                            Pico8.Sfx(21);
                        }
                        if (hitTile.Floor == PcraftData.FtGrass && hitTile.Surface is null && player.CurItem.Type == PcraftData.Scythe)
                        {
                            PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.FtSand), level);
                            if (Pico8.Rnd(1) > F32.FromDouble(0.4))
                                PcraftServices.AddItem(PcraftData.Seed, 1, hitx, hity, level.Ent);
                        }
                        if (hitTile.Floor == PcraftData.FtSand && hitTile.Surface is null && player.CurItem.Type == PcraftData.Shovel)
                        {
                            if (player.CurItem is ToolItem shovelTool && shovelTool.Power > 3)
                            {
                                PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.FtWater), level);
                                PcraftServices.AddItem(PcraftData.Sand, 2, hitx, hity, level.Ent);
                            }
                            else
                            {
                                PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.FtFarm, GrowthTimer: level.Time + 15 + Pico8.Rnd(5)), level);
                                PcraftServices.AddItem(PcraftData.Sand, F32.FloorToInt(Pico8.Rnd(2)), hitx, hity, level.Ent);
                            }
                        }
                        if (hitTile.Floor == PcraftData.FtWater && hitTile.Surface is null && player.CurItem.Type == PcraftData.Sand)
                        {
                            PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.FtSand), level);
                            PcraftServices.RemInList(player.Invent, new StackableItem(PcraftData.Sand, 1));
                        }
                        if (hitTile.Floor == PcraftData.FtWater && hitTile.Surface is null && player.CurItem.Type == PcraftData.Boat)
                        {
                            Pico8.Reload();
                            Pico8.MapToSpritesheet1D();
                            player.CurMenu = PcraftData.WinMenu;
                            Pico8.Music(4);
                        }
                        if (hitTile.Floor == PcraftData.FtFarm && player.CurItem.Type == PcraftData.Seed)
                        {
                            PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.FtWheat, GrowthTimer: level.Time + 15 + Pico8.Rnd(5)), level);
                            PcraftServices.RemInList(player.Invent, new StackableItem(PcraftData.Seed, 1));
                        }
                        if (hitTile.Floor == PcraftData.FtWheat && player.CurItem.Type == PcraftData.Scythe)
                        {
                            PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.FtSand), level);
                            var dw = F32.Clamp(F32.FromInt(4) - (hitTile.GrowthTimer.GetValueOrDefault(F32.Zero) - level.Time),
                                F32.Zero,
                                F32.FromInt(4));
                            PcraftServices.AddItem(PcraftData.Wheat,
                                F32.FloorToInt(dw / F32.FromInt(2) + Pico8.Rnd(dw / F32.FromInt(2))),
                                hitx, hity, level.Ent);
                            PcraftServices.AddItem(PcraftData.Seed, 1, hitx, hity, level.Ent);
                        }
                    }
                }
                player.Stam -= stamcost;
            }
        }

        // ── Banim countdown ───────────────────────────────────────────────────
        if (player.Banim > F32.Zero)
            player.Banim -= F32.One;

        // ── Stamina regen ─────────────────────────────────────────────────────
        if (player.Stam < F32.FromInt(100))
            player.Stam = F32.Min(F32.FromInt(100), player.Stam + F32.One);

        // ── Inventory shortcut (Btnp 4) ───────────────────────────────────────
        if (Pico8.Btnp(4) && !player.Lb4)
        {
            player.CurMenu = new InventoryMenu(player.Invent);
            Pico8.Sfx(13);
        }

        // ── Button-state latch ────────────────────────────────────────────────
        player.Lb4 = Pico8.Btn(4);
        player.Lb5 = Pico8.Btn(5);
        if (!Pico8.Btn(5))
            player.Block5 = false;

        // ── Time advance ──────────────────────────────────────────────────────
        level.Time += F32.FromDouble(1.0 / 30.0);

        // ── Death check ───────────────────────────────────────────────────────
        if (player.Life <= F32.Zero)
        {
            Pico8.Reload();
            Pico8.MapToSpritesheet1D();
            player.CurMenu = PcraftData.DeathMenu;
            Pico8.Music(4);
        }
    }
}
