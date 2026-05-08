using Microsoft.Xna.Framework.Input;
using PSharp8.Input;

namespace CSharpCraft.Input;

internal static class InputEventTranslator
{
    // SDL_Keycode to XNA Keys
    private static readonly Dictionary<uint, Keys> KeycodeMap = new()
    {
        [0x00000061u] = Keys.A,
        [0x00000062u] = Keys.B,
        [0x00000063u] = Keys.C,
        [0x00000064u] = Keys.D,
        [0x00000065u] = Keys.E,
        [0x00000066u] = Keys.F,
        [0x00000067u] = Keys.G,
        [0x00000068u] = Keys.H,
        [0x00000069u] = Keys.I,
        [0x0000006au] = Keys.J,
        [0x0000006bu] = Keys.K,
        [0x0000006cu] = Keys.L,
        [0x0000006du] = Keys.M,
        [0x0000006eu] = Keys.N,
        [0x0000006fu] = Keys.O,
        [0x00000070u] = Keys.P,
        [0x00000071u] = Keys.Q,
        [0x00000072u] = Keys.R,
        [0x00000073u] = Keys.S,
        [0x00000074u] = Keys.T,
        [0x00000075u] = Keys.U,
        [0x00000076u] = Keys.V,
        [0x00000077u] = Keys.W,
        [0x00000078u] = Keys.X,
        [0x00000079u] = Keys.Y,
        [0x0000007au] = Keys.Z,
        [0x00000030u] = Keys.D0,
        [0x00000031u] = Keys.D1,
        [0x00000032u] = Keys.D2,
        [0x00000033u] = Keys.D3,
        [0x00000034u] = Keys.D4,
        [0x00000035u] = Keys.D5,
        [0x00000036u] = Keys.D6,
        [0x00000037u] = Keys.D7,
        [0x00000038u] = Keys.D8,
        [0x00000039u] = Keys.D9,

        [0x40000059u] = Keys.NumPad1,
        [0x4000005au] = Keys.NumPad2,
        [0x4000005bu] = Keys.NumPad3,
        [0x4000005cu] = Keys.NumPad4,
        [0x4000005du] = Keys.NumPad5,
        [0x4000005eu] = Keys.NumPad6,
        [0x4000005fu] = Keys.NumPad7,
        [0x40000060u] = Keys.NumPad8,
        [0x40000061u] = Keys.NumPad9,
        [0x40000062u] = Keys.NumPad0,
        [0x400000d8u] = Keys.OemClear,   // KP_CLEAR
        [0x400000dcu] = Keys.Decimal,    // KP_DECIMAL
        [0x40000054u] = Keys.Divide,     // KP_DIVIDE
        [0x40000058u] = Keys.Enter,      // KP_ENTER
        [0x40000056u] = Keys.Subtract,   // KP_MINUS
        [0x40000055u] = Keys.Multiply,   // KP_MULTIPLY
        [0x40000063u] = Keys.OemPeriod,  // KP_PERIOD
        [0x40000057u] = Keys.Add,        // KP_PLUS

        [0x4000003au] = Keys.F1,
        [0x4000003bu] = Keys.F2,
        [0x4000003cu] = Keys.F3,
        [0x4000003du] = Keys.F4,
        [0x4000003eu] = Keys.F5,
        [0x4000003fu] = Keys.F6,
        [0x40000040u] = Keys.F7,
        [0x40000041u] = Keys.F8,
        [0x40000042u] = Keys.F9,
        [0x40000043u] = Keys.F10,
        [0x40000044u] = Keys.F11,
        [0x40000045u] = Keys.F12,
        [0x40000068u] = Keys.F13,
        [0x40000069u] = Keys.F14,
        [0x4000006au] = Keys.F15,
        [0x4000006bu] = Keys.F16,
        [0x4000006cu] = Keys.F17,
        [0x4000006du] = Keys.F18,
        [0x4000006eu] = Keys.F19,
        [0x4000006fu] = Keys.F20,
        [0x40000070u] = Keys.F21,
        [0x40000071u] = Keys.F22,
        [0x40000072u] = Keys.F23,
        [0x40000073u] = Keys.F24,

        [0x40000050u] = Keys.Left,
        [0x4000004fu] = Keys.Right,
        [0x40000052u] = Keys.Up,
        [0x40000051u] = Keys.Down,
        [0x4000004au] = Keys.Home,
        [0x4000004du] = Keys.End,
        [0x40000049u] = Keys.Insert,
        [0x0000007fu] = Keys.Delete,
        [0x4000004bu] = Keys.PageUp,
        [0x4000004eu] = Keys.PageDown,

        [0x400000e2u] = Keys.LeftAlt,
        [0x400000e6u] = Keys.RightAlt,
        [0x400000e0u] = Keys.LeftControl,
        [0x400000e4u] = Keys.RightControl,
        [0x400000e3u] = Keys.LeftWindows,
        [0x400000e7u] = Keys.RightWindows,
        [0x400000e1u] = Keys.LeftShift,
        [0x400000e5u] = Keys.RightShift,
        [0x40000065u] = Keys.Apps,        // APPLICATION
        [0x40000076u] = Keys.Apps,        // MENU

        [0x00000020u] = Keys.Space,
        [0x00000008u] = Keys.Back,        // BACKSPACE
        [0x0000000du] = Keys.Enter,       // RETURN
        [0x0000001bu] = Keys.Escape,
        [0x00000009u] = Keys.Tab,
        [0x40000039u] = Keys.CapsLock,
        [0x40000053u] = Keys.NumLock,
        [0x40000047u] = Keys.Scroll,      // SCROLLLOCK
        [0x40000048u] = Keys.Pause,
        [0x40000046u] = Keys.PrintScreen,
        [0x40000102u] = Keys.Sleep,
        [0x40000080u] = Keys.VolumeUp,
        [0x40000081u] = Keys.VolumeDown,

        [0x00000027u] = Keys.OemQuotes,       // APOSTROPHE
        [0x0000002cu] = Keys.OemComma,        // COMMA
        [0x0000002du] = Keys.OemMinus,        // MINUS
        [0x0000002eu] = Keys.OemPeriod,       // PERIOD
        [0x0000002fu] = Keys.OemQuestion,     // SLASH
        [0x0000003bu] = Keys.OemSemicolon,    // SEMICOLON
        [0x0000003du] = Keys.OemPlus,         // EQUALS
        [0x0000005bu] = Keys.OemOpenBrackets, // LEFTBRACKET
        [0x0000005cu] = Keys.OemPipe,         // BACKSLASH
        [0x0000005du] = Keys.OemCloseBrackets,// RIGHTBRACKET
        [0x00000060u] = Keys.OemTilde,        // GRAVE
    };

