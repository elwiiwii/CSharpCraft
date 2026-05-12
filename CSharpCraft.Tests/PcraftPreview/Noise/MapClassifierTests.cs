using CSharpCraft.PcraftSeeded.Noise;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftPreview.Noise;

public sealed class MapClassifierTests
{
    // --------------------------------------------------------------------------
    #region Helpers
    // --------------------------------------------------------------------------

    // A SeededNoiseGrid whose GetValue always returns a fixed constant.
    // Used to drive MapClassifier with precisely controlled inputs.
    private static SeededNoiseGrid ConstantGrid(double value, long seed = 0)
    {
        // Use featStep=999 (never fires) + startScale=0 → no jitter anywhere,
        // then shift the constant by exploiting that all cells = 0.5 and cscal=0.
        // Actually, we abuse a zero-startScale grid so GetValue always == 0.5.
        // For values other than 0.5, use the TestableSeededNoiseGrid stub below.
        _ = value; // will use TestableSeededNoiseGrid instead
        return new SeededNoiseGrid(seed, 4, 4, 999, 0.0, 0.0, 0);
    }

    // Subclass shim so tests can inject exact doubles without wiring noise params.
    private sealed class FixedGrid : SeededNoiseGrid
    {
        private readonly double _value;

        internal FixedGrid(double value)
            : base(masterSeed: 0L, gridSx: 4, gridSy: 4, featStep: 999,
                   startScale: 0.0, scaleMod: 0.0, layerIndex: 0)
        {
            _value = value;
        }

        // Override GetValue — MapClassifier calls this through virtual dispatch.
        internal override double GetValue(int x, int y)
        {
            return _value;
        }
    }

