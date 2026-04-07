using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Inventory;
using CSharpCraft.Pcraft.Map;
using PSharp8;

namespace CSharpCraft.Pcraft.Update;

internal static class PcraftUpdate
{
    internal static void Update(WorldState state, PcraftGame game, Random rng)
    {
        // ── Menu guard ────────────────────────────────────────────────────────
        if (MenuUpdater.Update(state, game, rng))
            return;

        // ── Curitem validation ────────────────────────────────────────────────
        if (state.CurItem != null && InventoryOps.HowMany(state.Invent, state.CurItem) <= 0)
            state.CurItem = null;

        // ── Input → dx/dy ─────────────────────────────────────────────────────
        var dx = F32.Zero;
        var dy = F32.Zero;

        if (Pico8.Btn(0)) dx -= F32.One;
        if (Pico8.Btn(1)) dx += F32.One;
        if (Pico8.Btn(2)) dy -= F32.One;
        if (Pico8.Btn(3)) dy += F32.One;

        var dl = PcraftMath.GetInvLen(dx, dy);
        dx *= dl;
        dy *= dl;

        if (F32.Abs(dx) > F32.Zero || F32.Abs(dy) > F32.Zero)
        {
            state.Lrot   = PcraftMath.GetRot(dx, dy);
            state.Panim += F32.FromFloat(1f / 33f);
        }
        else
        {
            state.Panim = F32.Zero;
        }

        // ── Speed multiplier (water or no stamina → 1, otherwise 2) ──────────
        var playHit = MapOps.GetGr(state.Plx, state.Ply, state);
        var s = (playHit == PcraftData.GrWater || state.Pstam <= F32.Zero)
            ? F32.One
            : F32.FromInt(2);

        dx *= s;
        dy *= s;

        // ── Sub-updaters ──────────────────────────────────────────────────────
        var (fdx, fdy, canAct) = EntityUpdater.Update(state, dx, dy);
        EnemyUpdater.Update(state);
        PlayerActionUpdater.Update(state, game, fdx, fdy, canAct, rng);
        CameraUpdater.Update(state, fdx, fdy);
    }
}
