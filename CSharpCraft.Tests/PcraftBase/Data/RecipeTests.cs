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
        Recipe sut = new(new StackableItem(IronBar, 1), []);
        _ = sut.Output.Should().BeOfType<StackableItem>()
            .Which.Type.Should().BeSameAs(IronBar);
    }

    [Fact]
    public void Constructor_StoresOutput_AsToolItem()
    {
        Recipe sut = new(new ToolItem(Sword, 5), []);
        _ = sut.Output.Should().BeOfType<ToolItem>()
            .Which.Power.Should().Be(5);
    }

    [Fact]
    public void Constructor_StoresOutput_AsUnstackableItem()
    {
        Recipe sut = new(new UnstackableItem(IronBar), []);
        _ = sut.Output.Should().BeOfType<UnstackableItem>()
            .Which.Type.Should().BeSameAs(IronBar);
    }

    [Fact]
    public void Constructor_StoresReq()
    {
        List<StackableItem> req = [new(Iron, 3)];
        Recipe sut = new(new StackableItem(IronBar, 1), req);
        _ = sut.Req.Should().BeSameAs(req);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOutputIsNull()
    {
        Func<Recipe> act = () => new Recipe(null!, []);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Pcraft recipe equivalences
    // --------------------------------------------------------------------------

    // recipe(instc(ironbar,1), {instc(iron,3)}) → output=StackableItem(IronBar,1), req=[{Iron,3}]
    [Fact]
    public void IronBarRecipe_HasCorrectOutputCountAndIngredient()
    {
        List<StackableItem> req = [new(Iron, 3)];
        Recipe sut = new(new StackableItem(IronBar, 1), req);

        _ = sut.Output.Should().BeOfType<StackableItem>()
            .Which.Count.Should().Be(1);
        _ = sut.Output.Type.Should().BeSameAs(IronBar);
        _ = sut.Req.Should().HaveCount(1);
        _ = sut.Req[0].Type.Should().BeSameAs(Iron);
        _ = sut.Req[0].Count.Should().Be(3);
    }

    // recipe(setpower(5, inst(sword)), {instc(gem, 21)}) → output=ToolItem(Sword,5), req=[{Gem,21}]
    [Fact]
    public void GemSwordRecipe_HasPowerFiveAndTwentyOneGems()
    {
        List<StackableItem> req = [new(Gem, 21)];
        Recipe sut = new(new ToolItem(Sword, 5), req);

        _ = sut.Output.Should().BeOfType<ToolItem>()
            .Which.Power.Should().Be(5);
        _ = sut.Req[0].Type.Should().BeSameAs(Gem);
        _ = sut.Req[0].Count.Should().Be(21);
    }

    // --------------------------------------------------------------------------
    #endregion
}
