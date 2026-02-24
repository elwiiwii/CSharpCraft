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
            type.GetProperty("MusicDictionary").Should().NotBeNull();
            type.GetProperty("SoundEffectDictionary").Should().NotBeNull();
            type.GetProperty("Settings").Should().NotBeNull();
            type.GetProperty("InputBindings").Should().NotBeNull();
            type.GetProperty("Scenes").Should().NotBeNull();
        }

        #endregion

        #region GAME ORCHESTRATOR PRODUCTION CONSTRUCTOR

        [Fact]
        public void GameOrchestrator_ProductionConstructor_AcceptsGameHostContext()
        {
            // Verify the unified constructor has a GameHostContext parameter
            var ctors = typeof(GameOrchestrator).GetConstructors();
            var unifiedCtor = ctors.FirstOrDefault(c =>
                c.GetParameters().Any(p => p.ParameterType == typeof(GameHostContext)));

            unifiedCtor.Should().NotBeNull(
                "GameOrchestrator should have a constructor accepting GameHostContext");
        }

        [Fact]
        public void GameOrchestrator_HasSingleUnifiedConstructor()
        {
            var ctors = typeof(GameOrchestrator).GetConstructors();
            ctors.Should().HaveCount(1,
                "GameOrchestrator should have exactly one unified constructor");
        }

        [Fact]
        public void GameOrchestrator_UnifiedConstructor_HasReducedParameterCount()
        {
            var ctors = typeof(GameOrchestrator).GetConstructors();
            var ctor = ctors.Single();

            // 4 required + 8 optional = 12 params
            // (inputManager + graphicsAPI + audioAPI + sceneManager
            //  + cartDataLoader? + host? + cart? + titleSceneFactory?
            //  + paletteManager? + mapManager? + serviceFactory? + displayManager?)
            ctor.GetParameters().Length.Should().BeLessOrEqualTo(12,
                "unified constructor should have at most 12 params (4 required + 8 optional)");
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

            // Settings now defaults to InMemorySettings — always non-null
            orchestrator.Settings.Should().NotBeNull("Settings should always be available via Null Object default");
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

        #region UNIFIED CONSTRUCTOR WORKS WITH MINIMAL PARAMS

        [Fact]
        public void UnifiedConstructor_StillWorks_WithFourParams()
        {
            // The unified constructor should work with just the 4 required params
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            orchestrator.Should().NotBeNull();
            orchestrator.Graphics.Should().NotBeNull();
            orchestrator.Audio.Should().NotBeNull();
        }

        [Fact]
        public void UnifiedConstructor_NoNullBangDefaults()
        {
            // Verify that settings, inputBindings, scenes, and currentCart
            // all get safe Null Object defaults — no null! fragility
            var orchestrator = new GameOrchestrator(
                new Mock<IInputStateManager>().Object,
                new Mock<IGraphicsAPI>().Object,
                new Mock<IAudioAPI>().Object,
                new Mock<ISceneManager>().Object);

            orchestrator.Settings.Should().NotBeNull("Settings should default to InMemorySettings");
            orchestrator.InputBindings.Should().NotBeNull("InputBindings should default to DefaultInputBindings");
            orchestrator.Scenes.Should().NotBeNull("Scenes should default to empty list");
            orchestrator.CurrentCart.Should().NotBeNull("CurrentCart should default to NullScene");
        }

        #endregion
    }
}
