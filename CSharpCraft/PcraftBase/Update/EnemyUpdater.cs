using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Update;

internal static class EnemyUpdater
{
    internal static List<CharacterEntity> Update(PlayerEntity player, Level level)
    {
        var camera = player.Camera;
        var nearEnemies = new List<CharacterEntity>();

        var ebx = Pico8.Cos(player.Prot);
        var eby = Pico8.Sin(player.Prot);

        foreach (var e in level.Ene)
        {
            if (!PcraftServices.IsIn(e, F32.FromInt(100), camera.Clx, camera.Cly)) continue;

            var distp  = PcraftServices.GetLen(e.X - player.X, e.Y - player.Y);
            var mspeed = F32.FromDouble(0.8);

            var disten = PcraftServices.GetLen(
                e.X - player.X - ebx * F32.FromInt(8),
                e.Y - player.Y - eby * F32.FromInt(8));
            if (disten < F32.FromInt(10))
                nearEnemies.Add(e);

            if (distp < F32.FromInt(8))
            {
                e.Ox += F32.Clamp(e.X - player.X, F32.FromDouble(-0.4), F32.FromDouble(0.4));
                e.Oy += F32.Clamp(e.Y - player.Y, F32.FromDouble(-0.4), F32.FromDouble(0.4));
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
                        e.Dx   += player.X - e.X;
                        e.Dy   += player.Y - e.Y;
                        e.Banim = F32.Zero;
                    }
                    else
                    {
                        e.Dx    = F32.Zero;
                        e.Dy    = F32.Zero;
                        e.Banim -= F32.One;
                        e.Banim = Pico8.Mod(e.Banim, F32.FromInt(8));
                        var pow = F32.FromInt(10);
                        if (e.Banim == F32.FromInt(4))
                        {
                            player.Life -= pow;
                            var popup = new TextPopupEntity(pow, 8, player.X, player.Y - F32.FromInt(10), -F32.One);
                            level.Ent.Add(popup);
                            Pico8.Sfx(14 + Pico8.Rnd(2).Float);
                        }
                        player.Life = F32.Max(F32.Zero, player.Life);
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

            var dl = mspeed * PcraftServices.GetInvLen(e.Dx, e.Dy);
            e.Dx *= dl;
            e.Dy *= dl;

            var fx = e.Dx + e.Ox;
            var fy = e.Dy + e.Oy;
            (fx, fy) = PcraftServices.ReflectCol(
                e.X, e.Y, fx, fy,
                (x2, y2) => PcraftServices.IsFreeEnem(x2, y2, level),
                F32.Zero);

            if (F32.Abs(e.Dx) > F32.Zero || F32.Abs(e.Dy) > F32.Zero)
            {
                e.Lrot  = PcraftServices.GetRot(e.Dx, e.Dy);
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

            e.Prot = PcraftServices.UpRot(e.Lrot, e.Prot);
        }

        return nearEnemies;
    }
}
