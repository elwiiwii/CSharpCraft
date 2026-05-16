namespace CSharpCraft.PcraftSeeded;

/// <summary>
/// Deterministic seed mixing for seeded world and drop generation.
/// Combines a long master seed with up to four int selectors into an int
/// suitable for seeding <see cref="System.Random"/>.
/// </summary>
/// <remarks>
/// Unlike <see cref="System.HashCode.Combine"/>, this is stable across process restarts,
/// machines, and .NET versions because it uses only unchecked arithmetic (no runtime seeds).
/// The finalizer is the MurmurHash3 64-bit avalanche pass.
/// </remarks>
internal static class SeedMixer
{
    internal static int Combine(long seed, int a, int b = 0, int c = 0, int d = 0)
    {
        unchecked
        {
            long h = seed;
            h ^= (long)a; h = Avalanche(h);
            h ^= (long)b; h = Avalanche(h);
            h ^= (long)c; h = Avalanche(h);
            h ^= (long)d; h = Avalanche(h);
            return (int)h;
        }
    }

    /// <summary>
    /// Deterministic hash of a string — stable across process restarts and .NET versions.
    /// Feeds each char through the MurmurHash3 avalanche in sequence.
    /// Unlike <see cref="string.GetHashCode()"/> or <see cref="string.GetHashCode(StringComparison)"/>,
    /// this does not use .NET's randomized Marvin32 seed.
    /// </summary>
    internal static int HashString(string s)
    {
        unchecked
        {
            long h = 0;
            foreach (char c in s)
            {
                h ^= (long)c;
                h = Avalanche(h);
            }
            return (int)h;
        }
    }

    private static long Avalanche(long h)
    {
        unchecked
        {
            h ^= h >> 33;
            h *= unchecked((long)0xff51afd7ed558ccdL);
            h ^= h >> 33;
            h *= unchecked((long)0xc4ceb9fe1a85ec53L);
            h ^= h >> 33;
            return h;
        }
    }
}
