using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class TileTypeTests
{
    private static readonly ItemDef Stone = new("stone", 118, [0, 1, 5, 13]);
    private static readonly ItemDef Wood  = new("wood",  103);
    private static readonly TileType UnderType = new(BlendGroup.Sand);

    // --------------------------------------------------------------------------
    #region TileType
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_StoresBlendGroup()
    {
        var sut = new TileType(BlendGroup.Grass);
        sut.BlendGroup.Should().Be(BlendGroup.Grass);
    }

    [Fact]
    public void Constructor_DefaultsSpritePalToNull()
    {
        var sut = new TileType(BlendGroup.Sand);
        sut.SpritePal.Should().BeNull();
    }

    [Fact]
    public void Constructor_StoresSpritePal()
    {
        var pal = new[] { 1, 5, 3, 11 };
        var sut = new TileType(BlendGroup.Grass, pal);
        sut.SpritePal.Should().BeSameAs(pal);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region WallTileType
    // --------------------------------------------------------------------------

    [Fact]
    public void WallTileType_IsA_TileType()
    {
        var sut = new WallTileType(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        sut.Should().BeAssignableTo<TileType>();
    }

    [Fact]
    public void WallTileType_StoresBlendGroup()
    {
        var sut = new WallTileType(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        sut.BlendGroup.Should().Be(BlendGroup.Rock);
    }

    [Fact]
    public void WallTileType_StoresMat()
    {
        var sut = new WallTileType(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        sut.Mat.Should().BeSameAs(Stone);
    }

    [Fact]
    public void WallTileType_StoresLife()
    {
        var sut = new WallTileType(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        sut.Life.Should().Be(15);
    }

    [Fact]
    public void WallTileType_StoresUnderlyingType()
    {
        var sut = new WallTileType(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        sut.UnderlyingType.Should().BeSameAs(UnderType);
    }

    [Fact]
    public void WallTileType_StoresSpritePal_WhenProvided()
    {
        var pal = new[] { 1, 5, 3, 11 };
        var sut = new WallTileType(BlendGroup.Grass, pal, Wood, UnderType, life: 8);
        sut.SpritePal.Should().BeSameAs(pal);
    }

    [Fact]
    public void WallTileType_SpritePalIsNull_ForBlendedSurface()
    {
        var sut = new WallTileType(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        sut.SpritePal.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Tile
    // --------------------------------------------------------------------------

    [Fact]
    public void Tile_Constructor_StoresType()
    {
        var type = new TileType(BlendGroup.Sand);
        var sut = new Tile(type);
        sut.Type.Should().BeSameAs(type);
    }

    [Fact]
    public void Tile_Constructor_DefaultsNullHarvestLife()
    {
        var sut = new Tile(new TileType(BlendGroup.Sand));
        sut.HarvestLife.Should().BeNull();
    }

    [Fact]
    public void Tile_Constructor_DefaultsNullGrowthTimer()
    {
        var sut = new Tile(new TileType(BlendGroup.Sand));
        sut.GrowthTimer.Should().BeNull();
    }

    [Fact]
    public void Tile_With_UpdatesHarvestLife()
    {
        var sut = new Tile(new TileType(BlendGroup.Sand)) with { HarvestLife = F32.FromInt(10) };
        sut.HarvestLife.Should().Be(F32.FromInt(10));
    }

    // --------------------------------------------------------------------------
    #endregion
}
