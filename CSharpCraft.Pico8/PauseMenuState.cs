namespace CSharpCraft.Pico8;

/// <summary>
/// Encapsulates pause menu state and behavior.
/// </summary>
public class PauseMenuState
{
    private bool _isPaused;
    private int _menuSelected;
    private readonly List<MenuItem> _mainMenuItems;
    private readonly List<MenuItem> _currentMenuItems;
    private readonly GameOrchestrator _orchestrator;

    public bool IsPaused => _isPaused;
    public int SelectedIndex => _menuSelected;
    public List<MenuItem> CurrentMenuItems => _currentMenuItems;

    public PauseMenuState(GameOrchestrator orchestrator)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _isPaused = false;
        _menuSelected = 0;
        _mainMenuItems = [];
        _currentMenuItems = [];
    }

    public void InitializeMenuStructure()
    {
        _mainMenuItems.Clear();
        _currentMenuItems.Clear();

        var menuBuilder = new PauseMenuBuilder(_orchestrator, _mainMenuItems, _currentMenuItems);
        menuBuilder.Build();

        _currentMenuItems.Clear();
        foreach (var item in _mainMenuItems)
        {
            _currentMenuItems.Add(item.Clone());
        }

        _menuSelected = 0;
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        _menuSelected = 0;
    }

    public void HandleMenuInput(bool upPressed, bool downPressed, bool selectPressed)
    {
        if (selectPressed)
        {
            _currentMenuItems[_menuSelected].Function();
        }

        if (upPressed) { _menuSelected -= 1; }
        if (downPressed) { _menuSelected += 1; }

        _menuSelected = Pico8MathUtils.Loop(_menuSelected, _currentMenuItems);
    }

    public void Reset()
    {
        _isPaused = false;
        _menuSelected = 0;
        _mainMenuItems.Clear();
        _currentMenuItems.Clear();
    }
}
