using CSharpCraft.PcraftFilter.Filters;

namespace CSharpCraft.PcraftFilter.Bias;

/// <summary>
/// Receives diagnostic notifications from <see cref="FilterBiasComputer"/> when
/// a filter requirement cannot be fully or optimally satisfied.
/// </summary>
internal interface IFilterDiagnosticSink
{
    /// <summary>
    /// Called when a <see cref="TileCountFilter"/> or <see cref="LocalConcentrationFilter"/>
    /// could not fully satisfy its deficit.
    /// </summary>
    /// <param name="filter">The filter that could not be fully satisfied.</param>
    /// <param name="deficit">The total number of additional tiles required.</param>
    /// <param name="resolved">The number of tiles that were successfully biased.</param>
    void OnFilterConflict(MapFilter filter, int deficit, int resolved);

    /// <summary>Called when a spawn fallback bias was applied to satisfy a <see cref="SpawnConstraintFilter"/>.</summary>
    void OnSpawnFallbackApplied(SpawnConstraintFilter filter);

    /// <summary>Called when cluster growth stalled before reaching <paramref name="required"/>.</summary>
    void OnClusterGrowthStalled(LocalConcentrationFilter filter, int reached, int required);
}
