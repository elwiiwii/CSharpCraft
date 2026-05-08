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
    #region SetLevel
    // --------------------------------------------------------------------------

    [Fact]
    public void SetLevel_SetsPlayerPosition_FromLevelSpawn()
    {
        // setlevel sets player.X=l.stx, player.Y=l.sty
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 8, 8, LevelTheme.Surface)
        {
            Stx = F32.FromInt(72),
            Sty = F32.FromInt(88)
        };

        LevelManager.SetLevel(level, player);

        _ = player.X.Should().Be(F32.FromInt(72));
        _ = player.Y.Should().Be(F32.FromInt(88));
    }

    // --------------------------------------------------------------------------
    #endregion
    #region AddItem
    // --------------------------------------------------------------------------

    [Fact]
    public void AddItem_AddsExactly_CountEntities_ToList()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        List<Entity> entities = [];

        LevelManager.AddItem(PcraftData.Wood, 3, F32.FromInt(32), F32.FromInt(32), entities);

        _ = entities.Should().HaveCount(3);
    }

    [Fact]
    public void AddItem_SetsType_OnEachEntity()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        List<Entity> entities = [];

        LevelManager.AddItem(PcraftData.Stone, 2, F32.FromInt(48), F32.FromInt(16), entities);

        _ = entities.Should().AllSatisfy(e => (e as DroppedItemEntity)!.Type.Should().BeSameAs(PcraftData.Stone));
    }

    [Fact]
    public void AddItem_CreatesDroppedItemEntities()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        List<Entity> entities = [];

        LevelManager.AddItem(PcraftData.Wood, 2, F32.FromInt(32), F32.FromInt(32), entities);

        _ = entities.Should().AllSatisfy(e => (e is DroppedItemEntity).Should().BeTrue());
    }

    [Fact]
    public void AddItem_SetsTimer_InExpectedRange_OnEachEntity()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Lua: timer = 110 + rnd(20)  ->  [110, 130)
        List<Entity> entities = [];

        LevelManager.AddItem(PcraftData.Wood, 5, F32.FromInt(32), F32.FromInt(32), entities);

        _ = entities.Should().AllSatisfy(e =>
        {
            DroppedItemEntity dropped = (e as DroppedItemEntity)!;
            _ = dropped.Timer.Should().BeGreaterThanOrEqualTo(F32.FromInt(110));
            _ = dropped.Timer.Should().BeLessThan(F32.FromInt(130));
        });
    }

    [Fact]
    public void AddItem_SpawnsWithinTileContainingHitPoint()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        List<Entity> entities = [];

        LevelManager.AddItem(PcraftData.Wood, 20, F32.FromInt(32), F32.FromInt(32), entities);

        _ = entities.Should().AllSatisfy(e =>
        {
            _ = e.X.Should().BeGreaterThanOrEqualTo(F32.FromInt(33));
            _ = e.X.Should().BeLessThanOrEqualTo(F32.FromInt(47));
        });
    }

    // --------------------------------------------------------------------------
    #endregion
}

