using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftBase.Draw;

internal static class BackDrawer
{
    internal static int RenderGr(Tile tile)
        => (tile.Surface is not null and not OverlaySurface) ? tile.Surface.Gr : tile.Floor.Gr;

    internal static void DrawBack(Level level, PlayerEntity player)
    {
        var camera = player.Camera;
        int ci = F32.FloorToInt((camera.Clx - 64) / F32.FromInt(16));
        int cj = F32.FloorToInt((camera.Cly - 64) / F32.FromInt(16));

        // Pass 1 — write tile indices into the map work area
        for (int i = ci; i <= ci + 8; i++)
        {
            for (int j = cj; j <= cj + 8; j++)
            {
                var tile = PcraftServices.GetDirectTile(i, j, level);
                int renderGr = RenderGr(tile);
                int gi = (i - ci) * 2 + 64;
                int gj = (j - cj) * 2 + 32;

                if (renderGr == 1)
                {
                    int sv = (tile.Floor == PcraftData.FtFarm || tile.Floor == PcraftData.FtWheat) ? 3 : 0;
                    Pico8.Mset(gi,     gj,     PcraftServices.RndSand(i,       j,       level) + sv);
                    Pico8.Mset(gi + 1, gj,     PcraftServices.RndSand(i + 0.5, j,       level) + sv);
                    Pico8.Mset(gi,     gj + 1, PcraftServices.RndSand(i,       j + 0.5, level) + sv);
                    Pico8.Mset(gi + 1, gj + 1, PcraftServices.RndSand(i + 0.5, j + 0.5, level) + sv);
                }
                else
                {
                    bool u = PcraftServices.Comp(i,     j - 1, renderGr, level);
                    bool d = PcraftServices.Comp(i,     j + 1, renderGr, level);
                    bool l = PcraftServices.Comp(i - 1, j,     renderGr, level);
                    bool r = PcraftServices.Comp(i + 1, j,     renderGr, level);

                    int b = (tile.Surface == PcraftData.StRock) ? 21
                          : (tile.Floor   == PcraftData.FtWater) ? 26
                          : 16;

                    int tl = PcraftServices.CornerOffset(l, u, PcraftServices.Comp(i - 1, j - 1, renderGr, level), PcraftServices.RndCenter(i,       j,       level), innerCorner: 20, hOnly:  1, vOnly: 16, outer:  0);
                    int tr = PcraftServices.CornerOffset(r, u, PcraftServices.Comp(i + 1, j - 1, renderGr, level), PcraftServices.RndCenter(i + 0.5, j,       level), innerCorner: 19, hOnly:  1, vOnly: 18, outer:  2);
                    int bl = PcraftServices.CornerOffset(l, d, PcraftServices.Comp(i - 1, j + 1, renderGr, level), PcraftServices.RndCenter(i,       j + 0.5, level), innerCorner:  4, hOnly: 33, vOnly: 16, outer: 32);
                    int br = PcraftServices.CornerOffset(r, d, PcraftServices.Comp(i + 1, j + 1, renderGr, level), PcraftServices.RndCenter(i + 0.5, j + 0.5, level), innerCorner:  3, hOnly: 33, vOnly: 18, outer: 34);

                    Pico8.Mset(gi,     gj,     b + tl);
                    Pico8.Mset(gi + 1, gj,     b + tr);
                    Pico8.Mset(gi,     gj + 1, b + bl);
                    Pico8.Mset(gi + 1, gj + 1, b + br);
                }
            }
        }

        Pico8.Pal();
        if (level.Theme == LevelTheme.Cave)
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
                var tile = PcraftServices.GetDirectTile(i, j, level);
                int gi = i * 16;
                int gj = j * 16;

                Pico8.Pal();

                if (tile.Floor == PcraftData.FtWater)
                {
                    PcraftServices.WatAnim(i,       j,       level);
                    PcraftServices.WatAnim(i + 0.5, j,       level);
                    PcraftServices.WatAnim(i,       j + 0.5, level);
                    PcraftServices.WatAnim(i + 0.5, j + 0.5, level);
                }

                if (tile.Floor == PcraftData.FtWheat)
                {
                    F32 dd = tile.GrowthTimer.GetValueOrDefault(F32.Zero) - level.Time;
                    for (int pp = 2; pp <= 4; pp++)
                    {
                        Pico8.Pal(pp, 3);
                        if (dd > F32.FromInt(10 - pp * 2))
                            Pico8.Palt(pp, true);
                    }
                    if (dd < F32.Zero) Pico8.Pal(4, 9);
                    PcraftServices.Spr4(i, j, gi, gj, 6, 6, 6, 6, 0, (x, y) => PcraftServices.RndSand(x, y, level));
                }

                if (tile.Surface is OverlaySurface os)
                {
                    PcraftServices.SetPal(os.Pal);
                    PcraftServices.Spr4(i, j, gi, gj, 64, 65, 80, 81, 0, (x, y) => PcraftServices.RndTree(x, y, level));
                }

                if (tile.Floor == PcraftData.FtHole)
                {
                    Pico8.Pal();
                    if (level.Theme == LevelTheme.Surface)
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

    internal static bool Comp(int i, int j, int renderGr, Level level)
    {
        var tile2 = PcraftServices.GetDirectTile(i, j, level);
        return renderGr == RenderGr(tile2);
    }

    internal static int CornerOffset(
        bool sideH, bool sideV, bool diag, int rnd,
        int innerCorner, int hOnly, int vOnly, int outer)
    {
        if (sideH && sideV) return diag ? 17 + rnd : innerCorner;
        if (sideH)          return hOnly;
        if (sideV)          return vOnly;
        return outer;
    }

    internal static F32 WatVal(double i, double j, Level level)
    {
        int xi = (int)(i * 2 % 16);
        int yj = (int)(j * 2 % 16);
        if (xi < 0) xi += 16;
        if (yj < 0) yj += 16;
        return level.RndWat[xi][yj];
    }

    internal static void WatAnim(double i, double j, Level level)
    {
        F32 a = ((level.Time * F32.FromFloat(0.6f) + PcraftServices.WatVal(i, j, level) / F32.FromInt(100)) % F32.One) * F32.FromInt(19);
        if (a > F32.FromInt(16))
            Pico8.Spr(F32.FloorToInt(F32.FromInt(13) + a - F32.FromInt(16)), i * 16, j * 16);
    }

    internal static int RndCenter(double i, double j, Level level)
        => (F32.FloorToInt(PcraftServices.WatVal(i, j, level) / F32.FromInt(34)) + 18) % 20;

    internal static int RndSand(double i, double j, Level level)
        => F32.FloorToInt(PcraftServices.WatVal(i, j, level) / F32.FromInt(34)) + 1;

    internal static int RndTree(double i, double j, Level level)
        => F32.FloorToInt(PcraftServices.WatVal(i, j, level) / F32.FromInt(51)) * 32;

    internal static void Spr4(double i, double j, int gi, int gj, int a, int b, int c, int d, int off, Func<double, double, int> f)
    {
        Pico8.Spr(f(i,       j + off)       + a, gi,     gj + 2 * off);
        Pico8.Spr(f(i + 0.5, j + off)       + b, gi + 8, gj + 2 * off);
        Pico8.Spr(f(i,       j + 0.5 + off) + c, gi,     gj + 8 + 2 * off);
        Pico8.Spr(f(i + 0.5, j + 0.5 + off) + d, gi + 8, gj + 8 + 2 * off);
    }

}

