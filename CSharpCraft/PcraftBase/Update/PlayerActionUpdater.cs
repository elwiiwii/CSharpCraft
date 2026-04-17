using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Inventory;
using CSharpCraft.PcraftBase.Map;
using CSharpCraft.PcraftBase.Menu;
using CSharpCraft.PcraftBase.Physics;

namespace CSharpCraft.PcraftBase.Update;

internal static class PlayerActionUpdater
{
    internal static void Update(
        WorldState state, PcraftGame game,
        F32 dx, F32 dy, bool canAct)
    {
        // ── Final collision + player advance ─────────────────────────────────
        (dx, dy) = CollisionSystem.ReflectCol(
            state.Plx, state.Ply, dx, dy,
            (x, y) => MapOps.IsFree(x, y, state),
            F32.Zero);
        state.Plx += dx;
        state.Ply += dy;

        state.Prot = PcraftMath.UpRot(state.Lrot, state.Prot);

        // ── Smooth bars (llife / lstam) ───────────────────────────────────────
        state.Llife += F32.Clamp(state.Plife - state.Llife, -F32.One, F32.One);
        state.Lstam += F32.Clamp(state.Pstam - state.Lstam, -F32.One, F32.One);

        // ── Btn(5) action block ───────────────────────────────────────────────
        if (Pico8.Btn(5) && !state.Block5 && canAct)
        {
            var bx       = Pico8.Cos(state.Prot);
            var by       = Pico8.Sin(state.Prot);
            var hitx     = state.Plx + bx * F32.FromInt(8);
            var hity     = state.Ply + by * F32.FromInt(8);
            var hit      = MapOps.GetGr(hitx, hity, state);
            var stamcost = F32.FromInt(20);

            // Place bench
            if (!state.Lb5 && state.CurItem != null && state.CurItem.Type.Drop)
            {
                if (hit == PcraftData.GrSand || hit == PcraftData.GrGrass)
                {
                    var tileX  = F32.Floor(hitx / F32.FromInt(16)) * 16 + 8;
                    var tileY  = F32.Floor(hity / F32.FromInt(16)) * 16 + 8;
                    var placed = new ItemEntity(state.CurItem.Type, tileX, tileY)
                    {
                        HasCol = true,
                        List   = state.CurItem.List
                    };
                    state.Entities.Add(placed);
                    InventoryOps.RemInList(state.Invent, state.CurItem);
                    canAct = false;
                }
            }

            if (state.Banim == F32.Zero && state.Pstam > F32.Zero && canAct)
            {
                state.Banim = F32.FromInt(8);

                if (state.NearEnemies.Count > 0)
                {
                    // ── Attack near enemies ───────────────────────────────────
                    Pico8.Sfx(19);
                    var pow = F32.One;
                    if (state.CurItem != null && state.CurItem.Type == PcraftData.Sword)
                    {
                        var p = state.CurItem.Power ?? 0;
                        pow = F32.One + p + Pico8.Rnd(p * p);
                        stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                        pow = F32.FromInt(F32.FloorToInt(pow));
                        Pico8.Sfx(14 + Pico8.Rnd(2).Double);
                    }
                    var count = F32.FromInt(state.NearEnemies.Count);
                    foreach (var e in state.NearEnemies)
                    {
                        e.Life -= pow / count;
                        var push = (pow - F32.One) * F32.FromDouble(0.5);
                        e.Ox += F32.Clamp(e.X - state.Plx, -push, push);
                        e.Oy += F32.Clamp(e.Y - state.Ply, -push, push);
                        if (e.Life <= F32.Zero)
                        {
                            state.Enemies.Remove(e);
                            state.LevelMgr.AddItem(PcraftData.Ichor,  F32.FloorToInt(Pico8.Rnd(3)), e.X, e.Y, state.Entities);
                            state.LevelMgr.AddItem(PcraftData.Fabric, F32.FloorToInt(Pico8.Rnd(3)), e.X, e.Y, state.Entities);
                        }
                        var popup = new ItemEntity(PcraftData.EText, e.X, e.Y - F32.FromInt(10), F32.Zero, -F32.One)
                        {
                            TextValue = pow,
                            TextColor = 9,
                            Timer     = F32.FromInt(20)
                        };
                        state.Entities.Add(popup);
                    }
                }
                else if (hit.Mat != null)
                {
                    // ── Harvest tile ──────────────────────────────────────────
                    Pico8.Sfx(15);
                    var pow = F32.One;
                    if (state.CurItem != null)
                    {
                        var p = state.CurItem.Power ?? 0;
                        if (hit == PcraftData.GrTree)
                        {
                            if (state.CurItem.Type == PcraftData.Haxe)
                            {
                                pow = F32.One + p + Pico8.Rnd(p * p);
                                stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                                Pico8.Sfx(12);
                            }
                        }
                        else if ((hit == PcraftData.GrRock || hit.IsTree) && state.CurItem.Type == PcraftData.Pick)
                        {
                            pow = F32.One + p * 2 + Pico8.Rnd(p * p);
                            stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                            Pico8.Sfx(12);
                        }
                    }
                    pow = F32.Floor(pow);
                    var d = MapOps.GetData(hitx, hity, F32.FromInt(hit.Life), state);
                    if (d - pow <= F32.Zero)
                    {
                        MapOps.SetGr(hitx, hity, hit.Tile ?? PcraftData.GrSand, state);
                        MapOps.ClearData(hitx, hity, state);
                        state.LevelMgr.AddItem(hit.Mat, F32.FloorToInt(Pico8.Rnd(3)) + 2, hitx, hity, state.Entities);
                        if (hit == PcraftData.GrTree && Pico8.Rnd(1) > F32.FromDouble(0.7))
                            state.LevelMgr.AddItem(PcraftData.Apple, 1, hitx, hity, state.Entities);
                    }
                    else
                    {
                        MapOps.SetData(hitx, hity, d - pow, state);
                    }
                    var popup2 = new ItemEntity(PcraftData.EText, hitx, hity, F32.Zero, -F32.One)
                    {
                        TextValue = pow,
                        TextColor = 10,
                        Timer     = F32.FromInt(20)
                    };
                    state.Entities.Add(popup2);
                }
                else
                {
                    // ── Other item use ────────────────────────────────────────
                    Pico8.Sfx(19);
                    if (state.CurItem != null)
                    {
                        if (state.CurItem.Power is not null)
                            stamcost = F32.Max(F32.Zero, F32.FromInt(20 - (int)state.CurItem.Power * 2));

                        if (state.CurItem.Type.GiveLife > 0)
                        {
                            state.Plife = F32.Min(F32.FromInt(100), state.Plife + F32.FromInt(state.CurItem.Type.GiveLife));
                            InventoryOps.RemInList(state.Invent, new ItemStack(state.CurItem.Type, count: 1));
                            Pico8.Sfx(21);
                        }
                        if (hit == PcraftData.GrGrass && state.CurItem.Type == PcraftData.Scythe)
                        {
                            MapOps.SetGr(hitx, hity, PcraftData.GrSand, state);
                            if (Pico8.Rnd(1) > F32.FromDouble(0.4))
                                state.LevelMgr.AddItem(PcraftData.Seed, 1, hitx, hity, state.Entities);
                        }
                        if (hit == PcraftData.GrSand && state.CurItem.Type == PcraftData.Shovel)
                        {
                            if ((state.CurItem.Power ?? 0) > 3)
                            {
                                MapOps.SetGr(hitx, hity, PcraftData.GrWater, state);
                                state.LevelMgr.AddItem(PcraftData.Sand, 2, hitx, hity, state.Entities);
                            }
                            else
                            {
                                MapOps.SetGr(hitx, hity, PcraftData.GrFarm, state);
                                MapOps.SetData(hitx, hity, state.Time + 15 + Pico8.Rnd(5), state);
                                state.LevelMgr.AddItem(PcraftData.Sand, F32.FloorToInt(Pico8.Rnd(2)), hitx, hity, state.Entities);
                            }
                        }
                        if (hit == PcraftData.GrWater && state.CurItem.Type == PcraftData.Sand)
                        {
                            MapOps.SetGr(hitx, hity, PcraftData.GrSand, state);
                            InventoryOps.RemInList(state.Invent, new ItemStack(PcraftData.Sand, count: 1));
                        }
                        if (hit == PcraftData.GrWater && state.CurItem.Type == PcraftData.Boat)
                        {
                            Pico8.Reload();
                            Pico8.MapToSpritesheet1D();
                            state.CurMenu = PcraftData.WinMenu;
                            Pico8.Music(4);
                        }
                        if (hit == PcraftData.GrFarm && state.CurItem.Type == PcraftData.Seed)
                        {
                            MapOps.SetGr(hitx, hity, PcraftData.GrWheat, state);
                            MapOps.SetData(hitx, hity, state.Time + 15 + Pico8.Rnd(5), state);
                            InventoryOps.RemInList(state.Invent, new ItemStack(PcraftData.Seed, count: 1));
                        }
                        if (hit == PcraftData.GrWheat && state.CurItem.Type == PcraftData.Scythe)
                        {
                            MapOps.SetGr(hitx, hity, PcraftData.GrSand, state);
                            var dw = F32.Clamp(F32.FromInt(4) - (MapOps.GetData(hitx, hity, F32.Zero, state) - state.Time),
                                F32.Zero,
                                F32.FromInt(4));
                            state.LevelMgr.AddItem(PcraftData.Wheat,
                                F32.FloorToInt(dw / F32.FromInt(2) + Pico8.Rnd(dw / F32.FromInt(2))),
                                hitx, hity, state.Entities);
                            state.LevelMgr.AddItem(PcraftData.Seed, 1, hitx, hity, state.Entities);
                        }
                    }
                }
                state.Pstam -= stamcost;
            }
        }

        // ── Banim countdown ───────────────────────────────────────────────────
        if (state.Banim > F32.Zero)
            state.Banim -= F32.One;

        // ── Stamina regen ─────────────────────────────────────────────────────
        if (state.Pstam < F32.FromInt(100))
            state.Pstam = F32.Min(F32.FromInt(100), state.Pstam + F32.One);

        // ── Inventory shortcut (Btnp 4) ───────────────────────────────────────
        if (Pico8.Btnp(4) && !state.Lb4)
        {
            state.CurMenu = new InventoryMenu(state.Invent);
            Pico8.Sfx(13);
        }

        // ── Button-state latch ────────────────────────────────────────────────
        state.Lb4 = Pico8.Btn(4);
        state.Lb5 = Pico8.Btn(5);
        if (!Pico8.Btn(5))
            state.Block5 = false;

        // ── Time advance ──────────────────────────────────────────────────────
        state.Time += F32.FromDouble(1.0 / 30.0);

        // ── Death check ───────────────────────────────────────────────────────
        if (state.Plife <= F32.Zero)
        {
            Pico8.Reload();
            Pico8.MapToSpritesheet1D();
            state.CurMenu = PcraftData.DeathMenu;
            Pico8.Music(4);
        }
    }
}
