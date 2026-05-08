using CSharpCraft.PcraftFilter.Bias;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftFilter.Bias;

public sealed class BiasLayersTests
{
    // --------------------------------------------------------------------------
    #region Default values
    // --------------------------------------------------------------------------

    [Fact]
    public void GetCoast_ReturnsZero_WhenNoBiasSet()
    {
        var layers = new BiasLayers();
        layers.GetCoast(3, 7).Should().Be(0.0);
    }

    [Fact]
    public void GetV2_ReturnsZero_WhenNoBiasSet()
    {
        var layers = new BiasLayers();
        layers.GetV2(0, 0).Should().Be(0.0);
    }

    [Fact]
    public void GetV3_ReturnsZero_WhenNoBiasSet()
    {
        var layers = new BiasLayers();
        layers.GetV3(10, 20).Should().Be(0.0);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region AddCoast
    // --------------------------------------------------------------------------

    [Fact]
    public void GetCoast_ReturnsDelta_AfterSingleAdd()
    {
        var layers = new BiasLayers();
        layers.AddCoast(3, 7, 0.5);
        layers.GetCoast(3, 7).Should().BeApproximately(0.5, 1e-10);
    }

    [Fact]
    public void GetCoast_AccumulatesDeltas_AfterMultipleAdds()
    {
        var layers = new BiasLayers();
        layers.AddCoast(1, 2, 0.3);
        layers.AddCoast(1, 2, 0.2);
        layers.GetCoast(1, 2).Should().BeApproximately(0.5, 1e-10);
    }

    [Fact]
    public void GetCoast_IsolatesCell_DoesNotAffectNeighbour()
    {
        var layers = new BiasLayers();
        layers.AddCoast(1, 1, 1.0);
        layers.GetCoast(1, 2).Should().Be(0.0);
        layers.GetCoast(2, 1).Should().Be(0.0);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region AddV2
    // --------------------------------------------------------------------------

    [Fact]
    public void GetV2_ReturnsDelta_AfterSingleAdd()
    {
        var layers = new BiasLayers();
        layers.AddV2(5, 5, -0.3);
        layers.GetV2(5, 5).Should().BeApproximately(-0.3, 1e-10);
    }

    [Fact]
    public void GetV2_AccumulatesNegativeAndPositive()
    {
        var layers = new BiasLayers();
        layers.AddV2(0, 0, 0.6);
        layers.AddV2(0, 0, -0.4);
        layers.GetV2(0, 0).Should().BeApproximately(0.2, 1e-10);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region AddV3
    // --------------------------------------------------------------------------

    [Fact]
    public void GetV3_ReturnsDelta_AfterSingleAdd()
    {
        var layers = new BiasLayers();
        layers.AddV3(2, 9, 0.7);
        layers.GetV3(2, 9).Should().BeApproximately(0.7, 1e-10);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Channel independence
    // --------------------------------------------------------------------------

    [Fact]
    public void Channels_AreIndependent_AddingCoastDoesNotAffectV2OrV3()
    {
        var layers = new BiasLayers();
        layers.AddCoast(3, 3, 1.0);
        layers.GetV2(3, 3).Should().Be(0.0);
        layers.GetV3(3, 3).Should().Be(0.0);
    }

    [Fact]
    public void Channels_AreIndependent_AddingV2DoesNotAffectCoastOrV3()
    {
        var layers = new BiasLayers();
        layers.AddV2(3, 3, 1.0);
        layers.GetCoast(3, 3).Should().Be(0.0);
        layers.GetV3(3, 3).Should().Be(0.0);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region HasAnyBias
    // --------------------------------------------------------------------------

    [Fact]
    public void HasAnyBias_ReturnsFalse_WhenEmpty()
    {
        var layers = new BiasLayers();
        layers.HasAnyBias(4, 4).Should().BeFalse();
    }

    [Fact]
    public void HasAnyBias_ReturnsTrue_AfterAddingCoastBias()
    {
        var layers = new BiasLayers();
        layers.AddCoast(4, 4, 0.1);
        layers.HasAnyBias(4, 4).Should().BeTrue();
    }

    [Fact]
    public void HasAnyBias_ReturnsTrue_AfterAddingV2Bias()
    {
        var layers = new BiasLayers();
        layers.AddV2(6, 2, -0.1);
        layers.HasAnyBias(6, 2).Should().BeTrue();
    }

    [Fact]
    public void HasAnyBias_ReturnsFalse_ForUnsetCell_WhenOtherCellsAreSet()
    {
        var layers = new BiasLayers();
        layers.AddCoast(1, 1, 0.5);
        layers.HasAnyBias(2, 2).Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
}
