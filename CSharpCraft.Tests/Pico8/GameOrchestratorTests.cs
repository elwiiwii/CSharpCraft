using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Create a properly configured mock IScene suitable for LoadCart/LoadScene.
        /// </summary>
        private Mock<IScene> CreateMockScene(string name = "TestScene")
        {
            var mockScene = new Mock<IScene>();
            mockScene.Setup(s => s.SceneName).Returns(name);
            mockScene.Setup(s => s.Fps).Returns(60.0);
            mockScene.Setup(s => s.Resolution).Returns((128, 128));
            mockScene.Setup(s => s.SpriteData).Returns("");
            mockScene.Setup(s => s.SpriteImage).Returns("");
            mockScene.Setup(s => s.FlagData).Returns("");
            mockScene.Setup(s => s.MapDimensions).Returns((0, 0));
            mockScene.Setup(s => s.MapData).Returns("");
            mockScene.Setup(s => s.Music).Returns(new Dictionary<string, List<SongInst>>());
            mockScene.Setup(s => s.Sfx).Returns(new Dictionary<string, Dictionary<int, string>>());
            return mockScene;
        }

        #region CONSTRUCTOR TESTS

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenInputManagerIsNull()
        {
            var act = () => new GameOrchestrator(
                null!,
                _mockGraphics.Object,
                _mockAudio.Object,
                _mockSceneManager.Object
            );
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("inputManager");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenGraphicsIsNull()
        {
            var act = () => new GameOrchestrator(
                _mockInput.Object,
                null!,
                _mockAudio.Object,
                _mockSceneManager.Object
            );
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("graphicsAPI");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenAudioIsNull()
        {
            var act = () => new GameOrchestrator(
                _mockInput.Object,
                _mockGraphics.Object,
                null!,
                _mockSceneManager.Object
            );
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("audioAPI");
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenSceneManagerIsNull()
        {
            var act = () => new GameOrchestrator(
                _mockInput.Object,
                _mockGraphics.Object,
                _mockAudio.Object,
                null!
            );
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("sceneManager");
        }

        [Fact]
        public void Constructor_SetsManagerProperties_Correctly()
        {
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
            _orchestrator.IsPaused.Should().BeFalse("orchestrator should not be paused on creation");
        }

        #endregion

        #region INITIALIZATION TESTS

        [Fact]
        public void Initialize_RegistersPico8StaticAPI()
        {
            _orchestrator.Initialize();
            _mockInput.Setup(i => i.Btn(0, 0)).Returns(true);
            var result = CSharpCraft.Pico8.Pico8.Btn(0);
            result.Should().BeTrue("Pico8 static API should work after Initialize()");
        }

        #endregion

        #region SCENE MANAGEMENT TESTS

        [Fact]
        public void LoadScene_SetsCurrentScene()
        {
            _orchestrator.Initialize();
            var mockScene = CreateMockScene();
            _orchestrator.LoadScene(mockScene.Object);
            _orchestrator.CurrentCart.Should().BeSameAs(mockScene.Object,
                "CurrentCart should reference the loaded scene");
        }

        [Fact]
        public void LoadScene_CallsInit_OnNewScene()
        {
            _orchestrator.Initialize();
            var mockScene = CreateMockScene();
            _orchestrator.LoadScene(mockScene.Object);
            mockScene.Verify(s => s.Init(), Times.Once(),
                "LoadScene should initialize the new scene");
        }

        [Fact]
        public void LoadScene_ThrowsArgumentNullException_WhenSceneIsNull()
        {
            _orchestrator.Initialize();
            var act = () => _orchestrator.LoadScene(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LoadScene_ReplacesCurrentScene()
        {
            _orchestrator.Initialize();
            var scene1 = CreateMockScene("Scene1");
            var scene2 = CreateMockScene("Scene2");
            _orchestrator.LoadScene(scene1.Object);
            _orchestrator.LoadScene(scene2.Object);
            _orchestrator.CurrentCart.Should().BeSameAs(scene2.Object,
                "LoadScene should replace the previous scene");
        }

        #endregion

        #region PAUSE STATE TESTS

        [Fact]
        public void Pause_SetsPausedState()
        {
            _orchestrator.Initialize();
            _orchestrator.Pause();
            _orchestrator.IsPaused.Should().BeTrue("game should be paused after Pause()");
        }

        [Fact]
        public void Resume_ClearsPausedState()
        {
            _orchestrator.Initialize();
            _orchestrator.Pause();
            _orchestrator.Resume();
            _orchestrator.IsPaused.Should().BeFalse("game should not be paused after Resume()");
        }

        [Fact]
        public void Pause_Resume_CanToggleMultipleTimes()
        {
            _orchestrator.Initialize();
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
            var act = () => _orchestrator.Update();
            act.Should().Throw<InvalidOperationException>(
                "Update should fail if Initialize() was not called");
        }

        #endregion

        #region DRAW TESTS

        [Fact]
        public void Draw_ThrowsInvalidOperationException_WhenNotInitialized()
        {
            var act = () => _orchestrator.Draw();
            act.Should().Throw<InvalidOperationException>(
                "Draw should fail if Initialize() was not called");
        }

        #endregion
    }
}
