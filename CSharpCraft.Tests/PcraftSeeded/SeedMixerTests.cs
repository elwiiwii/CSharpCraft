using CSharpCraft.PcraftSeeded.Noise;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftSeeded;

public sealed class SeedMixerTests
{
    // --------------------------------------------------------------------------
    #region Determinism
    // --------------------------------------------------------------------------

    [Fact]
    public void Combine_ReturnsSameValue_GivenIdenticalInputs()
    {
        int first = SeedMixer.Combine(12345L, 1, 2, 3, 4);
        int second = SeedMixer.Combine(12345L, 1, 2, 3, 4);

        _ = first.Should().Be(second);
    }

    [Theory]
    [InlineData(0L)]          // zero seed
    [InlineData(1L)]          // small seed
    [InlineData(long.MaxValue)] // boundary
    [InlineData(long.MinValue)] // negative boundary
    public void Combine_ReturnsSameValue_ForVariousSeeds(long seed)
    {
        int first = SeedMixer.Combine(seed, 7, 3);
        int second = SeedMixer.Combine(seed, 7, 3);

        _ = first.Should().Be(second, because: $"seed {seed} must be deterministic");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Sensitivity to each parameter
    // --------------------------------------------------------------------------

    [Fact]
    public void Combine_ReturnsDifferentValue_WhenSeedChanges()
    {
        int a = SeedMixer.Combine(1L, 0, 0, 0, 0);
        int b = SeedMixer.Combine(2L, 0, 0, 0, 0);

        _ = a.Should().NotBe(b);
    }

    [Fact]
    public void Combine_ReturnsDifferentValue_WhenAChanges()
    {
        int x = SeedMixer.Combine(100L, 0, 0, 0, 0);
        int y = SeedMixer.Combine(100L, 1, 0, 0, 0);

        _ = x.Should().NotBe(y);
    }

    [Fact]
    public void Combine_ReturnsDifferentValue_WhenBChanges()
    {
        int x = SeedMixer.Combine(100L, 0, 0, 0, 0);
        int y = SeedMixer.Combine(100L, 0, 1, 0, 0);

        _ = x.Should().NotBe(y);
    }

    [Fact]
    public void Combine_ReturnsDifferentValue_WhenCChanges()
    {
        int x = SeedMixer.Combine(100L, 0, 0, 0, 0);
        int y = SeedMixer.Combine(100L, 0, 0, 1, 0);

        _ = x.Should().NotBe(y);
    }

    [Fact]
    public void Combine_ReturnsDifferentValue_WhenDChanges()
    {
        int x = SeedMixer.Combine(100L, 0, 0, 0, 0);
        int y = SeedMixer.Combine(100L, 0, 0, 0, 1);

        _ = x.Should().NotBe(y);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Default parameter behaviour
    // --------------------------------------------------------------------------

    [Fact]
    public void Combine_DefaultParams_MatchExplicitZero()
    {
        int withDefaults = SeedMixer.Combine(42L, 5);
        int withZeros = SeedMixer.Combine(42L, 5, 0, 0, 0);

        _ = withDefaults.Should().Be(withZeros);
    }

    [Fact]
    public void Combine_SpawnSalt_DiffersFromWaterSalt_ForSameSeedAndPosition()
    {
        // The two salts used in the codebase must produce distinct series.
        const int spawnSalt = 0x5350_4157;
        const int waterSalt = 0x574F_4154;

        int spawn = SeedMixer.Combine(99L, 0, spawnSalt);
        int water = SeedMixer.Combine(99L, 0, waterSalt);

        _ = spawn.Should().NotBe(water);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region HashString determinism
    // --------------------------------------------------------------------------

    [Fact]
    public void HashString_ReturnsSameValue_GivenSameString()
    {
        int first = SeedMixer.HashString("wood");
        int second = SeedMixer.HashString("wood");

        _ = first.Should().Be(second);
    }

    [Fact]
    public void HashString_EmptyString_ReturnsZero()
    {
        int result = SeedMixer.HashString("");

        _ = result.Should().Be(0, because: "empty string feeds no chars through avalanche, leaving h=0");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region HashString sensitivity
    // --------------------------------------------------------------------------

    [Fact]
    public void HashString_ReturnsDifferentValues_ForDifferentStrings()
    {
        int a = SeedMixer.HashString("sand");
        int b = SeedMixer.HashString("iron");

        _ = a.Should().NotBe(b);
    }

    [Fact]
    public void HashString_ReturnsDifferentValues_WhenCharOrderDiffers()
    {
        int forward = SeedMixer.HashString("ab");
        int reversed = SeedMixer.HashString("ba");

        _ = forward.Should().NotBe(reversed, because: "avalanche is not commutative");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region HashString golden values (stability across process restarts)
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData("sand", -149329036)]    // material used in drop golden-value tests
    [InlineData("iron", 1102800317)]    // mineral material
    [InlineData("gold", 189377251)]     // mineral material
    [InlineData("gem", 690648055)]      // mineral material
    public void HashString_ReturnsGoldenValue_GivenKnownString(string input, int expected)
    {
        int actual = SeedMixer.HashString(input);

        _ = actual.Should().Be(expected,
            because: "HashString must produce the same value across process restarts (.NET's GetHashCode is randomized and must not be used)");
    }

    // --------------------------------------------------------------------------
    #endregion
}
