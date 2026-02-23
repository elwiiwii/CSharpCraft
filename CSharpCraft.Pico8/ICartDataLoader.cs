using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8;

/// <summary>
/// Loads and parses cart data from an IScene's raw string properties.
/// Follows Single Responsibility Principle: only concerned with data parsing.
/// </summary>
public interface ICartDataLoader
{
    /// <summary>
    /// Parse the scene's raw data (sprite hex, flags, map encoding, music/sfx dicts)
    /// into an immutable CartData instance.
    /// </summary>
    /// <param name="scene">The scene whose data to parse.</param>
    /// <param name="colors">The color palette for sprite data parsing.</param>
    /// <param name="textureDictionary">Texture dictionary for image-based sprite loading.</param>
    /// <returns>Parsed cart data ready for consumption by the game engine.</returns>
    CartData Load(IScene scene, List<Color> colors, Dictionary<string, Texture2D> textureDictionary);
}
