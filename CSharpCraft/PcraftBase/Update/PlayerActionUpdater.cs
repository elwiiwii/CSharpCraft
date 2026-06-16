using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;
using PSharp8.Graphics;
using PSharp8.Input;

namespace CSharpCraft.PcraftBase.Update;

internal static class PlayerActionUpdater
{
    internal static void Update(
        PlayerEntity player,
        F32 dx, F32 dy, bool canAct, List<CharacterEntity> nearEnemies)
    {
        Level level = player.CurrentLevel!;
        // ── Final collision + player advance ─────────────────────────────────
        (dx, dy) = PcraftServices.ReflectCol(
            player.X, player.Y, dx, dy,
            (x, y) => PcraftServices.IsFree(x, y, level),
            F32.Zero);
        player.X += dx;
        player.Y += dy;

        player.Prot = PcraftServices.UpRot(player.Lrot, player.Prot);

        // ── Smooth bars (Llife / Lstam) ───────────────────────────────────────
        player.Llife += F32.Clamp(player.Life - player.Llife, -F32.One, F32.One);
        player.Lstam += F32.Clamp(player.Stam - player.Lstam, -F32.One, F32.One);

        // ── Btn(5) action block ───────────────────────────────────────────────
        if (Pico8.Btn(PicoButton.Primary) && !player.Block5 && canAct)
        {
            F32 bx = Pico8.Cos(player.Prot);
            F32 by = Pico8.Sin(player.Prot);
            F32 hitx = player.X + (bx * F32.FromInt(8));
            F32 hity = player.Y + (by * F32.FromInt(8));
            Tile hitTile = PcraftServices.GetTile(hitx, hity, level);
            F32 stamcost = F32.FromInt(20);

            // Place bench
            if (Pico8.Btnp(PicoButton.Primary, repeat: false) && player.CurItem?.Type is PlaceableItemDef def)
            {
                if (hitTile.Type is not WallTileType && (hitTile.Type == PcraftData.TileSand || hitTile.Type == PcraftData.TileGrass))
                {
                    F32 tileX = (F32.Floor(hitx / F32.FromInt(16)) * 16) + 8;
                    F32 tileY = (F32.Floor(hity / F32.FromInt(16)) * 16) + 8;
                    PlacedItemEntity placed = new(def, tileX, tileY);
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
                    F32 pow = F32.One;
                    if (player.CurItem is not null && player.CurItem.Type == PcraftData.Sword)
                    {
                        int p = player.CurItem is ToolItem swordTool ? swordTool.Power : 0;
                        pow = F32.One + p + Pico8.Rnd(p * p);
                        stamcost = F32.Max(F32.Zero, F32.FromInt(20 - (p * 2)));
                        pow = F32.FromInt(F32.FloorToInt(pow));
                        Pico8.Sfx(14 + Pico8.Rnd(2).Double);
                    }
                    F32 count = F32.FromInt(nearEnemies.Count);
                    foreach (CharacterEntity e in nearEnemies)
                    {
                        e.Life -= pow / count;
                        F32 push = (pow - F32.One) * F32.FromDouble(0.5);
                        e.Ox += F32.Clamp(e.X - player.X, -push, push);
                        e.Oy += F32.Clamp(e.Y - player.Y, -push, push);
                        if (e.Life <= F32.Zero)
                        {
                            _ = level.Ene.Remove(e);
                            PcraftServices.AddItem(PcraftData.Ichor, 0, 2, e.X, e.Y, level.Ent, playerFacing: player.Prot);
                            PcraftServices.AddItem(PcraftData.Fabric, 0, 2, e.X, e.Y, level.Ent, playerFacing: player.Prot);
                        }
                        TextPopupEntity popup = new(pow, PicoColor._09Orange, e.X, e.Y - F32.FromInt(10), -F32.One);
                        level.Ent.Add(popup);
                    }
                }
                else if (hitTile.Type is WallTileType wall)
                {
                    // ── Harvest tile ──────────────────────────────────────────
                    F32 pow = F32.One;
                    bool toolSoundPlayed = false;
                    if (player.CurItem is not null)
                    {
                        int p = player.CurItem is ToolItem harvestTool ? harvestTool.Power : 0;
                        if (hitTile.Type == PcraftData.TileTree && player.CurItem.Type == PcraftData.Haxe)
                        {
                            pow = F32.One + p + Pico8.Rnd(p * p);
                            stamcost = F32.Max(F32.Zero, F32.FromInt(20 - (p * 2)));
                            Pico8.Sfx(12);
                            toolSoundPlayed = true;
                        }
                        else if (player.CurItem.Type == PcraftData.Pick)
                        {
                            pow = F32.One + (p * 2) + Pico8.Rnd(p * p);
                            stamcost = F32.Max(F32.Zero, F32.FromInt(20 - (p * 2)));
                            Pico8.Sfx(12);
                            toolSoundPlayed = true;
                        }
                    }
                    if (!toolSoundPlayed) Pico8.Sfx(15);
                    pow = F32.Floor(pow);
                    F32 harvestLife = hitTile.HarvestLife ?? F32.FromInt(wall.Life);
                    if (harvestLife - pow <= F32.Zero)
                    {
                        PcraftServices.SetTile(hitx, hity, new Tile(wall.UnderlyingType), level);
                        PcraftServices.AddItem(wall.Mat, 2, 4, hitx, hity, level.Ent, playerFacing: player.Prot);
                        if (hitTile.Type == PcraftData.TileTree)
                            PcraftServices.AddItem(PcraftData.Apple, 1, 1, hitx, hity, level.Ent, playerFacing: player.Prot, dropChance: 0.3);
                    }
                    else
                    {
                        PcraftServices.SetTile(hitx, hity, hitTile with { HarvestLife = harvestLife - pow }, level);
                    }
                    TextPopupEntity popup2 = new(pow, PicoColor._10Yellow, hitx, hity, -F32.One);
                    level.Ent.Add(popup2);
                }
                else
                {
                    // ── Other item use ────────────────────────────────────────
                    Pico8.Sfx(19);
                    if (player.CurItem is not null)
                    {
                        if (player.CurItem is ToolItem toolForStam)
                            stamcost = F32.Max(F32.Zero, F32.FromInt(20 - (toolForStam.Power * 2)));

                        if (player.CurItem.Type is HealthItemDef { GiveLife: > 0 } health)
                        {
                            player.Life = F32.Min(F32.FromInt(100), player.Life + F32.FromInt(health.GiveLife));
                            PcraftServices.RemInList(player.Invent, new StackableItem(player.CurItem.Type, 1));
                            Pico8.Sfx(21);
                        }
                        else if (hitTile.Type == PcraftData.TileGrass && player.CurItem.Type == PcraftData.Scythe)
                        {
                            PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.TileSand), level);
                            PcraftServices.AddItem(PcraftData.Seed, 1, 1, hitx, hity, level.Ent, playerFacing: player.Prot, dropChance: 0.6);
                        }
                        else if (hitTile.Type == PcraftData.TileSand && player.CurItem.Type == PcraftData.Shovel)
                        {
                            if (player.CurItem is ToolItem { Power: > 3 })
                            {
                                PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.TileWater), level);
                                PcraftServices.AddItem(PcraftData.Sand, 2, 2, hitx, hity, level.Ent, playerFacing: player.Prot);
                            }
                            else
                            {
                                PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.TileFarm, GrowthTimer: level.Time + 15 + Pico8.Rnd(5)), level);
                                PcraftServices.AddItem(PcraftData.Sand, 0, 1, hitx, hity, level.Ent, playerFacing: player.Prot);
                            }
                        }
                        else if (hitTile.Type == PcraftData.TileWater && player.CurItem.Type == PcraftData.Sand)
                        {
                            PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.TileSand), level);
                            PcraftServices.RemInList(player.Invent, new StackableItem(PcraftData.Sand, 1));
                        }
                        else if (hitTile.Type == PcraftData.TileWater && player.CurItem.Type == PcraftData.Boat)
                        {
                            Pico8.Reload();
                            Pico8.MapToSpritesheet1D();
                            player.CurMenu = PcraftData.WinMenu;
                            Pico8.Music(4);
                        }
                        else if (hitTile.Type == PcraftData.TileFarm && player.CurItem.Type == PcraftData.Seed)
                        {
                            PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.TileWheat, GrowthTimer: level.Time + 15 + Pico8.Rnd(5)), level);
                            PcraftServices.RemInList(player.Invent, new StackableItem(PcraftData.Seed, 1));
                        }
                        else if (hitTile.Type == PcraftData.TileWheat && player.CurItem.Type == PcraftData.Scythe)
                        {
                            PcraftServices.SetTile(hitx, hity, new Tile(PcraftData.TileSand), level);
                            F32 dw = F32.Clamp(F32.FromInt(4) - (hitTile.GrowthTimer.GetValueOrDefault(F32.Zero) - level.Time),
                                F32.Zero,
                                F32.FromInt(4));
                            PcraftServices.AddItem(PcraftData.Wheat, F32.FloorToInt(dw / F32.FromInt(2)), F32.FloorToInt(dw), hitx, hity, level.Ent, playerFacing: player.Prot);
                            PcraftServices.AddItem(PcraftData.Seed, 1, 1, hitx, hity, level.Ent, playerFacing: player.Prot);
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
        if (Pico8.Btnp(PicoButton.Secondary, repeat: false))
        {
            player.CurMenu = new InventoryMenu(player.Invent);
            Pico8.Sfx(13);
        }

        // ── Button-state latch ────────────────────────────────────────────────
        if (!Pico8.Btn(PicoButton.Primary))
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
