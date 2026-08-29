using CSharpCraft.PcraftPreview;
using CSharpCraft.PcraftScenes;
using FluentAssertions;
using Moq;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftPreview;

public sealed class PcraftPreviewSceneTests
{
    // --------------------------------------------------------------------------
    #region Helpers
    // --------------------------------------------------------------------------

    // Subclass that exposes AnimateWater override for test control and stubs out
    // the Pico8-dependent AnyButton/LaunchGame entry points.
    private sealed class TestablePcraftPreviewScene : PcraftPreviewScene
    {
        internal bool Water { get; init; } = true;
        public override bool AnimateWater => Water;

        internal int LaunchCount { get; private set; }
        protected override void LaunchGame()
        {
            LaunchCount++;
        }

        internal Func<bool>? ButtonOverride { get; set; }
        protected override bool AnyButton()
        {
            return ButtonOverride?.Invoke() ?? base.AnyButton();
        }
    }

    private static TestablePcraftPreviewScene MakeSut(Func<bool>? button = null)
    {
        var sut = new TestablePcraftPreviewScene { ButtonOverride = button ?? (() => false) };
        sut.Init(new Mock<ISceneSetup>().Object);
        return sut;
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Update — launch behaviour
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_DoesNotLaunch_WhenNoButtonPressed()
    {
        TestablePcraftPreviewScene sut = MakeSut(() => false);

        for (int i = 0; i < 5; i++)
            sut.Update();

        _ = sut.LaunchCount.Should().Be(0, because: "LaunchGame must not fire when no button is pressed");
    }

    [Fact]
    public void Update_LaunchesOnce_WhenButtonPressedOnFirstFrame()
    {
        TestablePcraftPreviewScene sut = MakeSut(() => true); // button always held

        for (int i = 0; i < 5; i++)
            sut.Update();

        _ = sut.LaunchCount.Should().Be(1, because: "LaunchGame must fire exactly once, not every frame");
    }

    [Fact]
    public void Update_LaunchesOnce_WhenButtonToggledAfterDelay()
    {
        TestablePcraftPreviewScene sut = new();
        bool pressed = false;
        sut.ButtonOverride = () => pressed;
        sut.Init(new Mock<ISceneSetup>().Object);

        // Several frames with no button.
        for (int i = 0; i < 3; i++)
            sut.Update();

        _ = sut.LaunchCount.Should().Be(0, because: "LaunchGame must not fire before any button press");

        // Press button and run several more frames.
        pressed = true;
        for (int i = 0; i < 4; i++)
            sut.Update();

        _ = sut.LaunchCount.Should().Be(1, because: "LaunchGame must fire exactly once after the button is first pressed");
    }

    [Fact]
    public void Update_DoesNotLaunchAgain_AfterSceneAlreadyScheduled()
    {
        TestablePcraftPreviewScene sut = MakeSut(() => true);

        // Run 20 frames — launch must still be exactly 1 despite constant button press.
        for (int i = 0; i < 20; i++)
            sut.Update();

        _ = sut.LaunchCount.Should().Be(1, because: "the launched flag must prevent re-triggering on subsequent frames");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Init
    // --------------------------------------------------------------------------

    [Fact]
    public void Init_DoesNotThrow_WhenAnimateWaterIsFalse()
    {
        var sut = new TestablePcraftPreviewScene { Water = false };

        var act = () => sut.Init(new Mock<ISceneSetup>().Object);

        _ = act.Should().NotThrow();
    }

    // --------------------------------------------------------------------------
    #endregion
}
