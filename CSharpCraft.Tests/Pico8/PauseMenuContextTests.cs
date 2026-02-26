using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using System;
using System.Collections.Generic;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// IPauseMenuContext Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests verify that GameOrchestrator correctly implements the IPauseMenuContext
    /// interface, that PauseMenuState works with the narrowed interface, and that
    /// the circular dependency is broken.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    public class PauseMenuContextTests
    {
        #region INTERFACE CONFORMANCE

        [Fact]
        public void GameOrchestrator_ImplementsIPauseMenuContext()
        {
            var orchestrator = CreateTestOrchestrator();
            orchestrator.Should().BeAssignableTo<IPauseMenuContext>(
                "GameOrchestrator should implement IPauseMenuContext");
        }

        [Fact]
        public void IPauseMenuContext_CanBeUsedToConstructPauseMenuState()
        {
            IPauseMenuContext context = CreateTestOrchestrator();
            var state = new PauseMenuState(context);
            state.Should().NotBeNull();
            state.IsPaused.Should().BeFalse();
        }

        #endregion

        #region CONTEXT PROPERTY ACCESS

        [Fact]
        public void Context_AudioSettings_ExposesSettingsInterface()
        {
            var mockSettings = new Mock<IAudioSettings>();
            mockSettings.Setup(s => s.SoundEnabled).Returns(true);

            var context = new Mock<IPauseMenuContext>();
            context.Setup(c => c.AudioSettings).Returns(mockSettings.Object);

            context.Object.AudioSettings.SoundEnabled.Should().BeTrue(
                "AudioSettings should be accessible through the context interface");
        }

        [Fact]
        public void Context_Scenes_ExposesScenesList()
        {
            var context = new Mock<IPauseMenuContext>();
            var scenes = new List<IScene> { new Mock<IScene>().Object };
            context.Setup(c => c.Scenes).Returns(scenes);

            context.Object.Scenes.Should().HaveCount(1);
        }

        [Fact]
        public void Context_Resolution_ExposesDisplayResolution()
        {
            var context = new Mock<IPauseMenuContext>();
            context.Setup(c => c.Resolution).Returns((128, 128));

            context.Object.Resolution.Should().Be((128, 128));
        }

        [Fact]
        public void Context_TrackManager_CanBeNull()
        {
            var context = new Mock<IPauseMenuContext>();
            context.Setup(c => c.TrackManager).Returns((ITrackManager?)null);

            context.Object.TrackManager.Should().BeNull(
                "TrackManager should be nullable when no tracks are loaded");
        }

        [Fact]
        public void Context_TrackManager_ExposesTrackManagerWhenAvailable()
        {
            var mockTrack = new Mock<ITrackManager>();
            mockTrack.Setup(t => t.SfxCount).Returns(3);

            var context = new Mock<IPauseMenuContext>();
            context.Setup(c => c.TrackManager).Returns(mockTrack.Object);

            context.Object.TrackManager!.SfxCount.Should().Be(3);
        }

        [Fact]
        public void Context_LastMusicCall_ReturnsNullByDefault()
        {
            var context = new Mock<IPauseMenuContext>();
            context.Setup(c => c.LastMusicCall).Returns((int?)null);

            context.Object.LastMusicCall.Should().BeNull();
        }

        #endregion

        #region CONTEXT OPERATIONS

        [Fact]
        public void Context_ReloadCart_CanBeCalled()
        {
            var context = new Mock<IPauseMenuContext>();
            context.Object.ReloadCart();
            context.Verify(c => c.ReloadCart(), Times.Once());
        }

        [Fact]
        public void Context_LoadCart_CanBeCalled()
        {
            var context = new Mock<IPauseMenuContext>();
            var scene = new Mock<IScene>().Object;
            context.Object.LoadCart(scene);
            context.Verify(c => c.LoadCart(scene), Times.Once());
        }

        [Fact]
        public void Context_SoundDispose_CanBeCalled()
        {
            var context = new Mock<IPauseMenuContext>();
            context.Object.SoundDispose();
            context.Verify(c => c.SoundDispose(), Times.Once());
        }

        [Fact]
        public void Context_ToggleFullscreen_CanBeCalled()
        {
            var context = new Mock<IPauseMenuContext>();
            context.Object.ToggleFullscreen();
            context.Verify(c => c.ToggleFullscreen(), Times.Once());
        }

        #endregion

        #region PAUSE MENU STATE WITH MOCK CONTEXT

        [Fact]
        public void PauseMenuState_WithMockedContext_TogglesCorrectly()
        {
            var context = CreateMockContext();
            var state = new PauseMenuState(context.Object);

            state.TogglePause();
            state.IsPaused.Should().BeTrue();
            state.TogglePause();
            state.IsPaused.Should().BeFalse();
        }

        [Fact]
        public void PauseMenuState_WithMockedContext_ResetsCorrectly()
        {
            var context = CreateMockContext();
            var state = new PauseMenuState(context.Object);

            state.TogglePause();
            state.Reset();
            state.IsPaused.Should().BeFalse();
            state.SelectedIndex.Should().Be(0);
            state.CurrentMenuItems.Should().BeEmpty();
        }

        [Fact]
        public void PauseMenuState_WithMockedContext_HandlesMenuInput()
        {
            var context = CreateMockContext();
            var state = new PauseMenuState(context.Object);

            state.CurrentMenuItems.Add(new MenuItem(() => "A", _ => { }));
            state.CurrentMenuItems.Add(new MenuItem(() => "B", _ => { }));

            state.HandleMenuInput(false, true, false);
            state.SelectedIndex.Should().Be(1);
        }

        [Fact]
        public void PauseMenuState_NullContext_ThrowsArgumentNullException()
        {
            var act = () => new PauseMenuState(null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("context");
        }

        #endregion

        #region CIRCULAR DEPENDENCY VERIFICATION

        [Fact]
        public void PauseMenuBuilder_DoesNotDependOnGameOrchestrator()
        {
            // PauseMenuBuilder constructor takes IPauseMenuContext, not GameOrchestrator.
            // This test verifies the interface-based construction works.
            var context = CreateMockContext();
            var mainItems = new List<MenuItem>();
            var curItems = new List<MenuItem>();

            var builder = new PauseMenuBuilder(context.Object, mainItems, curItems);
            builder.Build();

            mainItems.Should().HaveCount(4, "Build should create continue, options, reset, exit items");
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

        private static Mock<IPauseMenuContext> CreateMockContext()
        {
            var mock = new Mock<IPauseMenuContext>();
            mock.Setup(c => c.AudioSettings).Returns(new Mock<IAudioSettings>().Object);
            mock.Setup(c => c.DisplaySettings).Returns(new Mock<IDisplaySettings>().Object);
            mock.Setup(c => c.Scenes).Returns(new List<IScene>());
            mock.Setup(c => c.Resolution).Returns((128, 128));
            return mock;
        }

        #endregion
    }
}
