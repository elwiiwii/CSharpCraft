using FluentAssertions;
using Microsoft.Xna.Framework;
using Xunit;

namespace CSharpCraft.Tests;

public partial class SpriteFlagMapManagerTests
{
    // -------------------------------------------------------------------------
    // Reload
    // -------------------------------------------------------------------------

    [Fact]
    public void Reload_RestoresSpritePixelToOriginal_AfterSetSpritePixel()
    {
        var (sfm, _, _) = MakeDefault(Color.Red);
        sfm.SetSpritePixel(0, 0, Color.Blue);

        sfm.Reload();

        sfm.GetSpritePixel(0, 0).Should().Be(Color.Red);
    }

    [Fact]
    public void Reload_IncrementsSpritesheetVersion()
    {
        var (sfm, _, _) = MakeDefault();
        int before = sfm.SpritesheetVersion;

        sfm.Reload();

        sfm.SpritesheetVersion.Should().BeGreaterThan(before);
    }

    [Fact]
    public void Reload_IncrementsMapVersion()
    {
        var (sfm, _, _) = MakeDefault();
        int before = sfm.MapVersion;

        sfm.Reload();

        sfm.MapVersion.Should().BeGreaterThan(before);
    }

    // -------------------------------------------------------------------------
    // GetSpriteSourceRect
    // -------------------------------------------------------------------------

    [Fact]
    public void GetSpriteSourceRect_Sprite0_ReturnsTopLeftRect()
    {
        var (sfm, _, _) = MakeDefault();

        var rect = sfm.GetSpriteSourceRect(0);

        rect.Should().Be(new Rectangle(0, 0, 8, 8));
    }

    [Fact]
    public void GetSpriteSourceRect_Sprite1_ReturnsSecondColumnRect()
    {
        // Sheet is 16x8 -> sprite 1 is at (8, 0)
        var (sfm, _, _) = MakeDefault();

        var rect = sfm.GetSpriteSourceRect(1);

        rect.Should().Be(new Rectangle(8, 0, 8, 8));
    }

    [Fact]
    public void GetSpriteSourceRect_MultiSprite_ReturnsWidenedRect()
    {
        var (sfm, _, _) = MakeDefault();

        var rect = sfm.GetSpriteSourceRect(0, widthSprites: 2, heightSprites: 1);

        rect.Should().Be(new Rectangle(0, 0, 16, 8));
    }

    [Fact]
    public void GetSpriteSourceRect_WiderSheet_WrapsToNextRow()
    {
        // 32x8 sheet -> 4 sprites in row 0; sprite 4 wraps to (0, 8) on a 32x16 sheet
        var sprite = MakeSolidTexture(Color.White, 32, 16);
        var map = MakeSolidTexture(Color.White, 8, 8);
        var sfm = MakeSfm(sprite, map);

        var rect = sfm.GetSpriteSourceRect(4);

        rect.Should().Be(new Rectangle(0, 8, 8, 8));
    }
}
