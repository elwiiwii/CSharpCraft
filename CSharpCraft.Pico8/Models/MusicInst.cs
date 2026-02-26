using Microsoft.Xna.Framework.Audio;

namespace CSharpCraft.Pico8.Models;

/// <summary>
/// Represents an active music instance (a playing SoundEffectInstance).
/// </summary>
public record MusicInst(string Name, SoundEffectInstance Track, bool Loop, int Group);
