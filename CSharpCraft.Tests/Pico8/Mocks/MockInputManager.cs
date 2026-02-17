using CSharpCraft.Pico8;
using FixMath;

namespace CSharpCraft.Tests.Pico8.Mocks;

/// <summary>
/// Mock input manager for testing button states
/// Tracks button state for up to 4 players
/// </summary>
public class MockInputManager : IInputManager
{
    private readonly bool[][] _playerButtonStates;

    public MockInputManager()
    {
        // Support up to 4 players, 7 buttons each
        _playerButtonStates = new bool[4][];
        for (int p = 0; p < 4; p++)
        {
            _playerButtonStates[p] = new bool[7];
        }
    }

    public void SetButtonState(int button, bool pressed, int playerIndex = 0)
    {
        if (playerIndex >= 0 && playerIndex < _playerButtonStates.Length &&
            button >= 0 && button < _playerButtonStates[playerIndex].Length)
        {
            _playerButtonStates[playerIndex][button] = pressed;
        }
    }

    public bool Btn(int i, int p = 0)
    {
        if (p >= 0 && p < _playerButtonStates.Length &&
            i >= 0 && i < _playerButtonStates[p].Length)
        {
            return _playerButtonStates[p][i];
        }
        return false;
    }

    public bool Btnp(int i, int p = 0) => Btn(i, p); // Simplified - Phase 2 mock

    public P8Btns GetButtonState(int playerIndex = 0)
    {
        var state = new P8Btns();
        if (playerIndex >= 0 && playerIndex < _playerButtonStates.Length)
        {
            for (int i = 0; i < 7; i++)
            {
                state.Prev[i] = _playerButtonStates[playerIndex][i];
            }
        }
        return state;
    }

    public (F32 x, F32 y) GetAnalogStick(int playerIndex = 0)
    {
        return (F32.Zero, F32.Zero);
    }
}
