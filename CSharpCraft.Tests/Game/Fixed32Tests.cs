using Xunit;
using CSharpCraft.Game.FixMath;

namespace CSharpCraft.Tests.Game;

public class Fixed32Tests
{
    [Fact]
    public void Fixed32_Addition_Works()
    {
        // Arrange
        var a = new Fixed32(1);
        var b = new Fixed32(2);

        // Act
        var result = a + b;

        // Assert
        Assert.Equal(new Fixed32(3), result);
    }

    [Fact]
    public void Fixed32_Multiplication_Works()
    {
        // Arrange
        var a = new Fixed32(2);
        var b = new Fixed32(3);

        // Act
        var result = a * b;

        // Assert
        Assert.Equal(new Fixed32(6), result);
    }

    [Fact]
    public void Fixed32_Division_Works()
    {
        // Arrange
        var a = new Fixed32(6);
        var b = new Fixed32(2);

        // Act
        var result = a / b;

        // Assert
        Assert.Equal(new Fixed32(3), result);
    }
}
