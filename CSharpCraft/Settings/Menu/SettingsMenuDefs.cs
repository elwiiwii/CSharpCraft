using CSharpCraft.MenuShared;
using CSharpCraft.PcraftScenes;

namespace CSharpCraft.Settings.Menu;

internal static class SettingsMenuDefs
{
    internal static readonly MenuButtonDef[] All =
    [
        // return to main menu
        new(MonoSx: 60, MonoSy: 34, MonoSw: 14, MonoSh: 14,
            ColorSx: 74, ColorSy: 34, ColorSw: 14, ColorSh: 14,
            DestX: 63, DestY: 12, DestW: 14, DestH: 14,
            IsEnabled: true,
            Action: () => Pico8.ScheduleScene(() => new MainScene())),
    ];
}
