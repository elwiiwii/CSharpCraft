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
        // ── Splash branch: curmenu.spr != 0 ─────────────────────────
        if (state.CurMenu is { Spr: not 0 } splash)
        {
            Pico8.Camera();
            Pico8.Palt(0, false);
            Pico8.Rectfill(0, 0, 128, 46, 12);
            Pico8.Rectfill(0, 46, 128, 128, 1);
            Pico8.Spr(splash.Spr, 32, 14, 8, 8);
            PrintC(splash.Text ?? "", 64, 80, 6);
            PrintC(splash.Text2 ?? "", 64, 90, 6);
            int tc = 6 + F32.FloorToInt(state.Time % 2);
            PrintC("press button 1", 64, 112, tc);
            state.Time += F32.FromFloat(0.1f);
            return;
        }

        // ── Game branch ──────────────────────────────────────────────
        Pico8.Cls();
        Pico8.Camera(state.Clx - F32.FromInt(64), state.Cly - F32.FromInt(64));
        BackDrawer.DrawBack(state);
        EntDrawer.DrawEnt(state);
        EnemiesDrawer.DrawEnemies(state);
        Pico8.Camera();
        HudDrawer.DrawHud(state);
        MenuOverlayDrawer.DrawMenuOverlay(state, game);
    }

    private static void PrintC(string t, double x, double y, double c)
        => Pico8.Print(t, x - t.Length * 2, y, c);
}
