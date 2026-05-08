using System.Reflection;
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
        _ = new DroppedItemEntity(Wood, F32.Zero, F32.Zero, F32.FromInt(120)).Type.Should().BeSameAs(Wood);
    }

    [Fact]
    public void Constructor_SetsPosition()
    {
        DroppedItemEntity sut = new(Wood, F32.FromInt(10), F32.FromInt(20), F32.FromInt(120));
        _ = sut.X.Should().Be(F32.FromInt(10));
        _ = sut.Y.Should().Be(F32.FromInt(20));
    }

    [Fact]
    public void Constructor_SetsTimer()
    {
        DroppedItemEntity sut = new(Wood, F32.Zero, F32.Zero, timer: F32.FromInt(115));
        _ = sut.Timer.Should().Be(F32.FromInt(115));
    }

    [Fact]
    public void Constructor_SetsVelocity_WhenProvided()
    {
        DroppedItemEntity sut = new(Wood, F32.FromInt(10), F32.FromInt(20), F32.FromInt(120),
            vx: F32.FromInt(3), vy: F32.FromInt(-1));
        _ = sut.Vx.Should().Be(F32.FromInt(3));
        _ = sut.Vy.Should().Be(F32.FromInt(-1));
    }

    [Fact]
    public void Constructor_DefaultsVelocityToZero()
    {
        DroppedItemEntity sut = new(Wood, F32.Zero, F32.Zero, F32.FromInt(120));
        _ = sut.Vx.Should().Be(F32.Zero);
        _ = sut.Vy.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Design contract
    // --------------------------------------------------------------------------

    [Fact]
    public void HasCol_PropertyDoesNotExist()
    {
        PropertyInfo? prop = typeof(DroppedItemEntity).GetProperty(
            "HasCol",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        _ = prop.Should().BeNull("HasCol is replaced by entity type; DroppedItemEntity implies collision");
    }

    [Fact]
    public void GiveItem_PropertyDoesNotExist()
    {
        PropertyInfo? prop = typeof(DroppedItemEntity).GetProperty(
            "GiveItem",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        _ = prop.Should().BeNull("GiveItem was always equal to Type; use Type directly");
    }

    // --------------------------------------------------------------------------
    #endregion
}
