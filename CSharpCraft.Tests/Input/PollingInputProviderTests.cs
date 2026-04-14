using CSharpCraft.Input;
using FluentAssertions;
using Microsoft.Xna.Framework;
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
        => new(
            bindings     ?? InputBindings.Default,
            getKeyboard  ?? (() => default),
            getGamePad   ?? (() => default),
            getMouse     ?? (() => default));

    private static KeyboardState KeysDown(params Keys[] keys) => new KeyboardState(keys);

    // --------------------------------------------------------------------------
    #endregion
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenBindingsIsNull()
    {
        var act = () => new PollingInputProvider(bindings: null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("bindings");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region SetBindings
    // --------------------------------------------------------------------------

    [Fact]
    public void SetBindings_ThrowsArgumentNullException_WhenBindingsIsNull()
    {
        var sut = CreateSut();

        var act = () => sut.SetBindings(null!);

        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("bindings");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetHeldButtons — no input
    // --------------------------------------------------------------------------

    [Fact]
    public void GetHeldButtons_ReturnsFalseForAll_WhenNoInputHeld()
    {
        var sut = CreateSut(getKeyboard: () => default);

        sut.GetHeldButtons().Should().AllBeEquivalentTo(false);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetHeldButtons — keyboard
    // --------------------------------------------------------------------------

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForLeft_WhenLeftArrowHeld()
    {
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.Left));

        sut.GetHeldButtons()[(int)PicoButton.Left].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForLeft_WhenAlternateKeyHeld()
    {
        // A is an alternate binding for Left in default bindings
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.A));

        sut.GetHeldButtons()[(int)PicoButton.Left].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForRight_WhenRightArrowHeld()
    {
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.Right));

        sut.GetHeldButtons()[(int)PicoButton.Right].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForUp_WhenUpArrowHeld()
    {
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.Up));

        sut.GetHeldButtons()[(int)PicoButton.Up].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForDown_WhenDownArrowHeld()
    {
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.Down));

        sut.GetHeldButtons()[(int)PicoButton.Down].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForPrimary_WhenZHeld()
    {
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.Z));

        sut.GetHeldButtons()[(int)PicoButton.Primary].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForSecondary_WhenXHeld()
    {
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.X));

        sut.GetHeldButtons()[(int)PicoButton.Secondary].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForPause_WhenEscapeHeld()
    {
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.Escape));

        sut.GetHeldButtons()[(int)PicoButton.Pause].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsFalse_ForUnboundKey()
    {
        // OemTilde is not bound to any PicoButton in default bindings
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.OemTilde));

        sut.GetHeldButtons().Should().AllBeEquivalentTo(false);
    }

    [Fact]
    public void GetHeldButtons_ReturnsFalse_ForOtherButtons_WhenOnlyLeftHeld()
    {
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.Left));
        var held = sut.GetHeldButtons();

        for (int i = 1; i < 7; i++)
            held[i].Should().BeFalse($"only Left (index 0) should be held, not index {i}");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetHeldButtons — SetBindings rebinding
    // --------------------------------------------------------------------------

    [Fact]
    public void GetHeldButtons_RespectsUpdatedBindings()
    {
        // Q is not in default bindings for Left
        var sut = CreateSut(getKeyboard: () => KeysDown(Keys.Q));
        sut.GetHeldButtons()[(int)PicoButton.Left].Should().BeFalse();

        var newBindings = new InputBindings(new Dictionary<PicoButton, IReadOnlyList<InputSource>>
        {
            [PicoButton.Left] = [new KeyboardSource(Keys.Q)],
        });
        sut.SetBindings(newBindings);

        sut.GetHeldButtons()[(int)PicoButton.Left].Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetHeldButtons — mouse
    // --------------------------------------------------------------------------

    [Fact]
    public void GetHeldButtons_ReturnsTrue_ForPrimary_WhenMouseLeftHeld()
    {
        var bindings = new InputBindings(new Dictionary<PicoButton, IReadOnlyList<InputSource>>
        {
            [PicoButton.Primary] = [new MouseSource(MouseButton.Left)],
        });
        var mouseState = new MouseState(0, 0, 0,
            leftButton:   ButtonState.Pressed,
            middleButton: ButtonState.Released,
            rightButton:  ButtonState.Released,
            xButton1:     ButtonState.Released,
            xButton2:     ButtonState.Released);

        var sut = CreateSut(bindings: bindings, getMouse: () => mouseState);

        sut.GetHeldButtons()[(int)PicoButton.Primary].Should().BeTrue();
    }

    [Fact]
    public void GetHeldButtons_ReturnsFalse_ForPrimary_WhenMouseLeftNotHeld()
    {
        var bindings = new InputBindings(new Dictionary<PicoButton, IReadOnlyList<InputSource>>
        {
            [PicoButton.Primary] = [new MouseSource(MouseButton.Left)],
        });

        var sut = CreateSut(bindings: bindings, getMouse: () => default);

        sut.GetHeldButtons()[(int)PicoButton.Primary].Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
}
