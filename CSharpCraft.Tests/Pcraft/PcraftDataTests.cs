using CSharpCraft.Pcraft;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pcraft;

public sealed class PcraftDataTests
{
    // --------------------------------------------------------------------------
    #region Item singletons — name and spr
    // --------------------------------------------------------------------------

    [Fact]
    public void Wood_HasCorrectNameAndSpr()
    {
        PcraftData.Wood.Name.Should().Be("wood");
        PcraftData.Wood.Spr.Should().Be(103);
    }

    [Fact]
    public void Haxe_HasCorrectNameAndSpr()
    {
        PcraftData.Haxe.Name.Should().Be("haxe");
        PcraftData.Haxe.Spr.Should().Be(98);
    }

    [Fact]
    public void IronBar_HasCorrectNameAndSpr()
    {
        PcraftData.IronBar.Name.Should().Be("iron bar");
        PcraftData.IronBar.Spr.Should().Be(119);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GiveLife post-init mutations
    // --------------------------------------------------------------------------

    [Fact]
    public void Apple_GiveLifeIsTwenty()
    {
        PcraftData.Apple.GiveLife.Should().Be(20);
    }

    [Fact]
    public void Potion_GiveLifeIsOneHundred()
    {
        PcraftData.Potion.GiveLife.Should().Be(100);
    }

    [Fact]
    public void Bread_GiveLifeIsForty()
    {
        PcraftData.Bread.GiveLife.Should().Be(40);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region bigspr buildings
    // --------------------------------------------------------------------------

    [Fact]
    public void Workbench_HasBigSpr104AndDropTrue()
    {
        PcraftData.Workbench.BigSpr.Should().Be(104);
        PcraftData.Workbench.Drop.Should().BeTrue();
    }

    [Fact]
    public void Workbench_IsBeCraft()
    {
        PcraftData.Workbench.BeCraft.Should().BeTrue();
    }

    [Fact]
    public void Furnace_HasBigSpr106()
    {
        PcraftData.Furnace.BigSpr.Should().Be(106);
        PcraftData.Furnace.Drop.Should().BeTrue();
    }

    [Fact]
    public void Chest_HasBigSpr110AndDropTrue()
    {
        PcraftData.Chest.BigSpr.Should().Be(110);
        PcraftData.Chest.Drop.Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Ground types
    // --------------------------------------------------------------------------

    [Fact]
    public void GrRock_HasCorrectIdAndLife()
    {
        PcraftData.GrRock.Id.Should().Be(3);
        PcraftData.GrRock.Life.Should().Be(15);
    }

    [Fact]
    public void GrRock_MatIsStone()
    {
        PcraftData.GrRock.Mat.Should().BeSameAs(PcraftData.Stone);
    }

    [Fact]
    public void GrRock_TileIsGrSand()
    {
        PcraftData.GrRock.Tile.Should().BeSameAs(PcraftData.GrSand);
    }

    [Fact]
    public void GrTree_IsTreeIsTrueAndPalIsCorrect()
    {
        PcraftData.GrTree.IsTree.Should().BeTrue();
        PcraftData.GrTree.Pal.Should().Equal(1, 5, 3, 11);
    }

    [Fact]
    public void GrGem_HasLife160()
    {
        PcraftData.GrGem.Life.Should().Be(160);
    }

    [Fact]
    public void Grounds_HasTwelveEntries()
    {
        PcraftData.Grounds.Should().HaveCount(12);
    }

    [Fact]
    public void Grounds_FirstIsGrWater()
    {
        PcraftData.Grounds[0].Should().BeSameAs(PcraftData.GrWater);
    }

    [Fact]
    public void Grounds_LastIsGrHole()
    {
        PcraftData.Grounds[11].Should().BeSameAs(PcraftData.GrHole);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Menus
    // --------------------------------------------------------------------------

    [Fact]
    public void MainMenu_HasSpr128AndCorrectText()
    {
        PcraftData.MainMenu.Spr.Should().Be(128);
        PcraftData.MainMenu.Text.Should().Be("by nusan");
        PcraftData.MainMenu.Text2.Should().Be("2016");
    }

    [Fact]
    public void MainMenu_IsSplashOnly_MenuState()
    {
        var type = PcraftData.MainMenu.GetType();
        type.GetProperty("Sel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Should().BeNull();
        type.GetProperty("Off", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Should().BeNull();
    }

    [Fact]
    public void DeathMenu_HasCorrectText()
    {
        PcraftData.DeathMenu.Text.Should().Be("you died");
        PcraftData.DeathMenu.Text2.Should().Be("alone ...");
    }

    [Fact]
    public void WinMenu_HasSpr136()
    {
        PcraftData.WinMenu.Spr.Should().Be(136);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Power metadata
    // --------------------------------------------------------------------------

    [Fact]
    public void PwrNames_HasFiveEntries()
    {
        PcraftData.PwrNames.Should().Equal("wood", "stone", "iron", "gold", "gem");
    }

    [Fact]
    public void PwrPal_HasFiveRowsOfFour()
    {
        PcraftData.PwrPal.Should().HaveCount(5);
        PcraftData.PwrPal[0].Should().Equal(2, 2, 4, 4);
        PcraftData.PwrPal[4].Should().Equal(13, 2, 14, 12);
    }

    // --------------------------------------------------------------------------
    #endregion
}
