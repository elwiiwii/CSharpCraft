using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class StackableItemTests
{
    private static readonly ItemDef Wood = new("wood", 103);

    [Fact]
    public void Constructor_StoresType()
    {
        var sut = new StackableItem(Wood, 5);
        sut.Type.Should().BeSameAs(Wood);
    }

    [Fact]
    public void Constructor_StoresCount()
    {
        var sut = new StackableItem(Wood, 30);
        sut.Count.Should().Be(30);
    }

    [Fact]
    public void Count_CanBeMutated()
    {
        var sut = new StackableItem(Wood, 5);
        sut.Count = 3;
        sut.Count.Should().Be(3);
    }

    [Fact]
    public void IsSubclassOfInventorySlot()
    {
        var sut = new StackableItem(Wood, 1);
        sut.Should().BeAssignableTo<InventorySlot>();
    }
}
