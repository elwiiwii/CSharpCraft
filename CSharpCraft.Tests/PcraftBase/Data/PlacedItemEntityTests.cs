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
        var sut = new PlacedItemEntity(Workbench, F32.Zero, F32.Zero);
        sut.Type.Should().BeSameAs(Workbench);
    }

    [Fact]
    public void Constructor_SetsPosition()
    {
        var sut = new PlacedItemEntity(Workbench, F32.FromInt(10), F32.FromInt(20));
        sut.X.Should().Be(F32.FromInt(10));
        sut.Y.Should().Be(F32.FromInt(20));
    }

    [Fact]
    public void Constructor_DefaultsVelocityToZero()
    {
        var sut = new PlacedItemEntity(Workbench, F32.Zero, F32.Zero);
        sut.Vx.Should().Be(F32.Zero);
        sut.Vy.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Design contract
    // --------------------------------------------------------------------------

    [Fact]
    public void Timer_PropertyDoesNotExist()
    {
        var prop = typeof(PlacedItemEntity).GetProperty(
            "Timer",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("placed items do not decay; they persist until picked up");
    }

    [Fact]
    public void Type_IsPlaceableItemDef()
    {
        var sut = new PlacedItemEntity(Workbench, F32.Zero, F32.Zero);
        (sut.Type is PlaceableItemDef).Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
}
