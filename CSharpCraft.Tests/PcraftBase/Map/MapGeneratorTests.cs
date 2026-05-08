using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Map;

[Collection("Fna")]
public sealed class MapGeneratorPureTests(FnaFixture fixture)
{
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

    // --------------------------------------------------------------------------
    #region Noise
    // --------------------------------------------------------------------------

    [Fact]
    public void Noise_ReturnsSizedArray_MatchingInputDimensions()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Lua n[0..sx][0..sy] → C# array of size (sx+1) × (sy+1)
        // sx and sy must be powers of 2 for the diamond-square algorithm
        F32[,] result = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);

        _ = result.GetLength(0).Should().Be(5);   // sx+1 = 5
        _ = result.GetLength(1).Should().Be(5);   // sy+1 = 5
    }

    [Fact]
    public void Noise_IsDeterministic_WithSameSeed()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Pico8.Srand(42);
        F32[,] result1 = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);
        Pico8.Srand(42);
        F32[,] result2 = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);

        // All interior values must match
        for (int i = 0; i <= 4; i++)
            for (int j = 0; j <= 4; j++)
                _ = result1[i, j].Should().Be(result2[i, j], because: $"element [{i},{j}] must be deterministic");
    }

    [Fact]
    public void Noise_ProducesDifferentOutput_WithDifferentSeeds()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Different seeds → different random perturbations → at least one cell differs
        F32[,] result1 = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);
        F32[,] result2 = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);

        bool anyDiffers = false;
        for (int i = 0; i <= 4 && !anyDiffers; i++)
            for (int j = 0; j <= 4 && !anyDiffers; j++)
                if (result1[i, j] != result2[i, j])
                    anyDiffers = true;

        _ = anyDiffers.Should().BeTrue("different seeds must produce different noise");
    }

    [Fact]
    public void Noise_CornersAreInitialisedToHalf_BeforeFirstIteration()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Corners of the grid start at 0.5 and the algorithm bypasses them in the
        // first step, so they remain 0.5 when step == sx (only midpoints are set).
        // Use featStep = sx to verify the algorithm runs at all with scale=1 at that step.
        F32[,] result = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);

        // Corner (0,0) was never touched by the midpoint subdivision steps
        _ = result[0, 0].Should().Be(F32.FromFloat(0.5f));
    }

    // --------------------------------------------------------------------------
    #endregion
    #region CreateMapStep
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateMapStep_ReturnsSizedArray_MatchingInputDimensions()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Returns an (sx+1) × (sy+1) array (same size as the noise arrays it uses internally)
        TileId[,] result = MapGenerator.CreateMapStep(4, 4, TileId.Water, TileId.Sand, TileId.Grass, TileId.Rock, TileId.Tree);

        _ = result.GetLength(0).Should().Be(5);   // sx+1
        _ = result.GetLength(1).Should().Be(5);   // sy+1
    }

    [Fact]
    public void CreateMapStep_AllGroundIds_AreFromProvidedSet()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Lua assigns one of the 5 provided ground IDs (a..e) per tile.
        // No other values should appear in the result.
        TileId a = TileId.Water, b = TileId.Sand, c = TileId.Grass, d = TileId.Rock, e = TileId.Tree;
        TileId[,] result = MapGenerator.CreateMapStep(4, 4, a, b, c, d, e);

        TileId[] allowed = [a, b, c, d, e];
        for (int i = 0; i <= 4; i++)
            for (int j = 0; j <= 4; j++)
                _ = allowed.Should().Contain(result[i, j], because: $"tile [{i},{j}] must be one of {a},{b},{c},{d},{e}");
    }

    [Fact]
    public void CreateMapStep_UndergroundMap_AllGroundIds_AreFromProvidedSet()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // The cave variant uses different IDs: a=3,b=8,c=1,d=9,e=10
        TileId a = TileId.Rock, b = TileId.Iron, c = TileId.Sand, d = TileId.Gold, e = TileId.Gem;
        TileId[,] result = MapGenerator.CreateMapStep(4, 4, a, b, c, d, e);

        TileId[] allowed = [a, b, c, d, e];
        for (int i = 0; i <= 4; i++)
            for (int j = 0; j <= 4; j++)
                _ = allowed.Should().Contain(result[i, j], because: $"cave tile [{i},{j}] must be one of {a},{b},{c},{d},{e}");
    }

    [Fact]
    public void CreateMapStep_IsDeterministic_WithSameSeed()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Pico8.Srand(42);
        TileId[,] result1 = MapGenerator.CreateMapStep(4, 4, TileId.Water, TileId.Sand, TileId.Grass, TileId.Rock, TileId.Tree);
        Pico8.Srand(42);
        TileId[,] result2 = MapGenerator.CreateMapStep(4, 4, TileId.Water, TileId.Sand, TileId.Grass, TileId.Rock, TileId.Tree);

        for (int i = 0; i <= 4; i++)
            for (int j = 0; j <= 4; j++)
                _ = result1[i, j].Should().Be(result2[i, j], because: $"element [{i},{j}] must be deterministic");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region InitRndWat
    // --------------------------------------------------------------------------

    [Fact]
    public void InitRndWat_ReturnsSixteenByShixteenJaggedArray()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        F32[][] result = MapGenerator.InitRndWat();

        _ = result.Should().HaveCount(16, "outer dimension must be 16");
        foreach (F32[] row in result)
            _ = row.Should().HaveCount(16, "each inner array must also have 16 elements");
    }

    [Fact]
    public void InitRndWat_IsDeterministic_WithSameSeed()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Pico8.Srand(42);
        F32[][] result1 = MapGenerator.InitRndWat();
        Pico8.Srand(42);
        F32[][] result2 = MapGenerator.InitRndWat();

        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                _ = result1[i][j].Should().Be(result2[i][j], because: $"rndwat[{i}][{j}] must be deterministic");
    }

    [Fact]
    public void InitRndWat_AllValues_AreInRangeZeroToHundred()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Lua: rnd(100) → [0, 100)
        F32[][] result = MapGenerator.InitRndWat();

        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
            {
                _ = result[i][j].Should().BeGreaterThanOrEqualTo(F32.Zero, because: $"rndwat[{i}][{j}] must be ≥ 0");
                _ = result[i][j].Should().BeLessThan(F32.FromInt(100), because: $"rndwat[{i}][{j}] must be < 100");
            }
    }

    // --------------------------------------------------------------------------
    #endregion
}

