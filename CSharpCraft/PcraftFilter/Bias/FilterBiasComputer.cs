using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftFilter.Zones;
using CSharpCraft.PcraftSeeded.Noise;

namespace CSharpCraft.PcraftFilter.Bias;

/// <summary>
/// Converts a <see cref="FilterSet"/> into a <see cref="BiasLayers"/> by analytically
/// computing the minimum per-cell bias needed to satisfy each filter in priority order.
/// <see cref="SpawnConstraintFilter"/> entries are skipped — they are handled by
/// <c>FilteredSpawnFinder</c>.
/// </summary>
internal static class FilterBiasComputer
{
    private const double Epsilon = 0.001;

    /// <summary>
    /// Computes bias layers that satisfy all bias-affecting filters in <paramref name="filterSet"/>.
    /// </summary>
    internal static BiasLayers Compute(
        FilterSet filterSet,
        MapClassifier baseClassifier,
        int gridSx,
        int gridSy,
        IFilterDiagnosticSink? sink = null)
    {
        if (filterSet is null) throw new ArgumentNullException(nameof(filterSet));
        if (baseClassifier is null) throw new ArgumentNullException(nameof(baseClassifier));

        BiasLayers biases = new();

        foreach (MapFilter filter in filterSet.Filters)
        {
            switch (filter)
            {
                case TileCountFilter tcf:
                    ApplyTileCountFilter(tcf, baseClassifier, biases, gridSx, gridSy, sink);
                    break;
                case LocalConcentrationFilter lcf:
                    ApplyLocalConcentrationFilter(lcf, baseClassifier, biases, gridSx, gridSy, sink);
                    break;
                case SpawnConstraintFilter:
                    // Handled by FilteredSpawnFinder — no bias contribution here.
                    break;
            }
        }

        return biases;
    }

    // -------------------------------------------------------------------------
    // TileCountFilter
    // -------------------------------------------------------------------------

    private static void ApplyTileCountFilter(
        TileCountFilter filter,
        MapClassifier baseClassifier,
        BiasLayers biases,
        int gridSx,
        int gridSy,
        IFilterDiagnosticSink? sink)
    {
        BiasApplier biased = new(baseClassifier, biases);
        IEnumerable<(int x, int y)> cells = filter.Zone is null
            ? AllCells(gridSx, gridSy)
            : filter.Zone.Cells(gridSx, gridSy);

        // Count how many already satisfy the requirement
        int current = cells.Count(c => biased.ClassifyTile(c.x, c.y) == filter.TileId);
        int deficit = Math.Max(0, filter.MinimumCount - current);
        if (deficit == 0) return;

        // Gather candidates: cells in zone that are NOT already committed to the
        // target tile (biased cells already matching are not candidates — they're
        // already counted above). Cells committed to a DIFFERENT tile by a prior
        // filter (HasAnyBias) are also excluded to avoid conflicts.
        List<(int x, int y, double dCoast, double dV2, double dV3, double cost)> candidates = (filter.Zone is null ? AllCells(gridSx, gridSy) : filter.Zone.Cells(gridSx, gridSy))
            .Where(c => biased.ClassifyTile(c.x, c.y) != filter.TileId
                     && !biases.HasAnyBias(c.x, c.y)
                     && !baseClassifier.IsFixedTile(c.x, c.y))
            .Select(c => ScoreCell(baseClassifier, biases, c.x, c.y, filter.TileId))
            .OrderBy(c => c.cost)
            .ToList();

        int resolved = 0;
        foreach ((int x, int y, double dCoast, double dV2, double dV3, double _) in candidates)
        {
            if (resolved >= deficit) break;
            biases.AddCoast(x, y, dCoast);
            biases.AddV2(x, y, dV2);
            biases.AddV3(x, y, dV3);
            resolved++;
        }

        if (resolved < deficit)
            sink?.OnFilterConflict(filter, deficit, resolved);
    }

    // -------------------------------------------------------------------------
    // LocalConcentrationFilter
    // -------------------------------------------------------------------------