    // SDL mouse button byte to PSharp8 MouseButton
    private static readonly Dictionary<byte, MouseButton> MouseButtonMap = new()
    {
        [1] = MouseButton.Left,
        [2] = MouseButton.Middle,
        [3] = MouseButton.Right,
        [4] = MouseButton.X1,
        [5] = MouseButton.X2,
    };

    // SDL_GamepadButton byte to XNA Buttons
    private static readonly Dictionary<byte, Buttons> GamepadButtonMap = new()
    {
        [0] = Buttons.A,
        [1] = Buttons.B,
        [2] = Buttons.X,
        [3] = Buttons.Y,
        [4] = Buttons.Back,
        [5] = Buttons.BigButton,
        [6] = Buttons.Start,
        [7] = Buttons.LeftStick,
        [8] = Buttons.RightStick,
        [9] = Buttons.LeftShoulder,
        [10] = Buttons.RightShoulder,
        [11] = Buttons.DPadUp,
        [12] = Buttons.DPadDown,
        [13] = Buttons.DPadLeft,
        [14] = Buttons.DPadRight,
    };

    internal static InputEvent? TranslateKeyboard(uint keycode, bool isDown, ulong timestampNs)
    {
        return !KeycodeMap.TryGetValue(keycode, out Keys key) ? null : new InputEvent(new KeyboardSource(key), isDown, timestampNs);
    }

    internal static InputEvent? TranslateMouse(byte sdlButton, bool isDown, ulong timestampNs)
    {
        return !MouseButtonMap.TryGetValue(sdlButton, out MouseButton button)
            ? null
            : new InputEvent(new MouseSource(button), isDown, timestampNs);
    }

    internal static InputEvent? TranslateGamepad(byte sdlButton, bool isDown, ulong timestampNs)
    {
        return !GamepadButtonMap.TryGetValue(sdlButton, out Buttons button)
            ? null
            : new InputEvent(new GamePadSource(button), isDown, timestampNs);
    }
}
