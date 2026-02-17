using FixMath;

namespace CSharpCraft.Pico8;

/// <summary>
/// Manages input state tracking for buttons and controllers
/// </summary>
public interface IInputManager
{
    /// <summary>
    /// Check if button i is currently held down (Pico-8 btn function)
    /// </summary>
    /// <param name="i">Button code (0-7, see P8Btns for mapping)</param>
    /// <param name="p">Player number (default 0)</param>
    bool Btn(int i, int p = 0);

    /// <summary>
    /// Check if button i was just pressed this frame (Pico-8 btnp function)
    /// </summary>
    /// <param name="i">Button code (0-7)</param>
    /// <param name="p">Player number (default 0)</param>
    bool Btnp(int i, int p = 0);

    /// <summary>
    /// Get current button state
    /// </summary>
    P8Btns GetButtonState(int playerIndex = 0);

    /// <summary>
    /// Get analog stick position (if available)
    /// </summary>
    (F32 x, F32 y) GetAnalogStick(int playerIndex = 0);
}
