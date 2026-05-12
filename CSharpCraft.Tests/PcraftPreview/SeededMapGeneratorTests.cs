using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftSeeded;
using CSharpCraft.PcraftSeeded.Noise;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftPreview;

// --------------------------------------------------------------------------
// SeededMapGenerator — no graphics needed for InitRndWat / spawn consistency.
// Tests that verify level.Map writes are in [Collection("Fna")].
// --------------------------------------------------------------------------

public sealed class SeededMapGeneratorTests
{
    private const long Seed = 12345L;
    private const int GridSx = 64;
    private const int GridSy = 64;

    // --------------------------------------------------------------------------
    #region InitRndWat — formula matches PcraftWorldSampler.Sample RndWat
    // --------------------------------------------------------------------------

    [Fact]
    public void InitRndWat_ReturnsJaggedArray16x16()
    {
        F32[][] rnd = SeededMapGenerator.InitRndWat(Seed);

        _ = rnd.Should().HaveCount(16, "outer dimension must be 16");
        foreach (F32[] row in rnd)
            _ = row.Should().HaveCount(16, "each inner row must be 16 elements");
    }

    [Fact]
    public void InitRndWat_MatchesSampleResultRndWat_ForEachCell()
    {
        // SampleResult.RndWat[i,j] and InitRndWat(seed)[i][j] use the same hash formula.
        SampleResult sampleResult = PcraftWorldSampler.Sample(Seed, radius: 4);
        F32[][] rnd = SeededMapGenerator.InitRndWat(Seed);

        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                _ = rnd[i][j].Should().Be(
                    F32.FromDouble(sampleResult.RndWat[i, j]),
                    because: $"InitRndWat[{i}][{j}] must match SampleResult.RndWat[{i},{j}]");
    }

    [Fact]
    public void InitRndWat_ProducesDifferentResults_ForDifferentSeeds()
    {
        F32[][] rnd1 = SeededMapGenerator.InitRndWat(Seed);
        F32[][] rnd2 = SeededMapGenerator.InitRndWat(Seed + 1);

        bool anyDifferent = false;
        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                if (rnd1[i][j] != rnd2[i][j]) { anyDifferent = true; break; }

        _ = anyDifferent.Should().BeTrue("different seeds must produce different noise tables");
    }

    [Fact]
    public void InitRndWat_ProducesIdenticalResults_ForSameSeed()
    {
        F32[][] rnd1 = SeededMapGenerator.InitRndWat(Seed);
        F32[][] rnd2 = SeededMapGenerator.InitRndWat(Seed);

        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                _ = rnd1[i][j].Should().Be(rnd2[i][j],
                    because: $"InitRndWat must be deterministic at [{i}][{j}]");
    }

