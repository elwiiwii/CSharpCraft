using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Map;

namespace CSharpCraft.Pcraft.Draw;

internal static class BackDrawer
{
    internal static void DrawBack(WorldState state)
    {
        int ci = F32.FloorToInt((state.Clx - F32.FromInt(64)) / F32.FromInt(16));
        int cj = F32.FloorToInt((state.Cly - F32.FromInt(64)) / F32.FromInt(16));

        // Pass 1 — write tile indices into the map work area
        for (int i = ci; i <= ci + 8; i++)
        {
            for (int j = cj; j <= cj + 8; j++)
            {
                var gr = MapOps.GetDirectGr(i, j, state);
                int gi = (i - ci) * 2 + 64;
                int gj = (j - cj) * 2 + 32;

                if (gr.Gr == 1) // sand (GrSand, GrFarm, GrWheat, GrPlant all have Gr==1)
                {
                    int sv = (gr == PcraftData.GrFarm || gr == PcraftData.GrWheat) ? 3 : 0;
                    Pico8.Mset(gi,     gj,     RndSand(i,       j,       state) + sv);
                    Pico8.Mset(gi + 1, gj,     RndSand(i + 0.5, j,       state) + sv);
                    Pico8.Mset(gi,     gj + 1, RndSand(i,       j + 0.5, state) + sv);
                    Pico8.Mset(gi + 1, gj + 1, RndSand(i + 0.5, j + 0.5, state) + sv);
                }
                else
                {
                    bool u = Comp(i,     j - 1, gr, state);
                    bool d = Comp(i,     j + 1, gr, state);
                    bool l = Comp(i - 1, j,     gr, state);
                    bool r = Comp(i + 1, j,     gr, state);

                    int b = gr == PcraftData.GrRock  ? 21
                          : gr == PcraftData.GrWater ? 26
                          : 16;

                    Pico8.Mset(gi,     gj,     b + (l ? (u ? (Comp(i-1,j-1,gr,state) ? 17+RndCenter(i,      j,      state) : 20) : 1) : (u ? 16 : 0)));
                    Pico8.Mset(gi + 1, gj,     b + (r ? (u ? (Comp(i+1,j-1,gr,state) ? 17+RndCenter(i+0.5,  j,      state) : 19) : 1) : (u ? 18 : 2)));
                    Pico8.Mset(gi,     gj + 1, b + (l ? (d ? (Comp(i-1,j+1,gr,state) ? 17+RndCenter(i,      j+0.5,  state) :  4) : 33) : (d ? 16 : 32)));
                    Pico8.Mset(gi + 1, gj + 1, b + (r ? (d ? (Comp(i+1,j+1,gr,state) ? 17+RndCenter(i+0.5,  j+0.5,  state) :  3) : 33) : (d ? 18 : 34)));
                }
            }
        }

        Pico8.Pal();
        if (state.LevelUnder)
        {
            Pico8.Pal(15, 5);
            Pico8.Pal(4,  1);
        }
        Pico8.Map(64, 32, ci * 16, cj * 16, 18, 18);

        // Pass 2 — overlay water / wheat / trees / holes
        for (int i = ci - 1; i <= ci + 8; i++)
        {
            for (int j = cj - 1; j <= cj + 8; j++)
            {
                var gr = MapOps.GetDirectGr(i, j, state);
                int gi = i * 16;
                int gj = j * 16;

                Pico8.Pal();

                if (gr == PcraftData.GrWater)
                {
                    WatAnim(i,       j,       state);
                    WatAnim(i + 0.5, j,       state);
                    WatAnim(i,       j + 0.5, state);
                    WatAnim(i + 0.5, j + 0.5, state);
                }

                if (gr == PcraftData.GrWheat)
                {
                    F32 dd = MapOps.DirGetData(i, j, F32.Zero, state) - state.Time;
                    for (int pp = 2; pp <= 4; pp++)
                    {
                        Pico8.Pal(pp, 3);
                        if (dd > F32.FromInt(10 - pp * 2))
                            Pico8.Palt(pp, true);
                    }
                    if (dd < F32.Zero) Pico8.Pal(4, 9);
                    Spr4(i, j, gi, gj, 6, 6, 6, 6, 0, (x, y) => RndSand(x, y, state));
                }

                if (gr.IsTree)
                {
                    if (gr.Pal != null) DrawHelpers.SetPal(gr.Pal);
                    Spr4(i, j, gi, gj, 64, 65, 80, 81, 0, (x, y) => RndTree(x, y, state));
                    // Lua: if mget(i+levelx,j+1)=='c' → string/int comparison always false in Lua
                    // retained as dead-code guard (99 = 'c'); never triggers in practice
                    if (Pico8.Mget(i + state.LevelX, j + 1) == 99)
                        Spr4(i, j, gi, gj, 64, 65, 80, 81, 4, (x, y) => RndTree(x, y, state));
                }

                if (gr == PcraftData.GrHole)
                {
                    Pico8.Pal();
                    if (!state.LevelUnder)
                    {
                        Pico8.Palt(0, false);
                        Pico8.Spr(31, gi,     gj, 1, 2);
                        Pico8.Spr(31, gi + 8, gj, 1, 2, true, false);
                    }
                    Pico8.Palt();
                    Pico8.Spr(77, gi + 4, gj, 1, 2);
                }
            }
        }
    }

    private static bool Comp(int i, int j, GroundType gr, WorldState state)
    {
        var gr2 = MapOps.GetDirectGr(i, j, state);
        return gr.Gr == gr2.Gr;
    }

    private static F32 WatVal(double i, double j, WorldState state)
    {
        int xi = (int)(i * 2 % 16);
        int yj = (int)(j * 2 % 16);
        if (xi < 0) xi += 16;
        if (yj < 0) yj += 16;
        return state.RndWat[xi][yj];
    }

    private static void WatAnim(double i, double j, WorldState state)
    {
        F32 a = ((state.Time * F32.FromFloat(0.6f) + WatVal(i, j, state) / F32.FromInt(100)) % F32.One) * F32.FromInt(19);
        if (a > F32.FromInt(16))
            Pico8.Spr(F32.FloorToInt(F32.FromInt(13) + a - F32.FromInt(16)), i * 16, j * 16);
    }

    private static int RndCenter(double i, double j, WorldState state)
        => (F32.FloorToInt(WatVal(i, j, state) / F32.FromInt(34)) + 18) % 20;

    private static int RndSand(double i, double j, WorldState state)
        => F32.FloorToInt(WatVal(i, j, state) / F32.FromInt(34)) + 1;

    private static int RndTree(double i, double j, WorldState state)
        => F32.FloorToInt(WatVal(i, j, state) / F32.FromInt(51)) * 32;

    private static void Spr4(double i, double j, int gi, int gj, int a, int b, int c, int d, int off, Func<double, double, int> f)
    {
        Pico8.Spr(f(i,       j + off)       + a, gi,     gj + 2 * off);
        Pico8.Spr(f(i + 0.5, j + off)       + b, gi + 8, gj + 2 * off);
        Pico8.Spr(f(i,       j + 0.5 + off) + c, gi,     gj + 8 + 2 * off);
        Pico8.Spr(f(i + 0.5, j + 0.5 + off) + d, gi + 8, gj + 8 + 2 * off);
    }

}
