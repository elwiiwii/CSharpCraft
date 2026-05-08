using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftFilter.Zones;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftFilter.Filters;

public sealed class TileCountFilterTests
{
    // --------------------------------------------------------------------------
    #region HashString
    // --------------------------------------------------------------------------

    [Fact]
    public void HashString_ContainsGlobalSuffix_WhenNoZone()
    {
        TileCountFilter filter = new(TileId: 3, MinimumCount: 30);
        _ = filter.HashString.Should().Be("tc.3.30.all");
    }

    [Fact]
    public void HashString_ContainsZoneHash_WhenZoneProvided()
    {
        RectangleZone zone = new(0, 0, 32, 32);
        TileCountFilter filter = new(TileId: 2, MinimumCount: 15, Zone: zone);
        _ = filter.HashString.Should().Be($"tc.2.15.{zone.HashString}");
    }

    [Fact]
    public void HashString_DiffersForDifferentTileIds()
    {
        TileCountFilter a = new(TileId: 3, MinimumCount: 30);
        TileCountFilter b = new(TileId: 4, MinimumCount: 30);
        _ = a.HashString.Should().NotBe(b.HashString);
    }

    [Fact]
    public void HashString_DiffersForDifferentMinimumCounts()
    {
        TileCountFilter a = new(TileId: 3, MinimumCount: 20);
        TileCountFilter b = new(TileId: 3, MinimumCount: 30);
        _ = a.HashString.Should().NotBe(b.HashString);
    }

    // --------------------------------------------------------------------------
    #endregion
}

public sealed class SpawnConstraintFilterTests
{
    // --------------------------------------------------------------------------
    #region HashString
    // --------------------------------------------------------------------------

    [Fact]
    public void HashString_ContainsZoneHashes_WhenNoProximity()
    {
        RectangleZone zone = new(10, 10, 44, 44);
        SpawnConstraintFilter filter = new(AllowedZones: [zone]);
        _ = filter.HashString.Should().Be($"spa.{zone.HashString}.");
    }

    [Fact]
    public void HashString_ContainsProximityHash_WhenProximitySet()
    {
        RectangleZone zone = new(0, 0, 64, 64);
        TileClusterProximity prox = new(MaxDistance: 5, ClusterTileId: 3, MinClusterSize: 8);
        SpawnConstraintFilter filter = new(AllowedZones: [zone], Proximity: prox);
        _ = filter.HashString.Should().Contain("prox.5.3.8");
    }

    [Fact]
    public void HashString_IncludesAllZoneHashes_WhenMultipleZones()
    {
        RectangleZone z1 = new(0, 0, 16, 16);
        RadiusZone z2 = new(32, 32, 8);
        SpawnConstraintFilter filter = new(AllowedZones: [z1, z2]);
        _ = filter.HashString.Should().Contain(z1.HashString).And.Contain(z2.HashString);
    }

    // --------------------------------------------------------------------------
    #endregion
}

public sealed class LocalConcentrationFilterTests
{
    // --------------------------------------------------------------------------
    #region HashString
    // --------------------------------------------------------------------------

    [Fact]
    public void HashString_ContainsZoneTileIdAndClusterSize()
    {
        RadiusZone zone = new(20, 20, 10);
        LocalConcentrationFilter filter = new(Zone: zone, TileId: 4, MinClusterSize: 12);
        _ = filter.HashString.Should().Be($"lc.{zone.HashString}.4.12");
    }

    [Fact]
    public void HashString_DiffersForDifferentClusterSizes()
    {
        RectangleZone zone = new(0, 0, 32, 32);
        LocalConcentrationFilter a = new(Zone: zone, TileId: 3, MinClusterSize: 10);
        LocalConcentrationFilter b = new(Zone: zone, TileId: 3, MinClusterSize: 20);
        _ = a.HashString.Should().NotBe(b.HashString);
    }

    // --------------------------------------------------------------------------
    #endregion
}

public sealed class TileClusterProximityTests
{
    [Fact]
    public void HashString_ContainsAllThreeParameters()
    {
        TileClusterProximity prox = new(MaxDistance: 3, ClusterTileId: 2, MinClusterSize: 6);
        _ = prox.HashString.Should().Be("prox.3.2.6");
    }
}

public sealed class FilterSetTests
{
    // --------------------------------------------------------------------------
    #region HashString
    // --------------------------------------------------------------------------

    [Fact]
    public void HashString_IsEmpty_WhenNoFilters()
    {
        FilterSet set = new([]);
        _ = set.HashString.Should().BeEmpty();
    }

    [Fact]
    public void HashString_IsFilterHash_WhenSingleFilter()
    {
        TileCountFilter filter = new(TileId: 3, MinimumCount: 30);
        FilterSet set = new([filter]);
        _ = set.HashString.Should().Be(filter.HashString);
    }

    [Fact]
    public void HashString_CombinesFilterHashesWithColon_WhenMultipleFilters()
    {
        TileCountFilter f1 = new(TileId: 3, MinimumCount: 30);
        TileCountFilter f2 = new(TileId: 4, MinimumCount: 30);
        FilterSet set = new([f1, f2]);
        _ = set.HashString.Should().Be($"{f1.HashString}:{f2.HashString}");
    }

    [Fact]
    public void HashString_IsStable_AcrossReconstructionsWithSameFilters()
    {
        TileCountFilter f1 = new(TileId: 3, MinimumCount: 30);
        TileCountFilter f2 = new(TileId: 4, MinimumCount: 30);
        FilterSet a = new([f1, f2]);
        FilterSet b = new([f1, f2]);
        _ = a.HashString.Should().Be(b.HashString);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region CombineWithSeed
    // --------------------------------------------------------------------------

    [Fact]
    public void CombineWithSeed_PrependsSeedWithColon_WhenFiltersExist()
    {
        TileCountFilter filter = new(TileId: 3, MinimumCount: 30);
        FilterSet set = new([filter]);
        _ = set.CombineWithSeed(12345L).Should().Be($"12345:{filter.HashString}");
    }

    [Fact]
    public void CombineWithSeed_ReturnsSeedString_WhenNoFilters()
    {
        FilterSet set = new([]);
        _ = set.CombineWithSeed(99L).Should().Be("99:");
    }

    [Fact]
    public void CombineWithSeed_DiffersForDifferentSeeds()
    {
        FilterSet set = new([new TileCountFilter(TileId: 3, MinimumCount: 30)]);
        _ = set.CombineWithSeed(1L).Should().NotBe(set.CombineWithSeed(2L));
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Filters collection
    // --------------------------------------------------------------------------

    [Fact]
    public void Filters_PreservesInsertionOrder()
    {
        TileCountFilter f1 = new(TileId: 3, MinimumCount: 30);
        TileCountFilter f2 = new(TileId: 4, MinimumCount: 20);
        FilterSet set = new([f1, f2]);
        _ = set.Filters.Should().ContainInOrder(f1, f2);
    }

    // --------------------------------------------------------------------------
    #endregion
}
