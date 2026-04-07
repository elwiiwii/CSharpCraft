using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Map;
using CSharpCraft.Pcraft.Physics;
using PSharp8;

namespace CSharpCraft.Pcraft.Update;

internal static class EnemyUpdater
{
    internal static void Update(WorldState state)
    {
        state.NearEnemies.Clear();

        var ebx = Pico8.Cos(state.Prot);
        var eby = Pico8.Sin(state.Prot);

        foreach (var e in state.Enemies)
        {
            if (!CollisionSystem.IsIn(e, F32.FromInt(100), state.Clx, state.Cly)) continue;

            if (e is PlayerEntity)
            {
                e.X = state.Plx;
                e.Y = state.Ply;
            }
            else
            {
                var distp  = PcraftMath.GetLen(e.X - state.Plx, e.Y - state.Ply);
                var mspeed = F32.FromFloat(0.8f);

                var disten = PcraftMath.GetLen(
                    e.X - state.Plx - ebx * F32.FromInt(8),
                    e.Y - state.Ply - eby * F32.FromInt(8));
                if (disten < F32.FromInt(10))
                    state.NearEnemies.Add(e);

                if (distp < F32.FromInt(8))
                {
                    e.Ox += F32.Max(F32.FromFloat(-0.4f), F32.Min(F32.FromFloat(0.4f), e.X - state.Plx));
                    e.Oy += F32.Max(F32.FromFloat(-0.4f), F32.Min(F32.FromFloat(0.4f), e.Y - state.Ply));
                }

                if (e.Dtim <= F32.Zero)
                {
                    // enstep_wait=0, enstep_patrol=3 → walk
                    if (e.Step == 0 || e.Step == 3)
                    {
                        e.Step = 1; // enstep_walk
                        e.Dx   = Pico8.Rnd(2) - F32.One;
                        e.Dy   = Pico8.Rnd(2) - F32.One;
                        e.Dtim = F32.FromInt(30) + Pico8.Rnd(60);
                    }
                    else if (e.Step == 1) // enstep_walk → wait
                    {
                        e.Step = 0; // enstep_wait
                        e.Dx   = F32.Zero;
                        e.Dy   = F32.Zero;
                        e.Dtim = F32.FromInt(30) + Pico8.Rnd(60);
                    }
                    else // chase — reset timer only
                    {
                        e.Dtim = F32.FromInt(10) + Pico8.Rnd(60);
                    }
                }
                else
                {
                    if (e.Step == 2) // enstep_chase
                    {
                        if (distp > F32.FromInt(10))
                        {
                            e.Dx   += state.Plx - e.X;
                            e.Dy   += state.Ply - e.Y;
                            e.Banim = F32.Zero;
                        }
                        else
                        {
                            e.Dx    = F32.Zero;
                            e.Dy    = F32.Zero;
                            e.Banim -= F32.One;
                            var eight = F32.FromInt(8);
                            e.Banim = ((e.Banim % eight) + eight) % eight;
                            var pow = F32.FromInt(10);
                            if (e.Banim == F32.FromInt(4))
                            {
                                state.Plife -= pow;
                                var popup = new ItemEntity(
                                    PcraftData.EText,
                                    state.Plx, state.Ply - F32.FromInt(10),
                                    F32.Zero, -F32.One);
                                popup.TextValue = pow;
                                popup.TextColor = 8;
                                popup.Timer     = F32.FromInt(20);
                                state.Entities.Add(popup);
                                Pico8.Sfx(14 + Pico8.Rnd(2).Float);
                            }
                            state.Plife = F32.Max(F32.Zero, state.Plife);
                        }
                        mspeed = F32.FromFloat(1.4f);
                        if (distp > F32.FromInt(70))
                        {
                            e.Step = 3; // enstep_patrol
                            e.Dtim = F32.FromInt(30) + Pico8.Rnd(60);
                        }
                    }
                    else
                    {
                        if (distp < F32.FromInt(40))
                        {
                            e.Step = 2; // enstep_chase
                            e.Dtim = F32.FromInt(10) + Pico8.Rnd(60);
                        }
                    }
                    e.Dtim -= F32.One;
                }

                var dl = mspeed * PcraftMath.GetInvLen(e.Dx, e.Dy);
                e.Dx *= dl;
                e.Dy *= dl;

                var fx = e.Dx + e.Ox;
                var fy = e.Dy + e.Oy;
                (fx, fy) = CollisionSystem.ReflectCol(
                    e.X, e.Y, fx, fy,
                    (x2, y2) => MapOps.IsFreeEnem(x2, y2, state),
                    F32.Zero);

                if (F32.Abs(e.Dx) > F32.Zero || F32.Abs(e.Dy) > F32.Zero)
                {
                    e.Lrot  = PcraftMath.GetRot(e.Dx, e.Dy);
                    e.Panim += F32.FromFloat(1f / 33f);
                }
                else
                {
                    e.Panim = F32.Zero;
                }

                e.X += fx;
                e.Y += fy;

                e.Ox *= F32.FromFloat(0.9f);
                e.Oy *= F32.FromFloat(0.9f);

                e.Prot = PcraftMath.UpRot(e.Lrot, e.Prot);
            }
        }
    }
}
