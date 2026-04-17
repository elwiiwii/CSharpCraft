using CSharpCraft.PcraftPreview.Noise;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftPreview.Noise;

public sealed class SpawnFinderTests
{
    // --------------------------------------------------------------------------
    #region Helpers
    // --------------------------------------------------------------------------

    // FixedGrid always returns the same double value regardless of coordinates.
    // Allows deterministic control of MapClassifier output in tests.
    private sealed class FixedGrid : SeededNoiseGrid
    {
        private readonly double _value;

        internal FixedGrid(double value, int gridSx = 64, int gridSy = 64)
            : base(masterSeed: 0L, gridSx: gridSx, gridSy: gridSy,
                   featStep: 999, startScale: 0.0, scaleMod: 0.0, layerIndex: 0)
        {
            _value = value;
        }

        internal override double GetValue(int x, int y) => _value;
    }

    // Classifier where every tile classifies to id=0 (base/water):
    // FixedGrid(0.5) for all four → v = 0 → coast = -4×dist⁴ ≤ 0 < 0.3 → id = a = 0.
    private static MapClassifier AllBaseClassifier(int gridSx = 64, int gridSy = 64)
    {
        var g = new FixedGrid(0.5, gridSx, gridSy);
        return new MapClassifier(g, g, g, g, gridSx, gridSy, a: 0, b: 1, c: 2, d: 3, e: 4);
    }

    // Classifier where every tile in the spawn range classifies to id=2 (rare/c):
    // v = 1.0 → coast = 4 - 4×dist⁴. Worst spawn-range edge for 64×64 grid:
    // dist⁴ = 0.75⁴ ≈ 0.316, coast ≈ 2.73 >> 0.6 → id = c = 2.
    // v2 = 0, v3 = 0 → no d/e override.
    private static MapClassifier AllRareClassifier(int gridSx = 64, int gridSy = 64)
    {
        var cur  = new FixedGrid(1.0, gridSx, gridSy);
        var cur2 = new FixedGrid(0.0, gridSx, gridSy);
        var cur3 = new FixedGrid(1.0, gridSx, gridSy);
        var cur4 = new FixedGrid(1.0, gridSx, gridSy);
        return new MapClassifier(cur, cur2, cur3, cur4, gridSx, gridSy, a: 0, b: 1, c: 2, d: 3, e: 4);
    }

    // Classifier where every tile in the spawn range classifies to id=3 (stone/d):
    // v = 1.0 → coast > 0.3; v2 = 1.0 > 0.5 → d-rule fires → id = d = 3.
    private static MapClassifier AllStoneClassifier(int gridSx = 64, int gridSy = 64)
    {
        var cur  = new FixedGrid(1.0, gridSx, gridSy);
        var cur2 = new FixedGrid(0.0, gridSx, gridSy);
        var cur3 = new FixedGrid(0.0, gridSx, gridSy); // v2 = 1.0 > 0.5 → d = 3
        var cur4 = new FixedGrid(1.0, gridSx, gridSy);
        return new MapClassifier(cur, cur2, cur3, cur4, gridSx, gridSy, a: 0, b: 1, c: 2, d: 3, e: 4);
    }

