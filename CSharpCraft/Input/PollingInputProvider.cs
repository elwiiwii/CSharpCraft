using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PSharp8.Input;

namespace CSharpCraft.Input;

internal sealed class PollingInputProvider : IInputProvider
{
    private InputBindings _bindings;
    private readonly Func<KeyboardState> _getKeyboard;
    private readonly Func<GamePadState> _getGamePad;
    private readonly Func<MouseState> _getMouse;

    /// <summary>Production constructor — reads live hardware state each frame.</summary>
    public PollingInputProvider(InputBindings bindings)
        : this(bindings,
               () => Keyboard.GetState(),
               () => GamePad.GetState(PlayerIndex.One),
               () => Mouse.GetState())
    {
    }

    /// <summary>Testable constructor — callers inject state-supplier delegates.</summary>
    internal PollingInputProvider(
        InputBindings bindings,
        Func<KeyboardState> getKeyboard,
        Func<GamePadState> getGamePad,
        Func<MouseState> getMouse)
    {
        _bindings   = bindings    ?? throw new ArgumentNullException(nameof(bindings));
        _getKeyboard = getKeyboard ?? throw new ArgumentNullException(nameof(getKeyboard));
        _getGamePad  = getGamePad  ?? throw new ArgumentNullException(nameof(getGamePad));
        _getMouse    = getMouse    ?? throw new ArgumentNullException(nameof(getMouse));
    }

    public void SetBindings(InputBindings bindings)
    {
        _bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
    }

    public bool[] GetHeldButtons()
    {
        KeyboardState keyboard = _getKeyboard();
        GamePadState  gamepad  = _getGamePad();
        MouseState    mouse    = _getMouse();

        var held = new bool[7];
        foreach (PicoButton button in Enum.GetValues<PicoButton>())
        {
            int i = (int)button;
            foreach (InputSource source in _bindings[button])
            {
                if (IsSourceHeld(source, keyboard, gamepad, mouse))
                {
                    held[i] = true;
                    break;
                }
            }
        }
        return held;
    }

    private static bool IsSourceHeld(
        InputSource source,
        KeyboardState keyboard,
        GamePadState gamepad,
        MouseState mouse)
        => source switch
        {
            KeyboardSource ks => keyboard.IsKeyDown(ks.Key),
            GamePadSource  gs => gamepad.IsButtonDown(gs.Button),
            MouseSource    ms => IsMouseButtonDown(mouse, ms.Button),
            _                 => false,
        };

    private static bool IsMouseButtonDown(MouseState state, MouseButton button)
        => button switch
        {
            MouseButton.Left   => state.LeftButton   == ButtonState.Pressed,
            MouseButton.Right  => state.RightButton  == ButtonState.Pressed,
            MouseButton.Middle => state.MiddleButton == ButtonState.Pressed,
            MouseButton.X1     => state.XButton1     == ButtonState.Pressed,
            MouseButton.X2     => state.XButton2     == ButtonState.Pressed,
            _                  => false,
        };
}