    private static void ApplyLocalConcentrationFilter(
        LocalConcentrationFilter filter,
        MapClassifier baseClassifier,
        BiasLayers biases,
        int gridSx,
        int gridSy,
        IFilterDiagnosticSink? sink)
    {
        BiasApplier biased = new(baseClassifier, biases);

        // Find the largest existing cluster of the target tile in the zone
        HashSet<(int x, int y)> cluster = FindLargestCluster(biased, filter.Zone, filter.TileId, gridSx, gridSy);

        if (cluster.Count >= filter.MinClusterSize) return;

        // If no cells of the target type exist yet, seed with the cheapest zone cell
        if (cluster.Count == 0)
        {
            (int x, int y, double dCoast, double dV2, double dV3, double cost) seed = filter.Zone.Cells(gridSx, gridSy)
                .Where(c => !biases.HasAnyBias(c.x, c.y) && !baseClassifier.IsFixedTile(c.x, c.y))
                .Select(c => ScoreCell(baseClassifier, biases, c.x, c.y, filter.TileId))
                .OrderBy(c => c.cost)
                .FirstOrDefault();

            if (seed == default) { sink?.OnClusterGrowthStalled(filter, 0, filter.MinClusterSize); return; }

            biases.AddCoast(seed.x, seed.y, seed.dCoast);
            biases.AddV2(seed.x, seed.y, seed.dV2);
            biases.AddV3(seed.x, seed.y, seed.dV3);

            // Re-classify to rebuild the cluster
            biased = new BiasApplier(baseClassifier, biases);
            cluster = FindLargestCluster(biased, filter.Zone, filter.TileId, gridSx, gridSy);
        }

        // Grow cluster outward until it reaches the required size
        int maxIter = gridSx * gridSy;
        for (int iter = 0; iter < maxIter && cluster.Count < filter.MinClusterSize; iter++)
        {
            // Frontier: zone cells 4-adjacent to the cluster that are not yet the target tile
            List<(int x, int y, double dCoast, double dV2, double dV3, double cost)> frontier = cluster
                .SelectMany(c => Neighbours4(c.x, c.y))
                .Where(n => filter.Zone.Contains(n.x, n.y)
                            && !cluster.Contains(n)
                            && biased.ClassifyTile(n.x, n.y) != filter.TileId
                            && !biases.HasAnyBias(n.x, n.y)
                            && !baseClassifier.IsFixedTile(n.x, n.y))
                .Distinct()
                .Select(n => ScoreCell(baseClassifier, biases, n.x, n.y, filter.TileId))
                .OrderBy(n => n.cost)
                .ToList();

            if (frontier.Count == 0)
            {
                sink?.OnClusterGrowthStalled(filter, cluster.Count, filter.MinClusterSize);
                return;
            }

            (int x, int y, double dCoast, double dV2, double dV3, double cost) best = frontier[0];
            biases.AddCoast(best.x, best.y, best.dCoast);
            biases.AddV2(best.x, best.y, best.dV2);
            biases.AddV3(best.x, best.y, best.dV3);

            // Recalculate classifier and cluster after each step
            biased = new BiasApplier(baseClassifier, biases);
            cluster = FindLargestCluster(biased, filter.Zone, filter.TileId, gridSx, gridSy);
        }

        if (cluster.Count < filter.MinClusterSize)
            sink?.OnClusterGrowthStalled(filter, cluster.Count, filter.MinClusterSize);
    }

    // -------------------------------------------------------------------------
    // Cost function
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the (dCoast, dV2, dV3) additive offsets needed to guarantee that
    /// (coast + dCoast, v2 + dV2, v3 + dV3) classifies as <paramref name="tileId"/>.
    /// ε margin ensures the relevant threshold is cleanly crossed.
    /// </summary>
    internal static (double dCoast, double dV2, double dV3) CostToConvert(
        int tileId, double coast, double v2, double v3)
    {
        return tileId switch
        {
            // Sand (b=1): coast ∈ (0.3, 0.6], v2 ≤ 0.5
            1 => (
                dCoast: Math.Max(0, 0.3 + Epsilon - coast) - Math.Max(0, coast - 0.6 + Epsilon),
                dV2: -Math.Max(0, v2 - 0.5 + Epsilon),
                dV3: 0),

            // Grass (c=2): coast > 0.6, v2 ≤ 0.5, v3 ≤ 0.5
            2 => (
                dCoast: Math.Max(0, 0.6 + Epsilon - coast),
                dV2: -Math.Max(0, v2 - 0.5 + Epsilon),
                dV3: -Math.Max(0, v3 - 0.5 + Epsilon)),

            // Rock (d=3): coast > 0.3, v2 > 0.5
            3 => (
                dCoast: Math.Max(0, 0.3 + Epsilon - coast),
                dV2: Math.Max(0, 0.5 + Epsilon - v2),
                dV3: 0),

            // Tree (e=4): coast > 0.6, v2 ≤ 0.5, v3 > 0.5
            4 => (
                dCoast: Math.Max(0, 0.6 + Epsilon - coast),
                dV2: -Math.Max(0, v2 - 0.5 + Epsilon),
                dV3: Math.Max(0, 0.5 + Epsilon - v3)),

            // Water (a=0) or any other: no bias needed
            _ => (0, 0, 0)
        };
    }

