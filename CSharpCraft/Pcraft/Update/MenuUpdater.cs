using CSharpCraft.Pcraft.Data;
using PSharp8;

namespace CSharpCraft.Pcraft.Update;

internal static class MenuUpdater
{
    /// <summary>
    /// Handles menu input for one frame.
    /// Returns true when a menu consumed the frame (caller should skip game logic).
    /// </summary>
    internal static bool Update(WorldState state, PcraftGame game)
    {
        if (state.CurMenu is null) return false;

        state.CurMenu.Update(state, game);
        state.Lb4 = Pico8.Btn(4);
        state.Lb5 = Pico8.Btn(5);
        return true;
    }
}
