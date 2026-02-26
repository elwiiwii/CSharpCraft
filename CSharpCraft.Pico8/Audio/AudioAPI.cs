using Microsoft.Xna.Framework.Audio;

namespace CSharpCraft.Pico8.Audio;

/// <summary>
/// Implementation of Pico-8 audio API using AudioChannels and MusicManager.
/// Consolidates sound effect and music playback into a single cohesive interface.
/// </summary>
public class AudioAPI : IAudioAPI
{
    private readonly AudioChannels? _audioChannels;
    private readonly MusicManager? _musicManager;
    private readonly Dictionary<string, SoundEffect>? _soundEffectDictionary;
    private readonly Func<Dictionary<string, Dictionary<int, string>>>? _getSfxDict;
    private readonly Func<int> _getCurrentSfxPack;
    private readonly Func<bool> _isSoundEnabled;
    private readonly Func<int> _getSfxVolume;

    public AudioAPI(
        AudioChannels? audioChannels,
        MusicManager? musicManager,
        Dictionary<string, SoundEffect>? soundEffectDictionary,
        Func<Dictionary<string, Dictionary<int, string>>>? getSfxDict,
        Func<int> getCurrentSfxPack,
        Func<bool> isSoundEnabled,
        Func<int> getSfxVolume)
    {
        _audioChannels = audioChannels;
        _musicManager = musicManager;
        _soundEffectDictionary = soundEffectDictionary;
        _getSfxDict = getSfxDict;
        _getCurrentSfxPack = getCurrentSfxPack ?? throw new ArgumentNullException(nameof(getCurrentSfxPack));
        _isSoundEnabled = isSoundEnabled ?? throw new ArgumentNullException(nameof(isSoundEnabled));
        _getSfxVolume = getSfxVolume ?? throw new ArgumentNullException(nameof(getSfxVolume));
    }

    public void Sfx(double n, double channel = -1.0, double offset = 0.0, double length = 31.0)
    {
        if (_audioChannels is null || _soundEffectDictionary is null || _getSfxDict is null)
            return;

        int nFlr = (int)Math.Floor(n);
        int channelFlr = (int)Math.Floor(channel);

        if (channelFlr < 0 || channelFlr > 3)
            throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be 0-3");

        var sfxDict = _getSfxDict();
        var currentSfxPack = _getCurrentSfxPack();

        // Get the sound effect key from the sfx dictionary
        string sfxKey = sfxDict.ElementAt(currentSfxPack).Value[nFlr];
        
        if (!_soundEffectDictionary.TryGetValue(sfxKey, out var _))
            return;

        SoundEffectInstance instance = _soundEffectDictionary[sfxKey].CreateInstance();
        instance.Volume = _isSoundEnabled() ? _getSfxVolume() / 100.0f : 0;

        _audioChannels.PlaySfx(channelFlr, instance);
    }

    public void Music(int n, double fadems = 0)
    {
        _musicManager?.PlayMusic(n, fadems);
    }

    public void Mute()
    {
        _musicManager?.Mute(0.0f);
        _audioChannels?.MuteAll(0.0f);
    }

    public void Pause()
    {
        _musicManager?.Pause();
        _audioChannels?.PauseAll();
    }

    public void Resume()
    {
        _musicManager?.Resume();
        _audioChannels?.ResumeAll();
    }

    public void StopAll()
    {
        _musicManager?.StopAll();
        _audioChannels?.StopAll();
    }

    public void Update()
    {
        _musicManager?.Update();
    }
}
