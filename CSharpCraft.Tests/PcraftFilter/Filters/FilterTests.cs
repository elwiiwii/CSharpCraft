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
        var filter = new TileCountFilter(TileId: 3, MinimumCount: 30);
        filter.HashString.Should().Be("tc.3.30.all");
    }

    [Fact]
    public void HashString_ContainsZoneHash_WhenZoneProvided()
    {
        var zone = new RectangleZone(0, 0, 32, 32);
        var filter = new TileCountFilter(TileId: 2, MinimumCount: 15, Zone: zone);
        filter.HashString.Should().Be($"tc.2.15.{zone.HashString}");
    }

    [Fact]
    public void HashString_DiffersForDifferentTileIds()
    {
        var a = new TileCountFilter(TileId: 3, MinimumCount: 30);
        var b = new TileCountFilter(TileId: 4, MinimumCount: 30);
        a.HashString.Should().NotBe(b.HashString);
    }

    [Fact]
    public void HashString_DiffersForDifferentMinimumCounts()
    {
        var a = new TileCountFilter(TileId: 3, MinimumCount: 20);
        var b = new TileCountFilter(TileId: 3, MinimumCount: 30);
        a.HashString.Should().NotBe(b.HashString);
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
        var zone = new RectangleZone(10, 10, 44, 44);
        var filter = new SpawnConstraintFilter(AllowedZones: [zone]);
        filter.HashString.Should().Be($"spa.{zone.HashString}.");
    }

    [Fact]
    public void HashString_ContainsProximityHash_WhenProximitySet()
    {
        var zone = new RectangleZone(0, 0, 64, 64);
        var prox = new TileClusterProximity(MaxDistance: 5, ClusterTileId: 3, MinClusterSize: 8);
        var filter = new SpawnConstraintFilter(AllowedZones: [zone], Proximity: prox);
        filter.HashString.Should().Contain("prox.5.3.8");
    }

    [Fact]
    public void HashString_IncludesAllZoneHashes_WhenMultipleZones()
    {
        var z1 = new RectangleZone(0, 0, 16, 16);
        var z2 = new RadiusZone(32, 32, 8);
        var filter = new SpawnConstraintFilter(AllowedZones: [z1, z2]);
        filter.HashString.Should().Contain(z1.HashString).And.Contain(z2.HashString);
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
        var zone = new RadiusZone(20, 20, 10);
        var filter = new LocalConcentrationFilter(Zone: zone, TileId: 4, MinClusterSize: 12);
        filter.HashString.Should().Be($"lc.{zone.HashString}.4.12");
    }

    [Fact]
    public void HashString_DiffersForDifferentClusterSizes()
    {
        var zone = new RectangleZone(0, 0, 32, 32);
        var a = new LocalConcentrationFilter(Zone: zone, TileId: 3, MinClusterSize: 10);
        var b = new LocalConcentrationFilter(Zone: zone, TileId: 3, MinClusterSize: 20);
        a.HashString.Should().NotBe(b.HashString);
    }

    // --------------------------------------------------------------------------
    #endregion
}

public sealed class TileClusterProximityTests
{
    [Fact]
    public void HashString_ContainsAllThreeParameters()
    {
        var prox = new TileClusterProximity(MaxDistance: 3, ClusterTileId: 2, MinClusterSize: 6);
        prox.HashString.Should().Be("prox.3.2.6");
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
        var set = new FilterSet([]);
        set.HashString.Should().BeEmpty();
    }

    [Fact]
    public void HashString_IsFilterHash_WhenSingleFilter()
    {
        var filter = new TileCountFilter(TileId: 3, MinimumCount: 30);
        var set = new FilterSet([filter]);
        set.HashString.Should().Be(filter.HashString);
    }

    [Fact]
    public void HashString_CombinesFilterHashesWithColon_WhenMultipleFilters()
    {
        var f1 = new TileCountFilter(TileId: 3, MinimumCount: 30);
        var f2 = new TileCountFilter(TileId: 4, MinimumCount: 30);
        var set = new FilterSet([f1, f2]);
        set.HashString.Should().Be($"{f1.HashString}:{f2.HashString}");
    }

    [Fact]
    public void HashString_IsStable_AcrossReconstructionsWithSameFilters()
    {
        var f1 = new TileCountFilter(TileId: 3, MinimumCount: 30);
        var f2 = new TileCountFilter(TileId: 4, MinimumCount: 30);
        var a = new FilterSet([f1, f2]);
        var b = new FilterSet([f1, f2]);
        a.HashString.Should().Be(b.HashString);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region CombineWithSeed
    // --------------------------------------------------------------------------

    [Fact]
    public void CombineWithSeed_PrependsSeedWithColon_WhenFiltersExist()
    {
        var filter = new TileCountFilter(TileId: 3, MinimumCount: 30);
        var set = new FilterSet([filter]);
        set.CombineWithSeed(12345L).Should().Be($"12345:{filter.HashString}");
    }

    [Fact]
    public void CombineWithSeed_ReturnsSeedString_WhenNoFilters()
    {
        var set = new FilterSet([]);
        set.CombineWithSeed(99L).Should().Be("99:");
    }

    [Fact]
    public void CombineWithSeed_DiffersForDifferentSeeds()
    {
        var set = new FilterSet([new TileCountFilter(TileId: 3, MinimumCount: 30)]);
        set.CombineWithSeed(1L).Should().NotBe(set.CombineWithSeed(2L));
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Filters collection
    // --------------------------------------------------------------------------

    [Fact]
    public void Filters_PreservesInsertionOrder()
    {
        var f1 = new TileCountFilter(TileId: 3, MinimumCount: 30);
        var f2 = new TileCountFilter(TileId: 4, MinimumCount: 20);
        var set = new FilterSet([f1, f2]);
        set.Filters.Should().ContainInOrder(f1, f2);
    }

    // --------------------------------------------------------------------------
    #endregion
}
