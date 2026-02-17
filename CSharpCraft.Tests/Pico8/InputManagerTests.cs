using Xunit;
using Moq;
using CSharpCraft.Pico8;
using CSharpCraft.Tests.Pico8.Mocks;
using FixMath;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Unit tests for input management
/// Tests button state tracking and input polling
/// </summary>
public class InputManagerTests
{
    private readonly MockInputManager _input;

    public InputManagerTests()
    {
        _input = new MockInputManager();
    }

    [Fact]
    public void Btn_InitiallyReturnsFalse()
    {
        // Arrange & Act
        bool result = _input.Btn(0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Btn_ReturnsTrue_WhenButtonPressed()
    {
        // Arrange
        _input.SetButtonState(0, true);

        // Act
        bool result = _input.Btn(0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Btn_ReturnsFalse_WhenButtonReleased()
    {
        // Arrange
        _input.SetButtonState(0, true);
        _input.SetButtonState(0, false);

        // Act
        bool result = _input.Btn(0);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Btn_IndependentButtons()
    {
        // Arrange
        _input.SetButtonState(0, true);
        _input.SetButtonState(1, false);
        _input.SetButtonState(2, true);

        // Act & Assert
        Assert.True(_input.Btn(0));
        Assert.False(_input.Btn(1));
        Assert.True(_input.Btn(2));
    }

    [Fact]
    public void Btnp_TracksButtonPress()
    {
        // Arrange
        _input.SetButtonState(0, true);

        // Act
        bool result = _input.Btnp(0);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AllButtons_InitiallyNotPressed()
    {
        // Arrange, Act & Assert
        for (int i = 0; i < 7; i++)
        {
            Assert.False(_input.Btn(i));
        }
    }

    [Fact]
    public void PressMultipleButtons_TrackIndependently()
    {
        // Arrange & Act
        _input.SetButtonState(0, true); // Left
        _input.SetButtonState(1, true); // Right
        _input.SetButtonState(4, true); // Menu

        // Assert
        Assert.True(_input.Btn(0));
        Assert.True(_input.Btn(1));
        Assert.False(_input.Btn(2)); // Up - not pressed
        Assert.False(_input.Btn(3)); // Down - not pressed
        Assert.True(_input.Btn(4));  // Menu - pressed
    }

    [Fact]
    public void GetButtonState_ReturnsValidP8Btns()
    {
        // Arrange & Act
        var buttons = _input.GetButtonState();

        // Assert
        Assert.NotNull(buttons);
    }

    [Fact]
    public void GetAnalogStick_ReturnsValidTuple()
    {
        // Arrange & Act
        var stick = _input.GetAnalogStick(0);

        // Assert
        Assert.Equal(F32.Zero, stick.x);
        Assert.Equal(F32.Zero, stick.y);
    }

    [Fact]
    public void MultipleButtonPresses_SimulatesGameInput()
    {
        // Arrange & Act - Simulates moving left, jumping
        _input.SetButtonState(0, true); // Press left
        Assert.True(_input.Btn(0));

        _input.SetButtonState(4, true); // Press action
        Assert.True(_input.Btn(4));

        _input.SetButtonState(0, false); // Release left
        Assert.False(_input.Btn(0));

        _input.SetButtonState(4, false); // Release action
        Assert.False(_input.Btn(4));

        // Assert
        Assert.False(_input.Btn(0));
        Assert.False(_input.Btn(4));
    }

    [Fact]
    public void InputManager_Btn_Returns_Expected_State()
    {
        // This is a placeholder pattern for testing with mocked services
        
        // Arrange
        var inputManagerMock = new Mock<IInputManager>();
        inputManagerMock.Setup(x => x.Btn(0, 0)).Returns(true);
        inputManagerMock.Setup(x => x.Btn(1, 0)).Returns(false);

        // Act
        var result1 = inputManagerMock.Object.Btn(0);
        var result2 = inputManagerMock.Object.Btn(1);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
    }

    [Fact]
    public void InputManager_Btnp_Called_Once_Returns_True()
    {
        // Arrange
        var inputManagerMock = new Mock<IInputManager>();
        inputManagerMock.Setup(x => x.Btnp(4, 0)).Returns(true);

        // Act
        var result = inputManagerMock.Object.Btnp(4);

        // Assert
        Assert.True(result);
        inputManagerMock.Verify(x => x.Btnp(4, It.IsAny<int>()), Times.Once);
    }
}
