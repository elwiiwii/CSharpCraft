using CSharpCraft.Pico8;
using CSharpCraft.Tests.Pico8.Mocks;
using Xunit;
using FluentAssertions;

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
        _audio.PlaybackCalls.Should().HaveCount(1);
        var call = _audio.PlaybackCalls[0];
        call.operation.Should().Be("PlayMusic");
        call.id.Should().Be(trackId);
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
        _audio.PlaybackCalls.Should().HaveCount(1);
        var call = _audio.PlaybackCalls[0];
        call.operation.Should().Be("PlaySfx");
        call.id.Should().Be(sfxId);
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
        _audio.PlaybackCalls.Should().HaveCount(4);
        for (int i = 0; i < 4; i++)
        {
            _audio.PlaybackCalls[i].operation.Should().Be("PlaySfx");
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
        _audio.PlaybackCalls.Should().HaveCount(3);
        var lastCall = _audio.PlaybackCalls[2];
        lastCall.operation.Should().Be("Mute");
    }

    [Fact]
    public void MultipleMusicTracks_SequentialPlayback()
    {
        // Arrange & Act
        _audio.PlayMusic(0);
        _audio.PlayMusic(1);
        _audio.PlayMusic(2);

        // Assert
        _audio.PlaybackCalls.Should().HaveCount(3);
        _audio.PlaybackCalls[0].operation.Should().Be("PlayMusic");
        _audio.PlaybackCalls[1].operation.Should().Be("PlayMusic");
        _audio.PlaybackCalls[2].operation.Should().Be("PlayMusic");
    }

    [Fact]
    public void PlaySfx_WithDefaultChannel()
    {
        // Arrange & Act
        _audio.PlaySfx(5); // Use default channel -1

        // Assert
        _audio.PlaybackCalls.Should().HaveCount(1);
        _audio.PlaybackCalls[0].operation.Should().Be("PlaySfx");
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
        _audio.PlaybackCalls.Should().HaveCount(4);
        _audio.PlaybackCalls[0].operation.Should().Be("PlayMusic");
        _audio.PlaybackCalls[1].operation.Should().Be("PlaySfx");
        _audio.PlaybackCalls[2].operation.Should().Be("PlaySfx");
        _audio.PlaybackCalls[3].operation.Should().Be("Mute");
    }
}
