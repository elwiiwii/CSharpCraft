using CSharpCraft.Pcraft.Data;

namespace CSharpCraft.Pcraft.Draw;

internal static class PcraftDraw
{
    internal static void Draw(WorldState state, PcraftGame game)
    {
        bool ownedFrame = !Pico8.IsDrawing;
        if (ownedFrame) Pico8.BeginFrame();
        try { DrawImpl(state, game); }
        finally { if (ownedFrame) Pico8.EndFrame(); }
    }

    private static void DrawImpl(WorldState state, PcraftGame game)
    {
        Pico8.Cls();
        Pico8.Camera(state.Clx - F32.FromInt(64), state.Cly - F32.FromInt(64));
        BackDrawer.DrawBack(state);
        EntDrawer.DrawEnt(state);
        EnemiesDrawer.DrawEnemies(state);
        Pico8.Camera();
        HudDrawer.DrawHud(state);
        state.CurMenu?.Draw(state);
    }
}