    [Fact]
    public void InitRndWat_AllValuesInRange_ZeroToOneHundred()
    {
        F32[][] rnd = SeededMapGenerator.InitRndWat(Seed);

        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                _ = rnd[i][j].Double.Should().BeInRange(0, 100,
                    because: $"InitRndWat[{i}][{j}] must be in [0, 100)");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Spawn consistency — FindSpawn result matches PcraftWorldSampler
    // --------------------------------------------------------------------------

    [Fact]
    public void FindSpawn_MatchesPcraftWorldSampler_SpawnTile_ForSameSeed()
    {
        // SeededMapGenerator uses the same SeededNoiseGrid + SpawnFinder internally.
        // Building both sides manually confirms the algorithm produces the same spawn.
        SeededNoiseGrid cur = new(Seed, GridSx, GridSy, GridSx, 0.9, 0.2, 0);
        SeededNoiseGrid cur2 = new(Seed, GridSx, GridSy, 8, 0.9, 0.4, 1);
        SeededNoiseGrid cur3 = new(Seed, GridSx, GridSy, 8, 0.9, 0.3, 2);
        SeededNoiseGrid cur4 = new(Seed, GridSx, GridSy, 4, 0.8, 1.1, 3);
        MapClassifier classifier = new(cur, cur2, cur3, cur4,
            GridSx, GridSy, 0, 1, 2, 3, 4);
        (int tileX, int tileY)? spawn = SpawnFinder.FindSpawn(Seed, classifier, GridSx, GridSy);

        SampleResult sample = PcraftWorldSampler.Sample(Seed, radius: 4);

        _ = spawn.Should().NotBeNull("seed 12345 produces a valid spawn tile");
        _ = spawn!.Value.tileX.Should().Be(sample.SpawnTileX,
            because: "spawn X must match PcraftWorldSampler for the same seed");
        _ = spawn!.Value.tileY.Should().Be(sample.SpawnTileY,
            because: "spawn Y must match PcraftWorldSampler for the same seed");
    }

    [Theory]
    [InlineData(0L)]      // zero seed
    [InlineData(1L)]      // small seed
    [InlineData(99999L)]  // larger seed
    [InlineData(-42L)]    // negative seed
    public void FindSpawn_IsValid_OrNullConsistentWithSampler_AcrossSeeds(long seed)
    {
        SeededNoiseGrid cur = new(seed, GridSx, GridSy, GridSx, 0.9, 0.2, 0);
        SeededNoiseGrid cur2 = new(seed, GridSx, GridSy, 8, 0.9, 0.4, 1);
        SeededNoiseGrid cur3 = new(seed, GridSx, GridSy, 8, 0.9, 0.3, 2);
        SeededNoiseGrid cur4 = new(seed, GridSx, GridSy, 4, 0.8, 1.1, 3);
        MapClassifier classifier = new(cur, cur2, cur3, cur4,
            GridSx, GridSy, 0, 1, 2, 3, 4);
        (int tileX, int tileY)? spawn = SpawnFinder.FindSpawn(seed, classifier, GridSx, GridSy);

        SampleResult sample = PcraftWorldSampler.Sample(seed, radius: 4);

        if (spawn is null)
        {
            _ = sample.SpawnTileX.Should().Be(-1, "null spawn must match sampler returning -1");
            _ = sample.SpawnTileY.Should().Be(-1);
        }
        else
        {
            _ = spawn.Value.tileX.Should().Be(sample.SpawnTileX);
            _ = spawn.Value.tileY.Should().Be(sample.SpawnTileY);
        }
    }

    // --------------------------------------------------------------------------
    #endregion
}

// --------------------------------------------------------------------------
// Fna-collection tests — require graphics device for SeededMapGenerator.
// --------------------------------------------------------------------------

[Collection("Fna")]
public sealed class SeededMapGeneratorFnaTests(FnaFixture fixture)
{
    private const long Seed = 12345L;
    private const int GridSx = 64;
    private const int GridSy = 64;

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

    // Minimal player+level configured for island (levelX=0, levelY=0, sx=64, sy=64).
    private static (Level level, PlayerEntity player) MakePlayerAndLevel()
    {
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, GridSx, GridSy, LevelTheme.Surface);
        return (level, player);
    }

    // --------------------------------------------------------------------------
    #region CreateMap — tiles written to Pico8 memory match MapClassifier output
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateMap_WritesTilesMatchingMapClassifier_ForFullGrid()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        // Build the expected tiles independently using the same grids.
        SeededNoiseGrid cur = new(Seed, GridSx, GridSy, GridSx, 0.9, 0.2, 0);
        SeededNoiseGrid cur2 = new(Seed, GridSx, GridSy, 8, 0.9, 0.4, 1);
        SeededNoiseGrid cur3 = new(Seed, GridSx, GridSy, 8, 0.9, 0.3, 2);
        SeededNoiseGrid cur4 = new(Seed, GridSx, GridSy, 4, 0.8, 1.1, 3);
        MapClassifier classifier = new(cur, cur2, cur3, cur4,
            GridSx, GridSy, 0, 1, 2, 3, 4);

        (Level? level, PlayerEntity? player) = MakePlayerAndLevel();
        _ = SeededMapGenerator.CreateMap(level, player, Seed);

