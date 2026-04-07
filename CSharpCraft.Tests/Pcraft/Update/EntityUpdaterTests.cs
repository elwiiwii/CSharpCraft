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
public sealed class EntityUpdaterTests(FnaFixture fixture)
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
    #region Physics — velocity and friction
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesEntityPosition_ByVelocity()
    {
        // e.x += e.vx; e.y += e.vy
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var e = new ItemEntity(PcraftData.Wood, x: F32.FromInt(50), y: F32.FromInt(50),
            vx: F32.FromInt(2), vy: F32.FromInt(3));
        state.Enemies = [new PlayerEntity(F32.FromInt(100), F32.FromInt(100))];
        state.Entities.Add(e);

        EntityUpdater.Update(state, F32.Zero, F32.Zero);

        e.X.Float.Should().BeApproximately(52f, 0.01f);
        e.Y.Float.Should().BeApproximately(53f, 0.01f);
    }

    [Fact]
    public void Update_DampensVelocity_ByFrictionFactor()
    {
        // e.vx *= 0.95; e.vy *= 0.95
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var e = new ItemEntity(PcraftData.Wood, x: F32.FromInt(50), y: F32.FromInt(50),
            vx: F32.FromInt(4), vy: F32.FromInt(4));
        state.Enemies = [new PlayerEntity(F32.FromInt(100), F32.FromInt(100))];
        state.Entities.Add(e);

        EntityUpdater.Update(state, F32.Zero, F32.Zero);

        e.Vx.Float.Should().BeApproximately(3.8f, 0.02f);
        e.Vy.Float.Should().BeApproximately(3.8f, 0.02f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Timer — expiry and countdown
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_RemovesEntity_WhenTimerExpiresBelow1()
    {
        // if e.timer and e.timer<1 then del(entities,e)
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var e = new ItemEntity(PcraftData.Wood, x: F32.FromInt(10), y: F32.FromInt(10))
        {
            Timer = F32.FromFloat(0.5f)  // < 1 → remove immediately
        };
        state.Enemies = [new PlayerEntity(F32.FromInt(100), F32.FromInt(100))];
        state.Entities.Add(e);

        EntityUpdater.Update(state, F32.Zero, F32.Zero);

        state.Entities.Should().NotContain(e);
    }

    [Fact]
    public void Update_DecrementTimer_WhenAbove1()
    {
        // if(e.timer) e.timer-=1
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var e = new ItemEntity(PcraftData.Wood, x: F32.FromInt(10), y: F32.FromInt(10))
        {
            Timer = F32.FromInt(10)
        };
        state.Enemies = [new PlayerEntity(F32.FromInt(100), F32.FromInt(100))];
        state.Entities.Add(e);

        EntityUpdater.Update(state, F32.Zero, F32.Zero);

        e.Timer!.Value.Float.Should().BeApproximately(9f, 0.01f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Item pickup
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AddsItemToInventory_WhenPickupEntityNearPlayer()
    {
        // GiveItem entity within dist<5 and timer<115 → additeminlist(invent, ...) + remove entity
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var player = new PlayerEntity(F32.FromInt(50), F32.FromInt(50));
        state.Enemies = [player];
        state.Plx = F32.FromInt(50);
        state.Ply = F32.FromInt(50);
        // place pickup at same position (dist=0, well within 5)
        var e = new ItemEntity(PcraftData.Wood, x: F32.FromInt(50), y: F32.FromInt(50))
        {
            GiveItem = PcraftData.Wood,
            Timer    = F32.FromInt(50)  // < 115
        };
        state.Entities.Add(e);

        EntityUpdater.Update(state, F32.Zero, F32.Zero);

        state.Invent.Should().ContainSingle(it => it.Type == PcraftData.Wood);
        state.Entities.Should().NotContain(e);
    }

    [Fact]
    public void Update_DoesNotPickUp_WhenPickupEntityFarFromPlayer()
    {
        // dist >= 5 → item stays on ground
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.Enemies = [new PlayerEntity(F32.Zero, F32.Zero)];
        state.Plx = F32.Zero;
        state.Ply = F32.Zero;
        var e = new ItemEntity(PcraftData.Wood, x: F32.FromInt(100), y: F32.FromInt(100))
        {
            GiveItem = PcraftData.Wood,
            Timer    = F32.FromInt(50)
        };
        state.Entities.Add(e);

        EntityUpdater.Update(state, F32.Zero, F32.Zero);

        state.Invent.Should().BeEmpty();
        state.Entities.Should().Contain(e);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region canAct return value
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ReturnsCanActTrue_WhenNoInteraction()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.Enemies = [new PlayerEntity(F32.Zero, F32.Zero)];

        var (_, _, canAct) = EntityUpdater.Update(state, F32.Zero, F32.Zero);

        canAct.Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
}
