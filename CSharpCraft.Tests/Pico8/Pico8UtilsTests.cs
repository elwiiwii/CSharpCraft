using Xunit;
using FluentAssertions;
using CSharpCraft.Pico8;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Tests for pure utility functions in Pico8Utils.
/// Excludes methods requiring GraphicsDevice/Texture2D (ImageToColorArray, CreateTextureFromSpriteData)
/// and methods requiring live input state (IsBindingDown, Ptn).
/// </summary>
public class Pico8UtilsTests
{
    private static readonly List<Color> TestColors =
    [
        new(0, 0, 0),        // 0 black
        new(29, 43, 83),     // 1 dark blue
        new(126, 37, 83),    // 2 dark purple
        new(0, 135, 81),     // 3 dark green
        new(171, 82, 54),    // 4 brown
        new(95, 87, 79),     // 5 dark grey
        new(194, 195, 199),  // 6 light grey
        new(255, 241, 232),  // 7 white
        new(255, 0, 77),     // 8 red
        new(255, 163, 0),    // 9 orange
        new(255, 236, 39),   // 10 yellow
        new(0, 228, 54),     // 11 green
        new(41, 173, 255),   // 12 blue
        new(131, 118, 156),  // 13 indigo
        new(255, 119, 168),  // 14 pink
        new(255, 204, 170),  // 15 peach
    ];

    // ══════════════════════════════════════════════════
    // DataToColorArray
    // ══════════════════════════════════════════════════

    [Fact]
    public void DataToColorArray_SingleCharPerColor_ReturnsCorrectColors()
    {
        // Each character is a hex nibble mapped to a color index
        var result = Pico8Utils.DataToColorArray(TestColors, "07", 1);
        result.Should().HaveCount(2);
        result[0].Should().Be(TestColors[0]);
        result[1].Should().Be(TestColors[7]);
    }

    [Fact]
    public void DataToColorArray_MultipleColors()
    {
        var result = Pico8Utils.DataToColorArray(TestColors, "0123", 1);
        result.Should().HaveCount(4);
        result[0].Should().Be(TestColors[0]);
        result[1].Should().Be(TestColors[1]);
        result[2].Should().Be(TestColors[2]);
        result[3].Should().Be(TestColors[3]);
    }

    [Fact]
    public void DataToColorArray_HexCharacters_AtoF()
    {
        // a=10, b=11, c=12, d=13, e=14, f=15
        var result = Pico8Utils.DataToColorArray(TestColors, "af", 1);
        result.Should().HaveCount(2);
        result[0].Should().Be(TestColors[10]);
        result[1].Should().Be(TestColors[15]);
    }

    [Fact]
    public void DataToColorArray_TwoCharsPerColor_ParsesAsTwoDigitHex()
    {
        // "07" as a 2-char chunk = 0x07 = 7
        var result = Pico8Utils.DataToColorArray(TestColors, "07", 2);
        result.Should().HaveCount(1);
        result[0].Should().Be(TestColors[7]);
    }

    [Fact]
    public void DataToColorArray_WrapsIndex_Modulo16()
    {
        // Index 18 = 0x12 → 18 % 16 = 2
        var result = Pico8Utils.DataToColorArray(TestColors, "12", 2);
        result.Should().HaveCount(1);
        result[0].Should().Be(TestColors[2]); // 0x12 = 18, 18 % 16 = 2
    }

    [Fact]
    public void DataToColorArray_EmptyString_ReturnsEmpty()
    {
        var result = Pico8Utils.DataToColorArray(TestColors, "", 1);
        result.Should().BeEmpty();
    }

    // ══════════════════════════════════════════════════
    // DataToArray
    // ══════════════════════════════════════════════════

    [Fact]
    public void DataToArray_ParsesHexPairs()
    {
        var result = Pico8Utils.DataToArray("0a0f", 2);
        result.Should().HaveCount(2);
        result[0].Should().Be(10);  // 0x0a
        result[1].Should().Be(15);  // 0x0f
    }

    [Fact]
    public void DataToArray_SingleDigitChunks()
    {
        var result = Pico8Utils.DataToArray("3c7", 1);
        result.Should().HaveCount(3);
        result[0].Should().Be(3);
        result[1].Should().Be(12); // 0xc
        result[2].Should().Be(7);
    }

    [Fact]
    public void DataToArray_FourDigitChunks()
    {
        var result = Pico8Utils.DataToArray("00ff0100", 4);
        result.Should().HaveCount(2);
        result[0].Should().Be(255);   // 0x00ff
        result[1].Should().Be(256);   // 0x0100
    }

    [Fact]
    public void DataToArray_EmptyString_ReturnsEmpty()
    {
        var result = Pico8Utils.DataToArray("", 2);
        result.Should().BeEmpty();
    }

    [Fact]
    public void DataToArray_AllZeros()
    {
        var result = Pico8Utils.DataToArray("000000", 2);
        result.Should().HaveCount(3);
        result.Should().AllBeEquivalentTo(0);
    }

    [Fact]
    public void DataToArray_MaxByteValues()
    {
        var result = Pico8Utils.DataToArray("ffff", 2);
        result.Should().HaveCount(2);
        result[0].Should().Be(255);
        result[1].Should().Be(255);
    }

