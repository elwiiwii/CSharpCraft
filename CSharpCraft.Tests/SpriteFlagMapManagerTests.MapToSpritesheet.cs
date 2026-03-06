using FluentAssertions;
using Microsoft.Xna.Framework;
using Xunit;

namespace CSharpCraft.Tests;

public partial class SpriteFlagMapManagerTests
{
    // -------------------------------------------------------------------------
    // MapToSpritesheet
    // -------------------------------------------------------------------------

    [Fact]
    public void MapToSpritesheet_Length2_CopiesBothDigitsFromSingleCell()
    {
        var sprite = MakeSolidTexture(Color.White, 512, 8);
        var map = MakeSolidTexture(Color.White, 8, 8);
        var sfm = MakeSfm(sprite, map);

        // 35 in base 16 => [2,3]
        sfm.SetMapTile(0, 0, 35);

        sfm.MapToSpritesheet(0, 0, 0, 0, 2, 16);

        Color c2 = Pico8.Pico8.Palette.ElementAt(2).Key;
        Color c3 = Pico8.Pico8.Palette.ElementAt(3).Key;

        sfm.GetSpritePixel(0, 0).Should().Be(c2);
        sfm.GetSpritePixel(1, 0).Should().Be(c3);
    }

    [Fact]
    public void MapToSpritesheet_UsesDestinationOffset_WhenWritingPixels()
    {
        var sprite = MakeSolidTexture(Color.White, 512, 8);
        var map = MakeSolidTexture(Color.White, 8, 8);
        var sfm = MakeSfm(sprite, map);

        // 18 in base 16 => [1,2]
        sfm.SetMapTile(0, 0, 18);

        sfm.MapToSpritesheet(0, 0, 4, 0, 2, 16);

        Color c1 = Pico8.Pico8.Palette.ElementAt(1).Key;
        Color c2 = Pico8.Pico8.Palette.ElementAt(2).Key;

        sfm.GetSpritePixel(4, 0).Should().Be(c1);
        sfm.GetSpritePixel(5, 0).Should().Be(c2);
    }

    [Fact]
    public void MapToSpritesheet_Base16_SplitsEachCellIntoTwoPixels()
    {
        // 64 sprites available (0..63), map is 2 cells wide.
        var sprite = MakeSolidTexture(Color.White, 512, 8);
        var map = MakeSolidTexture(Color.White, 16, 8);
        var sfm = MakeSfm(sprite, map);

        // cell(0,0)=33 -> [33/16=2, 33%16=1]
        // cell(1,0)=18 -> [18/16=1, 18%16=2]
        sfm.SetMapTile(0, 0, 33);
        sfm.SetMapTile(1, 0, 18);

        sfm.MapToSpritesheet(0, 0, 0, 0, 4, 16);

        Color c2 = Pico8.Pico8.Palette.ElementAt(2).Key;
        Color c1 = Pico8.Pico8.Palette.ElementAt(1).Key;

        sfm.GetSpritePixel(0, 0).Should().Be(c2);
        sfm.GetSpritePixel(1, 0).Should().Be(c1);
        sfm.GetSpritePixel(2, 0).Should().Be(c1);
        sfm.GetSpritePixel(3, 0).Should().Be(c2);
    }

    [Fact]
    public void MapToSpritesheet_OddLength_CopiesOnlyFirstHalfOfLastCell()
    {
        // 64 sprites available (0..63), map is 2 cells wide.
        var sprite = MakeSolidTexture(Color.White, 512, 8);
        var map = MakeSolidTexture(Color.White, 16, 8);
        var sfm = MakeSfm(sprite, map);

        // cell(0,0)=47 -> [2,15]
        // cell(1,0)=31 -> [1,15]
        // length=3 => [2,15,1]
        sfm.SetMapTile(0, 0, 47);
        sfm.SetMapTile(1, 0, 31);

        sfm.MapToSpritesheet(0, 0, 0, 0, 3, 16);

        Color c2 = Pico8.Pico8.Palette.ElementAt(2).Key;
        Color c15 = Pico8.Pico8.Palette.ElementAt(15).Key;
        Color c1 = Pico8.Pico8.Palette.ElementAt(1).Key;

        sfm.GetSpritePixel(0, 0).Should().Be(c2);
        sfm.GetSpritePixel(1, 0).Should().Be(c15);
        sfm.GetSpritePixel(2, 0).Should().Be(c1);
    }

    [Fact]
    public void MapToSpritesheet_UsesProvidedBaseForSplit()
    {
        // 64 sprites available; use base 10 split.
        var sprite = MakeSolidTexture(Color.White, 512, 8);
        var map = MakeSolidTexture(Color.White, 8, 8);
        var sfm = MakeSfm(sprite, map);

        // 27 in base 10 => [2,7]
        sfm.SetMapTile(0, 0, 27);

        sfm.MapToSpritesheet(0, 0, 5, 0, 2, 10);

        Color c2 = Pico8.Pico8.Palette.ElementAt(2).Key;
        Color c7 = Pico8.Pico8.Palette.ElementAt(7).Key;

        sfm.GetSpritePixel(5, 0).Should().Be(c2);
        sfm.GetSpritePixel(6, 0).Should().Be(c7);
    }

