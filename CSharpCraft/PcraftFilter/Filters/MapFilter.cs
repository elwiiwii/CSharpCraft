using CSharpCraft.PcraftFilter.Zones;

namespace CSharpCraft.PcraftFilter.Filters;

internal abstract record MapFilter
{
    internal abstract string HashString { get; }
}

internal sealed record TileCountFilter(
    int TileId,
    int MinimumCount,
    Zone? Zone = null) : MapFilter
{
    internal override string HashString =>
        $"tc.{TileId}.{MinimumCount}.{Zone?.HashString ?? "all"}";
}

internal sealed record TileClusterProximity(
    int MaxDistance,
    int ClusterTileId,
    int MinClusterSize)
{
    internal string HashString => $"prox.{MaxDistance}.{ClusterTileId}.{MinClusterSize}";
}

internal sealed record SpawnConstraintFilter(
    IReadOnlyList<Zone> AllowedZones,
    TileClusterProximity? Proximity = null) : MapFilter
{
    internal override string HashString =>
        $"spa.{string.Join("+", AllowedZones.Select(z => z.HashString))}.{Proximity?.HashString ?? ""}";
}

internal sealed record LocalConcentrationFilter(
    Zone Zone,
    int TileId,
    int MinClusterSize) : MapFilter
{
    internal override string HashString => $"lc.{Zone.HashString}.{TileId}.{MinClusterSize}";
}
