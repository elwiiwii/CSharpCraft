namespace CSharpCraft.Pico8;

/// <summary>
/// Manages input state and button processing.
/// Encapsulates P8Btns and provides clean input query interface.
/// Phase 8: Input Handling Extraction
/// </summary>
public class InputStateManager : IInputStateManager
{
    private readonly P8Btns _buttons;
    private bool _isPauseMode;

    /// <summary>
    /// Gets the underlying button state (for compatibility with button management).
    /// </summary>
    public P8Btns Buttons => _buttons;

    public InputStateManager()
    {
        _buttons = new P8Btns();
        _isPauseMode = false;
    }

    /// <summary>
    /// Checks if a button is currently held.
    /// Queries input through Pico8Functions context.
    /// </summary>
    public bool Btn(int buttonIndex, int player = 0)
    {
        // Note: Actual button query implementation is in Pico8Functions.Btn()
        // This is a wrapper that will be used through the manager interface
        // The real implementation requires access to InputBindings/GameWindow
        return false; // Default stub - overridden by Pico8Functions
    }

    /// <summary>
    /// Checks if a button was just pressed (first frame).
    /// Uses HeldCount and Lockout from P8Btns.
    /// </summary>
    public bool Btnp(int buttonIndex, int player = 0)
    {
        // Btnp logic: button just pressed if HeldCount == 1 and not locked out
        if (buttonIndex < _buttons.Prev.Length && _buttons.HeldCount.Length > buttonIndex)
        {
            bool justPressed = _buttons.HeldCount[buttonIndex] == 1 && !_buttons.Lockout[buttonIndex];
            return justPressed;
        }
        return false;
    }

    /// <summary>
    /// Resets button state when a new scene loads.
    /// Clears held counts and lockout flags.
    /// </summary>
    public void Reset(Pico8Functions context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _buttons.Reset(context);
    }

    /// <summary>
    /// Updates button state based on current input.
    /// Processes hold/press logic and updates previous button states.
    /// </summary>
    public void Update(Pico8Functions context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _buttons.Update(context);
    }

    /// <summary>
    /// Manages button state transitions for pause/unpause.
    /// </summary>
    public void SetPauseMode(bool isPauseMode)
    {
        _isPauseMode = isPauseMode;
    }

    /// <summary>
    /// Checks if pause button (button 6) was just pressed.
    /// Used to toggle pause menu state.
    /// </summary>
    public bool IsPauseButtonPressed()
    {
        // Button 6 is pause - check if just pressed
        return _buttons.HeldCount.Length > 6 && _buttons.HeldCount[6] == 1;
    }

    /// <summary>
    /// Updates pause-specific button handling.
    /// Called separately in Update() before UpLockout.
    /// </summary>
    public void UpdatePauseButton(Pico8Functions context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _buttons.UpPause(context);
    }

    /// <summary>
    /// Updates button lockout state based on pause mode.
    /// Prevents accidental input during menu navigation.
    /// </summary>
    public void UpdateLockout(Pico8Functions context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _buttons.UpLockout(context, _isPauseMode);
    }
}
