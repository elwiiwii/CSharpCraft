using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8;

/// <summary>
/// Parses cart/scene raw data strings into an immutable CartData instance.
/// Extracted from GameOrchestrator.Reload() to follow Single Responsibility Principle.
/// 
/// Handles:
/// - Sprite data parsing (hex string or texture image)
/// - Flag data parsing (hex string)
/// - Map data parsing (custom PICO-8 encoding) with dimension validation
/// - Music and SFX dictionary passthrough
/// </summary>
public class CartDataLoader : ICartDataLoader
{
    public CartData Load(IScene scene, List<Color> colors, Dictionary<string, Texture2D> textureDictionary)
    {
        ArgumentNullException.ThrowIfNull(scene, nameof(scene));
        ArgumentNullException.ThrowIfNull(colors, nameof(colors));
        ArgumentNullException.ThrowIfNull(textureDictionary, nameof(textureDictionary));

        // Parse sprites from hex string or texture image
        Color[] sprites = [];
        if (!string.IsNullOrEmpty(scene.SpriteData))
            sprites = Pico8Utils.DataToColorArray(colors, scene.SpriteData, 1);
        if (!string.IsNullOrEmpty(scene.SpriteImage))
            sprites = Pico8Utils.ImageToColorArray(textureDictionary, scene.SpriteImage);

        // Parse flag data from hex string
        int[] flags = Pico8Utils.DataToArray(scene.FlagData, 2);

        // Validate map dimensions and parse map data
        if (scene.MapDimensions.x * scene.MapDimensions.y != scene.MapData.Length / 2)
            throw new Exception("Map dimensions do not match map data length.");
        int[] map = Pico8Utils.MapDataToArray(scene.MapData);

        return new CartData(
            sprites,
            flags,
            map,
            scene.Music,
            scene.Sfx);
    }
}
