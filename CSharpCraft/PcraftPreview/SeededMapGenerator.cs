using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftPreview.Noise;

namespace CSharpCraft.PcraftPreview;

internal static class SeededMapGenerator
{
    private const int GridSx = 64;
    private const int GridSy = 64;

    internal static F32[][] InitRndWat(long seed)
    {
        var result = new F32[16][];
        for (int i = 0; i < 16; i++)
        {
            result[i] = new F32[16];
            for (int j = 0; j < 16; j++)
                result[i][j] = F32.FromDouble(
                    new Random(HashCode.Combine(seed.GetHashCode(), i, j, "rndwat".GetHashCode()))
                        .NextDouble() * 100);
        }
        return result;
    }

    internal static (int holeX, int holeY) CreateMap(WorldState state, long seed)
    {
        int levelX  = state.LevelX;
        int levelY  = state.LevelY;
        int levelSx = state.LevelSx;
        int levelSy = state.LevelSy;

        var cur  = new SeededNoiseGrid(seed, GridSx, GridSy, GridSx, 0.9, 0.2, 0);
        var cur2 = new SeededNoiseGrid(seed, GridSx, GridSy,      8, 0.9, 0.4, 1);
        var cur3 = new SeededNoiseGrid(seed, GridSx, GridSy,      8, 0.9, 0.3, 2);
        var cur4 = new SeededNoiseGrid(seed, GridSx, GridSy,      4, 0.8, 1.1, 3);
        var classifier = new MapClassifier(cur, cur2, cur3, cur4, GridSx, GridSy, 0, 1, 2, 3, 4);

        for (int i = 0; i < levelSx; i++)
            for (int j = 0; j < levelSy; j++)
                Pico8.Mset(i + levelX, j + levelY, classifier.ClassifyTile(i, j));

        var spawn = SpawnFinder.FindSpawn(seed, classifier, GridSx, GridSy);
        int spawnX = spawn?.tileX ?? (GridSx / 2);
        int spawnY = spawn?.tileY ?? (GridSy / 2);
        state.Plx = F32.FromInt(spawnX * 16 + 8);
        state.Ply = F32.FromInt(spawnY * 16 + 8);
        state.Clx = state.Plx;
        state.Cly = state.Ply;
        state.Cmx = state.Plx;
        state.Cmy = state.Ply;

        int holeX = levelSx / 2 + levelX;
        int holeY = levelSy / 2 + levelY;
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                Pico8.Mset(holeX + i, holeY + j, 3);
        Pico8.Mset(holeX, holeY, 11);

        return (holeX, holeY);
    }
}
