using CSharpCraft.Pcraft.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Data;

public sealed class PlayerEntityTests
{
    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_SetsPosition()
    {
        var sut = new PlayerEntity(F32.FromInt(32), F32.FromInt(48));
        sut.X.Should().Be(F32.FromInt(32));
        sut.Y.Should().Be(F32.FromInt(48));
    }

    [Fact]
    public void Constructor_DefaultsVelocityToZero()
    {
        var sut = new PlayerEntity(F32.Zero, F32.Zero);
        sut.Vx.Should().Be(F32.Zero);
        sut.Vy.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Type hierarchy
    // --------------------------------------------------------------------------

    [Fact]
    public void IsCharacterEntity()
    {
        new PlayerEntity(F32.Zero, F32.Zero).Should().BeAssignableTo<CharacterEntity>();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Field defaults
    // --------------------------------------------------------------------------

    [Fact]
    public void AiFields_DefaultToZero()
    {
        var sut = new PlayerEntity(F32.Zero, F32.Zero);
        sut.Life.Should().Be(F32.Zero);
        sut.Prot.Should().Be(F32.Zero);
        sut.Lrot.Should().Be(F32.Zero);
        sut.Panim.Should().Be(F32.Zero);
        sut.Banim.Should().Be(F32.Zero);
        sut.Step.Should().Be(0);
        sut.Dtim.Should().Be(F32.Zero);
        sut.Dx.Should().Be(F32.Zero);
        sut.Dy.Should().Be(F32.Zero);
        sut.Ox.Should().Be(F32.Zero);
        sut.Oy.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
}
