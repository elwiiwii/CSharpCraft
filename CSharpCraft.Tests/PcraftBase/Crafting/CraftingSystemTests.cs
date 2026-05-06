using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Crafting;
using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Crafting;

public sealed class CraftingSystemTests
{
    private static StackableItem Stack(ItemDef type, int count) => new(type, count);
    private static Recipe MakeRecipe(ItemDef result, int? count, int? power, params (ItemDef type, int qty)[] reqs)
    {
        var req = new List<StackableItem>(reqs.Length);
        foreach (var (t, q) in reqs)
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
        var invent = new List<InventorySlot> { Stack(PcraftData.Wood, 5) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    [Fact]
    public void CanCraft_ReturnsFalse_WhenIngredientMissing()
    {
        var invent = new List<InventorySlot>();
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.CanCraft(invent, recipe).Should().BeFalse();
    }

    [Fact]
    public void CanCraft_ReturnsFalse_WhenNotEnoughCount()
    {
        var invent = new List<InventorySlot> { Stack(PcraftData.Wood, 3) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.CanCraft(invent, recipe).Should().BeFalse();
    }

    [Fact]
    public void CanCraft_ReturnsTrue_WhenPlayerHasMoreThanRequired()
    {
        var invent = new List<InventorySlot> { Stack(PcraftData.Wood, 10) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    [Fact]
    public void CanCraft_ReturnsFalse_WhenOneOfMultipleIngredientsMissing()
    {
        var invent = new List<InventorySlot> { Stack(PcraftData.Wood, 5) };
        var recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null,
            (PcraftData.Wood, 5), (PcraftData.Stone, 10));

        CraftingSystem.CanCraft(invent, recipe).Should().BeFalse();
    }

    [Fact]
    public void CanCraft_ReturnsTrue_WhenAllMultipleIngredientsPresent()
    {
        var invent = new List<InventorySlot>
        {
            Stack(PcraftData.Wood,  15),
            Stack(PcraftData.Stone, 10)
        };
        var recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null,
            (PcraftData.Wood, 15), (PcraftData.Stone, 10));

        CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    [Fact]
    public void CanCraft_ReturnsTrue_WhenRecipeHasNoIngredients()
    {
        var invent = new List<InventorySlot>();
        var recipe = new Recipe(new StackableItem(PcraftData.Wood, 1), []);

        CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Craft
    // --------------------------------------------------------------------------

    [Fact]
    public void Craft_RemovesIngredients_FromInventory()
    {
        var invent = new List<InventorySlot> { Stack(PcraftData.Wood, 5) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().NotContain(s => s.Type == PcraftData.Wood);
    }

    [Fact]
    public void Craft_RemovesExactCount_LeavingRemainder()
    {
        var invent = new List<InventorySlot> { Stack(PcraftData.Wood, 10) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Wood)
              .Which.Should().BeOfType<StackableItem>().Which.Count.Should().Be(5);
    }

    [Fact]
    public void Craft_AddsResult_AtEndOfInventory()
    {
        var invent = new List<InventorySlot>
        {
            Stack(PcraftData.Stone, 10),
            Stack(PcraftData.Wood,  15)
        };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        invent[^1].Type.Should().Be(PcraftData.Haxe);
    }

    [Fact]
    public void Craft_ProducesStackableItem_WhenCountIsSet()
    {
        var invent = new List<InventorySlot> { Stack(PcraftData.Sand, 10) };
        var recipe = MakeRecipe(PcraftData.Glass, count: 3, power: null, (PcraftData.Sand, 3));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Glass)
              .Which.Should().BeOfType<StackableItem>().Which.Count.Should().Be(3);
    }

    [Fact]
    public void Craft_ProducesUnstackableItem_WhenCountAndPowerAreNull()
    {
        var invent = new List<InventorySlot> { Stack(PcraftData.Wood, 15) };
        var recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null, (PcraftData.Wood, 15));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Workbench)
              .Which.Should().BeOfType<UnstackableItem>();
    }

    [Fact]
    public void Craft_ProducesToolItem_WhenPowerIsSet()
    {
        var invent = new List<InventorySlot> { Stack(PcraftData.Wood, 5) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: 1, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Haxe)
              .Which.Should().BeOfType<ToolItem>().Which.Power.Should().Be(1);
    }

    [Fact]
    public void Craft_ProducesStackableItem_WhenCountIsSetAndPowerIsNull()
    {
        var invent = new List<InventorySlot> { Stack(PcraftData.Wood, 15) };
        var recipe = MakeRecipe(PcraftData.Workbench, count: 1, power: null, (PcraftData.Wood, 15));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Workbench)
              .Which.Should().BeOfType<StackableItem>("count set, power null → StackableItem not ToolItem");
    }

    [Fact]
    public void Craft_RemovesMultipleIngredients_AndAddsResult()
    {
        var invent = new List<InventorySlot>
        {
            Stack(PcraftData.Wood,  15),
            Stack(PcraftData.Stone, 15)
        };
        var recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null,
            (PcraftData.Wood, 15), (PcraftData.Stone, 15));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().ContainSingle().Which.Type.Should().Be(PcraftData.Workbench);
    }

    // --------------------------------------------------------------------------
    #endregion

}
