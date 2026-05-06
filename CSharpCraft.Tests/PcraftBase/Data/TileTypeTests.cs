#nullable enable
using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class FloorTypeTests
{
    [Fact]
    public void Constructor_StoresId()
    {
        var sut = new FloorType(TileId.Sand, gr: 1);
        sut.Id.Should().Be(TileId.Sand);
    }

    [Fact]
    public void Constructor_StoresGr()
    {
        var sut = new FloorType(TileId.Grass, gr: 2);
        sut.Gr.Should().Be(2);
    }
}

public sealed class SurfaceTypeTests
{
    private static readonly FloorType UnderFloor = new(TileId.Sand, gr: 1);
    private static readonly ItemDef Stone = new("stone", 118, [0, 1, 5, 13]);
    private static readonly ItemDef Wood  = new("wood", 103);

    [Fact]
    public void Constructor_StoresMat()
    {
        var sut = new SurfaceType(TileId.Rock, gr: 3, Stone, UnderFloor, life: 15);
        sut.Mat.Should().BeSameAs(Stone);
    }

    [Fact]
    public void Constructor_StoresLife()
    {
        var sut = new SurfaceType(TileId.Rock, gr: 3, Stone, UnderFloor, life: 15);
        sut.Life.Should().Be(15);
    }

    [Fact]
    public void Constructor_StoresUnderlyingFloor()
    {
        var sut = new SurfaceType(TileId.Rock, gr: 3, Stone, UnderFloor, life: 15);
        sut.UnderlyingFloor.Should().BeSameAs(UnderFloor);
    }

    [Fact]
    public void OverlaySurface_Constructor_StoresPal()
    {
        var pal = new[] { 1, 5, 3, 11 };
        var sut = new OverlaySurface(TileId.Tree, gr: 2, Wood, UnderFloor, life: 8, pal);
        sut.Pal.Should().BeSameAs(pal);
    }
}

public sealed class TileTests
{
    private static readonly FloorType Floor = new(TileId.Sand, gr: 1);

    [Fact]
    public void Constructor_DefaultsNullSurface()
    {
        var sut = new Tile(Floor);
        sut.Surface.Should().BeNull();
    }

    [Fact]
    public void Constructor_DefaultsNullHarvestLife()
    {
        var sut = new Tile(Floor);
        sut.HarvestLife.Should().BeNull();
    }

    [Fact]
    public void Constructor_DefaultsNullGrowthTimer()
    {
        var sut = new Tile(Floor);
        sut.GrowthTimer.Should().BeNull();
    }

    [Fact]
    public void With_UpdatesHarvestLife()
    {
        var sut = new Tile(Floor) with { HarvestLife = F32.FromInt(10) };
        sut.HarvestLife.Should().Be(F32.FromInt(10));
    }
}
