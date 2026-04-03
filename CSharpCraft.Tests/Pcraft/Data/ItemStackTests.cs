using CSharpCraft.Pcraft.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Data;

public sealed class ItemStackTests
{
    private static readonly ItemDef Wood = new("wood", 103);
    private static readonly ItemDef Sword = new("sword", 99);

    // --------------------------------------------------------------------------
    #region inst() equivalent — no count, no power
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_StoresType()
    {
        var sut = new ItemStack(Wood);
        sut.Type.Should().BeSameAs(Wood);
    }

    [Fact]
    public void Count_IsNull_ByDefault()
    {
        var sut = new ItemStack(Wood);
        sut.Count.Should().BeNull();
    }

    [Fact]
    public void Power_IsNull_ByDefault()
    {
        var sut = new ItemStack(Wood);
        sut.Power.Should().BeNull();
    }

    [Fact]
    public void List_IsNull_ByDefault()
    {
        var sut = new ItemStack(Wood);
        sut.List.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region instc() equivalent — with count
    // --------------------------------------------------------------------------

    [Fact]
    public void Count_IsSet_WhenProvidedAtConstruction()
    {
        var sut = new ItemStack(Wood, count: 30);
        sut.Count.Should().Be(30);
    }

    [Fact]
    public void Count_CanBeMutated()
    {
        var sut = new ItemStack(Wood, count: 5);
        sut.Count = 3;
        sut.Count.Should().Be(3);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region setpower() equivalent
    // --------------------------------------------------------------------------

    [Fact]
    public void Power_CanBeSetAfterConstruction()
    {
        var sut = new ItemStack(Sword);
        sut.Power = 2;
        sut.Power.Should().Be(2);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region crafting list
    // --------------------------------------------------------------------------

    [Fact]
    public void List_IsStored_WhenProvided()
    {
        var recipes = new List<Recipe>();
        var sut = new ItemStack(Wood, list: recipes);
        sut.List.Should().BeSameAs(recipes);
    }

    // --------------------------------------------------------------------------
    #endregion
}
