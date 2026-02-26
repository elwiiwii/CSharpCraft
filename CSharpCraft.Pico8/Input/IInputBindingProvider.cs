namespace CSharpCraft.Pico8.Input;

/// <summary>
/// Provides input key/button bindings for keyboard and controller.
/// Abstracts input configuration away from direct OptionsFile dependency.
/// </summary>
public interface IInputBindingProvider
{
    /// <summary>
    /// Keyboard binding for left direction
    /// </summary>
    InputBinding KeyboardLeft { get; }

    /// <summary>
    /// Keyboard binding for right direction
    /// </summary>
    InputBinding KeyboardRight { get; }

    /// <summary>
    /// Keyboard binding for up direction
    /// </summary>
    InputBinding KeyboardUp { get; }

    /// <summary>
    /// Keyboard binding for down direction
    /// </summary>
    InputBinding KeyboardDown { get; }

    /// <summary>
    /// Keyboard binding for use/action button
    /// </summary>
    InputBinding KeyboardUse { get; }

    /// <summary>
    /// Keyboard binding for menu button
    /// </summary>
    InputBinding KeyboardMenu { get; }

    /// <summary>
    /// Keyboard binding for pause button
    /// </summary>
    InputBinding KeyboardPause { get; }

    /// <summary>
    /// Controller binding for left direction
    /// </summary>
    InputBinding ControllerLeft { get; }

    /// <summary>
    /// Controller binding for right direction
    /// </summary>
    InputBinding ControllerRight { get; }

    /// <summary>
    /// Controller binding for up direction
    /// </summary>
    InputBinding ControllerUp { get; }

    /// <summary>
    /// Controller binding for down direction
    /// </summary>
    InputBinding ControllerDown { get; }

    /// <summary>
    /// Controller binding for use/action button
    /// </summary>
    InputBinding ControllerUse { get; }

    /// <summary>
    /// Controller binding for menu button
    /// </summary>
    InputBinding ControllerMenu { get; }

    /// <summary>
    /// Controller binding for pause button
    /// </summary>
    InputBinding ControllerPause { get; }
}

/// <summary>
/// Single input binding with two alternative key/button options.
/// Supports both first and second key for flexibility.
/// </summary>
public record InputBinding(string Bind1, string Bind2);
