using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System.Linq;

namespace CSharpCraft.Pico8.Services;

/// <summary>
/// Service responsible for all audio playback and management
/// Extracted from Pico8Functions to enable service-based architecture
/// Phase 3: Service Extraction - Part 2
/// </summary>
public class AudioService : IDisposable
{
    private readonly Dictionary<string, SoundEffect> _musicDictionary;
    private readonly Dictionary<string, SoundEffect> _soundEffectDictionary;
    private readonly List<List<MusicInst>> _channelMusic;
    private readonly List<SoundEffectInstance> _channel0;
    private readonly List<SoundEffectInstance> _channel1;
    private readonly List<SoundEffectInstance> _channel2;
    private readonly List<SoundEffectInstance> _channel3;

    private int _curSoundtrack;
    private int _curSfxPack;
    private int _curTrack;
    private int? _lastMusicCall;
    private double _musicFadeMs;
    private double _musicFadeElapsed;
    private bool _isMusicMuted;

    public AudioService(
        Dictionary<string, SoundEffect> musicDictionary,
        Dictionary<string, SoundEffect> soundEffectDictionary,
        List<List<MusicInst>> channelMusic,
        List<SoundEffectInstance> channel0,
        List<SoundEffectInstance> channel1,
        List<SoundEffectInstance> channel2,
        List<SoundEffectInstance> channel3)
    {
        _musicDictionary = musicDictionary;
        _soundEffectDictionary = soundEffectDictionary;
        _channelMusic = channelMusic;
        _channel0 = channel0;
        _channel1 = channel1;
        _channel2 = channel2;
        _channel3 = channel3;

        _curSoundtrack = 0;
        _curSfxPack = 0;
        _curTrack = 0;
        _lastMusicCall = null;
        _isMusicMuted = false;
    }

    /// <summary>
    /// Play music track with optional fade (Pico-8: Music)
    /// </summary>
    public void Music(int n, double fadems = 0)
    {
        if (n < 0) return;

        _curTrack = n;
        _lastMusicCall = n;
        _musicFadeMs = fadems;
        _musicFadeElapsed = 0;

        // Stop existing music
        foreach (var instance in _channel0.Concat(_channel1).Concat(_channel2).Concat(_channel3))
        {
            instance.Stop();
        }
        _channel0.Clear();
        _channel1.Clear();
        _channel2.Clear();
        _channel3.Clear();

        // Load new music track (simplified - in real implementation would load from _musicDictionary)
        // This is a placeholder for the actual music loading logic
    }

    /// <summary>
    /// Play sound effect on specified channel (Pico-8: Sfx)
    /// </summary>
    public void Sfx(double n, double channel = -1.0, double offset = 0.0, double length = 31.0)
    {
        if (n < 0) return;

        int sfxId = (int)n;
        int channelId = (int)channel;

        // Default channel selection if not specified
        if (channelId < 0)
        {
            channelId = GetAvailableChannel();
        }

        if (channelId < 0 || channelId > 3) return;

        // Play sound effect on the appropriate channel (simplified)
        var channelList = channelId switch
        {
            0 => _channel0,
            1 => _channel1,
            2 => _channel2,
            3 => _channel3,
            _ => _channel0
        };

        // In real implementation, would load and play from _soundEffectDictionary
    }

    /// <summary>
    /// Mute all audio (Pico-8: Mute)
    /// </summary>
    public void Mute()
    {
        _isMusicMuted = !_isMusicMuted;

        foreach (var instance in _channel0.Concat(_channel1).Concat(_channel2).Concat(_channel3))
        {
            if (_isMusicMuted)
                instance.Pause();
            else
                instance.Resume();
        }
    }

    /// <summary>
    /// Get the next available audio channel
    /// </summary>
    private int GetAvailableChannel()
    {
        // Check channels in order for first empty one
        if (_channel0.Count == 0) return 0;
        if (_channel1.Count == 0) return 1;
        if (_channel2.Count == 0) return 2;
        if (_channel3.Count == 0) return 3;

        // All channels full, use round-robin
        return 0; // Default to channel 0 if all full
    }

    /// <summary>
    /// Update audio state (called each frame)
    /// </summary>
    public void Update(double deltaTime)
    {
        // Handle music fade transitions
        if (_musicFadeMs > 0)
        {
            _musicFadeElapsed += deltaTime;
            double fadeProgress = Math.Min(1.0, _musicFadeElapsed / _musicFadeMs);

            // Apply fade to music instances
            foreach (var instance in _channel0.Concat(_channel1).Concat(_channel2).Concat(_channel3))
            {
                instance.Volume = (float)(1.0 - fadeProgress);
            }
        }

        // Clean up finished sound effects
        _channel0.RemoveAll(i => i.State == SoundState.Stopped);
        _channel1.RemoveAll(i => i.State == SoundState.Stopped);
        _channel2.RemoveAll(i => i.State == SoundState.Stopped);
        _channel3.RemoveAll(i => i.State == SoundState.Stopped);
    }

    /// <summary>
    /// Set current soundtrack for music tracks
    /// </summary>
    public void SetCurrentSoundtrack(int soundtrackId)
    {
        _curSoundtrack = soundtrackId;
    }

    /// <summary>
    /// Set current SFX pack index
    /// </summary>
    public void SetCurrentSfxPack(int sfxPackId)
    {
        _curSfxPack = sfxPackId;
    }

    /// <summary>
    /// Get current music track number
    /// </summary>
    public int GetCurrentTrack() => _curTrack;

    /// <summary>
    /// Get current soundtrack index
    /// </summary>
    public int GetCurrentSoundtrack() => _curSoundtrack;

    public void Dispose()
    {
        _channel0.ForEach(i => i?.Dispose());
        _channel1.ForEach(i => i?.Dispose());
        _channel2.ForEach(i => i?.Dispose());
        _channel3.ForEach(i => i?.Dispose());

        _musicDictionary?.Clear();
        _soundEffectDictionary?.Clear();
    }
}
