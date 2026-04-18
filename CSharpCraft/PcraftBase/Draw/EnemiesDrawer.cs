using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;
using CSharpCraft.PcraftBase.Physics;

namespace CSharpCraft.PcraftBase.Draw;

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

    internal static void DrawPlayer(F32 x, F32 y, F32 rot, F32 anim, F32 subAnim, bool isPlayer, WorldState state)
    {
        F32 cr = Pico8.Cos(rot);
        F32 sr = Pico8.Sin(rot);
        F32 cv = -sr;
        F32 sv =  cr;

        F32 fxF = F32.Floor(x);
        F32 fyF = F32.Floor(y - 4);

        F32 lan = Pico8.Sin(anim * 2) * F32.FromDouble(1.5);
        var bel = MapOps.GetGr(x, y, state);

        if (bel == PcraftData.GrWater)
        {
            fyF += 4;
            Pico8.Circ(fxF + cv * 3 + cr * lan,
                       fyF + sv * 3 + sr * lan,
                       F32.FromInt(3), F32.FromInt(6));
            Pico8.Circ(fxF - cv * 3 - cr * lan,
                       fyF - sv * 3 - sr * lan,
                       F32.FromInt(3), F32.FromInt(6));

            F32 anc = 3 + state.Time * 3 % F32.One * 3;
            Pico8.Circ(fxF + cv * 3 + cr * lan,
                       fyF + sv * 3 + sr * lan,
                       anc, F32.FromInt(6));
            Pico8.Circ(fxF - cv * 3 - cr * lan,
                       fyF - sv * 3 - sr * lan,
                       anc, F32.FromInt(6));
        }
        else
        {
            Pico8.Circfill(fxF + cv * 2 - cr * lan,
                           fyF + 3 + sv * 2 - sr * lan,
                           F32.FromInt(3), F32.FromInt(1));
            Pico8.Circfill(fxF - cv * 2 + cr * lan,
                           fyF + 3 - sv * 2 + sr * lan,
                           F32.FromInt(3), F32.FromInt(1));
        }

        F32 blade = (rot + F32.FromDouble(0.25)) % F32.One;
        if (subAnim > F32.Zero)
            blade = blade - F32.FromDouble(0.3) + subAnim * F32.FromDouble(0.04);

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
            Pico8.Circfill(fxF + cv * 3 + cr * lan,
                           fyF + sv * 3 + sr * lan,
                           F32.FromInt(3), F32.FromInt(2));
            Pico8.Circfill(fxF - cv * 3 - cr * lan,
                           fyF - sv * 3 - sr * lan,
                           F32.FromInt(3), F32.FromInt(2));

            var (my2, mx2) = PcraftMath.Mirror((rot + F32.FromDouble(0.75)) % F32.One);
            Pico8.Spr(75,
                fxF.Double + cv.Double * 4 + cr.Double * lan.Double - 8 + mx2 * 8 + 1,
                fyF.Double + sv.Double * 4 + sr.Double * lan.Double + my2 * 8 - 7,
                1, 1, mx2 == 0, my2 == 1);
        }

        Pico8.Circfill(fxF + cr,                       fyF + sr - 2,                       F32.FromInt(4), F32.FromInt(2));
        Pico8.Circfill(fxF + cr,                       fyF + sr,                           F32.FromInt(4), F32.FromInt(2));
        Pico8.Circfill(fxF + cr * F32.FromDouble(1.5), fyF + sr * F32.FromDouble(1.5) - 2, F32.FromDouble(2.5), F32.FromInt(15));
        Pico8.Circfill(fxF - cr,                       fyF - sr - 3,                       F32.FromInt(3), F32.FromInt(4));
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
