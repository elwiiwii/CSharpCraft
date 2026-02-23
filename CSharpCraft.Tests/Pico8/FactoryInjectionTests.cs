using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// Factory Injection Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests verify that GameOrchestrator routes object creation
    /// through IServiceFactory instead of using direct `new` calls.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    public class FactoryInjectionTests
    {
        #region FACTORY METHODS EXIST

        [Fact]
        public void IServiceFactory_HasCreateGraphicsOrchestrator()
        {
            var type = typeof(IServiceFactory);
            var method = type.GetMethod("CreateGraphicsOrchestrator");
            method.Should().NotBeNull(
                "IServiceFactory should have CreateGraphicsOrchestrator method");
        }

        [Fact]
        public void IServiceFactory_HasCreateAudioOrchestrator()
        {
            var type = typeof(IServiceFactory);
            var method = type.GetMethod("CreateAudioOrchestrator");
            method.Should().NotBeNull(
                "IServiceFactory should have CreateAudioOrchestrator method");
        }

        [Fact]
        public void ServiceFactory_CreateGraphicsOrchestrator_ReturnsInstance()
        {
            var factory = new ServiceFactory();
            var graphics = new Mock<IGraphicsAPI>().Object;
            var result = factory.CreateGraphicsOrchestrator(graphics);
            result.Should().NotBeNull();
            result.Should().BeOfType<GraphicsOrchestrator>();
        }

        [Fact]
        public void ServiceFactory_CreateAudioOrchestrator_ReturnsInstance()
        {
            var factory = new ServiceFactory();
            var audio = new Mock<IAudioAPI>().Object;
            var result = factory.CreateAudioOrchestrator(audio);
            result.Should().NotBeNull();
            result.Should().BeOfType<AudioOrchestrator>();
        }

        #endregion

        #region GAME ORCHESTRATOR USES FACTORY

        [Fact]
        public void GameOrchestrator_UsesFactory_ForGraphicsOrchestrator()
        {
            var mockFactory = new Mock<IServiceFactory>();
            var mockGraphicsAPI = new Mock<IGraphicsAPI>();
            var expectedOrch = new GraphicsOrchestrator(mockGraphicsAPI.Object);

            mockFactory
                .Setup(f => f.CreateGraphicsOrchestrator(It.IsAny<IGraphicsAPI>(), It.IsAny<IPaletteManager?>()))
                .Returns(expectedOrch);
            mockFactory
                .Setup(f => f.CreateAudioOrchestrator(It.IsAny<IAudioAPI>()))
                .Returns(new AudioOrchestrator(new Mock<IAudioAPI>().Object));
            mockFactory
                .Setup(f => f.CreatePauseMenuState(It.IsAny<IPauseMenuContext>()))
                .Returns((IPauseMenuContext ctx) => new PauseMenuState(ctx));

            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                mockGraphicsAPI.Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object,
                serviceFactory: mockFactory.Object);

            orchestrator.Graphics.Should().BeSameAs(expectedOrch,
                "GameOrchestrator should use the factory-created GraphicsOrchestrator");
            mockFactory.Verify(f => f.CreateGraphicsOrchestrator(
                mockGraphicsAPI.Object, null), Times.Once());
        }

        [Fact]
        public void GameOrchestrator_UsesFactory_ForAudioOrchestrator()
        {
            var mockFactory = new Mock<IServiceFactory>();
            var mockAudioAPI = new Mock<IAudioAPI>();
            var expectedOrch = new AudioOrchestrator(mockAudioAPI.Object);

            mockFactory
                .Setup(f => f.CreateGraphicsOrchestrator(It.IsAny<IGraphicsAPI>(), It.IsAny<IPaletteManager?>()))
                .Returns(new GraphicsOrchestrator(new Mock<IGraphicsAPI>().Object));
            mockFactory
                .Setup(f => f.CreateAudioOrchestrator(It.IsAny<IAudioAPI>()))
                .Returns(expectedOrch);
            mockFactory
                .Setup(f => f.CreatePauseMenuState(It.IsAny<IPauseMenuContext>()))
                .Returns((IPauseMenuContext ctx) => new PauseMenuState(ctx));

            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                mockAudioAPI.Object,
                new Mock<ISceneManager>().Object,
                serviceFactory: mockFactory.Object);

            orchestrator.Audio.Should().BeSameAs(expectedOrch,
                "GameOrchestrator should use the factory-created AudioOrchestrator");
            mockFactory.Verify(f => f.CreateAudioOrchestrator(mockAudioAPI.Object), Times.Once());
        }

        [Fact]
        public void GameOrchestrator_UsesFactory_ForPauseMenuState()
        {
            var mockFactory = new Mock<IServiceFactory>();
            var expectedState = new PauseMenuState(new Mock<IPauseMenuContext>().Object);

            mockFactory
                .Setup(f => f.CreateGraphicsOrchestrator(It.IsAny<IGraphicsAPI>(), It.IsAny<IPaletteManager?>()))
                .Returns(new GraphicsOrchestrator(new Mock<IGraphicsAPI>().Object));
            mockFactory
                .Setup(f => f.CreateAudioOrchestrator(It.IsAny<IAudioAPI>()))
                .Returns(new AudioOrchestrator(new Mock<IAudioAPI>().Object));
            mockFactory
                .Setup(f => f.CreatePauseMenuState(It.IsAny<IPauseMenuContext>()))
                .Returns(expectedState);

            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object,
                serviceFactory: mockFactory.Object);

            // The pause menu state is internal, but we can verify the factory was called
            mockFactory.Verify(f => f.CreatePauseMenuState(orchestrator), Times.Once());
        }

        [Fact]
        public void GameOrchestrator_DefaultFactory_WhenNoneProvided()
        {
            // When no factory is provided, GameOrchestrator should still work
            // (uses default ServiceFactory internally)
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            orchestrator.Graphics.Should().NotBeNull();
            orchestrator.Audio.Should().NotBeNull();
        }

        #endregion

        #region NO DIRECT NEW CALLS

        [Fact]
        public void GameOrchestrator_DoesNotDirectlyNewGraphicsOrchestrator()
        {
            // This test verifies injection works by confirming
            // the mock factory's return value is used, not a direct `new`.
            var mockFactory = new Mock<IServiceFactory>(MockBehavior.Strict);
            var graphicsOrch = new GraphicsOrchestrator(new Mock<IGraphicsAPI>().Object);
            var audioOrch = new AudioOrchestrator(new Mock<IAudioAPI>().Object);

            mockFactory
                .Setup(f => f.CreateGraphicsOrchestrator(It.IsAny<IGraphicsAPI>(), It.IsAny<IPaletteManager?>()))
                .Returns(graphicsOrch);
            mockFactory
                .Setup(f => f.CreateAudioOrchestrator(It.IsAny<IAudioAPI>()))
                .Returns(audioOrch);
            mockFactory
                .Setup(f => f.CreatePauseMenuState(It.IsAny<IPauseMenuContext>()))
                .Returns((IPauseMenuContext ctx) => new PauseMenuState(ctx));

            // If GameOrchestrator uses `new` directly instead of factory,
            // strict mock will throw for unexpected calls, or returned value won't match.
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object,
                serviceFactory: mockFactory.Object);

            orchestrator.Graphics.Should().BeSameAs(graphicsOrch);
            orchestrator.Audio.Should().BeSameAs(audioOrch);
        }

        #endregion
    }
}
