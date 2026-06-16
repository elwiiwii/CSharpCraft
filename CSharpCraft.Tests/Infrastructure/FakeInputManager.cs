using PSharp8.Input;

namespace CSharpCraft.Tests.Infrastructure;

/// <summary>
/// A controllable IInputManager for use in tests that need to simulate button presses.
/// Btnp auto-consumes on first read per frame, matching real PICO-8 semantics.
/// </summary>
public sealed class FakeInputManager : IInputManager
{
    private const int ButtonCount = 7;
    private readonly bool[] _btn = new bool[ButtonCount];
    private readonly bool[] _btnp = new bool[ButtonCount];

    public bool InputBlocked { get; set; }

    /// Sets the held state of a button (Btn).
    public void SetBtn(int button, bool value)
    {
        _btn[button] = value;
    }

    /// Queues a single press for the button — consumed on the next Btnp() call.
    public void PressOnce(int button)
    {
        _btnp[button] = true;
    }

    public int Btn()
    {
        if (InputBlocked) return 0;
        int result = 0;
        for (int i = 0; i < ButtonCount; i++)
        {
            if (_btn[i])
                result |= (1 << i);
        }
        return result;
    }

    public bool Btn(PicoButton button, int player)
    {
        return !InputBlocked && _btn[(int)button];
    }

    public bool Btnp(PicoButton button, int player, bool repeat = true)
    {
        if (InputBlocked) return false;
        var i = (int)button;
        bool value = _btnp[i];
        _btnp[i] = false; // auto-consume: once per frame, matching real InputManager
        return value;
    }

    public PicoMouseState MouseState() => default;

    public void Update(TimeSpan elapsed) { }

    public void ResetInputStates()
    {
        Array.Clear(_btn, 0, ButtonCount);
        Array.Clear(_btnp, 0, ButtonCount);
    }

    public void ConsumePressedFlags()
    {
        Array.Clear(_btnp, 0, ButtonCount);
    }
}
