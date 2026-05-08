using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Inventory;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Inventory;

public sealed class InventoryOpsTests
{
    private static readonly ItemDef Wood = new("wood", 103);
    private static readonly ItemDef Stone = new("stone", 118);
    private static readonly ItemDef Sword = new("sword", 99);

    // --------------------------------------------------------------------------
    #region HowMany
    // --------------------------------------------------------------------------

    [Fact]
    public void HowMany_ReturnsZero_WhenListIsEmpty()
    {
        List<InventorySlot> list = [];
        _ = InventoryOps.HowMany(list, new StackableItem(Wood, 1)).Should().Be(0);
    }

    [Fact]
    public void HowMany_ReturnsCount_ForCountedItem()
    {
        List<InventorySlot> list = [new StackableItem(Wood, 15)];
        _ = InventoryOps.HowMany(list, new StackableItem(Wood, 1)).Should().Be(15);
    }

    [Fact]
    public void HowMany_SumsAcrossMultipleMatchingSlots()
    {
        List<InventorySlot> list =
        [
            new StackableItem(Wood, 10),
            new StackableItem(Wood, 5),
        ];
        _ = InventoryOps.HowMany(list, new StackableItem(Wood, 1)).Should().Be(15);
    }

    [Fact]
    public void HowMany_IgnoresNonMatchingType()
    {
        List<InventorySlot> list = [new StackableItem(Stone, 10)];
        _ = InventoryOps.HowMany(list, new StackableItem(Wood, 1)).Should().Be(0);
    }

    [Fact]
    public void HowMany_CountsToolItemOfSameType_AsOne()
    {
        List<InventorySlot> list = [new ToolItem(Sword, 1)];
        _ = InventoryOps.HowMany(list, new StackableItem(Sword, 1)).Should().Be(1);
    }

