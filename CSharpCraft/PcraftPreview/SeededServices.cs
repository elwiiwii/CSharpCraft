using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;

namespace CSharpCraft.PcraftPreview;

internal class SeededServices : PcraftServices
{
    internal SeededServices(long seed)
    {
        SeededLevelManager.Initialize(seed);
        SetServices(this);
    }

    protected override Level OnCreateLevel(int x, int y, int sx, int sy, bool isUnder, WorldState state)
        => SeededLevelManager.CreateLevel(x, y, sx, sy, isUnder, state);
}
