using CSharpCraft.PcraftFilter.Bias;
using CSharpCraft.PcraftFilter.Noise;
using CSharpCraft.PcraftPreview.Noise;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftFilter.Noise;

public sealed class BiasedMapClassifierTests
{
    // --------------------------------------------------------------------------
    #region Helpers
    // --------------------------------------------------------------------------

    // Injects a fixed constant for every cell in the grid.
    private sealed class FixedGrid : SeededNoiseGrid
    {
        private readonly double _value;

        internal FixedGrid(double value)
            : base(masterSeed: 0L, gridSx: 4, gridSy: 4, featStep: 999,
                   startScale: 0.0, scaleMod: 0.0, layerIndex: 0)
        {
            _value = value;
        }

        internal override double GetValue(int x, int y)
        {
            return _value;
        }
    }

    // Standard surface tile ids: a=0(Water) b=1(Sand) c=2(Grass) d=3(Rock) e=4(Tree)
    // gridSx=gridSy=4, so dist is non-zero for non-centre cells; use cell (2,2) as
    // the canonical test cell (di=dj=0, dist=0 → pure coast signal).
    private static BiasedMapClassifier Make(
        double cur, double cur2, double cur3, double cur4,
        BiasLayers biases)
    {
        return new BiasedMapClassifier(
            new FixedGrid(cur), new FixedGrid(cur2),
            new FixedGrid(cur3), new FixedGrid(cur4),
            gridSx: 4, gridSy: 4,
            a: 0, b: 1, c: 2, d: 3, e: 4,
            biases);
    }

