using CSharpCraft.PcraftFilter.Bias;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftFilter.Noise;
using CSharpCraft.PcraftFilter.Zones;
using CSharpCraft.PcraftSeeded.Noise;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftFilter.Bias;

public sealed class FilterBiasComputerTests
{
    // --------------------------------------------------------------------------
    #region Helpers
    // --------------------------------------------------------------------------

    // Injects a fixed constant for every cell — allows precise control over
    // which tile the base classifier produces before any bias is applied.
    private sealed class FixedGrid : SeededNoiseGrid
    {
        private readonly double _value;
        internal FixedGrid(double value)
            : base(0L, 64, 64, 999, 0.0, 0.0, 0)
        {
            _value = value;
        }

        internal override double GetValue(int x, int y)
        {
            return _value;
        }
    }

    // Builds a MapClassifier whose four noise grids all return the given constants.
    // Uses standard surface tile ids (a=0 Water, b=1 Sand, c=2 Grass, d=3 Rock, e=4 Tree)
    // on a 64×64 grid (the same size PcraftWorldSampler uses).
    private static MapClassifier MakeBaseClassifier(
        double cur, double cur2, double cur3, double cur4)
    {
        return new(new FixedGrid(cur), new FixedGrid(cur2),
            new FixedGrid(cur3), new FixedGrid(cur4),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4);
    }

    // Applies FilterBiasComputer then wraps in BiasedMapClassifier.
    private static BiasedMapClassifier Compute(
        FilterSet filters, MapClassifier baseClassifier,
        IFilterDiagnosticSink? sink = null)
    {
        BiasLayers biases = FilterBiasComputer.Compute(filters, baseClassifier, 64, 64, sink);
        return new BiasedMapClassifier(
            new FixedGrid(0.5), new FixedGrid(0.5),
            new FixedGrid(0.5), new FixedGrid(0.5),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4,
            biases);
    }

    // Counts how many cells in the 64×64 grid classify as tileId.
    private static int CountTiles(MapClassifier classifier, int tileId)
    {
        int n = 0;
        for (int x = 0; x < 64; x++)
            for (int y = 0; y < 64; y++)
                if (classifier.ClassifyTile(x, y) == tileId)
                    n++;
        return n;
    }

