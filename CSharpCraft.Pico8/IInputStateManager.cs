namespace CSharpCraft.Pico8;

/// <summary>
/// Manages input state and button processing for the game.
/// Abstracts away button state management from Pico8Functions.
/// Phase 8: Input Handling Extraction
/// </summary>
public interface IInputStateManager
{
    /// <summary>
    /// Checks if a button is currently held.
    /// </summary>
    /// <param name="buttonIndex">Button index (0-7 for game buttons, values defined by InputBindings)</param>
    /// <param name="player">Player number (default 0)</param>
    /// <returns>True if button is held</returns>
    bool Btn(int buttonIndex, int player = 0);

    /// <summary>
    /// Checks if a button was just pressed (first frame of press).
    /// </summary>
    /// <param name="buttonIndex">Button index</param>
    /// <param name="player">Player number (default 0)</param>
    /// <returns>True if button just pressed</returns>
    bool Btnp(int buttonIndex, int player = 0);

    /// <summary>
    /// Resets button state when a new scene loads.
    /// </summary>
    void Reset(Pico8Functions context);

    /// <summary>
    /// Updates button state based on current input.
    /// Processes hold/press logic and input lockout.
    /// </summary>
    void Update(Pico8Functions context);

    /// <summary>
    /// Sets whether input should be locked out (during pause menu).
    /// </summary>
    void SetPauseMode(bool isPauseMode);

    /// <summary>
    /// Checks if pause button (button 6) was just pressed.
    /// </summary>
    bool IsPauseButtonPressed();

    /// <summary>
    /// Updates pause-specific button handling (button 6).
    /// Called separately in Update() before UpLockout.
    /// </summary>
    void UpdatePauseButton(Pico8Functions context);

    /// <summary>
    /// Updates button lockout state based on pause mode.
    /// Prevents accidental input during menu navigation.
    /// </summary>
    void UpdateLockout(Pico8Functions context);
}
