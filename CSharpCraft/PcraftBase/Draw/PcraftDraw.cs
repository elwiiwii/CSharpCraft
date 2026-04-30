using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Draw;

internal static class PcraftDraw
{
    internal static void Draw(PlayerEntity player, Level level)
    {
        bool ownedFrame = !Pico8.IsDrawing;
        if (ownedFrame) Pico8.BeginFrame();
        try { PcraftServices.DrawMain(player, level); }
        finally { if (ownedFrame) Pico8.EndFrame(); }
    }

    internal static void DrawMain(PlayerEntity player, Level level)
    {
        Pico8.Cls();
        Pico8.Camera(player.Camera.Clx - F32.FromInt(64), player.Camera.Cly - F32.FromInt(64));
        PcraftServices.DrawBack(level, player);
        PcraftServices.DrawEnt(level);
        PcraftServices.DrawEnemies(player, level);
        Pico8.Camera();
        PcraftServices.DrawHud(player);
        player.CurMenu?.Draw(player, level);
    }
}
