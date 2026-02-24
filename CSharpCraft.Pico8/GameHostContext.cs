using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pico8;

/// <summary>
/// Groups FNA/MonoGame platform dependencies needed by GameOrchestrator's
/// production constructor. These are host-specific objects not needed for
/// unit testing (the test constructor bypasses them entirely).
/// 
/// Reduces the production constructor from 20 individual params to a
/// single context object plus behavioral dependencies.
/// </summary>
public record GameHostContext(
    SpriteBatch Batch,
    Texture2D Pixel,
    GraphicsDeviceManager Graphics,
    GraphicsDevice GraphicsDevice,
    GameWindow Window,
    Dictionary<string, Texture2D> TextureDictionary,
    Dictionary<string, SoundEffect> MusicDictionary,
    Dictionary<string, SoundEffect> SoundEffectDictionary,
    IAudioGraphicsSettings Settings,
    IInputBindingProvider InputBindings,
    List<IScene> Scenes);
