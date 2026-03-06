using FluentAssertions;
using Microsoft.Xna.Framework;
using Xunit;

namespace CSharpCraft.Tests;

public partial class SpriteFlagMapManagerTests
{
    // -------------------------------------------------------------------------
    // Constructor validation
    // -------------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenSpriteWidthNotMultipleOf8()
    {
        var badSprite = MakeSolidTexture(Color.White, 10, 8);
        var map = MakeSolidTexture(Color.White, 8, 8);

        var act = () => MakeSfm(badSprite, map);

        act.Should().Throw<ArgumentException>().WithParameterName("spriteTexture");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenSpriteHeightNotMultipleOf8()
    {
        var badSprite = MakeSolidTexture(Color.White, 8, 10);
        var map = MakeSolidTexture(Color.White, 8, 8);

        var act = () => MakeSfm(badSprite, map);

        act.Should().Throw<ArgumentException>().WithParameterName("spriteTexture");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenMapWidthNotMultipleOf8()
    {
        var sprite = MakeSolidTexture(Color.White, 8, 8);
        var badMap = MakeSolidTexture(Color.White, 10, 8);

        var act = () => MakeSfm(sprite, badMap);

        act.Should().Throw<ArgumentException>().WithParameterName("mapTexture");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenMapHeightNotMultipleOf8()
    {
        var sprite = MakeSolidTexture(Color.White, 8, 8);
        var badMap = MakeSolidTexture(Color.White, 8, 10);

        var act = () => MakeSfm(sprite, badMap);

        act.Should().Throw<ArgumentException>().WithParameterName("mapTexture");
    }

    // -------------------------------------------------------------------------
    // GetSpritePixel
    // -------------------------------------------------------------------------

    [Fact]
    public void GetSpritePixel_ReturnsSeededColor_ForValidCoordinate()
    {
        var (sfm, _, _) = MakeDefault(Color.Cyan);

        sfm.GetSpritePixel(0, 0).Should().Be(Color.Cyan);
    }

    [Fact]
    public void GetSpritePixel_ReturnsSeededColor_ForLastValidCoordinate()
    {
        var (sfm, _, _) = MakeDefault(Color.Magenta);

        // 16x8 sheet: last valid pixel is (15, 7)
        sfm.GetSpritePixel(15, 7).Should().Be(Color.Magenta);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(16, 0)]
    [InlineData(0, 8)]
    public void GetSpritePixel_ReturnsBlack_ForOutOfBoundsCoordinates(int x, int y)
    {
        var (sfm, _, _) = MakeDefault();

        sfm.GetSpritePixel(x, y).Should().Be(Color.Black);
    }

    // -------------------------------------------------------------------------
    // SetSpritePixel
    // -------------------------------------------------------------------------

    [Fact]
    public void SetSpritePixel_WritesColorAtValidCoordinate()
    {
        var (sfm, _, _) = MakeDefault(Color.Red);

        sfm.SetSpritePixel(3, 3, Color.Blue);

        sfm.GetSpritePixel(3, 3).Should().Be(Color.Blue);
    }

    [Fact]
    public void SetSpritePixel_IncrementsSpritesheetVersionByOne()
    {
        var (sfm, _, _) = MakeDefault();
        int before = sfm.SpritesheetVersion;

        sfm.SetSpritePixel(0, 0, Color.Blue);

        sfm.SpritesheetVersion.Should().Be(before + 1);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(16, 0)]
    [InlineData(0, 8)]
    public void SetSpritePixel_DoesNotThrow_ForOutOfBoundsCoordinates(int x, int y)
    {
        var (sfm, _, _) = MakeDefault();

        var act = () => sfm.SetSpritePixel(x, y, Color.Blue);

        act.Should().NotThrow();
    }
}
