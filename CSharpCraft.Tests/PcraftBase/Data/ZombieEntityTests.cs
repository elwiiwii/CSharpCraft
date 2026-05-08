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
        ZombieEntity sut = new(F32.FromInt(50), F32.FromInt(60));
        _ = sut.X.Should().Be(F32.FromInt(50));
        _ = sut.Y.Should().Be(F32.FromInt(60));
    }

    [Fact]
    public void Constructor_DefaultsVelocityToZero()
    {
        ZombieEntity sut = new(F32.Zero, F32.Zero);
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
        _ = new ZombieEntity(F32.Zero, F32.Zero).Should().BeAssignableTo<CharacterEntity>();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Field defaults
    // --------------------------------------------------------------------------

    [Fact]
    public void AiFields_DefaultToZero()
    {
        ZombieEntity sut = new(F32.Zero, F32.Zero);
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
    #region Field mutation
    // --------------------------------------------------------------------------

    [Fact]
    public void Fields_CanBeMutated()
    {
        ZombieEntity sut = new(F32.Zero, F32.Zero)
        {
            Life = F32.FromInt(10),
            Step = EnStep.Chase,
            Ox = F32.FromFloat(0.4f)
        };

        _ = sut.Life.Should().Be(F32.FromInt(10));
        _ = sut.Step.Should().Be(EnStep.Chase);
        _ = sut.Ox.Should().Be(F32.FromFloat(0.4f));
    }

    // --------------------------------------------------------------------------
    #endregion
}
