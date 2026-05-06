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
public sealed class PlayerActionUpdaterTests(FnaFixture fixture) : IDisposable
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
            "pcraft_og_19", "pcraft_og_21");

        var scene = new NullScene();
        var orch = new GameOrchestrator(
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
    #region Player position -- movement finalization
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesPlayerPosition_ByDxDy()
    {
        // dx,dy = reflectcol(plx,ply,dx,dy,isfree,0) -> plx+=dx; ply+=dy
        // With null level every tile is GrWater (passable) -> reflectcol passes through unchanged.
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level  = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.FromInt(3), F32.FromInt(4), canAct: false, nearEnemies: []);

        player.X.Float.Should().BeApproximately(3f, 0.01f);
        player.Y.Float.Should().BeApproximately(4f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Llife / Lstam smoothing
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SmoothsLlife_TowardPlife_ByOnePerFrame()
    {
        // llife += max(-1, min(1, plife-llife))
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Life  = F32.FromInt(100);
        player.Llife = F32.Zero;
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.Llife.Float.Should().BeApproximately(1f, 0.01f);
    }

    [Fact]
    public void Update_SmoothsLstam_TowardPstam_ByOnePerFrame()
    {
        // lstam += max(-1, min(1, pstam-lstam))
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Stam  = F32.FromInt(100);
        player.Lstam = F32.Zero;
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.Lstam.Float.Should().BeApproximately(1f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Stamina regeneration
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_IncrementsStamina_WhenBelowMax()
    {
        // if pstam<100: pstam = min(100, pstam+1)
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Stam = F32.FromInt(50);
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.Stam.Float.Should().BeApproximately(51f, 0.01f);
    }

    [Fact]
    public void Update_DoesNotExceedMaxStamina()
    {
        // pstam=100 -> stays 100, no regen
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Stam = F32.FromInt(100);
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.Stam.Float.Should().BeApproximately(100f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Banim countdown
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_DecrementsBanim_WhenAboveZero()
    {
        // if banim>0: banim -= 1
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Banim = F32.FromInt(5);
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.Banim.Float.Should().BeApproximately(4f, 0.01f);
    }

    [Fact]
    public void Update_DoesNotChangeBanim_WhenAtZero()
    {
        // banim=0 -> stays 0; the btn(5) block only triggers with canAct=true and btn(5) pressed
        var fakeInput = new FakeInputManager();
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Banim = F32.Zero;
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.Banim.Float.Should().BeApproximately(0f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Game time advance
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesGameTime_ByOneThirtieth()
    {
        // time += 1/30
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level  = new Level(0, 0, 8, 8, LevelTheme.Surface);
        level.Time = F32.Zero;

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        level.Time.Float.Should().BeApproximately(1f / 30f, 0.005f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Last-button-state update (lb4, lb5, block5)
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_UpdatesLb4_FromBtn4HeldState()
    {
        // lb4 = btn(4)
        var fakeInput = new FakeInputManager();
        fakeInput.SetBtn(4, true);
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Lb4 = false;
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.Lb4.Should().BeTrue();
    }

    [Fact]
    public void Update_UpdatesLb5_FromBtn5HeldState()
    {
        // lb5 = btn(5)
        var fakeInput = new FakeInputManager();
        fakeInput.SetBtn(5, true);
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        // canAct=false prevents action block from firing; also set banim>0 to prevent attack
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Block5 = true;
        player.Banim  = F32.FromInt(1);
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.Lb5.Should().BeTrue();
    }

    [Fact]
    public void Update_ClearsBlock5_WhenBtn5NotHeld()
    {
        // if not btn(5) then block5=false
        var fakeInput = new FakeInputManager();
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Block5 = true;
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.Block5.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Inventory open (Btnp 4)
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SetsCurMenu_ToMenuInvent_WhenBtnp4()
    {
        // if btnp(4) and not lb4 then curmenu=inventorymenu
        var fakeInput = new FakeInputManager();
        fakeInput.PressOnce(4);
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Lb4  = false;
        player.Life = F32.FromInt(10);
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.CurMenu.Should().BeOfType<InventoryMenu>()
            .Which.List.Should().BeSameAs(player.Invent);
    }

    [Fact]
    public void Update_DoesNotOpenMenu_WhenBtnp4ButLb4IsTrue()
    {
        // lb4 guard prevents double-open on held press
        var fakeInput = new FakeInputManager();
        fakeInput.PressOnce(4);
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Lb4    = true;
        player.CurMenu = null;
        player.Life   = F32.FromInt(10);
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.CurMenu.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Death check
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SetsCurMenu_ToDeathMenu_WhenPlifeZero()
    {
        // if plife<=0: curmenu=deathmenu
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Life = F32.Zero;
        var level = new Level(0, 0, 8, 8, LevelTheme.Surface);

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        player.CurMenu.Should().BeSameAs(PcraftData.DeathMenu);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Attack -- Btn(5) with near enemies
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ReducesEnemyLife_WhenBtn5PressedWithNearEnemies()
    {
        // banim==0 and pstam>0 and canact and nearenemies>0 -> e.life -= pow/#nearenemies (pow=1 by default)
        var fakeInput = new FakeInputManager();
        fakeInput.SetBtn(5, true);
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Banim = F32.Zero;
        player.Stam  = F32.FromInt(60);
        var zombie = new ZombieEntity(F32.Zero, F32.Zero) { Life = F32.FromInt(10) };
        var level  = new Level(0, 0, 8, 8, LevelTheme.Surface);
        level.Ene.Add(zombie);
        var nearEnemies = new List<CharacterEntity> { zombie };

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: true, nearEnemies: nearEnemies);

        zombie.Life.Float.Should().BeLessThan(10f);
    }

    [Fact]
    public void Update_RemovesEnemy_FromEnemiesList_WhenKilledByAttack()
    {
        // e.life <= 0 -> del(enemies, e)
        var fakeInput = new FakeInputManager();
        fakeInput.SetBtn(5, true);
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Banim = F32.Zero;
        player.Stam  = F32.FromInt(60);
        // Life=0.5 -- one unarmed hit (pow=1) kills it
        var zombie = new ZombieEntity(F32.Zero, F32.Zero) { Life = F32.FromFloat(0.5f) };
        var level  = new Level(0, 0, 8, 8, LevelTheme.Surface);
        level.Ene.Add(zombie);
        var nearEnemies = new List<CharacterEntity> { zombie };

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: true, nearEnemies: nearEnemies);

        level.Ene.Should().NotContain(zombie);
    }

    [Fact]
    public void Update_DoesNotAttack_WhenBanimAboveZero()
    {
        // banim>0 -> attack block skipped; enemy life unchanged
        var fakeInput = new FakeInputManager();
        fakeInput.SetBtn(5, true);
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Banim = F32.FromInt(3);
        player.Stam  = F32.FromInt(60);
        var zombie = new ZombieEntity(F32.Zero, F32.Zero) { Life = F32.FromInt(10) };
        var level  = new Level(0, 0, 8, 8, LevelTheme.Surface);
        level.Ene.Add(zombie);
        var nearEnemies = new List<CharacterEntity> { zombie };

        PlayerActionUpdater.Update(player, level, F32.Zero, F32.Zero, canAct: true, nearEnemies: nearEnemies);

        zombie.Life.Float.Should().BeApproximately(10f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
}
