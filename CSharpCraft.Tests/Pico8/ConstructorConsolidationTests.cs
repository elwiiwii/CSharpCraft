using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// Constructor Consolidation Test Suite
    /// 
    /// Verifies:
    /// - Single unified constructor replaces the old production + test split
    /// - Null Object defaults (NullScene, InMemorySettings, DefaultInputBindings) are safe
    /// - No null! fragility remains
    /// - Both production-style and test-style construction work
    /// </summary>
    public class ConstructorConsolidationTests
    {
        #region NULL OBJECT TYPES

        [Fact]
        public void NullScene_IsSingleton()
        {
            var a = NullScene.Instance;
            var b = NullScene.Instance;
            a.Should().BeSameAs(b, "NullScene should be a singleton");
        }

        [Fact]
        public void NullScene_HasSafeDefaults()
        {
            var scene = NullScene.Instance;
            scene.SceneName.Should().Be("NullScene");
            scene.Fps.Should().BeGreaterThan(0);
            scene.Resolution.w.Should().BeGreaterThan(0);
            scene.Resolution.h.Should().BeGreaterThan(0);
            scene.SpriteData.Should().NotBeNull();
            scene.SpriteImage.Should().NotBeNull();
            scene.FlagData.Should().NotBeNull();
            scene.MapData.Should().NotBeNull();
            scene.Music.Should().NotBeNull();
            scene.Sfx.Should().NotBeNull();
        }

        [Fact]
        public void NullScene_OperationsAreNoOp()
        {
            var scene = NullScene.Instance;

            // Should not throw
            scene.Init();
            scene.Update();
            scene.Draw();
            scene.Dispose();
        }

        [Fact]
        public void InMemorySettings_HasSensibleDefaults()
        {
            var settings = InMemorySettings.Default;
            settings.SoundEnabled.Should().BeTrue();
            settings.MusicVolume.Should().BeGreaterThan(0);
            settings.SfxVolume.Should().BeGreaterThan(0);
            settings.WindowWidth.Should().BeGreaterThan(0);
            settings.WindowHeight.Should().BeGreaterThan(0);
        }

        [Fact]
        public void InMemorySettings_Save_DoesNotThrow()
        {
            var settings = new InMemorySettings();
            settings.SoundEnabled = false;
            settings.IsFullscreen = true;

            // Save should be a no-op but not throw
            var act = () => settings.Save();
            act.Should().NotThrow();
        }

        [Fact]
        public void InMemorySettings_StoresValues()
        {
            var settings = new InMemorySettings();
            settings.SoundEnabled = false;
            settings.MusicVolume = 42;
            settings.IsFullscreen = true;

            settings.SoundEnabled.Should().BeFalse();
            settings.MusicVolume.Should().Be(42);
            settings.IsFullscreen.Should().BeTrue();
        }

        [Fact]
        public void DefaultInputBindings_IsSingleton()
        {
            var a = DefaultInputBindings.Instance;
            var b = DefaultInputBindings.Instance;
            a.Should().BeSameAs(b, "DefaultInputBindings should be a singleton");
        }

        [Fact]
        public void DefaultInputBindings_AllBindingsAreNonNull()
        {
            var bindings = DefaultInputBindings.Instance;
            bindings.KeyboardLeft.Should().NotBeNull();
            bindings.KeyboardRight.Should().NotBeNull();
            bindings.KeyboardUp.Should().NotBeNull();
            bindings.KeyboardDown.Should().NotBeNull();
            bindings.KeyboardUse.Should().NotBeNull();
            bindings.KeyboardMenu.Should().NotBeNull();
            bindings.KeyboardPause.Should().NotBeNull();
            bindings.ControllerLeft.Should().NotBeNull();
            bindings.ControllerRight.Should().NotBeNull();
            bindings.ControllerUp.Should().NotBeNull();
            bindings.ControllerDown.Should().NotBeNull();
            bindings.ControllerUse.Should().NotBeNull();
            bindings.ControllerMenu.Should().NotBeNull();
            bindings.ControllerPause.Should().NotBeNull();
        }

        #endregion

        #region UNIFIED CONSTRUCTOR

        [Fact]
        public void GameOrchestrator_HasExactlyOneConstructor()
        {
            typeof(GameOrchestrator).GetConstructors()
                .Should().HaveCount(1, "should have a single unified constructor");
        }

        [Fact]
        public void UnifiedConstructor_FourRequiredParams_AllDefaultsSafe()
        {
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            // Every field should have a safe non-null default
            orchestrator.CurrentCart.Should().NotBeNull()
                .And.BeOfType<NullScene>("should default to NullScene");
            orchestrator.Settings.Should().NotBeNull()
                .And.BeOfType<InMemorySettings>("should default to InMemorySettings");
            orchestrator.InputBindings.Should().NotBeNull()
                .And.BeOfType<DefaultInputBindings>("should default to DefaultInputBindings");
            orchestrator.Scenes.Should().NotBeNull().And.BeEmpty();
            orchestrator.Colors.Should().HaveCount(32);
        }

        [Fact]
        public void UnifiedConstructor_WithCart_UsesProvidedCart()
        {
            var mockCart = new Mock<IScene>();
            mockCart.Setup(s => s.SceneName).Returns("TestCart");

            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object,
                cart: mockCart.Object);

            orchestrator.CurrentCart.Should().BeSameAs(mockCart.Object);
        }

        [Fact]
        public void UnifiedConstructor_WithOptionalParams_AllAccepted()
        {
            var mockPalette = new Mock<IPaletteManager>();
            var mockMap = new Mock<IMapManager>();
            var mockDisplay = new Mock<IDisplayManager>();
            mockDisplay.Setup(d => d.Resolution).Returns((128, 128));
            mockDisplay.Setup(d => d.Cell).Returns((8, 6));

            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object,
                paletteManager: mockPalette.Object,
                mapManager: mockMap.Object,
                displayManager: mockDisplay.Object);

            orchestrator.MapManager.Should().BeSameAs(mockMap.Object);
        }

        [Fact]
        public void UnifiedConstructor_ThrowsOnNull_InputManager()
        {
            var act = () => new GameOrchestrator(
                null!,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("inputManager");
        }

        [Fact]
        public void UnifiedConstructor_ThrowsOnNull_GraphicsAPI()
        {
            var act = () => new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                null!,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("graphicsAPI");
        }

        [Fact]
        public void UnifiedConstructor_ThrowsOnNull_AudioAPI()
        {
            var act = () => new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                null!,
                new Mock<ISceneManager>().Object);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("audioAPI");
        }

        [Fact]
        public void UnifiedConstructor_ThrowsOnNull_SceneManager()
        {
            var act = () => new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                null!);

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("sceneManager");
        }

        #endregion

        #region NO NULL BANG FRAGILITY

        [Fact]
        public void Settings_CanToggleSound_WithDefaultSettings()
        {
            // Previously this would NRE with null! settings
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            // ToggleSound needs Notifications.Current set (it shows a popup)
            Notifications.Current = new Mock<IPopupService>().Object;

            var act = () => orchestrator.ToggleSound();
            act.Should().NotThrow("InMemorySettings should handle ToggleSound safely");
        }

        [Fact]
        public void InputBindings_Accessible_WithDefaultBindings()
        {
            // Previously this would NRE with null! inputBindings
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            var bindings = orchestrator.InputBindings;
            bindings.Should().NotBeNull();
            bindings.KeyboardLeft.Should().NotBeNull();
        }

        #endregion
    }
}
