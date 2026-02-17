namespace CSharpCraft.Pico8;

/// <summary>
/// Default implementation of input bindings for Pico8-style game.
/// Provides keyboard and controller input configuration.
/// </summary>
public class InputBindings : IInputBindingProvider
{
    public InputBinding KeyboardLeft { get; set; } = new("Left", "NumPad4");
    public InputBinding KeyboardRight { get; set; } = new("Right", "NumPad6");
    public InputBinding KeyboardUp { get; set; } = new("Up", "NumPad8");
    public InputBinding KeyboardDown { get; set; } = new("Down", "NumPad5");
    public InputBinding KeyboardUse { get; set; } = new("X", "V");
    public InputBinding KeyboardMenu { get; set; } = new("Z", "C");
    public InputBinding KeyboardPause { get; set; } = new("Escape", "Enter");

    public InputBinding ControllerLeft { get; set; } = new("DPadLeft", "LeftThumbstickLeft");
    public InputBinding ControllerRight { get; set; } = new("DPadRight", "LeftThumbstickRight");
    public InputBinding ControllerUp { get; set; } = new("DPadUp", "LeftThumbstickUp");
    public InputBinding ControllerDown { get; set; } = new("DPadDown", "LeftThumbstickDown");
    public InputBinding ControllerUse { get; set; } = new("A", "LeftShoulder");
    public InputBinding ControllerMenu { get; set; } = new("B", "RightShoulder");
    public InputBinding ControllerPause { get; set; } = new("Start", "Back");
}
