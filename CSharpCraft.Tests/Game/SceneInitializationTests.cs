using Xunit;
using Moq;
using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Tests.Game;

/// <summary>
/// Example test suite showing how to use dependency injection and mocking
/// to test game logic without requiring the full Pico8Functions implementation
/// </summary>
public class SceneInitializationTests
{
    [Fact]
    public void Scene_Init_Receives_All_Required_Services()
    {
        // Arrange
        var graphicsMock = new Mock<IGraphicsEngine>();
        var audioMock = new Mock<IAudioManager>();
        var inputMock = new Mock<IInputManager>();
        var sceneManagerMock = new Mock<ISceneManager>();
        var clockMock = new Mock<IGameClock>();

        // Act - These would be passed to IScene.Init() method
        // Scene init should capture/ store these service references
        var services = new {
            Graphics = graphicsMock.Object,
            Audio = audioMock.Object,
            Input = inputMock.Object,
            SceneManager = sceneManagerMock.Object,
            Clock = clockMock.Object
        };

        // Assert - Verify all services are available
        Assert.NotNull(services.Graphics);
        Assert.NotNull(services.Audio);
        Assert.NotNull(services.Input);
        Assert.NotNull(services.SceneManager);
        Assert.NotNull(services.Clock);
    }

    [Fact]
    public void MockGraphicsEngine_Can_Record_Draw_Calls()
    {
        // Arrange  
        var graphicsMock = new Mock<IGraphicsEngine>();
        var graphics = graphicsMock.Object;

        // Act - Simulate game drawing
        graphics.Cls(0);
        graphics.Circle(new F32(64), new F32(64), 10, 3);
        graphics.Print("Test", new F32(0), new F32(0), 7);

        // Assert - Verify draw calls were made
        graphicsMock.Verify(x => x.Cls(0), Times.Once);
        graphicsMock.Verify(x => x.Circle(It.IsAny<F32>(), It.IsAny<F32>(), It.IsAny<double>(), It.IsAny<int>()), Times.Once);
        graphicsMock.Verify(x => x.Print(It.IsAny<string>(), It.IsAny<F32>(), It.IsAny<F32>(), It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public void MockInputManager_Can_Simulate_User_Input()
    {
        // Arrange
        var inputMock = new Mock<IInputManager>();
        inputMock.Setup(x => x.Btn(0)).Returns(true);  // Up pressed
        inputMock.Setup(x => x.Btn(1)).Returns(false); // Down not pressed
        inputMock.Setup(x => x.Btnp(4)).Returns(true); // Confirm just pressed

        var input = inputMock.Object;

        // Act - Simulate reading input
        var isUpPressed = input.Btn(0);
        var isDownPressed = input.Btn(1);
        var isConfirmPressed = input.Btnp(4);

        // Assert
        Assert.True(isUpPressed);
        Assert.False(isDownPressed);
        Assert.True(isConfirmPressed);
    }
}
