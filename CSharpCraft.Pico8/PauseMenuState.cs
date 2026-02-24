namespace CSharpCraft.Pico8;

/// <summary>
/// Encapsulates pause menu state and behavior.
/// </summary>
public class PauseMenuState(IPauseMenuContext context)
{
    private bool _isPaused = false;
    private int _menuSelected = 0;
    private readonly List<MenuItem> _mainMenuItems = [];
    private readonly List<MenuItem> _currentMenuItems = [];
    private readonly IPauseMenuContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public bool IsPaused => _isPaused;
    public int SelectedIndex => _menuSelected;
    public List<MenuItem> CurrentMenuItems => _currentMenuItems;

    public void InitializeMenuStructure()
    {
        _mainMenuItems.Clear();
        _currentMenuItems.Clear();

        var menuBuilder = new PauseMenuBuilder(_context, _mainMenuItems, _currentMenuItems);
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

    public void HandleMenuInput(bool upPressed, bool downPressed, bool selectPressed,
        bool leftPressed = false, bool rightPressed = false,
        bool actionAPressed = false, bool actionBPressed = false)
    {
        if (selectPressed)
        {
            var input = new MenuInput(leftPressed, rightPressed, actionAPressed, actionBPressed);
            _currentMenuItems[_menuSelected].Function(input);
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
