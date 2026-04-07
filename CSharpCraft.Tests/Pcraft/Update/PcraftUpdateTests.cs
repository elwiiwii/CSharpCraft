using CSharpCraft.Pcraft;
using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Update;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Input;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Update;

[Collection("Fna")]
public sealed class PcraftUpdateTests(FnaFixture fixture)
{
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

    private GameOrchestrator BuildOrchestrator(IInputManager? input = null)
        => new(
            ".",
            ".",
            ".",
            new NullScene(),
            fixture.GraphicsDevice,
            fixture.GraphicsDeviceManager,
            fixture.Window,
            inputManager: input);

    // --------------------------------------------------------------------------
    #region Menu guard — early exit when CurMenu is active
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SkipsGameLogic_WhenMenuActive()
    {
        // if curmenu → MenuUpdater returns true → PlayerActionUpdater never runs
        // Verifiable: state.Time does NOT advance (time advance is in PlayerActionUpdater)
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState
        {
            CurMenu = PcraftData.DeathMenu,   // splash menu: spr=128 → MenuUpdater returns true
            Plife   = F32.FromInt(100),
            Time    = F32.Zero
        };

        PcraftUpdate.Update(state, new PcraftGame(), new Random(0));

        state.Time.Should().Be(F32.Zero);
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
        var state = new WorldState
        {
            Plife = F32.FromInt(100),
            Time  = F32.Zero
        };

        PcraftUpdate.Update(state, new PcraftGame(), new Random(0));

        state.Time.Float.Should().BeApproximately(1f / 30f, 0.005f);
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
        var state = new WorldState { Plife = F32.FromInt(100) };

        PcraftUpdate.Update(state, new PcraftGame(), new Random(0));

        state.Lrot.Float.Should().BeApproximately(0.5f, 0.01f);
    }

    [Fact]
    public void Update_IncrementsPanim_WhenMoving()
    {
        // btn(1) pressed → dx=1, abs(dx)>0 → panim += 1/33
        var fakeInput = new FakeInputManager();
        fakeInput.SetBtn(1, true);
        using var orch = BuildOrchestrator(fakeInput);
        Pico8.Initialize(orch);
        var state = new WorldState { Plife = F32.FromInt(100), Panim = F32.Zero };

        PcraftUpdate.Update(state, new PcraftGame(), new Random(0));

        state.Panim.Float.Should().BeApproximately(1f / 33f, 0.003f);
    }

    [Fact]
    public void Update_ResetsPanim_WhenNotMoving()
    {
        // no direction buttons → dx=dy=0 → panim = 0
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState { Plife = F32.FromInt(100), Panim = F32.FromInt(5) };

        PcraftUpdate.Update(state, new PcraftGame(), new Random(0));

        state.Panim.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
}
