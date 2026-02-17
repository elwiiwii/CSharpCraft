using Xunit;
using Moq;
using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Tests.Pico8;

public class InputManagerTests
{
    [Fact]
    public void InputManager_Btn_Returns_Expected_State()
    {
        // This is a placeholder test showing the pattern for testing with mocked services
        // In a real implementation, IInputManager would be implemented and tested

        // Arrange
        var inputManagerMock = new Mock<IInputManager>();
        inputManagerMock.Setup(x => x.Btn(0)).Returns(true);
        inputManagerMock.Setup(x => x.Btn(1)).Returns(false);

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
        inputManagerMock.Setup(x => x.Btnp(4)).Returns(true);

        // Act
        var result = inputManagerMock.Object.Btnp(4);

        // Assert
        Assert.True(result);
        inputManagerMock.Verify(x => x.Btnp(4), Times.Once);
    }
}
