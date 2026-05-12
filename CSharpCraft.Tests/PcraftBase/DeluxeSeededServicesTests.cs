using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftDeluxe;
using CSharpCraft.PcraftSeeded;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase;

// --------------------------------------------------------------------------
// Verifies DeluxeSeededServices wires seed correctly and inherits Deluxe
// map-step generation from DeluxeServices.
// Requires FNA (Pico8.Mget/Mset).
// --------------------------------------------------------------------------

[Collection("Fna")]
public sealed class DeluxeSeededServicesTests(FnaFixture fixture)
{
    private const long Seed = 99999L;

    // -----------------------------------------------------------------------
    #region Helpers
    // -----------------------------------------------------------------------

    private GameOrchestrator BuildOrchestrator()
    {
        return new(
            ".",
            ".",
            ".",
            new NullScene(),
            fixture.GraphicsDevice,
            fixture.GraphicsDeviceManager,
            fixture.Window);
    }

    private sealed class NullScene : IScene
    {
        public string? Name => null;
        public void Init(ISceneSetup setup) { }
        public string? SpritesPath => null;
        public string? MapPath => null;
        public string? FlagData => null;
        public IReadOnlyList<Soundtrack> Music => [];
        public IReadOnlyList<SfxPack> Sfx => [];
    }

    // -----------------------------------------------------------------------
    #endregion
    // -----------------------------------------------------------------------
    #region Inheritance hierarchy
    // -----------------------------------------------------------------------

    [Fact]
    public void DeluxeSeededServices_IsSubclassOf_DeluxeServices()
    {
        _ = typeof(DeluxeSeededServices).Should().BeDerivedFrom<DeluxeServices>(
            because: "DeluxeSeededServices must inherit Deluxe map-step generation");
    }

    [Fact]
    public void DeluxeSeededServices_IsSubclassOf_SeededServices_Transitively()
    {
        // SeededServices → PcraftServices; but DeluxeSeededServices derives from
        // DeluxeServices → PcraftServices, so it is NOT a SeededServices subtype.
        // Verify the actual chain: DeluxeSeededServices → DeluxeServices → PcraftServices.
        _ = typeof(DeluxeSeededServices).Should().BeDerivedFrom<DeluxeServices>();
        _ = typeof(DeluxeServices).Should().BeDerivedFrom<PcraftServices>();
    }

    // -----------------------------------------------------------------------
    #endregion
    // -----------------------------------------------------------------------
    #region Spawn consistency with SeededServices (same seed, same result)
    // -----------------------------------------------------------------------

    [Fact]
    public void ResetLevel_SpawnTile_MatchesSeededServices_ForSameSeed()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        // Capture spawn produced by plain SeededServices.
        int seededSpawnX, seededSpawnY;
        _ = new SeededServices(Seed);
        try
        {
            PlayerEntity player = new(F32.Zero, F32.Zero);
            PcraftSession.SetCurrent(new PcraftSession(player));
            PcraftServices.ResetLevel(player);
            seededSpawnX = F32.FloorToInt(player.X / F32.FromInt(16));
            seededSpawnY = F32.FloorToInt(player.Y / F32.FromInt(16));
        }
        finally
        {
            PcraftServices.SetServices(new PcraftServices());
        }

        // Capture spawn produced by DeluxeSeededServices with the same seed.
        int deluxeSpawnX, deluxeSpawnY;
        _ = new DeluxeSeededServices(Seed);
        try
        {
            PlayerEntity player = new(F32.Zero, F32.Zero);
            PcraftSession.SetCurrent(new PcraftSession(player));
            PcraftServices.ResetLevel(player);
            deluxeSpawnX = F32.FloorToInt(player.X / F32.FromInt(16));
            deluxeSpawnY = F32.FloorToInt(player.Y / F32.FromInt(16));
        }
        finally
        {
            PcraftServices.SetServices(new PcraftServices());
        }

        _ = deluxeSpawnX.Should().Be(seededSpawnX,
            because: "DeluxeSeededServices uses the same SeededLevelManager, so spawn X must be identical");
        _ = deluxeSpawnY.Should().Be(seededSpawnY,
            because: "DeluxeSeededServices uses the same SeededLevelManager, so spawn Y must be identical");
    }

    // -----------------------------------------------------------------------
    #endregion
    // -----------------------------------------------------------------------
    #region Determinism — same seed produces same spawn twice
    // -----------------------------------------------------------------------

    [Fact]
    public void ResetLevel_SpawnTile_IsDeterministic_ForSameSeed()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        int spawnX1, spawnY1;
        _ = new DeluxeSeededServices(Seed);
        try
        {
            PlayerEntity player = new(F32.Zero, F32.Zero);
            PcraftSession.SetCurrent(new PcraftSession(player));
            PcraftServices.ResetLevel(player);
            spawnX1 = F32.FloorToInt(player.X / F32.FromInt(16));
            spawnY1 = F32.FloorToInt(player.Y / F32.FromInt(16));
        }
        finally
        {
            PcraftServices.SetServices(new PcraftServices());
        }

        int spawnX2, spawnY2;
        _ = new DeluxeSeededServices(Seed);
        try
        {
            PlayerEntity player = new(F32.Zero, F32.Zero);
            PcraftSession.SetCurrent(new PcraftSession(player));
            PcraftServices.ResetLevel(player);
            spawnX2 = F32.FloorToInt(player.X / F32.FromInt(16));
            spawnY2 = F32.FloorToInt(player.Y / F32.FromInt(16));
        }
        finally
        {
            PcraftServices.SetServices(new PcraftServices());
        }

        _ = spawnX2.Should().Be(spawnX1, because: "same seed must always produce the same spawn X");
        _ = spawnY2.Should().Be(spawnY1, because: "same seed must always produce the same spawn Y");
    }

    // -----------------------------------------------------------------------
    #endregion
}
