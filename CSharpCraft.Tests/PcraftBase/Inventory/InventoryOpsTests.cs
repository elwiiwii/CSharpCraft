using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Inventory;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Inventory;

public sealed class InventoryOpsTests
{
    private static readonly ItemDef Wood   = new("wood",  103);
    private static readonly ItemDef Stone  = new("stone", 118);
    private static readonly ItemDef Sword  = new("sword",  99);

    // --------------------------------------------------------------------------
    #region HowMany
    // --------------------------------------------------------------------------

    [Fact]
    public void HowMany_ReturnsZero_WhenListIsEmpty()
    {
        var list = new List<InventorySlot>();
        InventoryOps.HowMany(list, new StackableItem(Wood, 1)).Should().Be(0);
    }

    [Fact]
    public void HowMany_ReturnsCount_ForCountedItem()
    {
        var list = new List<InventorySlot> { new StackableItem(Wood, 15) };
        InventoryOps.HowMany(list, new StackableItem(Wood, 1)).Should().Be(15);
    }

    [Fact]
    public void HowMany_SumsAcrossMultipleMatchingSlots()
    {
        var list = new List<InventorySlot>
        {
            new StackableItem(Wood, 10),
            new StackableItem(Wood, 5),
        };
        InventoryOps.HowMany(list, new StackableItem(Wood, 1)).Should().Be(15);
    }

    [Fact]
    public void HowMany_IgnoresNonMatchingType()
    {
        var list = new List<InventorySlot> { new StackableItem(Stone, 10) };
        InventoryOps.HowMany(list, new StackableItem(Wood, 1)).Should().Be(0);
    }

    [Fact]
    public void HowMany_CountsToolItemOfSameType_AsOne()
    {
        var list = new List<InventorySlot> { new ToolItem(Sword, 1) };
        InventoryOps.HowMany(list, new StackableItem(Sword, 1)).Should().Be(1);
    }

