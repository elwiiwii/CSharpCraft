using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// AudioOrchestrator Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests define the audio coordination contract: delegation to IAudioAPI,
    /// music state tracking, and audio-specific orchestration.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    public class AudioOrchestratorTests
    {
        private readonly Mock<IAudioAPI> _mockAudio;
        private readonly AudioOrchestrator _orchestrator;

        public AudioOrchestratorTests()
        {
            _mockAudio = new Mock<IAudioAPI>();
            _orchestrator = new AudioOrchestrator(_mockAudio.Object);
        }

        #region CONSTRUCTOR TESTS

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenAudioIsNull()
        {
            // Act
            var act = () => new AudioOrchestrator(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("audioAPI");
        }

        [Fact]
        public void Constructor_StoresAudioAPI()
        {
            // Assert
            _orchestrator.API.Should().BeSameAs(_mockAudio.Object,
                "constructor should store the IAudioAPI reference");
        }

        #endregion

        #region DELEGATION TESTS

        [Fact]
        public void Sfx_DelegatesToAudioAPI_WithAllParameters()
        {
            // Act
            _orchestrator.Sfx(5.0, 2.0, 1.0, 16.0);

            // Assert
            _mockAudio.Verify(
                a => a.Sfx(5.0, 2.0, 1.0, 16.0),
                Times.Once(),
                "Sfx should delegate all parameters to IAudioAPI.Sfx"
            );
        }

        [Fact]
        public void Sfx_UsesDefaultParameters()
        {
            // Act
            _orchestrator.Sfx(3.0);

            // Assert
            _mockAudio.Verify(
                a => a.Sfx(3.0, -1.0, 0.0, 31.0),
                Times.Once(),
                "Sfx should use default channel=-1, offset=0, length=31"
            );
        }

        [Fact]
        public void Music_DelegatesToAudioAPI()
        {
            // Act
            _orchestrator.Music(1, 1000);

            // Assert
            _mockAudio.Verify(
                a => a.Music(1, 1000),
                Times.Once(),
                "Music should delegate to IAudioAPI.Music"
            );
        }

        [Fact]
        public void Music_UsesDefaultFade()
        {
            // Act
            _orchestrator.Music(5);

            // Assert
            _mockAudio.Verify(
                a => a.Music(5, 0),
                Times.Once(),
                "Music should default fade to 0"
            );
        }

        [Fact]
        public void Mute_DelegatesToAudioAPI()
        {
            // Act
            _orchestrator.Mute();

            // Assert
            _mockAudio.Verify(
                a => a.Mute(),
                Times.Once(),
                "Mute should delegate to IAudioAPI.Mute"
            );
        }

        #endregion

        #region STATE TRACKING TESTS

        [Fact]
        public void LastMusicCall_IsNull_Initially()
        {
            // Assert
            _orchestrator.LastMusicCall.Should().BeNull(
                "no music should have been played yet");
        }

        [Fact]
        public void Music_TracksLastMusicCall()
        {
            // Act
            _orchestrator.Music(7);

            // Assert
            _orchestrator.LastMusicCall.Should().Be(7,
                "LastMusicCall should track the most recent music track");
        }

        [Fact]
        public void Music_UpdatesLastMusicCall_OnSubsequentCalls()
        {
            // Arrange
            _orchestrator.Music(1);

            // Act
            _orchestrator.Music(5);

            // Assert
            _orchestrator.LastMusicCall.Should().Be(5,
                "LastMusicCall should reflect the most recent call, not the first");
        }

        #endregion
    }
}
