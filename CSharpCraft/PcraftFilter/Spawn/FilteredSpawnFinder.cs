using CSharpCraft.PcraftFilter.Bias;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftPreview.Noise;

namespace CSharpCraft.PcraftFilter.Spawn;

/// <summary>
/// Finds a valid spawn tile while respecting a <see cref="SpawnConstraintFilter"/>.
/// Delegates to <see cref="SpawnFinder"/> when no constraint is active.
/// If no spawn can be found inside the candidate area, a fallback coast bias is
/// applied to the cheapest unspawnable cells and the search is retried once.
/// </summary>
internal static class FilteredSpawnFinder
{
    private const int    MaxCandidates = 500;
    private const double Epsilon       = 0.001;

    internal static (int tileX, int tileY)? FindSpawn(
        long masterSeed,
        MapClassifier classifier,
        BiasLayers biases,
        int gridSx,
        int gridSy,
        SpawnConstraintFilter? filter,
        IFilterDiagnosticSink? sink = null)
    {
        if (classifier is null) throw new ArgumentNullException(nameof(classifier));
        if (biases is null)     throw new ArgumentNullException(nameof(biases));

        // No constraint — delegate directly.
        if (filter is null)
            return SpawnFinder.FindSpawn(masterSeed, classifier, gridSx, gridSy);

        var candidateArea = BuildCandidateArea(classifier, filter, gridSx, gridSy);
        if (candidateArea.Count == 0) return null;

        // Initial search with the caller's classifier (may be BiasedMapClassifier).
        var result = SearchCandidates(masterSeed, classifier, candidateArea, gridSx, gridSy);
        if (result.HasValue) return result;

        // Fallback: apply minimum coast bias to every unspawnable candidate cell so
        // that after the bias each cell crosses the Sand(1) threshold.
        ApplySpawnFallback(classifier, biases, candidateArea);
        sink?.OnSpawnFallbackApplied(filter);

        // Re-search using a wrapper that applies the (now mutated) biases.
        var wrapped = new WrapClassifier(classifier, biases);
        return SearchCandidates(masterSeed, wrapped, candidateArea, gridSx, gridSy);
    }

    // -------------------------------------------------------------------------
    // Candidate area construction
    // -------------------------------------------------------------------------

    private static HashSet<(int x, int y)> BuildCandidateArea(
        MapClassifier classifier,
        SpawnConstraintFilter filter,
        int gridSx,
        int gridSy)
    {
        // Start with the union of AllowedZones.
        var area = filter.AllowedZones
            .SelectMany(z => z.Cells(gridSx, gridSy))
            .ToHashSet();

        if (filter.Proximity is null) return area;

        // Intersect with cells near a qualifying cluster.
        var proxCells = FindProximityCells(classifier, filter.Proximity, gridSx, gridSy);
        area.IntersectWith(proxCells);
        return area;
    }

    private static HashSet<(int x, int y)> FindProximityCells(
        MapClassifier classifier,
        TileClusterProximity proximity,
        int gridSx,
        int gridSy)
    {
        // Collect all cells of the cluster tile type.
        var tileCells = new HashSet<(int, int)>();
        for (int x = 0; x < gridSx; x++)
            for (int y = 0; y < gridSy; y++)
                if (classifier.ClassifyTile(x, y) == proximity.ClusterTileId)
                    tileCells.Add((x, y));

        // BFS flood-fill to identify clusters; Chebyshev-expand qualifying ones.
        var visited  = new HashSet<(int, int)>();
        var expanded = new HashSet<(int x, int y)>();

        foreach (var seed in tileCells)
        {
            if (visited.Contains(seed)) continue;

            var cluster = BfsCluster(seed, tileCells, visited);
            if (cluster.Count < proximity.MinClusterSize) continue;

            foreach (var (cx, cy) in cluster)
                for (int dx = -proximity.MaxDistance; dx <= proximity.MaxDistance; dx++)
                    for (int dy = -proximity.MaxDistance; dy <= proximity.MaxDistance; dy++)
                    {
                        int nx = cx + dx, ny = cy + dy;
                        if ((uint)nx < (uint)gridSx && (uint)ny < (uint)gridSy)
                            expanded.Add((nx, ny));
                    }
        }

        return expanded;
    }

