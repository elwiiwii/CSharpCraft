using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftPreview;

internal sealed class SeededLevelManager : LevelManager
{
    private readonly long _seed;

    internal SeededLevelManager(long seed) => _seed = seed;

    protected override F32[][] CreateRndWat() => SeededMapGenerator.InitRndWat(_seed);

    internal override Level CreateLevel(int x, int y, int sx, int sy, bool isUnder, WorldState state)
    {
        //if (isUnder)
        //    return base.CreateLevel(x, y, sx, sy, isUnder, state);

        var level = new Level(x, y, sx, sy, isUnder);
        SetLevel(level, state);
        var (holeX, holeY) = SeededMapGenerator.CreateMap(state, _seed);
        FillEne(level, state);
        level.Stx = F32.FromInt((holeX - state.LevelX) * 16 + 8);
        level.Sty = F32.FromInt((holeY - state.LevelY) * 16 + 8);
        return level;
    }
}
