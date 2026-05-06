using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase;

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
    public void Workbench_HasBigSpr104()
    {
        PcraftData.Workbench.BigSpr.Should().Be(104);
    }

    [Fact]
    public void Workbench_IsBenchItemDef()
    {
        (PcraftData.Workbench is BenchItemDef).Should().BeTrue();
    }

    [Fact]
    public void Furnace_HasBigSpr106()
    {
        PcraftData.Furnace.BigSpr.Should().Be(106);
    }

    [Fact]
    public void Chest_HasBigSpr110()
    {
        PcraftData.Chest.BigSpr.Should().Be(110);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Tile types
    // --------------------------------------------------------------------------

    [Fact]
    public void TileRock_HasCorrectBlendGroupAndLife()
    {
        PcraftData.TileRock.BlendGroup.Should().Be(BlendGroup.Rock);
        PcraftData.TileRock.Life.Should().Be(15);
    }

    [Fact]
    public void TileRock_MatIsStone()
    {
        PcraftData.TileRock.Mat.Should().BeSameAs(PcraftData.Stone);
    }

    [Fact]
    public void TileRock_UnderlyingTypeIsTileSand()
    {
        PcraftData.TileRock.UnderlyingType.Should().BeSameAs(PcraftData.TileSand);
    }

    [Fact]
    public void TileTree_IsWallTileTypeWithSpritePal()
    {
        PcraftData.TileTree.Should().BeOfType<WallTileType>();
        PcraftData.TileTree.SpritePal.Should().Equal(1, 5, 3, 11);
    }

    [Fact]
    public void TileGem_HasLife160()
    {
        PcraftData.TileGem.Life.Should().Be(160);
    }

    [Fact]
    public void TileFor_Water_ReturnsTileWater()
    {
        PcraftData.TileFor(TileId.Water).Type.Should().BeSameAs(PcraftData.TileWater);
    }

    [Fact]
    public void TileFor_Hole_ReturnsTileHole()
    {
        PcraftData.TileFor(TileId.Hole).Type.Should().BeSameAs(PcraftData.TileHole);
    }

    [Fact]
    public void TileFor_GapId7_FallsBackToWater()
    {
        PcraftData.TileFor((TileId)7).Type.Should().BeSameAs(PcraftData.TileWater);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Menus
    // --------------------------------------------------------------------------

    [Fact]
    public void MainMenu_HasSpr128AndCorrectLines()
    {
        PcraftData.MainMenu.Spr.Should().Be(128);
        PcraftData.MainMenu.Lines[0].Should().Be("by nusan");
        PcraftData.MainMenu.Lines[1].Should().Be("2016");
    }

    [Fact]
    public void MainMenu_IsSplashOnly_SplashScreen()
    {
        var type = PcraftData.MainMenu.GetType();
        type.GetProperty("Sel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Should().BeNull();
        type.GetProperty("Off", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Should().BeNull();
    }

    [Fact]
    public void DeathMenu_HasCorrectLines()
    {
        PcraftData.DeathMenu.Lines[0].Should().Be("you died");
        PcraftData.DeathMenu.Lines[1].Should().Be("alone ...");
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