    private static HashSet<(int, int)> BfsCluster(
        (int, int) seed,
        HashSet<(int, int)> allCells,
        HashSet<(int, int)> visited)
    {
        var cluster = new HashSet<(int, int)>();
        var queue   = new Queue<(int, int)>();
        queue.Enqueue(seed);
        visited.Add(seed);

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            cluster.Add(cur);
            foreach (var n in Neighbours4(cur.Item1, cur.Item2))
                if (allCells.Contains(n) && visited.Add(n))
                    queue.Enqueue(n);
        }

        return cluster;
    }

    // -------------------------------------------------------------------------
    // Deterministic candidate search
    // -------------------------------------------------------------------------

    private static (int tileX, int tileY)? SearchCandidates(
        long masterSeed,
        MapClassifier classifier,
        HashSet<(int x, int y)> candidateArea,
        int gridSx,
        int gridSy)
    {
        int spawnHash = "spawn".GetHashCode();
        int minX = gridSx / 8, maxX = gridSx * 6 / 8;
        int minY = gridSy / 8, maxY = gridSy * 6 / 8;
        int rangeX = maxX - minX + 1;
        int rangeY = maxY - minY + 1;

        for (int k = 0; k < MaxCandidates; k++)
        {
            var rng = new Random(HashCode.Combine(masterSeed.GetHashCode(), k, spawnHash));
            int x = minX + rng.Next(rangeX);
            int y = minY + rng.Next(rangeY);

            if (!candidateArea.Contains((x, y))) continue;

            int tileId = classifier.ClassifyTile(x, y);
            if (tileId == 1 || tileId == 2) return (x, y);
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // Fallback: bias all unspawnable candidate cells to Sand(1) threshold
    // -------------------------------------------------------------------------

    private static void ApplySpawnFallback(
        MapClassifier classifier,
        BiasLayers biases,
        HashSet<(int x, int y)> candidateArea)
    {
        foreach (var (x, y) in candidateArea)
        {
            var (coast, _, _) = classifier.GetIntermediate(x, y);
            coast += biases.GetCoast(x, y);

            double dCoast = Math.Max(0.0, 0.3 + Epsilon - coast);
            if (dCoast > 0.0)
                biases.AddCoast(x, y, dCoast);
        }
    }

    // -------------------------------------------------------------------------
    // Inner helpers: WrapClassifier + NullGrid (same pattern as FilterBiasComputer)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies <see cref="BiasLayers"/> on top of any <see cref="MapClassifier"/>
    /// by reading raw intermediates from the base then adding bias offsets.
    /// </summary>
    private sealed class WrapClassifier : MapClassifier
    {
        private readonly MapClassifier _base;
        private readonly BiasLayers    _biases;

        internal WrapClassifier(MapClassifier b, BiasLayers biases)
            : base(new NullGrid(), new NullGrid(), new NullGrid(), new NullGrid(),
                   b.GridSx, b.GridSy,
                   b.TileA, b.TileB, b.TileC, b.TileD, b.TileE,
                   b.GenerateHole)
        {
            _base   = b;
            _biases = biases;
        }

        internal override int ClassifyTile(int i, int j)
        {
            if (FixedTileAt(i, j) is { } f) return f;
            var (coast, v2, v3) = _base.GetIntermediate(i, j);
            coast += _biases.GetCoast(i, j);
            v2    += _biases.GetV2(i, j);
            v3    += _biases.GetV3(i, j);
            return ClassifyFromValues(coast, v2, v3);
        }
    }

    private sealed class NullGrid : SeededNoiseGrid
    {
        internal NullGrid() : base(0L, 64, 64, 64, 0.0, 0.0, 0) { }
        internal override double GetValue(int x, int y) => 0.5;
    }

    private static IEnumerable<(int x, int y)> Neighbours4(int x, int y)
    {
        yield return (x - 1, y);
        yield return (x + 1, y);
        yield return (x, y - 1);
        yield return (x, y + 1);
    }
}
