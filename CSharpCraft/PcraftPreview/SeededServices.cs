using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftPreview;

internal class SeededServices : PcraftServices
{
    private readonly long _seed;

    internal SeededServices(long seed)
    {
        _seed = seed;
        SeededLevelManager.Initialize(seed);
        SetServices(this);
    }

    protected override Level OnCreateLevel(int x, int y, int sx, int sy, bool isUnder, PlayerEntity player)
        => SeededLevelManager.CreateLevel(x, y, sx, sy, isUnder, player);

    protected override F32[][] OnInitRndWat()
        => SeededMapGenerator.InitRndWat(_seed);
}
