using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class RecipeTests
{
    private static readonly ItemDef IronBar = new("iron bar", 119);
    private static readonly ItemDef Iron = new("iron", 118);
    private static readonly ItemDef Gem = new("gem", 118);
    private static readonly ItemDef Sword = new("sword", 99);

    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_StoresOutput_AsStackableItem()
    {
        var sut = new Recipe(new StackableItem(IronBar, 1), []);
        sut.Output.Should().BeOfType<StackableItem>()
            .Which.Type.Should().BeSameAs(IronBar);
    }

    [Fact]
    public void Constructor_StoresOutput_AsToolItem()
    {
        var sut = new Recipe(new ToolItem(Sword, 5), []);
        sut.Output.Should().BeOfType<ToolItem>()
            .Which.Power.Should().Be(5);
    }

    [Fact]
    public void Constructor_StoresOutput_AsUnstackableItem()
    {
        var sut = new Recipe(new UnstackableItem(IronBar), []);
        sut.Output.Should().BeOfType<UnstackableItem>()
            .Which.Type.Should().BeSameAs(IronBar);
    }

    [Fact]
    public void Constructor_StoresReq()
    {
        var req = new List<StackableItem> { new(Iron, 3) };
        var sut = new Recipe(new StackableItem(IronBar, 1), req);
        sut.Req.Should().BeSameAs(req);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOutputIsNull()
    {
        var act = () => new Recipe(null!, []);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Pcraft recipe equivalences
    // --------------------------------------------------------------------------

    // recipe(instc(ironbar,1), {instc(iron,3)}) → output=StackableItem(IronBar,1), req=[{Iron,3}]
    [Fact]
    public void IronBarRecipe_HasCorrectOutputCountAndIngredient()
    {
        var req = new List<StackableItem> { new(Iron, 3) };
        var sut = new Recipe(new StackableItem(IronBar, 1), req);

        sut.Output.Should().BeOfType<StackableItem>()
            .Which.Count.Should().Be(1);
        sut.Output.Type.Should().BeSameAs(IronBar);
        sut.Req.Should().HaveCount(1);
        sut.Req[0].Type.Should().BeSameAs(Iron);
        sut.Req[0].Count.Should().Be(3);
    }

    // recipe(setpower(5, inst(sword)), {instc(gem, 21)}) → output=ToolItem(Sword,5), req=[{Gem,21}]
    [Fact]
    public void GemSwordRecipe_HasPowerFiveAndTwentyOneGems()
    {
        var req = new List<StackableItem> { new(Gem, 21) };
        var sut = new Recipe(new ToolItem(Sword, 5), req);

        sut.Output.Should().BeOfType<ToolItem>()
            .Which.Power.Should().Be(5);
        sut.Req[0].Type.Should().BeSameAs(Gem);
        sut.Req[0].Count.Should().Be(21);
    }

    // --------------------------------------------------------------------------
    #endregion
}