    [Fact]
    public void HowMany_CountsUnstackableItemOfSameType_AsOne()
    {
        List<InventorySlot> list = [new UnstackableItem(Wood)];
        _ = InventoryOps.HowMany(list, new StackableItem(Wood, 1)).Should().Be(1);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region FindStackable
    // --------------------------------------------------------------------------

    [Fact]
    public void FindStackable_ReturnsNull_WhenListIsEmpty()
    {
        List<InventorySlot> list = [];
        _ = InventoryOps.FindStackable(list, new StackableItem(Wood, 1)).Should().BeNull();
    }

    [Fact]
    public void FindStackable_ReturnsNull_WhenNoMatchingType()
    {
        List<InventorySlot> list = [new StackableItem(Stone, 5)];
        _ = InventoryOps.FindStackable(list, new StackableItem(Wood, 1)).Should().BeNull();
    }

    [Fact]
    public void FindStackable_ReturnsSlot_WhenTypeMatches()
    {
        StackableItem slot = new(Wood, 5);
        List<InventorySlot> list = [slot];
        _ = InventoryOps.FindStackable(list, new StackableItem(Wood, 1)).Should().BeSameAs(slot);
    }

    [Fact]
    public void FindStackable_IgnoresToolItemOfSameType()
    {
        List<InventorySlot> list = [new ToolItem(Sword, 1)];
        _ = InventoryOps.FindStackable(list, new StackableItem(Sword, 1)).Should().BeNull();
    }

    [Fact]
    public void FindStackable_IgnoresUnstackableItemOfSameType()
    {
        List<InventorySlot> list = [new UnstackableItem(Wood)];
        _ = InventoryOps.FindStackable(list, new StackableItem(Wood, 1)).Should().BeNull();
    }

    [Fact]
    public void FindStackable_ReturnsFirst_WhenMultipleSlotsMatch()
    {
        StackableItem first = new(Wood, 5);
        StackableItem second = new(Wood, 3);
        List<InventorySlot> list = [first, second];
        _ = InventoryOps.FindStackable(list, new StackableItem(Wood, 1)).Should().BeSameAs(first);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region RemInList
    // --------------------------------------------------------------------------

    [Fact]
    public void RemInList_DoesNothing_WhenItemNotPresent()
    {
        List<InventorySlot> list = [new StackableItem(Stone, 5)];
        InventoryOps.RemInList(list, new StackableItem(Wood, 1));
        _ = list.Should().HaveCount(1);
    }

    [Fact]
    public void RemInList_DecrementsCount_WhenSufficientQuantity()
    {
        StackableItem slot = new(Wood, 5);
        List<InventorySlot> list = [slot];

        InventoryOps.RemInList(list, new StackableItem(Wood, 2));

        _ = list.Should().HaveCount(1);
        _ = slot.Count.Should().Be(3);
    }

    [Fact]
    public void RemInList_RemovesSlot_WhenCountReachesZero()
    {
        List<InventorySlot> list = [new StackableItem(Wood, 3)];
        InventoryOps.RemInList(list, new StackableItem(Wood, 3));
        _ = list.Should().BeEmpty();
    }

    [Fact]
    public void RemInList_RemovesSlot_WhenCountGoesNegative()
    {
        List<InventorySlot> list = [new StackableItem(Wood, 2)];
        InventoryOps.RemInList(list, new StackableItem(Wood, 5));
        _ = list.Should().BeEmpty();
    }

    [Fact]
    public void RemInList_RemovesUnstackableItem_ByType()
    {
        UnstackableItem item = new(Sword);
        List<InventorySlot> list = [item];

        InventoryOps.RemInList(list, new UnstackableItem(Sword));

        _ = list.Should().BeEmpty();
    }

    [Fact]
    public void RemInList_RemovesToolItem_ByType()
    {
        ToolItem item = new(Sword, 1);
        List<InventorySlot> list = [item];

        InventoryOps.RemInList(list, new ToolItem(Sword, 2));

        _ = list.Should().BeEmpty();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region AddItemInList
    // --------------------------------------------------------------------------

    [Fact]
    public void AddItemInList_AppendsItem_WhenListIsEmpty()
    {
        List<InventorySlot> list = [];
        StackableItem item = new(Wood, 5);

        InventoryOps.AddItemInList(list, item, pos: 0);

        _ = list.Should().ContainSingle().Which.Should().BeSameAs(item);
    }

    [Fact]
    public void AddItemInList_StacksOntoExistingCountedItem()
    {
        StackableItem existing = new(Wood, 10);
        List<InventorySlot> list = [existing];

        InventoryOps.AddItemInList(list, new StackableItem(Wood, 5), pos: 0);

        _ = list.Should().HaveCount(1);
        _ = existing.Count.Should().Be(15);
    }

    [Fact]
    public void AddItemInList_NeverMerges_ToolItem()
    {
        ToolItem existing = new(Sword, 1);
        List<InventorySlot> list = [existing];
        ToolItem newItem = new(Sword, 2);

        InventoryOps.AddItemInList(list, newItem, pos: 0);

        _ = list.Should().HaveCount(2);
    }

    [Fact]
    public void AddItemInList_NeverMerges_UnstackableItem()
    {
        UnstackableItem existing = new(Sword);
        List<InventorySlot> list = [existing];
        UnstackableItem newItem = new(Sword);

        InventoryOps.AddItemInList(list, newItem, pos: 0);

        _ = list.Should().HaveCount(2);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Loop
    // --------------------------------------------------------------------------

    [Fact]
    public void Loop_ReturnsSameValue_WhenInRange()
    {
        _ = InventoryOps.Loop(1, 3).Should().Be(1);
    }

    [Fact]
    public void Loop_WrapsForward_WhenPastEnd()
    {
        _ = InventoryOps.Loop(3, 3).Should().Be(0);
    }

    [Fact]
    public void Loop_WrapsBackward_WhenNegativeOne()
    {
        _ = InventoryOps.Loop(-1, 3).Should().Be(2);
    }

    [Fact]
    public void Loop_ReturnsMaxIndex_WhenSelIsMaxIndex()
    {
        _ = InventoryOps.Loop(2, 3).Should().Be(2);
    }

    [Fact]
    public void Loop_WrapsBackward_WhenNegative()
    {
        _ = InventoryOps.Loop(-2, 3).Should().Be(1);
    }

    // --------------------------------------------------------------------------
    #endregion
}
