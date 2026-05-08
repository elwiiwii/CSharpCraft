using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase;

public sealed class BenchRecipesTests
{
    // --------------------------------------------------------------------------
    #region Furnace.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void FurnaceRecipes_HasFourEntries()
    {
        _ = PcraftData.Furnace.Recipes.Should().HaveCount(4);
    }

    [Fact]
    public void FurnaceRecipes_ContainsIronBarFromThreeIron()
    {
        Recipe entry = PcraftData.Furnace.Recipes.First(r => r.Output.Type == PcraftData.IronBar);
        _ = entry.Output.Should().BeOfType<StackableItem>().Which.Count.Should().Be(1);
        _ = entry.Req.Should().HaveCount(1);
        _ = entry.Req[0].Type.Should().BeSameAs(PcraftData.Iron);
        _ = entry.Req[0].Count.Should().Be(3);
    }

    [Fact]
    public void FurnaceRecipes_ContainsBreadFromFiveWheat()
    {
        Recipe entry = PcraftData.Furnace.Recipes.First(r => r.Output.Type == PcraftData.Bread);
        _ = entry.Req[0].Type.Should().BeSameAs(PcraftData.Wheat);
        _ = entry.Req[0].Count.Should().Be(5);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Factory.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void FactoryRecipes_HasTwoEntries()
    {
        _ = PcraftData.Factory.Recipes.Should().HaveCount(2);
    }

    [Fact]
    public void FactoryRecipes_ContainsSailFromThreeFabricAndOneGlue()
    {
        Recipe entry = PcraftData.Factory.Recipes.First(r => r.Output.Type == PcraftData.Sail);
        _ = entry.Output.Should().BeOfType<StackableItem>().Which.Count.Should().Be(1);
        _ = entry.Req.Should().HaveCount(2);
        _ = entry.Req[0].Type.Should().BeSameAs(PcraftData.Fabric);
        _ = entry.Req[0].Count.Should().Be(3);
        _ = entry.Req[1].Type.Should().BeSameAs(PcraftData.Glue);
        _ = entry.Req[1].Count.Should().Be(1);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Chem.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void ChemRecipes_HasTwoEntries()
    {
        _ = PcraftData.Chem.Recipes.Should().HaveCount(2);
    }

    [Fact]
    public void ChemRecipes_ContainsGlueFromOneGlassAndThreeIchor()
    {
        Recipe entry = PcraftData.Chem.Recipes.First(r => r.Output.Type == PcraftData.Glue);
        _ = entry.Req[0].Type.Should().BeSameAs(PcraftData.Glass);
        _ = entry.Req[0].Count.Should().Be(1);
        _ = entry.Req[1].Type.Should().BeSameAs(PcraftData.Ichor);
        _ = entry.Req[1].Count.Should().Be(3);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Workbench.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void WorkbenchRecipes_HasTenEntries()
    {
        _ = PcraftData.Workbench.Recipes.Should().HaveCount(10);
    }

    [Fact]
    public void WorkbenchRecipes_ContainsWoodHaxe_WithPowerOneAndFiveWood()
    {
        Recipe entry = PcraftData.Workbench.Recipes.First(r =>
            r.Output.Type == PcraftData.Haxe && (r.Output as ToolItem)?.Power == 1);
        _ = entry.Req.Should().HaveCount(1);
        _ = entry.Req[0].Type.Should().BeSameAs(PcraftData.Wood);
        _ = entry.Req[0].Count.Should().Be(5);
    }

    [Fact]
    public void WorkbenchRecipes_ContainsWoodSword_WithPowerOneAndSevenWood()
    {
        Recipe entry = PcraftData.Workbench.Recipes.First(r =>
            r.Output.Type == PcraftData.Sword && (r.Output as ToolItem)?.Power == 1);
        _ = entry.Req[0].Count.Should().Be(7);
    }

    [Fact]
    public void WorkbenchRecipes_ContainsWorkbenchCraft_WithFifteenWood()
    {
        Recipe entry = PcraftData.Workbench.Recipes.First(r => r.Output.Type == PcraftData.Workbench);
        _ = entry.Req[0].Type.Should().BeSameAs(PcraftData.Wood);
        _ = entry.Req[0].Count.Should().Be(15);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Stonebench.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void StonebenchRecipes_HasSevenEntries()
    {
        _ = PcraftData.Stonebench.Recipes.Should().HaveCount(7);
    }

    [Fact]
    public void StonebenchRecipes_ContainsStoneSword_WithPowerTwoAndSevenStone()
    {
        Recipe entry = PcraftData.Stonebench.Recipes.First(r =>
            r.Output.Type == PcraftData.Sword && (r.Output as ToolItem)?.Power == 2);
        _ = entry.Req[0].Type.Should().BeSameAs(PcraftData.Stone);
        _ = entry.Req[0].Count.Should().Be(7);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Anvil.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void AnvilRecipes_HasFifteenEntries()
    {
        _ = PcraftData.Anvil.Recipes.Should().HaveCount(15);
    }

    [Fact]
    public void AnvilRecipes_ContainsGemSword_WithPowerFiveAndTwentyOneGem()
    {
        Recipe entry = PcraftData.Anvil.Recipes.First(r =>
            r.Output.Type == PcraftData.Sword && (r.Output as ToolItem)?.Power == 5);
        _ = entry.Req[0].Type.Should().BeSameAs(PcraftData.Gem);
        _ = entry.Req[0].Count.Should().Be(21);
    }

    [Fact]
    public void AnvilRecipes_ContainsGemHaxe_WithPowerFiveAndFifteenGem()
    {
        Recipe entry = PcraftData.Anvil.Recipes.First(r =>
            r.Output.Type == PcraftData.Haxe && (r.Output as ToolItem)?.Power == 5);
        _ = entry.Req[0].Count.Should().Be(15);
    }

    [Fact]
    public void AnvilRecipes_ContainsIronHaxe_WithPowerThreeAndFiveIronBar()
    {
        Recipe entry = PcraftData.Anvil.Recipes.First(r =>
            r.Output.Type == PcraftData.Haxe && (r.Output as ToolItem)?.Power == 3);
        _ = entry.Req[0].Type.Should().BeSameAs(PcraftData.IronBar);
        _ = entry.Req[0].Count.Should().Be(5);
    }

    // --------------------------------------------------------------------------
    #endregion
}

