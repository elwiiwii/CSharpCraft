using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class DroppedItemEntityTests
{
    private static readonly ItemDef Wood = new("wood", 103);

    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_SetsType()
    {
        new DroppedItemEntity(Wood, F32.Zero, F32.Zero, F32.FromInt(120)).Type.Should().BeSameAs(Wood);
    }

    [Fact]
    public void Constructor_SetsPosition()
    {
        var sut = new DroppedItemEntity(Wood, F32.FromInt(10), F32.FromInt(20), F32.FromInt(120));
        sut.X.Should().Be(F32.FromInt(10));
        sut.Y.Should().Be(F32.FromInt(20));
    }

    [Fact]
    public void Constructor_SetsTimer()
    {
        var sut = new DroppedItemEntity(Wood, F32.Zero, F32.Zero, timer: F32.FromInt(115));
        sut.Timer.Should().Be(F32.FromInt(115));
    }

    [Fact]
    public void Constructor_SetsVelocity_WhenProvided()
    {
        var sut = new DroppedItemEntity(Wood, F32.FromInt(10), F32.FromInt(20), F32.FromInt(120),
            vx: F32.FromInt(3), vy: F32.FromInt(-1));
        sut.Vx.Should().Be(F32.FromInt(3));
        sut.Vy.Should().Be(F32.FromInt(-1));
    }

    [Fact]
    public void Constructor_DefaultsVelocityToZero()
    {
        var sut = new DroppedItemEntity(Wood, F32.Zero, F32.Zero, F32.FromInt(120));
        sut.Vx.Should().Be(F32.Zero);
        sut.Vy.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Design contract
    // --------------------------------------------------------------------------

    [Fact]
    public void HasCol_PropertyDoesNotExist()
    {
        var prop = typeof(DroppedItemEntity).GetProperty(
            "HasCol",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("HasCol is replaced by entity type; DroppedItemEntity implies collision");
    }

    [Fact]
    public void GiveItem_PropertyDoesNotExist()
    {
        var prop = typeof(DroppedItemEntity).GetProperty(
            "GiveItem",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("GiveItem was always equal to Type; use Type directly");
    }

    // --------------------------------------------------------------------------
    #endregion
}
