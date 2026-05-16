using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftFilter.Bias;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftFilter.Noise;
using CSharpCraft.PcraftFilter.Spawn;
using CSharpCraft.PcraftSeeded;
using CSharpCraft.PcraftSeeded.Noise;

namespace CSharpCraft.PcraftFilter;

internal static class FilteredLevelManager
{
    internal static Level CreateLevel(
        long islandSeed, long caveSeed,
        int x, int y, int sx, int sy, LevelTheme theme,
        PlayerEntity player, FilterSet filters)
    {
        if (theme == LevelTheme.Cave)
            return SeededLevelManager.CreateLevel(islandSeed, caveSeed, x, y, sx, sy, theme, player);

        Level level = new(x, y, sx, sy, theme);
        PcraftServices.SetLevel(level, player);

        // Mirror the FilteredWorldSampler pipeline so the preview and the played map are identical.
        var (cur, cur2, cur3, cur4) = SeededMapGenerator.CreateIslandNoiseGrids(islandSeed);

        MapClassifier baseClassifier = new(cur, cur2, cur3, cur4,
            SeededMapGenerator.IslandGridSx, SeededMapGenerator.IslandGridSy,
            a: 0, b: 1, c: 2, d: 3, e: 4, generateHole: true);

        SpawnConstraintFilter? spawnFilter = filters.Filters.OfType<SpawnConstraintFilter>().FirstOrDefault();

        BiasLayers biases = FilterBiasComputer.Compute(filters, baseClassifier,
            SeededMapGenerator.IslandGridSx, SeededMapGenerator.IslandGridSy);

        MapClassifier classifier = biases.IsEmpty
            ? baseClassifier
            : new BiasedMapClassifier(cur, cur2, cur3, cur4,
                SeededMapGenerator.IslandGridSx, SeededMapGenerator.IslandGridSy,
                a: 0, b: 1, c: 2, d: 3, e: 4, biases, generateHole: true);

        (int tileX, int tileY)? spawn = FilteredSpawnFinder.FindSpawn(islandSeed, classifier, biases,
            SeededMapGenerator.IslandGridSx, SeededMapGenerator.IslandGridSy, spawnFilter);

        (int holeX, int holeY) = SeededMapGenerator.CreateMap(level, player, classifier, spawn);

        PcraftServices.FillEne(level, player);
        level.Stx = F32.FromInt(((holeX - level.X) * 16) + 8);
        level.Sty = F32.FromInt(((holeY - level.Y) * 16) + 8);
        return level;
    }
}
