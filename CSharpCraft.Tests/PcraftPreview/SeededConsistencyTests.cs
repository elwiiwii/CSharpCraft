using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;
using CSharpCraft.PcraftPreview;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftPreview;

// --------------------------------------------------------------------------
// Verifies that PcraftGameScene and PcraftPreviewBase render the same island
// tiles for an identical seed. Requires FNA (Pico8.Mget/Mset).
// --------------------------------------------------------------------------

[Collection("Fna")]
public sealed class SeededConsistencyTests(FnaFixture fixture)
{
    private const long Seed   = 12345L;
    private const int  Radius = 4;

    // -----------------------------------------------------------------------
    #region Helpers
    // -----------------------------------------------------------------------

    private GameOrchestrator BuildOrchestrator()
        => new(
            ".",
            ".",
            ".",
            new NullScene(),
            fixture.GraphicsDevice,
            fixture.GraphicsDeviceManager,
            fixture.Window);

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
    #region Tile consistency
    // -----------------------------------------------------------------------

    [Fact]
    public void ResetLevel_WithSeededServices_SpawnTile_MatchesPcraftWorldSampler()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        new SeededServices(Seed);
        try
        {
            var player = new PlayerEntity(F32.Zero, F32.Zero);
            PcraftServices.ResetLevel(player, out Level cave, out Level island);

            var sample = PcraftWorldSampler.Sample(Seed, Radius);

            int spawnTileX = F32.FloorToInt(player.X / F32.FromInt(16));
            int spawnTileY = F32.FloorToInt(player.Y / F32.FromInt(16));

            spawnTileX.Should().Be(sample.SpawnTileX,
                because: "island spawn X must match PcraftWorldSampler for the same seed");
            spawnTileY.Should().Be(sample.SpawnTileY,
                because: "island spawn Y must match PcraftWorldSampler for the same seed");
        }
        finally
        {
            PcraftServices.SetServices(new PcraftServices());
        }
    }

    [Fact]
    public void ResetLevel_WithSeededServices_IslandTiles_MatchPcraftWorldSampler()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        new SeededServices(Seed);
        try
        {
            var player = new PlayerEntity(F32.Zero, F32.Zero);
            PcraftServices.ResetLevel(player, out Level cave, out Level island);

            var sample = PcraftWorldSampler.Sample(Seed, Radius);

            // SeededMapGenerator.CreateMap overwrites a 3×3 hole at the level centre
            // (GridSx/2 + levelX, GridSy/2 + levelY) = (32, 32). Skip those tiles.
            const int holeX = 32;
            const int holeY = 32;

            int side = 2 * Radius + 1;
            for (int i = 0; i < side; i++)
            for (int j = 0; j < side; j++)
            {
                int tileX = sample.CenterTileX - Radius + i;
                int tileY = sample.CenterTileY - Radius + j;

                bool isHoleArea = Math.Abs(tileX - holeX) <= 1 && Math.Abs(tileY - holeY) <= 1;
                if (isHoleArea) continue;

                int expected = sample.Tiles[i, j];
                int actual   = (int)(island.Map[tileX, tileY].Surface?.Id ?? island.Map[tileX, tileY].Floor.Id);
                actual.Should().Be(expected,
                    because: $"tile ({tileX},{tileY}) must match PcraftWorldSampler output for seed {Seed}");
            }
        }
        finally
        {
            PcraftServices.SetServices(new PcraftServices());
        }
    }

    [Fact]
    public void ResetLevel_WithSeededServices_WaterTable_MatchesPcraftWorldSampler()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        new SeededServices(Seed);
        try
        {
            var player = new PlayerEntity(F32.Zero, F32.Zero);
            PcraftServices.ResetLevel(player, out Level cave, out Level island);

            var sample = PcraftWorldSampler.Sample(Seed, Radius);

            for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                island.RndWat[i][j].Should().Be(
                    F32.FromDouble(sample.RndWat[i, j]),
                    because: $"RndWat[{i}][{j}] must match PcraftWorldSampler for seed {Seed}");
        }
        finally
        {
            PcraftServices.SetServices(new PcraftServices());
        }
    }

    // -----------------------------------------------------------------------
    #endregion
}
