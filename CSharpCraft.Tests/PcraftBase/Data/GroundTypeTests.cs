using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class GroundTypeTests
{
    private static readonly ItemDef Stone = new("stone", 118, [0, 1, 5, 13]);
    private static readonly ItemDef Wood = new("wood", 103);

    // --------------------------------------------------------------------------
    #region Simple ground (id and gr only)
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_StoresId()
    {
        var sut = new GroundType(0, 0);
        sut.Id.Should().Be(0);
    }

    [Fact]
    public void Constructor_StoresGr()
    {
        var sut = new GroundType(1, 1);
        sut.Gr.Should().Be(1);
    }

    [Fact]
    public void Mat_IsNull_ByDefault()
    {
        var sut = new GroundType(0, 0);
        sut.Mat.Should().BeNull();
    }

    [Fact]
    public void Tile_IsNull_ByDefault()
    {
        var sut = new GroundType(0, 0);
        sut.Tile.Should().BeNull();
    }

    [Fact]
    public void Life_IsZero_ByDefault()
    {
        var sut = new GroundType(0, 0);
        sut.Life.Should().Be(0);
    }

    [Fact]
    public void IsTree_IsFalse_ByDefault()
    {
        var sut = new GroundType(0, 0);
        sut.IsTree.Should().BeFalse();
    }

    [Fact]
    public void Pal_IsNull_ByDefault()
    {
        var sut = new GroundType(0, 0);
        sut.Pal.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Resource ground (grrock equivalent)
    // --------------------------------------------------------------------------

    // grrock = {id=3, gr=3, mat=stone, tile=grsand, life=15}
    [Fact]
    public void GrRockEquivalent_HasMatAndLife()
    {
        var grSand = new GroundType(1, 1);
        var sut = new GroundType(3, 3) { Mat = Stone, Tile = grSand, Life = 15 };

        sut.Id.Should().Be(3);
        sut.Mat.Should().BeSameAs(Stone);
        sut.Tile.Should().BeSameAs(grSand);
        sut.Life.Should().Be(15);
        sut.IsTree.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Tree ground (grtree equivalent)
    // --------------------------------------------------------------------------

    // grtree = {id=4, gr=2, mat=wood, tile=grgrass, life=8, istree=true, pal={1,5,3,11}}
    [Fact]
    public void GrTreeEquivalent_HasIsTreeAndPal()
    {
        var grGrass = new GroundType(2, 2);
        var pal = new[] { 1, 5, 3, 11 };
        var sut = new GroundType(4, 2) { Mat = Wood, Tile = grGrass, Life = 8, IsTree = true, Pal = pal };

        sut.IsTree.Should().BeTrue();
        sut.Pal.Should().BeSameAs(pal);
        sut.Life.Should().Be(8);
    }

    // --------------------------------------------------------------------------
    #endregion
}
