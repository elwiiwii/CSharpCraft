using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Crafting;
using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Crafting;

public sealed class CraftingSystemTests
{
    private static StackableItem Stack(ItemDef type, int count)
    {
        return new(type, count);
    }

    private static Recipe MakeRecipe(ItemDef result, int? count, int? power, params (ItemDef type, int qty)[] reqs)
    {
        List<StackableItem> req = new(reqs.Length);
        foreach ((ItemDef? t, int q) in reqs)
            req.Add(new StackableItem(t, q));
        InventorySlot output = power.HasValue
            ? new ToolItem(result, power.Value)
            : count.HasValue
                ? new StackableItem(result, count.Value)
                : new UnstackableItem(result);
        return new Recipe(output, req);
    }

    // --------------------------------------------------------------------------
    #region CanCraft
    // --------------------------------------------------------------------------

    [Fact]
    public void CanCraft_ReturnsTrue_WhenAllIngredientsPresent()
    {
        List<InventorySlot> invent = [Stack(PcraftData.Wood, 5)];
        Recipe recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        _ = CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    [Fact]
    public void CanCraft_ReturnsFalse_WhenIngredientMissing()
    {
        List<InventorySlot> invent = [];
        Recipe recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        _ = CraftingSystem.CanCraft(invent, recipe).Should().BeFalse();
    }

    [Fact]
    public void CanCraft_ReturnsFalse_WhenNotEnoughCount()
    {
        List<InventorySlot> invent = [Stack(PcraftData.Wood, 3)];
        Recipe recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        _ = CraftingSystem.CanCraft(invent, recipe).Should().BeFalse();
    }

    [Fact]
    public void CanCraft_ReturnsTrue_WhenPlayerHasMoreThanRequired()
    {
        List<InventorySlot> invent = [Stack(PcraftData.Wood, 10)];
        Recipe recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        _ = CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    [Fact]
    public void CanCraft_ReturnsFalse_WhenOneOfMultipleIngredientsMissing()
    {
        List<InventorySlot> invent = [Stack(PcraftData.Wood, 5)];
        Recipe recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null,
            (PcraftData.Wood, 5), (PcraftData.Stone, 10));

        _ = CraftingSystem.CanCraft(invent, recipe).Should().BeFalse();
    }

    [Fact]
    public void CanCraft_ReturnsTrue_WhenAllMultipleIngredientsPresent()
    {
        List<InventorySlot> invent =
        [
            Stack(PcraftData.Wood,  15),
            Stack(PcraftData.Stone, 10)
        ];
        Recipe recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null,
            (PcraftData.Wood, 15), (PcraftData.Stone, 10));

        _ = CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    [Fact]
    public void CanCraft_ReturnsTrue_WhenRecipeHasNoIngredients()
    {
        List<InventorySlot> invent = [];
        Recipe recipe = new(new StackableItem(PcraftData.Wood, 1), []);

        _ = CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Craft
    // --------------------------------------------------------------------------

    [Fact]
    public void Craft_RemovesIngredients_FromInventory()
    {
        List<InventorySlot> invent = [Stack(PcraftData.Wood, 5)];
        Recipe recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        _ = invent.Should().NotContain(s => s.Type == PcraftData.Wood);
    }

    [Fact]
    public void Craft_RemovesExactCount_LeavingRemainder()
    {
        List<InventorySlot> invent = [Stack(PcraftData.Wood, 10)];
        Recipe recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        _ = invent.Should().Contain(s => s.Type == PcraftData.Wood)
              .Which.Should().BeOfType<StackableItem>().Which.Count.Should().Be(5);
    }

    [Fact]
    public void Craft_AddsResult_AtEndOfInventory()
    {
        List<InventorySlot> invent =
        [
            Stack(PcraftData.Stone, 10),
            Stack(PcraftData.Wood,  15)
        ];
        Recipe recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        _ = invent[^1].Type.Should().Be(PcraftData.Haxe);
    }

    [Fact]
    public void Craft_ProducesStackableItem_WhenCountIsSet()
    {
        List<InventorySlot> invent = [Stack(PcraftData.Sand, 10)];
        Recipe recipe = MakeRecipe(PcraftData.Glass, count: 3, power: null, (PcraftData.Sand, 3));

        CraftingSystem.Craft(invent, recipe);

        _ = invent.Should().Contain(s => s.Type == PcraftData.Glass)
              .Which.Should().BeOfType<StackableItem>().Which.Count.Should().Be(3);
    }

    [Fact]
    public void Craft_ProducesUnstackableItem_WhenCountAndPowerAreNull()
    {
        List<InventorySlot> invent = [Stack(PcraftData.Wood, 15)];
        Recipe recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null, (PcraftData.Wood, 15));

        CraftingSystem.Craft(invent, recipe);

        _ = invent.Should().Contain(s => s.Type == PcraftData.Workbench)
              .Which.Should().BeOfType<UnstackableItem>();
    }

    [Fact]
    public void Craft_ProducesToolItem_WhenPowerIsSet()
    {
        List<InventorySlot> invent = [Stack(PcraftData.Wood, 5)];
        Recipe recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: 1, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        _ = invent.Should().Contain(s => s.Type == PcraftData.Haxe)
              .Which.Should().BeOfType<ToolItem>().Which.Power.Should().Be(1);
    }

    [Fact]
    public void Craft_ProducesStackableItem_WhenCountIsSetAndPowerIsNull()
    {
        List<InventorySlot> invent = [Stack(PcraftData.Wood, 15)];
        Recipe recipe = MakeRecipe(PcraftData.Workbench, count: 1, power: null, (PcraftData.Wood, 15));

        CraftingSystem.Craft(invent, recipe);

        _ = invent.Should().Contain(s => s.Type == PcraftData.Workbench)
              .Which.Should().BeOfType<StackableItem>("count set, power null → StackableItem not ToolItem");
    }

    [Fact]
    public void Craft_RemovesMultipleIngredients_AndAddsResult()
    {
        List<InventorySlot> invent =
        [
            Stack(PcraftData.Wood,  15),
            Stack(PcraftData.Stone, 15)
        ];
        Recipe recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null,
            (PcraftData.Wood, 15), (PcraftData.Stone, 15));

        CraftingSystem.Craft(invent, recipe);

        _ = invent.Should().ContainSingle().Which.Type.Should().Be(PcraftData.Workbench);
    }

    // --------------------------------------------------------------------------
    #endregion

}
