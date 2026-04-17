using CSharpCraft.PcraftPreview;
using CSharpCraft.PcraftPreview.Noise;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftPreview;

public sealed class PcraftWorldSamplerTests
{
    // --------------------------------------------------------------------------
    #region Tiles dimensions
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(1)]  // minimal radius
    [InlineData(2)]  // small preview
    [InlineData(4)]  // typical radius
    [InlineData(8)]  // larger preview
    public void Sample_TilesDimensions_AreTwiceRadiusPlusOne(int radius)
    {
        var result = PcraftWorldSampler.Sample(1L, radius, forceCenterX: 32, forceCenterY: 32);

        int expectedSide = 2 * radius + 1;
        result.Tiles.GetLength(0).Should().Be(expectedSide,
            because: $"radius={radius}: Tiles first dimension must be 2×radius+1");
        result.Tiles.GetLength(1).Should().Be(expectedSide,
            because: $"radius={radius}: Tiles second dimension must be 2×radius+1");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Center tile
    // --------------------------------------------------------------------------

    [Fact]
    public void Sample_CenterTile_EqualsForcedCoordinates_WhenForceCenterProvided()
    {
        var result = PcraftWorldSampler.Sample(0L, radius: 4, forceCenterX: 30, forceCenterY: 35);

        result.CenterTileX.Should().Be(30,
            because: "forced center X must be used as CenterTileX");
        result.CenterTileY.Should().Be(35,
            because: "forced center Y must be used as CenterTileY");
    }

    [Theory]
    [InlineData(20, 20)]  // away from spawn region
    [InlineData(32, 32)]  // grid centre
    [InlineData(40, 40)]  // typical forced position
    public void Sample_CenterTile_EqualsForcedCoordinates_ForVariousPositions(int fx, int fy)
    {
        var result = PcraftWorldSampler.Sample(7L, radius: 2, forceCenterX: fx, forceCenterY: fy);

        result.CenterTileX.Should().Be(fx);
        result.CenterTileY.Should().Be(fy);
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(2L)]
    [InlineData(42L)]
    public void Sample_CenterTile_EqualsSpawnTile_WhenNoCenterForcedAndSpawnFound(long seed)
    {
        var result = PcraftWorldSampler.Sample(seed, radius: 4);

        if (result.SpawnTileX >= 0)
        {
            result.CenterTileX.Should().Be(result.SpawnTileX,
                because: "CenterTile must equal the spawn tile when no center override is provided");
            result.CenterTileY.Should().Be(result.SpawnTileY,
                because: "CenterTile must equal the spawn tile when no center override is provided");
        }
        // If no spawn found: CenterTile defaults to grid centre — no assertion made here.
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Spawn tile
    // --------------------------------------------------------------------------

    [Fact]
    public void Sample_SpawnTileXAndY_AreBothValidOrBothNegativeOne()
    {
        // SpawnTileX >= 0 iff SpawnTileY >= 0 — they must be a consistent pair.
        var result = PcraftWorldSampler.Sample(42L, radius: 4);

        bool xFound = result.SpawnTileX >= 0;
        bool yFound = result.SpawnTileY >= 0;
        xFound.Should().Be(yFound,
            because: "SpawnTileX and SpawnTileY must both be valid or both -1");
    }

    [Theory]
    [InlineData(1L)]
    [InlineData(2L)]
    [InlineData(42L)]
    public void Sample_SpawnTileId_IsInValidSpawnSet_WhenSpawnFound(long seed)
    {
        var result = PcraftWorldSampler.Sample(seed, radius: 4);

        if (result.SpawnTileX < 0)
            return; // no spawn for this seed — SpawnFinder contract tested separately

        // Reconstruct the same classifier with matching noise parameters.
        var cur  = new SeededNoiseGrid(seed, 64, 64, 64, 0.9, 0.2, 0);
        var cur2 = new SeededNoiseGrid(seed, 64, 64,  8, 0.9, 0.4, 1);
        var cur3 = new SeededNoiseGrid(seed, 64, 64,  8, 0.9, 0.3, 2);
        var cur4 = new SeededNoiseGrid(seed, 64, 64,  4, 0.8, 1.1, 3);
        var classifier = new MapClassifier(cur, cur2, cur3, cur4, 64, 64, 0, 1, 2, 3, 4);

        classifier.ClassifyTile(result.SpawnTileX, result.SpawnTileY)
            .Should().BeOneOf(new[] { 1, 2 },
                because: "SpawnTile must classify to a valid spawn id (sand=1 or rare=2)");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region RndWat
    // --------------------------------------------------------------------------

    [Fact]
    public void Sample_RndWat_HasDimensions16x16()
    {
        var result = PcraftWorldSampler.Sample(0L, radius: 2, forceCenterX: 32, forceCenterY: 32);

        result.RndWat.GetLength(0).Should().Be(16,
            because: "RndWat first dimension must be 16");
        result.RndWat.GetLength(1).Should().Be(16,
            because: "RndWat second dimension must be 16");
    }

    [Fact]
    public void Sample_RndWat_AllValuesInRangeZeroToOneHundred()
    {
        var result = PcraftWorldSampler.Sample(5L, radius: 2, forceCenterX: 32, forceCenterY: 32);

        for (int i = 0; i < 16; i++)
        for (int j = 0; j < 16; j++)
            result.RndWat[i, j].Should().BeInRange(0.0, 100.0,
                because: $"RndWat[{i},{j}] must be in [0, 100)");
    }

    [Fact]
    public void Sample_RndWat_IsDeterministic_ForSameSeed()
    {
        var first  = PcraftWorldSampler.Sample(13L, radius: 2, forceCenterX: 32, forceCenterY: 32);
        var second = PcraftWorldSampler.Sample(13L, radius: 2, forceCenterX: 32, forceCenterY: 32);

        for (int i = 0; i < 16; i++)
        for (int j = 0; j < 16; j++)
            second.RndWat[i, j].Should().Be(first.RndWat[i, j],
                because: $"RndWat[{i},{j}] must be identical across repeated calls with the same seed");
    }

    [Fact]
    public void Sample_RndWat_DiffersAcrossSeeds()
    {
        var a = PcraftWorldSampler.Sample(1L, radius: 2, forceCenterX: 32, forceCenterY: 32);
        var b = PcraftWorldSampler.Sample(2L, radius: 2, forceCenterX: 32, forceCenterY: 32);

        bool anyDiffers = false;
        for (int i = 0; i < 16 && !anyDiffers; i++)
        for (int j = 0; j < 16 && !anyDiffers; j++)
            if (a.RndWat[i, j] != b.RndWat[i, j])
                anyDiffers = true;

        anyDiffers.Should().BeTrue(
            because: "different seeds must produce different water animation tables");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Tile validity
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(1L)]
    [InlineData(2L)]
    [InlineData(42L)]
    public void Sample_AllTileIds_AreInValidSet(long seed)
    {
        var result = PcraftWorldSampler.Sample(seed, radius: 4, forceCenterX: 32, forceCenterY: 32);

        int[] valid = [0, 1, 2, 3, 4];
        int side = result.Tiles.GetLength(0);

        for (int i = 0; i < side; i++)
        for (int j = 0; j < side; j++)
            result.Tiles[i, j].Should().BeOneOf(valid,
                because: $"seed={seed}: Tiles[{i},{j}] must be one of the five tile ids {{0..4}}");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Determinism
    // --------------------------------------------------------------------------

    [Fact]
    public void Sample_IsIdempotent_ForSameSeed()
    {
        var first  = PcraftWorldSampler.Sample(99L, radius: 3, forceCenterX: 32, forceCenterY: 32);
        var second = PcraftWorldSampler.Sample(99L, radius: 3, forceCenterX: 32, forceCenterY: 32);

        int side = first.Tiles.GetLength(0);
        for (int i = 0; i < side; i++)
        for (int j = 0; j < side; j++)
            second.Tiles[i, j].Should().Be(first.Tiles[i, j],
                because: $"Tiles[{i},{j}] must be identical across repeated calls with the same seed");

        second.SpawnTileX.Should().Be(first.SpawnTileX);
        second.SpawnTileY.Should().Be(first.SpawnTileY);
        second.CenterTileX.Should().Be(first.CenterTileX);
        second.CenterTileY.Should().Be(first.CenterTileY);
    }

    [Fact]
    public void Sample_ProducesDifferentTiles_ForDifferentSeeds()
    {
        var a = PcraftWorldSampler.Sample(1L, radius: 4, forceCenterX: 32, forceCenterY: 32);
        var b = PcraftWorldSampler.Sample(2L, radius: 4, forceCenterX: 32, forceCenterY: 32);

        int side = a.Tiles.GetLength(0);
        bool anyDiffers = false;
        for (int i = 0; i < side && !anyDiffers; i++)
        for (int j = 0; j < side && !anyDiffers; j++)
            if (a.Tiles[i, j] != b.Tiles[i, j])
                anyDiffers = true;

        anyDiffers.Should().BeTrue(
            because: "different seeds should produce different tile maps");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Tile slice correctness
    // --------------------------------------------------------------------------

    [Fact]
    public void Sample_Tiles00_MatchesDirectClassification_AtRadius0()
    {
        // With radius=0, Tiles is 1×1 and Tiles[0,0] must equal the direct
        // classification of the forced center tile using the same noise parameters.
        const long Seed = 42L;
        const int Cx = 30, Cy = 30;

        var result = PcraftWorldSampler.Sample(Seed, radius: 0, forceCenterX: Cx, forceCenterY: Cy);

        var cur  = new SeededNoiseGrid(Seed, 64, 64, 64, 0.9, 0.2, 0);
        var cur2 = new SeededNoiseGrid(Seed, 64, 64,  8, 0.9, 0.4, 1);
        var cur3 = new SeededNoiseGrid(Seed, 64, 64,  8, 0.9, 0.3, 2);
        var cur4 = new SeededNoiseGrid(Seed, 64, 64,  4, 0.8, 1.1, 3);
        var classifier = new MapClassifier(cur, cur2, cur3, cur4, 64, 64, 0, 1, 2, 3, 4);

        result.Tiles[0, 0].Should().Be(classifier.ClassifyTile(Cx, Cy),
            because: "Tiles[0,0] with radius=0 must equal the direct classification of the center tile");
    }

    [Fact]
    public void Sample_TileSlice_CornerAndCenterMatchDirectClassification()
    {
        // Verifies Tiles[0,0] maps to (Cx-R, Cy-R), Tiles[R,R] to (Cx, Cy),
        // and Tiles[2R,2R] to (Cx+R, Cy+R).
        const long Seed = 7L;
        const int Cx = 32, Cy = 32, R = 3;

        var result = PcraftWorldSampler.Sample(Seed, radius: R, forceCenterX: Cx, forceCenterY: Cy);

        var cur  = new SeededNoiseGrid(Seed, 64, 64, 64, 0.9, 0.2, 0);
        var cur2 = new SeededNoiseGrid(Seed, 64, 64,  8, 0.9, 0.4, 1);
        var cur3 = new SeededNoiseGrid(Seed, 64, 64,  8, 0.9, 0.3, 2);
        var cur4 = new SeededNoiseGrid(Seed, 64, 64,  4, 0.8, 1.1, 3);
        var classifier = new MapClassifier(cur, cur2, cur3, cur4, 64, 64, 0, 1, 2, 3, 4);

        result.Tiles[0, 0].Should().Be(classifier.ClassifyTile(Cx - R, Cy - R),
            because: "Tiles[0,0] must be the top-left corner of the slice at (Cx-R, Cy-R)");
        result.Tiles[R, R].Should().Be(classifier.ClassifyTile(Cx, Cy),
            because: "Tiles[R,R] (center index) must be the center tile classification");
        result.Tiles[2 * R, 2 * R].Should().Be(classifier.ClassifyTile(Cx + R, Cy + R),
            because: "Tiles[2R,2R] must be the bottom-right corner of the slice at (Cx+R, Cy+R)");
    }

    // --------------------------------------------------------------------------
    #endregion
}
