using static CSharpCraft.Pico8.Pico8;

namespace CSharpCraft.Pico8;

/// <summary>
/// Builds the pause menu structure and handles menu item callbacks.
/// Uses GameOrchestrator for game state and static Pico8 API for input.
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

    private static void AddMenuItem(int pos, Func<string> getName, Action function, List<MenuItem> list)
    {
        while (list.Count <= pos)
            list.Add(new MenuItem(() => "", () => { }));
        list[pos] = new MenuItem(getName, function);
    }

    private void BuildContinueMenuItem()
    {
        void ContinueAction()
        {
            if (Btnp(4) || Btnp(5))
            {
                // Unpause will be handled by pause menu toggle
            }
        }

        AddMenuItem(0, () => "continue", ContinueAction, _mainMenuItems);
    }

    private void BuildOptionsMenuItem()
    {
        void OptionsAction()
        {
            if (Btnp(4) || Btnp(5))
            {
                ShowOptionsSubmenu();
            }
        }

        AddMenuItem(1, () => "options", OptionsAction, _mainMenuItems);
    }

    private void BuildResetMenuItem()
    {
        void ResetAction()
        {
            if (Btnp(4) || Btnp(5))
            {
                _context.ReloadCart();
            }
        }

        AddMenuItem(2, () => "reset cart", ResetAction, _mainMenuItems);
    }

    private void BuildExitMenuItem()
    {
        void ExitAction()
        {
            if (Btnp(4) || Btnp(5))
            {
                var scenes = _context.Scenes;
                if (scenes.Count > 0 && scenes[0] is IScene titleScreen)
                {
                    _context.LoadCart(titleScreen);
                }
            }
        }

        AddMenuItem(3, () => "exit", ExitAction, _mainMenuItems);
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
        void SoundAction()
        {
            if (Btnp(0) || Btnp(1) || Btnp(4) || Btnp(5))
            {
                _context.Settings.SoundEnabled = !_context.Settings.SoundEnabled;
                if (!_context.Settings.SoundEnabled)
                {
                    Mute();
                }
            }
        }

        AddMenuItem(0, () => $"sound:{(_context.Settings.SoundEnabled ? "on" : "off")}", SoundAction, _curMenuItems);
    }

    private void BuildMusicVolumeMenuItem()
    {
        void MusicVolAction()
        {
            if (Btnp(0))
                _context.Settings.MusicVolume = Math.Max(_context.Settings.MusicVolume - 10, 0);
            if (Btnp(1))
                _context.Settings.MusicVolume = Math.Min(_context.Settings.MusicVolume + 10, 100);
        }

        AddMenuItem(1, () => $"music vol:{_context.Settings.MusicVolume}%", MusicVolAction, _curMenuItems);
    }

    private void BuildSfxVolumeMenuItem()
    {
        void SfxVolAction()
        {
            if (Btnp(0))
                _context.Settings.SfxVolume = Math.Max(_context.Settings.SfxVolume - 10, 0);
            if (Btnp(1))
                _context.Settings.SfxVolume = Math.Min(_context.Settings.SfxVolume + 10, 100);
        }

        AddMenuItem(2, () => $"sfx vol:{_context.Settings.SfxVolume}%", SfxVolAction, _curMenuItems);
    }

    private void BuildFullscreenToggleMenuItem()
    {
        void FullscreenAction()
        {
            if (Btnp(4) || Btnp(5))
            {
                _context.ToggleFullscreen();
            }
        }

        AddMenuItem(3, () => $"fullscreen:{(_context.Settings.IsFullscreen ? "on" : "off")}", FullscreenAction, _curMenuItems);
    }

    private void BuildBackMenuItem(List<MenuItem> previousMenu)
    {
        void BackAction()
        {
            if (Btnp(4) || Btnp(5))
            {
                _curMenuItems.Clear();
                foreach (var item in previousMenu)
                {
                    _curMenuItems.Add(item.Clone());
                }
            }
        }

        AddMenuItem(4, () => "back", BackAction, _curMenuItems);
    }

    private void BuildSfxPackMenuItem()
    {
        if ((_context.TrackManager?.SfxCount ?? 0) <= 1)
            return;

        void SfxPackAction()
        {
            if (Btnp(0))
                _context.TrackManager?.DecrementSfxPack();
            if (Btnp(1))
                _context.TrackManager?.IncrementSfxPack();
        }

        AddMenuItem(5, () => $"sfx:{_context.TrackManager?.GetCurrentSfxPackName() ?? "sfx"}", SfxPackAction, _curMenuItems);
    }

    private void BuildSoundtrackMenuItem()
    {
        if ((_context.TrackManager?.MusicCount ?? 0) <= 1)
            return;

        void SoundtrackAction()
        {
            if (Btnp(0))
                _context.TrackManager?.DecrementSoundtrack();
            if (Btnp(1))
                _context.TrackManager?.IncrementSoundtrack();

            if (Btnp(0) || Btnp(1))
            {
                _context.SoundDispose();
                if (_context.LastMusicCall is not null)
                {
                    Music((int)_context.LastMusicCall);
                }
            }
        }

        AddMenuItem(6, () => $"music:{_context.TrackManager?.GetCurrentSoundtrackName() ?? "music"}", SoundtrackAction, _curMenuItems);
    }
}
