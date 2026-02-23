using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// Constructor Parameter Object Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests verify that GameHostContext groups FNA platform dependencies
    /// and that GameOrchestrator accepts it to reduce constructor params.
    /// Also verifies zombie field removal.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    public class GameHostContextTests
    {
        #region GAME HOST CONTEXT RECORD EXISTS

        [Fact]
        public void GameHostContext_IsARecord()
        {
            var type = typeof(GameHostContext);
            type.Should().NotBeNull("GameHostContext should exist as a type");
            type.IsClass.Should().BeTrue();

            // Records have a special <Clone>$ method
            var cloneMethod = type.GetMethod("<Clone>$");
            cloneMethod.Should().NotBeNull("GameHostContext should be a record type");
        }

        [Fact]
        public void GameHostContext_HasRequiredProperties()
        {
            var type = typeof(GameHostContext);
            type.GetProperty("Batch").Should().NotBeNull();
            type.GetProperty("Pixel").Should().NotBeNull();
            type.GetProperty("Graphics").Should().NotBeNull();
            type.GetProperty("GraphicsDevice").Should().NotBeNull();
            type.GetProperty("Window").Should().NotBeNull();
            type.GetProperty("TextureDictionary").Should().NotBeNull();
            type.GetProperty("Settings").Should().NotBeNull();
            type.GetProperty("InputBindings").Should().NotBeNull();
            type.GetProperty("Scenes").Should().NotBeNull();
        }

        #endregion

        #region GAME ORCHESTRATOR PRODUCTION CONSTRUCTOR

        [Fact]
        public void GameOrchestrator_ProductionConstructor_AcceptsGameHostContext()
        {
            // Verify the production constructor has a GameHostContext parameter
            var ctors = typeof(GameOrchestrator).GetConstructors();
            var productionCtor = ctors.FirstOrDefault(c =>
                c.GetParameters().Any(p => p.ParameterType == typeof(GameHostContext)));

            productionCtor.Should().NotBeNull(
                "GameOrchestrator should have a constructor accepting GameHostContext");
        }

        [Fact]
        public void GameOrchestrator_ProductionConstructor_HasReducedParameterCount()
        {
            var ctors = typeof(GameOrchestrator).GetConstructors();
            var productionCtor = ctors.FirstOrDefault(c =>
                c.GetParameters().Any(p => p.ParameterType == typeof(GameHostContext)));

            productionCtor.Should().NotBeNull();
            // GameHostContext + cart + inputManager + graphicsAPI + audioAPI + sceneManager 
            // + cartDataLoader + optional titleSceneFactory + optional paletteManager
            // + optional mapManager + optional serviceFactory
            // = 11 params max (down from 20)
            productionCtor!.GetParameters().Length.Should().BeLessOrEqualTo(11,
                "production constructor should have at most 11 params (down from 20)");
        }

        #endregion

        #region PROPERTIES STILL ACCESSIBLE

        [Fact]
        public void GameOrchestrator_StillExposes_Settings()
        {
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            // Settings is null in test constructor — that's fine, just verify the property exists
            var prop = typeof(GameOrchestrator).GetProperty("Settings");
            prop.Should().NotBeNull("Settings property should still be accessible");
        }

        [Fact]
        public void GameOrchestrator_StillExposes_Scenes()
        {
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            orchestrator.Scenes.Should().NotBeNull(
                "Scenes should still be accessible and default to empty list");
        }

        [Fact]
        public void GameOrchestrator_StillExposes_InputBindings()
        {
            var prop = typeof(GameOrchestrator).GetProperty("InputBindings");
            prop.Should().NotBeNull("InputBindings property should still be accessible");
        }

        [Fact]
        public void GameOrchestrator_StillExposes_Colors()
        {
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            orchestrator.Colors.Should().NotBeNull()
                .And.HaveCount(32, "should still have 32 default PICO-8 colors");
        }

        #endregion

        #region ZOMBIE FIELD REMOVAL

        [Fact]
        public void GameOrchestrator_DoesNotHave_CosDict()
        {
            var field = typeof(GameOrchestrator).GetField("_cosDict",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.Should().BeNull("_cosDict is a zombie field and should be removed");
        }

        [Fact]
        public void GameOrchestrator_DoesNotHave_SinDict()
        {
            var field = typeof(GameOrchestrator).GetField("_sinDict",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.Should().BeNull("_sinDict is a zombie field and should be removed");
        }

        [Fact]
        public void GameOrchestrator_DoesNotHave_Random()
        {
            var field = typeof(GameOrchestrator).GetField("_random",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.Should().BeNull("_random is a zombie field and should be removed");
        }

        [Fact]
        public void GameOrchestrator_DoesNotHave_SoundEffectDictionary()
        {
            var field = typeof(GameOrchestrator).GetField("_soundEffectDictionary",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.Should().BeNull(
                "_soundEffectDictionary is a zombie field — audio uses AudioAPI directly");
        }

        [Fact]
        public void GameOrchestrator_DoesNotHave_MusicDictionaryField()
        {
            var field = typeof(GameOrchestrator).GetField("_musicDictionary",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.Should().BeNull(
                "_musicDictionary is a zombie field — audio uses AudioAPI directly");
        }

        #endregion

        #region TEST CONSTRUCTOR UNCHANGED

        [Fact]
        public void TestConstructor_StillWorks_WithFourParams()
        {
            // The simplified test constructor should not change
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            orchestrator.Should().NotBeNull();
            orchestrator.Graphics.Should().NotBeNull();
            orchestrator.Audio.Should().NotBeNull();
        }

        #endregion
    }
}
