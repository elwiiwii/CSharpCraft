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
public sealed class LevelManagerPureTests(FnaFixture fixture)
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
    #region SetLevel
    // --------------------------------------------------------------------------

    [Fact]
    public void SetLevel_SetsPlayerPosition_FromLevelSpawn()
    {
        // setlevel sets player.X=l.stx, player.Y=l.sty
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level = new Level(0, 0, 8, 8, isUnder: false);
        level.Stx = F32.FromInt(72);
        level.Sty = F32.FromInt(88);

        LevelManager.SetLevel(level, player);

        player.X.Should().Be(F32.FromInt(72));
        player.Y.Should().Be(F32.FromInt(88));
    }

    // --------------------------------------------------------------------------
    #endregion
    #region AddItem
    // --------------------------------------------------------------------------

    [Fact]
    public void AddItem_AddsExactly_CountEntities_ToList()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var entities = new List<ItemEntity>();

        LevelManager.AddItem(PcraftData.Wood, 3, F32.FromInt(32), F32.FromInt(32), entities);

        entities.Should().HaveCount(3);
    }

    [Fact]
    public void AddItem_SetsMaterial_AsGiveItem_OnEachEntity()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var entities = new List<ItemEntity>();

        LevelManager.AddItem(PcraftData.Stone, 2, F32.FromInt(48), F32.FromInt(16), entities);

        entities.Should().AllSatisfy(e => e.GiveItem.Should().BeSameAs(PcraftData.Stone));
    }

    [Fact]
    public void AddItem_SetsHasCol_ToTrue_OnEachEntity()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var entities = new List<ItemEntity>();

        LevelManager.AddItem(PcraftData.Wood, 2, F32.FromInt(32), F32.FromInt(32), entities);

        entities.Should().AllSatisfy(e => e.HasCol.Should().BeTrue());
    }

    [Fact]
    public void AddItem_SetsTimer_InExpectedRange_OnEachEntity()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Lua: timer = 110 + rnd(20)  ->  [110, 130)
        var entities = new List<ItemEntity>();

        LevelManager.AddItem(PcraftData.Wood, 5, F32.FromInt(32), F32.FromInt(32), entities);

        entities.Should().AllSatisfy(e =>
        {
            e.Timer.Should().NotBeNull();
            e.Timer!.Value.Should().BeGreaterThanOrEqualTo(F32.FromInt(110));
            e.Timer!.Value.Should().BeLessThan(F32.FromInt(130));
        });
    }

    [Fact]
    public void AddItem_SpawnsWithinTileContainingHitPoint()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var entities = new List<ItemEntity>();

        LevelManager.AddItem(PcraftData.Wood, 20, F32.FromInt(32), F32.FromInt(32), entities);

        entities.Should().AllSatisfy(e =>
        {
            e.X.Should().BeGreaterThanOrEqualTo(F32.FromInt(33));
            e.X.Should().BeLessThanOrEqualTo(F32.FromInt(47));
        });
    }

    // --------------------------------------------------------------------------
    #endregion
}

