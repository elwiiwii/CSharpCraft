using CSharpCraft.PcraftPreview.Noise;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftPreview.Noise;

public sealed class SeededNoiseGridTests
{
    // --------------------------------------------------------------------------
    #region Constructor argument validation
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(3)]   // odd prime
    [InlineData(5)]   // odd prime
    [InlineData(6)]   // even but not power of 2
    [InlineData(7)]   // 2^3 - 1
    [InlineData(9)]   // 3^2
    [InlineData(12)]  // multiple of 4 but not a power of 2
    [InlineData(24)]  // multiple of 8 but not a power of 2
    public void Constructor_ThrowsArgumentException_WhenGridSxIsNotPowerOfTwo(int sx)
    {
        var act = () => new SeededNoiseGrid(masterSeed: 1L, gridSx: sx, gridSy: 4,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        act.Should().Throw<ArgumentException>().WithParameterName("gridSx");
    }

    [Theory]
    [InlineData(3)]   // odd prime
    [InlineData(5)]   // odd prime
    [InlineData(6)]   // even but not power of 2
    [InlineData(7)]   // 2^3 - 1
    [InlineData(9)]   // 3^2
    [InlineData(12)]  // multiple of 4 but not a power of 2
    public void Constructor_ThrowsArgumentException_WhenGridSyIsNotPowerOfTwo(int sy)
    {
        var act = () => new SeededNoiseGrid(masterSeed: 1L, gridSx: 4, gridSy: sy,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        act.Should().Throw<ArgumentException>().WithParameterName("gridSy");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenGridSxIsZero()
    {
        var act = () => new SeededNoiseGrid(masterSeed: 1L, gridSx: 0, gridSy: 4,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        act.Should().Throw<ArgumentException>().WithParameterName("gridSx");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenGridSyIsZero()
    {
        var act = () => new SeededNoiseGrid(masterSeed: 1L, gridSx: 4, gridSy: 0,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        act.Should().Throw<ArgumentException>().WithParameterName("gridSy");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenGridSxIsNegative()
    {
        var act = () => new SeededNoiseGrid(masterSeed: 1L, gridSx: -4, gridSy: 4,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        act.Should().Throw<ArgumentException>().WithParameterName("gridSx");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenGridSyIsNegative()
    {
        var act = () => new SeededNoiseGrid(masterSeed: 1L, gridSx: 4, gridSy: -4,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        act.Should().Throw<ArgumentException>().WithParameterName("gridSy");
    }

    [Theory]
    [InlineData(4,   4)]    // smallest valid grid
    [InlineData(8,  16)]    // non-square power-of-2
    [InlineData(16,  8)]    // non-square power-of-2, flipped
    [InlineData(64, 64)]    // full island size
    [InlineData(32, 32)]    // cave size
    public void Constructor_DoesNotThrow_WhenDimensionsArePowersOfTwo(int sx, int sy)
    {
        var act = () => new SeededNoiseGrid(masterSeed: 1L, gridSx: sx, gridSy: sy,
            featStep: sx, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        act.Should().NotThrow();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Constant cells — corners and boundary edges
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(0,  0)]   // top-left corner
    [InlineData(4,  0)]   // top-right corner
    [InlineData(0,  4)]   // bottom-left corner
    [InlineData(4,  4)]   // bottom-right corner
    public void GetValue_ReturnsExactlyHalf_ForAllFourCorners(int x, int y)
    {
        var grid = new SeededNoiseGrid(masterSeed: 99L, gridSx: 4, gridSy: 4,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        grid.GetValue(x, y).Should().Be(0.5, because: "corners are always initialised to 0.5 by the algorithm");
    }

    [Theory]
    [InlineData(1)]  // right boundary, interior y
    [InlineData(2)]  // right boundary, mid y
    [InlineData(3)]  // right boundary, interior y
    public void GetValue_ReturnsExactlyHalf_ForRightBoundaryColumn(int y)
    {
        // x == gridSx, 0 < y < gridSy — these cells are never written by the algorithm
        var grid = new SeededNoiseGrid(masterSeed: 42L, gridSx: 4, gridSy: 4,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        grid.GetValue(4, y).Should().Be(0.5, because: "right-boundary non-corner cells are constant 0.5");
    }

    [Theory]
    [InlineData(1)]  // bottom boundary, interior x
    [InlineData(2)]  // bottom boundary, mid x
    [InlineData(3)]  // bottom boundary, interior x
    public void GetValue_ReturnsExactlyHalf_ForBottomBoundaryRow(int x)
    {
        // y == gridSy, 0 < x < gridSx — these cells are never written by the algorithm
        var grid = new SeededNoiseGrid(masterSeed: 42L, gridSx: 4, gridSy: 4,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        grid.GetValue(x, 4).Should().Be(0.5, because: "bottom-boundary non-corner cells are constant 0.5");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Determinism and memoization
    // --------------------------------------------------------------------------

    [Fact]
    public void GetValue_ReturnsSameResult_OnRepeatedCallsToSameCell()
    {
        var grid = new SeededNoiseGrid(masterSeed: 77L, gridSx: 4, gridSy: 4,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        var first  = grid.GetValue(2, 2);
        var second = grid.GetValue(2, 2);

        second.Should().Be(first, because: "results must be memoized and stable");
    }

    [Fact]
    public void GetValue_ReturnsSameResult_ForTwoGridsWithIdenticalParameters()
    {
        // Two separately constructed grids sharing the same seed must produce identical values
        var gridA = new SeededNoiseGrid(masterSeed: 123L, gridSx: 4, gridSy: 4,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);
        var gridB = new SeededNoiseGrid(masterSeed: 123L, gridSx: 4, gridSy: 4,
            featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        for (int x = 0; x <= 4; x++)
        for (int y = 0; y <= 4; y++)
            gridA.GetValue(x, y).Should().Be(gridB.GetValue(x, y),
                because: $"cell ({x},{y}) must be deterministic for the same master seed");
    }

    [Fact]
    public void GetValue_ProducesDistinctInteriorValues_AcrossManyDifferentSeeds()
    {
        // Different master seeds must produce different noise — test across 30 seeds for reliability
        var interiorValues = Enumerable.Range(1, 30)
            .Select(s => new SeededNoiseGrid((long)s, 4, 4, 4, 0.9, 0.2, 0).GetValue(2, 2))
            .ToList();

        interiorValues.Distinct().Should().HaveCountGreaterThan(1,
            because: "different master seeds must produce different interior cell values");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Layer isolation
    // --------------------------------------------------------------------------

    [Fact]
    public void GetValue_DiffersAcrossLayerIndices_ForInteriorCell()
    {
        // The same master seed with a different layerIndex must produce distinct noise fields.
        // Tested across several layer index pairs to guard against accidental collision for one pair.
        long masterSeed = 555L;
        var layers = Enumerable.Range(0, 5)
            .Select(l => new SeededNoiseGrid(masterSeed, 4, 4, 4, 0.9, 0.2, l).GetValue(2, 2))
            .ToList();

        layers.Distinct().Should().HaveCountGreaterThan(1,
            because: "different layerIndex values must produce distinct per-cell jitter");
    }

    [Fact]
    public void GetValue_ConstantCells_RemainsHalfRegardlessOfLayerIndex()
    {
        // Layer index must not affect the constant corner and boundary cells
        for (int layer = 0; layer < 4; layer++)
        {
            var grid = new SeededNoiseGrid(masterSeed: 1L, gridSx: 4, gridSy: 4,
                featStep: 4, startScale: 0.9, scaleMod: 0.2, layerIndex: layer);

            grid.GetValue(0, 0).Should().Be(0.5, because: "corners are always 0.5 regardless of layer");
            grid.GetValue(4, 0).Should().Be(0.5, because: "corners are always 0.5 regardless of layer");
        }
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region cscal and featStep behaviour
    // --------------------------------------------------------------------------

    // --- Non-featStep cells with startScale=0.0 produce no jitter (pure averages) ---

    [Fact]
    public void GetValue_AllCellsReturnHalf_WhenFeatStepNeverFiresAndStartScaleIsZero()
    {
        // featStep=999 never matches any valid step (steps are powers of 2 ≤ gridSx)
        // startScale=0.0 → cscal=0.0 for every step → no jitter anywhere → all values = 0.5
        var grid = new SeededNoiseGrid(masterSeed: 42L, gridSx: 4, gridSy: 4,
            featStep: 999, startScale: 0.0, scaleMod: 0.0, layerIndex: 0);

        for (int x = 0; x <= 4; x++)
        for (int y = 0; y <= 4; y++)
            grid.GetValue(x, y).Should().Be(0.5,
                because: $"with startScale=0 and no featStep match, cell ({x},{y}) must be a pure average of 0.5 ancestors");
    }

    [Fact]
    public void GetValue_HorizontalMidpointCell_IsExactAverageOfParents_WhenStartScaleIsZero()
    {
        // With featStep=999 (never fires) and startScale=0.0:
        // (1,0) is a horizontal midpoint at step=2; its parents are (0,0)=0.5 and (2,0).
        // (2,0) is a horizontal midpoint at step=4; its parents are (0,0)=0.5 and (4,0)=0.5 → value=0.5.
        // Therefore (1,0) = avg(0.5, 0.5) = 0.5. This tests the parent resolution logic.
        var grid = new SeededNoiseGrid(masterSeed: 77L, gridSx: 4, gridSy: 4,
            featStep: 999, startScale: 0.0, scaleMod: 0.0, layerIndex: 0);

        var parent0 = grid.GetValue(0, 0);   // = 0.5 (corner)
        var parent2 = grid.GetValue(2, 0);   // = 0.5 (no jitter case)
        var mid     = grid.GetValue(1, 0);   // should equal (parent0 + parent2) / 2

        mid.Should().Be((parent0 + parent2) / 2,
            because: "horizontal midpoint with cscal=0 is the exact average of its two parents");
    }

    [Fact]
    public void GetValue_CenterCell_IsExactAverageOfFourParents_WhenStartScaleIsZero()
    {
        // (2,2) is the center of the full 4×4 grid, placed at step=4.
        // Parents: (0,0), (4,0), (0,4), (4,4) — all constant 0.5.
        // With featStep=999 and startScale=0.0 → value = (0.5+0.5+0.5+0.5)/4 = 0.5 exactly.
        var grid = new SeededNoiseGrid(masterSeed: 13L, gridSx: 4, gridSy: 4,
            featStep: 999, startScale: 0.0, scaleMod: 0.0, layerIndex: 0);

        grid.GetValue(2, 2).Should().Be(0.5,
            because: "center cell with cscal=0 is the exact average of its four corner parents");
    }

    [Fact]
    public void GetValue_HorizontalMidpointCell_IsExactAverageOfParents_WhenOnlyFeatStepFiresAtHigherStep()
    {
        // featStep=4, startScale=0.0 — only step=4 cells have cscal=1.0; step=2 cells have cscal=0.0.
        // (1,0) at step=2 must equal (parent0 + parent2) / 2 with no jitter.
        // parent0=(0,0)=0.5 (corner), parent2=(2,0) has featStep jitter but is still a valid value.
        var grid = new SeededNoiseGrid(masterSeed: 9L, gridSx: 4, gridSy: 4,
            featStep: 4, startScale: 0.0, scaleMod: 0.0, layerIndex: 0);

        var parent0 = grid.GetValue(0, 0);   // corner = 0.5
        var parent2 = grid.GetValue(2, 0);   // has featStep jitter
        var mid     = grid.GetValue(1, 0);

        mid.Should().Be((parent0 + parent2) / 2,
            because: "step-2 horizontal midpoint with cscal=0 is the exact average of its parents");
    }

    // --- featStep cells DO have jitter ---

    [Fact]
    public void GetValue_AtFeatStepCells_ProducesDistinctValues_AcrossSeeds()
    {
        // Cell (2,0) is a step=4 horizontal midpoint. When featStep=4, cscal=1.0 and jitter fires.
        // Different seeds must yield different jitter → different values across 30 seeds.
        var values = Enumerable.Range(1, 30)
            .Select(s => new SeededNoiseGrid((long)s, 4, 4, featStep: 4, 0.0, 0.0, 0).GetValue(2, 0))
            .ToList();

        values.Distinct().Should().HaveCountGreaterThan(1,
            because: "cscal=1.0 at featStep must inject distinct per-seed jitter into step=4 cells");
    }

    [Fact]
    public void GetValue_AtFeatStepCell_DiffersFromNonFeatStepEquivalent_ForSameSeed()
    {
        // Same seed, same cell (2,2), but featStep=4 (fires) vs featStep=999 (doesn't fire).
        // The cell without featStep will always be 0.5; the one with featStep will have jitter.
        // Over 20 seeds, at least some of the featStep results must differ from 0.5.
        bool anyDiffers = Enumerable.Range(1, 20).Any(s =>
        {
            var withFeat    = new SeededNoiseGrid((long)s, 4, 4, featStep: 4,   0.0, 0.0, 0).GetValue(2, 2);
            var withoutFeat = new SeededNoiseGrid((long)s, 4, 4, featStep: 999, 0.0, 0.0, 0).GetValue(2, 2);
            return withFeat != withoutFeat;
        });

        anyDiffers.Should().BeTrue(
            because: "featStep override of cscal=1.0 must produce jitter not present when featStep never fires");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Value range constraints
    // --------------------------------------------------------------------------

    [Fact]
    public void GetValue_AllValues_AreInUnitRange_WhenCscalNeverExceedsOne()
    {
        // With startScale=0.0 and featStep=gridSx (cscal=1.0 only at step=4 for a 4×4 grid):
        // All step=4 cells: parents all 0.5, jitter in (-0.5, +0.5) → value in (0.0, 1.0)
        // All step<4 cells: cscal=0 → pure averages of in-range parents → also in [0.0, 1.0]
        var grid = new SeededNoiseGrid(masterSeed: 321L, gridSx: 4, gridSy: 4,
            featStep: 4, startScale: 0.0, scaleMod: 0.0, layerIndex: 0);

        for (int x = 0; x <= 4; x++)
        for (int y = 0; y <= 4; y++)
            grid.GetValue(x, y).Should().BeInRange(0.0, 1.0,
                because: $"with cscal≤1.0 and all ancestors in [0,1], cell ({x},{y}) must stay in [0,1]");
    }

    [Fact]
    public void GetValue_FullIslandGrid_AllValuesInUnitRange_WithTypicalIslandParameters()
    {
        // Verify a realistic 64×64 island grid (cur layer params) for a concrete seed.
        // startScale=0.9, scaleMod=0.2, featStep=64 → cscal is at most 1.0.
        // Spot-check 50 interior cells; all should be in [0.0, 1.0].
        var grid = new SeededNoiseGrid(masterSeed: 1L, gridSx: 64, gridSy: 64,
            featStep: 64, startScale: 0.9, scaleMod: 0.2, layerIndex: 0);

        var rng = new Random(0); // deterministic sampling
        for (int probe = 0; probe < 50; probe++)
        {
            int x = rng.Next(1, 64);  // interior only
            int y = rng.Next(1, 64);
            grid.GetValue(x, y).Should().BeInRange(0.0, 1.0,
                because: $"interior cell ({x},{y}) with island cur-layer params must stay in [0,1]");
        }
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Structural parent–child consistency
    // --------------------------------------------------------------------------

    [Fact]
    public void GetValue_VerticalMidpointCell_IsExactAverageOfParents_WhenStartScaleIsZero()
    {
        // (0,1) is a vertical midpoint at step=2; parents are (0,0)=0.5 and (0,2).
        // (0,2) is a vertical midpoint at step=4 with no-jitter params → also 0.5.
        var grid = new SeededNoiseGrid(masterSeed: 55L, gridSx: 4, gridSy: 4,
            featStep: 999, startScale: 0.0, scaleMod: 0.0, layerIndex: 0);

        var parent0 = grid.GetValue(0, 0);  // corner = 0.5
        var parent2 = grid.GetValue(0, 2);  // vertical midpoint at step=4, no jitter = 0.5
        var mid     = grid.GetValue(0, 1);

        mid.Should().Be((parent0 + parent2) / 2,
            because: "vertical midpoint with cscal=0 is the exact average of its two parents");
    }

    [Fact]
    public void GetValue_NestedCenterCell_ReflectsParentJitter_WhenFeatStepFiresAtLargerStep()
    {
        // With featStep=4: (2,0), (0,2), (2,2) have featStep jitter.
        // (1,1) is a center cell at step=2. Its parents: (0,0), (2,0), (0,2), (2,2).
        // With startScale=0.0 (step=2 has cscal=0): (1,1) = avg of those four parents exactly.
        var grid = new SeededNoiseGrid(masterSeed: 7L, gridSx: 4, gridSy: 4,
            featStep: 4, startScale: 0.0, scaleMod: 0.0, layerIndex: 0);

        var p00 = grid.GetValue(0, 0);  // corner = 0.5
        var p20 = grid.GetValue(2, 0);  // featStep jitter
        var p02 = grid.GetValue(0, 2);  // featStep jitter
        var p22 = grid.GetValue(2, 2);  // featStep jitter (center type at step=4)
        var center11 = grid.GetValue(1, 1);

        var expectedAvg = (p00 + p20 + p02 + p22) / 4.0;
        center11.Should().BeApproximately(expectedAvg, precision: 1e-12,
            because: "step-2 center cell with cscal=0 must be the exact average of its four parents");
    }

    // --------------------------------------------------------------------------
    #endregion
}
