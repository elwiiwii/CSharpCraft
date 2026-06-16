using CSharpCraft.MenuShared;
using CSharpCraft.PcraftScenes;

namespace CSharpCraft.CreditsMenu;

internal static class CreditsMenuDefs
{
    internal static readonly MenuButtonDef[] All =
    [
        // return to main menu
        new(MonoSx: 54, MonoSy: 65, MonoSw: 14, MonoSh: 11,
            ColorSx: 68, ColorSy: 65, ColorSw: 14, ColorSh: 11,
            DestX: 63, DestY: 12, DestW: 14, DestH: 11,
            IsEnabled: true,
            Action: () => Pico8.ScheduleScene(() => new MainScene())),
    ];
}
