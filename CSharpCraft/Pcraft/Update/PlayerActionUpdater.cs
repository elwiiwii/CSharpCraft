using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Inventory;
using CSharpCraft.Pcraft.Map;
using CSharpCraft.Pcraft.Physics;
using PSharp8;

namespace CSharpCraft.Pcraft.Update;

internal static class PlayerActionUpdater
{
    internal static void Update(
        WorldState state, PcraftGame game,
        F32 dx, F32 dy, bool canAct, Random rng)
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
        state.Llife += F32.Max(-F32.One, F32.Min(F32.One, state.Plife - state.Llife));
        state.Lstam += F32.Max(-F32.One, F32.Min(F32.One, state.Pstam - state.Lstam));

        // ── Btn(5) action block ───────────────────────────────────────────────
        if (Pico8.Btn(5) && !state.Block5 && canAct)
        {
            var bx       = Pico8.Cos(state.Prot);
            var by       = Pico8.Sin(state.Prot);
            var hitx     = state.Plx + bx * F32.FromInt(8);
            var hity     = state.Ply + by * F32.FromInt(8);
            var hit      = MapOps.GetGr(hitx, hity, state);
            var stamcost = F32.FromInt(20);

            // Drop placed item
            if (!state.Lb5 && state.CurItem != null && state.CurItem.Type.Drop)
            {
                if (hit == PcraftData.GrSand || hit == PcraftData.GrGrass)
                {
                    var tileX  = F32.FromInt(F32.FloorToInt(hitx / F32.FromInt(16)) * 16 + 8);
                    var tileY  = F32.FromInt(F32.FloorToInt(hity / F32.FromInt(16)) * 16 + 8);
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
                        pow = F32.One + F32.FromInt(p) + Pico8.Rnd(p * p, rng);
                        stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                        pow = F32.FromInt(F32.FloorToInt(pow));
                        Pico8.Sfx(14 + Pico8.Rnd(2, rng).Float);
                    }
                    var count = F32.FromInt(state.NearEnemies.Count);
                    foreach (var e in state.NearEnemies.ToList())
                    {
                        e.Life -= pow / count;
                        var push = (pow - F32.One) * F32.FromFloat(0.5f);
                        e.Ox += F32.Max(-push, F32.Min(push, e.X - state.Plx));
                        e.Oy += F32.Max(-push, F32.Min(push, e.Y - state.Ply));
                        if (e.Life <= F32.Zero)
                        {
                            state.Enemies.Remove(e);
                            LevelManager.AddItem(PcraftData.Ichor,  F32.FloorToInt(Pico8.Rnd(3, rng)), e.X, e.Y, state.Entities, rng);
                            LevelManager.AddItem(PcraftData.Fabric, F32.FloorToInt(Pico8.Rnd(3, rng)), e.X, e.Y, state.Entities, rng);
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
                                pow = F32.One + F32.FromInt(p) + Pico8.Rnd(p * p, rng);
                                stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                                Pico8.Sfx(12);
                            }
                        }
                        else if ((hit == PcraftData.GrRock || hit.IsTree) && state.CurItem.Type == PcraftData.Pick)
                        {
                            pow = F32.One + F32.FromInt(p * 2) + Pico8.Rnd(p * p, rng);
                            stamcost = F32.Max(F32.Zero, F32.FromInt(20 - p * 2));
                            Pico8.Sfx(12);
                        }
                    }
                    pow = F32.FromInt(F32.FloorToInt(pow));
                    var d = MapOps.GetData(hitx, hity, F32.FromInt(hit.Life), state);
                    if (d - pow <= F32.Zero)
                    {
                        MapOps.SetGr(hitx, hity, hit.Tile ?? PcraftData.GrSand, state);
                        MapOps.ClearData(hitx, hity, state);
                        LevelManager.AddItem(hit.Mat, F32.FloorToInt(Pico8.Rnd(3, rng)) + 2, hitx, hity, state.Entities, rng);
                        if (hit == PcraftData.GrTree && Pico8.Rnd(1, rng).Float > 0.7f)
                            LevelManager.AddItem(PcraftData.Apple, 1, hitx, hity, state.Entities, rng);
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
                        if (state.CurItem.Power is {} pw)
                            stamcost = F32.Max(F32.Zero, F32.FromInt(20 - pw * 2));

                        if (state.CurItem.Type.GiveLife > 0)
                        {
                            state.Plife = F32.Min(F32.FromInt(100), state.Plife + F32.FromInt(state.CurItem.Type.GiveLife));
                            InventoryOps.RemInList(state.Invent, new ItemStack(state.CurItem.Type, count: 1));
                            Pico8.Sfx(21);
                        }
                        if (hit == PcraftData.GrGrass && state.CurItem.Type == PcraftData.Scythe)
                        {
                            MapOps.SetGr(hitx, hity, PcraftData.GrSand, state);
                            if (Pico8.Rnd(1, rng).Float > 0.4f)
                                LevelManager.AddItem(PcraftData.Seed, 1, hitx, hity, state.Entities, rng);
                        }
                        if (hit == PcraftData.GrSand && state.CurItem.Type == PcraftData.Shovel)
                        {
                            if ((state.CurItem.Power ?? 0) > 3)
                            {
                                MapOps.SetGr(hitx, hity, PcraftData.GrWater, state);
                                LevelManager.AddItem(PcraftData.Sand, 2, hitx, hity, state.Entities, rng);
                            }
                            else
                            {
                                MapOps.SetGr(hitx, hity, PcraftData.GrFarm, state);
                                MapOps.SetData(hitx, hity, state.Time + F32.FromInt(15) + Pico8.Rnd(5, rng), state);
                                LevelManager.AddItem(PcraftData.Sand, F32.FloorToInt(Pico8.Rnd(2, rng)), hitx, hity, state.Entities, rng);
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
                            state.CurMenu = PcraftData.WinMenu;
                            Pico8.Music(4);
                        }
                        if (hit == PcraftData.GrFarm && state.CurItem.Type == PcraftData.Seed)
                        {
                            MapOps.SetGr(hitx, hity, PcraftData.GrWheat, state);
                            MapOps.SetData(hitx, hity, state.Time + F32.FromInt(15) + Pico8.Rnd(5, rng), state);
                            InventoryOps.RemInList(state.Invent, new ItemStack(PcraftData.Seed, count: 1));
                        }
                        if (hit == PcraftData.GrWheat && state.CurItem.Type == PcraftData.Scythe)
                        {
                            MapOps.SetGr(hitx, hity, PcraftData.GrSand, state);
                            var dWh = F32.Max(F32.Zero, F32.Min(
                                F32.FromInt(4),
                                F32.FromInt(4) - (MapOps.GetData(hitx, hity, F32.Zero, state) - state.Time)));
                            LevelManager.AddItem(PcraftData.Wheat,
                                F32.FloorToInt(dWh / F32.FromInt(2) + Pico8.Rnd(dWh / F32.FromInt(2), rng)),
                                hitx, hity, state.Entities, rng);
                            LevelManager.AddItem(PcraftData.Seed, 1, hitx, hity, state.Entities, rng);
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

        // ── Death check (before inventory; lb4 suppresses when held, inventory overrides) ─
        if (!state.Lb4 && state.Plife <= F32.Zero)
        {
            Pico8.Reload();
            state.CurMenu = PcraftData.DeathMenu;
            Pico8.Music(4);
        }

        // ── Inventory shortcut (Btnp 4) ───────────────────────────────────────
        if (Pico8.Btnp(4) && !state.Lb4)
        {
            state.CurMenu = state.MenuInvent;
            Pico8.Sfx(13);
        }

        // ── Button-state latch ────────────────────────────────────────────────
        state.Lb4 = Pico8.Btn(4);
        state.Lb5 = Pico8.Btn(5);
        if (!Pico8.Btn(5))
            state.Block5 = false;

        // ── Time advance ──────────────────────────────────────────────────────
        state.Time += F32.FromFloat(1f / 30f);
    }
}
