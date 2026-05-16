using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftSeeded.Noise;

namespace CSharpCraft.PcraftSeeded;

internal static class SeededMapGenerator
{
    private const int GridSx = 64;
    private const int GridSy = 64;

    internal static F32[][] InitRndWat(long seed)
    {
        F32[][] result = new F32[16][];
        for (int i = 0; i < 16; i++)
        {
            result[i] = new F32[16];
            for (int j = 0; j < 16; j++)
                result[i][j] = F32.FromDouble(
                    new Random(SeedMixer.Combine(seed, i, j, 0x574F_4154))
                        .NextDouble() * 100);
        }
        return result;
    }

    internal static (int holeX, int holeY) CreateMap(Level level, PlayerEntity player, long seed)
    {
        SeededNoiseGrid cur = new(seed, GridSx, GridSy, GridSx, 0.9, 0.2, 0);
        SeededNoiseGrid cur2 = new(seed, GridSx, GridSy, 8, 0.9, 0.4, 1);
        SeededNoiseGrid cur3 = new(seed, GridSx, GridSy, 8, 0.9, 0.3, 2);
        SeededNoiseGrid cur4 = new(seed, GridSx, GridSy, 4, 0.8, 1.1, 3);
        MapClassifier classifier = new(cur, cur2, cur3, cur4, GridSx, GridSy, 0, 1, 2, 3, 4, generateHole: true);

        (int tileX, int tileY)? spawn = SpawnFinder.FindSpawn(seed, classifier, GridSx, GridSy);
        return CreateMap(level, player, classifier, spawn);
    }

    /// <summary>
    /// Writes tiles to <paramref name="level"/>, positions the player at <paramref name="spawn"/>
    /// (or grid centre if null), and places the portal hole structure.
    /// Used by the filter pipeline to inject a pre-built biased classifier.
    /// </summary>
    internal static (int holeX, int holeY) CreateMap(
        Level level,
        PlayerEntity player,
        MapClassifier classifier,
        (int tileX, int tileY)? spawn)
    {
        int levelX = level.X;
        int levelY = level.Y;
        int levelSx = level.Sx;
        int levelSy = level.Sy;

        for (int i = 0; i < levelSx; i++)
            for (int j = 0; j < levelSy; j++)
                level.SetTile(i, j, PcraftData.TileFor((TileId)classifier.ClassifyTile(i, j)));

        int spawnX = spawn?.tileX ?? (classifier.GridSx / 2);
        int spawnY = spawn?.tileY ?? (classifier.GridSy / 2);
        player.X = F32.FromInt((spawnX * 16) + 8);
        player.Y = F32.FromInt((spawnY * 16) + 8);
        player.Camera.Clx = player.X;
        player.Camera.Cly = player.Y;
        player.Camera.Cmx = player.X;
        player.Camera.Cmy = player.Y;

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
