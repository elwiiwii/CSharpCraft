using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class TileTypeTests
{
    private static readonly ItemDef Stone = new("stone", 118, [0, 1, 5, 13]);
    private static readonly ItemDef Wood = new("wood", 103);
    private static readonly TileType UnderType = new(BlendGroup.Sand);

    // --------------------------------------------------------------------------
    #region TileType
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_StoresBlendGroup()
    {
        TileType sut = new(BlendGroup.Grass);
        _ = sut.BlendGroup.Should().Be(BlendGroup.Grass);
    }

    [Fact]
    public void Constructor_DefaultsSpritePalToNull()
    {
        TileType sut = new(BlendGroup.Sand);
        _ = sut.SpritePal.Should().BeNull();
    }

    [Fact]
    public void Constructor_StoresSpritePal()
    {
        int[] pal = new[] { 1, 5, 3, 11 };
        TileType sut = new(BlendGroup.Grass, pal);
        _ = sut.SpritePal.Should().BeSameAs(pal);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region WallTileType
    // --------------------------------------------------------------------------

    [Fact]
    public void WallTileType_IsA_TileType()
    {
        WallTileType sut = new(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        _ = sut.Should().BeAssignableTo<TileType>();
    }

    [Fact]
    public void WallTileType_StoresBlendGroup()
    {
        WallTileType sut = new(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        _ = sut.BlendGroup.Should().Be(BlendGroup.Rock);
    }

    [Fact]
    public void WallTileType_StoresMat()
    {
        WallTileType sut = new(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        _ = sut.Mat.Should().BeSameAs(Stone);
    }

    [Fact]
    public void WallTileType_StoresLife()
    {
        WallTileType sut = new(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        _ = sut.Life.Should().Be(15);
    }

    [Fact]
    public void WallTileType_StoresUnderlyingType()
    {
        WallTileType sut = new(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        _ = sut.UnderlyingType.Should().BeSameAs(UnderType);
    }

    [Fact]
    public void WallTileType_StoresSpritePal_WhenProvided()
    {
        int[] pal = new[] { 1, 5, 3, 11 };
        WallTileType sut = new(BlendGroup.Grass, pal, Wood, UnderType, life: 8);
        _ = sut.SpritePal.Should().BeSameAs(pal);
    }

    [Fact]
    public void WallTileType_SpritePalIsNull_ForBlendedSurface()
    {
        WallTileType sut = new(BlendGroup.Rock, null, Stone, UnderType, life: 15);
        _ = sut.SpritePal.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Tile
    // --------------------------------------------------------------------------

    [Fact]
    public void Tile_Constructor_StoresType()
    {
        TileType type = new(BlendGroup.Sand);
        Tile sut = new(type);
        _ = sut.Type.Should().BeSameAs(type);
    }

    [Fact]
    public void Tile_Constructor_DefaultsNullHarvestLife()
    {
        Tile sut = new(new TileType(BlendGroup.Sand));
        _ = sut.HarvestLife.Should().BeNull();
    }

    [Fact]
    public void Tile_Constructor_DefaultsNullGrowthTimer()
    {
        Tile sut = new(new TileType(BlendGroup.Sand));
        _ = sut.GrowthTimer.Should().BeNull();
    }

    [Fact]
    public void Tile_With_UpdatesHarvestLife()
    {
        Tile sut = new Tile(new TileType(BlendGroup.Sand)) with { HarvestLife = F32.FromInt(10) };
        _ = sut.HarvestLife.Should().Be(F32.FromInt(10));
    }

    // --------------------------------------------------------------------------
    #endregion
}
