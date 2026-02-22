using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// Pico8 Display Config API Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests define contracts for platform property access:
    /// Cell size, Resolution, and Palette Color lookup.
    /// These replace direct access to p8.Cell, p8.Resolution, p8.Colors
    /// with clean static API delegations.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    [Collection("Pico8Static")]
    public class Pico8DisplayConfigTests : IDisposable
    {
        private readonly Mock<IInputStateManager> _mockInput;
        private readonly Mock<IGraphicsAPI> _mockGraphics;
        private readonly Mock<IAudioAPI> _mockAudio;
        private readonly Mock<ISceneManager> _mockSceneManager;
        private readonly Mock<IPaletteManager> _mockPalette;
        private readonly Mock<IMapManager> _mockMap;
        private readonly GameOrchestrator _orchestrator;

        public Pico8DisplayConfigTests()
        {
            _mockInput = new Mock<IInputStateManager>();
            _mockGraphics = new Mock<IGraphicsAPI>();
            _mockAudio = new Mock<IAudioAPI>();
            _mockSceneManager = new Mock<ISceneManager>();
            _mockPalette = new Mock<IPaletteManager>();
            _mockMap = new Mock<IMapManager>();

            _orchestrator = new GameOrchestrator(
                _mockInput.Object,
                _mockGraphics.Object,
                _mockAudio.Object,
                _mockSceneManager.Object,
                paletteManager: _mockPalette.Object,
                mapManager: _mockMap.Object
            );

            CSharpCraft.Pico8.Pico8.Initialize(_orchestrator);
        }

        public void Dispose()
        {
        }

        #region CELL SIZE TESTS

        [Fact]
        public void CellWidth_ReturnsDefaultCellWidth()
        {
            // Act & Assert
            CSharpCraft.Pico8.Pico8.CellWidth.Should().Be(1,
                "default cell width should be 1 (1:1 pixel mapping)");
        }

        [Fact]
        public void CellHeight_ReturnsDefaultCellHeight()
        {
            // Act & Assert
            CSharpCraft.Pico8.Pico8.CellHeight.Should().Be(1,
                "default cell height should be 1 (1:1 pixel mapping)");
        }

        [Fact]
        public void CellWidth_ReflectsDisplayConfigChange()
        {
            // Arrange
            _orchestrator.Graphics.SetDisplayConfig((128, 128), (4, 4));

            // Act & Assert
            CSharpCraft.Pico8.Pico8.CellWidth.Should().Be(4,
                "CellWidth should reflect the orchestrator's display config");
        }

        [Fact]
        public void CellHeight_ReflectsDisplayConfigChange()
        {
            // Arrange
            _orchestrator.Graphics.SetDisplayConfig((128, 128), (3, 5));

            // Act & Assert
            CSharpCraft.Pico8.Pico8.CellHeight.Should().Be(5,
                "CellHeight should reflect the orchestrator's display config");
        }

        #endregion

        #region RESOLUTION TESTS

        [Fact]
        public void ResolutionWidth_ReturnsDefault128()
        {
            // Act & Assert
            CSharpCraft.Pico8.Pico8.ResolutionWidth.Should().Be(128,
                "default resolution width should be 128 (PICO-8 standard)");
        }

        [Fact]
        public void ResolutionHeight_ReturnsDefault128()
        {
            // Act & Assert
            CSharpCraft.Pico8.Pico8.ResolutionHeight.Should().Be(128,
                "default resolution height should be 128 (PICO-8 standard)");
        }

        [Fact]
        public void ResolutionWidth_ReflectsDisplayConfigChange()
        {
            // Arrange
            _orchestrator.Graphics.SetDisplayConfig((256, 192), (2, 2));

            // Act & Assert
            CSharpCraft.Pico8.Pico8.ResolutionWidth.Should().Be(256,
                "ResolutionWidth should reflect the orchestrator's display config");
        }

        [Fact]
        public void ResolutionHeight_ReflectsDisplayConfigChange()
        {
            // Arrange
            _orchestrator.Graphics.SetDisplayConfig((256, 192), (2, 2));

            // Act & Assert
            CSharpCraft.Pico8.Pico8.ResolutionHeight.Should().Be(192,
                "ResolutionHeight should reflect the orchestrator's display config");
        }

        #endregion

        #region COLOR LOOKUP TESTS

        [Fact]
        public void GetColor_DelegatesToGraphicsOrchestrator()
        {
            // Arrange
            var red = new Color(255, 0, 77);
            _mockGraphics.Setup(g => g.GetColor(8)).Returns(red);

            // Act
            var result = CSharpCraft.Pico8.Pico8.GetColor(8);

            // Assert
            result.Should().Be(red,
                "GetColor should delegate through orchestrator to IGraphicsAPI");
            _mockGraphics.Verify(g => g.GetColor(8), Times.Once());
        }

        [Fact]
        public void GetColor_WithPaletteIndex0_ReturnsBlack()
        {
            // Arrange
            var black = new Color(0, 0, 0);
            _mockGraphics.Setup(g => g.GetColor(0)).Returns(black);

            // Act
            var result = CSharpCraft.Pico8.Pico8.GetColor(0);

            // Assert
            result.Should().Be(black);
        }

        [Fact]
        public void GetColor_WithPaletteIndex7_ReturnsWhite()
        {
            // Arrange
            var white = new Color(255, 236, 214);
            _mockGraphics.Setup(g => g.GetColor(7)).Returns(white);

            // Act
            var result = CSharpCraft.Pico8.Pico8.GetColor(7);

            // Assert
            result.Should().Be(white,
                "palette index 7 should return configured color");
        }

        [Fact]
        public void GetColor_MultipleCalls_EachDelegatesToAPI()
        {
            // Arrange
            _mockGraphics.Setup(g => g.GetColor(It.IsAny<int>()))
                .Returns<int>(i => new Color(i * 10, i * 10, i * 10));

            // Act
            var c0 = CSharpCraft.Pico8.Pico8.GetColor(0);
            var c7 = CSharpCraft.Pico8.Pico8.GetColor(7);
            var c15 = CSharpCraft.Pico8.Pico8.GetColor(15);

            // Assert
            c0.Should().Be(new Color(0, 0, 0));
            c7.Should().Be(new Color(70, 70, 70));
            c15.Should().Be(new Color(150, 150, 150));
            _mockGraphics.Verify(g => g.GetColor(It.IsAny<int>()), Times.Exactly(3));
        }

        #endregion
    }
}
