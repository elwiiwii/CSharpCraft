using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Update;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Update;

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
        PlayerEntity player = new(F32.FromInt(100), F32.Zero);
        player.Camera.Cmx = F32.FromInt(50);

        CameraUpdater.Update(player, F32.Zero, F32.Zero);

        // cmx = max(plx-16, cmx) = max(84, 50) = 84
        _ = player.Camera.Cmx.Float.Should().BeApproximately(84f, Eps);
    }

    [Fact]
    public void Update_ClampsCmxDown_WhenCmxAbovePlayerPlus16()
    {
        PlayerEntity player = new(F32.FromInt(100), F32.Zero);
        player.Camera.Cmx = F32.FromInt(200);

        CameraUpdater.Update(player, F32.Zero, F32.Zero);

        // cmx = min(plx+16, cmx) = min(116, 200) = 116
        _ = player.Camera.Cmx.Float.Should().BeApproximately(116f, Eps);
    }

    [Fact]
    public void Update_DoesNotMoveCmx_WhenWithinPlayerRange()
    {
        PlayerEntity player = new(F32.FromInt(100), F32.Zero);
        player.Camera.Cmx = F32.FromInt(100);

        CameraUpdater.Update(player, F32.Zero, F32.Zero);

        _ = player.Camera.Cmx.Float.Should().BeApproximately(100f, Eps);
    }

    [Fact]
    public void Update_ClampsCmy_WhenCmyOutsidePlayerRange()
    {
        PlayerEntity player = new(F32.Zero, F32.FromInt(100));
        player.Camera.Cmy = F32.FromInt(50);

        CameraUpdater.Update(player, F32.Zero, F32.Zero);

        _ = player.Camera.Cmy.Float.Should().BeApproximately(84f, Eps);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Coffx / Coffy momentum
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AddsDxMomentum_WhenCmxFarFromPlayer()
    {
        // |cmx - plx| = 100 > 16 -> coffx += dx * 0.4, then dampened
        PlayerEntity player = new(F32.Zero, F32.Zero);
        player.Camera.Cmx = F32.FromInt(100);
        player.Camera.Coffx = F32.Zero;

        CameraUpdater.Update(player, F32.FromFloat(1f), F32.Zero);

        // coffx = (0 + 1*0.4) * 0.9 = 0.36
        _ = player.Camera.Coffx.Float.Should().BeApproximately(0.36f, Eps);
    }

    [Fact]
    public void Update_DoesNotAddDxMomentum_WhenCmxNearPlayer()
    {
        // |cmx - plx| = 0 <= 16 -> coffx not boosted, only dampened
        PlayerEntity player = new(F32.FromInt(100), F32.Zero);
        player.Camera.Cmx = F32.FromInt(100);
        player.Camera.Coffx = F32.Zero;

        CameraUpdater.Update(player, F32.FromFloat(1f), F32.Zero);

        // coffx = 0 * 0.9 = 0
        _ = player.Camera.Coffx.Float.Should().BeApproximately(0f, Eps);
    }

    [Fact]
    public void Update_DampensCoffx_EachFrame()
    {
        // Existing momentum decays by 0.9 per frame
        PlayerEntity player = new(F32.FromInt(100), F32.Zero);
        player.Camera.Cmx = F32.FromInt(100);
        player.Camera.Coffx = F32.FromFloat(1f);

        CameraUpdater.Update(player, F32.Zero, F32.Zero);

        _ = player.Camera.Coffx.Float.Should().BeApproximately(0.9f, Eps);
    }

    [Fact]
    public void Update_ClampsCoffx_ToMaxSpeed4()
    {
        // Large preexisting coffx is clamped to msp=4 after dampen
        PlayerEntity player = new(F32.FromInt(100), F32.Zero);
        player.Camera.Cmx = F32.FromInt(100);
        player.Camera.Coffx = F32.FromInt(100);

        CameraUpdater.Update(player, F32.Zero, F32.Zero);

        // 100 * 0.9 = 90 -> clamped to 4
        _ = player.Camera.Coffx.Float.Should().BeApproximately(4f, Eps);
    }

    [Fact]
    public void Update_DampensCoffy_EachFrame()
    {
        PlayerEntity player = new(F32.Zero, F32.FromInt(100));
        player.Camera.Cmy = F32.FromInt(100);
        player.Camera.Coffy = F32.FromFloat(1f);

        CameraUpdater.Update(player, F32.Zero, F32.Zero);

        _ = player.Camera.Coffy.Float.Should().BeApproximately(0.9f, Eps);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Clx / Cly viewport position
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesClx_ByCoffx()
    {
        // clx += coffx (after coffx is dampened)
        // coffx=2 -> dampened to 1.8 -> clx = 64 + 1.8 = 65.8
        // cmx=64 plx=64 so [cmx-16=48, cmx+16=80] -> 65.8 in range
        PlayerEntity player = new(F32.FromInt(64), F32.Zero);
        player.Camera.Cmx = F32.FromInt(64);
        player.Camera.Clx = F32.FromInt(64);
        player.Camera.Coffx = F32.FromInt(2);

        CameraUpdater.Update(player, F32.Zero, F32.Zero);

        _ = player.Camera.Clx.Float.Should().BeApproximately(65.8f, Eps);
    }

    [Fact]
    public void Update_ClampsClx_WhenViewportAheadOfCm()
    {
        // clx is far right of cmx -> clamped to cmx+16
        PlayerEntity player = new(F32.FromInt(100), F32.Zero);
        player.Camera.Cmx = F32.FromInt(100);
        player.Camera.Clx = F32.FromInt(200);
        player.Camera.Coffx = F32.Zero;

        CameraUpdater.Update(player, F32.Zero, F32.Zero);

        // clamp: max(cmx-16, min(cmx+16, 200+0)) = max(84, 116) = 116
        _ = player.Camera.Clx.Float.Should().BeApproximately(116f, Eps);
    }

    // --------------------------------------------------------------------------
    #endregion
}
