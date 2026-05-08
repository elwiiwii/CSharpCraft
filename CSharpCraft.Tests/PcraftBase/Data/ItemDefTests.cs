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
        ItemDef sut = new("wood", 103);
        _ = sut.Name.Should().Be("wood");
    }

    [Fact]
    public void Constructor_StoresSpr()
    {
        ItemDef sut = new("wood", 103);
        _ = sut.Spr.Should().Be(103);
    }

    [Fact]
    public void Constructor_StoresPal_WhenProvided()
    {
        int[] pal = new[] { 1, 5, 3, 11 };
        ItemDef sut = new("tree", 4, pal);
        _ = sut.Pal.Should().BeSameAs(pal);
    }

    [Fact]
    public void Constructor_PalIsNull_WhenNotProvided()
    {
        ItemDef sut = new("wood", 103);
        _ = sut.Pal.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
}
