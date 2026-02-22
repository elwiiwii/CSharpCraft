using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// Pico8 Static API Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests define the complete contract for the static Pico8 API class.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    [Collection("Pico8Static")]
    public class Pico8StaticAPITests : IDisposable
    {
        private readonly Mock<IInputStateManager> _mockInput;
        private readonly Mock<IGraphicsAPI> _mockGraphics;
        private readonly Mock<IAudioAPI> _mockAudio;
        private readonly Mock<ISceneManager> _mockSceneManager;
        private readonly GameOrchestrator _gameOrchestrator;

        public Pico8StaticAPITests()
        {
            _mockInput = new Mock<IInputStateManager>();
            _mockGraphics = new Mock<IGraphicsAPI>();
            _mockAudio = new Mock<IAudioAPI>();
            _mockSceneManager = new Mock<ISceneManager>();

            _gameOrchestrator = new GameOrchestrator(
                _mockInput.Object,
                _mockGraphics.Object,
                _mockAudio.Object,
                _mockSceneManager.Object
            );

            CSharpCraft.Pico8.Pico8.Initialize(_gameOrchestrator);
        }

        public void Dispose()
        {
            // Clean up static state between tests
        }

        #region INPUT TESTS

        [Fact]
        public void Btn_DelegatesTo_InputState_Correctly()
        {
            // Arrange
            _mockInput.Setup(i => i.Btn(4, 0)).Returns(true);

            // Act
            var result = CSharpCraft.Pico8.Pico8.Btn(4);

            // Assert
            result.Should().BeTrue("button 4 was pressed");
            _mockInput.Verify(
                i => i.Btn(4, 0),
                Times.Once(),
                "Btn should query InputStateManager exactly once"
            );
        }

        [Fact]
        public void Btn_WithPlayer_DelegatesCorrectly()
        {
            // Arrange
            _mockInput.Setup(i => i.Btn(2, 1)).Returns(false);

            // Act
            var result = CSharpCraft.Pico8.Pico8.Btn(2, 1);

            // Assert
            result.Should().BeFalse("button for player 1 was not pressed");
            _mockInput.Verify(i => i.Btn(2, 1), Times.Once());
        }

        [Fact]
        public void Btn_WithInvalidButton_ReturnsFalse()
        {
            // Arrange
            _mockInput.Setup(i => i.Btn(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            // Act
            var result = CSharpCraft.Pico8.Pico8.Btn(99);

            // Assert
            result.Should().BeFalse("invalid button should return false");
        }

        [Fact]
        public void Btnp_DelegatesTo_InputState_ForPressThisFrame()
        {
            // Arrange
            _mockInput.Setup(i => i.Btnp(3, 0)).Returns(true);

            // Act
            var result = CSharpCraft.Pico8.Pico8.Btnp(3);

            // Assert
            result.Should().BeTrue("button was pressed this frame");
            _mockInput.Verify(i => i.Btnp(3, 0), Times.Once());
        }

        [Fact]
        public void Btnp_WithPlayer_DelegatesCorrectly()
        {
            // Arrange
            _mockInput.Setup(i => i.Btnp(1, 1)).Returns(true);

            // Act
            var result = CSharpCraft.Pico8.Pico8.Btnp(1, 1);

            // Assert
            result.Should().BeTrue();
            _mockInput.Verify(i => i.Btnp(1, 1), Times.Once());
        }

        #endregion

        #region GRAPHICS TESTS

        [Fact]
        public void Cls_ClearsScreen_WithDefaultColor()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Cls();

            // Assert
            _mockGraphics.Verify(
                g => g.Cls(0),
                Times.Once(),
                "Cls should clear screen with default color 0"
            );
        }

        [Fact]
        public void Cls_ClearsScreen_WithSpecifiedColor()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Cls(3);

            // Assert
            _mockGraphics.Verify(
                g => g.Cls(3),
                Times.Once(),
                "Cls should clear screen with color 3"
            );
        }

        [Fact]
        public void Circ_DrawsCircle_WithCorrectParameters()
        {
            // Arrange
            F32 x = F32.FromInt(64);
            F32 y = F32.FromInt(64);

            // Act
            CSharpCraft.Pico8.Pico8.Circ(x, y, 8, 3);

            // Assert
            _mockGraphics.Verify(
                g => g.Circ(It.IsAny<F32>(), It.IsAny<F32>(), 8.0, 3),
                Times.Once(),
                "Circ should draw circle at (64,64) with radius 8 and color 3"
            );
        }

        [Fact]
        public void Circfill_DrawsFilledCircle_WithCorrectParameters()
        {
            // Arrange
            F32 x = F32.FromInt(50);
            F32 y = F32.FromInt(50);

            // Act
            CSharpCraft.Pico8.Pico8.Circfill(x, y, 5, 7);

            // Assert
            _mockGraphics.Verify(
                g => g.Circfill(It.IsAny<F32>(), It.IsAny<F32>(), 5.0, 7),
                Times.Once(),
                "Circfill should draw filled circle at (50,50) with radius 5"
            );
        }

        [Fact]
        public void Rect_DrawsRectangleOutline()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Rect(10.0, 20.0, 30.0, 40.0, 1.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Rect(10.0, 20.0, 30.0, 40.0, 1.0),
                Times.Once(),
                "Rect should draw rectangle outline from (10,20) to (30,40) with color 1"
            );
        }

        [Fact]
        public void Rectfill_DrawsFilledRectangle()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Rectfill(5.0, 5.0, 25.0, 25.0, 2.0);

            // Assert
            _mockGraphics.Verify(
                g => g.Rectfill(5.0, 5.0, 25.0, 25.0, 2.0),
                Times.Once(),
                "Rectfill should draw filled rectangle from (5,5) to (25,25) with color 2"
            );
        }

        #endregion

        #region AUDIO TESTS

        [Fact]
        public void Sfx_PlaysSoundEffect_WithDefaultChannel()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Sfx(2.0);

            // Assert
            _mockAudio.Verify(
                a => a.Sfx(2.0, -1.0, 0.0, 31.0),
                Times.Once(),
                "Sfx should play sound 2 on auto channel"
            );
        }

        [Fact]
        public void Sfx_PlaysSoundEffect_OnSpecificChannel()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Sfx(5.0, 2.0);

            // Assert
            _mockAudio.Verify(
                a => a.Sfx(5.0, 2.0, 0.0, 31.0),
                Times.Once(),
                "Sfx should play sound 5 on channel 2"
            );
        }

        [Fact]
        public void Music_PlaysMusic_WithFadeout()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Music(1, 1000);

            // Assert
            _mockAudio.Verify(
                a => a.Music(1, 1000),
                Times.Once(),
                "Music should play track 1 with 1000ms fadeout"
            );
        }

        [Fact]
        public void Music_PlaysMusic_WithoutFadeout()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Music(3);

            // Assert
            _mockAudio.Verify(
                a => a.Music(3, 0),
                Times.Once()
            );
        }

        [Fact]
        public void Mute_MutesAllAudio()
        {
            // Act
            CSharpCraft.Pico8.Pico8.Mute();

            // Assert
            _mockAudio.Verify(
                a => a.Mute(),
                Times.Once(),
                "Mute should mute all audio"
            );
        }

        #endregion

        #region MATH TESTS

        [Fact]
        public void Cos_ReturnsCorrectValue_At0()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Cos(0);

            // Assert
            result.Should().Be(1.0f, "Cos(0) should equal 1.0");
        }

        [Fact]
        public void Cos_ReturnsCorrectValue_At0Point25()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Cos(0.25f);

            // Assert
            result.Should().BeApproximately(0.0f, 0.01f, "Cos(0.25) should be approximately 0");
        }

        [Fact]
        public void Sin_ReturnsCorrectValue_At0()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Sin(0);

            // Assert
            result.Should().BeApproximately(0.0f, 0.01f, "Sin(0) should equal approximately 0");
        }

        [Fact]
        public void Sin_ReturnsCorrectValue_At0Point25()
        {
            // PICO-8 sin is inverted: sin(0.25) = -1
            // Act
            var result = CSharpCraft.Pico8.Pico8.Sin(0.25f);

            // Assert
            result.Should().BeApproximately(-1.0f, 0.01f, "Sin(0.25) should be approximately -1.0 (PICO-8 inverted)");
        }

        [Fact]
        public void Rnd_ReturnsValueInRange_0To1()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Rnd(1);

            // Assert
            result.Float.Should()
                .BeGreaterThanOrEqualTo(0, "Rnd result should be >= 0")
                .And.BeLessThan(1, "Rnd result should be < 1");
        }

        [Fact]
        public void Rnd_ReturnsValueInRange_0ToMax()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Rnd(10);

            // Assert
            result.Float.Should()
                .BeGreaterThanOrEqualTo(0)
                .And.BeLessThan(10, "Rnd(10) should return value in [0, 10)");
        }

        [Fact]
        public void Mid_ReturnsMiddleValue()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Mid(5, 15, 10);

            // Assert
            result.Should().Be(10, "Mid(5, 15, 10) should return 10 (the middle value)");
        }

        [Fact]
        public void Mid_ReturnsMin_WhenValueBelowRange()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Mid(5, 15, 2);

            // Assert
            result.Should().Be(5, "Mid should return 5 when 2 is below range");
        }

        [Fact]
        public void Mid_ReturnsMax_WhenValueAboveRange()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Mid(5, 15, 20);

            // Assert
            result.Should().Be(15, "Mid should return 15 when 20 is above range");
        }

        [Fact]
        public void Flr_FloorsFractionalValue()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Flr(3.7f);

            // Assert
            result.Should().Be(3f, "Flr(3.7) should return 3");
        }

        [Fact]
        public void Flr_ReturnsInteger_ForIntegerInput()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Flr(5.0f);

            // Assert
            result.Should().Be(5f, "Flr(5.0) should return 5");
        }

        [Fact]
        public void Flr_ReturnsInteger_ForNegativeValue()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Flr(-2.3f);

            // Assert
            result.Should().Be(-3f, "Flr(-2.3) should return -3");
        }

        [Fact]
        public void Abs_ReturnsAbsoluteValue()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Abs(-5.0f);

            // Assert
            result.Should().Be(5.0f, "Abs(-5) should return 5");
        }

        [Fact]
        public void Sgn_ReturnsPositive_ForPositiveInput()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Sgn(42f);

            // Assert
            result.Should().Be(1, "Sgn of positive should return 1");
        }

        [Fact]
        public void Sgn_ReturnsNegative_ForNegativeInput()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Sgn(-7f);

            // Assert
            result.Should().Be(-1, "Sgn of negative should return -1");
        }

        [Fact]
        public void Sgn_ReturnsZero_ForZeroInput()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Sgn(0f);

            // Assert
            result.Should().Be(0, "Sgn of zero should return 0");
        }

        [Fact]
        public void Min_ReturnsSmaller()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Min(3f, 7f);

            // Assert
            result.Should().Be(3f, "Min(3, 7) should return 3");
        }

        [Fact]
        public void Max_ReturnsLarger()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Max(3f, 7f);

            // Assert
            result.Should().Be(7f, "Max(3, 7) should return 7");
        }

        [Fact]
        public void Ceil_RoundsUp()
        {
            // Act
            var result = CSharpCraft.Pico8.Pico8.Ceil(2.1f);

            // Assert
            result.Should().Be(3f, "Ceil(2.1) should return 3");
        }

        #endregion
    }
}
