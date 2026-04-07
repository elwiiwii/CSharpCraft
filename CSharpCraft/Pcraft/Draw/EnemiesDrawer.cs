using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Map;
using CSharpCraft.Pcraft.Physics;

namespace CSharpCraft.Pcraft.Draw;

internal static class EnemiesDrawer
{
    internal static void DrawEnemies(WorldState state)
    {
        SortY(state.Enemies);

        foreach (var e in state.Enemies)
        {
            if (e is PlayerEntity)
            {
                Pico8.Pal();
                DrawPlayer(state.Plx, state.Ply, state.Prot, state.Panim, state.Banim, isPlayer: true, state);
            }
            else if (CollisionSystem.IsIn(e, F32.FromInt(72), state.Clx, state.Cly))
            {
                Pico8.Pal();
                Pico8.Pal(15, 3);
                Pico8.Pal(4,  1);
                Pico8.Pal(2,  8);
                Pico8.Pal(1,  1);
                DrawPlayer(e.X, e.Y, e.Prot, e.Panim, e.Banim, isPlayer: false, state);
            }
        }
    }

    private static void DrawPlayer(F32 x, F32 y, F32 rot, F32 anim, F32 subAnim, bool isPlayer, WorldState state)
    {
        F32 cr = Pico8.Cos(rot);
        F32 sr = Pico8.Sin(rot);
        F32 cv = -sr;
        F32 sv =  cr;

        F32 fxF = F32.FromInt(F32.FloorToInt(x));
        F32 fyF = F32.FromInt(F32.FloorToInt(y - F32.FromInt(4)));

        F32 lan = Pico8.Sin(anim * F32.FromInt(2)) * F32.FromFloat(1.5f);
        var bel = MapOps.GetGr(x, y, state);

        if (bel == PcraftData.GrWater)
        {
            fyF += F32.FromInt(4);
            Pico8.Circ(fxF + cv * F32.FromInt(3) + cr * lan,
                       fyF + sv * F32.FromInt(3) + sr * lan,
                       F32.FromInt(3), F32.FromInt(6));
            Pico8.Circ(fxF - cv * F32.FromInt(3) - cr * lan,
                       fyF - sv * F32.FromInt(3) - sr * lan,
                       F32.FromInt(3), F32.FromInt(6));

            F32 anc = F32.FromInt(3) + (state.Time * F32.FromInt(3) % F32.One) * F32.FromInt(3);
            Pico8.Circ(fxF + cv * F32.FromInt(3) + cr * lan,
                       fyF + sv * F32.FromInt(3) + sr * lan,
                       anc, F32.FromInt(6));
            Pico8.Circ(fxF - cv * F32.FromInt(3) - cr * lan,
                       fyF - sv * F32.FromInt(3) - sr * lan,
                       anc, F32.FromInt(6));
        }
        else
        {
            Pico8.Circfill(fxF + cv * F32.FromInt(2) - cr * lan,
                           fyF + F32.FromInt(3) + sv * F32.FromInt(2) - sr * lan,
                           F32.FromInt(3), F32.FromInt(1));
            Pico8.Circfill(fxF - cv * F32.FromInt(2) + cr * lan,
                           fyF + F32.FromInt(3) - sv * F32.FromInt(2) + sr * lan,
                           F32.FromInt(3), F32.FromInt(1));
        }

        F32 blade = (rot + F32.FromFloat(0.25f)) % F32.One;
        if (subAnim > F32.Zero)
            blade = blade - F32.FromFloat(0.3f) + subAnim * F32.FromFloat(0.04f);

        F32 bcr = Pico8.Cos(blade);
        F32 bsr = Pico8.Sin(blade);
        var (mx, my) = PcraftMath.Mirror(blade);

        int weap = 75;
        if (isPlayer && state.CurItem != null)
        {
            Pico8.Pal();
            weap = state.CurItem.Type.Spr;
            if (state.CurItem.Power.HasValue)
            {
                int pw = state.CurItem.Power.Value - 1;
                if (pw >= 0 && pw < PcraftData.PwrPal.Length)
                    DrawHelpers.SetPal(PcraftData.PwrPal[pw]);
            }
            if (state.CurItem.Type.Pal != null)
                DrawHelpers.SetPal(state.CurItem.Type.Pal);
        }

        Pico8.Spr(weap,
            fxF.Double + bcr.Double * 4 - cr.Double * lan.Double - mx * 8 + 1,
            fyF.Double + bsr.Double * 4 - sr.Double * lan.Double + my * 8 - 7,
            1, 1, mx == 1, my == 1);

        if (isPlayer) Pico8.Pal();

        if (bel != PcraftData.GrWater)
        {
            Pico8.Circfill(fxF + cv * F32.FromInt(3) + cr * lan,
                           fyF + sv * F32.FromInt(3) + sr * lan,
                           F32.FromInt(3), F32.FromInt(2));
            Pico8.Circfill(fxF - cv * F32.FromInt(3) - cr * lan,
                           fyF - sv * F32.FromInt(3) - sr * lan,
                           F32.FromInt(3), F32.FromInt(2));

            var (my2, mx2) = PcraftMath.Mirror((rot + F32.FromFloat(0.75f)) % F32.One);
            Pico8.Spr(75,
                fxF.Double + cv.Double * 4 + cr.Double * lan.Double - 8 + mx2 * 8 + 1,
                fyF.Double + sv.Double * 4 + sr.Double * lan.Double + my2 * 8 - 7,
                1, 1, mx2 == 0, my2 == 1);
        }

        Pico8.Circfill(fxF + cr,                       fyF + sr - F32.FromInt(2),               F32.FromInt(4), F32.FromInt(2));
        Pico8.Circfill(fxF + cr,                       fyF + sr,                                F32.FromInt(4), F32.FromInt(2));
        Pico8.Circfill(fxF + cr * F32.FromFloat(1.5f), fyF + sr * F32.FromFloat(1.5f) - F32.FromInt(2), F32.FromFloat(2.5f), F32.FromInt(15));
        Pico8.Circfill(fxF - cr,                       fyF - sr - F32.FromInt(3),               F32.FromInt(3), F32.FromInt(4));
    }

    internal static void SortY(List<CharacterEntity> enemies)
    {
        int tv = enemies.Count - 1;
        for (int i = 0; i < tv; i++)
        {
            var t1 = enemies[i];
            var t2 = enemies[i + 1];
            if (t1.Y > t2.Y)
            {
                enemies[i]     = t2;
                enemies[i + 1] = t1;
            }
        }
    }
}
