using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Update;

internal static class CameraUpdater
{
    internal static void Update(PlayerEntity player, F32 dx, F32 dy)
    {
        var camera = player.Camera;
        var m   = F32.FromInt(16);
        var msp = F32.FromInt(4);

        if (F32.Abs(camera.Cmx - player.X) > m)
            camera.Coffx += dx * F32.FromDouble(0.4);
        if (F32.Abs(camera.Cmy - player.Y) > m)
            camera.Coffy += dy * F32.FromDouble(0.4);

        camera.Cmx = F32.Max(player.X - m, camera.Cmx);
        camera.Cmx = F32.Min(player.X + m, camera.Cmx);
        camera.Cmy = F32.Max(player.Y - m, camera.Cmy);
        camera.Cmy = F32.Min(player.Y + m, camera.Cmy);

        camera.Coffx *= F32.FromDouble(0.9);
        camera.Coffy *= F32.FromDouble(0.9);
        camera.Coffx = F32.Clamp(camera.Coffx, -msp, msp);
        camera.Coffy = F32.Clamp(camera.Coffy, -msp, msp);

        camera.Clx += camera.Coffx;
        camera.Cly += camera.Coffy;

        camera.Clx = F32.Max(camera.Cmx - m, camera.Clx);
        camera.Clx = F32.Min(camera.Cmx + m, camera.Clx);
        camera.Cly = F32.Max(camera.Cmy - m, camera.Cly);
        camera.Cly = F32.Min(camera.Cmy + m, camera.Cly);
    }
}
