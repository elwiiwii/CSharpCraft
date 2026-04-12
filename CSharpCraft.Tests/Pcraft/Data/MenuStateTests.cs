using CSharpCraft.Pcraft.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Data;

public sealed class MenuStateTests
{
    private static readonly ItemDef Inventary = new("inventory", 89);

    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_StoresType()
    {
        var sut = new MenuState(Inventary, spr: 128, text: "by nusan", text2: "2016");
        sut.Type.Should().BeSameAs(Inventary);
    }

    [Fact]
    public void Constructor_StoresSpr()
    {
        var sut = new MenuState(Inventary, spr: 128, text: null, text2: null);
        sut.Spr.Should().Be(128);
    }

    [Fact]
    public void Constructor_StoresText()
    {
        var sut = new MenuState(Inventary, spr: 128, text: "by nusan", text2: "2016");
        sut.Text.Should().Be("by nusan");
        sut.Text2.Should().Be("2016");
    }

    [Fact]
    public void Type_DoesNotExpose_InteractiveListProperty()
    {
        var prop = typeof(MenuState).GetProperty(
            "List",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("MenuState is splash-only in Phase 5");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Design contract
    // --------------------------------------------------------------------------

    [Fact]
    public void Type_DoesNotExpose_RecipeListProperty()
    {
        var prop = typeof(MenuState).GetProperty(
            "RecipeList",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("crafting state now lives on CraftingMenu");
    }

    [Fact]
    public void Type_DoesNotExpose_SelProperty()
    {
        var prop = typeof(MenuState).GetProperty(
            "Sel",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("selection state now lives on concrete interactive menus");
    }

    [Fact]
    public void Type_DoesNotExpose_OffProperty()
    {
        var prop = typeof(MenuState).GetProperty(
            "Off",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("scroll state now lives on concrete interactive menus");
    }

    [Fact]
    public void Type_DoesNotExpose_ToogleMenuProperty()
    {
        var prop = typeof(MenuState).GetProperty(
            "ToogleMenu",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("tab-toggle state now lives on ChestMenu");
    }

    // --------------------------------------------------------------------------
    #endregion
}