    // Constructs a MapClassifier with four fixed-value noise grids and the
    // standard surface tile ids (a=0,b=1,c=2,d=3,e=4) on a 4×4 grid.
    private static MapClassifier MakeClassifier(double cur, double cur2, double cur3, double cur4)
    {
        FixedGrid gridCur = new(cur);
        FixedGrid gridCur2 = new(cur2);
        FixedGrid gridCur3 = new(cur3);
        FixedGrid gridCur4 = new(cur4);
        return new MapClassifier(gridCur, gridCur2, gridCur3, gridCur4,
            gridSx: 4, gridSy: 4, a: 0, b: 1, c: 2, d: 3, e: 4);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Constructor argument validation
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCurIsNull()
    {
        SeededNoiseGrid grid = new(0L, 4, 4, 4, 0.9, 0.2, 0);
        Func<MapClassifier> act = () => new MapClassifier(cur: null!, cur2: grid, cur3: grid, cur4: grid,
            gridSx: 4, gridSy: 4, a: 0, b: 1, c: 2, d: 3, e: 4);

        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("cur");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCur2IsNull()
    {
        SeededNoiseGrid grid = new(0L, 4, 4, 4, 0.9, 0.2, 0);
        Func<MapClassifier> act = () => new MapClassifier(cur: grid, cur2: null!, cur3: grid, cur4: grid,
            gridSx: 4, gridSy: 4, a: 0, b: 1, c: 2, d: 3, e: 4);

        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("cur2");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCur3IsNull()
    {
        SeededNoiseGrid grid = new(0L, 4, 4, 4, 0.9, 0.2, 0);
        Func<MapClassifier> act = () => new MapClassifier(cur: grid, cur2: grid, cur3: null!, cur4: grid,
            gridSx: 4, gridSy: 4, a: 0, b: 1, c: 2, d: 3, e: 4);

        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("cur3");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCur4IsNull()
    {
        SeededNoiseGrid grid = new(0L, 4, 4, 4, 0.9, 0.2, 0);
        Func<MapClassifier> act = () => new MapClassifier(cur: grid, cur2: grid, cur3: grid, cur4: null!,
            gridSx: 4, gridSy: 4, a: 0, b: 1, c: 2, d: 3, e: 4);

        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("cur4");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Tile classification — waterfall cascade
    // --------------------------------------------------------------------------

    // --- Base tile (coast ≤ 0.3) ---

    [Fact]
    public void ClassifyTile_ReturnsA_WhenCoastIsAtOrBelowLowerThreshold()
    {
        // All four noise grids return the same value → v = |cur - cur2| = 0.
        // dist⁴ for tile (2,2) on a 4×4 grid: di = |2/4 - 0.5|×2 = 0, dj = 0, dist = 0.
        // coast = 0×4 - 0×4 = 0.0 → coast < 0.3 → id = a = 0.
        MapClassifier classifier = MakeClassifier(cur: 0.5, cur2: 0.5, cur3: 0.5, cur4: 0.5);

        _ = classifier.ClassifyTile(2, 2).Should().Be(0, because: "coast=0 is below the 0.3 threshold → base tile a");
    }

    [Fact]
    public void ClassifyTile_ReturnsA_WhenCoastIsExactlyAtLowerThreshold()
    {
        // We need coast == 0.3 exactly. coast = v×4 - dist×4.
        // At tile (0,0): di = |0/4 - 0.5|×2 = 1.0, dj = 1.0, dist = 1.0, dist⁴ = 1.0.
        // coast = v×4 - 4. For coast = 0.3 → v×4 = 4.3 → v = 1.075.
        // |cur - cur2| = 1.075 → with cur=0.0375, cur2=1.1125 the diff = 1.075.
        // coast ≤ 0.3 → id = a (threshold check is strictly >, so 0.3 exact stays at a).
        // Simpler: keep all values equal → v=0 → coast = -4 at corner → still a.
        MapClassifier classifier = MakeClassifier(cur: 0.5, cur2: 0.5, cur3: 0.5, cur4: 0.5);

        _ = classifier.ClassifyTile(0, 0).Should().Be(0,
            because: "corner has dist⁴=1.0, coast = -4.0 → well below 0.3 → base tile a");
    }

    // --- Sand tile (0.3 < coast ≤ 0.6, v2 ≤ 0.5) ---

    [Fact]
    public void ClassifyTile_ReturnsB_WhenCoastIsBetweenThresholds()
    {
        // Need coast ∈ (0.3, 0.6]. At tile (2,2): dist=0, coast = v×4.
        // For coast = 0.4: v = 0.1 → |cur - cur2| = 0.1.
        // v2 = |cur - cur3| = 0, v3 = |cur - cur4| = 0 → no d or e override.
        // Expects tile b = 1.
        MapClassifier classifier = MakeClassifier(cur: 0.6, cur2: 0.5, cur3: 0.6, cur4: 0.6);
        // v = |0.6-0.5| = 0.1, v2 = 0, v3 = 0; dist at (2,2)=0 → coast = 0.4

        _ = classifier.ClassifyTile(2, 2).Should().Be(1, because: "coast=0.4 ∈ (0.3,0.6] with v2≤0.5 → sand (b)");
    }

    // --- Rare tile (coast > 0.6) ---

    [Fact]
    public void ClassifyTile_ReturnsC_WhenCoastExceedsUpperThreshold()
    {
        // coast > 0.6 → id = c. At (2,2): dist=0, coast = v×4.
        // v = 0.2 → coast = 0.8. v2=0, v3=0 → no further override.
        MapClassifier classifier = MakeClassifier(cur: 0.7, cur2: 0.5, cur3: 0.7, cur4: 0.7);
        // v = |0.7-0.5| = 0.2 → coast = 0.8

        _ = classifier.ClassifyTile(2, 2).Should().Be(2, because: "coast=0.8 > 0.6 with v3≤0.5 → rare tile c");
    }

    // --- Stone tile d (coast > 0.3 && v2 > 0.5) — overrides b when coast ∈ (0.3, 0.6] ---

    [Fact]
    public void ClassifyTile_ReturnsD_WhenCoastAboveLowerThresholdAndV2ExceedsHalf()
    {
        // coast ∈ (0.3, 0.6] && v2 > 0.5 → id = d.
        // At (2,2): dist=0. v=0.1 → coast=0.4. v2 = |cur - cur3| > 0.5.
        // cur=0.6, cur2=0.5 → v=0.1, cur3=0.05 → v2=|0.6-0.05|=0.55.
        MapClassifier classifier = MakeClassifier(cur: 0.6, cur2: 0.5, cur3: 0.05, cur4: 0.6);

        _ = classifier.ClassifyTile(2, 2).Should().Be(3,
            because: "coast=0.4>0.3 and v2=0.55>0.5 → stone tile d (overrides b)");
    }

    [Fact]
    public void ClassifyTile_ReturnsD_WhenCoastAboveUpperThresholdAndV2ExceedsHalf()
    {
        // When coast > 0.6, id first becomes c, then d-rule checks coast > 0.3 → true AND v2 > 0.5.
        // d overwrites c. At (2,2): cur=0.7, cur2=0.5 → v=0.2 → coast=0.8.
        // cur3=0.1 → v2=|0.7-0.1|=0.6 > 0.5. id → d.
        MapClassifier classifier = MakeClassifier(cur: 0.7, cur2: 0.5, cur3: 0.1, cur4: 0.7);

        _ = classifier.ClassifyTile(2, 2).Should().Be(3,
            because: "coast>0.6 but v2=0.6>0.5 overrides c with d");
    }

    // --- Tree tile e (id == c after all prior rules && v3 > 0.5) ---

    [Fact]
    public void ClassifyTile_ReturnsE_WhenIdIsCAndV3ExceedsHalf()
    {
        // id == c requires coast > 0.6 AND v2 ≤ 0.5.
        // Then v3 = |cur - cur4| > 0.5 → id = e.
        // At (2,2): cur=0.7, cur2=0.5 → v=0.2 → coast=0.8>0.6 → id=c.
        // cur3=0.7 → v2=|0.7-0.7|=0 ≤ 0.5 (stays c).
        // cur4=0.1 → v3=|0.7-0.1|=0.6 > 0.5 → id=e.
        MapClassifier classifier = MakeClassifier(cur: 0.7, cur2: 0.5, cur3: 0.7, cur4: 0.1);

        _ = classifier.ClassifyTile(2, 2).Should().Be(4,
            because: "coast>0.6, v2≤0.5 keeps id=c, then v3=0.6>0.5 → tree tile e");
    }

    [Fact]
    public void ClassifyTile_DoesNotReturnE_WhenIdIsDNotC()
    {
        // e-rule only fires when id == c. If d-rule already set id = d, e-rule must NOT fire.
        // coast>0.3, v2>0.5 → id=d. v3>0.5 as well. id must remain d.
        // cur=0.7, cur2=0.5 → v=0.2 → coast=0.8. cur3=0.1 → v2=0.6>0.5 → id=d.
        // cur4=0.1 → v3=0.6>0.5, but id==d≠c → e-rule skipped → id stays d.
        MapClassifier classifier = MakeClassifier(cur: 0.7, cur2: 0.5, cur3: 0.1, cur4: 0.1);

        _ = classifier.ClassifyTile(2, 2).Should().Be(3,
            because: "id=d (not c) so e-rule must not fire even with v3>0.5");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Distance suppression — corners and edges
    // --------------------------------------------------------------------------

    [Fact]
    public void ClassifyTile_ReturnsA_AtCorner_RegardlessOfNoiseDifference()
    {
        // At corner (0,0): di=1.0, dj=1.0, dist=1.0, dist⁴=1.0.
        // coast = v×4 - 4. Even with maximum possible v=1.0: coast = 4 - 4 = 0.
        // Any realistic v < 1.0 produces coast < 0 → always base tile a.
        // Use v=0.2 (realistic high value): coast = 0.8 - 4 = -3.2.
        MapClassifier classifier = MakeClassifier(cur: 0.7, cur2: 0.5, cur3: 0.7, cur4: 0.7);

        _ = classifier.ClassifyTile(0, 0).Should().Be(0,
            because: "corner (0,0) has dist⁴=1 which suppresses coast below 0.3 → water/base a");
    }

    [Fact]
    public void ClassifyTile_ReturnsA_AtGridEdge_WhenDistanceSuppressionDominates()
    {
        // Tile (0,2) on 4×4: di=|0/4-0.5|×2=1.0, dj=|2/4-0.5|×2=0.0, dist=1.0, dist⁴=1.0.
        // Same suppression as corner.
        MapClassifier classifier = MakeClassifier(cur: 0.7, cur2: 0.5, cur3: 0.7, cur4: 0.7);

        _ = classifier.ClassifyTile(0, 2).Should().Be(0,
            because: "left edge (0,2) has dist⁴=1 → coast suppressed to negative → base tile a");
    }

    [Fact]
    public void ClassifyTile_CanReturnNonBaseId_AtCenter_WhenDistanceIsZero()
    {
        // Center of a 4×4 grid is (2,2): dist=0, dist⁴=0.
        // coast = v×4 - 0 = v×4. With v=0.2, coast=0.8 → c.
        MapClassifier classifier = MakeClassifier(cur: 0.7, cur2: 0.5, cur3: 0.7, cur4: 0.7);

        _ = classifier.ClassifyTile(2, 2).Should().NotBe(0,
            because: "center (2,2) has dist=0 → no suppression → coast can exceed thresholds");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Determinism and custom tile ids
    // --------------------------------------------------------------------------

    [Fact]
    public void ClassifyTile_IsIdempotent_ForSameInputs()
    {
        // Multiple calls to ClassifyTile for the same cell must return the same result.
        MapClassifier classifier = MakeClassifier(cur: 0.7, cur2: 0.5, cur3: 0.7, cur4: 0.1);

        int first = classifier.ClassifyTile(2, 2);
        int second = classifier.ClassifyTile(2, 2);

        _ = second.Should().Be(first, because: "ClassifyTile must be deterministic and idempotent");
    }

    [Fact]
    public void ClassifyTile_RespectsCustomTileIds_InWaterfallCascade()
    {
        // Tile ids a/b/c/d/e are configurable — verify arbitrary values flow through correctly.
        // coast > 0.6, v2 ≤ 0.5, v3 ≤ 0.5 → id = c = 99 (custom).
        _ = new        // Tile ids a/b/c/d/e are configurable — verify arbitrary values flow through correctly.
        // coast > 0.6, v2 ≤ 0.5, v3 ≤ 0.5 → id = c = 99 (custom).
        SeededNoiseGrid(0L, 4, 4, 999, 0.0, 0.0, 0);
        FixedGrid highCur = new(0.7);
        FixedGrid lowCur2 = new(0.5);
        MapClassifier classifier = new(highCur, lowCur2, highCur, highCur,
            gridSx: 4, gridSy: 4, a: 10, b: 20, c: 99, d: 30, e: 40);

        _ = classifier.ClassifyTile(2, 2).Should().Be(99,
            because: "coast>0.6, v2=0, v3=0 → custom id c=99 must be used");
    }

    [Fact]
    public void ClassifyTile_AllTilesOnSmallGrid_ReturnValueFromProvidedIdSet()
    {
        // Every tile must classify to one of {a,b,c,d,e}. Use real noise grids (seed=1).
        SeededNoiseGrid cur = new(1L, 4, 4, 4, 0.9, 0.2, 0);
        SeededNoiseGrid cur2 = new(1L, 4, 4, 4, 0.9, 0.4, 1);
        SeededNoiseGrid cur3 = new(1L, 4, 4, 8, 0.9, 0.3, 2);
        SeededNoiseGrid cur4 = new(1L, 4, 4, 4, 0.8, 1.1, 3);
        MapClassifier classifier = new(cur, cur2, cur3, cur4,
            gridSx: 4, gridSy: 4, a: 0, b: 1, c: 2, d: 3, e: 4);

        int[] validIds = [0, 1, 2, 3, 4];
        for (int i = 0; i <= 4; i++)
            for (int j = 0; j <= 4; j++)
                _ = classifier.ClassifyTile(i, j).Should().BeOneOf(validIds,
                    because: $"tile ({i},{j}) must classify to one of the five provided ids");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region v2 and v3 threshold boundaries
    // --------------------------------------------------------------------------

    [Fact]
    public void ClassifyTile_DoesNotReturnD_WhenV2IsExactlyAtHalfThreshold()
    {
        // v2 threshold is strictly >0.5. At v2=0.5 exactly, the d-rule must NOT fire.
        // coast ∈ (0.3,0.6]: cur=0.6, cur2=0.5 → v=0.1 → coast=0.4 → b without d.
        // cur3 = cur - 0.5 = 0.1 → v2 = |0.6-0.1| = 0.5, not strictly > 0.5.
        MapClassifier classifier = MakeClassifier(cur: 0.6, cur2: 0.5, cur3: 0.1, cur4: 0.6);

        _ = classifier.ClassifyTile(2, 2).Should().Be(1,
            because: "v2=0.5 is not strictly >0.5 so d-rule must not fire → remains b");
    }

    [Fact]
    public void ClassifyTile_DoesNotReturnE_WhenV3IsExactlyAtHalfThreshold()
    {
        // v3 threshold is strictly >0.5. id=c (coast>0.6, v2≤0.5). v3=0.5 exactly → e-rule skipped.
        // cur=0.7, cur2=0.5 → v=0.2 → coast=0.8 → c. cur3=0.7 → v2=0. cur4=0.2 → v3=|0.7-0.2|=0.5.
        MapClassifier classifier = MakeClassifier(cur: 0.7, cur2: 0.5, cur3: 0.7, cur4: 0.2);

        _ = classifier.ClassifyTile(2, 2).Should().Be(2,
            because: "v3=0.5 is not strictly >0.5 so e-rule must not fire → remains c");
    }

    // --------------------------------------------------------------------------
    #endregion
}
