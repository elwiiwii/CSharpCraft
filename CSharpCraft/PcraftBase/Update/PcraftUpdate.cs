using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftBase.Update;

internal static class PcraftUpdate
{
    internal static bool Update(
        PlayerEntity player, Level level,
        ref bool switchLevel, ref bool canSwitchLevel)
    {
        // ── Menu guard ────────────────────────────────────────────────────────
        var (consumed, needsReset) = PcraftServices.UpdateMenu(player);
        if (consumed) return needsReset;

        // ── Curitem validation ────────────────────────────────────────────────
        if (player.CurItem is not null)
        {
            bool stillPresent = player.CurItem is StackableItem curStackable
                ? PcraftServices.HowMany(player.Invent, curStackable) > 0
                : player.Invent.Contains(player.CurItem);
            if (!stillPresent)
                player.CurItem = null;
        }

        PcraftServices.UpGround(level, player);

        // ── Speed multiplier (water or no stamina → 1, otherwise 2) ──────────
        var playHit = PcraftServices.GetGr(player.X, player.Y, level);
        if (playHit != player.LastGround && playHit == PcraftData.GrWater)
            Pico8.Sfx(11);
        var s = (playHit == PcraftData.GrWater || player.Stam <= F32.Zero)
            ? F32.One
            : F32.FromInt(2);
        if (playHit == PcraftData.GrHole)
            switchLevel = switchLevel || canSwitchLevel;
        else
            canSwitchLevel = true;

        player.LastGround = playHit;

        // ── Input → dx/dy ─────────────────────────────────────────────────────
        var dx = F32.Zero;
        var dy = F32.Zero;

        if (Pico8.Btn(0)) dx -= F32.One;
        if (Pico8.Btn(1)) dx += F32.One;
        if (Pico8.Btn(2)) dy -= F32.One;
        if (Pico8.Btn(3)) dy += F32.One;

        var dl = PcraftServices.GetInvLen(dx, dy);
        dx *= dl;
        dy *= dl;

        if (F32.Abs(dx) > F32.Zero || F32.Abs(dy) > F32.Zero)
        {
            player.Lrot  = PcraftServices.GetRot(dx, dy);
            player.Panim += F32.FromDouble(1.0 / 33.0);
        }
        else
        {
            player.Panim = F32.Zero;
        }

        dx *= s;
        dy *= s;

        // ── Sub-updaters ──────────────────────────────────────────────────────
        var (fdx, fdy, canAct) = PcraftServices.UpdateEntities(player, level, dx, dy);
        var nearEnemies = PcraftServices.UpdateEnemies(player, level);
        PcraftServices.UpdatePlayer(player, level, fdx, fdy, canAct, nearEnemies);
        PcraftServices.UpdateCamera(player, fdx, fdy);
        return false;
    }
}
