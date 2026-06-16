using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftSeeded;

internal class SeededServices : PcraftServices
{
    internal readonly long IslandSeed;
    internal readonly long CaveSeed;
    internal readonly long NonGenSeed;
    private readonly Dictionary<string, int> _harvestCounts = new();
    internal bool UseRelativeFacing { get; init; }

    private const int CaveSalt = 0x4341_5645;

    internal SeededServices(long masterSeed, long? islandSeed = null, long? caveSeed = null, long? nonGenSeed = null)
    {
        IslandSeed = islandSeed ?? masterSeed;
        CaveSeed = caveSeed ?? SeedMixer.Combine(masterSeed, CaveSalt);
        NonGenSeed = nonGenSeed ?? masterSeed;
        SetServices(this);
    }

    protected override Level OnCreateLevel(int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
        => SeededLevelManager.CreateLevel(IslandSeed, CaveSeed, x, y, sx, sy, theme, player);

    protected override F32[][] OnInitRndWat()
        => SeededMapGenerator.InitRndWat(IslandSeed);

    protected override void OnAddItem(ItemDef mat, int minCount, int maxCount, F32 hitX, F32 hitY, List<Entity> entities,
        F32? playerFacing = null, double dropChance = 1.0)
        => SeededDropManager.AddItem(mat, minCount, maxCount, hitX, hitY, entities,
            NonGenSeed, _harvestCounts, UseRelativeFacing, playerFacing, dropChance);
}
