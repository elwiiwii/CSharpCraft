using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;

namespace CSharpCraft.PcraftBase.Update;

internal static class PlayerActionUpdater
{
    internal static void Update(
        PlayerEntity player, Level level, PcraftGame game,
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
            var hit      = PcraftServices.GetGr(hitx, hity, level);
            var stamcost = F32.FromInt(20);

            // Place bench
            if (!player.Lb5 && player.CurItem != null && player.CurItem.Type.Drop)
            {
                if (hit == PcraftData.GrSand || hit == PcraftData.GrGrass)
                {
                    var tileX  = F32.Floor(hitx / F32.FromInt(16)) * 16 + 8;
                    var tileY  = F32.Floor(hity / F32.FromInt(16)) * 16 + 8;
                    var placed = new ItemEntity(player.CurItem.Type, tileX, tileY)
                    {
                        HasCol = true,
                        List   = player.CurItem.List
                    };
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
                    if (player.CurItem != null && player.CurItem.Type == PcraftData.Sword)
                    {
                        var p = player.CurItem.Power ?? 0;
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
                        var popup = new ItemEntity(PcraftData.EText, e.X, e.Y - F32.FromInt(10), F32.Zero, -F32.One)
                        {
                            TextValue = pow,
                            TextColor = 9,
                            Timer     = F32.FromInt(20)
                        };
                        level.Ent.Add(popup);
                    }
                }
                else if (hit.Mat != null)
                {
                    // ── Harvest tile ──────────────────────────────────────────
                    Pico8.Sfx(15);
                    var pow = F32.One;
                    if (player.CurItem != null)
                    {
                        var p = player.CurItem.Power ?? 0;
                        if (hit == PcraftData.GrRock || hit.IsTree)
                        {
                            if (hit == PcraftData.GrTree && player.CurItem.Type == PcraftData.Haxe)
                            {
                                pow = F32.One + p + Pico8.Rnd(p * p);
                                stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                                Pico8.Sfx(12);
                            }
                            else if (player.CurItem.Type == PcraftData.Pick)
                            {
                                pow = F32.One + p * 2 + Pico8.Rnd(p * p);
                                stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                                Pico8.Sfx(12);
                            }
                        }
                    }
                    pow = F32.Floor(pow);
                    var d = PcraftServices.GetData(hitx, hity, F32.FromInt(hit.Life), level);
                    if (d - pow <= F32.Zero)
                    {
                        PcraftServices.SetGr(hitx, hity, hit.Tile ?? PcraftData.GrSand, level);
                        PcraftServices.ClearData(hitx, hity, level);
                        PcraftServices.AddItem(hit.Mat, F32.FloorToInt(Pico8.Rnd(3)) + 2, hitx, hity, level.Ent);
                        if (hit == PcraftData.GrTree && Pico8.Rnd(1) > F32.FromDouble(0.7))
                            PcraftServices.AddItem(PcraftData.Apple, 1, hitx, hity, level.Ent);
                    }
                    else
                    {
                        PcraftServices.SetData(hitx, hity, d - pow, level);
                    }
                    var popup2 = new ItemEntity(PcraftData.EText, hitx, hity, F32.Zero, -F32.One)
                    {
                        TextValue = pow,
                        TextColor = 10,
                        Timer     = F32.FromInt(20)
                    };
                    level.Ent.Add(popup2);
                }
                else
                {
                    // ── Other item use ────────────────────────────────────────
                    Pico8.Sfx(19);
                    if (player.CurItem != null)
                    {
                        if (player.CurItem.Power is not null)
                            stamcost = F32.Max(F32.Zero, F32.FromInt(20 - (int)player.CurItem.Power * 2));

                        if (player.CurItem.Type.GiveLife > 0)
                        {
                            player.Life = F32.Min(F32.FromInt(100), player.Life + F32.FromInt(player.CurItem.Type.GiveLife));
                            PcraftServices.RemInList(player.Invent, new ItemStack(player.CurItem.Type, count: 1));
                            Pico8.Sfx(21);
                        }
                        if (hit == PcraftData.GrGrass && player.CurItem.Type == PcraftData.Scythe)
                        {
                            PcraftServices.SetGr(hitx, hity, PcraftData.GrSand, level);
                            if (Pico8.Rnd(1) > F32.FromDouble(0.4))
                                PcraftServices.AddItem(PcraftData.Seed, 1, hitx, hity, level.Ent);
                        }
                        if (hit == PcraftData.GrSand && player.CurItem.Type == PcraftData.Shovel)
                        {
                            if ((player.CurItem.Power ?? 0) > 3)
                            {
                                PcraftServices.SetGr(hitx, hity, PcraftData.GrWater, level);
                                PcraftServices.AddItem(PcraftData.Sand, 2, hitx, hity, level.Ent);
                            }
                            else
                            {
                                PcraftServices.SetGr(hitx, hity, PcraftData.GrFarm, level);
                                PcraftServices.SetData(hitx, hity, level.Time + 15 + Pico8.Rnd(5), level);
                                PcraftServices.AddItem(PcraftData.Sand, F32.FloorToInt(Pico8.Rnd(2)), hitx, hity, level.Ent);
                            }
                        }
                        if (hit == PcraftData.GrWater && player.CurItem.Type == PcraftData.Sand)
                        {
                            PcraftServices.SetGr(hitx, hity, PcraftData.GrSand, level);
                            PcraftServices.RemInList(player.Invent, new ItemStack(PcraftData.Sand, count: 1));
                        }
                        if (hit == PcraftData.GrWater && player.CurItem.Type == PcraftData.Boat)
                        {
                            Pico8.Reload();
                            Pico8.MapToSpritesheet1D();
                            player.CurMenu = PcraftData.WinMenu;
                            Pico8.Music(4);
                        }
                        if (hit == PcraftData.GrFarm && player.CurItem.Type == PcraftData.Seed)
                        {
                            PcraftServices.SetGr(hitx, hity, PcraftData.GrWheat, level);
                            PcraftServices.SetData(hitx, hity, level.Time + 15 + Pico8.Rnd(5), level);
                            PcraftServices.RemInList(player.Invent, new ItemStack(PcraftData.Seed, count: 1));
                        }
                        if (hit == PcraftData.GrWheat && player.CurItem.Type == PcraftData.Scythe)
                        {
                            PcraftServices.SetGr(hitx, hity, PcraftData.GrSand, level);
                            var dw = F32.Clamp(F32.FromInt(4) - (PcraftServices.GetData(hitx, hity, F32.Zero, level) - level.Time),
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
