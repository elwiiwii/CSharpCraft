using CSharpCraft.PcraftBase;
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

    // --------------------------------------------------------------------------
    #region Noise
    // --------------------------------------------------------------------------

    [Fact]
    public void Noise_ReturnsSizedArray_MatchingInputDimensions()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Lua n[0..sx][0..sy] → C# array of size (sx+1) × (sy+1)
        // sx and sy must be powers of 2 for the diamond-square algorithm
        var result = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);

        result.GetLength(0).Should().Be(5);   // sx+1 = 5
        result.GetLength(1).Should().Be(5);   // sy+1 = 5
    }

    [Fact]
    public void Noise_IsDeterministic_WithSameSeed()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Pico8.Srand(42);
        var result1 = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);
        Pico8.Srand(42);
        var result2 = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);

        // All interior values must match
        for (int i = 0; i <= 4; i++)
        for (int j = 0; j <= 4; j++)
            result1[i, j].Should().Be(result2[i, j], because: $"element [{i},{j}] must be deterministic");
    }

    [Fact]
    public void Noise_ProducesDifferentOutput_WithDifferentSeeds()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Different seeds → different random perturbations → at least one cell differs
        var result1 = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);
        var result2 = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);

        bool anyDiffers = false;
        for (int i = 0; i <= 4 && !anyDiffers; i++)
        for (int j = 0; j <= 4 && !anyDiffers; j++)
            if (result1[i, j] != result2[i, j])
                anyDiffers = true;

        anyDiffers.Should().BeTrue("different seeds must produce different noise");
    }

    [Fact]
    public void Noise_CornersAreInitialisedToHalf_BeforeFirstIteration()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Corners of the grid start at 0.5 and the algorithm bypasses them in the
        // first step, so they remain 0.5 when step == sx (only midpoints are set).
        // Use featStep = sx to verify the algorithm runs at all with scale=1 at that step.
        var result = MapGenerator.Noise(4, 4, F32.FromFloat(0.9f), F32.FromFloat(0.2f), 4);

        // Corner (0,0) was never touched by the midpoint subdivision steps
        result[0, 0].Should().Be(F32.FromFloat(0.5f));
    }

    // --------------------------------------------------------------------------
    #endregion
    #region CreateMapStep
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateMapStep_ReturnsSizedArray_MatchingInputDimensions()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Returns an (sx+1) × (sy+1) array (same size as the noise arrays it uses internally)
        var result = MapGenerator.CreateMapStep(4, 4, 0, 1, 2, 3, 4);

        result.GetLength(0).Should().Be(5);   // sx+1
        result.GetLength(1).Should().Be(5);   // sy+1
    }

    [Fact]
    public void CreateMapStep_AllGroundIds_AreFromProvidedSet()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Lua assigns one of the 5 provided ground IDs (a..e) per tile.
        // No other values should appear in the result.
        int a = 0, b = 1, c = 2, d = 3, e = 4;
        var result = MapGenerator.CreateMapStep(4, 4, a, b, c, d, e);

        int[] allowed = [a, b, c, d, e];
        for (int i = 0; i <= 4; i++)
        for (int j = 0; j <= 4; j++)
            allowed.Should().Contain(result[i, j], because: $"tile [{i},{j}] must be one of {a},{b},{c},{d},{e}");
    }

    [Fact]
    public void CreateMapStep_UndergroundMap_AllGroundIds_AreFromProvidedSet()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // The cave variant uses different IDs: a=3,b=8,c=1,d=9,e=10
        int a = 3, b = 8, c = 1, d = 9, e = 10;
        var result = MapGenerator.CreateMapStep(4, 4, a, b, c, d, e);

        int[] allowed = [a, b, c, d, e];
        for (int i = 0; i <= 4; i++)
        for (int j = 0; j <= 4; j++)
            allowed.Should().Contain(result[i, j], because: $"cave tile [{i},{j}] must be one of {a},{b},{c},{d},{e}");
    }

    [Fact]
    public void CreateMapStep_IsDeterministic_WithSameSeed()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Pico8.Srand(42);
        var result1 = MapGenerator.CreateMapStep(4, 4, 0, 1, 2, 3, 4);
        Pico8.Srand(42);
        var result2 = MapGenerator.CreateMapStep(4, 4, 0, 1, 2, 3, 4);

        for (int i = 0; i <= 4; i++)
        for (int j = 0; j <= 4; j++)
            result1[i, j].Should().Be(result2[i, j], because: $"element [{i},{j}] must be deterministic");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region InitRndWat
    // --------------------------------------------------------------------------

    [Fact]
    public void InitRndWat_ReturnsSixteenByShixteenJaggedArray()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var result = MapGenerator.InitRndWat();

        result.Should().HaveCount(16, "outer dimension must be 16");
        foreach (var row in result)
            row.Should().HaveCount(16, "each inner array must also have 16 elements");
    }

    [Fact]
    public void InitRndWat_IsDeterministic_WithSameSeed()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Pico8.Srand(42);
        var result1 = MapGenerator.InitRndWat();
        Pico8.Srand(42);
        var result2 = MapGenerator.InitRndWat();

        for (int i = 0; i < 16; i++)
        for (int j = 0; j < 16; j++)
            result1[i][j].Should().Be(result2[i][j], because: $"rndwat[{i}][{j}] must be deterministic");
    }

    [Fact]
    public void InitRndWat_AllValues_AreInRangeZeroToHundred()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Lua: rnd(100) → [0, 100)
        var result = MapGenerator.InitRndWat();

        for (int i = 0; i < 16; i++)
        for (int j = 0; j < 16; j++)
        {
            result[i][j].Should().BeGreaterThanOrEqualTo(F32.Zero,   because: $"rndwat[{i}][{j}] must be ≥ 0");
            result[i][j].Should().BeLessThan(F32.FromInt(100),       because: $"rndwat[{i}][{j}] must be < 100");
        }
    }

    // --------------------------------------------------------------------------
    #endregion
}

