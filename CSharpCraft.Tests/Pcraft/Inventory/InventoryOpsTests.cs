using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Inventory;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Inventory;

public sealed class InventoryOpsTests
{
    // Shared ItemDef instances (treated as singletons, matching PcraftData pattern)
    private static readonly ItemDef Wood   = new("wood",  103);
    private static readonly ItemDef Stone  = new("stone", 118);
    private static readonly ItemDef Sword  = new("sword",  99);

    // --------------------------------------------------------------------------
    #region HowMany
    // --------------------------------------------------------------------------

    [Fact]
    public void HowMany_ReturnsZero_WhenListIsEmpty()
    {
        var list = new List<ItemStack>();
        InventoryOps.HowMany(list, new ItemStack(Wood)).Should().Be(0);
    }

    [Fact]
    public void HowMany_ReturnsOne_ForSingleUncountedMatchingItem()
    {
        // inst(wood) in Lua — no Count
        var list = new List<ItemStack> { new ItemStack(Wood) };
        InventoryOps.HowMany(list, new ItemStack(Wood)).Should().Be(1);
    }

    [Fact]
    public void HowMany_ReturnsCount_ForCountedItem()
    {
        var list = new List<ItemStack> { new ItemStack(Wood, count: 15) };
        InventoryOps.HowMany(list, new ItemStack(Wood)).Should().Be(15);
    }

    [Fact]
    public void HowMany_SumsAcrossMultipleMatchingSlots()
    {
        var list = new List<ItemStack>
        {
            new ItemStack(Wood, count: 10),
            new ItemStack(Wood, count: 5),
        };
        InventoryOps.HowMany(list, new ItemStack(Wood)).Should().Be(15);
    }

    [Fact]
    public void HowMany_IgnoresNonMatchingType()
    {
        var list = new List<ItemStack> { new ItemStack(Stone, count: 10) };
        InventoryOps.HowMany(list, new ItemStack(Wood)).Should().Be(0);
    }

    [Fact]
    public void HowMany_FiltersByPower_WhenQueryHasPower()
    {
        var sword2 = new ItemStack(Sword) { Power = 2 };
        var sword3 = new ItemStack(Sword) { Power = 3 };
        var list = new List<ItemStack> { sword2, sword3 };

        var query = new ItemStack(Sword) { Power = 2 };
        InventoryOps.HowMany(list, query).Should().Be(1);
    }

