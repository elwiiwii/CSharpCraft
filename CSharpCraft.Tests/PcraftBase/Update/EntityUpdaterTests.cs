using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;
using CSharpCraft.PcraftBase.Update;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Input;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Update;

[Collection("Fna")]
public sealed class EntityUpdaterTests(FnaFixture fixture) : IDisposable
{
    private TempMusicDirectory? _musicDir;
    private string? _sfxDir;

    public void Dispose()
    {
        _musicDir?.Dispose();
        if (_sfxDir is not null && Directory.Exists(_sfxDir))
            Directory.Delete(_sfxDir, recursive: true);
    }

    private sealed class NullScene : IScene
    {
        public string? Name => null;
        public void Init(ISceneSetup setup) { }
        public string? SpritesPath => null;
        public string? MapPath => null;
        public string? FlagData => null;
        public IReadOnlyList<Soundtrack> Music => [];
        public IReadOnlyList<SfxPack> Sfx => [new SfxPack("test", "pcraft_og_")];
    }

    private GameOrchestrator BuildOrchestrator(IInputManager? input = null)
    {
        _musicDir = FnaFixture.CreateTempMusicDirectory(
            "s0.ogg", "s1.ogg", "s2.ogg", "s3.ogg", "s4.ogg");
        _sfxDir = FnaFixture.CreateTempSfxDirectory(
            "pcraft_og_12", "pcraft_og_13", "pcraft_og_14", "pcraft_og_15",
            "pcraft_og_16", "pcraft_og_17", "pcraft_og_18", "pcraft_og_19", "pcraft_og_21");

        NullScene scene = new();
        GameOrchestrator orch = new(
            _musicDir.Path,
            _sfxDir,
            ".",
            scene,
            fixture.GraphicsDevice,
            fixture.GraphicsDeviceManager,
            fixture.Window,
            inputManager: input);

        orch.LoadSoundtracks([
            new Soundtrack("test", [
                new Track([new TrackPart("s0", false)], 0),
                new Track([new TrackPart("s1", true)],  1),
                new Track([new TrackPart("s2", true)],  2),
                new Track([new TrackPart("s3", true)],  3),
                new Track([new TrackPart("s4", true)],  4),
            ])
        ], "test");
        orch.LoadSfxPacks(scene.Sfx, "test");
        orch.Update(TimeSpan.Zero);
        return orch;
    }

