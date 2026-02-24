namespace CSharpCraft.Pico8;

/// <summary>
/// Default implementation of IInputBindingProvider.
/// Returns empty bindings for all inputs — safe for test usage where
/// input bindings are not exercised.
/// </summary>
public sealed class DefaultInputBindings : IInputBindingProvider
{
    /// <summary>
    /// Singleton instance — stateless, can be safely shared.
    /// </summary>
    public static readonly DefaultInputBindings Instance = new();

    private static readonly InputBinding Empty = new("", "");

    public InputBinding KeyboardLeft => Empty;
    public InputBinding KeyboardRight => Empty;
    public InputBinding KeyboardUp => Empty;
    public InputBinding KeyboardDown => Empty;
    public InputBinding KeyboardUse => Empty;
    public InputBinding KeyboardMenu => Empty;
    public InputBinding KeyboardPause => Empty;
    public InputBinding ControllerLeft => Empty;
    public InputBinding ControllerRight => Empty;
    public InputBinding ControllerUp => Empty;
    public InputBinding ControllerDown => Empty;
    public InputBinding ControllerUse => Empty;
    public InputBinding ControllerMenu => Empty;
    public InputBinding ControllerPause => Empty;
}
