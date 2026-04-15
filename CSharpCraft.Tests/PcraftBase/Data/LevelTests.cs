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
        new Level(x: 64, y: 0, sx: 32, sy: 32, isUnder: true).X.Should().Be(64);
    }

    [Fact]
    public void Constructor_SetsY()
    {
        new Level(x: 0, y: 0, sx: 64, sy: 64, isUnder: false).Y.Should().Be(0);
    }

    [Fact]
    public void Constructor_SetsSxAndSy()
    {
        var sut = new Level(0, 0, 32, 32, false);
        sut.Sx.Should().Be(32);
        sut.Sy.Should().Be(32);
    }

    [Fact]
    public void Constructor_SetsIsUnder()
    {
        new Level(64, 0, 32, 32, isUnder: true).IsUnder.Should().BeTrue();
        new Level(0,  0, 64, 64, isUnder: false).IsUnder.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Entity lists — initialised empty
    // --------------------------------------------------------------------------

    [Fact]
    public void Ent_IsEmptyOnConstruction()
    {
        new Level(0, 0, 64, 64, false).Ent.Should().BeEmpty();
    }

    [Fact]
    public void Ene_IsEmptyOnConstruction()
    {
        new Level(0, 0, 64, 64, false).Ene.Should().BeEmpty();
    }

    [Fact]
    public void Dat_IsEmptyOnConstruction()
    {
        new Level(0, 0, 64, 64, false).Dat.Should().BeEmpty();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Spawn point — mutable
    // --------------------------------------------------------------------------

    [Fact]
    public void Stx_DefaultsZero()
    {
        new Level(0, 0, 64, 64, false).Stx.Should().Be(F32.Zero);
    }

    [Fact]
    public void Sty_DefaultsZero()
    {
        new Level(0, 0, 64, 64, false).Sty.Should().Be(F32.Zero);
    }

    [Fact]
    public void SpawnPoint_CanBeMutated()
    {
        var sut = new Level(0, 0, 64, 64, false);
        sut.Stx = F32.FromInt(520);
        sut.Sty = F32.FromInt(312);

        sut.Stx.Should().Be(F32.FromInt(520));
        sut.Sty.Should().Be(F32.FromInt(312));
    }

    // --------------------------------------------------------------------------
    #endregion
}
