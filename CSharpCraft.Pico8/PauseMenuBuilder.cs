namespace CSharpCraft.Pico8;

/// <summary>
/// Builds the pause menu structure and handles menu item callbacks.
/// Extracted from LoadCart to reduce nesting and improve maintainability.
/// </summary>
public class PauseMenuBuilder
{
    private readonly Pico8Functions _pico8;
    private readonly List<MenuItem> _mainMenuItems;
    private readonly List<MenuItem> _curMenuItems;

    public PauseMenuBuilder(Pico8Functions pico8, List<MenuItem> mainItems, List<MenuItem> curItems)
    {
        _pico8 = pico8;
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

    private void BuildContinueMenuItem()
    {
        void ContinueAction()
        {
            if (_pico8.Btnp(4) || _pico8.Btnp(5))
            {
                // Unpause will be handled by pause menu
            }
        }

        _pico8.Menuitem(0, () => "continue", ContinueAction, _mainMenuItems);
    }

    private void BuildOptionsMenuItem()
    {
        void OptionsAction()
        {
            if (_pico8.Btnp(4) || _pico8.Btnp(5))
            {
                ShowOptionsSubmenu();
            }
        }

        _pico8.Menuitem(1, () => "options", OptionsAction, _mainMenuItems);
    }

    private void BuildResetMenuItem()
    {
        void ResetAction()
        {
            if (_pico8.Btnp(4) || _pico8.Btnp(5))
            {
                _pico8.ReloadCart();
            }
        }

        _pico8.Menuitem(2, () => "reset cart", ResetAction, _mainMenuItems);
    }

    private void BuildExitMenuItem()
    {
        void ExitAction()
        {
            if (_pico8.Btnp(4) || _pico8.Btnp(5))
            {
                if (_pico8.TitleSceneInstance is IScene titleScreen)
                {
                    _pico8.LoadCart(titleScreen);
                }
            }
        }

        _pico8.Menuitem(3, () => "exit", ExitAction, _mainMenuItems);
    }

    private void ShowOptionsSubmenu()
    {
        // Save current menu to return to it
        var previousMenu = new List<MenuItem>(_mainMenuItems);

        _mainMenuItems.Clear();
        foreach (var item in _curMenuItems)
        {
            _mainMenuItems.Add(item.Clone());
        }

        _curMenuItems.Clear();

        // Options submenu items
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
            if (_pico8.Btnp(0) || _pico8.Btnp(1) || _pico8.Btnp(4) || _pico8.Btnp(5))
            {
                _pico8.Settings.SoundEnabled = !_pico8.Settings.SoundEnabled;
                if (!_pico8.Settings.SoundEnabled)
                {
                    _pico8.Mute();
                }
            }
        }

        _pico8.Menuitem(0, () => $"sound:{(_pico8.Settings.SoundEnabled ? "on" : "off")}", SoundAction, _curMenuItems);
    }

    private void BuildMusicVolumeMenuItem()
    {
        void MusicVolAction()
        {
            if (_pico8.Btnp(0))
            {
                _pico8.Settings.MusicVolume = Math.Max(_pico8.Settings.MusicVolume - 10, 0);
            }
            if (_pico8.Btnp(1))
            {
                _pico8.Settings.MusicVolume = Math.Min(_pico8.Settings.MusicVolume + 10, 100);
            }
        }

        _pico8.Menuitem(1, () => $"music vol:{_pico8.Settings.MusicVolume}%", MusicVolAction, _curMenuItems);
    }

    private void BuildSfxVolumeMenuItem()
    {
        void SfxVolAction()
        {
            if (_pico8.Btnp(0))
            {
                _pico8.Settings.SfxVolume = Math.Max(_pico8.Settings.SfxVolume - 10, 0);
            }
            if (_pico8.Btnp(1))
            {
                _pico8.Settings.SfxVolume = Math.Min(_pico8.Settings.SfxVolume + 10, 100);
            }
        }

        _pico8.Menuitem(2, () => $"sfx vol:{_pico8.Settings.SfxVolume}%", SfxVolAction, _curMenuItems);
    }

    private void BuildFullscreenToggleMenuItem()
    {
        void FullscreenAction()
        {
            if (_pico8.Btnp(4) || _pico8.Btnp(5))
            {
                _pico8.Settings.IsFullscreen = !_pico8.Settings.IsFullscreen;
                _pico8.Graphics.IsFullScreen = _pico8.Settings.IsFullscreen;
                _pico8.Graphics.PreferredBackBufferWidth = _pico8.Settings.WindowWidth / 128 * _pico8.Resolution.w;
                _pico8.Graphics.PreferredBackBufferHeight = _pico8.Settings.WindowHeight / 128 * _pico8.Resolution.h;
                _pico8.Graphics.ApplyChanges();
                _pico8.UpdateViewport();
            }
        }

        _pico8.Menuitem(3, () => $"fullscreen:{(_pico8.Settings.IsFullscreen ? "on" : "off")}", FullscreenAction, _curMenuItems);
    }

    private void BuildBackMenuItem(List<MenuItem> previousMenu)
    {
        void BackAction()
        {
            if (_pico8.Btnp(4) || _pico8.Btnp(5))
            {
                _curMenuItems.Clear();
                foreach (var item in previousMenu)
                {
                    _curMenuItems.Add(item.Clone());
                }
            }
        }

        _pico8.Menuitem(4, () => "back", BackAction, _curMenuItems);
    }

    private void BuildSfxPackMenuItem()
    {
        // Only show if multiple sfx packs available
        if (_pico8.SfxCount <= 1)
            return;

        void SfxPackAction()
        {
            if (_pico8.Btnp(0))
            {
                _pico8.DecrementSfxPack();
            }
            if (_pico8.Btnp(1))
            {
                _pico8.IncrementSfxPack();
            }
        }

        _pico8.Menuitem(5, () => $"sfx:{_pico8.GetCurrentSfxPackName()}", SfxPackAction, _curMenuItems);
    }

    private void BuildSoundtrackMenuItem()
    {
        // Only show if multiple soundtracks available
        if (_pico8.MusicCount <= 1)
            return;

        void SoundtrackAction()
        {
            if (_pico8.Btnp(0))
            {
                _pico8.DecrementSoundtrack();
            }
            if (_pico8.Btnp(1))
            {
                _pico8.IncrementSoundtrack();
            }

            if (_pico8.Btnp(0) || _pico8.Btnp(1))
            {
                _pico8.SoundDispose();
                if (_pico8.LastMusicCall is not null)
                {
                    _pico8.Music((int)_pico8.LastMusicCall);
                }
            }
        }

        _pico8.Menuitem(6, () => $"music:{_pico8.GetCurrentSoundtrackName()}", SoundtrackAction, _curMenuItems);
    }
}
