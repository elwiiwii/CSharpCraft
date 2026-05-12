using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftDeluxe.Map;

internal static class MapGenerator
{
    private static readonly F32 Half = F32.FromDouble(0.5);

    internal static TileId[,] CreateMapStep(int sx, int sy, TileId a, TileId b, TileId c, TileId d, TileId e)
    {
        F32[,] cur = DeluxeServices.Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.2), sx);
        F32[,] cur2 = DeluxeServices.Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.4), 8);
        F32[,] cur3 = DeluxeServices.Noise(sx, sy, F32.FromDouble(0.9), F32.FromDouble(0.3), 8);
        F32[,] cur4 = DeluxeServices.Noise(sx, sy, F32.FromDouble(0.8), F32.FromDouble(1.1), 4);

        TileId[,] result = new TileId[sx + 1, sy + 1];

        for (int i = 0; i <= sx; i++)
        {
            for (int j = 0; j <= sy; j++)
            {
                F32 v = F32.Abs(cur[i, j] - cur2[i, j]);
                F32 v2 = F32.Abs(cur[i, j] - cur3[i, j]);
                F32 v3 = F32.Abs(cur[i, j] - cur4[i, j]);

                F32 di = F32.Abs(F32.FromDouble((double)i / sx) - Half) * 2;
                F32 dj = F32.Abs(F32.FromDouble((double)j / sy) - Half) * 2;
                F32 dist = F32.Max(di, dj);
                dist = dist * dist * dist * dist;

                var coast = (v * F32.FromInt(4)) - (dist * F32.FromInt(4));

                TileId id = a == TileId.Rock
                    ? (coast < F32.FromDouble(-1.3) ? 0 : a)
                    : a;
                if (coast > F32.FromDouble(0.3)) id = b;
                if (coast > F32.FromDouble(0.6)) id = c;
                if (coast > F32.FromDouble(0.3) && v2 > F32.FromDouble(0.5)) id = d;
                if (id == c && v3 > F32.FromDouble(0.5)) id = e;

                result[i, j] = id;
            }
        }

        return result;
    }

}