    // -------------------------------------------------------------------------
    // MapToSpritesheet (rectangle overload)
    // -------------------------------------------------------------------------

    [Fact]
    public void MapToSpritesheet_Rectangle_FillsDestinationArea_WhenSourceExactlyMatches()
    {
        var sprite = MakeSolidTexture(Color.White, 512, 8);
        var map = MakeSolidTexture(Color.White, 16, 16); // 2x2 map cells
        var sfm = MakeSfm(sprite, map);

        // Source stream in base 16 (row-major by cells):
        // (0,0)=33 -> [2,1], (1,0)=18 -> [1,2], (0,1)=47 -> [2,15], (1,1)=31 -> [1,15]
        // Final stream: [2,1,1,2,2,15,1,15]
        sfm.SetMapTile(0, 0, 33);
        sfm.SetMapTile(1, 0, 18);
        sfm.SetMapTile(0, 1, 47);
        sfm.SetMapTile(1, 1, 31);

        sfm.MapToSpritesheet(0, 0, 2, 2, 4, 0, 4, 2, 16);

        Color c1 = Pico8.Pico8.Palette.ElementAt(1).Key;
        Color c2 = Pico8.Pico8.Palette.ElementAt(2).Key;
        Color c15 = Pico8.Pico8.Palette.ElementAt(15).Key;

        sfm.GetSpritePixel(4, 0).Should().Be(c2);
        sfm.GetSpritePixel(5, 0).Should().Be(c1);
        sfm.GetSpritePixel(6, 0).Should().Be(c1);
        sfm.GetSpritePixel(7, 0).Should().Be(c2);
        sfm.GetSpritePixel(4, 1).Should().Be(c2);
        sfm.GetSpritePixel(5, 1).Should().Be(c15);
        sfm.GetSpritePixel(6, 1).Should().Be(c1);
        sfm.GetSpritePixel(7, 1).Should().Be(c15);
    }

    [Fact]
    public void MapToSpritesheet_Rectangle_StopsWhenDestinationAreaIsFull()
    {
        var sprite = MakeSolidTexture(Color.White, 512, 8);
        var map = MakeSolidTexture(Color.White, 16, 16); // 2x2 map cells => 8 source pixels
        var sfm = MakeSfm(sprite, map);

        sfm.SetMapTile(0, 0, 33); // [2,1]
        sfm.SetMapTile(1, 0, 18); // [1,2]
        sfm.SetMapTile(0, 1, 47); // [2,15]
        sfm.SetMapTile(1, 1, 31); // [1,15]

        // Destination area is only 3x2 = 6 pixels.
        sfm.MapToSpritesheet(0, 0, 2, 2, 0, 0, 3, 2, 16);

        Color c1 = Pico8.Pico8.Palette.ElementAt(1).Key;
        Color c2 = Pico8.Pico8.Palette.ElementAt(2).Key;
        Color c15 = Pico8.Pico8.Palette.ElementAt(15).Key;

        // Copied stream prefix: [2,1,1,2,2,15]
        sfm.GetSpritePixel(0, 0).Should().Be(c2);
        sfm.GetSpritePixel(1, 0).Should().Be(c1);
        sfm.GetSpritePixel(2, 0).Should().Be(c1);
        sfm.GetSpritePixel(0, 1).Should().Be(c2);
        sfm.GetSpritePixel(1, 1).Should().Be(c2);
        sfm.GetSpritePixel(2, 1).Should().Be(c15);

        // Destination remainder outside 3x2 should be untouched.
        sfm.GetSpritePixel(3, 1).Should().Be(Color.White);
    }

    [Fact]
    public void MapToSpritesheet_Rectangle_LeavesDestinationRemainderUnchanged_WhenSourceRunsOut()
    {
        var sprite = MakeSolidTexture(Color.White, 512, 8);
        var map = MakeSolidTexture(Color.White, 8, 8); // 1x1 map cell => 2 source pixels
        var sfm = MakeSfm(sprite, map);

        sfm.SetMapTile(0, 0, 27); // base 10 => [2,7]

        // Destination area is 4 pixels; only first 2 should be written.
        sfm.MapToSpritesheet(0, 0, 1, 1, 0, 0, 4, 1, 10);

        Color c2 = Pico8.Pico8.Palette.ElementAt(2).Key;
        Color c7 = Pico8.Pico8.Palette.ElementAt(7).Key;

        sfm.GetSpritePixel(0, 0).Should().Be(c2);
        sfm.GetSpritePixel(1, 0).Should().Be(c7);
        sfm.GetSpritePixel(2, 0).Should().Be(Color.White);
        sfm.GetSpritePixel(3, 0).Should().Be(Color.White);
    }
}