    // ══════════════════════════════════════════════════
    // MapDataToArray
    // ══════════════════════════════════════════════════

    [Fact]
    public void MapDataToArray_ParsesTwoCharChunks()
    {
        // Formula: (chunk[0] - 35) * 91 + chunk[1] - 35
        // '#' = 35, so "##" → (35-35)*91 + (35-35) = 0
        var result = Pico8Utils.MapDataToArray("##");
        result.Should().HaveCount(1);
        result[0].Should().Be(0);
    }

    [Fact]
    public void MapDataToArray_MultipleEntries()
    {
        var result = Pico8Utils.MapDataToArray("####");
        result.Should().HaveCount(2);
    }

    [Fact]
    public void MapDataToArray_EmptyString_ReturnsEmpty()
    {
        var result = Pico8Utils.MapDataToArray("");
        result.Should().BeEmpty();
    }

    [Fact]
    public void MapDataToArray_KnownValue()
    {
        // '$' = 36, '#' = 35 → (36-35)*91 + (35-35) = 91
        var result = Pico8Utils.MapDataToArray("$#");
        result.Should().HaveCount(1);
        result[0].Should().Be(91);
    }

    [Fact]
    public void MapDataToArray_AnotherKnownValue()
    {
        // '#' = 35, '$' = 36 → (35-35)*91 + (36-35) = 1
        var result = Pico8Utils.MapDataToArray("#$");
        result.Should().HaveCount(1);
        result[0].Should().Be(1);
    }

    // ══════════════════════════════════════════════════
    // MapFlip
    // ══════════════════════════════════════════════════

    [Fact]
    public void MapFlip_ReversesPairsOfCharacters()
    {
        var result = Pico8Utils.MapFlip("abcd");
        result.Should().Be("badc");
    }

    [Fact]
    public void MapFlip_SinglePair()
    {
        var result = Pico8Utils.MapFlip("ab");
        result.Should().Be("ba");
    }

    [Fact]
    public void MapFlip_EmptyString()
    {
        var result = Pico8Utils.MapFlip("");
        result.Should().BeEmpty();
    }

    [Fact]
    public void MapFlip_OddLength_HandlesLastCharacter()
    {
        // With odd length, last chunk has 1 char, reversing it stays same
        var result = Pico8Utils.MapFlip("abc");
        result.Should().Be("bac");
    }

    [Fact]
    public void MapFlip_DoubleFlip_RestoresOriginal()
    {
        const string original = "abcdef";
        var flipped = Pico8Utils.MapFlip(original);
        var restored = Pico8Utils.MapFlip(flipped);
        restored.Should().Be(original);
    }

    // ══════════════════════════════════════════════════
    // HexToColor
    // ══════════════════════════════════════════════════

    [Fact]
    public void HexToColor_ParsesRedWithHash()
    {
        var result = Pico8Utils.HexToColor("#FF0000");
        result.R.Should().Be(255);
        result.G.Should().Be(0);
        result.B.Should().Be(0);
    }

    [Fact]
    public void HexToColor_ParsesRedWithoutHash()
    {
        var result = Pico8Utils.HexToColor("FF0000");
        result.R.Should().Be(255);
        result.G.Should().Be(0);
        result.B.Should().Be(0);
    }

    [Fact]
    public void HexToColor_ParsesGreen()
    {
        var result = Pico8Utils.HexToColor("#00FF00");
        result.G.Should().Be(255);
        result.R.Should().Be(0);
        result.B.Should().Be(0);
    }

    [Fact]
    public void HexToColor_ParsesBlue()
    {
        var result = Pico8Utils.HexToColor("#0000FF");
        result.B.Should().Be(255);
        result.R.Should().Be(0);
        result.G.Should().Be(0);
    }

    [Fact]
    public void HexToColor_ParsesWhite()
    {
        var result = Pico8Utils.HexToColor("#FFFFFF");
        result.R.Should().Be(255);
        result.G.Should().Be(255);
        result.B.Should().Be(255);
    }

    [Fact]
    public void HexToColor_ParsesBlack()
    {
        var result = Pico8Utils.HexToColor("#000000");
        result.R.Should().Be(0);
        result.G.Should().Be(0);
        result.B.Should().Be(0);
    }

    [Fact]
    public void HexToColor_LowercaseHex()
    {
        var result = Pico8Utils.HexToColor("#ff8800");
        result.R.Should().Be(255);
        result.G.Should().Be(136);
        result.B.Should().Be(0);
    }

    [Fact]
    public void HexToColor_MixedCase()
    {
        var result = Pico8Utils.HexToColor("#Ff0088");
        result.R.Should().Be(255);
        result.G.Should().Be(0);
        result.B.Should().Be(136);
    }

    [Fact]
    public void HexToColor_Pico8DarkBlue()
    {
        // PICO-8 dark blue = #1D2B53
        var result = Pico8Utils.HexToColor("#1D2B53");
        result.R.Should().Be(29);
        result.G.Should().Be(43);
        result.B.Should().Be(83);
    }

    [Fact]
    public void HexToColor_Pico8Red()
    {
        // PICO-8 red = #FF004D
        var result = Pico8Utils.HexToColor("#FF004D");
        result.R.Should().Be(255);
        result.G.Should().Be(0);
        result.B.Should().Be(77);
    }
}
