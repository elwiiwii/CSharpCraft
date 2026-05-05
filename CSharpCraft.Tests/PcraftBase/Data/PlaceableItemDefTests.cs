using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class PlaceableItemDefTests
{
    [Fact]
    public void BigSpr_StoredFromConstructor()
    {
        var sut = new PlaceableItemDefStub("chest", 92, 110);
        sut.BigSpr.Should().Be(110);
    }

    [Fact]
    public void Drop_PropertyDoesNotExist()
    {
        var prop = typeof(PlaceableItemDef).GetProperty(
            "Drop",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("Drop was removed; PlaceableItemDef instances are always placeable by definition");
    }

    // Concrete subclass for testing the abstract parent
    private sealed class PlaceableItemDefStub(string name, int spr, int bigSpr) : PlaceableItemDef(name, spr, bigSpr);
}
