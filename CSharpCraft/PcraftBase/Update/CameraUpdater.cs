using CSharpCraft.PcraftBase;

namespace CSharpCraft.PcraftBase.Update;

internal static class CameraUpdater
{
    internal static void Update(WorldState state, F32 dx, F32 dy)
    {
        var m   = F32.FromInt(16);
        var msp = F32.FromInt(4);

        if (F32.Abs(state.Cmx - state.Plx) > m)
            state.Coffx += dx * F32.FromDouble(0.4);
        if (F32.Abs(state.Cmy - state.Ply) > m)
            state.Coffy += dy * F32.FromDouble(0.4);

        state.Cmx = F32.Max(state.Plx - m, state.Cmx);
        state.Cmx = F32.Min(state.Plx + m, state.Cmx);
        state.Cmy = F32.Max(state.Ply - m, state.Cmy);
        state.Cmy = F32.Min(state.Ply + m, state.Cmy);

        state.Coffx *= F32.FromDouble(0.9);
        state.Coffy *= F32.FromDouble(0.9);
        state.Coffx = F32.Clamp(state.Coffx, -msp, msp);
        state.Coffy = F32.Clamp(state.Coffy, -msp, msp);

        state.Clx += state.Coffx;
        state.Cly += state.Coffy;

        state.Clx = F32.Max(state.Cmx - m, state.Clx);
        state.Clx = F32.Min(state.Cmx + m, state.Clx);
        state.Cly = F32.Max(state.Cmy - m, state.Cly);
        state.Cly = F32.Min(state.Cmy + m, state.Cly);
    }
}
