using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftBase.Update;

internal static class PcraftUpdate
{
    internal static void Update(PlayerEntity player)
    {
        var session = PcraftSession.Current;
        var level   = player.CurrentLevel!;

        // ── Level switch (top of frame — matches Lua _update() order) ─────────
        if (player.SwitchLevel)
        {
            player.CurrentLevel = (player.CurrentLevel == session.Cave)
                ? session.Island!
                : session.Cave!;
            level = player.CurrentLevel;
            PcraftServices.SetLevel(level, player);
            PcraftServices.FillEne(level, player);
            player.SwitchLevel    = false;
            player.CanSwitchLevel = false;
            Pico8.Music(player.CurrentLevel == session.Cave ? 2 : 1);
        }

        // ── Menu guard ────────────────────────────────────────────────────────
        var (consumed, needsReset) = PcraftServices.UpdateMenu(player);
        if (consumed)
        {
            if (needsReset)
            {
                PcraftServices.ResetLevel(player);
                Pico8.Music(1);
            }
            return;
        }

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
        var playTile = PcraftServices.GetTile(player.X, player.Y, level);
        if (playTile.Type != player.LastGround && playTile.Type == PcraftData.TileWater)
            Pico8.Sfx(11);
        var s = (playTile.Type == PcraftData.TileWater || player.Stam <= F32.Zero)
            ? F32.One
            : F32.FromInt(2);
        if (playTile.Type == PcraftData.TileHole)
            player.SwitchLevel = player.SwitchLevel || player.CanSwitchLevel;
        else
            player.CanSwitchLevel = true;

        player.LastGround = playTile.Type;

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
        PcraftServices.UpdatePlayer(player, fdx, fdy, canAct, nearEnemies);
        PcraftServices.UpdateCamera(player, fdx, fdy);
    }
}