[Collection("Fna")]
public sealed class LevelManagerFnaTests(FnaFixture fixture)
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
    #region FillEne
    // --------------------------------------------------------------------------

    [Fact]
    public void FillEne_PopulatesEnemies_WithZombies_OnSuitableTiles()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level = new Level(0, 0, 16, 16, isUnder: false);

        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                Pico8.Mset(i, j, 2);

        LevelManager.FillEne(level, player);

        level.Ene.Count.Should().BeGreaterThan(0, "zombies should spawn on a large grass field");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region CreateLevel
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateLevel_ReturnsLevel_WithCorrectDimensions()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);

        var level = LevelManager.CreateLevel(0, 0, 64, 64, isUnder: false, player);

        level.X.Should().Be(0);
        level.Y.Should().Be(0);
        level.Sx.Should().Be(64);
        level.Sy.Should().Be(64);
        level.IsUnder.Should().BeFalse();
    }

    [Fact]
    public void CreateLevel_SetsSpawnPositionOnLevel()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);

        var level = LevelManager.CreateLevel(0, 0, 64, 64, isUnder: false, player);

        level.Stx.Should().BeGreaterThan(F32.Zero, "spawn x must be positive");
        level.Sty.Should().BeGreaterThan(F32.Zero, "spawn y must be positive");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region ResetLevel
    // --------------------------------------------------------------------------

    [Fact]
    public void ResetLevel_SetsPlayerStats_ToInitialValues()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var game   = new PcraftGame();

        LevelManager.ResetLevel(player, game, out _, out _);

        player.Stam.Should().Be(F32.FromInt(100));
        player.Lstam.Should().Be(F32.FromInt(100));
        player.Life.Should().Be(F32.FromInt(100));
        player.Llife.Should().Be(F32.FromInt(100));
    }

    [Fact]
    public void ResetLevel_ResetsMovementStats()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Prot         = F32.FromFloat(0.5f);
        player.Lrot         = F32.FromFloat(0.5f);
        player.Panim        = F32.FromInt(3);
        player.Banim        = F32.FromInt(2);
        player.Camera.Coffx = F32.FromInt(5);
        player.Camera.Coffy = F32.FromInt(5);
        var game = new PcraftGame();

        LevelManager.ResetLevel(player, game, out _, out _);

        player.Prot.Should().Be(F32.Zero);
        player.Lrot.Should().Be(F32.Zero);
        player.Panim.Should().Be(F32.Zero);
        player.Banim.Should().Be(F32.Zero);
        player.Camera.Coffx.Should().Be(F32.Zero);
        player.Camera.Coffy.Should().Be(F32.Zero);
    }

    [Fact]
    public void ResetLevel_InventoryContainsWorkbench_AsFirstItem()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var game   = new PcraftGame();

        LevelManager.ResetLevel(player, game, out _, out _);

        player.Invent.Should().NotBeEmpty("inventory must be seeded after reset");
        player.Invent[0].Type.Should().BeSameAs(PcraftData.Workbench,
            "first inventory item must be a workbench");
    }

    [Fact]
    public void ResetLevel_InventoryContainsPickupTool_AsSecondItem()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var game   = new PcraftGame();

        LevelManager.ResetLevel(player, game, out _, out _);

        player.Invent.Should().HaveCountGreaterThanOrEqualTo(2);
        player.Invent[1].Type.Should().BeSameAs(PcraftData.PickupTool,
            "second inventory item must be the pickup tool");
    }

    [Fact]
    public void ResetLevel_CreatesCaveLevel()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var game   = new PcraftGame();

        LevelManager.ResetLevel(player, game, out Level cave, out _);

        cave.Sx.Should().Be(32);
        cave.Sy.Should().Be(32);
        cave.IsUnder.Should().BeTrue();
    }

    [Fact]
    public void ResetLevel_CreatesIslandLevel()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var game   = new PcraftGame();

        LevelManager.ResetLevel(player, game, out _, out Level island);

        island.Sx.Should().Be(64);
        island.Sy.Should().Be(64);
        island.IsUnder.Should().BeFalse();
    }

    [Fact]
    public void ResetLevel_InitialisesRndWat_WithNonZeroValues()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var game   = new PcraftGame();

        LevelManager.ResetLevel(player, game, out _, out Level island);

        bool anyNonZero = false;
        for (int i = 0; i < 16 && !anyNonZero; i++)
            for (int j = 0; j < 16 && !anyNonZero; j++)
                if (island.RndWat[i][j] != F32.Zero)
                    anyNonZero = true;

        anyNonZero.Should().BeTrue("rndwat values are random and not all zero after reset");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region UpGround
    // --------------------------------------------------------------------------

    [Fact]
    public void UpGround_ConvertsFarmTile_ToSandId_WhenTimeExceedsStoredValue()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level  = new Level(0, 0, 8, 8, isUnder: false);

        // Camera at (64,64) -> ci=0, cj=0; tile (0,0) scanned
        player.Camera.Clx = F32.FromInt(64);
        player.Camera.Cly = F32.FromInt(64);

        Pico8.Mset(0, 0, PcraftData.GrFarm.Id);
        CSharpCraft.PcraftBase.Map.MapOps.DirSetData(0, 0, F32.FromInt(1), level);
        level.Time = F32.FromInt(2);

        LevelManager.UpGround(level, player);

        Pico8.Mget(0, 0).Should().Be(PcraftData.GrSand.Id,
            "expired farm tile must be replaced with sand");
    }

    [Fact]
    public void UpGround_DoesNotConvertFarmTile_WhenTimeHasNotExpired()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level  = new Level(0, 0, 8, 8, isUnder: false);

        player.Camera.Clx = F32.FromInt(64);
        player.Camera.Cly = F32.FromInt(64);

        Pico8.Mset(0, 0, PcraftData.GrFarm.Id);
        CSharpCraft.PcraftBase.Map.MapOps.DirSetData(0, 0, F32.FromInt(100), level);
        level.Time = F32.FromInt(1);

        LevelManager.UpGround(level, player);

        Pico8.Mget(0, 0).Should().Be(PcraftData.GrFarm.Id, "tile must stay farm when time has not expired");
    }

    // --------------------------------------------------------------------------
    #endregion
}
