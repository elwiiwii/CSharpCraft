using CSharpCraft.Input;
using FluentAssertions;
using Microsoft.Xna.Framework.Input;
using PSharp8.Input;
using Xunit;

namespace CSharpCraft.Tests.Input;

public class InputEventTranslatorTests
{
    // --------------------------------------------------------------------------
    #region Translate Keyboard
    // --------------------------------------------------------------------------

    [Fact]
    public void TranslateKeyboard_ReturnsNull_WhenKeycodeUnrecognised()
    {
        InputEvent? result = InputEventTranslator.TranslateKeyboard(0xDEADBEEFu, isDown: true, timestampNs: 0UL);

        _ = result.Should().BeNull();
    }

    [Theory]
    // letters (representative: one currently mapped, one from each end of alphabet)
    [InlineData(0x00000061u, (int)Keys.A)]           // SDLK_A
    [InlineData(0x00000062u, (int)Keys.B)]           // SDLK_B — letter not previously in map
    [InlineData(0x00000079u, (int)Keys.Y)]           // SDLK_Y — letter not previously in map
    [InlineData(0x0000007au, (int)Keys.Z)]           // SDLK_Z
    // digits
    [InlineData(0x00000030u, (int)Keys.D0)]          // SDLK_0
    [InlineData(0x00000031u, (int)Keys.D1)]          // SDLK_1
    [InlineData(0x00000039u, (int)Keys.D9)]          // SDLK_9
    // numpad
    [InlineData(0x40000062u, (int)Keys.NumPad0)]     // SDLK_KP_0
    [InlineData(0x40000059u, (int)Keys.NumPad1)]     // SDLK_KP_1
    [InlineData(0x40000061u, (int)Keys.NumPad9)]     // SDLK_KP_9
    [InlineData(0x40000058u, (int)Keys.Enter)]       // SDLK_KP_ENTER
    [InlineData(0x400000d8u, (int)Keys.OemClear)]    // SDLK_KP_CLEAR
    [InlineData(0x400000dcu, (int)Keys.Decimal)]     // SDLK_KP_DECIMAL
    [InlineData(0x40000054u, (int)Keys.Divide)]      // SDLK_KP_DIVIDE
    [InlineData(0x40000057u, (int)Keys.Add)]         // SDLK_KP_PLUS
    // function keys
    [InlineData(0x4000003au, (int)Keys.F1)]          // SDLK_F1
    [InlineData(0x40000045u, (int)Keys.F12)]         // SDLK_F12
    [InlineData(0x40000068u, (int)Keys.F13)]         // SDLK_F13
    [InlineData(0x40000073u, (int)Keys.F24)]         // SDLK_F24
    // navigation
    [InlineData(0x40000050u, (int)Keys.Left)]        // SDLK_LEFT
    [InlineData(0x4000004fu, (int)Keys.Right)]       // SDLK_RIGHT
    [InlineData(0x40000052u, (int)Keys.Up)]          // SDLK_UP
    [InlineData(0x40000051u, (int)Keys.Down)]        // SDLK_DOWN
    [InlineData(0x4000004au, (int)Keys.Home)]        // SDLK_HOME
    [InlineData(0x4000004du, (int)Keys.End)]         // SDLK_END
    [InlineData(0x40000049u, (int)Keys.Insert)]      // SDLK_INSERT
    [InlineData(0x0000007fu, (int)Keys.Delete)]      // SDLK_DELETE
    [InlineData(0x4000004bu, (int)Keys.PageUp)]      // SDLK_PAGEUP
    [InlineData(0x4000004eu, (int)Keys.PageDown)]    // SDLK_PAGEDOWN
    // modifiers
    [InlineData(0x400000e1u, (int)Keys.LeftShift)]   // SDLK_LSHIFT
    [InlineData(0x400000e5u, (int)Keys.RightShift)]  // SDLK_RSHIFT
    [InlineData(0x400000e0u, (int)Keys.LeftControl)] // SDLK_LCTRL
    [InlineData(0x400000e4u, (int)Keys.RightControl)]// SDLK_RCTRL
    [InlineData(0x400000e2u, (int)Keys.LeftAlt)]     // SDLK_LALT
    [InlineData(0x400000e6u, (int)Keys.RightAlt)]    // SDLK_RALT
    [InlineData(0x400000e3u, (int)Keys.LeftWindows)] // SDLK_LGUI
    [InlineData(0x400000e7u, (int)Keys.RightWindows)]// SDLK_RGUI
    [InlineData(0x40000065u, (int)Keys.Apps)]        // SDLK_APPLICATION
    [InlineData(0x40000076u, (int)Keys.Apps)]        // SDLK_MENU (also Apps)
    // control keys
    [InlineData(0x00000020u, (int)Keys.Space)]       // SDLK_SPACE
    [InlineData(0x00000008u, (int)Keys.Back)]        // SDLK_BACKSPACE
    [InlineData(0x0000000du, (int)Keys.Enter)]       // SDLK_RETURN
    [InlineData(0x0000001bu, (int)Keys.Escape)]      // SDLK_ESCAPE
    [InlineData(0x00000009u, (int)Keys.Tab)]         // SDLK_TAB
    [InlineData(0x40000039u, (int)Keys.CapsLock)]    // SDLK_CAPSLOCK
    [InlineData(0x40000053u, (int)Keys.NumLock)]     // SDLK_NUMLOCKCLEAR
    [InlineData(0x40000047u, (int)Keys.Scroll)]      // SDLK_SCROLLLOCK
    [InlineData(0x40000048u, (int)Keys.Pause)]       // SDLK_PAUSE
    [InlineData(0x40000046u, (int)Keys.PrintScreen)] // SDLK_PRINTSCREEN
    [InlineData(0x40000102u, (int)Keys.Sleep)]       // SDLK_SLEEP
    [InlineData(0x40000080u, (int)Keys.VolumeUp)]    // SDLK_VOLUMEUP
    [InlineData(0x40000081u, (int)Keys.VolumeDown)]  // SDLK_VOLUMEDOWN
    // punctuation
    [InlineData(0x00000027u, (int)Keys.OemQuotes)]      // SDLK_APOSTROPHE
    [InlineData(0x0000002cu, (int)Keys.OemComma)]        // SDLK_COMMA
    [InlineData(0x0000002du, (int)Keys.OemMinus)]        // SDLK_MINUS
    [InlineData(0x0000002eu, (int)Keys.OemPeriod)]       // SDLK_PERIOD
    [InlineData(0x0000002fu, (int)Keys.OemQuestion)]     // SDLK_SLASH
    [InlineData(0x0000003bu, (int)Keys.OemSemicolon)]    // SDLK_SEMICOLON
    [InlineData(0x0000003du, (int)Keys.OemPlus)]         // SDLK_EQUALS
    [InlineData(0x0000005bu, (int)Keys.OemOpenBrackets)] // SDLK_LEFTBRACKET
    [InlineData(0x0000005cu, (int)Keys.OemPipe)]         // SDLK_BACKSLASH
    [InlineData(0x0000005du, (int)Keys.OemCloseBrackets)]// SDLK_RIGHTBRACKET
    [InlineData(0x00000060u, (int)Keys.OemTilde)]        // SDLK_GRAVE
    public void TranslateKeyboard_ReturnsKeyboardSourceEvent_WhenKeycodeKnown(uint keycode, int expectedKeyInt)
    {
        InputEvent? result = InputEventTranslator.TranslateKeyboard(keycode, isDown: true, timestampNs: 0UL);

        _ = result.Should().NotBeNull();
        _ = result!.Source.Should().Be(new KeyboardSource((Keys)expectedKeyInt));
    }

