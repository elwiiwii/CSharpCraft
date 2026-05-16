using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftPreview;
using CSharpCraft.PcraftSeeded;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftPreview;

// --------------------------------------------------------------------------
// Verifies that PreviewDrawer.WatVal uses world tile coordinates so the
// texture lookup matches the game's PcraftBase.WatVal for the same tile.
// --------------------------------------------------------------------------

public sealed class PreviewDrawerTests
{
    private const long Seed = 12345L;

    // --------------------------------------------------------------------------
    #region WatVal — world-coordinate consistency with the game
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(0, 0)]     // top-left corner of map
    [InlineData(7, 7)]     // mid-range both axes
    [InlineData(15, 15)]   // boundary index (wraps to 30 % 16 = 14)
    [InlineData(8, 0)]     // x wraps: 8*2 = 16 % 16 = 0
    [InlineData(31, 28)]   // typical spawn-area world coordinates
    public void WatVal_MatchesRndWatLookup_ForWorldCoordinates(int worldX, int worldY)
    {
        double[,] rndWat = PcraftWorldSampler.Sample(Seed, radius: 4).RndWat;

        int expectedXi = (int)(Math.Abs(worldX * 2.0) % 16);
        int expectedYj = (int)(Math.Abs(worldY * 2.0) % 16);
        F32 expected = F32.FromDouble(rndWat[expectedXi, expectedYj]);

        F32 actual = PreviewDrawer.WatVal(worldX, worldY, rndWat);

        _ = actual.Should().Be(expected,
            because: $"WatVal at world ({worldX},{worldY}) must index RndWat[{expectedXi},{expectedYj}]");
    }

    [Theory]
    [InlineData(3, 5)]    // half-tile x offset
    [InlineData(0, 9)]    // half-tile y offset
    [InlineData(12, 12)]  // half-tile both
    public void WatVal_MatchesRndWatLookup_ForHalfTileWorldCoordinates(int baseTileX, int baseTileY)
    {
        double[,] rndWat = PcraftWorldSampler.Sample(Seed, radius: 4).RndWat;
        double i = baseTileX + 0.5;
        double j = baseTileY + 0.5;

        int expectedXi = (int)(Math.Abs(i * 2) % 16);
        int expectedYj = (int)(Math.Abs(j * 2) % 16);
        F32 expected = F32.FromDouble(rndWat[expectedXi, expectedYj]);

        F32 actual = PreviewDrawer.WatVal(i, j, rndWat);

        _ = actual.Should().Be(expected,
            because: $"half-tile WatVal at ({i},{j}) must index RndWat[{expectedXi},{expectedYj}]");
    }

    [Fact]
    public void WatVal_DiffersAtDifferentWorldCoordinates_WhenRndWatVariesAcrossTable()
    {
        // Verify world offset matters: tile (0,0) and tile (1,0) should differ
        // whenever RndWat[0,0] != RndWat[2,0] (they virtually always will for any real seed).
        double[,] rndWat = PcraftWorldSampler.Sample(Seed, radius: 4).RndWat;

        F32 atOrigin = PreviewDrawer.WatVal(0, 0, rndWat);
        F32 atNextTile = PreviewDrawer.WatVal(1, 0, rndWat);

        // These index RndWat[0,0] and RndWat[2,0] respectively — different slots.
        _ = atOrigin.Should().NotBe(atNextTile,
            because: "consecutive world tiles must index different RndWat slots");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region WatVal — preview origin equals game lookup for centre tile
    // --------------------------------------------------------------------------

    [Fact]
    public void WatVal_AtCentreTile_MatchesGameRndWatLookup()
    {
        // The centre tile in the preview has world coordinates (CenterTileX, CenterTileY).
        // PreviewDrawer should look it up the same way the game does.
        SampleResult result = PcraftWorldSampler.Sample(Seed, radius: 4);
        F32[][] gameRndWat = SeededMapGenerator.InitRndWat(Seed);

        int xi = (int)(Math.Abs(result.CenterTileX * 2.0) % 16);
        int yj = (int)(Math.Abs(result.CenterTileY * 2.0) % 16);

        F32 fromPreview = PreviewDrawer.WatVal(result.CenterTileX, result.CenterTileY, result.RndWat);
        F32 fromGame = gameRndWat[xi][yj];

        _ = fromPreview.Should().Be(fromGame,
            because: "preview WatVal at centre tile must equal the game's Rndwat lookup");
    }

    // --------------------------------------------------------------------------
    #endregion
}
