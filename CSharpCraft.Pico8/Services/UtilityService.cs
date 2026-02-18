using FixMath;
using System;
using System.Collections.Generic;

namespace CSharpCraft.Pico8.Services;

/// <summary>
/// Service responsible for all mathematical and utility operations
/// Extracted from Pico8Functions to enable service-based architecture
/// Phase 3: Service Extraction - Part 2
/// </summary>
public class UtilityService
{
    private readonly CosDict _cosDict;
    private readonly SinDict _sinDict;
    private readonly int[] _flags;
    private Random _random;

    public UtilityService(CosDict cosDict, SinDict sinDict, int[] flags, Random? random = null)
    {
        _cosDict = cosDict ?? throw new ArgumentNullException(nameof(cosDict));
        _sinDict = sinDict ?? throw new ArgumentNullException(nameof(sinDict));
        _flags = flags ?? throw new ArgumentNullException(nameof(flags));
        _random = random ?? new Random();
    }

    /// <summary>
    /// Calculate cosine of an angle (Pico-8: Cos)
    /// angle is in pico 8 turns (0-1 = full rotation)
    /// https://pico-8.fandom.com/wiki/Cos
    /// </summary>
    public F32 Cos(F32 angle)
    {
        angle = Mod(angle, 1);
        F32 val = F32.FromRaw((int)(_cosDict.LookupTable[angle.Raw / 10.0] * 10));
        return val;
    }

    /// <summary>
    /// Calculate sine of an angle (Pico-8: Sin)
    /// angle is in pico 8 turns (0-1 = full rotation)
    /// https://pico-8.fandom.com/wiki/Sin
    /// </summary>
    public F32 Sin(F32 angle)
    {
        angle = Mod(angle, 1);
        F32 val = F32.FromRaw((int)(_sinDict.LookupTable[angle.Raw / 10.0] * 10));
        return val;
    }

    /// <summary>
    /// Delete an item from a list (Pico-8: Del)
    /// https://pico-8.fandom.com/wiki/Del
    /// </summary>
    public void Del<T>(List<T> table, T value)
    {
        table.Remove(value);
    }

    /// <summary>
    /// Get a flag value (Pico-8: Fget)
    /// https://pico-8.fandom.com/wiki/Fget
    /// </summary>
    public int Fget(int n)
    {
        return _flags[n];
    }

    /// <summary>
    /// Set a flag value (Pico-8: Fset)
    /// https://pico-8.fandom.com/wiki/Fset
    /// </summary>
    public void Fset(int n, int v)
    {
        _flags[n] = v;
    }

    /// <summary>
    /// Modulo operation that handles negative results correctly
    /// Returns result in range [0, m)
    /// </summary>
    public static F32 Mod(F32 x, int m)
    {
        F32 r = x % m;
        return r < 0 ? r + m : r;
    }

    /// <summary>
    /// Generate a random F32 between 0 and 1
    /// https://pico-8.fandom.com/wiki/Rnd
    /// </summary>
    public F32 Random()
    {
        return F32.FromDouble(_random.NextDouble());
    }

    /// <summary>
    /// Generate a random F32 between min and max
    /// </summary>
    public F32 Random(F32 min, F32 max)
    {
        F32 range = max - min;
        return min + (F32.FromDouble(_random.NextDouble()) * range);
    }

    /// <summary>
    /// Generate a random integer
    /// </summary>
    public int Random(int max)
    {
        return _random.Next(max);
    }

    /// <summary>
    /// Set the random seed (Pico-8: Srand)
    /// https://pico-8.fandom.com/wiki/Srand
    /// </summary>
    public void Srand(int seed)
    {
        _random = new Random(seed);
    }

    /// <summary>
    /// Copy memory from source to destination (Pico-8: Memcpy)
    /// https://pico-8.fandom.com/wiki/Memcpy
    /// </summary>
    public void Memcpy(Span<byte> dest, Span<byte> source, int len)
    {
        if (len < 0 || len > source.Length || len > dest.Length)
            throw new ArgumentException("Invalid length for memcpy");

        source.Slice(0, len).CopyTo(dest);
    }

    /// <summary>
    /// Fill memory with a value (similar to Pico-8 memory operations)
    /// </summary>
    public void Memset(Span<byte> dest, int value, int len)
    {
        if (len < 0 || len > dest.Length)
            throw new ArgumentException("Invalid length for memset");

        dest.Slice(0, len).Fill((byte)value);
    }
}
