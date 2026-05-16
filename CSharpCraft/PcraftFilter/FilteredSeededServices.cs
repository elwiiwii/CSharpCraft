using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftSeeded;

namespace CSharpCraft.PcraftFilter;

/// <summary>
/// Applies the same bias pipeline used by <see cref="FilteredWorldSampler"/> when
/// generating the actual game level, so the map the player sees in a preview is
/// always identical to the map they play on.
/// </summary>
internal sealed class FilteredSeededServices : SeededServices
{
    private readonly FilterSet _filters;

    internal FilteredSeededServices(long seed, FilterSet filters)
        : this(seed, filters ?? throw new ArgumentNullException(nameof(filters)), false)
    {
    }

    // Private chaining constructor — only reached after filters is validated above.
    private FilteredSeededServices(long seed, FilterSet filters, bool _)
        : base(seed)
    {
        _filters = filters;
    }

    protected override Level OnCreateLevel(int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
        => FilteredLevelManager.CreateLevel(_islandSeed, _caveSeed, x, y, sx, sy, theme, player, _filters);
}
