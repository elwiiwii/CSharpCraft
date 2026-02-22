using Microsoft.Xna.Framework.Audio;

namespace CSharpCraft.Pico8;

/// <summary>
/// Manages 4 audio channels for sound effects with proper disposal of finished sounds.
/// Fixes critical bug where Sfx() would dispose ALL sounds on a channel instead of just finished ones.
/// </summary>
public class AudioChannels : IDisposable
{
    private readonly List<SoundEffectInstance>[] _channels = new List<SoundEffectInstance>[4];

    public AudioChannels()
    {
        for (int i = 0; i < 4; i++)
        {
            _channels[i] = [];
        }
    }

    /// <summary>
    /// Play a sound effect on the specified channel.
    /// FIXED BUG: Only disposes finished sounds, allows overlapping SFX on same channel.
    /// </summary>
    public void PlaySfx(int channel, SoundEffectInstance instance)
    {
        if (channel < 0 || channel >= 4)
            throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be 0-3");

        var channelList = _channels[channel];

        // Clean up finished sounds first
        for (int i = channelList.Count - 1; i >= 0; i--)
        {
            if (channelList[i].State == SoundState.Stopped)
            {
                channelList[i].Dispose();
                channelList.RemoveAt(i);
            }
        }

        // Add new sound (allows overlapping sounds on same channel)
        channelList.Add(instance);
        instance.Play();
    }

    /// <summary>
    /// Mute all sounds on all channels.
    /// </summary>
    public void MuteAll(float volume = 0.0f)
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (var sfx in _channels[i])
            {
                sfx.Volume = volume;
            }
        }
    }

    /// <summary>
    /// Mute a specific channel.
    /// </summary>
    public void MuteChannel(int channel, float volume = 0.0f)
    {
        if (channel < 0 || channel >= 4)
            throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be 0-3");

        foreach (var sfx in _channels[channel])
        {
            sfx.Volume = volume;
        }
    }

    /// <summary>
    /// Set volume for all channels.
    /// </summary>
    public void SetVolume(float volume)
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (var sfx in _channels[i])
            {
                sfx.Volume = Math.Clamp(volume, 0.0f, 1.0f);
            }
        }
    }

    /// <summary>
    /// Get the list of sounds on a specific channel (for direct access if needed).
    /// </summary>
    public List<SoundEffectInstance> GetChannel(int channel)
    {
        if (channel < 0 || channel >= 4)
            throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be 0-3");

        return _channels[channel];
    }

    /// <summary>
    /// Stop all sounds on all channels and clean up resources.
    /// </summary>
    public void StopAll()
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (var sfx in _channels[i])
            {
                sfx.Stop();
            }
            _channels[i].Clear();
        }
    }

    /// <summary>
    /// Pause all sounds on all channels.
    /// </summary>
    public void PauseAll()
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (var sfx in _channels[i])
            {
                if (sfx.State == SoundState.Playing)
                    sfx.Pause();
            }
        }
    }

    /// <summary>
    /// Resume all paused sounds on all channels.
    /// </summary>
    public void ResumeAll()
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (var sfx in _channels[i])
            {
                if (sfx.State == SoundState.Paused)
                    sfx.Play();
            }
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (var sfx in _channels[i])
            {
                sfx?.Dispose();
            }
            _channels[i].Clear();
        }
    }
}
