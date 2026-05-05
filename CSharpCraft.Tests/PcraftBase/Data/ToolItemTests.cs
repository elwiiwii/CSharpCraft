using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class ToolItemTests
{
    private static readonly ItemDef Haxe = new("haxe", 99);

    [Fact]
    public void Constructor_StoresType()
    {
        var sut = new ToolItem(Haxe, 1);
        sut.Type.Should().BeSameAs(Haxe);
    }

    [Fact]
    public void Constructor_StoresPower()
    {
        var sut = new ToolItem(Haxe, 3);
        sut.Power.Should().Be(3);
    }

    [Fact]
    public void IsSubclassOfInventorySlot()
    {
        var sut = new ToolItem(Haxe, 1);
        sut.Should().BeAssignableTo<InventorySlot>();
    }
}
