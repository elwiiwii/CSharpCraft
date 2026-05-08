using System.Reflection;
using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class SplashScreenTests
{
    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_StoresSpr()
    {
        SplashScreen sut = new(spr: 128, lines: ["by nusan", "2016"]);
        _ = sut.Spr.Should().Be(128);
    }

    [Fact]
    public void Constructor_StoresLines()
    {
        SplashScreen sut = new(spr: 128, lines: ["by nusan", "2016"]);
        _ = sut.Lines.Should().Equal("by nusan", "2016");
    }

    [Fact]
    public void Type_HasNoTypeProperty()
    {
        PropertyInfo? prop = typeof(SplashScreen).GetProperty(
            "Type",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        _ = prop.Should().BeNull("SplashScreen no longer carries an ItemDef type reference");
    }

    [Fact]
    public void Type_DoesNotExpose_InteractiveListProperty()
    {
        PropertyInfo? prop = typeof(SplashScreen).GetProperty(
            "List",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        _ = prop.Should().BeNull("SplashScreen is splash-only");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Design contract
    // --------------------------------------------------------------------------

    [Fact]
    public void Type_DoesNotExpose_RecipeListProperty()
    {
        PropertyInfo? prop = typeof(SplashScreen).GetProperty(
            "RecipeList",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        _ = prop.Should().BeNull("crafting state lives on CraftingMenu");
    }

    [Fact]
    public void Type_DoesNotExpose_SelProperty()
    {
        PropertyInfo? prop = typeof(SplashScreen).GetProperty(
            "Sel",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        _ = prop.Should().BeNull("selection state lives on concrete interactive menus");
    }

    [Fact]
    public void Type_DoesNotExpose_OffProperty()
    {
        PropertyInfo? prop = typeof(SplashScreen).GetProperty(
            "Off",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        _ = prop.Should().BeNull("scroll state lives on concrete interactive menus");
    }

    [Fact]
    public void Type_DoesNotExpose_ToogleMenuProperty()
    {
        PropertyInfo? prop = typeof(SplashScreen).GetProperty(
            "ToogleMenu",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        _ = prop.Should().BeNull("tab-toggle state lives on ChestMenu");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region PcraftData static instances
    // --------------------------------------------------------------------------

    [Fact]
    public void MainMenu_HasExpectedLines()
    {
        _ = PcraftData.MainMenu.Lines.Should().Equal("by nusan", "2016");
    }

    [Fact]
    public void MainMenu_HasExpectedSpr()
    {
        _ = PcraftData.MainMenu.Spr.Should().Be(128);
    }

    [Fact]
    public void IntroMenu_HasExpectedLines()
    {
        _ = PcraftData.IntroMenu.Lines.Should().Equal("a storm leaved you", "on a deserted island");
    }

    [Fact]
    public void IntroMenu_HasExpectedSpr()
    {
        _ = PcraftData.IntroMenu.Spr.Should().Be(136);
    }

    [Fact]
    public void DeathMenu_HasExpectedLines()
    {
        _ = PcraftData.DeathMenu.Lines.Should().Equal("you died", "alone ...");
    }

    [Fact]
    public void DeathMenu_HasExpectedSpr()
    {
        _ = PcraftData.DeathMenu.Spr.Should().Be(128);
    }

    [Fact]
    public void WinMenu_HasExpectedLines()
    {
        _ = PcraftData.WinMenu.Lines.Should().Equal("you successfully escaped", "from the island");
    }

    [Fact]
    public void WinMenu_HasExpectedSpr()
    {
        _ = PcraftData.WinMenu.Spr.Should().Be(136);
    }

    [Fact]
    public void AllStaticInstances_AreDistinct()
    {
        SplashScreen[] all = new[] { PcraftData.MainMenu, PcraftData.IntroMenu, PcraftData.DeathMenu, PcraftData.WinMenu };
        _ = all.Should().OnlyHaveUniqueItems(because: "each screen represents a distinct game state");
    }

    // --------------------------------------------------------------------------
    #endregion
}
