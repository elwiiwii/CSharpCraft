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
        ToolItem sut = new(Haxe, 1);
        _ = sut.Type.Should().BeSameAs(Haxe);
    }

    [Fact]
    public void Constructor_StoresPower()
    {
        ToolItem sut = new(Haxe, 3);
        _ = sut.Power.Should().Be(3);
    }

    [Fact]
    public void IsSubclassOfInventorySlot()
    {
        ToolItem sut = new(Haxe, 1);
        _ = sut.Should().BeAssignableTo<InventorySlot>();
    }
}
