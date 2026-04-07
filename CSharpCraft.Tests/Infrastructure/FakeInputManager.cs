using PSharp8.Input;

namespace CSharpCraft.Tests.Infrastructure;

/// <summary>
/// A controllable IInputManager for use in tests that need to simulate button presses.
/// Btnp auto-consumes on first read per frame, matching real PICO-8 semantics.
/// </summary>
public sealed class FakeInputManager : IInputManager
{
    private const int ButtonCount = 7;
    private readonly bool[] _btn  = new bool[ButtonCount];
    private readonly bool[] _btnp = new bool[ButtonCount];

    public bool InputBlocked { get; set; }

    /// Sets the held state of a button (Btn).
    public void SetBtn(int button, bool value) => _btn[button] = value;

    /// Queues a single press for the button — consumed on the next Btnp() call.
    public void PressOnce(int button) => _btnp[button] = true;

    public bool Btn(int button, int player)
    {
        if (InputBlocked) return false;
        return _btn[button];
    }

    public bool Btnp(int button, int player)
    {
        if (InputBlocked) return false;
        var value = _btnp[button];
        _btnp[button] = false; // auto-consume: once per frame, matching real InputManager
        return value;
    }

    public void Update(TimeSpan elapsed, IReadOnlyList<InputEvent> events) { }
    public void SetBindings(InputBindings bindings) { }
}