    // -------------------------------------------------------------------------
    // Flood-fill helpers
    // -------------------------------------------------------------------------

    private static HashSet<(int x, int y)> FindLargestCluster(
        MapClassifier classifier,
        Zone zone,
        int tileId,
        int gridSx,
        int gridSy)
    {
        HashSet<(int x, int y)> cells = zone.Cells(gridSx, gridSy)
            .Where(c => classifier.ClassifyTile(c.x, c.y) == tileId)
            .ToHashSet();

        HashSet<(int x, int y)> best = [];
        HashSet<(int, int)> visited = [];

        foreach ((int x, int y) seed in cells)
        {
            if (visited.Contains(seed)) continue;
            HashSet<(int x, int y)> cluster = [];
            Queue<(int x, int y)> queue = new();
            queue.Enqueue(seed);
            _ = visited.Add(seed);
            while (queue.Count > 0)
            {
                (int x, int y) cur = queue.Dequeue();
                _ = cluster.Add(cur);
                foreach ((int x, int y) n in Neighbours4(cur.x, cur.y))
                {
                    if (cells.Contains(n) && visited.Add(n))
                        queue.Enqueue(n);
                }
            }
            if (cluster.Count > best.Count) best = cluster;
        }
        return best;
    }

    private static IEnumerable<(int x, int y)> Neighbours4(int x, int y)
    {
        yield return (x - 1, y);
        yield return (x + 1, y);
        yield return (x, y - 1);
        yield return (x, y + 1);
    }

    private static IEnumerable<(int x, int y)> AllCells(int gridSx, int gridSy)
    {
        for (int x = 0; x < gridSx; x++)
            for (int y = 0; y < gridSy; y++)
                yield return (x, y);
    }

    // -------------------------------------------------------------------------
    // -------------------------------------------------------------------------

    private static (int x, int y, double dCoast, double dV2, double dV3, double cost) ScoreCell(
        MapClassifier baseClassifier, BiasLayers biases, int x, int y, int tileId)
    {
        (double coast, double v2, double v3) = baseClassifier.GetIntermediate(x, y);
        coast += biases.GetCoast(x, y);
        v2 += biases.GetV2(x, y);
        v3 += biases.GetV3(x, y);
        (double dCoast, double dV2, double dV3) = CostToConvert(tileId, coast, v2, v3);
        double cost = Math.Abs(dCoast) + Math.Abs(dV2) + Math.Abs(dV3);
        return (x, y, dCoast, dV2, dV3, cost);
    }

    // Wraps a MapClassifier + BiasLayers into a single ClassifyTile call
    // without needing to duplicate or copy the underlying noise grids.
    private sealed class BiasApplier : MapClassifier
    {
        private readonly MapClassifier _base;
        private readonly BiasLayers _biases;

        internal BiasApplier(MapClassifier baseClassifier, BiasLayers biases)
            : base(new NullGrid(), new NullGrid(), new NullGrid(), new NullGrid(),
                   baseClassifier.GridSx, baseClassifier.GridSy,
                   baseClassifier.TileA, baseClassifier.TileB, baseClassifier.TileC,
                   baseClassifier.TileD, baseClassifier.TileE,
                   baseClassifier.GenerateHole)
        {
            _base = baseClassifier;
            _biases = biases;
        }

        internal override int ClassifyTile(int i, int j)
        {
            if (FixedTileAt(i, j) is { } f) return f;
            (double coast, double v2, double v3) = _base.GetIntermediate(i, j);
            coast += _biases.GetCoast(i, j);
            v2 += _biases.GetV2(i, j);
            v3 += _biases.GetV3(i, j);
            return ClassifyFromValues(coast, v2, v3);
        }
    }

    private sealed class NullGrid : SeededNoiseGrid
    {
        internal NullGrid() : base(0L, 64, 64, 64, 0.0, 0.0, 0) { }
        internal override double GetValue(int x, int y)
        {
            return 0.5;
        }
    }
}