    // --------------------------------------------------------------------------
    #region Physics — velocity and friction
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesEntityPosition_ByVelocity()
    {
        // e.x += e.vx; e.y += e.vy
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.FromInt(100), F32.FromInt(100));
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);
        DroppedItemEntity e = new(PcraftData.Wood, x: F32.FromInt(50), y: F32.FromInt(50),
            timer: F32.FromInt(120), vx: F32.FromInt(2), vy: F32.FromInt(3));
        level.Ent.Add(e);

        player.CurrentLevel = level;
        _ = EntityUpdater.Update(player, level, F32.Zero, F32.Zero);

        _ = e.X.Float.Should().BeApproximately(52f, 0.01f);
        _ = e.Y.Float.Should().BeApproximately(53f, 0.01f);
    }

    [Fact]
    public void Update_DampensVelocity_ByFrictionFactor()
    {
        // e.vx *= 0.95; e.vy *= 0.95
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.FromInt(100), F32.FromInt(100));
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);
        DroppedItemEntity e = new(PcraftData.Wood, x: F32.FromInt(50), y: F32.FromInt(50),
            timer: F32.FromInt(120), vx: F32.FromInt(4), vy: F32.FromInt(4));
        level.Ent.Add(e);

        player.CurrentLevel = level;
        _ = EntityUpdater.Update(player, level, F32.Zero, F32.Zero);

        _ = e.Vx.Float.Should().BeApproximately(3.8f, 0.02f);
        _ = e.Vy.Float.Should().BeApproximately(3.8f, 0.02f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Timer — expiry and countdown
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_RemovesEntity_WhenTimerExpiresBelow1()
    {
        // if e.timer and e.timer<1 then del(entities,e)
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.FromInt(100), F32.FromInt(100));
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);
        DroppedItemEntity e = new(PcraftData.Wood, x: F32.FromInt(10), y: F32.FromInt(10),
            timer: F32.FromFloat(0.5f));  // < 1 → remove immediately
        level.Ent.Add(e);

        player.CurrentLevel = level;
        _ = EntityUpdater.Update(player, level, F32.Zero, F32.Zero);

        _ = level.Ent.Should().NotContain(e);
    }

    [Fact]
    public void Update_DecrementTimer_WhenAbove1()
    {
        // if(e.timer) e.timer-=1
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.FromInt(100), F32.FromInt(100));
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);
        DroppedItemEntity e = new(PcraftData.Wood, x: F32.FromInt(10), y: F32.FromInt(10),
            timer: F32.FromInt(10));
        level.Ent.Add(e);

        player.CurrentLevel = level;
        _ = EntityUpdater.Update(player, level, F32.Zero, F32.Zero);

        _ = e.Timer.Float.Should().BeApproximately(9f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Item pickup
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AddsItemToInventory_WhenPickupEntityNearPlayer()
    {
        // GiveItem entity within dist<5 and timer<115 → additeminlist(invent, ...) + remove entity
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.FromInt(50), F32.FromInt(50));
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);
        // place pickup at same position (dist=0, well within 5)
        DroppedItemEntity e = new(PcraftData.Wood, x: F32.FromInt(50), y: F32.FromInt(50),
            timer: F32.FromInt(50));  // < 115
        level.Ent.Add(e);

        player.CurrentLevel = level;
        _ = EntityUpdater.Update(player, level, F32.Zero, F32.Zero);

        _ = player.Invent.Should().ContainSingle(it => it.Type == PcraftData.Wood);
        _ = level.Ent.Should().NotContain(e);
    }

    [Fact]
    public void Update_DoesNotPickUp_WhenPickupEntityFarFromPlayer()
    {
        // dist >= 5 → item stays on ground
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);
        DroppedItemEntity e = new(PcraftData.Wood, x: F32.FromInt(100), y: F32.FromInt(100),
            timer: F32.FromInt(50));
        level.Ent.Add(e);

        player.CurrentLevel = level;
        _ = EntityUpdater.Update(player, level, F32.Zero, F32.Zero);

        _ = player.Invent.Should().BeEmpty();
        _ = level.Ent.Should().Contain(e);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region canAct return value
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ReturnsCanActTrue_WhenNoInteraction()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);

        player.CurrentLevel = level;
        (F32 _, F32 _, bool canAct) = EntityUpdater.Update(player, level, F32.Zero, F32.Zero);

        _ = canAct.Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Menu opening — chest and crafting entities
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SetsCurMenu_ToChestMenu_WhenBtn5AndNearChestEntity()
    {
        // When pressing Btn5 near a chest entity (without PickupTool equipped),
        // EntityUpdater should open a ChestMenu with the player's inventory.
        FakeInputManager fakeInput = new();
        fakeInput.SetBtn(5, true);
        using GameOrchestrator orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero) { Block5 = false, Lb5 = false };
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);
        PlacedItemEntity chestEntity = new(PcraftData.Chest, x: F32.Zero, y: F32.Zero);
        level.Ent.Add(chestEntity);

        player.CurrentLevel = level;
        _ = EntityUpdater.Update(player, level, F32.Zero, F32.Zero);

        _ = player.CurMenu.Should().BeOfType<ChestMenu>()
            .Which.PlayerItems.Should().BeSameAs(player.Invent);
    }

    [Fact]
    public void Update_SetsCurMenu_ToCraftingMenu_WhenBtn5AndNearCraftBenchEntity()
    {
        // When pressing Btn5 near a crafting bench entity (without PickupTool equipped),
        // EntityUpdater should open a CraftingMenu with the bench's recipe list.
        List<Recipe> recipes = [];
        BenchItemDef testBench = new("test bench", 89, 104) { Recipes = recipes };
        FakeInputManager fakeInput = new();
        fakeInput.SetBtn(5, true);
        using GameOrchestrator orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero) { Block5 = false, Lb5 = false };
        Level level = new(0, 0, 64, 64, LevelTheme.Surface);
        PlacedItemEntity benchEntity = new(testBench, x: F32.Zero, y: F32.Zero);
        level.Ent.Add(benchEntity);

        player.CurrentLevel = level;
        _ = EntityUpdater.Update(player, level, F32.Zero, F32.Zero);

        _ = player.CurMenu.Should().BeOfType<CraftingMenu>()
            .Which.Recipes.Should().BeSameAs(recipes);
    }

    // --------------------------------------------------------------------------
    #endregion
}
