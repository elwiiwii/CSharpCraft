using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class LevelTests
{
    // --------------------------------------------------------------------------
    #region Constructor — immutable fields
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_SetsX()
    {
        new Level(x: 64, y: 0, sx: 32, sy: 32, LevelTheme.Cave).X.Should().Be(64);
    }

    [Fact]
    public void Constructor_SetsY()
    {
        new Level(x: 0, y: 0, sx: 64, sy: 64, LevelTheme.Surface).Y.Should().Be(0);
    }

    [Fact]
    public void Constructor_SetsSxAndSy()
    {
        var sut = new Level(0, 0, 32, 32, LevelTheme.Surface);
        sut.Sx.Should().Be(32);
        sut.Sy.Should().Be(32);
    }

    [Fact]
    public void Constructor_SetsTheme()
    {
        new Level(64, 0, 32, 32, LevelTheme.Cave).Theme.Should().Be(LevelTheme.Cave);
        new Level(0,  0, 64, 64, LevelTheme.Surface).Theme.Should().Be(LevelTheme.Surface);
    }

    [Fact]
    public void Map_HasCorrectDimensions()
    {
        var sut = new Level(0, 0, 32, 32, LevelTheme.Surface);
        sut.Map.GetLength(0).Should().Be(32);
        sut.Map.GetLength(1).Should().Be(32);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Entity lists — initialised empty
    // --------------------------------------------------------------------------

    [Fact]
    public void Ent_IsEmptyOnConstruction()
    {
        new Level(0, 0, 64, 64, LevelTheme.Surface).Ent.Should().BeEmpty();
    }

    [Fact]
    public void Ene_IsEmptyOnConstruction()
    {
        new Level(0, 0, 64, 64, LevelTheme.Surface).Ene.Should().BeEmpty();
    }

    // Dat removed — tile-level data is now stored in Tile.HarvestLife / Tile.GrowthTimer

    // --------------------------------------------------------------------------
    #endregion
    #region Spawn point — mutable
    // --------------------------------------------------------------------------

    [Fact]
    public void Stx_DefaultsZero()
    {
        new Level(0, 0, 64, 64, LevelTheme.Surface).Stx.Should().Be(F32.Zero);
    }

    [Fact]
    public void Sty_DefaultsZero()
    {
        new Level(0, 0, 64, 64, LevelTheme.Surface).Sty.Should().Be(F32.Zero);
    }

    [Fact]
    public void SpawnPoint_CanBeMutated()
    {
        var sut = new Level(0, 0, 64, 64, LevelTheme.Surface);
        sut.Stx = F32.FromInt(520);
        sut.Sty = F32.FromInt(312);

        sut.Stx.Should().Be(F32.FromInt(520));
        sut.Sty.Should().Be(F32.FromInt(312));
    }

    // --------------------------------------------------------------------------
    #endregion
}