[Collection("Fna")]
public sealed class MapGeneratorFnaTests(FnaFixture fixture)
{
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

    // --------------------------------------------------------------------------
    #region CreateMap — island level
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateMap_SetsPlayerSpawnToPositiveCoordinates_ForIslandLevel()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);

        _ = MapGenerator.CreateMap(level, player);

        _ = player.X.Should().BeGreaterThan(F32.Zero, "island spawn x must be positive");
        _ = player.Y.Should().BeGreaterThan(F32.Zero, "island spawn y must be positive");
    }

    [Fact]
    public void CreateMap_SetsCameraToPlayerSpawn_ForIslandLevel()
    {
        // Lua: clx=plx, cly=ply, cmx=plx, cmy=ply after createmap
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);

        _ = MapGenerator.CreateMap(level, player);

        _ = player.Camera.Clx.Should().Be(player.X, "camera x must be initialised to player x");
        _ = player.Camera.Cly.Should().Be(player.Y, "camera y must be initialised to player y");
        _ = player.Camera.Cmx.Should().Be(player.X, "map-camera x must be initialised to player x");
        _ = player.Camera.Cmy.Should().Be(player.Y, "map-camera y must be initialised to player y");
    }

    [Fact]
    public void CreateMap_ReturnsHolePosition_AtLevelCenter_ForIslandLevel()
    {
        // Lua: holex = levelsx/2+levelx = 64/2+0 = 32
        //      holey = levelsy/2+levely = 64/2+0 = 32
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);

        (int holeX, int holeY) = MapGenerator.CreateMap(level, player);

        _ = holeX.Should().Be(32);
        _ = holeY.Should().Be(32);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region CreateMap — cave level
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateMap_DoesNotThrow_ForCaveLevel()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(64, 0, 32, 32, LevelTheme.Cave);

        Func<(int holeX, int holeY)> act = () => MapGenerator.CreateMap(level, player);

        _ = act.Should().NotThrow();
    }

    [Fact]
    public void CreateMap_ReturnsHolePosition_AtLevelCenter_ForCaveLevel()
    {
        // Lua: holex = 32/2+64 = 80,  holey = 32/2+0 = 16
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(64, 0, 32, 32, LevelTheme.Cave);

        (int holeX, int holeY) = MapGenerator.CreateMap(level, player);

        _ = holeX.Should().Be(80);
        _ = holeY.Should().Be(16);
    }

    [Fact]
    public void CreateMap_SetsPlayerSpawnToPositiveCoordinates_ForCaveLevel()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(64, 0, 32, 32, LevelTheme.Cave);

        _ = MapGenerator.CreateMap(level, player);

        _ = player.X.Should().BeGreaterThan(F32.Zero, "cave spawn x must be positive");
        _ = player.Y.Should().BeGreaterThan(F32.Zero, "cave spawn y must be positive");
    }

    // --------------------------------------------------------------------------
    #endregion
}
