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
        _ = new Level(x: 64, y: 0, sx: 32, sy: 32, LevelTheme.Cave).X.Should().Be(64);
    }

    [Fact]
    public void Constructor_SetsY()
    {
        _ = new Level(x: 0, y: 0, sx: 64, sy: 64, LevelTheme.Surface).Y.Should().Be(0);
    }

    [Fact]
    public void Constructor_SetsSxAndSy()
    {
        Level sut = new(0, 0, 32, 32, LevelTheme.Surface);
        _ = sut.Sx.Should().Be(32);
        _ = sut.Sy.Should().Be(32);
    }

    [Fact]
    public void Constructor_SetsTheme()
    {
        _ = new Level(64, 0, 32, 32, LevelTheme.Cave).Theme.Should().Be(LevelTheme.Cave);
        _ = new Level(0, 0, 64, 64, LevelTheme.Surface).Theme.Should().Be(LevelTheme.Surface);
    }

    [Fact]
    public void Map_HasCorrectDimensions()
    {
        Level sut = new(0, 0, 32, 32, LevelTheme.Surface);
        _ = sut.Map.GetLength(0).Should().Be(32);
        _ = sut.Map.GetLength(1).Should().Be(32);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Entity lists — initialised empty
    // --------------------------------------------------------------------------

    [Fact]
    public void Ent_IsEmptyOnConstruction()
    {
        _ = new Level(0, 0, 64, 64, LevelTheme.Surface).Ent.Should().BeEmpty();
    }

    [Fact]
    public void Ene_IsEmptyOnConstruction()
    {
        _ = new Level(0, 0, 64, 64, LevelTheme.Surface).Ene.Should().BeEmpty();
    }

    // Dat removed — tile-level data is now stored in Tile.HarvestLife / Tile.GrowthTimer

    // --------------------------------------------------------------------------
    #endregion
    #region Spawn point — mutable
    // --------------------------------------------------------------------------

    [Fact]
    public void Stx_DefaultsZero()
    {
        _ = new Level(0, 0, 64, 64, LevelTheme.Surface).Stx.Should().Be(F32.Zero);
    }

    [Fact]
    public void Sty_DefaultsZero()
    {
        _ = new Level(0, 0, 64, 64, LevelTheme.Surface).Sty.Should().Be(F32.Zero);
    }

    [Fact]
    public void SpawnPoint_CanBeMutated()
    {
        Level sut = new(0, 0, 64, 64, LevelTheme.Surface)
        {
            Stx = F32.FromInt(520),
            Sty = F32.FromInt(312)
        };

        _ = sut.Stx.Should().Be(F32.FromInt(520));
        _ = sut.Sty.Should().Be(F32.FromInt(312));
    }

    // --------------------------------------------------------------------------
    #endregion
}
