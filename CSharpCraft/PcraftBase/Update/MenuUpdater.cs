using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Update;

internal static class MenuUpdater
{
    /// Returns true when a menu consumed the frame (caller should skip game logic).
    internal static bool Update(PlayerEntity player, PcraftGame game)
    {
        if (player.CurMenu is null) return false;

        player.CurMenu.Update(player, game);
        player.Lb4 = Pico8.Btn(4);
        player.Lb5 = Pico8.Btn(5);
        return true;
    }
}
