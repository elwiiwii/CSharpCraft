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
        UnstackableItem sut = new(Workbench);
        _ = sut.Type.Should().BeSameAs(Workbench);
    }

    [Fact]
    public void IsSubclassOfInventorySlot()
    {
        UnstackableItem sut = new(Workbench);
        _ = sut.Should().BeAssignableTo<InventorySlot>();
    }
}
