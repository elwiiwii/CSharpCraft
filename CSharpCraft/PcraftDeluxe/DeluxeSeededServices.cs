using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftSeeded;

namespace CSharpCraft.PcraftDeluxe;

/// <summary>
/// Combines Deluxe-edition generation (inherited from <see cref="DeluxeServices"/>)
/// with deterministic seeded level creation. Any future overrides added to
/// <see cref="DeluxeServices"/> are automatically inherited here.
/// </summary>
internal sealed class DeluxeSeededServices : DeluxeServices
{
    private readonly long _seed;

    internal DeluxeSeededServices(long seed)
    {
        _seed = seed;
    }

    protected override Level OnCreateLevel(int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
        => SeededLevelManager.CreateLevel(_seed, _seed, x, y, sx, sy, theme, player);

    protected override F32[][] OnInitRndWat()
        => SeededMapGenerator.InitRndWat(_seed);
}
