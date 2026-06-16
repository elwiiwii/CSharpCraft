using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using PSharp8.Graphics;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class TextPopupEntityTests
{
    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_SetsTextValue()
    {
        TextPopupEntity sut = new(F32.FromInt(42), textColor: PicoColor._09Orange, x: F32.Zero, y: F32.Zero, vy: -F32.One);
        _ = sut.TextValue.Should().Be(F32.FromInt(42));
    }

    [Fact]
    public void Constructor_SetsTextColor()
    {
        TextPopupEntity sut = new(F32.FromInt(10), textColor: PicoColor._08Red, x: F32.Zero, y: F32.Zero, vy: -F32.One);
        _ = sut.TextColor.Should().Be(PicoColor._08Red);
    }

    [Fact]
    public void Constructor_SetsTimerTo20()
    {
        TextPopupEntity sut = new(F32.One, PicoColor._11Green, F32.Zero, F32.Zero, -F32.One);
        _ = sut.Timer.Should().Be(F32.FromInt(20));
    }

    [Fact]
    public void Constructor_SetsPosition()
    {
        TextPopupEntity sut = new(F32.One, PicoColor._09Orange, F32.FromInt(10), F32.FromInt(20), -F32.One);
        _ = sut.X.Should().Be(F32.FromInt(10));
        _ = sut.Y.Should().Be(F32.FromInt(20));
    }

    [Fact]
    public void Constructor_SetsVy()
    {
        TextPopupEntity sut = new(F32.One, PicoColor._09Orange, F32.Zero, F32.Zero, vy: F32.FromDouble(-1.5));
        _ = sut.Vy.Should().Be(F32.FromDouble(-1.5));
    }

    [Fact]
    public void Constructor_SetsVxToZero()
    {
        TextPopupEntity sut = new(F32.One, PicoColor._09Orange, F32.Zero, F32.Zero, -F32.One);
        _ = sut.Vx.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
}
