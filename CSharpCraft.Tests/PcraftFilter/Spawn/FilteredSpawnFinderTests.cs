using CSharpCraft.PcraftFilter.Bias;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftFilter.Spawn;
using CSharpCraft.PcraftFilter.Zones;
using CSharpCraft.PcraftPreview.Noise;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftFilter.Spawn;

public sealed class FilteredSpawnFinderTests
{
    // --------------------------------------------------------------------------
    #region Helpers
    // --------------------------------------------------------------------------

    // Injects a fixed constant for every cell — fully deterministic grid.
    private sealed class FixedGrid : SeededNoiseGrid
    {
        private readonly double _value;
        internal FixedGrid(double value)
            : base(0L, 64, 64, 999, 0.0, 0.0, 0) => _value = value;
        internal override double GetValue(int x, int y) => _value;
    }

    // All tiles classify as Grass (id=2): cur=1, cur2=0 → coast=4-dist*4 >> 0.6;
    // v2=0, v3=0 → no rock/tree override. Grass(2) is a valid spawn tile.
    private static MapClassifier AllGrass() =>
        new(new FixedGrid(1.0), new FixedGrid(0.0),
            new FixedGrid(1.0), new FixedGrid(1.0),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4);

    // All tiles classify as Water (id=0): cur=cur2=0.5 → coast=-dist*4 ≤ 0 < 0.3 → Water.
    // Not a valid spawn tile.
    private static MapClassifier AllWater() =>
        new(new FixedGrid(0.5), new FixedGrid(0.5),
            new FixedGrid(0.5), new FixedGrid(0.5),
            64, 64, a: 0, b: 1, c: 2, d: 3, e: 4);

    // A classifier that returns Rock(3) for cells inside rockPatch, Grass(2) everywhere else.
    // Used for proximity constraint tests.
    private sealed class PatchClassifier : MapClassifier
    {
        private readonly RectangleZone _rockPatch;

        internal PatchClassifier(RectangleZone rockPatch)
            : base(new FixedGrid(0.5), new FixedGrid(0.5),
                   new FixedGrid(0.5), new FixedGrid(0.5),
                   64, 64, a: 0, b: 1, c: 2, d: 3, e: 4)
        {
            _rockPatch = rockPatch;
        }

