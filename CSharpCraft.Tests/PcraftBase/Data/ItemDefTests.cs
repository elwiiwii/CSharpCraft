using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class ItemDefTests
{
    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_StoresName()
    {
        var sut = new ItemDef("wood", 103);
        sut.Name.Should().Be("wood");
    }

    [Fact]
    public void Constructor_StoresSpr()
    {
        var sut = new ItemDef("wood", 103);
        sut.Spr.Should().Be(103);
    }

    [Fact]
    public void Constructor_StoresPal_WhenProvided()
    {
        var pal = new[] { 1, 5, 3, 11 };
        var sut = new ItemDef("tree", 4, pal);
        sut.Pal.Should().BeSameAs(pal);
    }

    [Fact]
    public void Constructor_PalIsNull_WhenNotProvided()
    {
        var sut = new ItemDef("wood", 103);
        sut.Pal.Should().BeNull();
    }

    [Fact]
    public void Constructor_StoresBeCraft_WhenTrue()
    {
        var sut = new ItemDef("workbench", 89, null, beCraft: true);
        sut.BeCraft.Should().BeTrue();
    }

    [Fact]
    public void Constructor_BeCraftIsFalse_ByDefault()
    {
        var sut = new ItemDef("wood", 103);
        sut.BeCraft.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Mutable post-init properties
    // --------------------------------------------------------------------------

    [Fact]
    public void GiveLife_DefaultsToZero()
    {
        var sut = new ItemDef("apple", 116);
        sut.GiveLife.Should().Be(0);
    }

    [Fact]
    public void GiveLife_CanBeSetAfterConstruction()
    {
        var sut = new ItemDef("apple", 116);
        sut.GiveLife = 20;
        sut.GiveLife.Should().Be(20);
    }

    [Fact]
    public void BigSpr_DefaultsToZero()
    {
        var sut = new ItemDef("workbench", 89);
        sut.BigSpr.Should().Be(0);
    }

    [Fact]
    public void BigSpr_CanBeSetAfterConstruction()
    {
        var sut = new ItemDef("workbench", 89);
        sut.BigSpr = 104;
        sut.BigSpr.Should().Be(104);
    }

    [Fact]
    public void Drop_DefaultsToFalse()
    {
        var sut = new ItemDef("workbench", 89);
        sut.Drop.Should().BeFalse();
    }

    [Fact]
    public void Drop_CanBeSetAfterConstruction()
    {
        var sut = new ItemDef("workbench", 89);
        sut.Drop = true;
        sut.Drop.Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
}
