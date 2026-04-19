using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Map;

internal static class MapOps
{
    internal static (int i, int j) GetMCoord(F32 x, F32 y)
        => (F32.FloorToInt(x / F32.FromInt(16)),
            F32.FloorToInt(y / F32.FromInt(16)));

    internal static GroundType GetDirectGr(int i, int j, Level level)
    {
        if (OutOfBounds(i, j, level))
            return PcraftData.GrWater;

        int id = PSharp8.Pico8.Mget(i + level.X, j);
        return PcraftData.Grounds[id];
    }

    internal static GroundType GetGr(F32 x, F32 y, Level level)
    {
        var (i, j) = GetMCoord(x, y);
        return GetDirectGr(i, j, level);
    }

    internal static void SetGr(F32 x, F32 y, GroundType v, Level level)
    {
        var (i, j) = GetMCoord(x, y);
        if (OutOfBounds(i, j, level)) return;
        PSharp8.Pico8.Mset(i + level.X, j, v.Id);
    }

    internal static F32 DirGetData(int i, int j, F32 def, Level level)
    {
        int key = i + j * level.Sx;
        if (!level.Dat.ContainsKey(key))
            level.Dat[key] = def;
        return level.Dat[key];
    }

    internal static void DirSetData(int i, int j, F32 v, Level level)
        => level.Dat[i + j * level.Sx] = v;

    internal static F32 GetData(F32 x, F32 y, F32 def, Level level)
    {
        var (i, j) = GetMCoord(x, y);
        if (OutOfBounds(i, j, level))
            return def;
        return DirGetData(i, j, def, level);
    }

    internal static void SetData(F32 x, F32 y, F32 v, Level level)
    {
        var (i, j) = GetMCoord(x, y);
        if (OutOfBounds(i, j, level)) return;
        DirSetData(i, j, v, level);
    }

    internal static void ClearData(F32 x, F32 y, Level level)
    {
        var (i, j) = GetMCoord(x, y);
        if (OutOfBounds(i, j, level)) return;
        level.Dat.Remove(i + j * level.Sx);
    }

    internal static bool OutOfBounds(int i, int j, Level level)
        => i < 0 || j < 0 || i >= level.Sx || j >= level.Sy;

    internal static bool IsFree(F32 x, F32 y, Level level)
    {
        var gr = GetGr(x, y, level);
        return !(gr.IsTree || gr == PcraftData.GrRock);
    }

    internal static bool IsFreeEnem(F32 x, F32 y, Level level)
    {
        var gr = GetGr(x, y, level);
        return !(gr.IsTree || gr == PcraftData.GrRock || gr == PcraftData.GrWater);
    }

    internal static bool IsCool(F32 x, F32 y, Level level)
        => !IsFree(x, y, level);
}
