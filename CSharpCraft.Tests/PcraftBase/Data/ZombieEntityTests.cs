using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class ZombieEntityTests
{
    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_SetsPosition()
    {
        var sut = new ZombieEntity(F32.FromInt(50), F32.FromInt(60));
        sut.X.Should().Be(F32.FromInt(50));
        sut.Y.Should().Be(F32.FromInt(60));
    }

    [Fact]
    public void Constructor_DefaultsVelocityToZero()
    {
        var sut = new ZombieEntity(F32.Zero, F32.Zero);
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
        new ZombieEntity(F32.Zero, F32.Zero).Should().BeAssignableTo<CharacterEntity>();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Field defaults
    // --------------------------------------------------------------------------

    [Fact]
    public void AiFields_DefaultToZero()
    {
        var sut = new ZombieEntity(F32.Zero, F32.Zero);
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
    #region Field mutation
    // --------------------------------------------------------------------------

    [Fact]
    public void Fields_CanBeMutated()
    {
        var sut = new ZombieEntity(F32.Zero, F32.Zero);
        sut.Life = F32.FromInt(10);
        sut.Step = EnStep.Chase;
        sut.Ox   = F32.FromFloat(0.4f);

        sut.Life.Should().Be(F32.FromInt(10));
        sut.Step.Should().Be(EnStep.Chase);
        sut.Ox.Should().Be(F32.FromFloat(0.4f));
    }

    // --------------------------------------------------------------------------
    #endregion
}
