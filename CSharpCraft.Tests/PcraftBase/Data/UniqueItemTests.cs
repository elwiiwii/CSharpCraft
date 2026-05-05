using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class UnstackableItemTests
{
    private static readonly ItemDef Workbench = new("workbench", 80);

    [Fact]
    public void Constructor_StoresType()
    {
        var sut = new UnstackableItem(Workbench);
        sut.Type.Should().BeSameAs(Workbench);
    }

    [Fact]
    public void IsSubclassOfInventorySlot()
    {
        var sut = new UnstackableItem(Workbench);
        sut.Should().BeAssignableTo<InventorySlot>();
    }
}
