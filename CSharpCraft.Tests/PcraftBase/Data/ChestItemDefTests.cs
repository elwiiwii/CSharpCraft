using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class ChestItemDefTests
{
    [Fact]
    public void Constructor_InheritsNameSprBigSpr()
    {
        ChestItemDef sut = new("chest", 92, 110);
        _ = sut.Name.Should().Be("chest");
        _ = sut.Spr.Should().Be(92);
        _ = sut.BigSpr.Should().Be(110);
    }

    [Fact]
    public void IsChestItemDef_IsTrue_ForChest()
    {
        ItemDef sut = new ChestItemDef("chest", 92, 110);
        _ = (sut is ChestItemDef).Should().BeTrue();
    }

    [Fact]
    public void IsPlaceableItemDef_IsTrue_ForChest()
    {
        ItemDef sut = new ChestItemDef("chest", 92, 110);
        _ = (sut is PlaceableItemDef).Should().BeTrue();
    }

    [Fact]
    public void IsBenchItemDef_IsFalse_ForChest()
    {
        ItemDef sut = new ChestItemDef("chest", 92, 110);
        _ = (sut is BenchItemDef).Should().BeFalse();
    }
}