[Collection("Fna")]
public sealed class MapGeneratorFnaTests(FnaFixture fixture)
{
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

    // --------------------------------------------------------------------------
    #region CreateMap — island level
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateMap_SetsPlayerSpawnToPositiveCoordinates_ForIslandLevel()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level = new Level(0, 0, 64, 64, isUnder: false);

        MapGenerator.CreateMap(level, player);

        player.X.Should().BeGreaterThan(F32.Zero, "island spawn x must be positive");
        player.Y.Should().BeGreaterThan(F32.Zero, "island spawn y must be positive");
    }

    [Fact]
    public void CreateMap_SetsCameraToPlayerSpawn_ForIslandLevel()
    {
        // Lua: clx=plx, cly=ply, cmx=plx, cmy=ply after createmap
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level = new Level(0, 0, 64, 64, isUnder: false);

        MapGenerator.CreateMap(level, player);

        player.Camera.Clx.Should().Be(player.X, "camera x must be initialised to player x");
        player.Camera.Cly.Should().Be(player.Y, "camera y must be initialised to player y");
        player.Camera.Cmx.Should().Be(player.X, "map-camera x must be initialised to player x");
        player.Camera.Cmy.Should().Be(player.Y, "map-camera y must be initialised to player y");
    }

    [Fact]
    public void CreateMap_ReturnsHolePosition_AtLevelCenter_ForIslandLevel()
    {
        // Lua: holex = levelsx/2+levelx = 64/2+0 = 32
        //      holey = levelsy/2+levely = 64/2+0 = 32
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level = new Level(0, 0, 64, 64, isUnder: false);

        var (holeX, holeY) = MapGenerator.CreateMap(level, player);

        holeX.Should().Be(32);
        holeY.Should().Be(32);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region CreateMap — cave level
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateMap_DoesNotThrow_ForCaveLevel()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level = new Level(64, 0, 32, 32, isUnder: true);

        var act = () => MapGenerator.CreateMap(level, player);

        act.Should().NotThrow();
    }

    [Fact]
    public void CreateMap_ReturnsHolePosition_AtLevelCenter_ForCaveLevel()
    {
        // Lua: holex = 32/2+64 = 80,  holey = 32/2+0 = 16
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level = new Level(64, 0, 32, 32, isUnder: true);

        var (holeX, holeY) = MapGenerator.CreateMap(level, player);

        holeX.Should().Be(80);
        holeY.Should().Be(16);
    }

    [Fact]
    public void CreateMap_SetsPlayerSpawnToPositiveCoordinates_ForCaveLevel()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level = new Level(64, 0, 32, 32, isUnder: true);

        MapGenerator.CreateMap(level, player);

        player.X.Should().BeGreaterThan(F32.Zero, "cave spawn x must be positive");
        player.Y.Should().BeGreaterThan(F32.Zero, "cave spawn y must be positive");
    }

    // --------------------------------------------------------------------------
    #endregion
}
