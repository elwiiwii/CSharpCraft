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

    // Creates a mock ISceneSetup whose RegisterUpdate/RegisterDraw return a no-op handle.
    private static (Mock<ISceneSetup> setup, List<Action> updateCallbacks, List<Action> drawCallbacks) MakeSetup()
    {
        Mock<ISceneSetup> mock = new();
        Mock<IFunctionHandle> handle = new();
        List<Action> updates = [];
        List<Action> draws = [];

        _ = mock.Setup(s => s.RegisterUpdate(It.IsAny<Action>(), It.IsAny<double>(), It.IsAny<PauseBehavior>()))
            .Callback<Action, double, PauseBehavior>((cb, _, _) => updates.Add(cb))
            .Returns(handle.Object);
        _ = mock.Setup(s => s.RegisterDraw(It.IsAny<Action>(), It.IsAny<double>(), It.IsAny<PauseBehavior>()))
            .Callback<Action, double, PauseBehavior>((cb, _, _) => draws.Add(cb))
            .Returns(handle.Object);

        return (mock, updates, draws);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Init — update registration
    // --------------------------------------------------------------------------

    [Fact]
    public void Init_RegistersTwoUpdateCallbacks_WhenAnimateWaterIsTrue()
    {
        // base.Init registers one update (water time counter), PcraftPreviewScene
        // registers one more for the button listener — total: 2.
        (Mock<ISceneSetup>? setup, List<Action>? updates, List<Action> _) = MakeSetup();
        TestablePcraftPreviewScene sut = new();

        sut.Init(setup.Object);

        _ = updates.Should().HaveCount(2,
            because: "one update from base (water animation) plus one for button polling");
    }

    [Fact]
    public void Init_RegistersOneUpdateCallback_WhenAnimateWaterIsFalse()
    {
        // When AnimateWater=false, base registers no update; only the button listener
        // update is registered.
        (Mock<ISceneSetup>? setup, List<Action>? updates, List<Action> _) = MakeSetup();
        TestablePcraftPreviewScene sut = new() { Water = false };

        sut.Init(setup.Object);

        _ = updates.Should().HaveCount(1,
            because: "only the button-listener update must be registered when AnimateWater=false");
    }

    [Fact]
    public void Init_RegistersButtonUpdate_At30Fps()
    {
        (Mock<ISceneSetup>? setup, List<Action> _, List<Action> _) = MakeSetup();
        TestablePcraftPreviewScene sut = new();

        sut.Init(setup.Object);

        // At least one RegisterUpdate call must use 30 fps (the button listener).
        setup.Verify(
            s => s.RegisterUpdate(It.IsAny<Action>(), 30.0, It.IsAny<PauseBehavior>()),
            Times.AtLeastOnce,
            "the button-listener update must be registered at 30 fps");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Button callback — launch behaviour
    // --------------------------------------------------------------------------

    [Fact]
    public void ButtonCallback_DoesNotLaunch_WhenNoButtonPressed()
    {
        (Mock<ISceneSetup>? setup, List<Action>? updates, List<Action> _) = MakeSetup();
        TestablePcraftPreviewScene sut = new()
        {
            ButtonOverride = () => false
        };
        sut.Init(setup.Object);

        // Invoke all update callbacks several times.
        for (int i = 0; i < 5; i++)
            foreach (Action cb in updates)
                cb();

        _ = sut.LaunchCount.Should().Be(0, because: "LaunchGame must not fire when no button is pressed");
    }

    [Fact]
    public void ButtonCallback_LaunchesOnce_WhenButtonPressedOnFirstFrame()
    {
        (Mock<ISceneSetup>? setup, List<Action>? updates, List<Action> _) = MakeSetup();
        TestablePcraftPreviewScene sut = new()
        {
            ButtonOverride = () => true  // button always held
        };
        sut.Init(setup.Object);

        // Invoke all update callbacks multiple times — launch must happen exactly once.
        for (int i = 0; i < 5; i++)
            foreach (Action cb in updates)
                cb();

        _ = sut.LaunchCount.Should().Be(1, because: "LaunchGame must fire exactly once, not every frame");
    }

    [Fact]
    public void ButtonCallback_LaunchesOnce_WhenButtonToggledAfterDelay()
    {
        (Mock<ISceneSetup>? setup, List<Action>? updates, List<Action> _) = MakeSetup();
        TestablePcraftPreviewScene sut = new();
        bool pressed = false;
        sut.ButtonOverride = () => pressed;
        sut.Init(setup.Object);

        // Several frames with no button.
        for (int i = 0; i < 3; i++)
            foreach (Action cb in updates)
                cb();

        _ = sut.LaunchCount.Should().Be(0, because: "LaunchGame must not fire before any button press");

        // Press button and run several more frames.
        pressed = true;
        for (int i = 0; i < 4; i++)
            foreach (Action cb in updates)
                cb();

        _ = sut.LaunchCount.Should().Be(1, because: "LaunchGame must fire exactly once after the button is first pressed");
    }

    [Fact]
    public void ButtonCallback_DoesNotLaunchAgain_AfterSceneAlreadyScheduled()
    {
        (Mock<ISceneSetup>? setup, List<Action>? updates, List<Action> _) = MakeSetup();
        TestablePcraftPreviewScene sut = new()
        {
            ButtonOverride = () => true
        };
        sut.Init(setup.Object);

        // Run 20 frames — launch must still be exactly 1 despite constant button press.
        for (int i = 0; i < 20; i++)
            foreach (Action cb in updates)
                cb();

        _ = sut.LaunchCount.Should().Be(1, because: "the launched flag must prevent re-triggering on subsequent frames");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Subclass — overridable helpers
    // --------------------------------------------------------------------------

    // Helper: subclass that exposes AnimateWater override for test control.
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

    // --------------------------------------------------------------------------
    #endregion
}
