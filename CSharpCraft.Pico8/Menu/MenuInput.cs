namespace CSharpCraft.Pico8.Menu;

/// <summary>
/// Button state passed to menu item callbacks.
/// Replaces static Btnp() calls, breaking the circular PauseMenuBuilder → Pico8 dependency.
/// </summary>
public record MenuInput(bool Left, bool Right, bool ActionA, bool ActionB);
