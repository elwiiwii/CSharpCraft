using CSharpCraft.Pcraft;
using CSharpCraft.Pcraft.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pcraft;

public sealed class InitRecipesTests
{
    private readonly PcraftGame _sut = new();

    public InitRecipesTests()
    {
        _sut.InitRecipes();
    }

    // --------------------------------------------------------------------------
    #region FurnaceRecipe
    // --------------------------------------------------------------------------

    [Fact]
    public void FurnaceRecipe_HasFourEntries()
    {
        _sut.FurnaceRecipe.Should().HaveCount(4);
    }

    [Fact]
    public void FurnaceRecipe_ContainsIronBarFromThreeIron()
    {
        var entry = _sut.FurnaceRecipe.First(r => r.Type == PcraftData.IronBar);
        entry.Count.Should().Be(1);
        entry.Req.Should().HaveCount(1);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Iron);
        entry.Req[0].Count.Should().Be(3);
    }

    [Fact]
    public void FurnaceRecipe_ContainsBreadFromFiveWheat()
    {
        var entry = _sut.FurnaceRecipe.First(r => r.Type == PcraftData.Bread);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Wheat);
        entry.Req[0].Count.Should().Be(5);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region FactoryRecipe
    // --------------------------------------------------------------------------

    [Fact]
    public void FactoryRecipe_HasTwoEntries()
    {
        _sut.FactoryRecipe.Should().HaveCount(2);
    }

    [Fact]
    public void FactoryRecipe_ContainsSailFromThreeFabricAndOneGlue()
    {
        var entry = _sut.FactoryRecipe.First(r => r.Type == PcraftData.Sail);
        entry.Count.Should().Be(1);
        entry.Req.Should().HaveCount(2);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Fabric);
        entry.Req[0].Count.Should().Be(3);
        entry.Req[1].Type.Should().BeSameAs(PcraftData.Glue);
        entry.Req[1].Count.Should().Be(1);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region ChemRecipe
    // --------------------------------------------------------------------------

    [Fact]
    public void ChemRecipe_HasTwoEntries()
    {
        _sut.ChemRecipe.Should().HaveCount(2);
    }

    [Fact]
    public void ChemRecipe_ContainsGlueFromOneGlassAndThreeIchor()
    {
        var entry = _sut.ChemRecipe.First(r => r.Type == PcraftData.Glue);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Glass);
        entry.Req[0].Count.Should().Be(1);
        entry.Req[1].Type.Should().BeSameAs(PcraftData.Ichor);
        entry.Req[1].Count.Should().Be(3);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region WorkbenchRecipe
    // --------------------------------------------------------------------------

    [Fact]
    public void WorkbenchRecipe_HasTenEntries()
    {
        _sut.WorkbenchRecipe.Should().HaveCount(10);
    }

    [Fact]
    public void WorkbenchRecipe_ContainsWoodHaxe_WithPowerOneAndFiveWood()
    {
        var entry = _sut.WorkbenchRecipe.First(r => r.Type == PcraftData.Haxe && r.Power == 1);
        entry.Req.Should().HaveCount(1);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Wood);
        entry.Req[0].Count.Should().Be(5);
    }

    [Fact]
    public void WorkbenchRecipe_ContainsWoodSword_WithPowerOneAndSevenWood()
    {
        var entry = _sut.WorkbenchRecipe.First(r => r.Type == PcraftData.Sword && r.Power == 1);
        entry.Req[0].Count.Should().Be(7);
    }

    [Fact]
    public void WorkbenchRecipe_ContainsWorkbenchCraft_WithRecipeListAndFifteenWood()
    {
        var entry = _sut.WorkbenchRecipe.First(r => r.Type == PcraftData.Workbench);
        entry.List.Should().BeSameAs(_sut.WorkbenchRecipe);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Wood);
        entry.Req[0].Count.Should().Be(15);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region StonebenchRecipe
    // --------------------------------------------------------------------------

    [Fact]
    public void StonebenchRecipe_HasSevenEntries()
    {
        _sut.StonebenchRecipe.Should().HaveCount(7);
    }

    [Fact]
    public void StonebenchRecipe_ContainsStoneSword_WithPowerTwoAndSevenStone()
    {
        var entry = _sut.StonebenchRecipe.First(r => r.Type == PcraftData.Sword && r.Power == 2);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Stone);
        entry.Req[0].Count.Should().Be(7);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region AnvilRecipe
    // --------------------------------------------------------------------------

    [Fact]
    public void AnvilRecipe_HasFifteenEntries()
    {
        _sut.AnvilRecipe.Should().HaveCount(15);
    }

    [Fact]
    public void AnvilRecipe_ContainsGemSword_WithPowerFiveAndTwentyOneGem()
    {
        var entry = _sut.AnvilRecipe.First(r => r.Type == PcraftData.Sword && r.Power == 5);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.Gem);
        entry.Req[0].Count.Should().Be(21);
    }

    [Fact]
    public void AnvilRecipe_ContainsGemHaxe_WithPowerFiveAndFifteenGem()
    {
        var entry = _sut.AnvilRecipe.First(r => r.Type == PcraftData.Haxe && r.Power == 5);
        entry.Req[0].Count.Should().Be(15);
    }

    [Fact]
    public void AnvilRecipe_ContainsIronHaxe_WithPowerThreeAndFiveIronBar()
    {
        var entry = _sut.AnvilRecipe.First(r => r.Type == PcraftData.Haxe && r.Power == 3);
        entry.Req[0].Type.Should().BeSameAs(PcraftData.IronBar);
        entry.Req[0].Count.Should().Be(5);
    }

    // --------------------------------------------------------------------------
    #endregion
}
