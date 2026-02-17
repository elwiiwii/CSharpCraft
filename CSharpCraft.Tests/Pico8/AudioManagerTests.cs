using CSharpCraft.Pico8;
using CSharpCraft.Tests.Pico8.Mocks;
using Xunit;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Unit tests for audio management
/// Tests music playback, SFX triggering, and muting
/// </summary>
public class AudioManagerTests
{
    private readonly MockAudioManager _audio;

    public AudioManagerTests()
    {
        _audio = new MockAudioManager();
    }

    [Fact]
    public void PlayMusic_RecordsMusicPlayback()
    {
        // Arrange
        int trackId = 0;
        double fade = 500;

        // Act
        _audio.PlayMusic(trackId, fade);

        // Assert
        Assert.Single(_audio.PlaybackCalls);
        var call = _audio.PlaybackCalls[0];
        Assert.Equal("PlayMusic", call.operation);
        Assert.Equal(trackId, call.id);
    }

    [Fact]
    public void PlaySfx_RecordsSfxPlayback()
    {
        // Arrange
        int sfxId = 3;
        int channel = 0;

        // Act
        _audio.PlaySfx(sfxId, channel);

        // Assert
        Assert.Single(_audio.PlaybackCalls);
        var call = _audio.PlaybackCalls[0];
        Assert.Equal("PlaySfx", call.operation);
        Assert.Equal(sfxId, call.id);
    }

    [Fact]
    public void PlaySfx_OnMultipleChannels()
    {
        // Arrange & Act
        _audio.PlaySfx(0, 0);
        _audio.PlaySfx(1, 1);
        _audio.PlaySfx(2, 2);
        _audio.PlaySfx(3, 3);

        // Assert
        Assert.Equal(4, _audio.PlaybackCalls.Count);
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal("PlaySfx", _audio.PlaybackCalls[i].operation);
        }
    }

    [Fact]
    public void Mute_RecordsMuteOperation()
    {
        // Arrange & Act
        _audio.PlayMusic(0);
        _audio.PlaySfx(0);
        _audio.Mute();

        // Assert
        Assert.Equal(3, _audio.PlaybackCalls.Count);
        var lastCall = _audio.PlaybackCalls[2];
        Assert.Equal("Mute", lastCall.operation);
    }

    [Fact]
    public void MultipleMusicTracks_SequentialPlayback()
    {
        // Arrange & Act
        _audio.PlayMusic(0);
        _audio.PlayMusic(1);
        _audio.PlayMusic(2);

        // Assert
        Assert.Equal(3, _audio.PlaybackCalls.Count);
        Assert.Equal("PlayMusic", _audio.PlaybackCalls[0].operation);
        Assert.Equal("PlayMusic", _audio.PlaybackCalls[1].operation);
        Assert.Equal("PlayMusic", _audio.PlaybackCalls[2].operation);
    }

    [Fact]
    public void PlaySfx_WithDefaultChannel()
    {
        // Arrange & Act
        _audio.PlaySfx(5); // Use default channel -1

        // Assert
        Assert.Single(_audio.PlaybackCalls);
        Assert.Equal("PlaySfx", _audio.PlaybackCalls[0].operation);
    }

    [Fact]
    public void AudioSequence_ComplexScenario()
    {
        // Arrange & Act - Simulates game audio lifecycle
        _audio.PlayMusic(0, 500); // Start background music
        _audio.PlaySfx(1, 0);     // Play jump sound
        _audio.PlaySfx(2, 1);     // Play different sound on different channel
        _audio.Mute();            // Pause all audio

        // Assert
        Assert.Equal(4, _audio.PlaybackCalls.Count);
        Assert.Equal("PlayMusic", _audio.PlaybackCalls[0].operation);
        Assert.Equal("PlaySfx", _audio.PlaybackCalls[1].operation);
        Assert.Equal("PlaySfx", _audio.PlaybackCalls[2].operation);
        Assert.Equal("Mute", _audio.PlaybackCalls[3].operation);
    }
}
