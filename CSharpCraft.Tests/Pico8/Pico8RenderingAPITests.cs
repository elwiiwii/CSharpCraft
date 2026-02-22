using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// Pico8 Rendering API Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests define contracts for the high-level rendering methods:
    /// Print, Spr, Sspr, Map, Line, Pset.
    /// These are the most-used scene methods (Print: 131, Spr: 69, etc.)
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    [Collection("Pico8Static")]
    public class Pico8RenderingAPITests : IDisposable
    {
        private readonly Mock<IInputStateManager> _mockInput;
        private readonly Mock<IGraphicsAPI> _mockGraphics;
        private readonly Mock<IAudioAPI> _mockAudio;
        private readonly Mock<ISceneManager> _mockSceneManager;
        private readonly Mock<IPaletteManager> _mockPalette;
        private readonly Mock<IMapManager> _mockMap;
        private readonly GameOrchestrator _orchestrator;

        public Pico8RenderingAPITests()
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

        #region PRINT TESTS

        [Fact]
        public void Print_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Print("hello", 10.0, 20.0, 7.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Print("hello", 10.0, 20.0, 7.0),
                Times.Once(),
                "Print should delegate text rendering to IGraphicsAPI"
            );
        }

        [Fact]
        public void Print_WithDifferentParameters()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Print("world", 0.0, 0.0, 1.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Print("world", 0.0, 0.0, 1.0),
                Times.Once()
            );
        }

        #endregion

        #region SPR TESTS

        [Fact]
        public void Spr_DelegatesToGraphicsAPI_WithDefaults()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Spr(3.0, 16.0, 32.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Spr(3.0, 16.0, 32.0, 1.0, 1.0, false, false),
                Times.Once(),
                "Spr should delegate with default w=1, h=1, no flips"
            );
        }

        [Fact]
        public void Spr_WithFlip_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Spr(5.0, 8.0, 8.0, 2.0, 2.0, true, false);

            // Assert
            _mockGraphics.Verify(
                g => g.Spr(5.0, 8.0, 8.0, 2.0, 2.0, true, false),
                Times.Once(),
                "Spr should pass flip parameters through"
            );
        }

        #endregion

        #region SSPR TESTS

        [Fact]
        public void Sspr_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Sspr(0.0, 0.0, 16.0, 16.0, 32.0, 32.0, 32.0, 32.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Sspr(0.0, 0.0, 16.0, 16.0, 32.0, 32.0, 32.0, 32.0, false, false),
                Times.Once(),
                "Sspr should delegate sprite stretch to IGraphicsAPI"
            );
        }

        [Fact]
        public void Sspr_WithFlip_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Sspr(8.0, 8.0, 8.0, 8.0, 0.0, 0.0, 16.0, 16.0, true, true);

            // Assert
            _mockGraphics.Verify(
                g => g.Sspr(8.0, 8.0, 8.0, 8.0, 0.0, 0.0, 16.0, 16.0, true, true),
                Times.Once()
            );
        }

        #endregion

        #region MAP TESTS

        [Fact]
        public void Map_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Map(0.0, 0.0, 0.0, 0.0, 16.0, 16.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Map(0.0, 0.0, 0.0, 0.0, 16.0, 16.0, 0),
                Times.Once(),
                "Map should delegate tilemap rendering to IGraphicsAPI"
            );
        }

        [Fact]
        public void Map_WithFlags_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Map(2.0, 3.0, 16.0, 16.0, 8.0, 8.0, 1);

            // Assert
            _mockGraphics.Verify(
                g => g.Map(2.0, 3.0, 16.0, 16.0, 8.0, 8.0, 1),
                Times.Once()
            );
        }

        #endregion

        #region LINE TESTS

        [Fact]
        public void Line_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Line(
                F32.FromInt(0), F32.FromInt(0),
                F32.FromInt(127), F32.FromInt(127),
                7);

            // Assert
            _mockGraphics.Verify(
                g => g.Line(
                    F32.FromInt(0), F32.FromInt(0),
                    F32.FromInt(127), F32.FromInt(127),
                    7),
                Times.Once(),
                "Line should delegate to IGraphicsAPI"
            );
        }

        #endregion

        #region PSET TESTS

        [Fact]
        public void Pset_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Pset(F32.FromInt(64), F32.FromInt(64), 8.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Pset(F32.FromInt(64), F32.FromInt(64), 8.0),
                Times.Once(),
                "Pset should delegate pixel draw to IGraphicsAPI"
            );
        }

        #endregion

        #region MEMCPY TESTS

        [Fact]
        public void Memcpy_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Memcpy(0x1000, 0x2000, 0x1000);

            // Assert
            _mockGraphics.Verify(
                g => g.Memcpy(0x1000, 0x2000, 0x1000),
                Times.Once(),
                "Memcpy should delegate memory operations to IGraphicsAPI"
            );
        }

        #endregion

        #region RELOAD TESTS

        [Fact]
        public void Reload_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Reload();

            // Assert
            _mockGraphics.Verify(
                g => g.Reload(0, 0, 0, ""),
                Times.Once(),
                "Reload should delegate cart data reload to IGraphicsAPI"
            );
        }

        [Fact]
        public void Reload_WithParameters_DelegatesToGraphicsAPI()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Reload(0x1000, 0x2000, 0x1000, "data.p8");

            // Assert
            _mockGraphics.Verify(
                g => g.Reload(0x1000, 0x2000, 0x1000, "data.p8"),
                Times.Once()
            );
        }

        #endregion
    }
}
