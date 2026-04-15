using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Crafting;
using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Crafting;

public sealed class CraftingSystemTests
{
    // Helpers: all ingredient counts are vanilla integers (no Power)
    private static ItemStack Stack(ItemDef type, int count) => new(type, count);
    private static Recipe MakeRecipe(ItemDef result, int? count, int? power, params (ItemDef type, int qty)[] reqs)
    {
        var req = new List<ItemStack>(reqs.Length);
        foreach (var (t, q) in reqs)
            req.Add(Stack(t, q));
        return new Recipe(result, power, count, list: null, req);
    }

    // --------------------------------------------------------------------------
    #region CanCraft
    // --------------------------------------------------------------------------

    [Fact]
    public void CanCraft_ReturnsTrue_WhenAllIngredientsPresent()
    {
        var invent = new List<ItemStack> { Stack(PcraftData.Wood, 5) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    [Fact]
    public void CanCraft_ReturnsFalse_WhenIngredientMissing()
    {
        var invent = new List<ItemStack>();
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.CanCraft(invent, recipe).Should().BeFalse();
    }

    [Fact]
    public void CanCraft_ReturnsFalse_WhenNotEnoughCount()
    {
        var invent = new List<ItemStack> { Stack(PcraftData.Wood, 3) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.CanCraft(invent, recipe).Should().BeFalse();
    }

    [Fact]
    public void CanCraft_ReturnsTrue_WhenPlayerHasMoreThanRequired()
    {
        var invent = new List<ItemStack> { Stack(PcraftData.Wood, 10) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    [Fact]
    public void CanCraft_ReturnsFalse_WhenOneOfMultipleIngredientsMissing()
    {
        var invent = new List<ItemStack> { Stack(PcraftData.Wood, 5) };
        var recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null,
            (PcraftData.Wood, 5), (PcraftData.Stone, 10));

        CraftingSystem.CanCraft(invent, recipe).Should().BeFalse();
    }

    [Fact]
    public void CanCraft_ReturnsTrue_WhenAllMultipleIngredientsPresent()
    {
        var invent = new List<ItemStack>
        {
            Stack(PcraftData.Wood,  15),
            Stack(PcraftData.Stone, 10)
        };
        var recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null,
            (PcraftData.Wood, 15), (PcraftData.Stone, 10));

        CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    [Fact]
    public void CanCraft_RespectsPower_WhenRecipeRequiresPoweredItem()
    {
        // Stone-tier haxe (power=2) requires a wood-tier haxe (power=1) as ingredient
        var poweredHaxe = new ItemStack(PcraftData.Haxe, count: 1) { Power = 1 };
        var invent = new List<ItemStack> { poweredHaxe };

        var req = new List<ItemStack> { new(PcraftData.Haxe, count: 1) { Power = 1 } };
        var recipe = new Recipe(PcraftData.Haxe, power: 2, count: 1, list: null, req);

        CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    [Fact]
    public void CanCraft_ReturnsFalse_WhenIngredientHasWrongPower()
    {
        // Has an untiered haxe (power=null), but recipe requires wood-tier haxe (power=1)
        var unpoweredHaxe = Stack(PcraftData.Haxe, 1);
        var invent = new List<ItemStack> { unpoweredHaxe };

        var req = new List<ItemStack> { new(PcraftData.Haxe, count: 1) { Power = 1 } };
        var recipe = new Recipe(PcraftData.Haxe, power: 2, count: 1, list: null, req);

        CraftingSystem.CanCraft(invent, recipe).Should().BeFalse();
    }

    [Fact]
    public void CanCraft_ReturnsTrue_WhenRecipeHasNoIngredients()
    {
        var invent = new List<ItemStack>();
        var recipe = new Recipe(PcraftData.Wood, power: null, count: 1, list: null, req: []);

        CraftingSystem.CanCraft(invent, recipe).Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Craft
    // --------------------------------------------------------------------------

    [Fact]
    public void Craft_RemovesIngredients_FromInventory()
    {
        var invent = new List<ItemStack> { Stack(PcraftData.Wood, 5) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().NotContain(s => s.Type == PcraftData.Wood);
    }

    [Fact]
    public void Craft_RemovesExactCount_LeavingRemainder()
    {
        var invent = new List<ItemStack> { Stack(PcraftData.Wood, 10) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Wood)
              .Which.Count.Should().Be(5);
    }

    [Fact]
    public void Craft_AddsResult_AtFrontOfInventory()
    {
        var invent = new List<ItemStack>
        {
            Stack(PcraftData.Stone, 10),
            Stack(PcraftData.Wood,  15)
        };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: null, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        invent[0].Type.Should().Be(PcraftData.Haxe);
    }

    [Fact]
    public void Craft_SetsCount_OnResult()
    {
        var invent = new List<ItemStack> { Stack(PcraftData.Sand, 10) };
        var recipe = MakeRecipe(PcraftData.Glass, count: 3, power: null, (PcraftData.Sand, 3));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Glass)
              .Which.Count.Should().Be(3);
    }

    [Fact]
    public void Craft_SetsNullCount_WhenRecipeCountIsNull()
    {
        var invent = new List<ItemStack> { Stack(PcraftData.Wood, 15) };
        var recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null, (PcraftData.Wood, 15));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Workbench)
              .Which.Count.Should().BeNull();
    }

    [Fact]
    public void Craft_SetsPower_OnResult()
    {
        var invent = new List<ItemStack> { Stack(PcraftData.Wood, 5) };
        var recipe = MakeRecipe(PcraftData.Haxe, count: 1, power: 1, (PcraftData.Wood, 5));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Haxe)
              .Which.Power.Should().Be(1);
    }

    [Fact]
    public void Craft_SetsNullPower_WhenRecipePowerIsNull()
    {
        var invent = new List<ItemStack> { Stack(PcraftData.Wood, 15) };
        var recipe = MakeRecipe(PcraftData.Workbench, count: null, power: null, (PcraftData.Wood, 15));

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Workbench)
              .Which.Power.Should().BeNull();
    }

    [Fact]
    public void Craft_SetsRecipeList_OnResult()
    {
        var subList = new List<Recipe>();
        var req = new List<ItemStack> { Stack(PcraftData.Wood, 15) };
        var recipe = new Recipe(PcraftData.Workbench, power: null, count: null, list: subList, req);
        var invent = new List<ItemStack> { Stack(PcraftData.Wood, 15) };

        CraftingSystem.Craft(invent, recipe);

        invent.Should().Contain(s => s.Type == PcraftData.Workbench)
              .Which.List.Should().BeSameAs(subList);
    }

    [Fact]
    public void Craft_RemovesMultipleIngredients_AndAddsResult()
    {
        var invent = new List<ItemStack>
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
