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

    // --------------------------------------------------------------------------
    #endregion
}
