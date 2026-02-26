namespace CSharpCraft.Pico8.Menu;

/// <summary>
/// A menu item with a display name and callback action.
/// Used by the pause menu system.
/// Callbacks receive MenuInput so they don't need to query input state globally.
/// </summary>
public class MenuItem
{
    public Func<string> GetName { get; }
    public Action<MenuInput> Function { get; }
    public MenuItem Clone() => new(this.GetName, this.Function);

    public MenuItem(Func<string> getName, Action<MenuInput> function)
    {
        GetName = getName;
        Function = function;
    }
}
