using CSharpCraft.Pcraft;
using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Update;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Update;

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
    #region Player entity sync
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SyncsPlayerEntityPosition_ToPlxPly()
    {
        // if e.type == player then e.x=plx; e.y=ply
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState
        {
            Plx = F32.FromInt(80),
            Ply = F32.FromInt(90)
        };
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        state.Enemies = [player];

        EnemyUpdater.Update(state);

        player.X.Float.Should().BeApproximately(80f, 0.01f);
        player.Y.Float.Should().BeApproximately(90f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region NearEnemies population
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ClearsNearEnemies_AtStartOfFrame()
    {
        // nearenemies={} at top of loop
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState { Plx = F32.Zero, Ply = F32.Zero };
        var stale = new ZombieEntity(F32.FromInt(200), F32.FromInt(200));
        state.NearEnemies.Add(stale);
        state.Enemies = [new PlayerEntity(F32.Zero, F32.Zero)];

        EnemyUpdater.Update(state);

        state.NearEnemies.Should().NotContain(stale);
    }

    [Fact]
    public void Update_AddsZombieToNearEnemies_WhenWithinAttackRadius()
    {
        // disten = getlen(e.x-plx - ebx*8, e.y-ply - eby*8) < 10 → add to nearenemies
        // With prot=0: ebx=cos(0)=1, eby=sin(0)=0
        // Place zombie at (plx+8+4, ply) → disten = getlen(4, 0) = 4 < 10
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState
        {
            Plx  = F32.Zero,
            Ply  = F32.Zero,
            Prot = F32.Zero
        };
        var zombie = new ZombieEntity(F32.FromInt(12), F32.Zero)
        {
            Life = F32.FromInt(10),
            Dtim = F32.FromInt(10), // non-zero so AI step doesn't reset
            Step = 0               // wait — not patrolling, just stable
        };
        state.Enemies = [new PlayerEntity(F32.Zero, F32.Zero), zombie];

        EnemyUpdater.Update(state);

        state.NearEnemies.Should().Contain(zombie);
    }

    [Fact]
    public void Update_DoesNotAddZombie_WhenOutsideViewport()
    {
        // isin(e, 100) is false → skip entirely, not added to nearenemies
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState { Plx = F32.Zero, Ply = F32.Zero, Clx = F32.Zero, Cly = F32.Zero };
        var distantZombie = new ZombieEntity(F32.FromInt(500), F32.Zero) { Life = F32.FromInt(10) };
        state.Enemies = [new PlayerEntity(F32.Zero, F32.Zero), distantZombie];

        EnemyUpdater.Update(state);

        state.NearEnemies.Should().NotContain(distantZombie);
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
        var state = new WorldState { Plx = F32.FromInt(30), Ply = F32.Zero };
        var zombie = new ZombieEntity(F32.FromInt(30), F32.Zero)
        {
            Life = F32.FromInt(10),
            Dtim = F32.Zero,  // expired
            Step = 0          // wait
        };
        state.Enemies = [new PlayerEntity(F32.FromInt(30), F32.Zero), zombie];

        EnemyUpdater.Update(state);

        zombie.Step.Should().Be(1); // enstep_walk
        zombie.Dtim.Float.Should().BeGreaterThan(0f);
    }

    [Fact]
    public void Update_TransitionsFromWalk_ToWait_WhenDtimZero()
    {
        // e.dtim<=0 and step==walk → step=wait, dx=dy=0, dtim=30+rnd(60)
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState { Plx = F32.FromInt(30), Ply = F32.Zero };
        var zombie = new ZombieEntity(F32.FromInt(30), F32.Zero)
        {
            Life = F32.FromInt(10),
            Dtim = F32.Zero,
            Step = 1          // walk
        };
        state.Enemies = [new PlayerEntity(F32.FromInt(30), F32.Zero), zombie];

        EnemyUpdater.Update(state);

        zombie.Step.Should().Be(0); // enstep_wait
        zombie.Dx.Float.Should().BeApproximately(0f, 0.01f);
        zombie.Dy.Float.Should().BeApproximately(0f, 0.01f);
    }

    [Fact]
    public void Update_TransitionsToChase_WhenZombieCloseToPlayer()
    {
        // distp<40 and step!=chase → step=enstep_chase
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState { Plx = F32.FromInt(30), Ply = F32.Zero };
        var zombie = new ZombieEntity(F32.FromInt(50), F32.Zero) // dist=20 < 40
        {
            Life = F32.FromInt(10),
            Dtim = F32.FromInt(5), // non-zero so we enter the else branch
            Step = 0               // wait — becomes chase
        };
        state.Enemies = [new PlayerEntity(F32.FromInt(30), F32.Zero), zombie];

        EnemyUpdater.Update(state);

        zombie.Step.Should().Be(2); // enstep_chase
    }

    // --------------------------------------------------------------------------
    #endregion
}
