using CSharpCraft.Input;
using FluentAssertions;
using Microsoft.Xna.Framework.Input;
using PSharp8.Input;
using Xunit;

namespace CSharpCraft.Tests.Input;

public sealed class PollingInputProviderTests
{
    // --------------------------------------------------------------------------
    #region Helpers
    // --------------------------------------------------------------------------

    private static PollingInputProvider CreateSut(
        InputBindings? bindings = null,
        Func<KeyboardState>? getKeyboard = null,
        Func<GamePadState>? getGamePad = null,
        Func<MouseState>? getMouse = null)
    {
        return new(
                bindings ?? InputBindings.Default,
                getKeyboard ?? (() => default),
                getGamePad ?? (() => default),
                getMouse ?? (() => default));
    }

    private static KeyboardState KeysDown(params Keys[] keys)
    {
        return new(keys);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenBindingsIsNull()
    {
        Func<PollingInputProvider> act = () => new PollingInputProvider(bindings: null!);

        _ = act.Should().Throw<ArgumentNullException>()
           .WithParameterName("bindings");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region SetBindings
    // --------------------------------------------------------------------------

    [Fact]
    public void SetBindings_ThrowsArgumentNullException_WhenBindingsIsNull()
    {
        PollingInputProvider sut = CreateSut();

        Action act = () => sut.SetBindings(null!);

        _ = act.Should().Throw<ArgumentNullException>()
           .WithParameterName("bindings");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetHeldButtons — no input
    // --------------------------------------------------------------------------

    [Fact]
    public void GetHeldButtons_ReturnsFalseForAll_WhenNoInputHeld()
    {
        PollingInputProvider sut = CreateSut(getKeyboard: () => default);

        _ = sut.GetHeldButtons().Should().AllBeEquivalentTo(false);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetHeldButtons — keyboard
    // --------------------------------------------------------------------------

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForLeft_WhenLeftArrowHeld()
    {
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.Left));

        _ = sut.GetHeldButtons()[(int)PicoButton.Left].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForLeft_WhenAlternateKeyHeld()
    {
        // A is an alternate binding for Left in default bindings
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.A));

        _ = sut.GetHeldButtons()[(int)PicoButton.Left].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForRight_WhenRightArrowHeld()
    {
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.Right));

        _ = sut.GetHeldButtons()[(int)PicoButton.Right].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForUp_WhenUpArrowHeld()
    {
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.Up));

        _ = sut.GetHeldButtons()[(int)PicoButton.Up].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForDown_WhenDownArrowHeld()
    {
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.Down));

        _ = sut.GetHeldButtons()[(int)PicoButton.Down].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForPrimary_WhenZHeld()
    {
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.Z));

        _ = sut.GetHeldButtons()[(int)PicoButton.Primary].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForSecondary_WhenXHeld()
    {
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.X));

        _ = sut.GetHeldButtons()[(int)PicoButton.Secondary].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForPause_WhenEscapeHeld()
    {
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.Escape));

        _ = sut.GetHeldButtons()[(int)PicoButton.Pause].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsFalse_ForUnboundKey()
    {
        // OemTilde is not bound to any PicoButton in default bindings
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.OemTilde));

        _ = sut.GetHeldButtons().Should().AllBeEquivalentTo(false);
    }

    [Fact]
    public void GetHeldButtons_ReturnsFalse_ForOtherButtons_WhenOnlyLeftHeld()
    {
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.Left));
        bool[] held = sut.GetHeldButtons();

        for (int i = 1; i < 7; i++)
            _ = held[i].Should().BeFalse($"only Left (index 0) should be held, not index {i}");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetHeldButtons — SetBindings rebinding
    // --------------------------------------------------------------------------

    [Fact]
    public void GetHeldButtons_RespectsUpdatedBindings()
    {
        // Q is not in default bindings for Left
        PollingInputProvider sut = CreateSut(getKeyboard: () => KeysDown(Keys.Q));
        _ = sut.GetHeldButtons()[(int)PicoButton.Left].Should().BeFalse();

        InputBindings newBindings = new(new Dictionary<PicoButton, IReadOnlyList<InputSource>>
        {
            [PicoButton.Left] = [new KeyboardSource(Keys.Q)],
        });
        sut.SetBindings(newBindings);

        _ = sut.GetHeldButtons()[(int)PicoButton.Left].Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetHeldButtons — mouse
    // --------------------------------------------------------------------------

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForPrimary_WhenMouseLeftHeld()
    {
        InputBindings bindings = new(new Dictionary<PicoButton, IReadOnlyList<InputSource>>
        {
            [PicoButton.Primary] = [new MouseSource(MouseButton.Left)],
        });
        var mouseState = new MouseState(0, 0, 0,
            leftButton: ButtonState.Pressed,
            middleButton: ButtonState.Released,
            rightButton: ButtonState.Released,
            xButton1: ButtonState.Released,
            xButton2: ButtonState.Released);

        PollingInputProvider sut = CreateSut(bindings: bindings, getMouse: () => mouseState);

        _ = sut.GetHeldButtons()[(int)PicoButton.Primary].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsFalse_ForPrimary_WhenMouseLeftNotHeld()
    {
        InputBindings bindings = new(new Dictionary<PicoButton, IReadOnlyList<InputSource>>
        {
            [PicoButton.Primary] = [new MouseSource(MouseButton.Left)],
        });

        PollingInputProvider sut = CreateSut(bindings: bindings, getMouse: () => default);

        _ = sut.GetHeldButtons()[(int)PicoButton.Primary].Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
}
