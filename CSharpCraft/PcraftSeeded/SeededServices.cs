using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftSeeded;

internal class SeededServices : PcraftServices
{
    protected readonly long _seed;
    protected readonly long _islandSeed;
    protected readonly long _caveSeed;
    private readonly long _nonGenSeed;
    private readonly Dictionary<string, int> _harvestCounts = new();
    internal bool UseRelativeFacing { get; init; }

    private const int CaveSalt = 0x4341_5645;

    internal SeededServices(long masterSeed, long? islandSeed = null, long? caveSeed = null, long? nonGenSeed = null)
    {
        _seed = masterSeed;
        _islandSeed = islandSeed ?? masterSeed;
        _caveSeed = caveSeed ?? (long)SeedMixer.Combine(masterSeed, CaveSalt);
        _nonGenSeed = nonGenSeed ?? masterSeed;
        SetServices(this);
    }

    protected override Level OnCreateLevel(int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
        => SeededLevelManager.CreateLevel(_islandSeed, _caveSeed, x, y, sx, sy, theme, player);

    protected override F32[][] OnInitRndWat()
        => SeededMapGenerator.InitRndWat(_islandSeed);

    protected override void OnAddItem(ItemDef mat, int minCount, int maxCount, F32 hitX, F32 hitY, List<Entity> entities,
        F32? playerFacing = null, double dropChance = 1.0)
        => SeededDropManager.AddItem(mat, minCount, maxCount, hitX, hitY, entities,
            _nonGenSeed, _harvestCounts, UseRelativeFacing, playerFacing, dropChance);
}
