using System;
using System.Collections.Generic;

namespace CSharpCraft.Pico8.Services;

/// <summary>
/// Service responsible for UI menu management and rendering
/// Extracted from Pico8Functions to enable service-based architecture
/// Phase 3: Service Extraction - Part 2
/// </summary>
public class MenuService
{
    private List<MenuItem> _mainMenuItems = [];
    private List<MenuItem> _currentMenuItems = [];
    private int _menuSelected = 0;

    public List<MenuItem> MainMenuItems
    {
        get => _mainMenuItems;
        set => _mainMenuItems = value;
    }

    public List<MenuItem> CurrentMenuItems
    {
        get => _currentMenuItems;
        set => _currentMenuItems = value;
    }

    public int MenuSelected
    {
        get => _menuSelected;
        set => _menuSelected = value;
    }

    /// <summary>
    /// Add a menu item (Pico-8: Menuitem)
    /// https://pico-8.fandom.com/wiki/Menuitem
    /// </summary>
    public void Menuitem(int pos, Func<string> getName, Action function, List<MenuItem>? list = null)
    {
        list ??= _currentMenuItems;
        list.Insert(pos, new MenuItem(getName, function));
    }

    /// <summary>
    /// Move selection to next menu item
    /// </summary>
    public void SelectNext(List<MenuItem> items)
    {
        if (items.Count > 0)
        {
            _menuSelected = (_menuSelected + 1) % items.Count;
        }
    }

    /// <summary>
    /// Move selection to previous menu item
    /// </summary>
    public void SelectPrev(List<MenuItem> items)
    {
        if (items.Count > 0)
        {
            _menuSelected = (_menuSelected - 1 + items.Count) % items.Count;
        }
    }

    /// <summary>
    /// Execute the currently selected menu item
    /// </summary>
    public void ExecuteSelected(List<MenuItem> items)
    {
        if (items.Count > 0 && _menuSelected >= 0 && _menuSelected < items.Count)
        {
            items[_menuSelected].Function();
        }
    }

    /// <summary>
    /// Clear all menu items
    /// </summary>
    public void ClearMenu(List<MenuItem>? list = null)
    {
        list ??= _currentMenuItems;
        list.Clear();
    }

    /// <summary>
    /// Reset menu selection to first item
    /// </summary>
    public void ResetSelection()
    {
        _menuSelected = 0;
    }
}
