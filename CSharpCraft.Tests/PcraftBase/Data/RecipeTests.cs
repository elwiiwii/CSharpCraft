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
    public void Constructor_StoresType()
    {
        var sut = new Recipe(IronBar, power: null, count: 1, list: null, req: []);
        sut.Type.Should().BeSameAs(IronBar);
    }

    [Fact]
    public void Constructor_StoresPower()
    {
        var sut = new Recipe(Sword, power: 5, count: null, list: null, req: []);
        sut.Power.Should().Be(5);
    }

    [Fact]
    public void Constructor_PowerIsNull_WhenNotProvided()
    {
        var sut = new Recipe(IronBar, power: null, count: 1, list: null, req: []);
        sut.Power.Should().BeNull();
    }

    [Fact]
    public void Constructor_StoresCount()
    {
        var sut = new Recipe(IronBar, power: null, count: 1, list: null, req: []);
        sut.Count.Should().Be(1);
    }

    [Fact]
    public void Constructor_StoresList()
    {
        var craftingList = new List<Recipe>();
        var sut = new Recipe(IronBar, power: null, count: null, list: craftingList, req: []);
        sut.List.Should().BeSameAs(craftingList);
    }

    [Fact]
    public void Constructor_StoresReq()
    {
        var req = new List<ItemStack> { new(Iron, count: 3) };
        var sut = new Recipe(IronBar, power: null, count: 1, list: null, req: req);
        sut.Req.Should().BeSameAs(req);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Pcraft recipe equivalences
    // --------------------------------------------------------------------------

    // recipe(instc(ironbar,1), {instc(iron,3)}) → type=IronBar, count=1, req=[{Iron,3}]
    [Fact]
    public void IronBarRecipe_HasCorrectTypeCountAndIngredient()
    {
        var req = new List<ItemStack> { new(Iron, count: 3) };
        var sut = new Recipe(IronBar, power: null, count: 1, list: null, req: req);

        sut.Type.Should().BeSameAs(IronBar);
        sut.Count.Should().Be(1);
        sut.Req.Should().HaveCount(1);
        sut.Req[0].Type.Should().BeSameAs(Iron);
        sut.Req[0].Count.Should().Be(3);
    }

    // recipe(setpower(5, inst(sword)), {instc(gem, 21)}) → power=5, req=[{Gem,21}]
    [Fact]
    public void GemSwordRecipe_HasPowerFiveAndTwentyOneGems()
    {
        var req = new List<ItemStack> { new(Gem, count: 21) };
        var sut = new Recipe(Sword, power: 5, count: null, list: null, req: req);

        sut.Power.Should().Be(5);
        sut.Req[0].Type.Should().BeSameAs(Gem);
        sut.Req[0].Count.Should().Be(21);
    }

    // --------------------------------------------------------------------------
    #endregion
}
