using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// State Unification Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests verify that CameraOffset, Cell, and Resolution each have exactly
    /// ONE canonical owner, eliminating the triple-copy duplication.
    /// 
    /// Canonical owners after unification:
    ///   CameraOffset → GraphicsOrchestrator (set via Camera API)
    ///   Cell          → GameOrchestrator (computed from viewport)
    ///   Resolution    → GameOrchestrator (set from scene)
    /// 
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    public class StateUnificationTests
    {
        #region CAMERA OFFSET IS OWNED BY GRAPHICS ORCHESTRATOR ONLY

        [Fact]
        public void GameOrchestrator_DoesNotExposeCameraOffset()
        {
            // GameOrchestrator should NOT have its own CameraOffset property.
            // CameraOffset is owned solely by GraphicsOrchestrator.
            var type = typeof(GameOrchestrator);
            type.GetProperty("CameraOffset").Should().BeNull(
                "CameraOffset should only exist on GraphicsOrchestrator, not GameOrchestrator");
        }

        [Fact]
        public void GraphicsOrchestrator_OwnsCameraOffset()
        {
            var graphicsAPI = new Mock<IGraphicsAPI>();
            var orch = new GraphicsOrchestrator(graphicsAPI.Object);
            orch.CameraOffset.x.Should().Be(F32.Zero);
            orch.CameraOffset.y.Should().Be(F32.Zero);
        }

        #endregion

        #region CELL AND RESOLUTION OWNED BY GAME ORCHESTRATOR ONLY

        [Fact]
        public void GraphicsOrchestrator_DoesNotExposeCell()
        {
            // Cell should be owned by GameOrchestrator, not GraphicsOrchestrator.
            var type = typeof(GraphicsOrchestrator);
            type.GetProperty("Cell").Should().BeNull(
                "Cell should only exist on GameOrchestrator, not GraphicsOrchestrator");
        }

        [Fact]
        public void GraphicsOrchestrator_DoesNotExposeResolution()
        {
            var type = typeof(GraphicsOrchestrator);
            type.GetProperty("Resolution").Should().BeNull(
                "Resolution should only exist on GameOrchestrator, not GraphicsOrchestrator");
        }

        [Fact]
        public void GraphicsOrchestrator_DoesNotHaveSetDisplayConfig()
        {
            var type = typeof(GraphicsOrchestrator);
            type.GetMethod("SetDisplayConfig").Should().BeNull(
                "SetDisplayConfig should not exist on GraphicsOrchestrator");
        }

        [Fact]
        public void GameOrchestrator_OwnsCell()
        {
            var orchestrator = CreateTestOrchestrator();
            orchestrator.Cell.Width.Should().Be(1, "default Cell is (1,1) representing 1:1 pixel mapping");
            orchestrator.Cell.Height.Should().Be(1);
        }

        [Fact]
        public void GameOrchestrator_OwnsResolution()
        {
            var orchestrator = CreateTestOrchestrator();
            orchestrator.Resolution.w.Should().Be(128, "default resolution is 128x128");
            orchestrator.Resolution.h.Should().Be(128);
        }

        [Fact]
        public void GameOrchestrator_SetDisplayConfig_UpdatesCellAndResolution()
        {
            var orchestrator = CreateTestOrchestrator();
            orchestrator.SetDisplayConfig((256, 192), (4, 3));

            orchestrator.Cell.Width.Should().Be(4);
            orchestrator.Cell.Height.Should().Be(3);
            orchestrator.Resolution.w.Should().Be(256);
            orchestrator.Resolution.h.Should().Be(192);
        }

        #endregion

        #region DEAD CODE REMOVED

        [Fact]
        public void GameOrchestrator_DoesNotExposeGameState()
        {
            var type = typeof(GameOrchestrator);
            type.GetProperty("GameState").Should().BeNull(
                "IGameState/GameStateContainer was dead code and should be removed");
        }

        [Fact]
        public void IGameState_InterfaceDoesNotExist()
        {
            // IGameState was dead code — verify it's removed from the assembly.
            var assembly = typeof(GameOrchestrator).Assembly;
            var type = assembly.GetType("CSharpCraft.Pico8.IGameState");
            type.Should().BeNull("IGameState was unused dead code and should be deleted");
        }

        [Fact]
        public void GameStateContainer_ClassDoesNotExist()
        {
            var assembly = typeof(GameOrchestrator).Assembly;
            var type = assembly.GetType("CSharpCraft.Pico8.GameStateContainer");
            type.Should().BeNull("GameStateContainer was unused dead code and should be deleted");
        }

        [Fact]
        public void IServiceFactory_DoesNotHaveCreateGameState()
        {
            var type = typeof(IServiceFactory);
            type.GetMethod("CreateGameState").Should().BeNull(
                "CreateGameState should be removed since GameStateContainer is dead code");
        }

        #endregion

        #region HELPERS

        private static GameOrchestrator CreateTestOrchestrator()
        {
            return new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);
        }

        #endregion
    }
}
