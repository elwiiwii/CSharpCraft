namespace CSharpCraft.Pico8.Menu;

/// <summary>
/// Builds the pause menu structure and handles menu item callbacks.
/// All callbacks receive MenuInput — no static Pico8 dependency.
/// </summary>
public class PauseMenuBuilder
{
    private readonly IPauseMenuContext _context;
    private readonly List<MenuItem> _mainMenuItems;
    private readonly List<MenuItem> _curMenuItems;

    public PauseMenuBuilder(IPauseMenuContext context, List<MenuItem> mainItems, List<MenuItem> curItems)
    {
        _context = context;
        _mainMenuItems = mainItems;
        _curMenuItems = curItems;
    }

    /// <summary>
    /// Build the complete pause menu structure.
    /// </summary>
    public void Build()
    {
        // Main menu
        BuildContinueMenuItem();
        BuildOptionsMenuItem();
        BuildResetMenuItem();
        BuildExitMenuItem();

        // Initialize current menu to show main menu
        _curMenuItems.Clear();
        foreach (var item in _mainMenuItems)
        {
            _curMenuItems.Add(item.Clone());
        }
    }

    private static void AddMenuItem(int pos, Func<string> getName, Action<MenuInput> function, List<MenuItem> list)
    {
        while (list.Count <= pos)
            list.Add(new MenuItem(() => "", _ => { }));
        list[pos] = new MenuItem(getName, function);
    }

    private void BuildContinueMenuItem()
    {
        AddMenuItem(0, () => "continue", input =>
        {
            if (input.ActionA || input.ActionB)
            {
                // Unpause will be handled by pause menu toggle
            }
        }, _mainMenuItems);
    }

    private void BuildOptionsMenuItem()
    {
        AddMenuItem(1, () => "options", input =>
        {
            if (input.ActionA || input.ActionB)
            {
                ShowOptionsSubmenu();
            }
        }, _mainMenuItems);
    }

    private void BuildResetMenuItem()
    {
        AddMenuItem(2, () => "reset cart", input =>
        {
            if (input.ActionA || input.ActionB)
            {
                _context.ReloadCart();
            }
        }, _mainMenuItems);
    }

    private void BuildExitMenuItem()
    {
        AddMenuItem(3, () => "exit", input =>
        {
            if (input.ActionA || input.ActionB)
            {
                _context.QuitToTitle();
            }
        }, _mainMenuItems);
    }

    private void ShowOptionsSubmenu()
    {
        var previousMenu = new List<MenuItem>(_mainMenuItems);

        _mainMenuItems.Clear();
        foreach (var item in _curMenuItems)
        {
            _mainMenuItems.Add(item.Clone());
        }

        _curMenuItems.Clear();

        BuildSoundToggleMenuItem();
        BuildMusicVolumeMenuItem();
        BuildSfxVolumeMenuItem();
        BuildFullscreenToggleMenuItem();
        BuildBackMenuItem(previousMenu);
        BuildSfxPackMenuItem();
        BuildSoundtrackMenuItem();
    }

    private void BuildSoundToggleMenuItem()
    {
        AddMenuItem(0, () => $"sound:{(_context.AudioSettings.SoundEnabled ? "on" : "off")}", input =>
        {
            if (input.Left || input.Right || input.ActionA || input.ActionB)
            {
                _context.ToggleSound();
            }
        }, _curMenuItems);
    }

    private void BuildMusicVolumeMenuItem()
    {
        AddMenuItem(1, () => $"music vol:{_context.AudioSettings.MusicVolume}%", input =>
        {
            if (input.Left)
                _context.AudioSettings.MusicVolume = Math.Max(_context.AudioSettings.MusicVolume - 10, 0);
            if (input.Right)
                _context.AudioSettings.MusicVolume = Math.Min(_context.AudioSettings.MusicVolume + 10, 100);
        }, _curMenuItems);
    }

    private void BuildSfxVolumeMenuItem()
    {
        AddMenuItem(2, () => $"sfx vol:{_context.AudioSettings.SfxVolume}%", input =>
        {
            if (input.Left)
                _context.AudioSettings.SfxVolume = Math.Max(_context.AudioSettings.SfxVolume - 10, 0);
            if (input.Right)
                _context.AudioSettings.SfxVolume = Math.Min(_context.AudioSettings.SfxVolume + 10, 100);
        }, _curMenuItems);
    }

    private void BuildFullscreenToggleMenuItem()
    {
        AddMenuItem(3, () => $"fullscreen:{(_context.DisplaySettings.IsFullscreen ? "on" : "off")}", input =>
        {
            if (input.ActionA || input.ActionB)
            {
                _context.ToggleFullscreen();
            }
        }, _curMenuItems);
    }

    private void BuildBackMenuItem(List<MenuItem> previousMenu)
    {
        AddMenuItem(4, () => "back", input =>
        {
            if (input.ActionA || input.ActionB)
            {
                _curMenuItems.Clear();
                foreach (var item in previousMenu)
                {
                    _curMenuItems.Add(item.Clone());
                }
            }
        }, _curMenuItems);
    }

    private void BuildSfxPackMenuItem()
    {
        if ((_context.TrackManager?.SfxCount ?? 0) <= 1)
            return;

        AddMenuItem(5, () => $"sfx:{_context.TrackManager?.GetCurrentSfxPackName() ?? "sfx"}", input =>
        {
            if (input.Left)
                _context.TrackManager?.DecrementSfxPack();
            if (input.Right)
                _context.TrackManager?.IncrementSfxPack();
        }, _curMenuItems);
    }

    private void BuildSoundtrackMenuItem()
    {
        if ((_context.TrackManager?.MusicCount ?? 0) <= 1)
            return;

        AddMenuItem(6, () => $"music:{_context.TrackManager?.GetCurrentSoundtrackName() ?? "music"}", input =>
        {
            if (input.Left)
                _context.TrackManager?.DecrementSoundtrack();
            if (input.Right)
                _context.TrackManager?.IncrementSoundtrack();

            if (input.Left || input.Right)
            {
                _context.SoundDispose();
                if (_context.LastMusicCall is not null)
                {
                    _context.PlayMusic((int)_context.LastMusicCall);
                }
            }
        }, _curMenuItems);
    }
}
