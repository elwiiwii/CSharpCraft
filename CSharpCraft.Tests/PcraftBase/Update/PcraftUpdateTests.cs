using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
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
public sealed class PcraftUpdateTests(FnaFixture fixture) : IDisposable
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
            "pcraft_og_11", "pcraft_og_12", "pcraft_og_13", "pcraft_og_14", "pcraft_og_15",
            "pcraft_og_16", "pcraft_og_17", "pcraft_og_18", "pcraft_og_19", "pcraft_og_21");

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
    #region Menu guard — early exit when CurMenu is active
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SkipsGameLogic_WhenMenuActive()
    {
        // if curmenu → MenuUpdater returns true → PlayerActionUpdater never runs
        // Verifiable: level.Time does NOT advance (time advance is in PlayerActionUpdater)
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Life = F32.FromInt(100);
        player.CurMenu = PcraftData.DeathMenu;   // splash menu: spr=128 → MenuUpdater returns true
        var level = new Level(0, 0, 64, 64, false);
        bool switchLevel = false, canSwitchLevel = false;

        PcraftUpdate.Update(player, level, new PcraftGame(), ref switchLevel, ref canSwitchLevel);

        level.Time.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Full pipeline — no menu active
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesTime_WhenNoMenuActive()
    {
        // CurMenu=null → full pipeline → PlayerActionUpdater advances time by 1/30
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Life = F32.FromInt(100);
        var level = new Level(0, 0, 64, 64, false);
        bool switchLevel = false, canSwitchLevel = false;

        PcraftUpdate.Update(player, level, new PcraftGame(), ref switchLevel, ref canSwitchLevel);

        level.Time.Float.Should().BeApproximately(1f / 30f, 0.005f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region dx/dy input computation — lrot and panim
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SetsLrot_WhenLeftButtonPressed()
    {
        // btn(0) pressed → dx=-1; GetInvLen normalises; GetRot(-1,0) → lrot ≈ 0.5
        var fakeInput = new FakeInputManager();
        fakeInput.SetBtn(0, true);
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Life = F32.FromInt(100);
        var level = new Level(0, 0, 64, 64, false);
        bool switchLevel = false, canSwitchLevel = false;

        PcraftUpdate.Update(player, level, new PcraftGame(), ref switchLevel, ref canSwitchLevel);

        player.Lrot.Float.Should().BeApproximately(0.5f, 0.01f);
    }

    [Fact]
    public void Update_IncrementsPanim_WhenMoving()
    {
        // btn(1) pressed → dx=1, abs(dx)>0 → panim += 1/33
        var fakeInput = new FakeInputManager();
        fakeInput.SetBtn(1, true);
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Life = F32.FromInt(100);
        var level = new Level(0, 0, 64, 64, false);
        bool switchLevel = false, canSwitchLevel = false;

        PcraftUpdate.Update(player, level, new PcraftGame(), ref switchLevel, ref canSwitchLevel);

        player.Panim.Float.Should().BeApproximately(1f / 33f, 0.003f);
    }

    [Fact]
    public void Update_ResetsPanim_WhenNotMoving()
    {
        // no direction buttons → dx=dy=0 → panim = 0
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Life = F32.FromInt(100);
        player.Panim = F32.FromInt(5);
        var level = new Level(0, 0, 64, 64, false);
        bool switchLevel = false, canSwitchLevel = false;

        PcraftUpdate.Update(player, level, new PcraftGame(), ref switchLevel, ref canSwitchLevel);

        player.Panim.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
}
