namespace CSharpCraft.Pico8;

using Microsoft.Xna.Framework.Input;

/// <summary>
/// Manages input state and button processing for the game.
/// Uses the Pico8 static API for button state queries.
/// </summary>
public interface IInputStateManager
{
    /// <summary>
    /// Checks if a button is currently held.
    /// </summary>
    bool Btn(int buttonIndex, int player = 0);

    /// <summary>
    /// Checks if a button was just pressed (first frame of press).
    /// </summary>
    bool Btnp(int buttonIndex, int player = 0);

    /// <summary>
    /// Resets button state when a new scene loads.
    /// </summary>
    void Reset();

    /// <summary>
    /// Updates button state based on current input.
    /// </summary>
    void Update();

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
    /// </summary>
    void UpdatePauseButton();

    /// <summary>
    /// Updates button lockout state based on pause mode.
    /// </summary>
    void UpdateLockout();

    /// <summary>
    /// Checks if a raw keyboard key is currently held down.
    /// </summary>
    bool IsKeyDown(Keys key);

    /// <summary>
    /// Checks if a raw keyboard key was just pressed this frame (rising edge).
    /// </summary>
    bool IsKeyJustPressed(Keys key);
}
