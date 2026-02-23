using Microsoft.Xna.Framework;

namespace CSharpCraft.Pico8;

/// <summary>
/// Immutable container for parsed cart/scene data.
/// Separates raw game data (sprites, flags, map, music, sfx) from orchestration concerns.
/// Created by ICartDataLoader from an IScene's raw string data.
/// </summary>
public record CartData(
    Color[] Sprites,
    int[] Flags,
    int[] Map,
    Dictionary<string, List<SongInst>> Music,
    Dictionary<string, Dictionary<int, string>> Sfx)
{
    /// <summary>
    /// Empty CartData instance for use in tests and as initial state.
    /// </summary>
    public static CartData Empty => new(
        [],
        [],
        [],
        new Dictionary<string, List<SongInst>>(),
        new Dictionary<string, Dictionary<int, string>>());
}
