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
