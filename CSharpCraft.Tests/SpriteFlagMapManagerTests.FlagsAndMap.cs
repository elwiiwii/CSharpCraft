using FluentAssertions;
using Microsoft.Xna.Framework;
using Xunit;

namespace CSharpCraft.Tests;

public partial class SpriteFlagMapManagerTests
{
    // -------------------------------------------------------------------------
    // GetFlag / SetFlag
    // -------------------------------------------------------------------------

    [Fact]
    public void GetFlag_Int_ReturnsZero_WhenNothingSet()
    {
        var (sfm, _, _) = MakeDefault();

        sfm.GetFlag(0).Should().Be(0);
    }

    [Fact]
    public void GetFlag_Int_ReturnsValueFromFlagString()
    {
        // "07" -> sprite 0 = 0x07, "03" -> sprite 1 = 0x03
        var sprite = MakeSolidTexture(Color.White, 16, 8);
        var map = MakeSolidTexture(Color.White, 8, 8);
        var sfm = MakeSfm(sprite, map, "0703");

        sfm.GetFlag(0).Should().Be(0x07);
        sfm.GetFlag(1).Should().Be(0x03);
    }

    [Fact]
    public void GetFlag_Int_ReturnsZero_ForOutOfBoundsSprite()
    {
        var (sfm, _, _) = MakeDefault();

        sfm.GetFlag(-1).Should().Be(0);
        sfm.GetFlag(999).Should().Be(0);
    }

    [Fact]
    public void GetFlag_Bool_ReturnsFalse_WhenBitNotSet()
    {
        var (sfm, _, _) = MakeDefault();

        sfm.GetFlag(0, 0).Should().BeFalse();
    }

    [Fact]
    public void GetFlag_Bool_ReturnsTrue_WhenBitSet()
    {
        var sprite = MakeSolidTexture(Color.White, 16, 8);
        var map = MakeSolidTexture(Color.White, 8, 8);
        var sfm = MakeSfm(sprite, map, "05"); // 0x05 = bits 0 and 2

        sfm.GetFlag(0, 0).Should().BeTrue();
        sfm.GetFlag(0, 1).Should().BeFalse();
        sfm.GetFlag(0, 2).Should().BeTrue();
    }

    [Fact]
    public void GetFlag_Bool_ReturnsFalse_ForOutOfBoundsSprite()
    {
        var (sfm, _, _) = MakeDefault();

        sfm.GetFlag(-1, 0).Should().BeFalse();
        sfm.GetFlag(999, 0).Should().BeFalse();
    }

    [Fact]
    public void GetFlag_Bool_ReturnsFalse_ForOutOfBoundsBit()
    {
        var sprite = MakeSolidTexture(Color.White, 16, 8);
        var map = MakeSolidTexture(Color.White, 8, 8);
        var sfm = MakeSfm(sprite, map, "FF"); // all bits set

        sfm.GetFlag(0, -1).Should().BeFalse();
        sfm.GetFlag(0, 8).Should().BeFalse();
    }

    [Fact]
    public void SetFlag_Int_SetsAndMasksToByteRange()
    {
        var (sfm, _, _) = MakeDefault();

        sfm.SetFlag(0, 0xAB);

        sfm.GetFlag(0).Should().Be(0xAB);
    }

    [Fact]
    public void SetFlag_Int_MasksHighBitsToFF()
    {
        var (sfm, _, _) = MakeDefault();

        sfm.SetFlag(0, 0x1FF); // exceeds byte range

        sfm.GetFlag(0).Should().Be(0xFF);
    }

    [Fact]
    public void SetFlag_Bool_SetsBit()
    {
        var (sfm, _, _) = MakeDefault();

        sfm.SetFlag(0, 3, true);

        sfm.GetFlag(0, 3).Should().BeTrue();
    }

    [Fact]
    public void SetFlag_Bool_ClearsBit()
    {
        var sprite = MakeSolidTexture(Color.White, 16, 8);
        var map = MakeSolidTexture(Color.White, 8, 8);
        var sfm = MakeSfm(sprite, map, "FF"); // all bits set

        sfm.SetFlag(0, 3, false);

        sfm.GetFlag(0, 3).Should().BeFalse();
        sfm.GetFlag(0, 0).Should().BeTrue(); // other bits untouched
    }

    // -------------------------------------------------------------------------
    // GetMapTile / SetMapTile
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(2, 0)]
    [InlineData(0, 1)]
    public void GetMapTile_ReturnsZero_ForOutOfBoundsCoordinates(int x, int y)
    {
        // 16x8 map texture -> 2x1 map tiles
        var (sfm, _, _) = MakeDefault();

        sfm.GetMapTile(x, y).Should().Be(0);
    }

    [Fact]
    public void SetMapTile_UpdatesReturnedTileValue()
    {
        // Need a 3-sprite sheet and a 3-tile-wide map so sprite 2 is valid
        var sprite = MakeSolidTexture(Color.White, 24, 8); // 3 sprites wide
        var map = MakeSolidTexture(Color.White, 24, 8);    // 3 tiles wide
        var sfm = MakeSfm(sprite, map);

        sfm.SetMapTile(1, 0, 2);

        sfm.GetMapTile(1, 0).Should().Be(2);
    }

    [Fact]
    public void SetMapTile_IncrementsMapVersionByOne()
    {
        var sprite = MakeSolidTexture(Color.White, 16, 8);
        var map = MakeSolidTexture(Color.White, 16, 8);
        var sfm = MakeSfm(sprite, map);
        int before = sfm.MapVersion;

        sfm.SetMapTile(0, 0, 0);

        sfm.MapVersion.Should().Be(before + 1);
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(2, 0, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 0, -1)]
    [InlineData(0, 0, 99)]
    public void SetMapTile_DoesNotUpdate_ForOOBArguments(int x, int y, int spriteNumber)
    {
        var (sfm, _, _) = MakeDefault();
        int beforeMap = sfm.MapVersion;

        sfm.SetMapTile(x, y, spriteNumber);

        sfm.MapVersion.Should().Be(beforeMap, because: "no valid update should have occurred");
    }
}
