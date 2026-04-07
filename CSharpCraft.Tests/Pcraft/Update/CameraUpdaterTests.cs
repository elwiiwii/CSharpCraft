using CSharpCraft.Pcraft;
using CSharpCraft.Pcraft.Update;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Update;

public sealed class CameraUpdaterTests
{
    private const float Eps = 0.001f;

    // Lua constants: m = 16, msp = 4, momentum factor = 0.4, dampen = 0.9

    // --------------------------------------------------------------------------
    #region Cmx / Cmy clamping to player range
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ClampsCmxUp_WhenCmxBelowPlayerMinus16()
    {
        var state = new WorldState { Plx = F32.FromInt(100), Cmx = F32.FromInt(50) };

        CameraUpdater.Update(state, F32.Zero, F32.Zero);

        // cmx = max(plx-16, cmx) = max(84, 50) = 84
        state.Cmx.Float.Should().BeApproximately(84f, Eps);
    }

    [Fact]
    public void Update_ClampsCmxDown_WhenCmxAbovePlayerPlus16()
    {
        var state = new WorldState { Plx = F32.FromInt(100), Cmx = F32.FromInt(200) };

        CameraUpdater.Update(state, F32.Zero, F32.Zero);

        // cmx = min(plx+16, cmx) = min(116, 200) = 116
        state.Cmx.Float.Should().BeApproximately(116f, Eps);
    }

    [Fact]
    public void Update_DoesNotMoveCmx_WhenWithinPlayerRange()
    {
        var state = new WorldState { Plx = F32.FromInt(100), Cmx = F32.FromInt(100) };

        CameraUpdater.Update(state, F32.Zero, F32.Zero);

        state.Cmx.Float.Should().BeApproximately(100f, Eps);
    }

    [Fact]
    public void Update_ClampsCmy_WhenCmyOutsidePlayerRange()
    {
        var state = new WorldState { Ply = F32.FromInt(100), Cmy = F32.FromInt(50) };

        CameraUpdater.Update(state, F32.Zero, F32.Zero);

        state.Cmy.Float.Should().BeApproximately(84f, Eps);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Coffx / Coffy momentum
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AddsDxMomentum_WhenCmxFarFromPlayer()
    {
        // |cmx - plx| = 100 > 16 → coffx += dx * 0.4, then dampened
        var state = new WorldState
        {
            Plx = F32.Zero,
            Cmx = F32.FromInt(100),
            Coffx = F32.Zero
        };

        CameraUpdater.Update(state, F32.FromFloat(1f), F32.Zero);

        // coffx = (0 + 1*0.4) * 0.9 = 0.36
        state.Coffx.Float.Should().BeApproximately(0.36f, Eps);
    }

    [Fact]
    public void Update_DoesNotAddDxMomentum_WhenCmxNearPlayer()
    {
        // |cmx - plx| = 0 ≤ 16 → coffx not boosted, only dampened
        var state = new WorldState
        {
            Plx = F32.FromInt(100),
            Cmx = F32.FromInt(100),
            Coffx = F32.Zero
        };

        CameraUpdater.Update(state, F32.FromFloat(1f), F32.Zero);

        // coffx = 0 * 0.9 = 0
        state.Coffx.Float.Should().BeApproximately(0f, Eps);
    }

    [Fact]
    public void Update_DampensCoffx_EachFrame()
    {
        // Existing momentum decays by 0.9 per frame
        var state = new WorldState
        {
            Plx = F32.FromInt(100),
            Cmx = F32.FromInt(100),
            Coffx = F32.FromFloat(1f)
        };

        CameraUpdater.Update(state, F32.Zero, F32.Zero);

        state.Coffx.Float.Should().BeApproximately(0.9f, Eps);
    }

    [Fact]
    public void Update_ClampsCoffx_ToMaxSpeed4()
    {
        // Large preexisting coffx is clamped to msp=4 after dampen
        var state = new WorldState
        {
            Plx = F32.FromInt(100),
            Cmx = F32.FromInt(100),
            Coffx = F32.FromInt(100)
        };

        CameraUpdater.Update(state, F32.Zero, F32.Zero);

        // 100 * 0.9 = 90 → clamped to 4
        state.Coffx.Float.Should().BeApproximately(4f, Eps);
    }

    [Fact]
    public void Update_DampensCoffy_EachFrame()
    {
        var state = new WorldState
        {
            Ply = F32.FromInt(100),
            Cmy = F32.FromInt(100),
            Coffy = F32.FromFloat(1f)
        };

        CameraUpdater.Update(state, F32.Zero, F32.Zero);

        state.Coffy.Float.Should().BeApproximately(0.9f, Eps);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Clx / Cly viewport position
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesClx_ByCoffx()
    {
        // clx += coffx (after coffx is dampened)
        // coffx=2 → dampened to 1.8 → clx = 64 + 1.8 = 65.8
        // cmx=64 plx=64 so [cmx-16=48, cmx+16=80] → 65.8 in range
        var state = new WorldState
        {
            Plx = F32.FromInt(64),
            Cmx = F32.FromInt(64),
            Clx = F32.FromInt(64),
            Coffx = F32.FromInt(2)
        };

        CameraUpdater.Update(state, F32.Zero, F32.Zero);

        state.Clx.Float.Should().BeApproximately(65.8f, Eps);
    }

    [Fact]
    public void Update_ClampsClx_WhenViewportAheadOfCm()
    {
        // clx is far right of cmx → clamped to cmx+16
        var state = new WorldState
        {
            Plx = F32.FromInt(100),
            Cmx = F32.FromInt(100),
            Clx = F32.FromInt(200),
            Coffx = F32.Zero
        };

        CameraUpdater.Update(state, F32.Zero, F32.Zero);

        // clamp: max(cmx-16, min(cmx+16, 200+0)) = max(84, 116) = 116
        state.Clx.Float.Should().BeApproximately(116f, Eps);
    }

    // --------------------------------------------------------------------------
    #endregion
}
