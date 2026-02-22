namespace CSharpCraft.Pico8;

/// <summary>
/// Encapsulates pause menu state and behavior.
/// Extracted to improve separation of concerns and testability.
/// Phase 7: Scene & Menu Management Extraction
/// </summary>
public class PauseMenuState
{
    private bool _isPaused;
    private int _menuSelected;
    private readonly List<MenuItem> _mainMenuItems;
    private readonly List<MenuItem> _currentMenuItems;
    private readonly Pico8Functions _pico8;

    /// <summary>
    /// Gets whether the game is currently paused.
    /// </summary>
    public bool IsPaused => _isPaused;

    /// <summary>
    /// Gets the currently selected menu item index.
    /// </summary>
    public int SelectedIndex => _menuSelected;

    /// <summary>
    /// Gets the current menu items being displayed.
    /// </summary>
    public List<MenuItem> CurrentMenuItems => _currentMenuItems;

    public PauseMenuState(Pico8Functions pico8)
    {
        _pico8 = pico8 ?? throw new ArgumentNullException(nameof(pico8));
        _isPaused = false;
        _menuSelected = 0;
        _mainMenuItems = [];
        _currentMenuItems = [];
    }

    /// <summary>
    /// Initializes menu structure from loaded scene.
    /// Called after LoadCart() to set up the pause menu.
    /// </summary>
    public void InitializeMenuStructure()
    {
        _mainMenuItems.Clear();
        _currentMenuItems.Clear();

        // Build pause menu structure using dedicated builder
        var menuBuilder = new PauseMenuBuilder(_pico8, _mainMenuItems, _currentMenuItems);
        menuBuilder.Build();

        // Reinitialize current menu to show main menu
        _currentMenuItems.Clear();
        foreach (var item in _mainMenuItems)
        {
            _currentMenuItems.Add(item.Clone());
        }

        _menuSelected = 0;
    }

    /// <summary>
    /// Toggles pause on/off and resets menu selection.
    /// </summary>
    public void TogglePause()
    {
        _isPaused = !_isPaused;
        _menuSelected = 0;
    }

    /// <summary>
    /// Processes menu input (up/down/select) and executes selected menu function.
    /// </summary>
    public void HandleMenuInput(bool upPressed, bool downPressed, bool selectPressed)
    {
        if (selectPressed)
        {
            _currentMenuItems[_menuSelected].Function();
        }

        if (upPressed) { _menuSelected -= 1; }
        if (downPressed) { _menuSelected += 1; }

        // Wrap selection with loop
        _menuSelected = Pico8MathUtils.Loop(_menuSelected, _currentMenuItems);
    }

    /// <summary>
    /// Resets pause state when loading a new scene.
    /// </summary>
    public void Reset()
    {
        _isPaused = false;
        _menuSelected = 0;
        _mainMenuItems.Clear();
        _currentMenuItems.Clear();
    }
}
