using CSharpCraft.PcraftFilter;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftFilter.Zones;
using CSharpCraft.PcraftPreview;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftFilter;

public sealed class FilteredWorldSamplerTests
{
    // Convenience: a FilterSet with no filters.
    private static FilterSet Empty => new([]);

    // --------------------------------------------------------------------------
    #region Argument validation
    // --------------------------------------------------------------------------

    [Fact]
    public void Sample_ThrowsArgumentNullException_WhenFilterSetIsNull()
    {
        var act = () => FilteredWorldSampler.Sample(0L, radius: 2, filters: null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("filters");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Return type compatibility with SampleResult
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]  // minimal
    [InlineData(4)]  // typical preview
    [InlineData(8)]  // larger window
    public void Sample_TilesDimensions_AreTwiceRadiusPlusOne(int radius)
    {
        var result = FilteredWorldSampler.Sample(1L, radius, Empty, forceCenterX: 32, forceCenterY: 32);

        int expected = 2 * radius + 1;
        result.Tiles.GetLength(0).Should().Be(expected);
        result.Tiles.GetLength(1).Should().Be(expected);
    }

    [Fact]
    public void Sample_CenterTile_EqualsForcedCoordinates_WhenForceCenterProvided()
    {
        var result = FilteredWorldSampler.Sample(0L, radius: 4, Empty,
            forceCenterX: 30, forceCenterY: 35);

        result.CenterTileX.Should().Be(30);
        result.CenterTileY.Should().Be(35);
    }

    [Fact]
    public void Sample_RndWat_Is16x16()
    {
        var result = FilteredWorldSampler.Sample(1L, radius: 2, Empty,
            forceCenterX: 32, forceCenterY: 32);

        result.RndWat.GetLength(0).Should().Be(16);
        result.RndWat.GetLength(1).Should().Be(16);
    }

    [Fact]
    public void Sample_RndWat_ValuesAreInRange()
    {
        var result = FilteredWorldSampler.Sample(7L, radius: 2, Empty,
            forceCenterX: 32, forceCenterY: 32);

        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                result.RndWat[i, j].Should().BeInRange(0.0, 100.0);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Empty FilterSet — matches PcraftWorldSampler exactly
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(1L)]
    [InlineData(42L)]
    [InlineData(99L)]
    public void Sample_WithEmptyFilterSet_ProducesSameTilesAsPcraftWorldSampler(long seed)
    {
        var filtered = FilteredWorldSampler.Sample(seed, radius: 4, Empty,
            forceCenterX: 32, forceCenterY: 32);
        var baseline = PcraftWorldSampler.Sample(seed, radius: 4,
            forceCenterX: 32, forceCenterY: 32);

        filtered.Tiles.Should().BeEquivalentTo(baseline.Tiles,
            because: "no filters should leave classification identical to the base sampler");
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(42L)]
    public void Sample_WithEmptyFilterSet_ProducesSameRndWatAsPcraftWorldSampler(long seed)
    {
        var filtered = FilteredWorldSampler.Sample(seed, radius: 4, Empty,
            forceCenterX: 32, forceCenterY: 32);
        var baseline = PcraftWorldSampler.Sample(seed, radius: 4,
            forceCenterX: 32, forceCenterY: 32);

        filtered.RndWat.Should().BeEquivalentTo(baseline.RndWat);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region TileCountFilter enforced
    // --------------------------------------------------------------------------

    [Fact]
    public void Sample_SatisfiesTileCountFilter_ForMinimumRockCount()
    {
        // Demand at least 30 Rock(3) tiles on the 64×64 grid.
        var filters = new FilterSet([new TileCountFilter(TileId: 3, MinimumCount: 30)]);

        var result = FilteredWorldSampler.Sample(1L, radius: 32, filters,
            forceCenterX: 32, forceCenterY: 32);

        int rockCount = CountTileId(result.Tiles, tileId: 3);
        rockCount.Should().BeGreaterThanOrEqualTo(30,
            because: "TileCountFilter(Rock, 30) must be satisfied");
    }

    [Fact]
    public void Sample_SatisfiesTileCountFilter_ForMinimumTreeCount()
    {
        var filters = new FilterSet([new TileCountFilter(TileId: 4, MinimumCount: 20)]);

        var result = FilteredWorldSampler.Sample(2L, radius: 32, filters,
            forceCenterX: 32, forceCenterY: 32);

        int treeCount = CountTileId(result.Tiles, tileId: 4);
        treeCount.Should().BeGreaterThanOrEqualTo(20,
            because: "TileCountFilter(Tree, 4) must be satisfied");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region SpawnConstraintFilter enforced
    // --------------------------------------------------------------------------

    [Fact]
    public void Sample_SpawnIsInsideAllowedZone_WhenSpawnConstraintFilterPresent()
    {
        var zone    = new RectangleZone(16, 16, 32, 32);
        var filters = new FilterSet([new SpawnConstraintFilter([zone])]);

        var result = FilteredWorldSampler.Sample(42L, radius: 4, filters);

        if (result.SpawnTileX >= 0)
            zone.Contains(result.SpawnTileX, result.SpawnTileY).Should().BeTrue(
                because: "SpawnConstraintFilter must confine the spawn to the allowed zone");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Determinism
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(1L)]
    [InlineData(42L)]
    public void Sample_IsDeterministic_ForSameSeedAndFilters(long seed)
    {
        var filters = new FilterSet([new TileCountFilter(TileId: 3, MinimumCount: 10)]);

        var r1 = FilteredWorldSampler.Sample(seed, radius: 4, filters,
            forceCenterX: 32, forceCenterY: 32);
        var r2 = FilteredWorldSampler.Sample(seed, radius: 4, filters,
            forceCenterX: 32, forceCenterY: 32);

        r1.Tiles.Should().BeEquivalentTo(r2.Tiles);
        r1.SpawnTileX.Should().Be(r2.SpawnTileX);
        r1.SpawnTileY.Should().Be(r2.SpawnTileY);
    }

    // --------------------------------------------------------------------------
    #endregion

    // --------------------------------------------------------------------------
    // Helpers
    // --------------------------------------------------------------------------

    private static int CountTileId(int[,] tiles, int tileId)
    {
        int count = 0;
        for (int i = 0; i < tiles.GetLength(0); i++)
            for (int j = 0; j < tiles.GetLength(1); j++)
                if (tiles[i, j] == tileId) count++;
        return count;
    }
}
