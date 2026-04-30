using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class PlaceableItemDefTests
{
    [Fact]
    public void BigSpr_DefaultsToZero()
    {
        var sut = new PlaceableItemDefStub("chest", 92);
        sut.BigSpr.Should().Be(0);
    }

    [Fact]
    public void BigSpr_StoredViaInit()
    {
        var sut = new PlaceableItemDefStub("chest", 92) { BigSpr = 110 };
        sut.BigSpr.Should().Be(110);
    }

    [Fact]
    public void Drop_IsFalse_WhenBigSprIsZero()
    {
        var sut = new PlaceableItemDefStub("chest", 92);
        sut.Drop.Should().BeFalse();
    }

    [Fact]
    public void Drop_IsTrue_WhenBigSprIsNonZero()
    {
        var sut = new PlaceableItemDefStub("chest", 92) { BigSpr = 110 };
        sut.Drop.Should().BeTrue();
    }

    // Concrete subclass for testing the abstract parent
    private sealed class PlaceableItemDefStub(string name, int spr) : PlaceableItemDef(name, spr);
}
