using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// GraphicsOrchestrator Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests define the graphics coordination contract: delegation to IGraphicsAPI,
    /// camera state tracking, and graphics-specific orchestration.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    public class GraphicsOrchestratorTests
    {
        private readonly Mock<IGraphicsAPI> _mockGraphics;
        private readonly GraphicsOrchestrator _orchestrator;

        public GraphicsOrchestratorTests()
        {
            _mockGraphics = new Mock<IGraphicsAPI>();
            _orchestrator = new GraphicsOrchestrator(_mockGraphics.Object);
        }

        #region CONSTRUCTOR TESTS

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenGraphicsIsNull()
        {
            // Act
            var act = () => new GraphicsOrchestrator(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("graphicsAPI");
        }

        [Fact]
        public void Constructor_StoresGraphicsAPI()
        {
            // Assert
            _orchestrator.API.Should().BeSameAs(_mockGraphics.Object,
                "constructor should store the IGraphicsAPI reference");
        }

        #endregion

        #region DELEGATION TESTS

        [Fact]
        public void Circ_DelegatesToGraphicsAPI()
        {
            // Arrange
            F32 x = F32.FromInt(64);
            F32 y = F32.FromInt(64);

            // Act
            _orchestrator.Circ(x, y, 8.0, 3);

            // Assert
            _mockGraphics.Verify(
                g => g.Circ(It.IsAny<F32>(), It.IsAny<F32>(), 8.0, 3),
                Times.Once(),
                "Circ should delegate to IGraphicsAPI.Circ"
            );
        }

        [Fact]
        public void Circfill_DelegatesToGraphicsAPI()
        {
            // Arrange
            F32 x = F32.FromInt(50);
            F32 y = F32.FromInt(50);

            // Act
            _orchestrator.Circfill(x, y, 5.0, 7);

            // Assert
            _mockGraphics.Verify(
                g => g.Circfill(It.IsAny<F32>(), It.IsAny<F32>(), 5.0, 7),
                Times.Once(),
                "Circfill should delegate to IGraphicsAPI.Circfill"
            );
        }

        [Fact]
        public void Rect_DelegatesToGraphicsAPI()
        {
            // Act
            _orchestrator.Rect(10.0, 20.0, 30.0, 40.0, 1.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Rect(10.0, 20.0, 30.0, 40.0, 1.0),
                Times.Once(),
                "Rect should delegate to IGraphicsAPI.Rect"
            );
        }

        [Fact]
        public void Rectfill_DelegatesToGraphicsAPI()
        {
            // Act
            _orchestrator.Rectfill(5.0, 5.0, 25.0, 25.0, 2.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Rectfill(5.0, 5.0, 25.0, 25.0, 2.0),
                Times.Once(),
                "Rectfill should delegate to IGraphicsAPI.Rectfill"
            );
        }

        [Fact]
        public void Cls_DelegatesToGraphicsAPI_WithDefaultColor()
        {
            // Act
            _orchestrator.Cls();

            // Assert
            _mockGraphics.Verify(
                g => g.Cls(0),
                Times.Once(),
                "Cls should delegate to IGraphicsAPI.Cls with default color 0"
            );
        }

        [Fact]
        public void Cls_DelegatesToGraphicsAPI_WithSpecifiedColor()
        {
            // Act
            _orchestrator.Cls(5);

            // Assert
            _mockGraphics.Verify(
                g => g.Cls(5),
                Times.Once(),
                "Cls should delegate to IGraphicsAPI.Cls with specified color"
            );
        }

        [Fact]
        public void Pset_DelegatesToGraphicsAPI()
        {
            // Arrange
            F32 x = F32.FromInt(10);
            F32 y = F32.FromInt(20);

            // Act
            _orchestrator.Pset(x, y, 3.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Pset(It.IsAny<F32>(), It.IsAny<F32>(), 3.0),
                Times.Once(),
                "Pset should delegate to IGraphicsAPI.Pset"
            );
        }

        #endregion

        #region CAMERA STATE TESTS

        [Fact]
        public void CameraOffset_DefaultsToZero()
        {
            // Assert
            _orchestrator.CameraOffset.x.Should().Be(F32.Zero,
                "camera X should default to zero");
            _orchestrator.CameraOffset.y.Should().Be(F32.Zero,
                "camera Y should default to zero");
        }

        [Fact]
        public void SetCamera_UpdatesCameraOffset()
        {
            // Arrange
            F32 x = F32.FromInt(16);
            F32 y = F32.FromInt(32);

            // Act
            _orchestrator.SetCamera(x, y);

            // Assert
            _orchestrator.CameraOffset.x.Should().Be(x,
                "camera X should be updated to 16");
            _orchestrator.CameraOffset.y.Should().Be(y,
                "camera Y should be updated to 32");
        }

        [Fact]
        public void ResetCamera_SetsCameraToZero()
        {
            // Arrange
            _orchestrator.SetCamera(F32.FromInt(100), F32.FromInt(200));

            // Act
            _orchestrator.ResetCamera();

            // Assert
            _orchestrator.CameraOffset.x.Should().Be(F32.Zero,
                "ResetCamera should set X to zero");
            _orchestrator.CameraOffset.y.Should().Be(F32.Zero,
                "ResetCamera should set Y to zero");
        }

        [Fact]
        public void SetCamera_OverwritesPreviousCamera()
        {
            // Arrange
            _orchestrator.SetCamera(F32.FromInt(10), F32.FromInt(20));

            // Act
            _orchestrator.SetCamera(F32.FromInt(50), F32.FromInt(60));

            // Assert
            _orchestrator.CameraOffset.x.Should().Be(F32.FromInt(50),
                "camera X should be overwritten to 50");
            _orchestrator.CameraOffset.y.Should().Be(F32.FromInt(60),
                "camera Y should be overwritten to 60");
        }

        #endregion

        #region DISPLAY CONFIG TESTS

        [Fact]
        public void GetColor_DelegatesToGraphicsAPI()
        {
            // Arrange
            var expectedColor = new Microsoft.Xna.Framework.Color(255, 0, 0);
            _mockGraphics.Setup(g => g.GetColor(8)).Returns(expectedColor);

            // Act
            var result = _orchestrator.GetColor(8);

            // Assert
            result.Should().Be(expectedColor,
                "GetColor should delegate to IGraphicsAPI.GetColor");
            _mockGraphics.Verify(g => g.GetColor(8), Times.Once());
        }

        [Fact]
        public void GetColor_WithDifferentIndex_DelegatesToGraphicsAPI()
        {
            // Arrange
            var white = new Microsoft.Xna.Framework.Color(255, 255, 255);
            _mockGraphics.Setup(g => g.GetColor(7)).Returns(white);

            // Act
            var result = _orchestrator.GetColor(7);

            // Assert
            result.Should().Be(white);
        }

        #endregion
    }
}
