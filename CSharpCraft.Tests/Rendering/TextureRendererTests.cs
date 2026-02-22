using CSharpCraft.Pico8;
using FluentAssertions;
using Microsoft.Xna.Framework;
using Moq;
using Xunit;
using Pico8API = CSharpCraft.Pico8.Pico8;

namespace CSharpCraft.Tests.Rendering;

/// <summary>
/// Tests for ITextureRenderer interface behavior and GameRendering static accessor.
/// TDD spike to validate the Option 3A architecture (separate static accessor).
/// </summary>
public class TextureRendererTests : IDisposable
{
    private readonly Mock<ITextureRenderer> _mockRenderer;

    public TextureRendererTests()
    {
        _mockRenderer = new Mock<ITextureRenderer>();
        GameRendering.Current = _mockRenderer.Object;
    }

    public void Dispose()
    {
        GameRendering.Reset();
    }

    #region GameRendering Static Accessor

    [Fact]
    public void Current_WhenSet_ReturnsSameInstance()
    {
        GameRendering.Current.Should().BeSameAs(_mockRenderer.Object);
    }

    [Fact]
    public void Current_WhenNotSet_ThrowsInvalidOperationException()
    {
        GameRendering.Reset();

        var act = () => GameRendering.Current;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not been initialized*");
    }

