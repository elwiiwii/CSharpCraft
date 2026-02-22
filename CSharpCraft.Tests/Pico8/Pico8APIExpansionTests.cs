using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// Pico8 API Expansion Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests define contracts for the expanded static API:
    /// Camera, Palette, Scene management, Map operations, Data utilities.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    [Collection("Pico8Static")]
    public class Pico8APIExpansionTests : IDisposable
    {
        private readonly Mock<IInputStateManager> _mockInput;
        private readonly Mock<IGraphicsAPI> _mockGraphics;
        private readonly Mock<IAudioAPI> _mockAudio;
        private readonly Mock<ISceneManager> _mockSceneManager;
        private readonly Mock<IPaletteManager> _mockPalette;
        private readonly Mock<IMapManager> _mockMap;
        private readonly GameOrchestrator _orchestrator;

        public Pico8APIExpansionTests()
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
            // Clean up between tests
        }

        #region CAMERA TESTS

        [Fact]
        public void Camera_WithNoArgs_ResetsToZero()
        {
            // Arrange - set camera to non-zero first
            CSharpCraft.Pico8.Pico8.Camera(F32.FromInt(10), F32.FromInt(20));

            // Act
            CSharpCraft.Pico8.Pico8.Camera();

            // Assert
            _orchestrator.Graphics.CameraOffset.x.Should().Be(F32.Zero,
                "Camera() should reset X to zero");
            _orchestrator.Graphics.CameraOffset.y.Should().Be(F32.Zero,
                "Camera() should reset Y to zero");
        }

        [Fact]
        public void Camera_WithArgs_SetsCameraOffset()
        {
            // Arrange
            F32 x = F32.FromInt(16);
            F32 y = F32.FromInt(32);

            // Act
            CSharpCraft.Pico8.Pico8.Camera(x, y);

            // Assert
            _orchestrator.Graphics.CameraOffset.x.Should().Be(x,
                "Camera(x,y) should set X offset");
            _orchestrator.Graphics.CameraOffset.y.Should().Be(y,
                "Camera(x,y) should set Y offset");
        }

        #endregion

        #region PALETTE TESTS

        [Fact]
        public void Pal_WithNoArgs_ResetsPaletteViaOrchestrator()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Pal();

            // Assert
            _mockPalette.Verify(
                p => p.ResetPalette(),
                Times.Once(),
                "Pal() should reset palette"
            );
        }

        [Fact]
        public void Pal_WithArgs_SetsPaletteMapping()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Pal(1, 2);

            // Assert
            _mockPalette.Verify(
                p => p.SetPalette(1, 2),
                Times.Once(),
                "Pal(1,2) should map color 1 → color 2"
            );
        }

        [Fact]
        public void Palt_WithNoArgs_ResetsTransparency()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Palt();

            // Assert
            _mockPalette.Verify(
                p => p.ResetTransparency(),
                Times.Once(),
                "Palt() should reset all transparency"
            );
        }

        [Fact]
        public void Palt_WithArgs_SetsTransparency()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Palt(3, true);

            // Assert
            _mockPalette.Verify(
                p => p.SetTransparency(3, true),
                Times.Once(),
                "Palt(3, true) should make color 3 transparent"
            );
        }

        #endregion

        #region SCENE MANAGEMENT TESTS

        [Fact]
        public void ScheduleScene_DelegatesToSceneManager()
        {
            // Arrange
            var mockScene = new Mock<IScene>();
            Func<IScene> factory = () => mockScene.Object;

            // Act
            CSharpCraft.Pico8.Pico8.ScheduleScene(factory);

            // Assert
            _mockSceneManager.Verify(
                sm => sm.ScheduleScene(factory),
                Times.Once(),
                "ScheduleScene should delegate to ISceneManager"
            );
        }

        #endregion

        #region MAP DATA TESTS

        [Fact]
        public void Mget_DelegatesToMapManager()
        {
            // Arrange
            _mockMap.Setup(m => m.Mget(5.0, 10.0)).Returns(42);

            // Act
            var result = CSharpCraft.Pico8.Pico8.Mget(5.0, 10.0);

            // Assert
            result.Should().Be(42, "Mget should return tile from map manager");
        }

        [Fact]
        public void Mset_DelegatesToMapManager()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Mset(3.0, 4.0, 7.0);

            // Assert
            _mockMap.Verify(
                m => m.Mset(3.0, 4.0, 7.0),
                Times.Once(),
                "Mset should delegate to map manager"
            );
        }

        [Fact]
        public void Fget_DelegatesToMapManager()
        {
            // Arrange
            _mockMap.Setup(m => m.Fget(12)).Returns(5);

            // Act
            var result = CSharpCraft.Pico8.Pico8.Fget(12);

            // Assert
            result.Should().Be(5, "Fget should return flag data from map manager");
        }

        #endregion

        #region DATA UTILITY TESTS

        [Fact]
        public void Add_AppendsToList()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act
            var result = CSharpCraft.Pico8.Pico8.Add(list, 4);

            // Assert
            result.Should().Be(4, "Add should return the added value");
            list.Should().HaveCount(4).And.ContainInOrder(1, 2, 3, 4);
        }

        [Fact]
        public void Add_InsertsAtIndex()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act
            var result = CSharpCraft.Pico8.Pico8.Add(list, 99, 1);

            // Assert
            result.Should().Be(99);
            list.Should().ContainInOrder(1, 99, 2, 3);
        }

        [Fact]
        public void Del_RemovesFromList()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act
            CSharpCraft.Pico8.Pico8.Del(list, 2);

            // Assert
            list.Should().HaveCount(2).And.ContainInOrder(1, 3);
        }

        [Fact]
        public void Del_DoesNothing_WhenValueNotInList()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act
            CSharpCraft.Pico8.Pico8.Del(list, 99);

            // Assert
            list.Should().HaveCount(3, "Del should not modify list if value not found");
        }

        #endregion

        #region MATH UTILITY TESTS

        [Fact]
        public void Mod_ReturnsCorrectModulus()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Mod(F32.FromInt(7), 3);

            // Assert
            result.Should().Be(F32.FromInt(1), "7 mod 3 should equal 1");
        }

        [Fact]
        public void Mod_HandlesNegativeValues()
        {
            // Act — PICO-8 mod always returns non-negative
            var result = CSharpCraft.Pico8.Pico8.Mod(F32.FromInt(-1), 3);

            // Assert
            result.Should().Be(F32.FromInt(2), "PICO-8 mod(-1, 3) should equal 2 (always non-negative)");
        }

        [Fact]
        public void Srand_SetsRandomSeed_DoesNotThrow()
        {
            // Act & Assert
            var act = () => CSharpCraft.Pico8.Pico8.Srand(42);
            act.Should().NotThrow("Srand should accept any seed");
        }

        [Fact]
        public void Srand_MakesRndDeterministic()
        {
            // Arrange
            CSharpCraft.Pico8.Pico8.Srand(42);
            var first = CSharpCraft.Pico8.Pico8.Rnd(100);

            // Act
            CSharpCraft.Pico8.Pico8.Srand(42);
            var second = CSharpCraft.Pico8.Pico8.Rnd(100);

            // Assert
            second.Should().Be(first,
                "Same seed should produce same random sequence");
        }

        #endregion

        #region STUB METHOD TESTS

        [Fact]
        public void CartData_DoesNotThrow()
        {
            var act = () => CSharpCraft.Pico8.Pico8.CartData("test_id");
            act.Should().NotThrow("CartData is a stub");
        }

        [Fact]
        public void Cstore_DoesNotThrow()
        {
            var act = () => CSharpCraft.Pico8.Pico8.Cstore();
            act.Should().NotThrow("Cstore is a stub");
        }

        [Fact]
        public void Load_DoesNotThrow()
        {
            var act = () => CSharpCraft.Pico8.Pico8.Load("test.p8");
            act.Should().NotThrow("Load is a stub");
        }

        [Fact]
        public void Dget_ReturnsIndexAsF32()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Dget(3);

            // Assert (matches existing Pico8Functions behavior)
            result.Should().Be(F32.FromInt(3));
        }

        [Fact]
        public void Dset_DoesNotThrow()
        {
            var act = () => CSharpCraft.Pico8.Pico8.Dset(0, 1.0);
            act.Should().NotThrow("Dset is a stub");
        }

        #endregion
    }
}
