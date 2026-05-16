using CSharpCraft.PcraftSeeded.Noise;

namespace CSharpCraft.PcraftSeeded;

/// <summary>
/// Immutable result of a <see cref="PcraftWorldSampler.Sample"/> call.
/// </summary>
/// <param name="Tiles">
/// Classified tile ids in a (2×radius+1) × (2×radius+1) grid.
/// Tiles[i, j] corresponds to map position (CenterTileX - radius + i, CenterTileY - radius + j).
/// </param>
/// <param name="SpawnTileX">X coordinate of the detected spawn tile, or -1 if none found.</param>
/// <param name="SpawnTileY">Y coordinate of the detected spawn tile, or -1 if none found.</param>
/// <param name="CenterTileX">X coordinate of the tile at the centre of the sampled window.</param>
/// <param name="CenterTileY">Y coordinate of the tile at the centre of the sampled window.</param>
/// <param name="RndWat">
/// 16×16 deterministic water-animation noise table, values in [0, 100).
/// </param>
internal sealed record SampleResult(
    int[,] Tiles,
    int SpawnTileX,
    int SpawnTileY,
    int CenterTileX,
    int CenterTileY,
    double[,] RndWat);

/// <summary>
/// Samples a partial view of the Pcraft island world around a target tile using
/// per-cell seeded diamond-square noise. No global RNG state is mutated.
/// </summary>
internal static class PcraftWorldSampler
{
    private const int GridSx = 64;
    private const int GridSy = 64;

    /// <summary>
    /// Generates a (2×radius+1)² tile window centred on the spawn tile (or a forced
    /// override). Tiles, spawn position, and water-animation table are all
    /// deterministic for a given <paramref name="masterSeed"/>.
    /// </summary>
    /// <param name="masterSeed">World seed — Unstackablely determines the entire map.</param>
    /// <param name="radius">
    /// Half-width of the sampled window. The result Tiles array is (2×radius+1)².
    /// </param>
    /// <param name="forceCenterX">
    /// Override the window centre X. Defaults to the detected spawn tile X.
    /// </param>
    /// <param name="forceCenterY">
    /// Override the window centre Y. Defaults to the detected spawn tile Y.
    /// </param>
    internal static SampleResult Sample(
        long masterSeed,
        int radius,
        int? forceCenterX = null,
        int? forceCenterY = null)
    {
        // --- Noise layers (matching MapGenerator.CreateMapStep params) ---
        SeededNoiseGrid cur = new(masterSeed, GridSx, GridSy, GridSx, 0.9, 0.2, 0);
        SeededNoiseGrid cur2 = new(masterSeed, GridSx, GridSy, 8, 0.9, 0.4, 1);
        SeededNoiseGrid cur3 = new(masterSeed, GridSx, GridSy, 8, 0.9, 0.3, 2);
        SeededNoiseGrid cur4 = new(masterSeed, GridSx, GridSy, 4, 0.8, 1.1, 3);

        MapClassifier classifier = new(cur, cur2, cur3, cur4,
            GridSx, GridSy, a: 0, b: 1, c: 2, d: 3, e: 4, generateHole: true);

        // --- Spawn detection ---
        (int tileX, int tileY)? spawn = SpawnFinder.FindSpawn(masterSeed, classifier, GridSx, GridSy);
        int spawnX = spawn?.tileX ?? -1;
        int spawnY = spawn?.tileY ?? -1;

        // --- Window centre ---
        int centerX = forceCenterX ?? spawn?.tileX ?? (GridSx / 2);
        int centerY = forceCenterY ?? spawn?.tileY ?? (GridSy / 2);

        // --- Tile slice ---
        int side = (2 * radius) + 1;
        int[,] tiles = new int[side, side];
        for (int i = 0; i < side; i++)
            for (int j = 0; j < side; j++)
                tiles[i, j] = classifier.ClassifyTile(centerX - radius + i, centerY - radius + j);

        // --- Water animation table ---
        double[,] rndWat = new double[16, 16];
        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                rndWat[i, j] = new Random(
                    SeedMixer.Combine(masterSeed, i, j, 0x574F_4154))
                    .NextDouble() * 100.0;

        return new SampleResult(tiles, spawnX, spawnY, centerX, centerY, rndWat);
    }
}
