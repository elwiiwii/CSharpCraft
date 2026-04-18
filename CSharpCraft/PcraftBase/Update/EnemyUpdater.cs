using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Update;

internal static class EnemyUpdater
{
    internal static void Update(WorldState state)
    {
        state.NearEnemies.Clear();

        var ebx = Pico8.Cos(state.Prot);
        var eby = Pico8.Sin(state.Prot);

        foreach (var e in state.Enemies)
        {
            if (!PcraftServices.IsIn(e, F32.FromInt(100), state.Clx, state.Cly)) continue;

            if (e is PlayerEntity)
            {
                e.X = state.Plx;
                e.Y = state.Ply;
            }
            else
            {
                var distp  = PcraftMath.GetLen(e.X - state.Plx, e.Y - state.Ply);
                var mspeed = F32.FromDouble(0.8);

                var disten = PcraftMath.GetLen(
                    e.X - state.Plx - ebx * F32.FromInt(8),
                    e.Y - state.Ply - eby * F32.FromInt(8));
                if (disten < F32.FromInt(10))
                    state.NearEnemies.Add(e);

                if (distp < F32.FromInt(8))
                {
                    e.Ox += F32.Clamp(e.X - state.Plx, F32.FromDouble(-0.4), F32.FromDouble(0.4));
                    e.Oy += F32.Clamp(e.Y - state.Ply, F32.FromDouble(-0.4), F32.FromDouble(0.4));
                }

                if (e.Dtim <= F32.Zero)
                {
                    if (e.Step == EnStep.Wait || e.Step == EnStep.Patrol)
                    {
                        e.Step = EnStep.Walk;
                        e.Dx   = Pico8.Rnd(2) - F32.One;
                        e.Dy   = Pico8.Rnd(2) - F32.One;
                        e.Dtim = F32.FromInt(30) + Pico8.Rnd(60);
                    }
                    else if (e.Step == EnStep.Walk)
                    {
                        e.Step = EnStep.Wait;
                        e.Dx   = F32.Zero;
                        e.Dy   = F32.Zero;
                        e.Dtim = F32.FromInt(30) + Pico8.Rnd(60);
                    }
                    else // chase
                    {
                        e.Dtim = F32.FromInt(10) + Pico8.Rnd(60);
                    }
                }
                else
                {
                    if (e.Step == EnStep.Chase)
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
                            e.Banim %= F32.FromInt(8);
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
                        mspeed = F32.FromDouble(1.4);
                        if (distp > F32.FromInt(70))
                        {
                            e.Step = EnStep.Patrol;
                            e.Dtim = F32.FromInt(30) + Pico8.Rnd(60);
                        }
                    }
                    else
                    {
                        if (distp < F32.FromInt(40))
                        {
                            e.Step = EnStep.Chase;
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
                (fx, fy) = PcraftServices.ReflectCol(
                    e.X, e.Y, fx, fy,
                    (x2, y2) => PcraftServices.IsFreeEnem(x2, y2, state),
                    F32.Zero);

                if (F32.Abs(e.Dx) > F32.Zero || F32.Abs(e.Dy) > F32.Zero)
                {
                    e.Lrot  = PcraftMath.GetRot(e.Dx, e.Dy);
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

                e.Prot = PcraftMath.UpRot(e.Lrot, e.Prot);
            }
        }
    }
}
