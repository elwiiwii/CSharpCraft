using CSharpCraft.Pico8;
using FluentAssertions;
using Moq;
using Xunit;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Tests for PopupService — popup notification system with severity-based color coding.
/// Phase 9: Split from OverlayRenderer, added severity support.
/// </summary>
public class PopupServiceTests
{
    private readonly Mock<IGraphicsAPI> _mockGraphics;
    private readonly PopupService _popupService;

    public PopupServiceTests()
    {
        _mockGraphics = new Mock<IGraphicsAPI>();
        _popupService = new PopupService(_mockGraphics.Object);
    }

    #region Interface

    [Fact]
    public void PopupService_ImplementsIPopupService()
    {
        _popupService.Should().BeAssignableTo<IPopupService>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullGraphics()
    {
        var act = () => new PopupService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Show

    [Fact]
    public void Show_SetsActivePopup()
    {
        _popupService.Show("test");

        _popupService.HasActivePopup.Should().BeTrue();
    }

    [Fact]
    public void HasActivePopup_WhenNoPopup_ReturnsFalse()
    {
        _popupService.HasActivePopup.Should().BeFalse();
    }

    [Fact]
    public void Show_DefaultSeverityIsInfo()
    {
        _popupService.Show("test");
        // Advance a few frames so clampFrame > 0
        for (int i = 0; i < 3; i++) _popupService.Update();

        _popupService.Draw((128, 128));

        // Info uses PICO-8 color 8 (red background)
        _mockGraphics.Verify(g => g.Rectfill(
            It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<double>(), It.IsAny<double>(), (double)8), Times.Once);
        // Info uses PICO-8 color 15 (peach text)
        _mockGraphics.Verify(g => g.Print(
            "test", 1, It.IsAny<double>(), (double)15), Times.Once);
    }

    #endregion

    #region Update Animation

    [Fact]
    public void Update_AdvancesFrame()
    {
        _popupService.Show("test");

        _popupService.Update();

        // After one update, frame should have advanced from 1.5 to 3.0
        _popupService.HasActivePopup.Should().BeTrue();
    }

    [Fact]
    public void Update_EventuallyDeactivatesPopup()
    {
        _popupService.Show("test");

        // Run enough updates to complete the animation cycle
        for (int i = 0; i < 100; i++)
        {
            _popupService.Update();
        }

        _popupService.HasActivePopup.Should().BeFalse();
    }

    #endregion

    #region Draw

    [Fact]
    public void Draw_WhenActive_DrawsRectfillAndPrint()
    {
        _popupService.Show("sound on (ctrl-m)");
        // Advance a few frames so clampFrame > 0
        for (int i = 0; i < 3; i++) _popupService.Update();

        _popupService.Draw((128, 128));

        _mockGraphics.Verify(g => g.Rectfill(
            It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<double>(), It.IsAny<double>(), (double)8), Times.Once);
        _mockGraphics.Verify(g => g.Print(
            "sound on (ctrl-m)", 1, It.IsAny<double>(), (double)15), Times.Once);
    }

    [Fact]
    public void Draw_WhenNotActive_DrawsNothing()
    {
        _popupService.Draw((128, 128));

        _mockGraphics.Verify(g => g.Rectfill(
            It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        _mockGraphics.Verify(g => g.Print(
            It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
    }

    #endregion

    #region Severity Color Coding

    [Fact]
    public void Draw_InfoSeverity_UsesRedBackgroundPeachText()
    {
        _popupService.Show("info message", PopupSeverity.Info);
        for (int i = 0; i < 3; i++) _popupService.Update();

        _popupService.Draw((128, 128));

        // Color 8 = red background
        _mockGraphics.Verify(g => g.Rectfill(
            It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<double>(), It.IsAny<double>(), (double)8), Times.Once);
        // Color 15 = peach text
        _mockGraphics.Verify(g => g.Print(
            "info message", 1, It.IsAny<double>(), (double)15), Times.Once);
    }

    [Fact]
    public void Draw_ErrorSeverity_UsesDarkPurpleBackgroundWhiteText()
    {
        _popupService.Show("error message", PopupSeverity.Error);
        for (int i = 0; i < 3; i++) _popupService.Update();

        _popupService.Draw((128, 128));

        // Color 2 = dark purple background
        _mockGraphics.Verify(g => g.Rectfill(
            It.IsAny<double>(), It.IsAny<double>(),
            It.IsAny<double>(), It.IsAny<double>(), (double)2), Times.Once);
        // Color 7 = white text
        _mockGraphics.Verify(g => g.Print(
            "error message", 1, It.IsAny<double>(), (double)7), Times.Once);
    }

    #endregion
}