    [Fact]
    public void TranslateKeyboard_PreservesIsDown_WhenTrue()
    {
        InputEvent? result = InputEventTranslator.TranslateKeyboard(0x40000050u, isDown: true, timestampNs: 0UL);

        _ = result.Should().NotBeNull();
        _ = result!.IsDown.Should().BeTrue();
    }

    [Fact]
    public void TranslateKeyboard_PreservesIsDown_WhenFalse()
    {
        InputEvent? result = InputEventTranslator.TranslateKeyboard(0x40000050u, isDown: false, timestampNs: 0UL);

        _ = result.Should().NotBeNull();
        _ = result!.IsDown.Should().BeFalse();
    }

    [Fact]
    public void TranslateKeyboard_PreservesTimestampNs()
    {
        InputEvent? result = InputEventTranslator.TranslateKeyboard(0x40000050u, isDown: true, timestampNs: 12345UL);

        _ = result.Should().NotBeNull();
        _ = result!.TimestampNs.Should().Be(12345UL);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region TranslateMouse
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(0)]    // below valid SDL range (1-based, so 0 is unrecognised)
    [InlineData(99)]   // far above valid SDL range
    public void TranslateMouse_ReturnsNull_WhenButtonUnrecognised(byte sdlButton)
    {
        InputEvent? result = InputEventTranslator.TranslateMouse(sdlButton, isDown: true, timestampNs: 0UL);

        _ = result.Should().BeNull();
    }

