namespace CSharpCraft.Pico8;

/// <summary>
/// Manages input state and button processing.
/// Encapsulates P8Btns and provides clean input query interface.
/// Uses the Pico8 static API for button state queries.
/// </summary>
public class InputStateManager : IInputStateManager
{
    private readonly P8Btns _buttons;
    private bool _isPauseMode;

    public P8Btns Buttons => _buttons;

    public InputStateManager()
    {
        _buttons = new P8Btns();
        _isPauseMode = false;
    }

    public bool Btn(int buttonIndex, int player = 0)
    {
        return Pico8Utils.Ptn(buttonIndex, player);
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
        _buttons.UpLockout(_isPauseMode);
    }
}
