using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Update;

internal static class MenuUpdater
{
    /// Returns (consumed, needsReset). consumed=true means a menu handled the frame.
    internal static (bool consumed, bool needsReset) Update(PlayerEntity player)
    {
        if (player.CurMenu is null) return (false, false);

        bool needsReset = player.CurMenu.Update(player);
        player.Lb4 = Pico8.Btn(4);
        player.Lb5 = Pico8.Btn(5);
        return (true, needsReset);
    }
}
