using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class PlayerEntityTests
{
    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_SetsPosition()
    {
        PlayerEntity sut = new(F32.FromInt(32), F32.FromInt(48));
        _ = sut.X.Should().Be(F32.FromInt(32));
        _ = sut.Y.Should().Be(F32.FromInt(48));
    }

    [Fact]
    public void Constructor_DefaultsVelocityToZero()
    {
        PlayerEntity sut = new(F32.Zero, F32.Zero);
        _ = sut.Vx.Should().Be(F32.Zero);
        _ = sut.Vy.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Type hierarchy
    // --------------------------------------------------------------------------

    [Fact]
    public void IsCharacterEntity()
    {
        _ = new PlayerEntity(F32.Zero, F32.Zero).Should().BeAssignableTo<CharacterEntity>();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Field defaults
    // --------------------------------------------------------------------------

    [Fact]
    public void AiFields_DefaultToZero()
    {
        PlayerEntity sut = new(F32.Zero, F32.Zero);
        _ = sut.Life.Should().Be(F32.Zero);
        _ = sut.Prot.Should().Be(F32.Zero);
        _ = sut.Lrot.Should().Be(F32.Zero);
        _ = sut.Panim.Should().Be(F32.Zero);
        _ = sut.Banim.Should().Be(F32.Zero);
        _ = sut.Step.Should().Be(0);
        _ = sut.Dtim.Should().Be(F32.Zero);
        _ = sut.Dx.Should().Be(F32.Zero);
        _ = sut.Dy.Should().Be(F32.Zero);
        _ = sut.Ox.Should().Be(F32.Zero);
        _ = sut.Oy.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
}