        // The 3×3 centre area is overwritten with the hole structure — skip those tiles.
        int holeX = GridSx / 2;
        int holeY = GridSy / 2;
        for (int i = 0; i < GridSx; i++)
        {
            for (int j = 0; j < GridSy; j++)
            {
                bool isHoleArea = Math.Abs(i - holeX) <= 1 && Math.Abs(j - holeY) <= 1;
                if (isHoleArea) continue;

                int expected = classifier.ClassifyTile(i, j);
                int actual = PcraftData.TileIdFor(level.Map[i, j].Type);
                _ = actual.Should().Be(expected,
                    because: $"tile ({i},{j}) must match MapClassifier output");
            }
        }
    }

    [Fact]
    public void CreateMap_SetsPlayerSpawn_ToValidSpawnTile()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        (Level? level, PlayerEntity? player) = MakePlayerAndLevel();
        _ = SeededMapGenerator.CreateMap(level, player, Seed);

        int spawnTileX = F32.FloorToInt(player.X / F32.FromInt(16));
        int spawnTileY = F32.FloorToInt(player.Y / F32.FromInt(16));
        int tileId = PcraftData.TileIdFor(level.Map[spawnTileX, spawnTileY].Type);

        _ = tileId.Should().BeOneOf(new[] { 1, 2 },
            because: "player spawn must be on a sand (1) or rare (2) tile");
    }

    [Fact]
    public void CreateMap_PlayerSpawn_MatchesPcraftWorldSampler_SpawnTile()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        (Level? level, PlayerEntity? player) = MakePlayerAndLevel();
        _ = SeededMapGenerator.CreateMap(level, player, Seed);

        int spawnTileX = F32.FloorToInt(player.X / F32.FromInt(16));
        int spawnTileY = F32.FloorToInt(player.Y / F32.FromInt(16));

        SampleResult sample = PcraftWorldSampler.Sample(Seed, radius: 4);

        _ = spawnTileX.Should().Be(sample.SpawnTileX,
            because: "spawn tile X must match PcraftWorldSampler for same seed");
        _ = spawnTileY.Should().Be(sample.SpawnTileY,
            because: "spawn tile Y must match PcraftWorldSampler for same seed");
    }

    [Fact]
    public void CreateMap_SetsCamera_ToMatchPlayerSpawn()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        (Level? level, PlayerEntity? player) = MakePlayerAndLevel();
        _ = SeededMapGenerator.CreateMap(level, player, Seed);

        _ = player.Camera.Clx.Should().Be(player.X, because: "camera X must be synced to player X after map creation");
        _ = player.Camera.Cly.Should().Be(player.Y, because: "camera Y must be synced to player Y after map creation");
        _ = player.Camera.Cmx.Should().Be(player.X, because: "camera move target X must be synced to player X");
        _ = player.Camera.Cmy.Should().Be(player.Y, because: "camera move target Y must be synced to player Y");
    }

    [Fact]
    public void CreateMap_IsIdempotent_ForSameSeed()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        // Running CreateMap twice with the same seed overwrites Pico8 memory identically.
        (Level? level1, PlayerEntity? player1) = MakePlayerAndLevel();
        _ = SeededMapGenerator.CreateMap(level1, player1, Seed);
        int plx1 = F32.FloorToInt(player1.X);

        (Level? level2, PlayerEntity? player2) = MakePlayerAndLevel();
        _ = SeededMapGenerator.CreateMap(level2, player2, Seed);
        int plx2 = F32.FloorToInt(player2.X);

        _ = plx1.Should().Be(plx2, because: "same seed must always produce the same spawn position");

        for (int i = 0; i < GridSx; i++)
            for (int j = 0; j < GridSy; j++)
                _ = level1.Map[i, j].Type.Should().BeSameAs(level2.Map[i, j].Type,
                    because: $"tile ({i},{j}) must be identical for same seed");
    }

    // --------------------------------------------------------------------------
    #endregion
}
