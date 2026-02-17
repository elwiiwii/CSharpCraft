using CSharpCraft.Pico8;
using CSharpCraft.Tests.Pico8.Mocks;
using FixMath;
using Xunit;
using FluentAssertions;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Unit tests for graphics primitives
/// Tests that drawing functions record operations correctly
/// </summary>
public class GraphicsPrimitivesTests
{
    private readonly MockGraphicsEngine _graphics;

    public GraphicsPrimitivesTests()
    {
        _graphics = new MockGraphicsEngine();
    }

    [Fact]
    public void Cls_RecordsScreenClear()
    {
        // Arrange
        int clearColor = 0;

        // Act
        _graphics.Cls(clearColor);

        // Assert
        _graphics.DrawCalls.Should().HaveCount(1);
        _graphics.DrawCalls[0].operation.Should().Be("Cls");
        _graphics.DrawCalls[0].param.Should().Be(clearColor);
    }

    [Fact]
    public void Pset_RecordsPixelDraw()
    {
        // Arrange
        F32 x = F32.FromInt(10);
        F32 y = F32.FromInt(20);
        int color = 3;

        // Act
        _graphics.Circle(x, y, 0.5, color); // Using circle as proxy for pset-like behavior

        // Assert
        _graphics.DrawCalls.Should().HaveCount(1);
        _graphics.DrawCalls[0].operation.Should().Be("Circle");
        _graphics.DrawCalls[0].x.Should().Be(10f);
        _graphics.DrawCalls[0].y.Should().Be(20f);
    }

    [Fact]
    public void Circle_RecordsCircleOperation()
    {
        // Arrange
        F32 x = F32.FromInt(64);
        F32 y = F32.FromInt(64);
        double radius = 5.0;
        int color = 7;

        // Act
        _graphics.Circle(x, y, radius, color);

        // Assert
        _graphics.DrawCalls.Should().HaveCount(1);
        var call = _graphics.DrawCalls[0];
        call.operation.Should().Be("Circle");
        call.x.Should().Be(64f);
        call.y.Should().Be(64f);
    }

    [Fact]
    public void CircleFilled_RecordsFilledCircleOperation()
    {
        // Arrange
        F32 centerX = F32.FromInt(50);
        F32 centerY = F32.FromInt(50);
        double radius = 10.0;
        int color = 11;

        // Act
        _graphics.CircleFilled(centerX, centerY, radius, color);

        // Assert
        _graphics.DrawCalls.Should().HaveCount(1);
        var call = _graphics.DrawCalls[0];
        call.operation.Should().Be("CircleFilled");
    }

    [Fact]
    public void RectangleFilled_RecordsFilledRectangleOperation()
    {
        // Arrange
        F32 x1 = F32.FromInt(10);
        F32 y1 = F32.FromInt(10);
        F32 x2 = F32.FromInt(50);
        F32 y2 = F32.FromInt(50);
        int color = 2;

        // Act
        _graphics.RectangleFilled(x1, y1, x2, y2, color);

        // Assert
        _graphics.DrawCalls.Should().HaveCount(1);
        var call = _graphics.DrawCalls[0];
        call.operation.Should().Be("RectangleFilled");
    }

    [Fact]
    public void Line_RecordsLineOperation()
    {
        // Arrange
        F32 x0 = F32.FromInt(0);
        F32 y0 = F32.FromInt(0);
        F32 x1 = F32.FromInt(100);
        F32 y1 = F32.FromInt(100);
        int color = 7;

        // Act
        _graphics.Line(x0, y0, x1, y1, color);

        // Assert
        _graphics.DrawCalls.Should().HaveCount(1);
        var call = _graphics.DrawCalls[0];
        call.operation.Should().Be("Line");
    }

    [Fact]
    public void Print_RecordsTextRenderingOperation()
    {
        // Arrange
        string text = "Hello";
        F32 x = F32.FromInt(10);
        F32 y = F32.FromInt(10);
        int color = 7;

        // Act
        _graphics.Print(text, x, y, color);

        // Assert
        _graphics.DrawCalls.Should().HaveCount(1);
        var call = _graphics.DrawCalls[0];
        call.operation.Should().Be("Print");
    }

    [Fact]
    public void SetCamera_UpdatesCameraOffset()
    {
        // Arrange
        F32 camX = F32.FromInt(16);
        F32 camY = F32.FromInt(16);

        // Act
        _graphics.SetCamera(camX, camY);

        // Assert
        _graphics.CameraOffset.x.Should().Be(camX);
        _graphics.CameraOffset.y.Should().Be(camY);
    }

    [Fact]
    public void ResetCamera_ClearsOffset()
    {
        // Arrange
        _graphics.SetCamera(F32.FromInt(10), F32.FromInt(10));

        // Act
        _graphics.ResetCamera();

        // Assert
        _graphics.CameraOffset.x.Should().Be(F32.Zero);
        _graphics.CameraOffset.y.Should().Be(F32.Zero);
    }

    [Fact]
    public void MultipleDrawCalls_AreRecorded()
    {
        // Arrange & Act
        _graphics.Cls(0);
        _graphics.Circle(F32.FromInt(50), F32.FromInt(50), 5, 7);
        _graphics.RectangleFilled(F32.FromInt(10), F32.FromInt(10), F32.FromInt(30), F32.FromInt(30), 3);
        _graphics.Print("TEXT", F32.FromInt(5), F32.FromInt(5), 7);

        // Assert
        _graphics.DrawCalls.Should().HaveCount(4);
        _graphics.DrawCalls[0].operation.Should().Be("Cls");
        _graphics.DrawCalls[1].operation.Should().Be("Circle");
        _graphics.DrawCalls[2].operation.Should().Be("RectangleFilled");
        _graphics.DrawCalls[3].operation.Should().Be("Print");
    }
}
