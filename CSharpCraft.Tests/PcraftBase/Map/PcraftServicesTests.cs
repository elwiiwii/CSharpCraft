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
public sealed class PcraftServicesPureTests(FnaFixture fixture)
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
    public void SetLevel_SetsCurrentLevel_InWorldState()
    {
        var state = new WorldState();
        var level = new Level(5, 0, 16, 16, isUnder: false);

        PcraftServices.SetLevel(level, state);

        state.CurrentLevel.Should().BeSameAs(level);
    }

    [Fact]
    public void SetLevel_SetsLevelXYSxSy_InWorldState()
    {
        var state = new WorldState();
        var level = new Level(x: 10, y: 3, sx: 32, sy: 24, isUnder: true);

        PcraftServices.SetLevel(level, state);

        state.LevelX.Should().Be(10);
        state.LevelY.Should().Be(3);
        state.LevelSx.Should().Be(32);
        state.LevelSy.Should().Be(24);
        state.LevelUnder.Should().BeTrue();
    }

    [Fact]
    public void SetLevel_SetsEntityLists_ToLevelLists()
    {
        var state = new WorldState();
        var level = new Level(0, 0, 8, 8, isUnder: false);
        var item = new ItemEntity(PcraftData.Wood, F32.Zero, F32.Zero);
        level.Ent.Add(item);

        PcraftServices.SetLevel(level, state);

        state.Entities.Should().ContainSingle().Which.Should().BeSameAs(item);
    }

    [Fact]
    public void SetLevel_SetsEnemyList_ToLevelEneList()
    {
        var state = new WorldState();
        var level = new Level(0, 0, 8, 8, isUnder: false);
        var enemy = new ZombieEntity(F32.FromInt(8), F32.FromInt(8));
        level.Ene.Add(enemy);

        PcraftServices.SetLevel(level, state);

        state.Enemies.Should().ContainSingle().Which.Should().BeSameAs(enemy);
    }

    [Fact]
    public void SetLevel_SetsData_ToLevelDat()
    {
        var state = new WorldState();
        var level = new Level(0, 0, 8, 8, isUnder: false);
        level.Dat[42] = F32.FromInt(7);

        PcraftServices.SetLevel(level, state);

        state.Data.Should().ContainKey(42).WhoseValue.Should().Be(F32.FromInt(7));
    }

    [Fact]
    public void SetLevel_SetsPlayerPosition_FromLevelSpawn()
    {
        // setlevel sets plx=l.stx, ply=l.sty
        var state = new WorldState();
        var level = new Level(0, 0, 8, 8, isUnder: false);
        level.Stx = F32.FromInt(72);
        level.Sty = F32.FromInt(88);

        PcraftServices.SetLevel(level, state);

        state.Plx.Should().Be(F32.FromInt(72));
        state.Ply.Should().Be(F32.FromInt(88));
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

        PcraftServices.AddItem(PcraftData.Wood, 3, F32.FromInt(32), F32.FromInt(32), entities);

        entities.Should().HaveCount(3);
    }

    [Fact]
    public void AddItem_SetsMaterial_AsGiveItem_OnEachEntity()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var entities = new List<ItemEntity>();

        PcraftServices.AddItem(PcraftData.Stone, 2, F32.FromInt(48), F32.FromInt(16), entities);

        entities.Should().AllSatisfy(e => e.GiveItem.Should().BeSameAs(PcraftData.Stone));
    }

    [Fact]
    public void AddItem_SetsHasCol_ToTrue_OnEachEntity()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var entities = new List<ItemEntity>();

        PcraftServices.AddItem(PcraftData.Wood, 2, F32.FromInt(32), F32.FromInt(32), entities);

        entities.Should().AllSatisfy(e => e.HasCol.Should().BeTrue());
    }

    [Fact]
    public void AddItem_SetsTimer_InExpectedRange_OnEachEntity()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // Lua: timer = 110 + rnd(20)  →  [110, 130)
        var entities = new List<ItemEntity>();

        PcraftServices.AddItem(PcraftData.Wood, 5, F32.FromInt(32), F32.FromInt(32), entities);

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
        // Lua: flr(hitx/16)*16 + rnd(14)+1  → entity spawns inside the hit tile
        // hitX=32 → tile 2 → x ∈ [32+1, 32+14+1) = [33, 47]
        var entities = new List<ItemEntity>();

        PcraftServices.AddItem(PcraftData.Wood, 20, F32.FromInt(32), F32.FromInt(32), entities);

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
public sealed class PcraftServicesFnaTests(FnaFixture fixture)
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
    public void FillEne_SetsPlayerAsFirstEnemy_InLevelEneList()
    {
        // Lua: l.ene = {entity(player, 0,0,0,0)} — player is always the first entry
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var level = new Level(0, 0, 8, 8, isUnder: false);
        state.SetLevel(level);

        PcraftServices.FillEne(level, state);

        level.Ene.Should().NotBeEmpty("FillEne must always add the player entity");
        level.Ene[0].Should().BeOfType<PlayerEntity>("first enemy slot is always the player");
    }

    [Fact]
    public void FillEne_PopulatesEnemies_WithZombies_OnSuitableTiles()
    {
        // Over a large grid with high rnd chance, zombies should appear
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        // Place a 16×16 island so there are many grass tiles
        var level = new Level(0, 0, 16, 16, isUnder: false);
        state.SetLevel(level);

        // Seed map with grass (id=2) everywhere so r<3 condition can fire
        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                Pico8.Mset(i, j, 2);

        // Set player far away from tiles so dist>50 condition is easy to satisfy
        state.Plx = F32.FromInt(0);
        state.Ply = F32.FromInt(0);

        PcraftServices.FillEne(level, state);

        // At minimum the player exists; some zombies should also spawn on a 16×16 grass map
        level.Ene.Count.Should().BeGreaterThan(1, "zombies should spawn on a large grass field");
    }

    [Fact]
    public void FillEne_SetsEnemyList_OnWorldState()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var level = new Level(0, 0, 8, 8, isUnder: false);
        state.SetLevel(level);

        PcraftServices.FillEne(level, state);

        state.Enemies.Should().BeSameAs(level.Ene);
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
        var state = new WorldState();

        var level = PcraftServices.CreateLevel(0, 0, 64, 64, isUnder: false, state);

        level.X.Should().Be(0);
        level.Y.Should().Be(0);
        level.Sx.Should().Be(64);
        level.Sy.Should().Be(64);
        level.IsUnder.Should().BeFalse();
    }

    [Fact]
    public void CreateLevel_SetsSpawnPositionOnLevel()
    {
        // l.stx = (holex - levelx)*16+8 — spawn points come from createmap hole position
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();

        var level = PcraftServices.CreateLevel(0, 0, 64, 64, isUnder: false, state);

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
        var state = new WorldState();
        var game  = new PcraftGame();

        PcraftServices.ResetLevel(state, game);

        // Lua: pstam=100, lstam=pstam, plife=100, llife=plife
        state.Pstam.Should().Be(F32.FromInt(100));
        state.Lstam.Should().Be(F32.FromInt(100));
        state.Plife.Should().Be(F32.FromInt(100));
        state.Llife.Should().Be(F32.FromInt(100));
    }

    [Fact]
    public void ResetLevel_ClearsGameFlags()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.SwitchLevel    = true;
        state.CanSwitchLevel = true;
        state.Time           = F32.FromInt(100);
        var game = new PcraftGame();

        PcraftServices.ResetLevel(state, game);

        state.SwitchLevel.Should().BeFalse();
        state.CanSwitchLevel.Should().BeFalse();
        state.Time.Should().Be(F32.Zero);
    }

    [Fact]
    public void ResetLevel_ResetsMovementStats()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.Prot  = F32.FromFloat(0.5f);
        state.Lrot  = F32.FromFloat(0.5f);
        state.Panim = F32.FromInt(3);
        state.Banim = F32.FromInt(2);
        state.Coffx = F32.FromInt(5);
        state.Coffy = F32.FromInt(5);
        var game = new PcraftGame();

        PcraftServices.ResetLevel(state, game);

        state.Prot.Should().Be(F32.Zero);
        state.Lrot.Should().Be(F32.Zero);
        state.Panim.Should().Be(F32.Zero);
        state.Banim.Should().Be(F32.Zero);
        state.Coffx.Should().Be(F32.Zero);
        state.Coffy.Should().Be(F32.Zero);
    }

    [Fact]
    public void ResetLevel_InventoryContainsWorkbench_AsFirstItem()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var game  = new PcraftGame();

        PcraftServices.ResetLevel(state, game);

        state.Invent.Should().NotBeEmpty("inventory must be seeded after reset");
        state.Invent[0].Type.Should().BeSameAs(PcraftData.Workbench,
            "first inventory item must be a workbench");
    }

    [Fact]
    public void ResetLevel_InventoryContainsPickupTool_AsSecondItem()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var game  = new PcraftGame();

        PcraftServices.ResetLevel(state, game);

        state.Invent.Should().HaveCountGreaterThanOrEqualTo(2);
        state.Invent[1].Type.Should().BeSameAs(PcraftData.PickupTool,
            "second inventory item must be the pickup tool");
    }

    [Fact]
    public void ResetLevel_CreatesCaveLevel_On_WorldState()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var game  = new PcraftGame();

        PcraftServices.ResetLevel(state, game);

        state.Cave.Should().NotBeNull("cave level must be created by reset");
        state.Cave!.Sx.Should().Be(32);
        state.Cave.Sy.Should().Be(32);
        state.Cave.IsUnder.Should().BeTrue();
    }

    [Fact]
    public void ResetLevel_CreatesIslandLevel_On_WorldState()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var game  = new PcraftGame();

        PcraftServices.ResetLevel(state, game);

        state.Island.Should().NotBeNull("island level must be created by reset");
        state.Island!.Sx.Should().Be(64);
        state.Island.Sy.Should().Be(64);
        state.Island.IsUnder.Should().BeFalse();
    }

    [Fact]
    public void ResetLevel_SetsCurrentLevel_ToIsland()
    {
        // After reset the active level is the island (last createlevel call)
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var game  = new PcraftGame();

        PcraftServices.ResetLevel(state, game);

        state.CurrentLevel.Should().BeSameAs(state.Island,
            "current level must be the island after reset");
    }

    [Fact]
    public void ResetLevel_InitialisesRndWat_WithNonZeroValues()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var game  = new PcraftGame();

        PcraftServices.ResetLevel(state, game);

        bool anyNonZero = false;
        for (int i = 0; i < 16 && !anyNonZero; i++)
            for (int j = 0; j < 16 && !anyNonZero; j++)
                if (state.RndWat[i][j] != F32.Zero)
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
        // Lua: if gr==grfarm and time>d then mset(i+levelx, j, grsand.id)
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var level = new Level(0, 0, 8, 8, isUnder: false);
        state.SetLevel(level);

        // Camera at (64,64) → ci=0, cj=0; tile (0,0) scanned
        state.Clx = F32.FromInt(64);
        state.Cly = F32.FromInt(64);

        // Set tile (0,0) to GrFarm (id=5)
        Pico8.Mset(0, 0, PcraftData.GrFarm.Id);

        // Store expiry data = 1; set time = 2 so time > d
        CSharpCraft.PcraftBase.Map.MapOps.DirSetData(0, 0, F32.FromInt(1), state);
        state.Time = F32.FromInt(2);

        PcraftServices.UpGround(state);

        Pico8.Mget(0, 0).Should().Be(PcraftData.GrSand.Id,
            "expired farm tile must be replaced with sand");
    }

    [Fact]
    public void UpGround_DoesNotConvertFarmTile_WhenTimeHasNotExpired()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var level = new Level(0, 0, 8, 8, isUnder: false);
        state.SetLevel(level);

        state.Clx = F32.FromInt(64);
        state.Cly = F32.FromInt(64);

        Pico8.Mset(0, 0, PcraftData.GrFarm.Id);
        CSharpCraft.PcraftBase.Map.MapOps.DirSetData(0, 0, F32.FromInt(100), state);
        state.Time = F32.FromInt(1);   // time < d

        PcraftServices.UpGround(state);

        Pico8.Mget(0, 0).Should().Be(PcraftData.GrFarm.Id, "tile must stay farm when time has not expired");
    }

    // --------------------------------------------------------------------------
    #endregion
}
