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
    #region Player position -- movement finalization
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesPlayerPosition_ByDxDy()
    {
        // dx,dy = reflectcol(plx,ply,dx,dy,isfree,0) -> plx+=dx; ply+=dy
        // With null level every tile is GrWater (passable) -> reflectcol passes through unchanged.
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.FromInt(3), F32.FromInt(4), canAct: false, nearEnemies: []);

        _ = player.X.Float.Should().BeApproximately(3f, 0.01f);
        _ = player.Y.Float.Should().BeApproximately(4f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Llife / Lstam smoothing
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SmoothsLlife_TowardPlife_ByOnePerFrame()
    {
        // llife += max(-1, min(1, plife-llife))
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Life = F32.FromInt(100),
            Llife = F32.Zero
        };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        _ = player.Llife.Float.Should().BeApproximately(1f, 0.01f);
    }

    [Fact]
    public void Update_SmoothsLstam_TowardPstam_ByOnePerFrame()
    {
        // lstam += max(-1, min(1, pstam-lstam))
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Stam = F32.FromInt(100),
            Lstam = F32.Zero
        };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        _ = player.Lstam.Float.Should().BeApproximately(1f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Stamina regeneration
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_IncrementsStamina_WhenBelowMax()
    {
        // if pstam<100: pstam = min(100, pstam+1)
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Stam = F32.FromInt(50)
        };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        _ = player.Stam.Float.Should().BeApproximately(51f, 0.01f);
    }

    [Fact]
    public void Update_DoesNotExceedMaxStamina()
    {
        // pstam=100 -> stays 100, no regen
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Stam = F32.FromInt(100)
        };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        _ = player.Stam.Float.Should().BeApproximately(100f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Banim countdown
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_DecrementsBanim_WhenAboveZero()
    {
        // if banim>0: banim -= 1
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Banim = F32.FromInt(5)
        };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        _ = player.Banim.Float.Should().BeApproximately(4f, 0.01f);
    }

    [Fact]
    public void Update_DoesNotChangeBanim_WhenAtZero()
    {
        // banim=0 -> stays 0; the btn(5) block only triggers with canAct=true and btn(5) pressed
        FakeInputManager fakeInput = new();
        using GameOrchestrator orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Banim = F32.Zero
        };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        _ = player.Banim.Float.Should().BeApproximately(0f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Game time advance
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesGameTime_ByOneThirtieth()
    {
        // time += 1/30
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        Level level = new(0, 0, 8, 8, LevelTheme.Surface)
        {
            Time = F32.Zero
        };

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        _ = level.Time.Float.Should().BeApproximately(1f / 30f, 0.005f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Last-button-state update (block5)
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ClearsBlock5_WhenBtn5NotHeld()
    {
        // if not btn(5) then block5=false
        FakeInputManager fakeInput = new();
        using GameOrchestrator orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Block5 = true
        };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        _ = player.Block5.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Inventory open (Btnp 4)
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SetsCurMenu_ToMenuInvent_WhenBtnp4()
    {
        // Btnp(PicoButton.Secondary) opens inventory menu
        FakeInputManager fakeInput = new();
        fakeInput.PressOnce(5);
        using GameOrchestrator orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Life = F32.FromInt(10)
        };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        _ = player.CurMenu.Should().BeOfType<InventoryMenu>()
            .Which.List.Should().BeSameAs(player.Invent);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Death check
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SetsCurMenu_ToDeathMenu_WhenPlifeZero()
    {
        // if plife<=0: curmenu=deathmenu
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Life = F32.Zero
        };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: false, nearEnemies: []);

        _ = player.CurMenu.Should().BeSameAs(PcraftData.DeathMenu);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Attack -- Btn(5) with near enemies
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ReducesEnemyLife_WhenBtn5PressedWithNearEnemies()
    {
        // banim==0 and pstam>0 and canact and nearenemies>0 -> e.life -= pow/#nearenemies (pow=1 by default)
        FakeInputManager fakeInput = new();
        fakeInput.SetBtn(4, true);
        using GameOrchestrator orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Banim = F32.Zero,
            Stam = F32.FromInt(60)
        };
        ZombieEntity zombie = new(F32.Zero, F32.Zero) { Life = F32.FromInt(10) };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);
        level.Ene.Add(zombie);
        List<CharacterEntity> nearEnemies = [zombie];

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: true, nearEnemies: nearEnemies);

        _ = zombie.Life.Float.Should().BeLessThan(10f);
    }

    [Fact]
    public void Update_RemovesEnemy_FromEnemiesList_WhenKilledByAttack()
    {
        // e.life <= 0 -> del(enemies, e)
        FakeInputManager fakeInput = new();
        fakeInput.SetBtn(4, true);
        using GameOrchestrator orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Banim = F32.Zero,
            Stam = F32.FromInt(60)
        };
        // Life=0.5 -- one unarmed hit (pow=1) kills it
        ZombieEntity zombie = new(F32.Zero, F32.Zero) { Life = F32.FromFloat(0.5f) };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);
        level.Ene.Add(zombie);
        List<CharacterEntity> nearEnemies = [zombie];

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: true, nearEnemies: nearEnemies);

        _ = level.Ene.Should().NotContain(zombie);
    }

    [Fact]
    public void Update_DoesNotAttack_WhenBanimAboveZero()
    {
        // banim>0 -> attack block skipped; enemy life unchanged
        FakeInputManager fakeInput = new();
        fakeInput.SetBtn(4, true);
        using GameOrchestrator orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero)
        {
            Banim = F32.FromInt(3),
            Stam = F32.FromInt(60)
        };
        ZombieEntity zombie = new(F32.Zero, F32.Zero) { Life = F32.FromInt(10) };
        Level level = new(0, 0, 8, 8, LevelTheme.Surface);
        level.Ene.Add(zombie);
        List<CharacterEntity> nearEnemies = [zombie];

        player.CurrentLevel = level;
        PlayerActionUpdater.Update(player, F32.Zero, F32.Zero, canAct: true, nearEnemies: nearEnemies);

        _ = zombie.Life.Float.Should().BeApproximately(10f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
}
