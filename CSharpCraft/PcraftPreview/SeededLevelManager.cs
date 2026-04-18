using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftPreview;

internal static class SeededLevelManager
{
    private static long _seed;

    internal static void Initialize(long seed) => _seed = seed;

    internal static Level CreateLevel(int x, int y, int sx, int sy, bool isUnder, WorldState state)
    {
        if (isUnder)
            return LevelManager.CreateLevel(x, y, sx, sy, isUnder, state);

        var level = new Level(x, y, sx, sy, isUnder);
        PcraftServices.SetLevel(level, state);
        var (holeX, holeY) = SeededMapGenerator.CreateMap(state, _seed);
        PcraftServices.FillEne(level, state);
        level.Stx = F32.FromInt((holeX - state.LevelX) * 16 + 8);
        level.Sty = F32.FromInt((holeY - state.LevelY) * 16 + 8);
        return level;
    }
}
