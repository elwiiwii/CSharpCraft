using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class ChestItemDefTests
{
    [Fact]
    public void Constructor_InheritsNameSprBigSpr()
    {
        var sut = new ChestItemDef("chest", 92) { BigSpr = 110 };
        sut.Name.Should().Be("chest");
        sut.Spr.Should().Be(92);
        sut.BigSpr.Should().Be(110);
    }

    [Fact]
    public void IsChestItemDef_IsTrue_ForChest()
    {
        ItemDef sut = new ChestItemDef("chest", 92);
        (sut is ChestItemDef).Should().BeTrue();
    }

    [Fact]
    public void IsPlaceableItemDef_IsTrue_ForChest()
    {
        ItemDef sut = new ChestItemDef("chest", 92);
        (sut is PlaceableItemDef).Should().BeTrue();
    }

    [Fact]
    public void IsBenchItemDef_IsFalse_ForChest()
    {
        ItemDef sut = new ChestItemDef("chest", 92);
        (sut is BenchItemDef).Should().BeFalse();
    }
}
