using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
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

    internal static (int holeX, int holeY) CreateMap(Level level, PlayerEntity player, long seed)
    {
        int levelX  = level.X;
        int levelY  = level.Y;
        int levelSx = level.Sx;
        int levelSy = level.Sy;

        var cur  = new SeededNoiseGrid(seed, GridSx, GridSy, GridSx, 0.9, 0.2, 0);
        var cur2 = new SeededNoiseGrid(seed, GridSx, GridSy,      8, 0.9, 0.4, 1);
        var cur3 = new SeededNoiseGrid(seed, GridSx, GridSy,      8, 0.9, 0.3, 2);
        var cur4 = new SeededNoiseGrid(seed, GridSx, GridSy,      4, 0.8, 1.1, 3);
        var classifier = new MapClassifier(cur, cur2, cur3, cur4, GridSx, GridSy, 0, 1, 2, 3, 4);

        for (int i = 0; i < levelSx; i++)
            for (int j = 0; j < levelSy; j++)
                level.SetTile(i, j, PcraftData.TileFor((TileId)classifier.ClassifyTile(i, j)));

        var spawn = SpawnFinder.FindSpawn(seed, classifier, GridSx, GridSy);
        int spawnX = spawn?.tileX ?? (GridSx / 2);
        int spawnY = spawn?.tileY ?? (GridSy / 2);
        player.X    = F32.FromInt(spawnX * 16 + 8);
        player.Y    = F32.FromInt(spawnY * 16 + 8);
        player.Camera.Clx  = player.X;
        player.Camera.Cly  = player.Y;
        player.Camera.Cmx  = player.X;
        player.Camera.Cmy  = player.Y;

        int localHoleX = levelSx / 2;
        int localHoleY = levelSy / 2;
        int holeX = localHoleX + levelX;
        int holeY = localHoleY + levelY;
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                level.SetTile(localHoleX + i, localHoleY + j, PcraftData.TileFor(TileId.Rock));
        level.SetTile(localHoleX, localHoleY, PcraftData.TileFor(TileId.Hole));

        return (holeX, holeY);
    }
}
