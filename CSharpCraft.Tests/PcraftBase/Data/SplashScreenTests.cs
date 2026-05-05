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
        var sut = new SplashScreen(spr: 128, lines: ["by nusan", "2016"]);
        sut.Spr.Should().Be(128);
    }

    [Fact]
    public void Constructor_StoresLines()
    {
        var sut = new SplashScreen(spr: 128, lines: ["by nusan", "2016"]);
        sut.Lines.Should().Equal("by nusan", "2016");
    }

    [Fact]
    public void Type_HasNoTypeProperty()
    {
        var prop = typeof(SplashScreen).GetProperty(
            "Type",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("SplashScreen no longer carries an ItemDef type reference");
    }

    [Fact]
    public void Type_DoesNotExpose_InteractiveListProperty()
    {
        var prop = typeof(SplashScreen).GetProperty(
            "List",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("SplashScreen is splash-only");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Design contract
    // --------------------------------------------------------------------------

    [Fact]
    public void Type_DoesNotExpose_RecipeListProperty()
    {
        var prop = typeof(SplashScreen).GetProperty(
            "RecipeList",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("crafting state lives on CraftingMenu");
    }

    [Fact]
    public void Type_DoesNotExpose_SelProperty()
    {
        var prop = typeof(SplashScreen).GetProperty(
            "Sel",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("selection state lives on concrete interactive menus");
    }

    [Fact]
    public void Type_DoesNotExpose_OffProperty()
    {
        var prop = typeof(SplashScreen).GetProperty(
            "Off",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("scroll state lives on concrete interactive menus");
    }

    [Fact]
    public void Type_DoesNotExpose_ToogleMenuProperty()
    {
        var prop = typeof(SplashScreen).GetProperty(
            "ToogleMenu",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("tab-toggle state lives on ChestMenu");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region PcraftData static instances
    // --------------------------------------------------------------------------

    [Fact]
    public void MainMenu_HasExpectedLines()
    {
        PcraftData.MainMenu.Lines.Should().Equal("by nusan", "2016");
    }

    [Fact]
    public void MainMenu_HasExpectedSpr()
    {
        PcraftData.MainMenu.Spr.Should().Be(128);
    }

    [Fact]
    public void IntroMenu_HasExpectedLines()
    {
        PcraftData.IntroMenu.Lines.Should().Equal("a storm leaved you", "on a deserted island");
    }

    [Fact]
    public void IntroMenu_HasExpectedSpr()
    {
        PcraftData.IntroMenu.Spr.Should().Be(136);
    }

    [Fact]
    public void DeathMenu_HasExpectedLines()
    {
        PcraftData.DeathMenu.Lines.Should().Equal("you died", "alone ...");
    }

    [Fact]
    public void DeathMenu_HasExpectedSpr()
    {
        PcraftData.DeathMenu.Spr.Should().Be(128);
    }

    [Fact]
    public void WinMenu_HasExpectedLines()
    {
        PcraftData.WinMenu.Lines.Should().Equal("you successfully escaped", "from the island");
    }

    [Fact]
    public void WinMenu_HasExpectedSpr()
    {
        PcraftData.WinMenu.Spr.Should().Be(136);
    }

    [Fact]
    public void AllStaticInstances_AreDistinct()
    {
        var all = new[] { PcraftData.MainMenu, PcraftData.IntroMenu, PcraftData.DeathMenu, PcraftData.WinMenu };
        all.Should().OnlyHaveUnstackableItems(because: "each screen represents a distinct game state");
    }

    // --------------------------------------------------------------------------
    #endregion
}
