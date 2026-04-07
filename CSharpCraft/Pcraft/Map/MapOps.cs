using CSharpCraft.Pcraft.Data;

namespace CSharpCraft.Pcraft.Map;

internal static class MapOps
{
    internal static (int i, int j) GetMCoord(F32 x, F32 y)
        => (F32.FloorToInt(x / F32.FromInt(16)),
            F32.FloorToInt(y / F32.FromInt(16)));

    internal static GroundType GetDirectGr(int i, int j, WorldState state)
    {
        if (OutOfBounds(i, j, state))
            return PcraftData.GrWater;

        int id = PSharp8.Pico8.Mget(i + state.LevelX, j);
        return PcraftData.Grounds[id];
    }

    internal static GroundType GetGr(F32 x, F32 y, WorldState state)
    {
        var (i, j) = GetMCoord(x, y);
        return GetDirectGr(i, j, state);
    }

    internal static void SetGr(F32 x, F32 y, GroundType v, WorldState state)
    {
        var (i, j) = GetMCoord(x, y);
        if (OutOfBounds(i, j, state)) return;
        PSharp8.Pico8.Mset(i + state.LevelX, j, v.Id);
    }

    internal static F32 DirGetData(int i, int j, F32 def, WorldState state)
    {
        int key = i + j * state.LevelSx;
        if (!state.Data.ContainsKey(key))
            state.Data[key] = def;
        return state.Data[key];
    }

    internal static void DirSetData(int i, int j, F32 v, WorldState state)
        => state.Data[i + j * state.LevelSx] = v;

    internal static F32 GetData(F32 x, F32 y, F32 def, WorldState state)
    {
        var (i, j) = GetMCoord(x, y);
        if (OutOfBounds(i, j, state))
            return def;
        return DirGetData(i, j, def, state);
    }

    internal static void SetData(F32 x, F32 y, F32 v, WorldState state)
    {
        var (i, j) = GetMCoord(x, y);
        if (OutOfBounds(i, j, state)) return;
        DirSetData(i, j, v, state);
    }

    internal static void ClearData(F32 x, F32 y, WorldState state)
    {
        var (i, j) = GetMCoord(x, y);
        if (OutOfBounds(i, j, state)) return;
        state.Data.Remove(i + j * state.LevelSx);
    }

    private static bool OutOfBounds(int i, int j, WorldState state)
        => i < 0 || j < 0 || i >= state.LevelSx || j >= state.LevelSy;

    internal static bool IsFree(F32 x, F32 y, WorldState state)
    {
        var gr = GetGr(x, y, state);
        return !(gr.IsTree || gr == PcraftData.GrRock);
    }

    internal static bool IsFreeEnem(F32 x, F32 y, WorldState state)
    {
        var gr = GetGr(x, y, state);
        return !(gr.IsTree || gr == PcraftData.GrRock || gr == PcraftData.GrWater);
    }

    internal static bool IsCool(F32 x, F32 y, WorldState state)
        => !IsFree(x, y, state);
}