    [Theory]
    [InlineData(1, (int)MouseButton.Left)]      // SDL button 1 → Left
    [InlineData(2, (int)MouseButton.Middle)]    // SDL button 2 → Middle
    [InlineData(3, (int)MouseButton.Right)]     // SDL button 3 → Right
    [InlineData(4, (int)MouseButton.X1)]        // SDL button 4 → X1
    [InlineData(5, (int)MouseButton.X2)]        // SDL button 5 → X2
    public void TranslateMouse_ReturnsMouseSourceEvent_ForEachSdlButton(byte sdlButton, int expectedButtonInt)
    {
        InputEvent? result = InputEventTranslator.TranslateMouse(sdlButton, isDown: true, timestampNs: 0UL);

        _ = result.Should().NotBeNull();
        _ = result!.Source.Should().Be(new MouseSource((MouseButton)expectedButtonInt));
    }

    [Fact]
    public void TranslateMouse_PreservesIsDown()
    {
        InputEvent? result = InputEventTranslator.TranslateMouse(sdlButton: 1, isDown: false, timestampNs: 0UL);

        _ = result.Should().NotBeNull();
        _ = result!.IsDown.Should().BeFalse();
    }

    [Fact]
    public void TranslateMouse_PreservesTimestampNs()
    {
        InputEvent? result = InputEventTranslator.TranslateMouse(sdlButton: 1, isDown: true, timestampNs: 99999UL);

        _ = result.Should().NotBeNull();
        _ = result!.TimestampNs.Should().Be(99999UL);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region TranslateGamepad
    // --------------------------------------------------------------------------

    [Fact]
    public void TranslateGamepad_ReturnsNull_WhenButtonUnrecognised()
    {
        InputEvent? result = InputEventTranslator.TranslateGamepad(sdlButton: 99, isDown: true, timestampNs: 0UL);

        _ = result.Should().BeNull();
    }

    [Theory]
    [InlineData(0, (int)Buttons.A)]              // SDL SOUTH  → A
    [InlineData(1, (int)Buttons.B)]              // SDL EAST   → B
    [InlineData(2, (int)Buttons.X)]              // SDL WEST   → X
    [InlineData(3, (int)Buttons.Y)]              // SDL NORTH  → Y
    [InlineData(4, (int)Buttons.Back)]           // SDL BACK   → Back
    [InlineData(5, (int)Buttons.BigButton)]      // SDL GUIDE  → BigButton
    [InlineData(6, (int)Buttons.Start)]          // SDL START  → Start
    [InlineData(7, (int)Buttons.LeftStick)]      // SDL LEFT_STICK  → LeftStick
    [InlineData(8, (int)Buttons.RightStick)]     // SDL RIGHT_STICK → RightStick
    [InlineData(9, (int)Buttons.LeftShoulder)]   // SDL LEFT_SHOULDER  → LeftShoulder
    [InlineData(10, (int)Buttons.RightShoulder)]  // SDL RIGHT_SHOULDER → RightShoulder
    [InlineData(11, (int)Buttons.DPadUp)]         // SDL DPAD_UP    → DPadUp
    [InlineData(12, (int)Buttons.DPadDown)]       // SDL DPAD_DOWN  → DPadDown
    [InlineData(13, (int)Buttons.DPadLeft)]       // SDL DPAD_LEFT  → DPadLeft
    [InlineData(14, (int)Buttons.DPadRight)]      // SDL DPAD_RIGHT → DPadRight
    public void TranslateGamepad_ReturnsGamePadSourceEvent_ForEachSdlButton(byte sdlButton, int expectedButtonInt)
    {
        InputEvent? result = InputEventTranslator.TranslateGamepad(sdlButton, isDown: true, timestampNs: 0UL);

        _ = result.Should().NotBeNull();
        _ = result!.Source.Should().Be(new GamePadSource((Buttons)expectedButtonInt));
    }

    [Fact]
    public void TranslateGamepad_PreservesIsDown()
    {
        InputEvent? result = InputEventTranslator.TranslateGamepad(sdlButton: 0, isDown: false, timestampNs: 0UL);

        _ = result.Should().NotBeNull();
        _ = result!.IsDown.Should().BeFalse();
    }

    [Fact]
    public void TranslateGamepad_PreservesTimestampNs()
    {
        InputEvent? result = InputEventTranslator.TranslateGamepad(sdlButton: 0, isDown: true, timestampNs: 77777UL);

        _ = result.Should().NotBeNull();
        _ = result!.TimestampNs.Should().Be(77777UL);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
}
