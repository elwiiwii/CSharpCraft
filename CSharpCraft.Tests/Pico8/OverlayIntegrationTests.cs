using CSharpCraft.Pico8;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Integration tests for Phase 8/9 refactoring.
/// Verifies that old overlay/rendering patterns are properly removed.
/// </summary>
public class OverlayIntegrationTests
{
    #region GameOrchestrator API Verification

    [Fact]
    public void GameOrchestrator_DrawPauseMenu_NoLongerExistsAsMethod()
    {
        // DrawPauseMenu should be private or moved — verify it's not callable externally
        var methods = typeof(GameOrchestrator).GetMethods(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        methods.Should().NotContain(m => m.Name == "DrawPauseMenu",
            "DrawPauseMenu rendering is now handled by IPauseMenuRenderer");
    }

    [Fact]
    public void GameOrchestrator_ShouldNotExposesBatch()
    {
        var props = typeof(GameOrchestrator).GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        props.Should().NotContain(p => p.Name == "Batch",
            "SpriteBatch should not leak from GameOrchestrator — use ITextureRenderer");
    }

    [Fact]
    public void GameOrchestrator_ShouldNotExposesPixel()
    {
        var props = typeof(GameOrchestrator).GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        props.Should().NotContain(p => p.Name == "Pixel",
            "Pixel texture is internal to rendering — should not be exposed");
    }

    [Fact]
    public void GameOrchestrator_ShouldNotExposesTextureDictionary()
    {
        var props = typeof(GameOrchestrator).GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        props.Should().NotContain(p => p.Name == "TextureDictionary",
            "TextureDictionary is internal to rendering — use ITextureRenderer");
    }

    [Fact]
    public void GameOrchestrator_HasNoOverlayRendererReference()
    {
        // IOverlayRenderer is deleted — verify no field or property with the old type name
        var props = typeof(GameOrchestrator).GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        props.Should().NotContain(p => p.Name == "OverlayRenderer",
            "IOverlayRenderer is replaced by IPauseMenuRenderer + IPopupService");
    }

    [Fact]
    public void GameOrchestrator_HasPauseMenuRendererProperty()
    {
        var props = typeof(GameOrchestrator).GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        props.Should().Contain(p => p.Name == "PauseMenuRenderer");
    }

    [Fact]
    public void GameOrchestrator_HasPopupServiceProperty()
    {
        var props = typeof(GameOrchestrator).GetProperties(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        props.Should().Contain(p => p.Name == "PopupService");
    }

    #endregion

    #region DefaultColors Extraction

    [Fact]
    public void Pico8Utils_HasDefaultColors()
    {
        var colors = Pico8Utils.DefaultColors;

        colors.Should().HaveCount(32);
        colors[0].Should().Be(Pico8Utils.HexToColor("000000")); // black
        colors[7].Should().Be(Pico8Utils.HexToColor("FFF1E8")); // white
        colors[8].Should().Be(Pico8Utils.HexToColor("FF004D")); // red
    }

    #endregion

    #region IPauseMenuContext Unified Operations

    [Fact]
    public void IPauseMenuContext_HasToggleSound()
    {
        var methods = typeof(IPauseMenuContext).GetMethods();
        methods.Should().Contain(m => m.Name == "ToggleSound",
            "IPauseMenuContext should expose unified ToggleSound operation");
    }

    [Fact]
    public void IPauseMenuContext_HasQuitToTitle()
    {
        var methods = typeof(IPauseMenuContext).GetMethods();
        methods.Should().Contain(m => m.Name == "QuitToTitle",
            "IPauseMenuContext should expose unified QuitToTitle operation");
    }

    [Fact]
    public void IPauseMenuContext_HasToggleFullscreen()
    {
        var methods = typeof(IPauseMenuContext).GetMethods();
        methods.Should().Contain(m => m.Name == "ToggleFullscreen",
            "IPauseMenuContext should expose unified ToggleFullscreen operation");
    }

    #endregion

    #region IAudioGraphicsSettings.Save

    [Fact]
    public void IAudioGraphicsSettings_HasSaveMethod()
    {
        var methods = typeof(IAudioGraphicsSettings).GetMethods();
        methods.Should().Contain(m => m.Name == "Save",
            "IAudioGraphicsSettings should expose Save for persistence");
    }

    #endregion

    #region IInputStateManager Raw Keyboard

    [Fact]
    public void IInputStateManager_HasIsKeyDown()
    {
        var methods = typeof(IInputStateManager).GetMethods();
        methods.Should().Contain(m => m.Name == "IsKeyDown",
            "IInputStateManager should expose raw keyboard key-down check");
    }

    [Fact]
    public void IInputStateManager_HasIsKeyJustPressed()
    {
        var methods = typeof(IInputStateManager).GetMethods();
        methods.Should().Contain(m => m.Name == "IsKeyJustPressed",
            "IInputStateManager should expose raw keyboard just-pressed check");
    }

    #endregion
}
