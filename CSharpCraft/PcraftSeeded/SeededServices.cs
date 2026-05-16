using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftFilter;

namespace CSharpCraft.PcraftSeeded;

internal class SeededServices : PcraftServices
{
    protected readonly long _seed;
    private readonly long _nonGenSeed;
    private readonly Dictionary<string, int> _harvestCounts = new();
    internal bool UseRelativeFacing { get; init; }
    private readonly Dictionary<string, IDropFilter> _dropFilters = new();

    internal SeededServices(long worldSeed, long? nonGenSeed = null)
    {
        _seed = worldSeed;
        _nonGenSeed = nonGenSeed ?? worldSeed;
        SeededLevelManager.Initialize(worldSeed);
        SetServices(this);
    }

    protected override Level OnCreateLevel(int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
        => SeededLevelManager.CreateLevel(x, y, sx, sy, theme, player);

    protected override F32[][] OnInitRndWat()
        => SeededMapGenerator.InitRndWat(_seed);

    protected override void OnAddItem(ItemDef mat, int minCount, int maxCount, F32 hitX, F32 hitY, List<Entity> entities,
        F32? playerFacing = null, double dropChance = 1.0)
        => SeededDropManager.AddItem(mat, minCount, maxCount, hitX, hitY, entities,
            _nonGenSeed, _harvestCounts, _dropFilters, UseRelativeFacing, playerFacing, dropChance);
}
