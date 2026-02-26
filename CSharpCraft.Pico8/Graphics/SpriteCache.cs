using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8.Graphics;

/// <summary>
/// Manages sprite texture caching with LRU eviction.
/// Used by Spr() and Sspr() methods to avoid recreating textures for identical sprite+palette combinations.
/// </summary>
public class SpriteCache : IDisposable
{
    private readonly Dictionary<int[], Texture2D> _cache;
    private readonly LinkedList<int[]> _lruOrder = [];
    private const int MaxCacheSize = 32;

    public SpriteCache()
    {
        _cache = new(new IntArrayEqualityComparer());
    }

    /// <summary>
    /// Try to get a cached sprite texture by palette-aware key.
    /// </summary>
    public bool TryGetTexture(int[] cacheKey, out Texture2D? texture)
    {
        return _cache.TryGetValue(cacheKey, out texture);
    }

    /// <summary>
    /// Add a newly created sprite texture to the cache with LRU eviction.
    /// </summary>
    public void AddTexture(int[] cacheKey, Texture2D texture)
    {
        // If cache already has this key, don't add duplicate
        if (_cache.ContainsKey(cacheKey))
            return;

        // If cache is full, evict oldest
        while (_cache.Count >= MaxCacheSize && _lruOrder.Count > 0)
        {
            var lruKey = _lruOrder.First!.Value;
            _lruOrder.RemoveFirst();
            
            if (_cache.Remove(lruKey, out var oldTexture))
            {
                oldTexture?.Dispose();
            }
        }

        // Add new texture
        _cache[cacheKey] = texture;
        _lruOrder.AddLast(cacheKey);
    }

    /// <summary>
    /// Clear the entire cache and dispose all textures.
    /// </summary>
    public void Clear()
    {
        foreach (var texture in _cache.Values)
        {
            texture?.Dispose();
        }
        _cache.Clear();
        _lruOrder.Clear();
    }

    public (int CacheSize, int LRUSize) GetStats() => (_cache.Count, _lruOrder.Count);

    public void Dispose()
    {
        Clear();
    }
}