    // Counts tiles of tileId within a zone.
    private static int CountTilesInZone(MapClassifier classifier, Zone zone, int tileId)
    {
        int n = 0;
        foreach ((int x, int y) in zone.Cells(64, 64))
            if (classifier.ClassifyTile(x, y) == tileId)
                n++;
        return n;
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Constructor / argument validation
    // --------------------------------------------------------------------------

    [Fact]
    public void Compute_ThrowsArgumentNullException_WhenFilterSetIsNull()
    {
        MapClassifier classifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        Func<BiasLayers> act = () => FilterBiasComputer.Compute(null!, classifier, 64, 64);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("filterSet");
    }

    [Fact]
    public void Compute_ThrowsArgumentNullException_WhenClassifierIsNull()
    {
        Func<BiasLayers> act = () => FilterBiasComputer.Compute(new FilterSet([]), null!, 64, 64);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("baseClassifier");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Empty filter set
    // --------------------------------------------------------------------------

    [Fact]
    public void Compute_ReturnsEmptyBiasLayers_WhenNoFilters()
    {
        MapClassifier classifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([]), classifier, 64, 64);
        // No cell should have any bias set
        _ = biases.HasAnyBias(0, 0).Should().BeFalse();
        _ = biases.HasAnyBias(32, 32).Should().BeFalse();
        _ = biases.HasAnyBias(63, 63).Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region SpawnConstraintFilter — skipped in bias computation
    // --------------------------------------------------------------------------

    [Fact]
    public void Compute_ProducesNoBias_WhenFilterSetContainsOnlySpawnConstraint()
    {
        MapClassifier classifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        SpawnConstraintFilter filter = new([new RectangleZone(10, 10, 40, 40)]);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), classifier, 64, 64);
        _ = biases.HasAnyBias(32, 32).Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region TileCountFilter — whole-grid
    // --------------------------------------------------------------------------

    [Fact]
    public void Compute_ProducesNoBias_WhenTileCountAlreadySatisfied()
    {
        // All cells are Water (tileId=0) — a TileCountFilter asking for ≥1 Water
        // is immediately satisfied without any bias.
        MapClassifier classifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        TileCountFilter filter = new(TileId: 0, MinimumCount: 1);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), classifier, 64, 64);
        _ = biases.HasAnyBias(0, 0).Should().BeFalse();
    }

    [Fact]
    public void Compute_AddsBiases_WhenTileCountDeficitExists_ForSand()
    {
        // All cells are Water (coast≈0). Ask for 10 Sand cells (tileId=1).
        // FilterBiasComputer must bias ≥10 cells so coast > 0.3 there.
        MapClassifier baseClassifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        TileCountFilter filter = new(TileId: 1, MinimumCount: 10);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64);

        // Verify via BiasedMapClassifier that ≥10 cells now classify as Sand
        BiasedMapClassifier biased = new(
            new FixedGrid(0.5), new FixedGrid(0.5),
            new FixedGrid(0.5), new FixedGrid(0.5),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4, biases);
        _ = CountTiles(biased, tileId: 1).Should().BeGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void Compute_AddsBiases_WhenTileCountDeficitExists_ForGrass()
    {
        // All cells are Water. Ask for 5 Grass cells (tileId=2).
        MapClassifier baseClassifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        TileCountFilter filter = new(TileId: 2, MinimumCount: 5);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64);

        BiasedMapClassifier biased = new(
            new FixedGrid(0.5), new FixedGrid(0.5),
            new FixedGrid(0.5), new FixedGrid(0.5),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4, biases);
        _ = CountTiles(biased, tileId: 2).Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void Compute_AddsBiases_WhenTileCountDeficitExists_ForRock()
    {
        // All cells produce Sand (coast=0.4 > 0.3, v2=0). Ask for 5 Rock (tileId=3).
        // cur=0.6, cur2=0.5 → coast = 0.4; cur3=0.6 → v2=0; cur4=0.6 → v3=0 → all Sand
        MapClassifier baseClassifier = MakeBaseClassifier(0.6, 0.5, 0.6, 0.6);
        TileCountFilter filter = new(TileId: 3, MinimumCount: 5);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64);

        BiasedMapClassifier biased = new(
            new FixedGrid(0.6), new FixedGrid(0.5),
            new FixedGrid(0.6), new FixedGrid(0.6),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4, biases);
        _ = CountTiles(biased, tileId: 3).Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void Compute_AddsBiases_WhenTileCountDeficitExists_ForTree()
    {
        // All cells are Grass (coast=1.2, v2=0, v3=0). Ask for 5 Tree (tileId=4).
        MapClassifier baseClassifier = MakeBaseClassifier(0.8, 0.5, 0.8, 0.8);
        TileCountFilter filter = new(TileId: 4, MinimumCount: 5);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64);

        BiasedMapClassifier biased = new(
            new FixedGrid(0.8), new FixedGrid(0.5),
            new FixedGrid(0.8), new FixedGrid(0.8),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4, biases);
        _ = CountTiles(biased, tileId: 4).Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void Compute_SatisfiesExactMinimumCount_NotMore()
    {
        // All cells Water; ask for exactly 3 Sand. Should bias exactly 3 cells.
        MapClassifier baseClassifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        TileCountFilter filter = new(TileId: 1, MinimumCount: 3);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64);

        BiasedMapClassifier biased = new(
            new FixedGrid(0.5), new FixedGrid(0.5),
            new FixedGrid(0.5), new FixedGrid(0.5),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4, biases);
        _ = CountTiles(biased, tileId: 1).Should().BeGreaterThanOrEqualTo(3);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region TileCountFilter — zone-scoped
    // --------------------------------------------------------------------------

    [Fact]
    public void Compute_SatisfiesTileCount_WithinZone()
    {
        // All Water; ask for 4 Sand within a 10×10 zone.
        MapClassifier baseClassifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        RectangleZone zone = new(20, 20, 10, 10);
        TileCountFilter filter = new(TileId: 1, MinimumCount: 4, Zone: zone);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64);

        BiasedMapClassifier biased = new(
            new FixedGrid(0.5), new FixedGrid(0.5),
            new FixedGrid(0.5), new FixedGrid(0.5),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4, biases);
        _ = CountTilesInZone(biased, zone, tileId: 1).Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void Compute_DoesNotBiasCellsOutsideZone_WhenZoneScopedFilter()
    {
        // All Water; ask for 1 Sand inside zone [0,0,5,5].
        // Cells outside the zone should have no bias.
        MapClassifier baseClassifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        RectangleZone zone = new(0, 0, 5, 5);
        TileCountFilter filter = new(TileId: 1, MinimumCount: 1, Zone: zone);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64);
        _ = biases.HasAnyBias(63, 63).Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Multiple TileCountFilters — priority ordering
    // --------------------------------------------------------------------------

    [Fact]
    public void Compute_SatisfiesBothFilters_WhenTheyTargetDifferentTileIds()
    {
        // All Water; ask for 3 Sand and 3 Grass.
        MapClassifier baseClassifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        TileCountFilter f1 = new(TileId: 1, MinimumCount: 3);
        TileCountFilter f2 = new(TileId: 2, MinimumCount: 3);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([f1, f2]), baseClassifier, 64, 64);

        BiasedMapClassifier biased = new(
            new FixedGrid(0.5), new FixedGrid(0.5),
            new FixedGrid(0.5), new FixedGrid(0.5),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4, biases);
        _ = CountTiles(biased, tileId: 1).Should().BeGreaterThanOrEqualTo(3);
        _ = CountTiles(biased, tileId: 2).Should().BeGreaterThanOrEqualTo(3);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region LocalConcentrationFilter
    // --------------------------------------------------------------------------

    [Fact]
    public void Compute_ProducesCluster_WhenLocalConcentrationDeficitExists()
    {
        // All Water; ask for a cluster of 5 contiguous Sand cells inside a zone.
        MapClassifier baseClassifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        RectangleZone zone = new(10, 10, 20, 20);
        LocalConcentrationFilter filter = new(Zone: zone, TileId: 1, MinClusterSize: 5);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64);

        BiasedMapClassifier biased = new(
            new FixedGrid(0.5), new FixedGrid(0.5),
            new FixedGrid(0.5), new FixedGrid(0.5),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4, biases);

        // Verify the zone contains a 4-connected cluster of ≥5 Sand cells
        _ = LargestClusterInZone(biased, zone, tileId: 1).Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void Compute_ProducesNoBias_WhenLocalConcentrationAlreadySatisfied()
    {
        // All cells are Sand (coast=0.4); ask for cluster of 2 Sand — already satisfied.
        MapClassifier baseClassifier = MakeBaseClassifier(0.6, 0.5, 0.6, 0.6);
        RectangleZone zone = new(10, 10, 20, 20);
        LocalConcentrationFilter filter = new(Zone: zone, TileId: 1, MinClusterSize: 2);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64);
        _ = biases.HasAnyBias(15, 15).Should().BeFalse();
    }

    [Fact]
    public void Compute_ClusterIsContiguous_AfterLocalConcentrationBias()
    {
        // All Water; grow cluster of 8 Grass cells in a zone.
        MapClassifier baseClassifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        RectangleZone zone = new(5, 5, 30, 30);
        LocalConcentrationFilter filter = new(Zone: zone, TileId: 2, MinClusterSize: 8);
        BiasLayers biases = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64);

        BiasedMapClassifier biased = new(
            new FixedGrid(0.5), new FixedGrid(0.5),
            new FixedGrid(0.5), new FixedGrid(0.5),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4, biases);

        _ = LargestClusterInZone(biased, zone, tileId: 2).Should().BeGreaterThanOrEqualTo(8);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Diagnostics
    // --------------------------------------------------------------------------

    [Fact]
    public void Compute_EmitsDiagnostic_WhenTileCountCannotBeFullySatisfied()
    {
        // Zone has only 1 cell; ask for 100 Sand → impossible.
        MapClassifier baseClassifier = MakeBaseClassifier(0.5, 0.5, 0.5, 0.5);
        RectangleZone zone = new(10, 10, 1, 1);
        TileCountFilter filter = new(TileId: 1, MinimumCount: 100, Zone: zone);

        CapturingSink sink = new();
        _ = FilterBiasComputer.Compute(new FilterSet([filter]), baseClassifier, 64, 64, sink);
        _ = sink.TileCountConflicts.Should().ContainSingle();
    }

    // --------------------------------------------------------------------------
    #endregion

    // --------------------------------------------------------------------------
    #region Helpers — cluster analysis
    // --------------------------------------------------------------------------

    private static int LargestClusterInZone(MapClassifier classifier, Zone zone, int tileId)
    {
        HashSet<(int x, int y)> cells = zone.Cells(64, 64).Where(c => classifier.ClassifyTile(c.x, c.y) == tileId).ToHashSet();
        int best = 0;
        HashSet<(int, int)> visited = [];
        foreach ((int x, int y) seed in cells)
        {
            if (visited.Contains(seed)) continue;
            int size = 0;
            Queue<(int, int)> queue = new();
            queue.Enqueue(seed);
            _ = visited.Add(seed);
            while (queue.Count > 0)
            {
                (int cx, int cy) = queue.Dequeue();
                size++;
                foreach ((int nx, int ny) in Neighbours(cx, cy))
                {
                    if (cells.Contains((nx, ny)) && visited.Add((nx, ny)))
                        queue.Enqueue((nx, ny));
                }
            }
            if (size > best) best = size;
        }
        return best;
    }

    private static IEnumerable<(int, int)> Neighbours(int x, int y)
    {
        yield return (x - 1, y);
        yield return (x + 1, y);
        yield return (x, y - 1);
        yield return (x, y + 1);
    }

    // --------------------------------------------------------------------------
    #endregion
}

/// <summary>Test double that records diagnostic calls.</summary>
internal sealed class CapturingSink : IFilterDiagnosticSink
{
    internal List<(MapFilter filter, int deficit, int resolved)> TileCountConflicts { get; } = [];
    internal List<MapFilter> SpawnFallbacks { get; } = [];
    internal List<(MapFilter filter, int reached, int required)> ClusterStalls { get; } = [];

    public void OnFilterConflict(MapFilter filter, int deficit, int resolved)
    {
        TileCountConflicts.Add((filter, deficit, resolved));
    }

    public void OnSpawnFallbackApplied(SpawnConstraintFilter filter)
    {
        SpawnFallbacks.Add(filter);
    }

    public void OnClusterGrowthStalled(LocalConcentrationFilter filter, int reached, int required)
    {
        ClusterStalls.Add((filter, reached, required));
    }
}
