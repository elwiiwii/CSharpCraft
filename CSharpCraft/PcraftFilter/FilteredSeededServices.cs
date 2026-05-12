using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;
using CSharpCraft.PcraftFilter.Bias;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftFilter.Noise;
using CSharpCraft.PcraftFilter.Spawn;
using CSharpCraft.PcraftSeeded;
using CSharpCraft.PcraftSeeded.Noise;

namespace CSharpCraft.PcraftFilter;

/// <summary>
/// Applies the same bias pipeline used by <see cref="FilteredWorldSampler"/> when
/// generating the actual game level, so the map the player sees in a preview is
/// always identical to the map they play on.
/// </summary>
internal sealed class FilteredSeededServices : SeededServices
{
    private const int GridSx = 64;
    private const int GridSy = 64;

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
    {
        if (theme == LevelTheme.Cave)
            return LevelManager.CreateLevel(x, y, sx, sy, theme, player);

        Level level = new(x, y, sx, sy, theme);
        PcraftServices.SetLevel(level, player);

        // --- Mirror the FilteredWorldSampler pipeline exactly ---
        SeededNoiseGrid cur = new(_seed, GridSx, GridSy, GridSx, 0.9, 0.2, 0);
        SeededNoiseGrid cur2 = new(_seed, GridSx, GridSy, 8, 0.9, 0.4, 1);
        SeededNoiseGrid cur3 = new(_seed, GridSx, GridSy, 8, 0.9, 0.3, 2);
        SeededNoiseGrid cur4 = new(_seed, GridSx, GridSy, 4, 0.8, 1.1, 3);

        MapClassifier baseClassifier = new(cur, cur2, cur3, cur4,
            GridSx, GridSy, a: 0, b: 1, c: 2, d: 3, e: 4, generateHole: true);

        SpawnConstraintFilter? spawnFilter = _filters.Filters.OfType<SpawnConstraintFilter>().FirstOrDefault();

        BiasLayers biases = FilterBiasComputer.Compute(_filters, baseClassifier, GridSx, GridSy);

        MapClassifier classifier = biases.IsEmpty
            ? baseClassifier
            : new BiasedMapClassifier(cur, cur2, cur3, cur4,
                GridSx, GridSy, a: 0, b: 1, c: 2, d: 3, e: 4, biases, generateHole: true);

        (int tileX, int tileY)? spawn = FilteredSpawnFinder.FindSpawn(_seed, classifier, biases, GridSx, GridSy, spawnFilter);

        (int holeX, int holeY) = SeededMapGenerator.CreateMap(level, player, classifier, spawn);

        PcraftServices.FillEne(level, player);
        level.Stx = F32.FromInt(((holeX - level.X) * 16) + 8);
        level.Sty = F32.FromInt(((holeY - level.Y) * 16) + 8);
        return level;
    }
}
