using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Map;

internal static class MapGenerator
{
    private static readonly F32 Half = F32.FromDouble(0.5);

    /// <summary>
    /// Diamond-square noise. Returns (sx+1) × (sy+1) array with all values initialised to 0.5.
    /// sx/sy must be powers of 2.
    /// </summary>
    internal static F32[,] Noise(int sx, int sy, F32 startScale, F32 scaleMod, int featStep)
    {
        var n = new F32[sx + 1, sy + 1];
        for (int i = 0; i <= sx; i++)
            for (int j = 0; j <= sy; j++)
                n[i, j] = Half;

        int step = sx;
        F32 scale = startScale;

        while (step > 1)
        {
            F32 cscal = (step == featStep) ? F32.One : scale;

            // Edge midpoints
            for (int i = 0; i < sx; i += step)
            {
                for (int j = 0; j < sy; j += step)
                {
                    var c1 = n[i, j];
                    var c2 = n[i + step, j];
                    var c3 = n[i, j + step];
                    n[i + step / 2, j]       = (c1 + c2) * Half + (Pico8.Rnd(1) - Half) * cscal;
                    n[i, j + step / 2]       = (c1 + c3) * Half + (Pico8.Rnd(1) - Half) * cscal;
                }
            }

            // Center midpoints
            for (int i = 0; i < sx; i += step)
            {
                for (int j = 0; j < sy; j += step)
                {
                    var c1 = n[i, j];
                    var c2 = n[i + step, j];
                    var c3 = n[i, j + step];
                    var c4 = n[i + step, j + step];
                    n[i + step / 2, j + step / 2] = (c1 + c2 + c3 + c4) * F32.FromDouble(0.25) + (Pico8.Rnd(1) - Half) * cscal;
                }
            }

            step /= 2;
            scale *= scaleMod;
        }

        return n;
    }

    internal static int[,] CreateMapStep(int sx, int sy, int a, int b, int c, int d, int e)
    {
        var cur  = Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.2), sx);
        var cur2 = Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.4), 8);
        var cur3 = Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.3), 8);
        var cur4 = Noise(sx, sy, F32.FromDouble(0.8), F32.FromDouble(1.1), 4);

        var result = new int[sx + 1, sy + 1];

        for (int i = 0; i <= sx; i++)
        {
            for (int j = 0; j <= sy; j++)
            {
                var v  = F32.Abs(cur[i, j] - cur2[i, j]);
                var v2 = F32.Abs(cur[i, j] - cur3[i, j]);
                var v3 = F32.Abs(cur[i, j] - cur4[i, j]);

                F32 di    = F32.Abs(F32.FromDouble((double)i / sx) - Half) * 2;
                F32 dj    = F32.Abs(F32.FromDouble((double)j / sy) - Half) * 2;
                F32 dist  = F32.Max(di, dj);
                dist = dist * dist * dist * dist;

                var coast = v * F32.FromInt(4) - dist * F32.FromInt(4);

                int id = a;
                if (coast > F32.FromDouble(0.3)) id = b;
                if (coast > F32.FromDouble(0.6)) id = c;
                if (coast > F32.FromDouble(0.3) && v2 > F32.FromDouble(0.5)) id = d;
                if (id == c && v3 > F32.FromDouble(0.5)) id = e;

                result[i, j] = id;
            }
        }

        return result;
    }

    internal static F32[][] InitRndWat()
    {
        var result = new F32[16][];
        for (int i = 0; i < 16; i++)
        {
            result[i] = new F32[16];
            for (int j = 0; j < 16; j++)
                result[i][j] = Pico8.Rnd(100);
        }
        return result;
    }

    internal static (int holeX, int holeY) CreateMap(Level level, PlayerEntity player)
    {
        int levelSx = level.Sx;
        int levelSy = level.Sy;
        int levelX  = level.X;
        int levelY  = level.Y;
        bool isUnder = level.IsUnder;

        var tiles = new int[levelSx + 1, levelSy + 1];
        bool needMap = true;

        while (needMap)
        {
            needMap = false;

            var typecount = new int[12];

            if (isUnder)
            {
                tiles = CreateMapStep(levelSx, levelSy, 3, 8, 1, 9, 10);
                CountTypes(tiles, levelSx, levelSy, typecount);
                if (typecount[8]  < 30) needMap = true;
                if (typecount[9]  < 20) needMap = true;
                if (typecount[10] < 15) needMap = true;
            }
            else
            {
                tiles = CreateMapStep(levelSx, levelSy, 0, 1, 2, 3, 4);
                CountTypes(tiles, levelSx, levelSy, typecount);
                if (typecount[3] < 30) needMap = true;
                if (typecount[4] < 30) needMap = true;
            }

            if (!needMap)
            {
                int plxTile = -1, plyTile = -1;
                for (int attempt = 0; attempt <= 500; attempt++)
                {
                    int depx = F32.FloorToInt(F32.FromDouble(levelSx / 8.0) + Pico8.Rnd(levelSx * 6.0 / 8.0));
                    int depy = F32.FloorToInt(F32.FromDouble(levelSy / 8.0) + Pico8.Rnd(levelSy * 6.0 / 8.0));
                    if (depx >= 0 && depx <= levelSx && depy >= 0 && depy <= levelSy)
                    {
                        int tileId = tiles[depx, depy];
                        if (tileId == 1 || tileId == 2)
                        {
                            plxTile = depx;
                            plyTile = depy;
                            break;
                        }
                    }
                }
                if (plxTile < 0)
                    needMap = true;
                else
                {
                    player.X = F32.FromInt(plxTile * 16 + 8);
                    player.Y = F32.FromInt(plyTile * 16 + 8);
                }
            }
        }

        for (int i = 0; i < levelSx; i++)
            for (int j = 0; j < levelSy; j++)
                Pico8.Mset(i + levelX, j + levelY, tiles[i, j]);

        int holeX = levelSx / 2 + levelX;
        int holeY = levelSy / 2 + levelY;
        int surroundId = isUnder ? 1 : 3;
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                Pico8.Mset(holeX + i, holeY + j, surroundId);
        Pico8.Mset(holeX, holeY, 11);

        player.Camera.Clx = player.X;
        player.Camera.Cly = player.Y;
        player.Camera.Cmx = player.X;
        player.Camera.Cmy = player.Y;

        return (holeX, holeY);
    }

    internal static void CountTypes(int[,] tiles, int sx, int sy, int[] typecount)
    {
        for (int i = 0; i <= sx; i++)
            for (int j = 0; j <= sy; j++)
                typecount[tiles[i, j]]++;
    }
}
