using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using System.Reflection;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Display Manager Test Suite — Phase 7
/// 
/// TDD Phase: RED → GREEN → REFACTOR
/// 
/// Tests verify:
/// 1. IDisplayManager interface exists with correct contract
/// 2. DisplayManager implements it with correct defaults
/// 3. GameOrchestrator delegates Cell/Resolution to IDisplayManager
/// 4. GameOrchestrator delegates viewport/fullscreen to IDisplayManager
/// 5. Platform fields (_graphics, _graphicsDevice, _window) removed from GameOrchestrator
/// 6. ITextureRenderer.Draw accepts virtual coordinates with default scales
/// </summary>
public class DisplayManagerTests
{
    #region IDisplayManager Interface Contract

    [Fact]
    public void IDisplayManager_Interface_Exists()
    {
        var type = typeof(IDisplayManager);
        type.Should().NotBeNull();
        type.IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IDisplayManager_Has_Cell_Property()
    {
        var prop = typeof(IDisplayManager).GetProperty("Cell");
        prop.Should().NotBeNull("IDisplayManager must expose Cell");
        prop!.PropertyType.Should().Be(typeof((int Width, int Height)));
        prop.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IDisplayManager_Has_Resolution_Property()
    {
        var prop = typeof(IDisplayManager).GetProperty("Resolution");
        prop.Should().NotBeNull("IDisplayManager must expose Resolution");
        prop!.CanRead.Should().BeTrue();
    }

    [Fact]
    public void IDisplayManager_Has_UpdateViewport_Method()
    {
        var method = typeof(IDisplayManager).GetMethod("UpdateViewport");
        method.Should().NotBeNull("IDisplayManager must have UpdateViewport");
        method!.GetParameters().Should().ContainSingle()
            .Which.ParameterType.Should().Be(typeof(IScene));
    }

    [Fact]
    public void IDisplayManager_Has_ToggleFullscreen_Method()
    {
        var method = typeof(IDisplayManager).GetMethod("ToggleFullscreen");
        method.Should().NotBeNull("IDisplayManager must have ToggleFullscreen");
        method!.GetParameters().Should().ContainSingle()
            .Which.ParameterType.Should().Be(typeof(IScene));
    }

    [Fact]
    public void IDisplayManager_Has_RecalculateCell_Method()
    {
        var method = typeof(IDisplayManager).GetMethod("RecalculateCell");
        method.Should().NotBeNull("IDisplayManager must have RecalculateCell");
    }

    [Fact]
    public void IDisplayManager_Has_SetDisplayConfig_Method()
    {
        var method = typeof(IDisplayManager).GetMethod("SetDisplayConfig");
        method.Should().NotBeNull("IDisplayManager must have SetDisplayConfig");
    }

    #endregion

    #region DisplayManager Implementation

    [Fact]
    public void DisplayManager_Implements_IDisplayManager()
    {
        typeof(IDisplayManager).IsAssignableFrom(typeof(DisplayManager))
            .Should().BeTrue("DisplayManager must implement IDisplayManager");
    }

    [Fact]
    public void DisplayManager_SetDisplayConfig_UpdatesCellAndResolution()
    {
        var dm = CreateTestDisplayManager();
        dm.SetDisplayConfig((256, 192), (4, 3));

        dm.Cell.Should().Be((4, 3));
        dm.Resolution.Should().Be((256, 192));
    }

    [Fact]
    public void DisplayManager_Cell_DefaultsTo_1_1()
    {
        var dm = CreateTestDisplayManager();
        dm.Cell.Should().Be((1, 1));
    }

    [Fact]
    public void DisplayManager_Resolution_DefaultsTo_128_128()
    {
        var dm = CreateTestDisplayManager();
        dm.Resolution.Should().Be((128, 128));
    }

    #endregion

    #region GameOrchestrator Delegation to IDisplayManager

    [Fact]
    public void GameOrchestrator_Cell_DelegatesToDisplayManager()
    {
        var mockDisplay = new Mock<IDisplayManager>();
        mockDisplay.Setup(d => d.Cell).Returns((4, 3));

        var orch = CreateOrchestrator(displayManager: mockDisplay.Object);

        orch.Cell.Should().Be((4, 3));
    }

    [Fact]
    public void GameOrchestrator_Resolution_DelegatesToDisplayManager()
    {
        var mockDisplay = new Mock<IDisplayManager>();
        mockDisplay.Setup(d => d.Resolution).Returns((256, 192));

        var orch = CreateOrchestrator(displayManager: mockDisplay.Object);

        orch.Resolution.Should().Be((256, 192));
    }

    [Fact]
    public void GameOrchestrator_SetDisplayConfig_DelegatesToDisplayManager()
    {
        var mockDisplay = new Mock<IDisplayManager>();
        var orch = CreateOrchestrator(displayManager: mockDisplay.Object);

        orch.SetDisplayConfig((256, 192), (4, 3));

        mockDisplay.Verify(d => d.SetDisplayConfig(
            It.Is<(int w, int h)>(r => r.w == 256 && r.h == 192),
            It.Is<(int Width, int Height)>(c => c.Width == 4 && c.Height == 3)),
            Times.Once);
    }

    [Fact]
    public void GameOrchestrator_TestConstructor_WithoutDisplayManager_UseDefaults()
    {
        var orch = CreateOrchestrator();

        orch.Cell.Should().Be((1, 1));
        orch.Resolution.Should().Be((128, 128));
    }

    [Fact]
    public void GameOrchestrator_TestConstructor_SetDisplayConfig_StillWorks()
    {
        // Existing tests call SetDisplayConfig directly — must still work
        // with the default internal display manager
        var orch = CreateOrchestrator();

        orch.SetDisplayConfig((192, 128), (8, 8));

        orch.Cell.Should().Be((8, 8));
        orch.Resolution.Should().Be((192, 128));
    }

    #endregion

    #region Platform Field Removal from GameOrchestrator

    [Fact]
    public void GameOrchestrator_DoesNotDeclare_graphicsField()
    {
        var field = typeof(GameOrchestrator).GetField("_graphics",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.Should().BeNull("_graphics should be owned by DisplayManager, not GameOrchestrator");
    }

    [Fact]
    public void GameOrchestrator_DoesNotDeclare_graphicsDeviceField()
    {
        var field = typeof(GameOrchestrator).GetField("_graphicsDevice",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.Should().BeNull("_graphicsDevice should be owned by DisplayManager, not GameOrchestrator");
    }

    [Fact]
    public void GameOrchestrator_DoesNotDeclare_windowField()
    {
        var field = typeof(GameOrchestrator).GetField("_window",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.Should().BeNull("_window should be owned by DisplayManager, not GameOrchestrator");
    }

    [Fact]
    public void GameOrchestrator_DoesNotExposeGraphicsManagerProperty()
    {
        typeof(GameOrchestrator).GetProperty("GraphicsManager")
            .Should().BeNull("GraphicsManager property has zero external consumers");
    }

    [Fact]
    public void GameOrchestrator_DoesNotExposeGraphicsDeviceProperty()
    {
        typeof(GameOrchestrator).GetProperty("GraphicsDevice")
            .Should().BeNull("GraphicsDevice property has zero external consumers");
    }

    [Fact]
    public void GameOrchestrator_DoesNotExposeWindowProperty()
    {
        typeof(GameOrchestrator).GetProperty("Window")
            .Should().BeNull("Window property has zero external consumers");
    }

    [Fact]
    public void GameOrchestrator_DoesNotDeclare_cellField()
    {
        var field = typeof(GameOrchestrator).GetField("_cell",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.Should().BeNull("_cell should be owned by DisplayManager, not GameOrchestrator");
    }

    [Fact]
    public void GameOrchestrator_DoesNotDeclare_resolutionField()
    {
        var field = typeof(GameOrchestrator).GetField("_resolution",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.Should().BeNull("_resolution should be owned by DisplayManager, not GameOrchestrator");
    }

    #endregion

    #region ITextureRenderer Virtual Coordinate API

    [Fact]
    public void ITextureRenderer_Draw_AcceptsDoubleCoordinates()
    {
        // The new Draw signature takes double x, y (virtual coordinates)
        // instead of Vector2 position (physical coordinates)
        var method = typeof(ITextureRenderer).GetMethods()
            .Where(m => m.Name == "Draw")
            .Where(m => m.GetParameters().Length >= 4)
            .Where(m => m.GetParameters()[1].ParameterType == typeof(double))
            .FirstOrDefault();

        method.Should().NotBeNull("ITextureRenderer.Draw should accept double x, y parameters");
    }

    [Fact]
    public void ITextureRenderer_Draw_HasDefaultScaleOf1()
    {
        var mockRenderer = new Mock<ITextureRenderer>();

        // Should be callable with just texture, x, y, color — scales default to 1
        mockRenderer.Object.Draw("MusicNote", 3.0, 36.0, Microsoft.Xna.Framework.Color.White);

        mockRenderer.Verify(r => r.Draw("MusicNote", 3.0, 36.0,
            Microsoft.Xna.Framework.Color.White, 1.0, 1.0, false, false), Times.Once);
    }

    [Fact]
    public void ITextureRenderer_Draw_WithHalfScale()
    {
        var mockRenderer = new Mock<ITextureRenderer>();

        // Half-scale calls use 0.5, 0.5 instead of CellWidth/2, CellHeight/2
        mockRenderer.Object.Draw("Cursor", 10.0, 20.0,
            Microsoft.Xna.Framework.Color.White, 0.5, 0.5);

        mockRenderer.Verify(r => r.Draw("Cursor", 10.0, 20.0,
            Microsoft.Xna.Framework.Color.White, 0.5, 0.5, false, false), Times.Once);
    }

    [Fact]
    public void ITextureRenderer_Draw_WithNonUniformScale()
    {
        var mockRenderer = new Mock<ITextureRenderer>();

        // Stretch calls: scaleX = labelLength + 1, scaleY = 1 (default)
        mockRenderer.Object.Draw("12pxHighlightCenter", 50.0, 108.0,
            Microsoft.Xna.Framework.Color.White, scaleX: 21.0);

        mockRenderer.Verify(r => r.Draw("12pxHighlightCenter", 50.0, 108.0,
            Microsoft.Xna.Framework.Color.White, 21.0, 1.0, false, false), Times.Once);
    }

    [Fact]
    public void ITextureRenderer_Draw_WithSourceRect_AcceptsDoubleCoordinates()
    {
        var mockRenderer = new Mock<ITextureRenderer>();
        var sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 10, 18);

        // Source rect overload also uses virtual coordinates
        mockRenderer.Object.Draw("SurfaceMediumTest", 3.0, 23.0, sourceRect,
            Microsoft.Xna.Framework.Color.White);

        mockRenderer.Verify(r => r.Draw("SurfaceMediumTest", 3.0, 23.0, sourceRect,
            Microsoft.Xna.Framework.Color.White, 1.0, 1.0, false, false), Times.Once);
    }

    [Fact]
    public void ITextureRenderer_Draw_WithFlipX_PassesCorrectly()
    {
        var mockRenderer = new Mock<ITextureRenderer>();

        mockRenderer.Object.Draw("SeedPickIndicator", 11.0, 3.0,
            Microsoft.Xna.Framework.Color.White, flipX: true);

        mockRenderer.Verify(r => r.Draw("SeedPickIndicator", 11.0, 3.0,
            Microsoft.Xna.Framework.Color.White, 1.0, 1.0, true, false), Times.Once);
    }

    #endregion

    #region Helpers

    private static DisplayManager CreateTestDisplayManager()
    {
        // DisplayManager constructed without FNA deps for basic property tests
        return new DisplayManager(null!, null!, null!, null!);
    }

    private static GameOrchestrator CreateOrchestrator(IDisplayManager? displayManager = null)
    {
        return new GameOrchestrator(
            new Mock<IInputStateManager>().Object,
            new Mock<IGraphicsAPI>().Object,
            new Mock<IAudioAPI>().Object,
            new Mock<ISceneManager>().Object,
            displayManager: displayManager);
    }

    #endregion
}

/// <summary>
/// Tests that the Pico8 static API still works correctly
/// after Cell/Resolution ownership moves to IDisplayManager.
/// </summary>
[Collection("Pico8Static")]
public class DisplayManagerStaticApiTests : IDisposable
{
    private readonly GameOrchestrator _orchestrator;

    public DisplayManagerStaticApiTests()
    {
        _orchestrator = new GameOrchestrator(
            new Mock<IInputStateManager>().Object,
            new Mock<IGraphicsAPI>().Object,
            new Mock<IAudioAPI>().Object,
            new Mock<ISceneManager>().Object);

        CSharpCraft.Pico8.Pico8.Initialize(_orchestrator);
    }

    public void Dispose() { }

    [Fact]
    public void Pico8_CellWidth_DelegatesToDisplayManager()
    {
        _orchestrator.SetDisplayConfig((128, 128), (8, 6));
        CSharpCraft.Pico8.Pico8.CellWidth.Should().Be(8);
    }

    [Fact]
    public void Pico8_CellHeight_DelegatesToDisplayManager()
    {
        _orchestrator.SetDisplayConfig((128, 128), (8, 6));
        CSharpCraft.Pico8.Pico8.CellHeight.Should().Be(6);
    }

    [Fact]
    public void Pico8_ResolutionWidth_DelegatesToDisplayManager()
    {
        _orchestrator.SetDisplayConfig((256, 192), (2, 2));
        CSharpCraft.Pico8.Pico8.ResolutionWidth.Should().Be(256);
    }

    [Fact]
    public void Pico8_ResolutionHeight_DelegatesToDisplayManager()
    {
        _orchestrator.SetDisplayConfig((256, 192), (2, 2));
        CSharpCraft.Pico8.Pico8.ResolutionHeight.Should().Be(192);
    }
}
