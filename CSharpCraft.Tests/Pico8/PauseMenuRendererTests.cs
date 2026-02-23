using CSharpCraft.Pico8;
using FluentAssertions;
using Microsoft.Xna.Framework;
using Moq;
using Xunit;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Tests for PauseMenuRenderer — renders the pause menu overlay.
/// Phase 9: Split from OverlayRenderer into dedicated pause menu renderer.
/// </summary>
public class PauseMenuRendererTests
{
    private readonly Mock<IGraphicsAPI> _mockGraphics;
    private readonly Mock<ITextureRenderer> _mockTextureRenderer;
    private readonly PauseMenuRenderer _renderer;

    public PauseMenuRendererTests()
    {
        _mockGraphics = new Mock<IGraphicsAPI>();
        _mockTextureRenderer = new Mock<ITextureRenderer>();
        _renderer = new PauseMenuRenderer(_mockGraphics.Object, _mockTextureRenderer.Object);
    }

    #region Interface

    [Fact]
    public void PauseMenuRenderer_ImplementsIPauseMenuRenderer()
    {
        _renderer.Should().BeAssignableTo<IPauseMenuRenderer>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullGraphics()
    {
        var act = () => new PauseMenuRenderer(null!, _mockTextureRenderer.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullTextureRenderer()
    {
        var act = () => new PauseMenuRenderer(_mockGraphics.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region DrawPauseMenu

    [Fact]
    public void DrawPauseMenu_DrawsThreeRectanglesForBorder()
    {
        var items = CreateMenuItems("continue", "quit");

        _renderer.DrawPauseMenu(items, 0, (4, 4));

        // Three concentric Rectfill calls for border (black, white, black)
        _mockGraphics.Verify(g => g.Rectfill(
            It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<double>(), It.IsAny<double>(), (double)0), Times.Exactly(2));
        _mockGraphics.Verify(g => g.Rectfill(
            It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<double>(), It.IsAny<double>(), (double)7), Times.Once);
    }

    [Fact]
    public void DrawPauseMenu_DrawsPauseArrowAtSelectedPosition()
    {
        var items = CreateMenuItems("continue", "options", "quit");

        _renderer.DrawPauseMenu(items, 1, (4, 4));

        // PauseArrow drawn via ITextureRenderer (not raw SpriteBatch)
        _mockTextureRenderer.Verify(r => r.Draw(
            "PauseArrow",
            It.IsAny<double>(), It.IsAny<double>(),
            Color.White,
            It.IsAny<double>(), It.IsAny<double>(),
            false, false), Times.Once);
    }

    [Fact]
    public void DrawPauseMenu_PrintsEachMenuItem()
    {
        var items = CreateMenuItems("continue", "options", "quit");

        _renderer.DrawPauseMenu(items, 0, (4, 4));

        // Each menu item printed
        _mockGraphics.Verify(g => g.Print(
            "continue", It.IsAny<double>(), It.IsAny<double>(), (double)7), Times.Once);
        _mockGraphics.Verify(g => g.Print(
            "options", It.IsAny<double>(), It.IsAny<double>(), (double)7), Times.Once);
        _mockGraphics.Verify(g => g.Print(
            "quit", It.IsAny<double>(), It.IsAny<double>(), (double)7), Times.Once);
    }

    [Fact]
    public void DrawPauseMenu_SelectedItemHasIndent()
    {
        var items = CreateMenuItems("continue", "quit");

        _renderer.DrawPauseMenu(items, 0, (4, 4));

        // Selected item (index 0, "continue") gets +1 indent (xborder + 1 + 12 = 36)
        // Non-selected item (index 1, "quit") gets no indent (xborder + 0 + 12 = 35)
        _mockGraphics.Verify(g => g.Print(
            "continue", 36, It.IsAny<double>(), (double)7), Times.Once);
        _mockGraphics.Verify(g => g.Print(
            "quit", 35, It.IsAny<double>(), (double)7), Times.Once);
    }

    [Fact]
    public void DrawPauseMenu_ArrowPositionMovesWithSelection()
    {
        var items = CreateMenuItems("a", "b", "c");

        // Capture the y position for selection index 0
        double arrowY0 = 0;
        _mockTextureRenderer.Setup(r => r.Draw("PauseArrow",
            It.IsAny<double>(), It.IsAny<double>(),
            Color.White, It.IsAny<double>(), It.IsAny<double>(), false, false))
            .Callback<string, double, double, Color, double, double, bool, bool>(
                (_, _, y, _, _, _, _, _) => arrowY0 = y);

        _renderer.DrawPauseMenu(items, 0, (4, 4));

        // Reset and capture for selection index 2
        _mockTextureRenderer.Invocations.Clear();
        double arrowY2 = 0;
        _mockTextureRenderer.Setup(r => r.Draw("PauseArrow",
            It.IsAny<double>(), It.IsAny<double>(),
            Color.White, It.IsAny<double>(), It.IsAny<double>(), false, false))
            .Callback<string, double, double, Color, double, double, bool, bool>(
                (_, _, y, _, _, _, _, _) => arrowY2 = y);

        _renderer.DrawPauseMenu(items, 2, (4, 4));

        // Arrow should be 16 pixels lower (2 items * 8 pixels each)
        (arrowY2 - arrowY0).Should().Be(16);
    }

    #endregion

    #region Helpers

    private static List<MenuItem> CreateMenuItems(params string[] names)
    {
        return names.Select(n => new MenuItem(() => n, () => { })).ToList();
    }

    #endregion
}