    // Computes raw coast = |cur-cur2|*4  (dist=0 at grid centre)
    private static double Coast(double cur, double cur2)
    {
        return Math.Abs(cur - cur2) * 4.0;
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenBiasLayersIsNull()
    {
        FixedGrid grid = new(0.5);
        Func<BiasedMapClassifier> act = () => new BiasedMapClassifier(
            grid, grid, grid, grid,
            gridSx: 4, gridSy: 4,
            a: 0, b: 1, c: 2, d: 3, e: 4,
            biasLayers: null!);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("biasLayers");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region No bias — matches base classifier
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(0.5, 0.5, 0)]   // coast=0.0  → Water(0)
    [InlineData(0.6, 0.5, 1)]   // coast=0.4  → Sand(1)
    [InlineData(0.8, 0.5, 2)]   // coast=1.2  → Grass(2)
    public void ClassifyTile_MatchesBaseClassifier_WhenNoBiasSet(
        double cur, double cur2, int expected)
    {
        // cur3=cur4=cur so v2=v3=0 (no rock or tree)
        BiasedMapClassifier sut = Make(cur, cur2, cur3: cur, cur4: cur, new BiasLayers());
        _ = sut.ClassifyTile(2, 2).Should().Be(expected);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Coast bias
    // --------------------------------------------------------------------------

    [Fact]
    public void ClassifyTile_UpgradesWaterToSand_WhenCoastBiasPushesAbove0_3()
    {
        // Raw coast = |0.5-0.5|*4 = 0.0 → Water(0)
        // With +0.35 coast bias → 0.35 > 0.3 → Sand(1)
        BiasLayers biases = new();
        biases.AddCoast(2, 2, 0.35);
        BiasedMapClassifier sut = Make(cur: 0.5, cur2: 0.5, cur3: 0.5, cur4: 0.5, biases);
        _ = sut.ClassifyTile(2, 2).Should().Be(1);
    }

    [Fact]
    public void ClassifyTile_UpgradesSandToGrass_WhenCoastBiasPushesAbove0_6()
    {
        // Raw coast = |0.6-0.5|*4 = 0.4 → Sand(1)
        // With +0.25 bias → 0.65 > 0.6 → Grass(2)
        BiasLayers biases = new();
        biases.AddCoast(2, 2, 0.25);
        BiasedMapClassifier sut = Make(cur: 0.6, cur2: 0.5, cur3: 0.6, cur4: 0.6, biases);
        _ = sut.ClassifyTile(2, 2).Should().Be(2);
    }

    [Fact]
    public void ClassifyTile_DowngradesGrassToSand_WhenNegativeCoastBiasDropsBelow0_6()
    {
        // Raw coast = |0.8-0.5|*4 = 1.2 → Grass(2)
        // With -0.7 bias → 0.5 which is in (0.3,0.6] → Sand(1)
        BiasLayers biases = new();
        biases.AddCoast(2, 2, -0.7);
        BiasedMapClassifier sut = Make(cur: 0.8, cur2: 0.5, cur3: 0.8, cur4: 0.8, biases);
        _ = sut.ClassifyTile(2, 2).Should().Be(1);
    }

    [Fact]
    public void ClassifyTile_BiasOnlyAppliesToTargetCell_NotNeighbour()
    {
        // Bias at (2,2) should not affect (3,2)
        BiasLayers biases = new();
        biases.AddCoast(2, 2, 0.35);
        BiasedMapClassifier sut = Make(cur: 0.5, cur2: 0.5, cur3: 0.5, cur4: 0.5, biases);
        _ = sut.ClassifyTile(3, 2).Should().Be(0); // still Water at unbiased cell
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region V2 bias (Rock discriminator)
    // --------------------------------------------------------------------------

    [Fact]
    public void ClassifyTile_ProducesRock_WhenV2BiasPushesAbove0_5WithCoastAbove0_3()
    {
        // coast = |0.6-0.5|*4 = 0.4 > 0.3; raw v2 = |0.6-0.6| = 0 → Sand(1)
        // With +0.55 v2 bias → v2=0.55 > 0.5 → Rock(3)
        BiasLayers biases = new();
        biases.AddV2(2, 2, 0.55);
        BiasedMapClassifier sut = Make(cur: 0.6, cur2: 0.5, cur3: 0.6, cur4: 0.6, biases);
        _ = sut.ClassifyTile(2, 2).Should().Be(3);
    }

    [Fact]
    public void ClassifyTile_DoesNotProduceRock_WhenCoastBelow0_3_EvenWithHighV2()
    {
        // coast = 0 → Water; v2 rule requires coast > 0.3 first
        BiasLayers biases = new();
        biases.AddV2(2, 2, 1.0);
        BiasedMapClassifier sut = Make(cur: 0.5, cur2: 0.5, cur3: 0.5, cur4: 0.5, biases);
        _ = sut.ClassifyTile(2, 2).Should().Be(0); // still Water
    }

    [Fact]
    public void ClassifyTile_SuppressesRock_WhenNegativeV2BiasDropsBelow0_5()
    {
        // Raw: coast=0.4 > 0.3, v2=|0.6-0.0|=0.6 > 0.5 → Rock(3)
        // With -0.15 v2 bias → v2=0.45 < 0.5 → Sand(1)
        BiasLayers biases = new();
        biases.AddV2(2, 2, -0.15);
        BiasedMapClassifier sut = Make(cur: 0.6, cur2: 0.5, cur3: 0.0, cur4: 0.6, biases);
        _ = sut.ClassifyTile(2, 2).Should().Be(1);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region V3 bias (Tree discriminator)
    // --------------------------------------------------------------------------

    [Fact]
    public void ClassifyTile_ProducesTree_WhenV3BiasPushesAbove0_5OnGrassTile()
    {
        // coast = |0.8-0.5|*4 = 1.2 > 0.6 → Grass(2); v2=0 < 0.5; raw v3=0 → Grass
        // With +0.55 v3 bias → v3=0.55 > 0.5, id==c → Tree(4)
        BiasLayers biases = new();
        biases.AddV3(2, 2, 0.55);
        BiasedMapClassifier sut = Make(cur: 0.8, cur2: 0.5, cur3: 0.8, cur4: 0.8, biases);
        _ = sut.ClassifyTile(2, 2).Should().Be(4);
    }

    [Fact]
    public void ClassifyTile_DoesNotProduceTree_OnSandTile_EvenWithHighV3()
    {
        // coast = 0.4 → Sand(1); Tree rule requires id==Grass(2) first
        BiasLayers biases = new();
        biases.AddV3(2, 2, 1.0);
        BiasedMapClassifier sut = Make(cur: 0.6, cur2: 0.5, cur3: 0.6, cur4: 0.6, biases);
        _ = sut.ClassifyTile(2, 2).Should().Be(1); // still Sand
    }

    [Fact]
    public void ClassifyTile_SuppressesTree_WhenNegativeV3BiasDropsBelow0_5()
    {
        // Raw: coast=1.2 → Grass; v3=|0.8-0.2|=0.6 > 0.5, id==c → Tree(4)
        // With -0.15 v3 bias → v3=0.45 < 0.5 → stays Grass(2)
        BiasLayers biases = new();
        biases.AddV3(2, 2, -0.15);
        BiasedMapClassifier sut = Make(cur: 0.8, cur2: 0.5, cur3: 0.8, cur4: 0.2, biases);
        _ = sut.ClassifyTile(2, 2).Should().Be(2);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Combined biases
    // --------------------------------------------------------------------------

    [Fact]
    public void ClassifyTile_AppliesAllThreeBiasesSimultaneously()
    {
        // Start: cur=0.5, cur2=0.5, cur3=0.5, cur4=0.5 → coast=0, v2=0, v3=0 → Water(0)
        // coast bias +0.7  → coast=0.7 > 0.6 → Grass(2), v2 still 0, v3 still 0
        // v3 bias +0.55    → v3=0.55 > 0.5, id==Grass → Tree(4)
        BiasLayers biases = new();
        biases.AddCoast(2, 2, 0.7);
        biases.AddV3(2, 2, 0.55);
        BiasedMapClassifier sut = Make(cur: 0.5, cur2: 0.5, cur3: 0.5, cur4: 0.5, biases);
        _ = sut.ClassifyTile(2, 2).Should().Be(4);
    }

    // --------------------------------------------------------------------------
    #endregion
}
