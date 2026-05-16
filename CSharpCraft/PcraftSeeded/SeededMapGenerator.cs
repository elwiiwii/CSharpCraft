using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftSeeded.Noise;

namespace CSharpCraft.PcraftSeeded;

internal static class SeededMapGenerator
{
    internal const int IslandGridSx = 64;
    internal const int IslandGridSy = 64;
    internal const int CaveGridSx = 32;
    internal const int CaveGridSy = 32;

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

    /// <summary>
    /// Creates the four deterministic noise layers for island (64×64) map generation,
    /// matching the <c>noise(sx, sy, startscale, scalemod, featstep)</c> calls in pcraft.p8.
    /// </summary>
    internal static (SeededNoiseGrid cur, SeededNoiseGrid cur2, SeededNoiseGrid cur3, SeededNoiseGrid cur4)
        CreateIslandNoiseGrids(long seed)
    {
        return (
            new SeededNoiseGrid(seed, IslandGridSx, IslandGridSy, IslandGridSx, 0.9, 0.2, 0),
            new SeededNoiseGrid(seed, IslandGridSx, IslandGridSy, 8, 0.9, 0.4, 1),
            new SeededNoiseGrid(seed, IslandGridSx, IslandGridSy, 8, 0.9, 0.3, 2),
            new SeededNoiseGrid(seed, IslandGridSx, IslandGridSy, 4, 0.8, 1.1, 3)
        );
    }

    /// <summary>
    /// Creates the four deterministic noise layers for cave (32×32) map generation,
    /// matching the <c>noise(sx, sy, startscale, scalemod, featstep)</c> calls in pcraft.p8.
    /// </summary>
    internal static (SeededNoiseGrid cur, SeededNoiseGrid cur2, SeededNoiseGrid cur3, SeededNoiseGrid cur4)
        CreateCaveNoiseGrids(long seed)
    {
        return (
            new SeededNoiseGrid(seed, CaveGridSx, CaveGridSy, CaveGridSx, 0.9, 0.2, 0),
            new SeededNoiseGrid(seed, CaveGridSx, CaveGridSy, 8, 0.9, 0.4, 1),
            new SeededNoiseGrid(seed, CaveGridSx, CaveGridSy, 8, 0.9, 0.3, 2),
            new SeededNoiseGrid(seed, CaveGridSx, CaveGridSy, 4, 0.8, 1.1, 3)
        );
    }

    internal static (int holeX, int holeY) CreateMap(Level level, PlayerEntity player, long seed)
    {
        var (cur, cur2, cur3, cur4) = CreateIslandNoiseGrids(seed);
        MapClassifier classifier = new(cur, cur2, cur3, cur4, IslandGridSx, IslandGridSy, 0, 1, 2, 3, 4, generateHole: true);

        (int tileX, int tileY)? spawn = SpawnFinder.FindSpawn(seed, classifier, IslandGridSx, IslandGridSy);
        return CreateMap(level, player, classifier, spawn);
    }

    /// <summary>
    /// Generates a seeded 32×32 cave level. Cave tile set follows pcraft.p8:
    /// Rock (wall), Iron, Sand (floor), Gold, Gem. The ladder hole is placed at the
    /// grid centre; the player is positioned there (caves are always entered via ladder).
    /// </summary>
    internal static (int holeX, int holeY) CreateCaveMap(Level level, PlayerEntity player, long seed)
    {
        var (cur, cur2, cur3, cur4) = CreateCaveNoiseGrids(seed);
        MapClassifier classifier = new(cur, cur2, cur3, cur4,
            CaveGridSx, CaveGridSy,
            a: (int)TileId.Rock, b: (int)TileId.Iron, c: (int)TileId.Sand,
            d: (int)TileId.Gold, e: (int)TileId.Gem,
            generateHole: true);

        return CreateMap(level, player, classifier, spawn: null, ladderSurround: TileId.Sand);
    }

    /// <summary>
    /// Writes tiles to <paramref name="level"/>, positions the player at <paramref name="spawn"/>
    /// (or grid centre if null), and places the ladder hole structure.
    /// Used by the filter pipeline to inject a pre-built biased classifier.
    /// </summary>
    internal static (int holeX, int holeY) CreateMap(
        Level level,
        PlayerEntity player,
        MapClassifier classifier,
        (int tileX, int tileY)? spawn,
        TileId ladderSurround = TileId.Rock)
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
                level.SetTile(localHoleX + i, localHoleY + j, PcraftData.TileFor(ladderSurround));
        level.SetTile(localHoleX, localHoleY, PcraftData.TileFor(TileId.Hole));

        return (holeX, holeY);
    }
}
