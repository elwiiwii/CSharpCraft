using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using System;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// GameOrchestrator Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests define the orchestration contract: scene management, pause state,
    /// update/draw lifecycle, and initialization behavior.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    [Collection("Pico8Static")]
    public class GameOrchestratorTests : IDisposable
    {
        private readonly Mock<IInputStateManager> _mockInput;
        private readonly Mock<IGraphicsAPI> _mockGraphics;
        private readonly Mock<IAudioAPI> _mockAudio;
        private readonly Mock<ISceneManager> _mockSceneManager;
        private readonly GameOrchestrator _orchestrator;

        public GameOrchestratorTests()
        {
            _mockInput = new Mock<IInputStateManager>();
            _mockGraphics = new Mock<IGraphicsAPI>();
            _mockAudio = new Mock<IAudioAPI>();
            _mockSceneManager = new Mock<ISceneManager>();

            _orchestrator = new GameOrchestrator(
                _mockInput.Object,
                _mockGraphics.Object,
                _mockAudio.Object,
                _mockSceneManager.Object
            );
        }

        public void Dispose()
        {
            // Clean up between tests
        }

        #region CONSTRUCTOR TESTS

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenInputManagerIsNull()
        {
            // Act
            var act = () => new GameOrchestrator(
                null!,
                _mockGraphics.Object,
                _mockAudio.Object,
                _mockSceneManager.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("inputManager");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenGraphicsIsNull()
        {
            // Act
            var act = () => new GameOrchestrator(
                _mockInput.Object,
                null!,
                _mockAudio.Object,
                _mockSceneManager.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("graphicsAPI");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenAudioIsNull()
        {
            // Act
            var act = () => new GameOrchestrator(
                _mockInput.Object,
                _mockGraphics.Object,
                null!,
                _mockSceneManager.Object
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("audioAPI");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenSceneManagerIsNull()
        {
            // Act
            var act = () => new GameOrchestrator(
                _mockInput.Object,
                _mockGraphics.Object,
                _mockAudio.Object,
                null!
            );

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("sceneManager");
        }

        [Fact]
        public void Constructor_SetsManagerProperties_Correctly()
        {
            // Assert
            _orchestrator.InputManager.Should().BeSameAs(_mockInput.Object);
            _orchestrator.Graphics.Should().NotBeNull();
            _orchestrator.Graphics.API.Should().BeSameAs(_mockGraphics.Object,
                "Graphics sub-orchestrator should wrap the provided IGraphicsAPI");
            _orchestrator.Audio.Should().NotBeNull();
            _orchestrator.Audio.API.Should().BeSameAs(_mockAudio.Object,
                "Audio sub-orchestrator should wrap the provided IAudioAPI");
            _orchestrator.SceneManager.Should().BeSameAs(_mockSceneManager.Object);
        }

        [Fact]
        public void Constructor_InitializesNotPaused()
        {
            // Assert
            _orchestrator.IsPaused.Should().BeFalse("orchestrator should not be paused on creation");
        }

        [Fact]
        public void Constructor_HasNoCurrentScene()
        {
            // Assert
            _orchestrator.CurrentScene.Should().BeNull("no scene should be loaded initially");
        }

        #endregion

        #region INITIALIZATION TESTS

        [Fact]
        public void Initialize_RegistersPico8StaticAPI()
        {
            // Act
            _orchestrator.Initialize();

            // Assert - verify Pico8 static class works after initialization
            _mockInput.Setup(i => i.Btn(0, 0)).Returns(true);
            var result = CSharpCraft.Pico8.Pico8.Btn(0);
            result.Should().BeTrue("Pico8 static API should work after Initialize()");
        }

        #endregion

        #region SCENE MANAGEMENT TESTS

        [Fact]
        public void LoadScene_SetsCurrentScene()
        {
            // Arrange
            _orchestrator.Initialize();
            var mockScene = new Mock<IScene>();

            // Act
            _orchestrator.LoadScene(mockScene.Object);

            // Assert
            _orchestrator.CurrentScene.Should().BeSameAs(mockScene.Object,
                "CurrentScene should reference the loaded scene");
        }

        [Fact]
        public void LoadScene_CallsInit_OnNewScene()
        {
            // Arrange
            _orchestrator.Initialize();
            var mockScene = new Mock<IScene>();

            // Act
            _orchestrator.LoadScene(mockScene.Object);

            // Assert
            mockScene.Verify(
                s => s.Init(),
                Times.Once(),
                "LoadScene should initialize the new scene"
            );
        }

        [Fact]
        public void LoadScene_ThrowsArgumentNullException_WhenSceneIsNull()
        {
            // Arrange
            _orchestrator.Initialize();

            // Act
            var act = () => _orchestrator.LoadScene(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("scene");
        }

        [Fact]
        public void LoadScene_ReplacesCurrentScene()
        {
            // Arrange
            _orchestrator.Initialize();
            var scene1 = new Mock<IScene>();
            var scene2 = new Mock<IScene>();

            // Act
            _orchestrator.LoadScene(scene1.Object);
            _orchestrator.LoadScene(scene2.Object);

            // Assert
            _orchestrator.CurrentScene.Should().BeSameAs(scene2.Object,
                "LoadScene should replace the previous scene");
        }

        #endregion

        #region PAUSE STATE TESTS

        [Fact]
        public void Pause_SetsPausedState()
        {
            // Arrange
            _orchestrator.Initialize();

            // Act
            _orchestrator.Pause();

            // Assert
            _orchestrator.IsPaused.Should().BeTrue("game should be paused after Pause()");
        }

        [Fact]
        public void Resume_ClearsPausedState()
        {
            // Arrange
            _orchestrator.Initialize();
            _orchestrator.Pause();

            // Act
            _orchestrator.Resume();

            // Assert
            _orchestrator.IsPaused.Should().BeFalse("game should not be paused after Resume()");
        }

        [Fact]
        public void Pause_Resume_CanToggleMultipleTimes()
        {
            // Arrange
            _orchestrator.Initialize();

            // Act & Assert
            _orchestrator.Pause();
            _orchestrator.IsPaused.Should().BeTrue();

            _orchestrator.Resume();
            _orchestrator.IsPaused.Should().BeFalse();

            _orchestrator.Pause();
            _orchestrator.IsPaused.Should().BeTrue();
        }

        #endregion

        #region UPDATE TESTS

        [Fact]
        public void Update_ThrowsInvalidOperationException_WhenNotInitialized()
        {
            // Act
            var act = () => _orchestrator.Update();

            // Assert
            act.Should().Throw<InvalidOperationException>(
                "Update should fail if Initialize() was not called");
        }

        [Fact]
        public void Update_CallsSceneUpdate_WhenNotPaused()
        {
            // Arrange
            _orchestrator.Initialize();
            var mockScene = new Mock<IScene>();
            _orchestrator.LoadScene(mockScene.Object);

            // Act
            _orchestrator.Update();

            // Assert
            mockScene.Verify(
                s => s.Update(),
                Times.Once(),
                "Update should call scene.Update() when not paused"
            );
        }

        [Fact]
        public void Update_DoesNotCallSceneUpdate_WhenPaused()
        {
            // Arrange
            _orchestrator.Initialize();
            var mockScene = new Mock<IScene>();
            _orchestrator.LoadScene(mockScene.Object);
            _orchestrator.Pause();

            // Act
            _orchestrator.Update();

            // Assert
            mockScene.Verify(
                s => s.Update(),
                Times.Never(),
                "Update should NOT call scene.Update() when paused"
            );
        }

        [Fact]
        public void Update_DoesNotThrow_WhenNoSceneLoaded()
        {
            // Arrange
            _orchestrator.Initialize();

            // Act
            var act = () => _orchestrator.Update();

            // Assert
            act.Should().NotThrow("Update should handle null scene gracefully");
        }

        #endregion

        #region DRAW TESTS

        [Fact]
        public void Draw_ThrowsInvalidOperationException_WhenNotInitialized()
        {
            // Act
            var act = () => _orchestrator.Draw();

            // Assert
            act.Should().Throw<InvalidOperationException>(
                "Draw should fail if Initialize() was not called");
        }

        [Fact]
        public void Draw_ClearsScreen_BeforeDrawingScene()
        {
            // Arrange
            _orchestrator.Initialize();
            var mockScene = new Mock<IScene>();
            _orchestrator.LoadScene(mockScene.Object);

            // Act
            _orchestrator.Draw();

            // Assert
            _mockGraphics.Verify(
                g => g.Cls(0),
                Times.Once(),
                "Draw should clear screen before drawing scene"
            );
        }

        [Fact]
        public void Draw_DrawsCurrentScene()
        {
            // Arrange
            _orchestrator.Initialize();
            var mockScene = new Mock<IScene>();
            _orchestrator.LoadScene(mockScene.Object);

            // Act
            _orchestrator.Draw();

            // Assert
            mockScene.Verify(
                s => s.Draw(),
                Times.Once(),
                "Draw should render current scene"
            );
        }

        [Fact]
        public void Draw_StillDrawsScene_WhenPaused()
        {
            // Arrange
            _orchestrator.Initialize();
            var mockScene = new Mock<IScene>();
            _orchestrator.LoadScene(mockScene.Object);
            _orchestrator.Pause();

            // Act
            _orchestrator.Draw();

            // Assert
            mockScene.Verify(
                s => s.Draw(),
                Times.Once(),
                "Draw should still render scene when paused (pause menu overlays)"
            );
        }

        [Fact]
        public void Draw_DoesNotThrow_WhenNoSceneLoaded()
        {
            // Arrange
            _orchestrator.Initialize();

            // Act
            var act = () => _orchestrator.Draw();

            // Assert
            act.Should().NotThrow("Draw should handle null scene gracefully");
        }

        #endregion
    }
}
