using CSharpCraft.PcraftPreview;
using FluentAssertions;
using Moq;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftPreview;

public sealed class PcraftPreviewBaseTests
{
    // --------------------------------------------------------------------------
    #region Helpers
    // --------------------------------------------------------------------------

    // Minimal concrete subclass with controllable properties.
    private sealed class StubPreview : PcraftPreviewBase
    {
        internal long Seed { get; init; }
        internal int Radius { get; init; }
        internal (int X, int Y)? OverrideCenter { get; init; }
        internal bool Water { get; init; } = true;

        protected override long PreviewSeed => Seed;
        protected override int PreviewRadius => Radius;
        public override (int X, int Y)? CenterOverride => OverrideCenter;
        public override bool AnimateWater => Water;
    }

    // Creates a mock ISceneSetup whose RegisterUpdate/RegisterDraw return a no-op handle.
    private static Mock<ISceneSetup> MakeSetup()
    {
        Mock<ISceneSetup> mock = new();
        Mock<IFunctionHandle> handle = new();
        _ = mock.Setup(s => s.RegisterUpdate(It.IsAny<Action>(), It.IsAny<double>(), It.IsAny<PauseBehavior>()))
            .Returns(handle.Object);
        _ = mock.Setup(s => s.RegisterDraw(It.IsAny<Action>(), It.IsAny<double>(), It.IsAny<PauseBehavior>()))
            .Returns(handle.Object);
        return mock;
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Abstract / virtual contract
    // --------------------------------------------------------------------------

    [Fact]
    public void CenterOverride_DefaultsToNull()
    {
        // The default implementation of CenterOverride must return null so that
        // PcraftWorldSampler uses the spawn tile as center.
        StubPreview sut = new() { Seed = 0, Radius = 4 };

        _ = sut.CenterOverride.Should().BeNull(
            because: "CenterOverride must default to null so spawn tile is used as center");
    }

    [Fact]
    public void AnimateWater_DefaultsToTrue()
    {
        // AnimateWater defaults to true — the RegisterUpdate time-counter is added by default.
        StubPreview sut = new() { Seed = 0, Radius = 4 };

        _ = sut.AnimateWater.Should().BeTrue(
            because: "AnimateWater must default to true");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Init — resolution
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]   // minimal radius: side = 2
    [InlineData(2)]   // side = 4
    [InlineData(4)]   // side = 8
    public void Init_SetsResolution_To16TimesSquareSide(int radius)
    {
        Mock<ISceneSetup> setup = MakeSetup();
        StubPreview sut = new() { Seed = 1, Radius = radius };
        int side = 2 * radius;

        sut.Init(setup.Object);

        setup.VerifySet(s => s.Resolution = (16 * side, 16 * side),
            $"radius={radius}: resolution must be 16\u00d7(2r) in both dimensions");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Init — draw registration
    // --------------------------------------------------------------------------

    [Fact]
    public void Init_RegistersDraw_ExactlyOnce()
    {
        Mock<ISceneSetup> setup = MakeSetup();
        StubPreview sut = new() { Seed = 1, Radius = 4 };

        sut.Init(setup.Object);

        setup.Verify(s => s.RegisterDraw(It.IsAny<Action>(), It.IsAny<double>(), It.IsAny<PauseBehavior>()),
            Times.Once, "Init must register exactly one draw callback");
    }

    [Fact]
    public void Init_RegistersDraw_At30Fps()
    {
        Mock<ISceneSetup> setup = MakeSetup();
        StubPreview sut = new() { Seed = 1, Radius = 4 };

        sut.Init(setup.Object);

        setup.Verify(s => s.RegisterDraw(It.IsAny<Action>(), 30.0, It.IsAny<PauseBehavior>()),
            Times.Once, "the draw callback must be registered at 30 fps");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Init — update registration
    // --------------------------------------------------------------------------

    [Fact]
    public void Init_RegistersUpdate_WhenAnimateWaterIsTrue()
    {
        Mock<ISceneSetup> setup = MakeSetup();
        StubPreview sut = new() { Seed = 1, Radius = 4, Water = true };

        sut.Init(setup.Object);

        setup.Verify(s => s.RegisterUpdate(It.IsAny<Action>(), It.IsAny<double>(), It.IsAny<PauseBehavior>()),
            Times.Once, "a time-counter update must be registered when AnimateWater is true");
    }

    [Fact]
    public void Init_DoesNotRegisterUpdate_WhenAnimateWaterIsFalse()
    {
        Mock<ISceneSetup> setup = MakeSetup();
        StubPreview sut = new() { Seed = 1, Radius = 4, Water = false };

        sut.Init(setup.Object);

        setup.Verify(s => s.RegisterUpdate(It.IsAny<Action>(), It.IsAny<double>(), It.IsAny<PauseBehavior>()),
            Times.Never, "no update callback must be registered when AnimateWater is false");
    }

    [Fact]
    public void Init_RegistersUpdate_At30Fps_WhenAnimateWaterIsTrue()
    {
        Mock<ISceneSetup> setup = MakeSetup();
        StubPreview sut = new() { Seed = 1, Radius = 4, Water = true };

        sut.Init(setup.Object);

        setup.Verify(s => s.RegisterUpdate(It.IsAny<Action>(), 30.0, It.IsAny<PauseBehavior>()),
            Times.Once, "the update callback must be registered at 30 fps");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Init — callbacks are invocable without throwing
    // --------------------------------------------------------------------------

    [Fact]
    public void Init_UpdateCallback_DoesNotThrow_WhenInvokedRepeatedly()
    {
        // Captures the update callback and invokes it multiple times to verify
        // the time counter increments without error.
        Action? capturedUpdate = null;
        Mock<ISceneSetup> setup = new();
        Mock<IFunctionHandle> handle = new();
        _ = setup.Setup(s => s.RegisterUpdate(It.IsAny<Action>(), It.IsAny<double>(), It.IsAny<PauseBehavior>()))
            .Callback<Action, double, PauseBehavior>((cb, _, _) => capturedUpdate = cb)
            .Returns(handle.Object);
        _ = setup.Setup(s => s.RegisterDraw(It.IsAny<Action>(), It.IsAny<double>(), It.IsAny<PauseBehavior>()))
            .Returns(handle.Object);

        StubPreview sut = new() { Seed = 1, Radius = 2, Water = true };
        sut.Init(setup.Object);

        _ = capturedUpdate.Should().NotBeNull();
        Action act = () => { for (int i = 0; i < 90; i++) capturedUpdate!(); };
        _ = act.Should().NotThrow(because: "the time counter must increment 90 ticks without error");
    }

    // --------------------------------------------------------------------------
    #endregion
}
