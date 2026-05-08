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
        StackableItem sut = new(Wood, 5);
        _ = sut.Type.Should().BeSameAs(Wood);
    }

    [Fact]
    public void Constructor_StoresCount()
    {
        StackableItem sut = new(Wood, 30);
        _ = sut.Count.Should().Be(30);
    }

    [Fact]
    public void Count_CanBeMutated()
    {
        StackableItem sut = new(Wood, 5)
        {
            Count = 3
        };
        _ = sut.Count.Should().Be(3);
    }

    [Fact]
    public void IsSubclassOfInventorySlot()
    {
        StackableItem sut = new(Wood, 1);
        _ = sut.Should().BeAssignableTo<InventorySlot>();
    }
}
