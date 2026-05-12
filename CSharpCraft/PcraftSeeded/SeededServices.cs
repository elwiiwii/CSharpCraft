using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftSeeded;

internal class SeededServices : PcraftServices
{
    protected readonly long _seed;

    internal SeededServices(long seed)
    {
        _seed = seed;
        SeededLevelManager.Initialize(seed);
        SetServices(this);
    }

    protected override Level OnCreateLevel(int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
        => SeededLevelManager.CreateLevel(x, y, sx, sy, theme, player);

    protected override F32[][] OnInitRndWat()
        => SeededMapGenerator.InitRndWat(_seed);
}
