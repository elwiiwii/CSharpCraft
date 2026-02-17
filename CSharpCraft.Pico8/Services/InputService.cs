using FixMath;
using System.Collections.Generic;

namespace CSharpCraft.Pico8.Services;

/// <summary>
/// Service responsible for input management and button tracking
/// Extracted from Pico8Functions to enable service-based architecture
/// Phase 3: Service Extraction - Part 3
/// </summary>
public class InputService : IInputManager
{
    private readonly IInputBindingProvider _inputBindings;
    private readonly P8Btns _buttons;
    private bool[] _currentButtonStates;
    private bool[] _previousButtonStates;

    public InputService(IInputBindingProvider inputBindings)
    {
        _inputBindings = inputBindings ?? throw new ArgumentNullException(nameof(inputBindings));
        _buttons = new P8Btns();
        _currentButtonStates = new bool[7];
        _previousButtonStates = new bool[7];
    }

    /// <summary>
    /// Check if button is currently held down (Pico-8: btn)
    /// </summary>
    public bool Btn(int i, int p = 0)
    {
        if (i < 0 || i >= 7) return false;
        return _currentButtonStates[i];
    }

    /// <summary>
    /// Check if button was just pressed this frame (Pico-8: btnp)
    /// Simplified: returns true if currently pressed
    /// Full implementation would need frame-based state tracking
    /// </summary>
    public bool Btnp(int i, int p = 0)
    {
        if (i < 0 || i >= 7) return false;
        return _currentButtonStates[i] && !_previousButtonStates[i];
    }

    /// <summary>
    /// Get current button state struct
    /// </summary>
    public P8Btns GetButtonState(int playerIndex = 0)
    {
        var state = new P8Btns();
        for (int i = 0; i < 7; i++)
        {
            state.Prev[i] = _currentButtonStates[i];
        }
        return state;
    }

    /// <summary>
    /// Get analog stick position (if available)
    /// </summary>
    public (F32 x, F32 y) GetAnalogStick(int playerIndex = 0)
    {
        // Placeholder - real implementation would check controller analog sticks
        return (F32.Zero, F32.Zero);
    }

    /// <summary>
    /// Update button states from input
    /// </summary>
    public void Update()
    {
        // Shift previous state
        for (int i = 0; i < 7; i++)
        {
            _previousButtonStates[i] = _currentButtonStates[i];
        }

        // Read current button bindings
        UpdateButtonStates();
    }

    /// <summary>
    /// Internal method to poll current button states
    /// </summary>
    private void UpdateButtonStates()
    {
        // Get keyboard and controller state (simplified - would use actual input system)
        // Button mapping:
        // 0 = Left, 1 = Right, 2 = Up, 3 = Down, 4 = Menu, 5 = Use/Action, 6 = Pause

        // In real implementation, would check:
        // - Keyboard input based on _inputBindings
        // - Controller input (DPad, analog stick)
        // - Touch input (if supported)

        // For now, keep previous state (service will receive input from host)
    }

    /// <summary>
    /// Inject button state from external input system
    /// Called by the host (Pico8Functions or test framework)
    /// </summary>
    public void SetButtonState(int buttonIndex, bool pressed)
    {
        if (buttonIndex >= 0 && buttonIndex < 7)
        {
            _currentButtonStates[buttonIndex] = pressed;
        }
    }

    /// <summary>
    /// Get Lua-style loop value for a button
    /// Implements Pico-8's Ptn() pattern
    /// </summary>
    public int GetButtonLoopValue(int i)
    {
        if (i < 0 || i >= 7) return 0;

        // Pico-8 pattern: loop value increases while held, resets when released
        // 0 = not held, 1 = first frame, 2-... = subsequent frames
        if (_currentButtonStates[i])
        {
            if (_previousButtonStates[i])
                return _buttons.HeldCount[i] + 1; // Continuing to hold
            else
                return 1; // Just pressed
        }
        return 0; // Not held
    }

    /// <summary>
    /// Reset input state (useful for level transitions)
    /// </summary>
    public void Reset()
    {
        for (int i = 0; i < 7; i++)
        {
            _currentButtonStates[i] = false;
            _previousButtonStates[i] = false;
            _buttons.Prev[i] = false;
        }
    }
}