    // Classifier where every tile in the spawn range classifies to id=4 (tree/e):
    // coast > 0.6 → c; v2 = 0 ≤ 0.5 → no d; v3 = 1.0 > 0.5 → e-rule fires → id = e = 4.
    private static MapClassifier AllTreeClassifier(int gridSx = 64, int gridSy = 64)
    {
        var cur  = new FixedGrid(1.0, gridSx, gridSy);
        var cur2 = new FixedGrid(0.0, gridSx, gridSy);
        var cur3 = new FixedGrid(1.0, gridSx, gridSy); // v2 = 0 → no d override
        var cur4 = new FixedGrid(0.0, gridSx, gridSy); // v3 = 1.0 > 0.5 → e = 4
        return new MapClassifier(cur, cur2, cur3, cur4, gridSx, gridSy, a: 0, b: 1, c: 2, d: 3, e: 4);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Parameter null guards
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_ThrowsArgumentNullException_WhenClassifierIsNull()
    {
        var act = () => SpawnFinder.FindSpawn(0L, classifier: null!, gridSx: 64, gridSy: 64);

        act.Should().Throw<ArgumentNullException>().WithParameterName("classifier");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Null result — no valid spawn found
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_ReturnsNull_WhenAllTilesAreBaseTile()
    {
        // id = 0 (base/water): not in the valid spawn set {1, 2}.
        var result = SpawnFinder.FindSpawn(0L, AllBaseClassifier(), gridSx: 64, gridSy: 64);

        result.Should().BeNull(because: "id=0 (base/water) is not a valid spawn tile");
    }

    [Fact]
    public void FindSpawn_ReturnsNull_WhenAllTilesAreStoneTile()
    {
        // id = 3 (stone/d): not in the valid spawn set {1, 2}.
        var result = SpawnFinder.FindSpawn(0L, AllStoneClassifier(), gridSx: 64, gridSy: 64);

        result.Should().BeNull(because: "id=3 (stone) is not a valid spawn tile");
    }

    [Fact]
    public void FindSpawn_ReturnsNull_WhenAllTilesAreTreeTile()
    {
        // id = 4 (tree/e): not in the valid spawn set {1, 2}.
        var result = SpawnFinder.FindSpawn(0L, AllTreeClassifier(), gridSx: 64, gridSy: 64);

        result.Should().BeNull(because: "id=4 (tree) is not a valid spawn tile");
    }

    [Fact]
    public void FindSpawn_ReturnsNull_AfterExhaustingAllCandidates_WithNoValidTile()
    {
        // Verifies the algorithm terminates (does not loop forever) when no tile qualifies.
        // AllStoneClassifier returns id=3 for every tile, so all 500 candidates are rejected.
        var result = SpawnFinder.FindSpawn(999L, AllStoneClassifier(), gridSx: 64, gridSy: 64);

        result.Should().BeNull(because: "500 candidates exhausted with no valid tile → null");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Non-null result — valid spawn found
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(42L)]
    [InlineData(-1L)]
    [InlineData(long.MaxValue)]
    public void FindSpawn_ReturnsNonNull_WhenAllTilesAreId2(long masterSeed)
    {
        // Every tile in the spawn range classifies to id=2 → a spawn must always be found.
        SpawnFinder.FindSpawn(masterSeed, AllRareClassifier(), 64, 64)
            .Should().NotBeNull(because: $"seed {masterSeed}: all tiles are id=2, so a spawn must be found");
    }

    [Fact]
    public void FindSpawn_ReturnsNonNull_WhenAllTilesAreId1()
    {
        // id=1 (sand/b) must be accepted as a valid spawn. Re-use AllRareClassifier geometry
        // (coast >> 0.6 everywhere in spawn range) but remap c → 1 via custom tile ids.
        var cur  = new FixedGrid(1.0);
        var cur2 = new FixedGrid(0.0);
        var cur3 = new FixedGrid(1.0);
        var cur4 = new FixedGrid(1.0);
        var classifier = new MapClassifier(cur, cur2, cur3, cur4, 64, 64,
            a: 0, b: 1, c: 1, d: 3, e: 4);

        SpawnFinder.FindSpawn(0L, classifier, 64, 64)
            .Should().NotBeNull(because: "id=1 (sand) must be accepted as a valid spawn tile");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Returned tile — bounds
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_ReturnedTile_IsWithinExpectedBounds()
    {
        // Spawn candidates must be drawn from [gridSx/8, gridSx*7/8] × [gridSy/8, gridSy*7/8].
        const int GridSx = 64;
        const int GridSy = 64;
        int minX = GridSx / 8;      // 8
        int maxX = GridSx * 7 / 8;  // 56
        int minY = GridSy / 8;
        int maxY = GridSy * 7 / 8;

        var result = SpawnFinder.FindSpawn(42L, AllRareClassifier(GridSx, GridSy), GridSx, GridSy);

        result.Should().NotBeNull();
        result!.Value.tileX.Should().BeInRange(minX, maxX,
            because: "spawn x must lie within [gridSx/8, gridSx*7/8]");
        result.Value.tileY.Should().BeInRange(minY, maxY,
            because: "spawn y must lie within [gridSy/8, gridSy*7/8]");
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(7L)]
    [InlineData(99L)]
    [InlineData(256L)]
    public void FindSpawn_ReturnedTile_NeverExceedsOuterEighthMargin(long seed)
    {
        const int GridSx = 64;
        const int GridSy = 64;

        var result = SpawnFinder.FindSpawn(seed, AllRareClassifier(GridSx, GridSy), GridSx, GridSy);

        result.Should().NotBeNull();
        result!.Value.tileX.Should().BeGreaterThanOrEqualTo(GridSx / 8,
            because: "spawn must not be placed in the outer 1/8 left margin");
        result.Value.tileX.Should().BeLessThanOrEqualTo(GridSx * 7 / 8,
            because: "spawn must not be placed in the outer 1/8 right margin");
        result.Value.tileY.Should().BeGreaterThanOrEqualTo(GridSy / 8,
            because: "spawn must not be placed in the outer 1/8 top margin");
        result.Value.tileY.Should().BeLessThanOrEqualTo(GridSy * 7 / 8,
            because: "spawn must not be placed in the outer 1/8 bottom margin");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Returned tile — validity
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_ReturnedTileId_IsInValidSpawnSet()
    {
        // ClassifyTile of the returned coordinates must resolve to an id ∈ {1, 2}.
        var classifier = AllRareClassifier();
        var result = SpawnFinder.FindSpawn(0L, classifier, 64, 64);

        result.Should().NotBeNull();
        int tileId = classifier.ClassifyTile(result!.Value.tileX, result.Value.tileY);
        tileId.Should().BeOneOf(new[] { 1, 2 },
            because: "FindSpawn must only return a tile whose id is in the valid spawn set {1, 2}");
    }

    [Fact]
    public void FindSpawn_ReturnedTileId_IsInValidSpawnSet_AcrossMultipleSeeds()
    {
        // Verify the validity contract holds for many seeds.
        var classifier = AllRareClassifier();

        for (int s = 0; s < 10; s++)
        {
            var result = SpawnFinder.FindSpawn((long)s, classifier, 64, 64);
            result.Should().NotBeNull(because: $"seed {s}: all tiles valid, a spawn must be found");
            int tileId = classifier.ClassifyTile(result!.Value.tileX, result.Value.tileY);
            tileId.Should().BeOneOf(new[] { 1, 2 },
                because: $"seed {s}: returned tile must have a valid spawn id");
        }
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Determinism
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_IsIdempotent_ForSameSeed()
    {
        // Multiple invocations with the same seed and same classifier must produce the same result.
        var classifier = AllRareClassifier();

        var first  = SpawnFinder.FindSpawn(123L, classifier, 64, 64);
        var second = SpawnFinder.FindSpawn(123L, classifier, 64, 64);

        second.Should().Be(first,
            because: "FindSpawn is purely deterministic — same seed must always yield the same spawn");
    }

    [Fact]
    public void FindSpawn_ProducesDifferentResults_ForDifferentSeeds()
    {
        // 20 consecutive seeds must produce at least 2 distinct spawn positions.
        // (The probability of all 20 landing on the same tile by chance is negligible.)
        var classifier = AllRareClassifier();

        var distinctCount = Enumerable.Range(0, 20)
            .Select(i => SpawnFinder.FindSpawn((long)i, classifier, 64, 64))
            .Distinct()
            .Count();

        distinctCount.Should().BeGreaterThan(1,
            because: "different seeds must explore different candidates and produce varied positions");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Spawn id contract — which ids are accepted
    // --------------------------------------------------------------------------

    // --- Valid spawn ids ---

    [Theory]
    [InlineData(1)] // sand tile
    [InlineData(2)] // rare tile
    public void FindSpawn_AcceptsTileId_WhenIdIsInSpawnSet(int validId)
    {
        // All five tile ids are remapped to validId so the classifier always returns validId.
        var cur  = new FixedGrid(1.0);
        var cur2 = new FixedGrid(0.0);
        var cur3 = new FixedGrid(1.0);
        var cur4 = new FixedGrid(1.0);
        var classifier = new MapClassifier(cur, cur2, cur3, cur4, 64, 64,
            a: validId, b: validId, c: validId, d: validId, e: validId);

        SpawnFinder.FindSpawn(0L, classifier, 64, 64)
            .Should().NotBeNull(because: $"id={validId} must be accepted as a valid spawn tile");
    }

    // --- Invalid spawn ids ---

    [Theory]
    [InlineData(0)] // base/water — not a spawn
    [InlineData(3)] // stone — not a spawn
    [InlineData(4)] // tree — not a spawn
    public void FindSpawn_RejectsTileId_WhenIdIsNotInSpawnSet(int invalidId)
    {
        // All five tile ids are remapped to invalidId → every candidate is rejected → null.
        var cur  = new FixedGrid(1.0);
        var cur2 = new FixedGrid(0.0);
        var cur3 = new FixedGrid(1.0);
        var cur4 = new FixedGrid(1.0);
        var classifier = new MapClassifier(cur, cur2, cur3, cur4, 64, 64,
            a: invalidId, b: invalidId, c: invalidId, d: invalidId, e: invalidId);

        SpawnFinder.FindSpawn(0L, classifier, 64, 64)
            .Should().BeNull(because: $"id={invalidId} must not be accepted as a valid spawn tile");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Real-noise integration
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_WithRealNoiseGrids_ReturnsValidSpawnOrNull()
    {
        // End-to-end sanity check using production-equivalent noise parameters.
        // The result can be null (no spawn found), but if non-null it must be within bounds
        // and have a valid tile id.
        var cur  = new SeededNoiseGrid(42L, 64, 64, 64, 0.9, 0.2, 0);
        var cur2 = new SeededNoiseGrid(42L, 64, 64, 8,  0.9, 0.4, 1);
        var cur3 = new SeededNoiseGrid(42L, 64, 64, 8,  0.9, 0.3, 2);
        var cur4 = new SeededNoiseGrid(42L, 64, 64, 4,  0.8, 1.1, 3);
        var classifier = new MapClassifier(cur, cur2, cur3, cur4,
            gridSx: 64, gridSy: 64, a: 0, b: 1, c: 2, d: 3, e: 4);

        var result = SpawnFinder.FindSpawn(42L, classifier, 64, 64);

        if (result is not null)
        {
            result.Value.tileX.Should().BeInRange(64 / 8, 64 * 7 / 8,
                because: "returned x must be within spawn bounds");
            result.Value.tileY.Should().BeInRange(64 / 8, 64 * 7 / 8,
                because: "returned y must be within spawn bounds");
            classifier.ClassifyTile(result.Value.tileX, result.Value.tileY)
                .Should().BeOneOf(new[] { 1, 2 }, because: "returned tile must have a valid spawn id");
        }
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(2L)]
    [InlineData(3L)]
    [InlineData(100L)]
    public void FindSpawn_WithRealNoiseGrids_WhenResultIsNonNull_IsWithinBoundsAndValid(long seed)
    {
        // Across a range of seeds, any non-null result must always satisfy both constraints.
        var cur  = new SeededNoiseGrid(seed, 64, 64, 64, 0.9, 0.2, 0);
        var cur2 = new SeededNoiseGrid(seed, 64, 64, 8,  0.9, 0.4, 1);
        var cur3 = new SeededNoiseGrid(seed, 64, 64, 8,  0.9, 0.3, 2);
        var cur4 = new SeededNoiseGrid(seed, 64, 64, 4,  0.8, 1.1, 3);
        var classifier = new MapClassifier(cur, cur2, cur3, cur4,
            gridSx: 64, gridSy: 64, a: 0, b: 1, c: 2, d: 3, e: 4);

        var result = SpawnFinder.FindSpawn(seed, classifier, 64, 64);

        if (result is null)
            return; // null is acceptable if no suitable tile exists for this seed

        result.Value.tileX.Should().BeInRange(64 / 8, 64 * 7 / 8);
        result.Value.tileY.Should().BeInRange(64 / 8, 64 * 7 / 8);
        classifier.ClassifyTile(result.Value.tileX, result.Value.tileY)
            .Should().BeOneOf(new[] { 1, 2 }, because: "non-null result must point to a valid spawn tile");
    }

    // --------------------------------------------------------------------------
    #endregion
}
