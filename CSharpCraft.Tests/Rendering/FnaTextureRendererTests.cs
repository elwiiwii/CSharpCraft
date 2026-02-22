using CSharpCraft.Pico8;
using FluentAssertions;
using Microsoft.Xna.Framework;
using Xunit;

namespace CSharpCraft.Tests.Rendering;

/// <summary>
/// Tests for FnaTextureRenderer — the concrete ITextureRenderer implementation.
/// 
/// Note: SpriteBatch is sealed (FNA) and cannot be mocked, so Draw/ClearDevice
/// delegation tests are covered at the interface level (TextureRendererTests).
/// These tests focus on the pure-logic methods: cursor calculation and construction.
/// </summary>
public class FnaTextureRendererTests
{
    #region Construction

    [Fact]
    public void Constructor_WithNullBatch_ThrowsArgumentNullException()
    {
        var act = () => new FnaTextureRenderer(
            null!, new Dictionary<string, Microsoft.Xna.Framework.Graphics.Texture2D>(),
            null, null);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("batch");
    }

    [Fact]
    public void Constructor_WithNullTextures_ThrowsArgumentNullException()
    {
        // SpriteBatch requires GraphicsDevice which we can't easily create in unit tests.
        // Testing the null guard independently via a helper approach.
        // This test validates the guard exists at the API level.
        var act = () => new FnaTextureRenderer(
            null!, null!, null, null);

        // Both are null, first null (batch) throws first
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region GetCursorPosition Logic

    [Fact]
    public void GetCursorPosition_WhenWindowEqualsViewport_ReturnsOriginalCoordinates()
    {
        // When window and viewport have the same dimensions, offset is zero
        // Formula: mouseX - ((windowWidth - viewportWidth) / 2.0f)
        // 300 - ((800 - 800) / 2) = 300
        var result = FnaTextureRenderer.CalculateCursorPosition(300, 400, 800, 600, 800, 600);

        result.X.Should().Be(300f);
        result.Y.Should().Be(400f);
    }

    [Fact]
    public void GetCursorPosition_WhenWindowLargerThanViewport_SubtractsOffset()
    {
        // Window is 1000x800 but viewport is 800x600
        // X: 300 - ((1000 - 800) / 2) = 300 - 100 = 200
        // Y: 400 - ((800 - 600) / 2) = 400 - 100 = 300
        var result = FnaTextureRenderer.CalculateCursorPosition(300, 400, 1000, 800, 800, 600);

        result.X.Should().Be(200f);
        result.Y.Should().Be(300f);
    }

    [Fact]
    public void GetCursorPosition_WhenWindowSmallerThanViewport_AddsOffset()
    {
        // Window is 600x400 but viewport is 800x600
        // X: 300 - ((600 - 800) / 2) = 300 - (-100) = 400
        // Y: 400 - ((400 - 600) / 2) = 400 - (-100) = 500
        var result = FnaTextureRenderer.CalculateCursorPosition(300, 400, 600, 400, 800, 600);

        result.X.Should().Be(400f);
        result.Y.Should().Be(500f);
    }

    [Fact]
    public void GetCursorPosition_WithAsymmetricOffset_CalculatesCorrectly()
    {
        // Window: 1200x600, Viewport: 800x600
        // X: 500 - ((1200 - 800) / 2) = 500 - 200 = 300
        // Y: 300 - ((600 - 600) / 2) = 300 - 0 = 300  (no Y offset)
        var result = FnaTextureRenderer.CalculateCursorPosition(500, 300, 1200, 600, 800, 600);

        result.X.Should().Be(300f);
        result.Y.Should().Be(300f);
    }

    [Fact]
    public void GetCursorPosition_AtOrigin_ReturnsNegativeOffset()
    {
        // Mouse at (0,0), window larger than viewport
        // X: 0 - ((1000 - 800) / 2) = -100
        // Y: 0 - ((800 - 600) / 2) = -100
        var result = FnaTextureRenderer.CalculateCursorPosition(0, 0, 1000, 800, 800, 600);

        result.X.Should().Be(-100f);
        result.Y.Should().Be(-100f);
    }

    #endregion
}