    [Fact]
    public void Current_WhenSetToNull_ThrowsArgumentNullException()
    {
        var act = () => GameRendering.Current = null!;

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Reset_ClearsCurrentRenderer()
    {
        GameRendering.Reset();

        var act = () => GameRendering.Current;

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region ITextureRenderer.Draw (no source rect — 81 of 83 legacy calls)

    [Fact]
    public void Draw_WithTextureName_DelegatesToRenderer()
    {
        var position = new Vector2(10, 20);

        GameRendering.Current.Draw("CompetitiveBackground", position,
            Color.White, 4f, 4f);

        _mockRenderer.Verify(r => r.Draw("CompetitiveBackground", position,
            Color.White, 4f, 4f, false, false), Times.Once);
    }

    [Fact]
    public void Draw_WithFlipX_PassesFlipCorrectly()
    {
        var position = new Vector2(5, 10);

        GameRendering.Current.Draw("12pxHighlightEdge", position,
            Color.White, 4f, 4f, flipX: true);

        _mockRenderer.Verify(r => r.Draw("12pxHighlightEdge", position,
            Color.White, 4f, 4f, true, false), Times.Once);
    }

    [Fact]
    public void Draw_WithPaletteColor_PassesColorCorrectly()
    {
        var lavender = new Color(131, 118, 156); // PICO-8 color 13
        var position = new Vector2(24, 288);

        GameRendering.Current.Draw("MusicNote", position, lavender, 8f, 8f);

        _mockRenderer.Verify(r => r.Draw("MusicNote", position,
            lavender, 8f, 8f, false, false), Times.Once);
    }

    [Fact]
    public void Draw_WithFlipY_PassesFlipCorrectly()
    {
        var position = new Vector2(0, 0);

        GameRendering.Current.Draw("Checker", position,
            Color.White, 4f, 4f, flipY: true);

        _mockRenderer.Verify(r => r.Draw("Checker", position,
            Color.White, 4f, 4f, false, true), Times.Once);
    }

    [Fact]
    public void Draw_WithCustomScale_PassesScaleCorrectly()
    {
        // Simulates: new Vector2(p8.Cell.Width * (labelLength + 1), p8.Cell.Height)
        var position = new Vector2(200, 400);

        GameRendering.Current.Draw("12pxHighlightCenter", position,
            Color.White, 80f, 4f); // scaleX = 4 * 20, scaleY = 4

        _mockRenderer.Verify(r => r.Draw("12pxHighlightCenter", position,
            Color.White, 80f, 4f, false, false), Times.Once);
    }

    #endregion

    #region ITextureRenderer.Draw (with source rect — 2 of 83 legacy calls)

    [Fact]
    public void Draw_WithSourceRectangle_DelegatesToRenderer()
    {
        var position = new Vector2(100, 200);
        var sourceRect = new Rectangle(0, 0, 10, 18);

        GameRendering.Current.Draw("SurfaceMediumTest", position, sourceRect,
            Color.White, 4f, 4f);

        _mockRenderer.Verify(r => r.Draw("SurfaceMediumTest", position, sourceRect,
            Color.White, 4f, 4f, false, false), Times.Once);
    }

    #endregion

    #region ITextureRenderer.ClearDevice

    [Fact]
    public void ClearDevice_DelegatesToRenderer()
    {
        GameRendering.Current.ClearDevice(Color.Black);

        _mockRenderer.Verify(r => r.ClearDevice(Color.Black), Times.Once);
    }

    #endregion

    #region ITextureRenderer.GetCursorPosition

    [Fact]
    public void GetCursorPosition_ReturnsCorrectedCoordinates()
    {
        _mockRenderer.Setup(r => r.GetCursorPosition(150, 200))
            .Returns((120.5f, 180.3f));

        var (x, y) = GameRendering.Current.GetCursorPosition(150, 200);

        x.Should().Be(120.5f);
        y.Should().Be(180.3f);
    }

    [Fact]
    public void GetCursorPosition_WithZeroOffset_ReturnsOriginalValues()
    {
        // When window and viewport are same size, no offset
        _mockRenderer.Setup(r => r.GetCursorPosition(100, 200))
            .Returns((100f, 200f));

        var (x, y) = GameRendering.Current.GetCursorPosition(100, 200);

        x.Should().Be(100f);
        y.Should().Be(200f);
    }

    #endregion

    #region ITextureRenderer.ApplyDisplaySettings

    [Fact]
    public void ApplyDisplaySettings_DelegatesToRenderer()
    {
        GameRendering.Current.ApplyDisplaySettings(true, 1920, 1080);

        _mockRenderer.Verify(r => r.ApplyDisplaySettings(true, 1920, 1080), Times.Once);
    }

    [Fact]
    public void ApplyDisplaySettings_Windowed_DelegatesToRenderer()
    {
        GameRendering.Current.ApplyDisplaySettings(false, 1280, 720);

        _mockRenderer.Verify(r => r.ApplyDisplaySettings(false, 1280, 720), Times.Once);
    }

    #endregion
}

/// <summary>
/// Tests that validate the 3A ergonomics: Pico8.* for PICO-8 + GameRendering.Current for textures.
/// Simulates how a real scene test would look with two static accessors.
/// </summary>
[Collection("Pico8Static")]
public class SceneRenderingErgonomicsTests : IDisposable
{
    private readonly Mock<IGraphicsAPI> _mockGraphics;
    private readonly Mock<IInputStateManager> _mockInput;
    private readonly Mock<IAudioAPI> _mockAudio;
    private readonly Mock<ISceneManager> _mockSceneManager;
    private readonly Mock<ITextureRenderer> _mockRenderer;
    private readonly GameOrchestrator _orchestrator;

    public SceneRenderingErgonomicsTests()
    {
        // Setup path 1: PICO-8 static API
        _mockGraphics = new Mock<IGraphicsAPI>();
        _mockInput = new Mock<IInputStateManager>();
        _mockAudio = new Mock<IAudioAPI>();
        _mockSceneManager = new Mock<ISceneManager>();

        _orchestrator = new GameOrchestrator(
            _mockInput.Object, _mockGraphics.Object,
            _mockAudio.Object, _mockSceneManager.Object);

        Pico8API.Initialize(_orchestrator);

        // Setup path 2: GameRendering (separate concern)
        _mockRenderer = new Mock<ITextureRenderer>();
        GameRendering.Current = _mockRenderer.Object;
    }

    public void Dispose()
    {
        GameRendering.Reset();
    }

    [Fact]
    public void SceneCanUseBothPico8AndGameRendering()
    {
        // This test proves scenes can use both static accessors simultaneously.
        // Pico8.* for PICO-8 primitives:
        _mockGraphics.Setup(g => g.GetColor(13)).Returns(new Color(131, 118, 156));

        // Act - simulate what a scene Draw() would do:
        // Step 1: PICO-8 operations
        Pico8API.Cls(0);
        Pico8API.Rectfill(0, 0, 127, 127, 17);

        // Step 2: Game texture rendering
        GameRendering.Current.ClearDevice(Color.Black);
        GameRendering.Current.Draw("CompetitiveBackground", new Vector2(0, 0),
            Color.White, Pico8API.CellWidth, Pico8API.CellHeight);

        // Step 3: More PICO-8 operations
        Pico8API.Print("loading...", 64, 61, 15);

        // Verify both pathways work
        _mockGraphics.Verify(g => g.Cls(0), Times.Once);
        _mockRenderer.Verify(r => r.ClearDevice(Color.Black), Times.Once);
        _mockRenderer.Verify(r => r.Draw("CompetitiveBackground",
            It.IsAny<Vector2>(), Color.White,
            It.IsAny<float>(), It.IsAny<float>(), false, false), Times.Once);
    }

    [Fact]
    public void SceneCanMixPico8ColorsWithTextureRenderer()
    {
        // Scenes often use Pico8.GetColor(i) for texture tinting
        var lavender = new Color(131, 118, 156);
        _mockGraphics.Setup(g => g.GetColor(13)).Returns(lavender);

        // Scene code would be:
        // GameRendering.Current.Draw("MusicNote", pos, Pico8API.GetColor(13), cellW, cellH);
        var color = Pico8API.GetColor(13);
        GameRendering.Current.Draw("MusicNote", new Vector2(24, 288),
            color, 4f, 4f);

        _mockRenderer.Verify(r => r.Draw("MusicNote",
            new Vector2(24, 288), lavender, 4f, 4f, false, false), Times.Once);
    }

    [Fact]
    public void CursorCalculation_ReplacesBoilerplate()
    {
        // Old pattern (copy-pasted 33 times):
        // cursorX = state.X - ((p8.Window.ClientBounds.Width - p8.Batch.GraphicsDevice.Viewport.Width) / 2.0f);
        // cursorY = state.Y - ((p8.Window.ClientBounds.Height - p8.Batch.GraphicsDevice.Viewport.Height) / 2.0f);

        // New pattern (1 method call):
        _mockRenderer.Setup(r => r.GetCursorPosition(300, 400))
            .Returns((280f, 380f));

        var (cursorX, cursorY) = GameRendering.Current.GetCursorPosition(300, 400);

        cursorX.Should().Be(280f);
        cursorY.Should().Be(380f);
    }

    [Fact]
    public void DisplaySettings_ReplacesGraphicsManagerCalls()
    {
        // Old pattern (10 calls across 3 blocks in GeneralOptions.cs):
        // p8.Graphics.IsFullScreen = f.Gen_Fullscreen;
        // p8.Graphics.PreferredBackBufferWidth = ...;
        // p8.Graphics.PreferredBackBufferHeight = ...;
        // p8.Graphics.ApplyChanges();

        // New pattern (1 method call):
        GameRendering.Current.ApplyDisplaySettings(true, 1920, 1080);

        _mockRenderer.Verify(r => r.ApplyDisplaySettings(true, 1920, 1080), Times.Once);
    }
}
