using System.Reflection;
using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class PlacedItemEntityTests
{
    private static readonly PlaceableItemDef Workbench =
        new BenchItemDef("workbench", 89, 104, [1, 4, 9]);

    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_SetsType()
    {
        PlacedItemEntity sut = new(Workbench, F32.Zero, F32.Zero);
        _ = sut.Type.Should().BeSameAs(Workbench);
    }

    [Fact]
    public void Constructor_SetsPosition()
    {
        PlacedItemEntity sut = new(Workbench, F32.FromInt(10), F32.FromInt(20));
        _ = sut.X.Should().Be(F32.FromInt(10));
        _ = sut.Y.Should().Be(F32.FromInt(20));
    }

    [Fact]
    public void Constructor_DefaultsVelocityToZero()
    {
        PlacedItemEntity sut = new(Workbench, F32.Zero, F32.Zero);
        _ = sut.Vx.Should().Be(F32.Zero);
        _ = sut.Vy.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Design contract
    // --------------------------------------------------------------------------

    [Fact]
    public void Timer_PropertyDoesNotExist()
    {
        PropertyInfo? prop = typeof(PlacedItemEntity).GetProperty(
            "Timer",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        _ = prop.Should().BeNull("placed items do not decay; they persist until picked up");
    }

    [Fact]
    public void Type_IsPlaceableItemDef()
    {
        PlacedItemEntity sut = new(Workbench, F32.Zero, F32.Zero);
        _ = (sut.Type is not null).Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
}
