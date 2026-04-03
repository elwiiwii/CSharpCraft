using CSharpCraft.Pcraft.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Data;

public sealed class ItemEntityTests
{
    private static readonly ItemDef Wood  = new("wood",  103);
    private static readonly ItemDef EText = new("text",  103);

    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_SetsType()
    {
        new ItemEntity(Wood, F32.FromInt(10), F32.FromInt(20)).Type.Should().BeSameAs(Wood);
    }

    [Fact]
    public void Constructor_SetsPosition()
    {
        var sut = new ItemEntity(Wood, F32.FromInt(10), F32.FromInt(20));
        sut.X.Should().Be(F32.FromInt(10));
        sut.Y.Should().Be(F32.FromInt(20));
    }

    [Fact]
    public void Constructor_SetsVelocity_WhenProvided()
    {
        var sut = new ItemEntity(Wood, F32.FromInt(10), F32.FromInt(20), vx: F32.FromInt(3), vy: F32.FromInt(-1));
        sut.Vx.Should().Be(F32.FromInt(3));
        sut.Vy.Should().Be(F32.FromInt(-1));
    }

    [Fact]
    public void Constructor_DefaultsVelocityToZero()
    {
        var sut = new ItemEntity(Wood, F32.Zero, F32.Zero);
        sut.Vx.Should().Be(F32.Zero);
        sut.Vy.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region IInventorySlot
    // --------------------------------------------------------------------------

    [Fact]
    public void IInventorySlot_Type_ReturnsDef()
    {
        ItemEntity sut = new ItemEntity(Wood, F32.Zero, F32.Zero);
        sut.Type.Should().BeSameAs(Wood);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Field defaults
    // --------------------------------------------------------------------------

    [Fact]
    public void HasCol_DefaultsFalse()
    {
        new ItemEntity(Wood, F32.Zero, F32.Zero).HasCol.Should().BeFalse();
    }

    [Fact]
    public void GiveItem_DefaultsNull()
    {
        new ItemEntity(Wood, F32.Zero, F32.Zero).GiveItem.Should().BeNull();
    }

    [Fact]
    public void Timer_DefaultsNull()
    {
        new ItemEntity(Wood, F32.Zero, F32.Zero).Timer.Should().BeNull();
    }

    [Fact]
    public void List_DefaultsNull()
    {
        new ItemEntity(Wood, F32.Zero, F32.Zero).List.Should().BeNull();
    }

    [Fact]
    public void TextValue_DefaultsZero()
    {
        new ItemEntity(EText, F32.Zero, F32.Zero).TextValue.Should().Be(F32.Zero);
    }

    [Fact]
    public void TextColor_DefaultsZero()
    {
        new ItemEntity(EText, F32.Zero, F32.Zero).TextColor.Should().Be(0);
    }

    // --------------------------------------------------------------------------
    #endregion
}

