using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftBase.Update;

internal static class PcraftUpdate
{
    internal static void Update(WorldState state, PcraftGame game)
    {
        // ── Menu guard ────────────────────────────────────────────────────────
        if (PcraftServices.UpdateMenu(state, game))
            return;

        if (state.SwitchLevel)
        {
            if (state.CurrentLevel == state.Cave)
                PcraftServices.SetLevel(state.Island!, state);
            else
                PcraftServices.SetLevel(state.Cave!, state);
            
            state.Plx = state.CurrentLevel!.Stx;
            state.Ply = state.CurrentLevel!.Sty;
            PcraftServices.FillEne(state.CurrentLevel!, state);
            state.SwitchLevel = false;
            state.CanSwitchLevel = false;
            Pico8.Music(state.CurrentLevel == state.Cave ? 2 : 1);
        }

        // ── Curitem validation ────────────────────────────────────────────────
        if (state.CurItem is not null && PcraftServices.HowMany(state.Invent, state.CurItem) <= 0)
            state.CurItem = null;

        PcraftServices.UpGround(state);

        // ── Speed multiplier (water or no stamina → 1, otherwise 2) ──────────
        var playHit = PcraftServices.GetGr(state.Plx, state.Ply, state);
        if (playHit != state.LastGround && playHit == PcraftData.GrWater)
            Pico8.Sfx(11);
        var s = (playHit == PcraftData.GrWater || state.Pstam <= F32.Zero)
            ? F32.One
            : F32.FromInt(2);
        if (playHit == PcraftData.GrHole)
            state.SwitchLevel = state.SwitchLevel || state.CanSwitchLevel;
        else
            state.CanSwitchLevel = true;

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
            state.Lrot = PcraftMath.GetRot(dx, dy);
            state.Panim += F32.FromDouble(1.0 / 33.0);
        }
        else
        {
            state.Panim = F32.Zero;
        }

        dx *= s;
        dy *= s;

        //(dx, dy) = CollisionSystem.ReflectCol(
        //    state.Plx, state.Ply, dx, dy,
        //    (x2, y2) => MapOps.IsFree(x2, y2, state),
        //    F32.Zero);

        // ── Sub-updaters ──────────────────────────────────────────────────────
        var (fdx, fdy, canAct) = PcraftServices.UpdateEntities(state, dx, dy);
        PcraftServices.UpdateEnemies(state);
        PcraftServices.UpdatePlayer(state, game, fdx, fdy, canAct);
        PcraftServices.UpdateCamera(state, fdx, fdy);
    }
}
