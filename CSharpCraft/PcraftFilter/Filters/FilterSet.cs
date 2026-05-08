namespace CSharpCraft.PcraftFilter.Filters;

internal sealed class FilterSet
{
    internal IReadOnlyList<MapFilter> Filters { get; }

    internal FilterSet(IReadOnlyList<MapFilter> filters)
    {
        Filters = filters ?? throw new ArgumentNullException(nameof(filters));
    }

    internal string HashString => string.Join(":", Filters.Select(f => f.HashString));

    internal string CombineWithSeed(long seed)
    {
        return $"{seed}:{HashString}";
    }
}
