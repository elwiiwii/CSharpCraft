using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class BenchItemDefTests
{
    [Fact]
    public void Constructor_InheritsNameSprBigSpr()
    {
        var sut = new BenchItemDef("workbench", 89, 104);
        sut.Name.Should().Be("workbench");
        sut.Spr.Should().Be(89);
        sut.BigSpr.Should().Be(104);
    }

    [Fact]
    public void Recipes_DefaultsToEmpty()
    {
        var sut = new BenchItemDef("furnace", 90, 106);
        sut.Recipes.Should().BeEmpty();
    }

    [Fact]
    public void Recipes_CanBeAssigned()
    {
        var sut = new BenchItemDef("furnace", 90, 106);
        var recipes = new List<Recipe>();
        sut.Recipes = recipes;
        sut.Recipes.Should().BeSameAs(recipes);
    }

    [Fact]
    public void IsBenchItemDef_IsTrue_ForBench()
    {
        ItemDef sut = new BenchItemDef("workbench", 89, 104);
        (sut is BenchItemDef).Should().BeTrue();
    }

    [Fact]
    public void IsPlaceableItemDef_IsTrue_ForBench()
    {
        ItemDef sut = new BenchItemDef("workbench", 89, 104);
        (sut is PlaceableItemDef).Should().BeTrue();
    }
}
