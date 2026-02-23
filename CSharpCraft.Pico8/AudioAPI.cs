using Microsoft.Xna.Framework.Audio;

namespace CSharpCraft.Pico8;

/// <summary>
/// Implementation of Pico-8 audio API using AudioChannels and MusicManager.
/// Consolidates sound effect and music playback into a single cohesive interface.
/// </summary>
public class AudioAPI(
    AudioChannels? audioChannels,
    MusicManager? musicManager,
    Dictionary<string, SoundEffect>? soundEffectDictionary,
    Func<Dictionary<string, Dictionary<int, string>>>? getSfxDict,
    Func<int> getCurrentSfxPack,
    Func<bool> isSoundEnabled,
    Func<int> getSfxVolume) : IAudioAPI
{
    public void Sfx(double n, double channel = -1.0, double offset = 0.0, double length = 31.0)
    {
        if (audioChannels is null || soundEffectDictionary is null || getSfxDict is null)
            return;

        int nFlr = (int)Math.Floor(n);
        int channelFlr = (int)Math.Floor(channel);

        if (channelFlr < 0 || channelFlr > 3)
            throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be 0-3");

        var sfxDict = getSfxDict();
        var currentSfxPack = getCurrentSfxPack();

        // Get the sound effect key from the sfx dictionary
        string sfxKey = sfxDict.ElementAt(currentSfxPack).Value[nFlr];
        
        if (!soundEffectDictionary.TryGetValue(sfxKey, out var _))
            return;

        SoundEffectInstance instance = soundEffectDictionary[sfxKey].CreateInstance();
        instance.Volume = isSoundEnabled() ? getSfxVolume() / 100.0f : 0;

        audioChannels.PlaySfx(channelFlr, instance);
    }

    public void Music(int n, double fadems = 0)
    {
        musicManager?.PlayMusic(n, fadems);
    }

    public void Mute()
    {
        musicManager?.Mute(0.0f);
        audioChannels?.MuteAll(0.0f);
    }
}
