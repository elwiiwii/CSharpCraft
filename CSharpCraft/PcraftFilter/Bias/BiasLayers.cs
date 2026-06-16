namespace CSharpCraft.PcraftFilter.Bias;

/// <summary>
/// Sparse additive bias offsets applied to the three intermediate classification
/// values (coast, v2, v3) computed by <see cref="PcraftSeeded.Noise.MapClassifier"/>.
/// All values default to zero (no bias). Biases accumulate additively.
/// </summary>
internal sealed class BiasLayers
{
    private readonly Dictionary<(int, int), double> _coast = [];
    private readonly Dictionary<(int, int), double> _v2 = [];
    private readonly Dictionary<(int, int), double> _v3 = [];

    internal double GetCoast(int x, int y)
    {
        return _coast.GetValueOrDefault((x, y));
    }

    internal double GetV2(int x, int y)
    {
        return _v2.GetValueOrDefault((x, y));
    }

    internal double GetV3(int x, int y)
    {
        return _v3.GetValueOrDefault((x, y));
    }

    internal void AddCoast(int x, int y, double delta)
    {
        _coast[(x, y)] = _coast.GetValueOrDefault((x, y)) + delta;
    }

    internal void AddV2(int x, int y, double delta)
    {
        _v2[(x, y)] = _v2.GetValueOrDefault((x, y)) + delta;
    }

    internal void AddV3(int x, int y, double delta)
    {
        _v3[(x, y)] = _v3.GetValueOrDefault((x, y)) + delta;
    }

    /// <summary>Returns true if any bias channel has been set for this cell.</summary>
    internal bool HasAnyBias(int x, int y)
    {
        return _coast.ContainsKey((x, y)) || _v2.ContainsKey((x, y)) || _v3.ContainsKey((x, y));
    }

    /// <summary>Returns true when no biases have been set on any cell or channel.</summary>
    internal bool IsEmpty => _coast.Count == 0 && _v2.Count == 0 && _v3.Count == 0;
}