    [Fact]
    public void HowMany_CountsUnstackableItemOfSameType_AsOne()
    {
        var list = new List<InventorySlot> { new UnstackableItem(Wood) };
        InventoryOps.HowMany(list, new StackableItem(Wood, 1)).Should().Be(1);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region FindStackable
    // --------------------------------------------------------------------------

    [Fact]
    public void FindStackable_ReturnsNull_WhenListIsEmpty()
    {
        var list = new List<InventorySlot>();
        InventoryOps.FindStackable(list, new StackableItem(Wood, 1)).Should().BeNull();
    }

    [Fact]
    public void FindStackable_ReturnsNull_WhenNoMatchingType()
    {
        var list = new List<InventorySlot> { new StackableItem(Stone, 5) };
        InventoryOps.FindStackable(list, new StackableItem(Wood, 1)).Should().BeNull();
    }

    [Fact]
    public void FindStackable_ReturnsSlot_WhenTypeMatches()
    {
        var slot = new StackableItem(Wood, 5);
        var list = new List<InventorySlot> { slot };
        InventoryOps.FindStackable(list, new StackableItem(Wood, 1)).Should().BeSameAs(slot);
    }

    [Fact]
    public void FindStackable_IgnoresToolItemOfSameType()
    {
        var list = new List<InventorySlot> { new ToolItem(Sword, 1) };
        InventoryOps.FindStackable(list, new StackableItem(Sword, 1)).Should().BeNull();
    }

    [Fact]
    public void FindStackable_IgnoresUnstackableItemOfSameType()
    {
        var list = new List<InventorySlot> { new UnstackableItem(Wood) };
        InventoryOps.FindStackable(list, new StackableItem(Wood, 1)).Should().BeNull();
    }

    [Fact]
    public void FindStackable_ReturnsFirst_WhenMultipleSlotsMatch()
    {
        var first  = new StackableItem(Wood, 5);
        var second = new StackableItem(Wood, 3);
        var list = new List<InventorySlot> { first, second };
        InventoryOps.FindStackable(list, new StackableItem(Wood, 1)).Should().BeSameAs(first);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region RemInList
    // --------------------------------------------------------------------------

    [Fact]
    public void RemInList_DoesNothing_WhenItemNotPresent()
    {
        var list = new List<InventorySlot> { new StackableItem(Stone, 5) };
        InventoryOps.RemInList(list, new StackableItem(Wood, 1));
        list.Should().HaveCount(1);
    }

    [Fact]
    public void RemInList_DecrementsCount_WhenSufficientQuantity()
    {
        var slot = new StackableItem(Wood, 5);
        var list = new List<InventorySlot> { slot };

        InventoryOps.RemInList(list, new StackableItem(Wood, 2));

        list.Should().HaveCount(1);
        slot.Count.Should().Be(3);
    }

    [Fact]
    public void RemInList_RemovesSlot_WhenCountReachesZero()
    {
        var list = new List<InventorySlot> { new StackableItem(Wood, 3) };
        InventoryOps.RemInList(list, new StackableItem(Wood, 3));
        list.Should().BeEmpty();
    }

    [Fact]
    public void RemInList_RemovesSlot_WhenCountGoesNegative()
    {
        var list = new List<InventorySlot> { new StackableItem(Wood, 2) };
        InventoryOps.RemInList(list, new StackableItem(Wood, 5));
        list.Should().BeEmpty();
    }

    [Fact]
    public void RemInList_RemovesUnstackableItem_ByType()
    {
        var item = new UnstackableItem(Sword);
        var list = new List<InventorySlot> { item };

        InventoryOps.RemInList(list, new UnstackableItem(Sword));

        list.Should().BeEmpty();
    }

    [Fact]
    public void RemInList_RemovesToolItem_ByType()
    {
        var item = new ToolItem(Sword, 1);
        var list = new List<InventorySlot> { item };

        InventoryOps.RemInList(list, new ToolItem(Sword, 2));

        list.Should().BeEmpty();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region AddItemInList
    // --------------------------------------------------------------------------

    [Fact]
    public void AddItemInList_AppendsItem_WhenListIsEmpty()
    {
        var list = new List<InventorySlot>();
        var item = new StackableItem(Wood, 5);

        InventoryOps.AddItemInList(list, item, pos: 0);

        list.Should().ContainSingle().Which.Should().BeSameAs(item);
    }

    [Fact]
    public void AddItemInList_StacksOntoExistingCountedItem()
    {
        var existing = new StackableItem(Wood, 10);
        var list = new List<InventorySlot> { existing };

        InventoryOps.AddItemInList(list, new StackableItem(Wood, 5), pos: 0);

        list.Should().HaveCount(1);
        existing.Count.Should().Be(15);
    }

    [Fact]
    public void AddItemInList_NeverMerges_ToolItem()
    {
        var existing = new ToolItem(Sword, 1);
        var list = new List<InventorySlot> { existing };
        var newItem = new ToolItem(Sword, 2);

        InventoryOps.AddItemInList(list, newItem, pos: 0);

        list.Should().HaveCount(2);
    }

    [Fact]
    public void AddItemInList_NeverMerges_UnstackableItem()
    {
        var existing = new UnstackableItem(Sword);
        var list = new List<InventorySlot> { existing };
        var newItem = new UnstackableItem(Sword);

        InventoryOps.AddItemInList(list, newItem, pos: 0);

        list.Should().HaveCount(2);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Loop
    // --------------------------------------------------------------------------

    [Fact]
    public void Loop_ReturnsSameValue_WhenInRange()
    {
        InventoryOps.Loop(1, 3).Should().Be(1);
    }

    [Fact]
    public void Loop_WrapsForward_WhenPastEnd()
    {
        InventoryOps.Loop(3, 3).Should().Be(0);
    }

    [Fact]
    public void Loop_WrapsBackward_WhenNegativeOne()
    {
        InventoryOps.Loop(-1, 3).Should().Be(2);
    }

    [Fact]
    public void Loop_ReturnsMaxIndex_WhenSelIsMaxIndex()
    {
        InventoryOps.Loop(2, 3).Should().Be(2);
    }

    [Fact]
    public void Loop_WrapsBackward_WhenNegative()
    {
        InventoryOps.Loop(-2, 3).Should().Be(1);
    }

    // --------------------------------------------------------------------------
    #endregion
}
