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

    private static StubPreview MakeSut(int radius = 4, bool water = true)
        => new() { Seed = 1, Radius = radius, Water = water };

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Virtual contract
    // --------------------------------------------------------------------------

    [Fact]
    public void CenterOverride_DefaultsToNull()
    {
        // The default implementation of CenterOverride must return null so that
        // PcraftWorldSampler uses the spawn tile as center.
        StubPreview sut = MakeSut();

        _ = sut.CenterOverride.Should().BeNull(
            because: "CenterOverride must default to null so spawn tile is used as center");
    }

    [Fact]
    public void AnimateWater_DefaultsToTrue()
    {
        StubPreview sut = MakeSut();

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
        Mock<ISceneSetup> setup = new();
        StubPreview sut = MakeSut(radius: radius);
        int side = 2 * radius;

        sut.Init(setup.Object);

        setup.VerifySet(s => s.Resolution = (16 * side, 16 * side),
            $"radius={radius}: resolution must be 16\u00d7(2r) in both dimensions");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Update — water time counter
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_DoesNotThrow_WhenInvokedRepeatedly_AnimateWaterTrue()
    {
        StubPreview sut = MakeSut(radius: 2, water: true);
        sut.Init(new Mock<ISceneSetup>().Object);

        Action act = () => { for (int i = 0; i < 90; i++) sut.Update(); };

        _ = act.Should().NotThrow(because: "the time counter must increment 90 ticks without error");
    }

    [Fact]
    public void Update_DoesNotThrow_WhenAnimateWaterFalse()
    {
        StubPreview sut = MakeSut(radius: 2, water: false);
        sut.Init(new Mock<ISceneSetup>().Object);

        Action act = () => { for (int i = 0; i < 90; i++) sut.Update(); };

        _ = act.Should().NotThrow();
    }

    // --------------------------------------------------------------------------
    #endregion
}
