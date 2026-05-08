namespace CSharpCraft.PcraftPreview.Noise;

/// <summary>
/// Finds a valid spawn tile by sampling deterministic candidates within the
/// inner three-quarters of the grid. The search is completely independent of
/// terrain noise — each candidate is derived solely from (masterSeed, k).
/// </summary>
internal static class SpawnFinder
{
    private const int MaxCandidates = 500;

    /// <summary>
    /// Searches up to 500 deterministic candidate positions and returns the first
    /// whose tile id is in the valid spawn set {1, 2}. Returns null if none qualifies.
    /// </summary>
    internal static (int tileX, int tileY)? FindSpawn(
        long masterSeed,
        MapClassifier classifier,
        int gridSx,
        int gridSy)
    {
        if (classifier is null)
            throw new ArgumentNullException(nameof(classifier));

        int minX = gridSx / 8;
        int maxX = gridSx * 6 / 8;
        int minY = gridSy / 8;
        int maxY = gridSy * 6 / 8;

        int rangeX = maxX - minX + 1;
        int rangeY = maxY - minY + 1;

        int spawnHash = "spawn".GetHashCode();

        for (int k = 0; k < MaxCandidates; k++)
        {
            Random rng = new(HashCode.Combine(masterSeed.GetHashCode(), k, spawnHash));
            int x = minX + rng.Next(rangeX);
            int y = minY + rng.Next(rangeY);

            int tileId = classifier.ClassifyTile(x, y);
            if (tileId is 1 or 2)
                return (x, y);
        }

        return null;
    }
}
