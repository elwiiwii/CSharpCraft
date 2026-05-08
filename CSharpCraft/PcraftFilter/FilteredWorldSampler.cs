using CSharpCraft.PcraftFilter.Bias;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftFilter.Noise;
using CSharpCraft.PcraftFilter.Spawn;
using CSharpCraft.PcraftPreview;
using CSharpCraft.PcraftPreview.Noise;

namespace CSharpCraft.PcraftFilter;

/// <summary>
/// Top-level API for the filter pipeline. Mirrors <see cref="PcraftWorldSampler.Sample"/>
/// but accepts a <see cref="FilterSet"/> that shapes the generated world before sampling.
/// Returns the same <see cref="SampleResult"/> type so it is a drop-in replacement.
/// </summary>
internal static class FilteredWorldSampler
{
    private const int GridSx = 64;
    private const int GridSy = 64;

    /// <summary>
    /// Generates a (2×radius+1)² tile window that satisfies all constraints in
    /// <paramref name="filters"/>. Results are fully deterministic for a given seed
    /// and filter set.
    /// </summary>
    internal static SampleResult Sample(
        long masterSeed,
        int radius,
        FilterSet filters,
        int? forceCenterX = null,
        int? forceCenterY = null,
        IFilterDiagnosticSink? sink = null)
    {
        if (filters is null) throw new ArgumentNullException(nameof(filters));

        // --- 1. Noise layers (identical parameters to PcraftWorldSampler) ---
        var cur  = new SeededNoiseGrid(masterSeed, GridSx, GridSy, GridSx, 0.9, 0.2, 0);
        var cur2 = new SeededNoiseGrid(masterSeed, GridSx, GridSy,      8, 0.9, 0.4, 1);
        var cur3 = new SeededNoiseGrid(masterSeed, GridSx, GridSy,      8, 0.9, 0.3, 2);
        var cur4 = new SeededNoiseGrid(masterSeed, GridSx, GridSy,      4, 0.8, 1.1, 3);

        var baseClassifier = new MapClassifier(cur, cur2, cur3, cur4,
            GridSx, GridSy, a: 0, b: 1, c: 2, d: 3, e: 4);

        // --- 2. Extract SpawnConstraintFilter (if present) ---
        var spawnFilter = filters.Filters.OfType<SpawnConstraintFilter>().FirstOrDefault();

        // --- 3. Compute bias layers from TileCount + LocalConcentration filters ---
        var biases = FilterBiasComputer.Compute(filters, baseClassifier, GridSx, GridSy, sink);

        // --- 4. Build biased classifier ---
        MapClassifier classifier = biases.IsEmpty
            ? baseClassifier
            : new BiasedMapClassifier(cur, cur2, cur3, cur4,
                GridSx, GridSy, a: 0, b: 1, c: 2, d: 3, e: 4, biases);

        // --- 5. Find spawn via FilteredSpawnFinder ---
        var spawn  = FilteredSpawnFinder.FindSpawn(masterSeed, classifier, biases, GridSx, GridSy, spawnFilter, sink);
        int spawnX = spawn?.tileX ?? -1;
        int spawnY = spawn?.tileY ?? -1;

        // --- 6. Window centre ---
        int centerX = forceCenterX ?? (spawn?.tileX ?? GridSx / 2);
        int centerY = forceCenterY ?? (spawn?.tileY ?? GridSy / 2);

        // --- 7. Tile slice ---
        int side  = 2 * radius + 1;
        var tiles = new int[side, side];
        for (int i = 0; i < side; i++)
        for (int j = 0; j < side; j++)
            tiles[i, j] = classifier.ClassifyTile(centerX - radius + i, centerY - radius + j);

        // --- 8. Water animation table (identical to PcraftWorldSampler) ---
        int rndWatHash = "rndwat".GetHashCode();
        var rndWat = new double[16, 16];
        for (int i = 0; i < 16; i++)
        for (int j = 0; j < 16; j++)
            rndWat[i, j] = new Random(
                HashCode.Combine(masterSeed.GetHashCode(), i, j, rndWatHash))
                .NextDouble() * 100.0;

        return new SampleResult(tiles, spawnX, spawnY, centerX, centerY, rndWat);
    }
}
