using CSharpCraft.Pico8;
using CSharpCraft.Tests.Pico8.Mocks;
using FixMath;
using Xunit;

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
        Assert.Single(_graphics.DrawCalls);
        Assert.Equal("Cls", _graphics.DrawCalls[0].operation);
        Assert.Equal(clearColor, _graphics.DrawCalls[0].param);
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
        Assert.Single(_graphics.DrawCalls);
        Assert.Equal("Circle", _graphics.DrawCalls[0].operation);
        Assert.Equal(10f, _graphics.DrawCalls[0].x);
        Assert.Equal(20f, _graphics.DrawCalls[0].y);
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
        Assert.Single(_graphics.DrawCalls);
        var call = _graphics.DrawCalls[0];
        Assert.Equal("Circle", call.operation);
        Assert.Equal(64f, call.x);
        Assert.Equal(64f, call.y);
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
        Assert.Single(_graphics.DrawCalls);
        var call = _graphics.DrawCalls[0];
        Assert.Equal("CircleFilled", call.operation);
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
        Assert.Single(_graphics.DrawCalls);
        var call = _graphics.DrawCalls[0];
        Assert.Equal("RectangleFilled", call.operation);
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
        Assert.Single(_graphics.DrawCalls);
        var call = _graphics.DrawCalls[0];
        Assert.Equal("Line", call.operation);
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
        Assert.Single(_graphics.DrawCalls);
        var call = _graphics.DrawCalls[0];
        Assert.Equal("Print", call.operation);
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
        Assert.Equal(camX, _graphics.CameraOffset.x);
        Assert.Equal(camY, _graphics.CameraOffset.y);
    }

    [Fact]
    public void ResetCamera_ClearsOffset()
    {
        // Arrange
        _graphics.SetCamera(F32.FromInt(10), F32.FromInt(10));

        // Act
        _graphics.ResetCamera();

        // Assert
        Assert.Equal(F32.Zero, _graphics.CameraOffset.x);
        Assert.Equal(F32.Zero, _graphics.CameraOffset.y);
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
        Assert.Equal(4, _graphics.DrawCalls.Count);
        Assert.Equal("Cls", _graphics.DrawCalls[0].operation);
        Assert.Equal("Circle", _graphics.DrawCalls[1].operation);
        Assert.Equal("RectangleFilled", _graphics.DrawCalls[2].operation);
        Assert.Equal("Print", _graphics.DrawCalls[3].operation);
    }
}
