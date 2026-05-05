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
        PcraftData.Furnace.Recipes.Should().HaveCount(4);
    }

    [Fact]
    public void FurnaceRecipes_ContainsIronBarFromThreeIron()
    {
        var entry = PcraftData.Furnace.Recipes.First(r => r.Output.Type == PcraftData.IronBar);
        entry.Output.Should().BeOfType<StackableItem>().Which.Count.Should().Be(1);
        entry.Req.Should().HaveCount(1);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Iron);
        entry.Req[0].Count.Should().Be(3);
    }

    [Fact]
    public void FurnaceRecipes_ContainsBreadFromFiveWheat()
    {
        var entry = PcraftData.Furnace.Recipes.First(r => r.Output.Type == PcraftData.Bread);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Wheat);
        entry.Req[0].Count.Should().Be(5);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Factory.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void FactoryRecipes_HasTwoEntries()
    {
        PcraftData.Factory.Recipes.Should().HaveCount(2);
    }

    [Fact]
    public void FactoryRecipes_ContainsSailFromThreeFabricAndOneGlue()
    {
        var entry = PcraftData.Factory.Recipes.First(r => r.Output.Type == PcraftData.Sail);
        entry.Output.Should().BeOfType<StackableItem>().Which.Count.Should().Be(1);
        entry.Req.Should().HaveCount(2);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Fabric);
        entry.Req[0].Count.Should().Be(3);
        entry.Req[1].Type.Should().BeSameAs(PcraftData.Glue);
        entry.Req[1].Count.Should().Be(1);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Chem.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void ChemRecipes_HasTwoEntries()
    {
        PcraftData.Chem.Recipes.Should().HaveCount(2);
    }

    [Fact]
    public void ChemRecipes_ContainsGlueFromOneGlassAndThreeIchor()
    {
        var entry = PcraftData.Chem.Recipes.First(r => r.Output.Type == PcraftData.Glue);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Glass);
        entry.Req[0].Count.Should().Be(1);
        entry.Req[1].Type.Should().BeSameAs(PcraftData.Ichor);
        entry.Req[1].Count.Should().Be(3);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Workbench.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void WorkbenchRecipes_HasTenEntries()
    {
        PcraftData.Workbench.Recipes.Should().HaveCount(10);
    }

    [Fact]
    public void WorkbenchRecipes_ContainsWoodHaxe_WithPowerOneAndFiveWood()
    {
        var entry = PcraftData.Workbench.Recipes.First(r =>
            r.Output.Type == PcraftData.Haxe && (r.Output as ToolItem)?.Power == 1);
        entry.Req.Should().HaveCount(1);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Wood);
        entry.Req[0].Count.Should().Be(5);
    }

    [Fact]
    public void WorkbenchRecipes_ContainsWoodSword_WithPowerOneAndSevenWood()
    {
        var entry = PcraftData.Workbench.Recipes.First(r =>
            r.Output.Type == PcraftData.Sword && (r.Output as ToolItem)?.Power == 1);
        entry.Req[0].Count.Should().Be(7);
    }

    [Fact]
    public void WorkbenchRecipes_ContainsWorkbenchCraft_WithFifteenWood()
    {
        var entry = PcraftData.Workbench.Recipes.First(r => r.Output.Type == PcraftData.Workbench);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Wood);
        entry.Req[0].Count.Should().Be(15);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Stonebench.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void StonebenchRecipes_HasSevenEntries()
    {
        PcraftData.Stonebench.Recipes.Should().HaveCount(7);
    }

    [Fact]
    public void StonebenchRecipes_ContainsStoneSword_WithPowerTwoAndSevenStone()
    {
        var entry = PcraftData.Stonebench.Recipes.First(r =>
            r.Output.Type == PcraftData.Sword && (r.Output as ToolItem)?.Power == 2);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Stone);
        entry.Req[0].Count.Should().Be(7);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Anvil.Recipes
    // --------------------------------------------------------------------------

    [Fact]
    public void AnvilRecipes_HasFifteenEntries()
    {
        PcraftData.Anvil.Recipes.Should().HaveCount(15);
    }

    [Fact]
    public void AnvilRecipes_ContainsGemSword_WithPowerFiveAndTwentyOneGem()
    {
        var entry = PcraftData.Anvil.Recipes.First(r =>
            r.Output.Type == PcraftData.Sword && (r.Output as ToolItem)?.Power == 5);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Gem);
        entry.Req[0].Count.Should().Be(21);
    }

    [Fact]
    public void AnvilRecipes_ContainsGemHaxe_WithPowerFiveAndFifteenGem()
    {
        var entry = PcraftData.Anvil.Recipes.First(r =>
            r.Output.Type == PcraftData.Haxe && (r.Output as ToolItem)?.Power == 5);
        entry.Req[0].Count.Should().Be(15);
    }

    [Fact]
    public void AnvilRecipes_ContainsIronHaxe_WithPowerThreeAndFiveIronBar()
    {
        var entry = PcraftData.Anvil.Recipes.First(r =>
            r.Output.Type == PcraftData.Haxe && (r.Output as ToolItem)?.Power == 3);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.IronBar);
        entry.Req[0].Count.Should().Be(5);
    }

    // --------------------------------------------------------------------------
    #endregion
}

