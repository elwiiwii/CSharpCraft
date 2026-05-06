using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Update;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Update;

[Collection("Fna")]
public sealed class EnemyUpdaterTests(FnaFixture fixture) : IDisposable
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

    private GameOrchestrator BuildOrchestrator()
    {
        _musicDir = FnaFixture.CreateTempMusicDirectory(
            "s0.ogg", "s1.ogg", "s2.ogg", "s3.ogg", "s4.ogg");
        _sfxDir = FnaFixture.CreateTempSfxDirectory(
            "pcraft_og_12", "pcraft_og_13", "pcraft_og_14", "pcraft_og_15",
            "pcraft_og_16", "pcraft_og_17", "pcraft_og_18", "pcraft_og_19", "pcraft_og_21");

        var scene = new NullScene();
        var orch = new GameOrchestrator(
            _musicDir.Path,
            _sfxDir,
            ".",
            scene,
            fixture.GraphicsDevice,
            fixture.GraphicsDeviceManager,
            fixture.Window);

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
    #region NearEnemies population
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ClearsNearEnemies_AtStartOfFrame()
    {
        // nearenemies={} at top of loop
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        var level = new Level(0, 0, 64, 64, LevelTheme.Surface);
        var stale = new ZombieEntity(F32.FromInt(200), F32.FromInt(200));

        player.CurrentLevel = level;
        var nearEnemies = EnemyUpdater.Update(player, level);

        nearEnemies.Should().NotContain(stale);
    }

    [Fact]
    public void Update_AddsZombieToNearEnemies_WhenWithinAttackRadius()
    {
        // disten = getlen(e.x-plx - ebx*8, e.y-ply - eby*8) < 10 → add to nearenemies
        // With prot=0: ebx=cos(0)=1, eby=sin(0)=0
        // Place zombie at (plx+8+4, ply) → disten = getlen(4, 0) = 4 < 10
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        // player.Prot defaults to F32.Zero
        var level = new Level(0, 0, 64, 64, LevelTheme.Surface);
        var zombie = new ZombieEntity(F32.FromInt(12), F32.Zero)
        {
            Life = F32.FromInt(10),
            Dtim = F32.FromInt(10), // non-zero so AI step doesn't reset
            Step = EnStep.Wait
        };
        level.Ene.Add(zombie);

        player.CurrentLevel = level;
        var nearEnemies = EnemyUpdater.Update(player, level);

        nearEnemies.Should().Contain(zombie);
    }

    [Fact]
    public void Update_DoesNotAddZombie_WhenOutsideViewport()
    {
        // isin(e, 100) is false → skip entirely, not added to nearenemies
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        player.Camera.Clx = F32.Zero;
        player.Camera.Cly = F32.Zero;
        var level = new Level(0, 0, 64, 64, LevelTheme.Surface);
        var distantZombie = new ZombieEntity(F32.FromInt(500), F32.Zero) { Life = F32.FromInt(10) };
        level.Ene.Add(distantZombie);

        player.CurrentLevel = level;
        var nearEnemies = EnemyUpdater.Update(player, level);

        nearEnemies.Should().NotContain(distantZombie);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region AI step transitions
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_TransitionsFromWait_ToWalk_WhenDtimZero()
    {
        // e.dtim<=0 and step==wait → step=walk, dtim=30+rnd(60)
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.FromInt(30), F32.Zero);
        var level = new Level(0, 0, 64, 64, LevelTheme.Surface);
        var zombie = new ZombieEntity(F32.FromInt(30), F32.Zero)
        {
            Life = F32.FromInt(10),
            Dtim = F32.Zero,  // expired
            Step = EnStep.Wait
        };
        level.Ene.Add(zombie);

        player.CurrentLevel = level;
        EnemyUpdater.Update(player, level);

        zombie.Step.Should().Be(EnStep.Walk);
        zombie.Dtim.Float.Should().BeGreaterThan(0f);
    }

    [Fact]
    public void Update_TransitionsFromWalk_ToWait_WhenDtimZero()
    {
        // e.dtim<=0 and step==walk → step=wait, dx=dy=0, dtim=30+rnd(60)
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.FromInt(30), F32.Zero);
        var level = new Level(0, 0, 64, 64, LevelTheme.Surface);
        var zombie = new ZombieEntity(F32.FromInt(30), F32.Zero)
        {
            Life = F32.FromInt(10),
            Dtim = F32.Zero,
            Step = EnStep.Walk
        };
        level.Ene.Add(zombie);

        player.CurrentLevel = level;
        EnemyUpdater.Update(player, level);

        zombie.Step.Should().Be(EnStep.Wait);
        zombie.Dx.Float.Should().BeApproximately(0f, 0.01f);
        zombie.Dy.Float.Should().BeApproximately(0f, 0.01f);
    }

    [Fact]
    public void Update_TransitionsToChase_WhenZombieCloseToPlayer()
    {
        // distp<40 and step!=chase → step=enstep_chase
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var player = new PlayerEntity(F32.FromInt(30), F32.Zero);
        var level = new Level(0, 0, 64, 64, LevelTheme.Surface);
        var zombie = new ZombieEntity(F32.FromInt(50), F32.Zero) // dist=20 < 40
        {
            Life = F32.FromInt(10),
            Dtim = F32.FromInt(5), // non-zero so we enter the else branch
            Step = EnStep.Wait     // becomes chase
        };
        level.Ene.Add(zombie);

        player.CurrentLevel = level;
        EnemyUpdater.Update(player, level);

        zombie.Step.Should().Be(EnStep.Chase);
    }

    // --------------------------------------------------------------------------
    #endregion
}