    [Fact]
    public void HowMany_CountsAllPowers_WhenQueryHasNoPower()
    {
        var sword1 = new ItemStack(Sword) { Power = 1 };
        var sword3 = new ItemStack(Sword) { Power = 3 };
        var list = new List<ItemStack> { sword1, sword3 };

        var query = new ItemStack(Sword); // Power is null
        InventoryOps.HowMany(list, query).Should().Be(2);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region IsInList
    // --------------------------------------------------------------------------

    [Fact]
    public void IsInList_ReturnsNull_WhenListIsEmpty()
    {
        var list = new List<ItemStack>();
        InventoryOps.IsInList(list, new ItemStack(Wood)).Should().BeNull();
    }

    [Fact]
    public void IsInList_ReturnsSlot_WhenTypeMatches()
    {
        var slot = new ItemStack(Wood, count: 5);
        var list = new List<ItemStack> { slot };

        InventoryOps.IsInList(list, new ItemStack(Wood)).Should().BeSameAs(slot);
    }

    [Fact]
    public void IsInList_ReturnsNull_WhenTypeDoesNotMatch()
    {
        var list = new List<ItemStack> { new ItemStack(Stone, count: 5) };
        InventoryOps.IsInList(list, new ItemStack(Wood)).Should().BeNull();
    }

    [Fact]
    public void IsInList_MatchesByPower_WhenQueryHasPower()
    {
        var sword1 = new ItemStack(Sword) { Power = 1 };
        var sword2 = new ItemStack(Sword) { Power = 2 };
        var list = new List<ItemStack> { sword1, sword2 };

        var query = new ItemStack(Sword) { Power = 2 };
        InventoryOps.IsInList(list, query).Should().BeSameAs(sword2);
    }

    [Fact]
    public void IsInList_ReturnsFirstMatch_WhenNoPowerFilter()
    {
        var sword1 = new ItemStack(Sword) { Power = 1 };
        var sword2 = new ItemStack(Sword) { Power = 2 };
        var list = new List<ItemStack> { sword1, sword2 };

        var query = new ItemStack(Sword); // no power
        InventoryOps.IsInList(list, query).Should().BeSameAs(sword1);
    }

    [Fact]
    public void IsInList_ReturnsNull_WhenPowerDoesNotMatch()
    {
        var list = new List<ItemStack> { new ItemStack(Sword) { Power = 1 } };
        var query = new ItemStack(Sword) { Power = 3 };
        InventoryOps.IsInList(list, query).Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region RemInList
    // --------------------------------------------------------------------------

    [Fact]
    public void RemInList_DoesNothing_WhenItemNotPresent()
    {
        var list = new List<ItemStack> { new ItemStack(Stone, count: 5) };
        InventoryOps.RemInList(list, new ItemStack(Wood, count: 1));
        list.Should().HaveCount(1);
    }

    [Fact]
    public void RemInList_DecrementsCount_WhenSufficientQuantity()
    {
        var slot = new ItemStack(Wood, count: 5);
        var list = new List<ItemStack> { slot };

        InventoryOps.RemInList(list, new ItemStack(Wood, count: 2));

        list.Should().HaveCount(1);
        slot.Count.Should().Be(3);
    }

    [Fact]
    public void RemInList_RemovesSlot_WhenCountReachesZero()
    {
        var list = new List<ItemStack> { new ItemStack(Wood, count: 3) };
        InventoryOps.RemInList(list, new ItemStack(Wood, count: 3));
        list.Should().BeEmpty();
    }

    [Fact]
    public void RemInList_RemovesSlot_WhenCountGoesNegative()
    {
        // Lua: count -= elem.count; if count <= 0 then del
        var list = new List<ItemStack> { new ItemStack(Wood, count: 2) };
        InventoryOps.RemInList(list, new ItemStack(Wood, count: 5));
        list.Should().BeEmpty();
    }

    [Fact]
    public void RemInList_RemovesUncountedSlot()
    {
        // inst(sword) — no Count
        var slot = new ItemStack(Sword);
        var list = new List<ItemStack> { slot };

        InventoryOps.RemInList(list, new ItemStack(Sword));

        list.Should().BeEmpty();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region AddItemInList
    // --------------------------------------------------------------------------

    [Fact]
    public void AddItemInList_AppendsItem_WhenListIsEmpty()
    {
        var list = new List<ItemStack>();
        var item = new ItemStack(Wood, count: 5);

        InventoryOps.AddItemInList(list, item, pos: 0);

        list.Should().ContainSingle().Which.Should().BeSameAs(item);
    }

    [Fact]
    public void AddItemInList_StacksOntoExistingCountedItem()
    {
        var existing = new ItemStack(Wood, count: 10);
        var list = new List<ItemStack> { existing };

        InventoryOps.AddItemInList(list, new ItemStack(Wood, count: 5), pos: 0);

        list.Should().HaveCount(1);
        existing.Count.Should().Be(15);
    }

    [Fact]
    public void AddItemInList_InsertsSeparately_WhenMatchingSlotIsUncounted()
    {
        // An uncounted sword (no Count) — should not stack: insert instead
        var existing = new ItemStack(Sword);
        var list = new List<ItemStack> { existing };
        var newItem = new ItemStack(Sword);

        InventoryOps.AddItemInList(list, newItem, pos: 0);

        list.Should().HaveCount(2);
    }

    [Fact]
    public void AddItemInList_InsertsSeparately_WhenDifferentPower()
    {
        // power=1 and power=2 swords must not stack
        var sword1 = new ItemStack(Sword) { Power = 1 };
        var list = new List<ItemStack> { sword1 };
        var sword2 = new ItemStack(Sword) { Power = 2 };

        InventoryOps.AddItemInList(list, sword2, pos: 0);

        list.Should().HaveCount(2);
    }

    [Fact]
    public void AddItemInList_StacksOntoMatchingPoweredItem()
    {
        var existing = new ItemStack(Wood, count: 8) { Power = 2 };
        var list = new List<ItemStack> { existing };

        InventoryOps.AddItemInList(list, new ItemStack(Wood, count: 3) { Power = 2 }, pos: 0);

        list.Should().HaveCount(1);
        existing.Count.Should().Be(11);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region AddPlace
    // --------------------------------------------------------------------------

    [Fact]
    public void AddPlace_InsertsAtFront_WhenPosIsZero()
    {
        var a = new ItemStack(Wood);
        var b = new ItemStack(Stone);
        var list = new List<ItemStack> { a };

        InventoryOps.AddPlace(list, b, pos: 0);

        list.Should().Equal(b, a);
    }

    [Fact]
    public void AddPlace_AppendsItem_WhenPosIsNegative()
    {
        var a = new ItemStack(Wood);
        var b = new ItemStack(Stone);
        var list = new List<ItemStack> { a };

        InventoryOps.AddPlace(list, b, pos: -1);

        list.Should().Equal(a, b);
    }

    [Fact]
    public void AddPlace_AppendsItem_WhenPosEqualsCount()
    {
        // pos >= list.Count → append (0-indexed: pos=1 == count=1 fails the insert condition)
        var a = new ItemStack(Wood);
        var b = new ItemStack(Stone);
        var list = new List<ItemStack> { a };

        InventoryOps.AddPlace(list, b, pos: 1); // 1 == list.Count → append

        list.Should().Equal(a, b);
    }

    [Fact]
    public void AddPlace_InsertsAtFront_WhenPosIsZeroAndListHasMultipleItems()
    {
        var a = new ItemStack(Wood);
        var b = new ItemStack(Stone);
        var inserted = new ItemStack(Sword);
        var list = new List<ItemStack> { a, b };

        InventoryOps.AddPlace(list, inserted, pos: 0);

        list.Should().Equal(inserted, a, b);
    }

    [Fact]
    public void AddPlace_InsertsMidList_AtCorrectPosition()
    {
        var a = new ItemStack(Wood);
        var b = new ItemStack(Stone);
        var c = new ItemStack(Sword);
        var inserted = new ItemStack(new ItemDef("gem", 118));
        var list = new List<ItemStack> { a, b, c };

        InventoryOps.AddPlace(list, inserted, pos: 1);

        // pos=1 (0-indexed) → inserts at index 1: [a, inserted, b, c]
        list.Should().Equal(a, inserted, b, c);
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
