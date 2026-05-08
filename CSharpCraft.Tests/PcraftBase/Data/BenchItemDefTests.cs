using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class BenchItemDefTests
{
    [Fact]
    public void Constructor_InheritsNameSprBigSpr()
    {
        BenchItemDef sut = new("workbench", 89, 104);
        _ = sut.Name.Should().Be("workbench");
        _ = sut.Spr.Should().Be(89);
        _ = sut.BigSpr.Should().Be(104);
    }

    [Fact]
    public void Recipes_DefaultsToEmpty()
    {
        BenchItemDef sut = new("furnace", 90, 106);
        _ = sut.Recipes.Should().BeEmpty();
    }

    [Fact]
    public void Recipes_CanBeAssigned()
    {
        BenchItemDef sut = new("furnace", 90, 106);
        List<Recipe> recipes = [];
        sut.Recipes = recipes;
        _ = sut.Recipes.Should().BeSameAs(recipes);
    }

    [Fact]
    public void IsBenchItemDef_IsTrue_ForBench()
    {
        ItemDef sut = new BenchItemDef("workbench", 89, 104);
        _ = (sut is BenchItemDef).Should().BeTrue();
    }

    [Fact]
    public void IsPlaceableItemDef_IsTrue_ForBench()
    {
        ItemDef sut = new BenchItemDef("workbench", 89, 104);
        _ = (sut is PlaceableItemDef).Should().BeTrue();
    }
}