[Collection("Fna")]
public sealed class LevelManagerFnaTests(FnaFixture fixture)
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
    #region FillEne
    // --------------------------------------------------------------------------

    [Fact]
    public void FillEne_PopulatesEnemies_WithZombies_OnSuitableTiles()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 16, 16, LevelTheme.Surface);

        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                level.Map[i, j] = PcraftData.TileFor(TileId.Grass);

        LevelManager.FillEne(level, player);

        _ = level.Ene.Count.Should().BeGreaterThan(0, "zombies should spawn on a large grass field");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region CreateLevel
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateLevel_ReturnsLevel_WithCorrectDimensions()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);

        Level level = LevelManager.CreateLevel(0, 0, 64, 64, LevelTheme.Surface, player);

        _ = level.X.Should().Be(0);
        _ = level.Y.Should().Be(0);
        _ = level.Sx.Should().Be(64);
        _ = level.Sy.Should().Be(64);
        _ = level.Theme.Should().Be(LevelTheme.Surface);
    }

    [Fact]
    public void CreateLevel_SetsSpawnPositionOnLevel()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);

        Level level = LevelManager.CreateLevel(0, 0, 64, 64, LevelTheme.Surface, player);

        _ = level.Stx.Should().BeGreaterThan(F32.Zero, "spawn x must be positive");
        _ = level.Sty.Should().BeGreaterThan(F32.Zero, "spawn y must be positive");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region ResetLevel
    // --------------------------------------------------------------------------

    [Fact]
    public void ResetLevel_SetsPlayerStats_ToInitialValues()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);

        PcraftSession.SetCurrent(new PcraftSession(player));

        LevelManager.ResetLevel(player);

        _ = player.Stam.Should().Be(F32.FromInt(100));
        _ = player.Lstam.Should().Be(F32.FromInt(100));
        _ = player.Life.Should().Be(F32.FromInt(100));
        _ = player.Llife.Should().Be(F32.FromInt(100));
    }

    [Fact]
    public void ResetLevel_ResetsMovementStats()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Prot = F32.FromFloat(0.5f),
            Lrot = F32.FromFloat(0.5f),
            Panim = F32.FromInt(3),
            Banim = F32.FromInt(2)
        };
        player.Camera.Coffx = F32.FromInt(5);
        player.Camera.Coffy = F32.FromInt(5);

        PcraftSession.SetCurrent(new PcraftSession(player));

        LevelManager.ResetLevel(player);

        _ = player.Prot.Should().Be(F32.Zero);
        _ = player.Lrot.Should().Be(F32.Zero);
        _ = player.Panim.Should().Be(F32.Zero);
        _ = player.Banim.Should().Be(F32.Zero);
        _ = player.Camera.Coffx.Should().Be(F32.Zero);
        _ = player.Camera.Coffy.Should().Be(F32.Zero);
    }

    [Fact]
    public void ResetLevel_InventoryContainsWorkbench_AsFirstItem()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);

        PcraftSession.SetCurrent(new PcraftSession(player));

        LevelManager.ResetLevel(player);

        _ = player.Invent.Should().NotBeEmpty("inventory must be seeded after reset");
        _ = player.Invent[0].Type.Should().BeSameAs(PcraftData.Workbench,
            "first inventory item must be a workbench");
    }

    [Fact]
    public void ResetLevel_InventoryContainsPickupTool_AsSecondItem()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);

        PcraftSession.SetCurrent(new PcraftSession(player));

        LevelManager.ResetLevel(player);

        _ = player.Invent.Should().HaveCountGreaterThanOrEqualTo(2);
        _ = player.Invent[1].Type.Should().BeSameAs(PcraftData.PickupTool,
            "second inventory item must be the pickup tool");
    }

    [Fact]
    public void ResetLevel_CreatesCaveLevel()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);

        PcraftSession.SetCurrent(new PcraftSession(player));

        LevelManager.ResetLevel(player);
        Level cave = PcraftSession.Current.Cave!;

        _ = cave.Sx.Should().Be(32);
        _ = cave.Sy.Should().Be(32);
        _ = cave.Theme.Should().Be(LevelTheme.Cave);
    }

    [Fact]
    public void ResetLevel_CreatesIslandLevel()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);

        PcraftSession.SetCurrent(new PcraftSession(player));

        LevelManager.ResetLevel(player);
        Level island = PcraftSession.Current.Island!;

        _ = island.Sx.Should().Be(64);
        _ = island.Sy.Should().Be(64);
        _ = island.Theme.Should().Be(LevelTheme.Surface);
    }

    [Fact]
    public void ResetLevel_InitialisesRndWat_WithNonZeroValues()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);

        PcraftSession.SetCurrent(new PcraftSession(player));

        LevelManager.ResetLevel(player);
        Level island = PcraftSession.Current.Island!;

        bool anyNonZero = false;
        for (int i = 0; i < 16 && !anyNonZero; i++)
            for (int j = 0; j < 16 && !anyNonZero; j++)
                if (island.RndWat[i][j] != F32.Zero)
                    anyNonZero = true;

        _ = anyNonZero.Should().BeTrue("rndwat values are random and not all zero after reset");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region UpGround
    // --------------------------------------------------------------------------

    [Fact]
    public void UpGround_ConvertsFarmTile_ToSandId_WhenTimeExceedsStoredValue()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        // Camera at (64,64) -> ci=0, cj=0; tile (0,0) scanned
        player.Camera.Clx = F32.FromInt(64);
        player.Camera.Cly = F32.FromInt(64);

        level.Map[0, 0] = PcraftData.TileFor(TileId.Farm) with { GrowthTimer = F32.FromInt(1) };
        level.Time = F32.FromInt(2);

        LevelManager.UpGround(level, player);

        _ = level.Map[0, 0].Type.Should().BeSameAs(PcraftData.TileSand,
            "expired farm tile must be replaced with sand");
    }

    [Fact]
    public void UpGround_DoesNotConvertFarmTile_WhenTimeHasNotExpired()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.Camera.Clx = F32.FromInt(64);
        player.Camera.Cly = F32.FromInt(64);

        level.Map[0, 0] = PcraftData.TileFor(TileId.Farm) with { GrowthTimer = F32.FromInt(100) };
        level.Time = F32.FromInt(1);

        LevelManager.UpGround(level, player);

        _ = level.Map[0, 0].Type.Should().BeSameAs(PcraftData.TileFarm, "tile must stay farm when time has not expired");
    }

    // --------------------------------------------------------------------------
    #endregion
}
