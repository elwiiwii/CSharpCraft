using System.Numerics;

namespace CSharpCraft.PcraftPreview.Noise;

/// <summary>
/// Computes diamond-square noise values on-demand using per-cell deterministic hashing.
/// Each cell's jitter is derived from (masterSeed, layerIndex, step, x, y), so any cell
/// can be evaluated independently without running the full sequential algorithm.
/// </summary>
internal class SeededNoiseGrid
{
    private readonly long _masterSeed;
    private readonly int _gridSx;
    private readonly int _gridSy;
    private readonly int _featStep;
    private readonly double _startScale;
    private readonly double _scaleMod;
    private readonly int _layerIndex;
    private readonly Dictionary<(int, int), double> _cache = [];

    internal SeededNoiseGrid(long masterSeed, int gridSx, int gridSy, int featStep,
        double startScale, double scaleMod, int layerIndex)
    {
        if (!BitOperations.IsPow2(gridSx))
            throw new ArgumentException("Must be a positive power of 2.", nameof(gridSx));
        if (!BitOperations.IsPow2(gridSy))
            throw new ArgumentException("Must be a positive power of 2.", nameof(gridSy));

        _masterSeed = masterSeed;
        _gridSx = gridSx;
        _gridSy = gridSy;
        _featStep = featStep;
        _startScale = startScale;
        _scaleMod = scaleMod;
        _layerIndex = layerIndex;
    }

    /// <summary>
    /// Returns the noise value at (x, y). Constant cells (right/bottom boundary and top-left
    /// corner) always return 0.5. All other cells are computed recursively from their
    /// diamond-square parents and memoized.
    /// </summary>
    internal virtual double GetValue(int x, int y)
    {
        // Right boundary (x == gridSx), bottom boundary (y == gridSy), and the top-left corner
        // (0, 0) are never written by the algorithm — they remain permanently at 0.5.
        if (x == _gridSx || y == _gridSy || (x == 0 && y == 0))
            return 0.5;

        if (_cache.TryGetValue((x, y), out double cached))
            return cached;

        int step = FindStep(x, y);
        double cscal = ComputeCscal(step);
        double avg = ComputeParentAverage(x, y, step);

        int hashSeed = HashCode.Combine(_masterSeed.GetHashCode(), _layerIndex, step, x, y);
        double jitter = (new Random(hashSeed).NextDouble() - 0.5) * cscal;

        double value = avg + jitter;
        _cache[(x, y)] = value;
        return value;
    }

    // Returns the step S (largest power-of-2 ≤ gridSx) at which (x, y) was placed
    // by the diamond-square algorithm.
    private int FindStep(int x, int y)
    {
        int s = _gridSx;
        while (s >= 2)
        {
            int half = s >> 1;
            bool isHMid = x % s == half && y % s == 0;
            bool isVMid = x % s == 0 && y % s == half;
            bool isCenter = x % s == half && y % s == half;
            if (isHMid || isVMid || isCenter)
                return s;
            s >>= 1;
        }
        throw new InvalidOperationException(
            $"Cannot determine step for interior cell ({x}, {y}) in grid {_gridSx}×{_gridSy}.");
    }

    // cscal mirrors the original algorithm: 1.0 when step == featStep, otherwise
    // startScale × scaleMod^halvings where halvings = log2(gridSx / step).
    private double ComputeCscal(int step)
    {
        if (step == _featStep)
            return 1.0;

        int halvings = BitOperations.Log2((uint)_gridSx) - BitOperations.Log2((uint)step);
        return _startScale * Math.Pow(_scaleMod, halvings);
    }

    // Computes the average of the diamond-square parents for (x, y) at the given step.
    private double ComputeParentAverage(int x, int y, int step)
    {
        int half = step >> 1;

        // Horizontal midpoint: parents are left and right along the x-axis
        if (x % step == half && y % step == 0)
            return (GetValue(x - half, y) + GetValue(x + half, y)) * 0.5;

        // Vertical midpoint: parents are above and below along the y-axis
        if (x % step == 0 && y % step == half)
            return (GetValue(x, y - half) + GetValue(x, y + half)) * 0.5;

        // Center: parents are the four block corners
        return (GetValue(x - half, y - half) + GetValue(x + half, y - half) +
                GetValue(x - half, y + half) + GetValue(x + half, y + half)) * 0.25;
    }
}
