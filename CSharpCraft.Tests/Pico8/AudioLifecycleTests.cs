using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// Audio Lifecycle Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests verify that audio lifecycle management (play/pause, stop, update, dispose)
    /// belongs in AudioOrchestrator, and that GameOrchestrator delegates to it
    /// instead of owning dead _audioChannels/_musicManager fields.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    public class AudioLifecycleTests
    {
        private readonly Mock<IAudioAPI> _mockAudioAPI;
        private readonly AudioOrchestrator _audioOrch;

        public AudioLifecycleTests()
        {
            _mockAudioAPI = new Mock<IAudioAPI>();
            _audioOrch = new AudioOrchestrator(_mockAudioAPI.Object);
        }

        #region AUDIO ORCHESTRATOR LIFECYCLE METHODS

        [Fact]
        public void AudioOrchestrator_HasPlaySoundMethod()
        {
            var method = typeof(AudioOrchestrator).GetMethod("PlaySound");
            method.Should().NotBeNull(
                "AudioOrchestrator should have a PlaySound method for pause/resume");
        }

        [Fact]
        public void AudioOrchestrator_HasSoundDisposeMethod()
        {
            var method = typeof(AudioOrchestrator).GetMethod("SoundDispose");
            method.Should().NotBeNull(
                "AudioOrchestrator should have a SoundDispose method to stop all audio");
        }

        [Fact]
        public void AudioOrchestrator_HasUpdateMethod()
        {
            var method = typeof(AudioOrchestrator).GetMethod("Update");
            method.Should().NotBeNull(
                "AudioOrchestrator should have an Update method for audio tick");
        }

        [Fact]
        public void AudioOrchestrator_ImplementsIDisposable()
        {
            typeof(AudioOrchestrator).Should().Implement<IDisposable>(
                "AudioOrchestrator should implement IDisposable for cleanup");
        }

        [Fact]
        public void PlaySound_True_CallsResumeOnAPI()
        {
            // Act - resume audio
            _audioOrch.PlaySound(true);

            // Assert
            _mockAudioAPI.Verify(a => a.Resume(), Times.Once(),
                "PlaySound(true) should call Resume on IAudioAPI");
        }

        [Fact]
        public void PlaySound_False_CallsPauseOnAPI()
        {
            // Act - pause audio
            _audioOrch.PlaySound(false);

            // Assert
            _mockAudioAPI.Verify(a => a.Pause(), Times.Once(),
                "PlaySound(false) should call Pause on IAudioAPI");
        }

        [Fact]
        public void SoundDispose_CallsStopAllOnAPI()
        {
            // Act
            _audioOrch.SoundDispose();

            // Assert
            _mockAudioAPI.Verify(a => a.StopAll(), Times.Once(),
                "SoundDispose should call StopAll on IAudioAPI");
        }

        [Fact]
        public void Update_CallsUpdateOnAPI()
        {
            // Act
            _audioOrch.Update();

            // Assert
            _mockAudioAPI.Verify(a => a.Update(), Times.Once(),
                "Update should call Update on IAudioAPI");
        }

        [Fact]
        public void Dispose_CallsStopAllOnAPI()
        {
            // Act
            _audioOrch.Dispose();

            // Assert
            _mockAudioAPI.Verify(a => a.StopAll(), Times.Once(),
                "Dispose should stop all audio");
        }

        #endregion

        #region GAME ORCHESTRATOR DELEGATION

        [Fact]
        public void GameOrchestrator_LastMusicCall_DelegatesToAudioOrchestrator()
        {
            // Arrange - play music through AudioOrchestrator
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            orchestrator.Audio.Music(42);

            // Assert - GameOrchestrator.LastMusicCall should reflect AudioOrchestrator state
            orchestrator.LastMusicCall.Should().Be(42,
                "GameOrchestrator.LastMusicCall should delegate to AudioOrchestrator.LastMusicCall");
        }

        [Fact]
        public void GameOrchestrator_DoesNotHaveAudioChannelsField()
        {
            var field = typeof(GameOrchestrator).GetField("_audioChannels",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.Should().BeNull(
                "GameOrchestrator should not own _audioChannels — audio lifecycle belongs in AudioOrchestrator");
        }

        [Fact]
        public void GameOrchestrator_DoesNotHaveMusicManagerField()
        {
            var field = typeof(GameOrchestrator).GetField("_musicManager",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.Should().BeNull(
                "GameOrchestrator should not own _musicManager — audio lifecycle belongs in AudioOrchestrator");
        }

        #endregion
    }
}