        internal override int ClassifyTile(int i, int j) =>
            _rockPatch.Contains(i, j) ? 3 : 2;  // 3=Rock, 2=Grass
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Constructor argument validation
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_ThrowsArgumentNullException_WhenClassifierIsNull()
    {
        var act = () => FilteredSpawnFinder.FindSpawn(0L, null!, new BiasLayers(), 64, 64, null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("classifier");
    }

    [Fact]
    public void FindSpawn_ThrowsArgumentNullException_WhenBiasesIsNull()
    {
        var act = () => FilteredSpawnFinder.FindSpawn(0L, AllGrass(), null!, 64, 64, null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("biases");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region No filter — delegates to SpawnFinder
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_ReturnsSpawn_WhenFilterIsNull_AndSpawnableTilesExist()
    {
        var result = FilteredSpawnFinder.FindSpawn(42L, AllGrass(), new BiasLayers(), 64, 64, null);
        result.Should().NotBeNull();
    }

    [Fact]
    public void FindSpawn_ReturnsNull_WhenFilterIsNull_AndNoSpawnableTiles()
    {
        var result = FilteredSpawnFinder.FindSpawn(42L, AllWater(), new BiasLayers(), 64, 64, null);
        result.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Zone restriction
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_ReturnsSpawnInsideZone_WhenZoneCoversSpawnRange()
    {
        // Zone 16..47 × 16..47 sits within the inner-¾ spawn search area (8..48).
        // All-Grass grid → every candidate in the zone is spawnable.
        var zone = new RectangleZone(16, 16, 32, 32);
        var filter = new SpawnConstraintFilter([zone]);

        var result = FilteredSpawnFinder.FindSpawn(42L, AllGrass(), new BiasLayers(), 64, 64, filter);

        result.Should().NotBeNull();
        var (x, y) = result!.Value;
        zone.Contains(x, y).Should().BeTrue();
    }

    [Fact]
    public void FindSpawn_SearchesUnionOfZones_WhenMultipleAllowedZones()
    {
        // Two non-overlapping zones. AllGrass → both have spawnable tiles.
        var zone1 = new RectangleZone(10, 10, 16, 16);  // 10..25 × 10..25
        var zone2 = new RectangleZone(38, 38, 16, 16);  // 38..53 × 38..53
        var filter = new SpawnConstraintFilter([zone1, zone2]);

        var result = FilteredSpawnFinder.FindSpawn(42L, AllGrass(), new BiasLayers(), 64, 64, filter);

        result.Should().NotBeNull();
        var (x, y) = result!.Value;
        (zone1.Contains(x, y) || zone2.Contains(x, y)).Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Proximity constraint
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_ReturnsSpawnNearCluster_WhenProximityConstraintMet()
    {
        // Rock patch at (28,28) size 8×8 = 64 cells. Grass everywhere else.
        // Proximity: must be within Chebyshev distance 5 of a cluster of ≥1 Rock cell.
        // Candidate area = Chebyshev-5 expansion of patch ∩ whole-grid zone
        //                = [23, 40] × [23, 40].
        var classifier = new PatchClassifier(new RectangleZone(28, 28, 8, 8));
        var proximity = new TileClusterProximity(MaxDistance: 5, ClusterTileId: 3, MinClusterSize: 1);
        var filter = new SpawnConstraintFilter([new RectangleZone(0, 0, 64, 64)], proximity);

        var result = FilteredSpawnFinder.FindSpawn(42L, classifier, new BiasLayers(), 64, 64, filter);

        result.Should().NotBeNull();
        var (x, y) = result!.Value;
        (x >= 23 && x <= 40 && y >= 23 && y <= 40).Should().BeTrue();
    }

    [Fact]
    public void FindSpawn_ReturnsNull_WhenNoClusterMeetsMinimumSize()
    {
        // Rock patch = 2×2 = 4 cells, but MinClusterSize requires 20.
        // No qualifying cluster → candidate area is empty → null.
        var classifier = new PatchClassifier(new RectangleZone(30, 30, 2, 2));
        var proximity = new TileClusterProximity(MaxDistance: 5, ClusterTileId: 3, MinClusterSize: 20);
        var filter = new SpawnConstraintFilter([new RectangleZone(0, 0, 64, 64)], proximity);

        var result = FilteredSpawnFinder.FindSpawn(42L, classifier, new BiasLayers(), 64, 64, filter);

        result.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Fallback bias application
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_CallsOnSpawnFallbackApplied_WhenNoInitialSpawnInZone()
    {
        // AllWater → no tile classifies as spawnable (id=1 or 2) initially.
        // Zone covers a large area within spawn range → fallback should be triggered.
        var zone = new RectangleZone(16, 16, 32, 32);
        var filter = new SpawnConstraintFilter([zone]);
        var sink = new FilteredSpawnFinderCapturingSink();

        FilteredSpawnFinder.FindSpawn(42L, AllWater(), new BiasLayers(), 64, 64, filter, sink);

        sink.SpawnFallbacks.Should().Equal([filter]);
    }

    [Fact]
    public void FindSpawn_ReturnsFallbackSpawn_AfterBiasApplicationMakesZoneSpawnable()
    {
        // AllWater (coast ≤ 0 everywhere). Zone covers inner spawn range.
        // Fallback adds coast bias to all zone cells → coast = 0.301 > 0.3 → Sand(1).
        // Re-search then finds a spawn.
        var zone = new RectangleZone(16, 16, 32, 32);
        var filter = new SpawnConstraintFilter([zone]);

        var result = FilteredSpawnFinder.FindSpawn(42L, AllWater(), new BiasLayers(), 64, 64, filter);

        result.Should().NotBeNull();
        var (x, y) = result!.Value;
        zone.Contains(x, y).Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
}

// --------------------------------------------------------------------------
// Test doubles
// --------------------------------------------------------------------------

internal sealed class FilteredSpawnFinderCapturingSink : IFilterDiagnosticSink
{
    internal List<(MapFilter filter, int deficit, int resolved)> TileCountConflicts { get; } = [];
    internal List<SpawnConstraintFilter> SpawnFallbacks { get; } = [];
    internal List<(LocalConcentrationFilter filter, int reached, int required)> ClusterStalls { get; } = [];

    public void OnFilterConflict(MapFilter filter, int deficit, int resolved) =>
        TileCountConflicts.Add((filter, deficit, resolved));

    public void OnSpawnFallbackApplied(SpawnConstraintFilter filter) =>
        SpawnFallbacks.Add(filter);

    public void OnClusterGrowthStalled(LocalConcentrationFilter filter, int reached, int required) =>
        ClusterStalls.Add((filter, reached, required));
}
