using Microsoft.Xna.Framework.Input;

namespace CSharpCraft.Pico8.Input;

/// <summary>
/// Manages input state and button processing.
/// Encapsulates P8Btns and provides clean input query interface.
/// Receives InputBindings explicitly to avoid coupling to Pico8 static class.
/// Also tracks raw keyboard state for system hotkeys.
/// </summary>
public class InputStateManager : IInputStateManager
{
    private readonly P8Btns _buttons;
    private readonly IInputBindingProvider _inputBindings;
    private bool _isPauseMode;
    private KeyboardState _prevKeyState;
    private KeyboardState _curKeyState;

    public P8Btns Buttons => _buttons;

    public InputStateManager(IInputBindingProvider? inputBindings = null)
    {
        _buttons = new P8Btns();
        _inputBindings = inputBindings ?? DefaultInputBindings.Instance;
        _isPauseMode = false;
        _prevKeyState = Keyboard.GetState();
        _curKeyState = _prevKeyState;
    }

    public bool Btn(int buttonIndex, int player = 0)
    {
        return Pico8Utils.Ptn(buttonIndex, _inputBindings, player);
    }

    public bool Btnp(int buttonIndex, int player = 0)
    {
        if (buttonIndex < _buttons.Prev.Length && _buttons.HeldCount.Length > buttonIndex)
        {
            return _buttons.HeldCount[buttonIndex] == 1 && !_buttons.Lockout[buttonIndex];
        }
        return false;
    }

    public void Reset()
    {
        _buttons.Reset();
    }

    public void Update()
    {
        _buttons.Update();
        _prevKeyState = _curKeyState;
        _curKeyState = Keyboard.GetState();
    }

    public void SetPauseMode(bool isPauseMode)
    {
        _isPauseMode = isPauseMode;
    }

    public bool IsPauseButtonPressed()
    {
        return _buttons.HeldCount.Length > 6 && _buttons.HeldCount[6] == 1;
    }

    public void UpdatePauseButton()
    {
        _buttons.UpPause();
    }

    public void UpdateLockout()
    {
        _buttons.UpLockout(_isPauseMode, _inputBindings);
    }

    public bool IsKeyDown(Keys key)
    {
        return _curKeyState.IsKeyDown(key);
    }

    public bool IsKeyJustPressed(Keys key)
    {
        return _curKeyState.IsKeyDown(key) && !_prevKeyState.IsKeyDown(key);
    }
}
