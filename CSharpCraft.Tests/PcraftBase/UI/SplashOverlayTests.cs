using CSharpCraft.PcraftBase.UI;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.UI;

public sealed class SplashOverlayTests
{
    // --------------------------------------------------------------------------
    #region Death overlay
    // --------------------------------------------------------------------------

    [Fact]
    public void Death_HasExpectedText()
    {
        SplashOverlay.Death.Text.Should().Be("you died");
    }

    [Fact]
    public void Death_HasExpectedText2()
    {
        SplashOverlay.Death.Text2.Should().Be("alone ...");
    }

    [Fact]
    public void Death_HasExpectedSpr()
    {
        SplashOverlay.Death.Spr.Should().Be(128);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Win overlay
    // --------------------------------------------------------------------------

    [Fact]
    public void Win_HasExpectedText()
    {
        SplashOverlay.Win.Text.Should().Be("you successfully escaped");
    }

    [Fact]
    public void Win_HasExpectedText2()
    {
        SplashOverlay.Win.Text2.Should().Be("from the island");
    }

    [Fact]
    public void Win_HasExpectedSpr()
    {
        SplashOverlay.Win.Spr.Should().Be(136);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Main overlay
    // --------------------------------------------------------------------------

    [Fact]
    public void Main_HasExpectedText()
    {
        SplashOverlay.Main.Text.Should().Be("by nusan");
    }

    [Fact]
    public void Main_HasExpectedText2()
    {
        SplashOverlay.Main.Text2.Should().Be("2016");
    }

    [Fact]
    public void Main_HasExpectedSpr()
    {
        SplashOverlay.Main.Spr.Should().Be(128);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Intro overlay
    // --------------------------------------------------------------------------

    [Fact]
    public void Intro_HasExpectedText()
    {
        SplashOverlay.Intro.Text.Should().Be("a storm leaved you");
    }

    [Fact]
    public void Intro_HasExpectedText2()
    {
        SplashOverlay.Intro.Text2.Should().Be("on a deserted island");
    }

    [Fact]
    public void Intro_HasExpectedSpr()
    {
        SplashOverlay.Intro.Spr.Should().Be(136);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Immutability — no Sel / Off mutable state
    // --------------------------------------------------------------------------

    [Fact]
    public void Type_HasNoSelProperty()
    {
        typeof(SplashOverlay).GetProperty("Sel").Should().BeNull(
            because: "SplashOverlays are not interactive and must not carry selection state");
    }

    [Fact]
    public void Type_HasNoOffProperty()
    {
        typeof(SplashOverlay).GetProperty("Off").Should().BeNull(
            because: "SplashOverlays are not interactive and must not carry scroll-offset state");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Factory properties return distinct instances
    // --------------------------------------------------------------------------

    [Fact]
    public void AllFactoryProperties_ReturnDistinctInstances()
    {
        var all = new[] { SplashOverlay.Death, SplashOverlay.Win, SplashOverlay.Main, SplashOverlay.Intro };
        all.Should().OnlyHaveUniqueItems(because: "each overlay represents a distinct game state");
    }

    // --------------------------------------------------------------------------
    #endregion
}
