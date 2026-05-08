using CSharpCraft.PcraftFilter.Bias;
using CSharpCraft.PcraftPreview.Noise;

namespace CSharpCraft.PcraftFilter.Noise;

/// <summary>
/// Subclass of <see cref="MapClassifier"/> that applies additive per-cell bias
/// offsets to the three intermediate classification values (coast, v2, v3) before
/// invoking the threshold logic. All other behaviour is identical to the base class.
/// </summary>
internal sealed class BiasedMapClassifier : MapClassifier
{
    private readonly BiasLayers _biasLayers;

    internal BiasedMapClassifier(
        SeededNoiseGrid cur,
        SeededNoiseGrid cur2,
        SeededNoiseGrid cur3,
        SeededNoiseGrid cur4,
        int gridSx,
        int gridSy,
        int a, int b, int c, int d, int e,
        BiasLayers biasLayers)
        : base(cur, cur2, cur3, cur4, gridSx, gridSy, a, b, c, d, e)
    {
        _biasLayers = biasLayers ?? throw new ArgumentNullException(nameof(biasLayers));
    }

    internal override int ClassifyTile(int i, int j)
    {
        var (coast, v2, v3) = ComputeIntermediate(i, j);
        coast += _biasLayers.GetCoast(i, j);
        v2    += _biasLayers.GetV2(i, j);
        v3    += _biasLayers.GetV3(i, j);
        return ClassifyFromValues(coast, v2, v3);
    }
}
