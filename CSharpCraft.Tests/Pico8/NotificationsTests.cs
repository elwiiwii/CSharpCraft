using CSharpCraft.Pico8;
using FluentAssertions;
using Moq;
using Xunit;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Tests for Notifications — static accessor for the popup notification service.
/// Phase 9: New static accessor following GameRendering.Current pattern.
/// </summary>
public class NotificationsTests : IDisposable
{
    public NotificationsTests()
    {
        Notifications.Reset();
    }

    public void Dispose()
    {
        Notifications.Reset();
    }

    [Fact]
    public void Current_WhenNotSet_ThrowsInvalidOperationException()
    {
        var act = () => { var _ = Notifications.Current; };
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Current_WhenSetToNull_ThrowsArgumentNullException()
    {
        var act = () => Notifications.Current = null!;
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Current_WhenSet_ReturnsSetInstance()
    {
        var mockPopup = new Mock<IPopupService>();
        Notifications.Current = mockPopup.Object;

        Notifications.Current.Should().Be(mockPopup.Object);
    }

    [Fact]
    public void Show_DelegatesToCurrentWithInfoSeverity()
    {
        var mockPopup = new Mock<IPopupService>();
        Notifications.Current = mockPopup.Object;

        Notifications.Show("test message");

        mockPopup.Verify(p => p.Show("test message", PopupSeverity.Info), Times.Once);
    }

    [Fact]
    public void ShowError_DelegatesToCurrentWithErrorSeverity()
    {
        var mockPopup = new Mock<IPopupService>();
        Notifications.Current = mockPopup.Object;

        Notifications.ShowError("error message");

        mockPopup.Verify(p => p.Show("error message", PopupSeverity.Error), Times.Once);
    }

    [Fact]
    public void Reset_ClearsCurrentInstance()
    {
        var mockPopup = new Mock<IPopupService>();
        Notifications.Current = mockPopup.Object;

        Notifications.Reset();

        var act = () => { var _ = Notifications.Current; };
        act.Should().Throw<InvalidOperationException>();
    }
}
