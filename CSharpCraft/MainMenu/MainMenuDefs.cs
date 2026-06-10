namespace CSharpCraft.MainMenu;

internal static class MainMenuDefs
{
    internal static readonly MainMenuButtonDef[] All =
    [
        // ranked
        new(MonoSx: 0, MonoSy: 0, MonoSw: 19, MonoSh: 17,
            ColorSx: 19, ColorSy: 0, ColorSw: 19, ColorSh: 17,
            DestX: 39, DestY: 11, DestW: 19, DestH: 17,
            Label: "ranked", LabelX: 37, LabelY: 30, LabelCol: 9,
            IsEnabled: false),
        
        // casual
        new(MonoSx: 38, MonoSy: 0, MonoSw: 19, MonoSh: 17,
            ColorSx: 57, ColorSy: 0, ColorSw: 19, ColorSh: 17,
            DestX: 83, DestY: 11, DestW: 19, DestH: 17,
            Label: "casual", LabelX: 81, LabelY: 30, LabelCol: 9,
            IsEnabled: false),

        // events
        new(MonoSx: 0, MonoSy: 17, MonoSw: 15, MonoSh: 17,
            ColorSx: 15, ColorSy: 17, ColorSw: 15, ColorSh: 17,
            DestX: 19, DestY: 47, DestW: 15, DestH: 17,
            Label: "events", LabelX: 15, LabelY: 66, LabelCol: 9,
            IsEnabled: false),

        // temple
        new(MonoSx: 30, MonoSy: 17, MonoSw: 23, MonoSh: 17,
            ColorSx: 53, ColorSy: 17, ColorSw: 23, ColorSh: 17,
            DestX: 59, DestY: 47, DestW: 23, DestH: 17,
            Label: "temple", LabelX: 59, LabelY: 66, LabelCol: 9,
            IsEnabled: false),

        // speedrun
        new(MonoSx: 76, MonoSy: 0, MonoSw: 15, MonoSh: 17,
            ColorSx: 76, ColorSy: 17, ColorSw: 15, ColorSh: 17,
            DestX: 107, DestY: 47, DestW: 15, DestH: 17,
            Label: "speedrun", LabelX: 99, LabelY: 66, LabelCol: 9,
            IsEnabled: false),

        // custom
        new(MonoSx: 0, MonoSy: 34, MonoSw: 15, MonoSh: 17,
            ColorSx: 15, ColorSy: 34, ColorSw: 15, ColorSh: 17,
            DestX: 41, DestY: 83, DestW: 15, DestH: 17,
            Label: "custom", LabelX: 37, LabelY: 102, LabelCol: 9,
            IsEnabled: true),

        // practice
        new(MonoSx: 30, MonoSy: 34, MonoSw: 15, MonoSh: 17,
            ColorSx: 45, ColorSy: 34, ColorSw: 15, ColorSh: 17,
            DestX: 85, DestY: 83, DestW: 15, DestH: 17,
            Label: "practice", LabelX: 77, LabelY: 102, LabelCol: 9,
            IsEnabled: false),

        

        // profile
        new(MonoSx: 0, MonoSy: 51, MonoSw: 11, MonoSh: 14,
            ColorSx: 11, ColorSy: 51, ColorSw: 11, ColorSh: 14,
            DestX: 6, DestY: 114, DestW: 11, DestH: 14,
            IsEnabled: false),

        // statistics
        new(MonoSx: 22, MonoSy: 51, MonoSw: 14, MonoSh: 11,
            ColorSx: 36, ColorSy: 51, ColorSw: 14, ColorSh: 11,
            DestX: 23, DestY: 119, DestW: 14, DestH: 11,
            IsEnabled: false),

        // search
        new(MonoSx: 50, MonoSy: 51, MonoSw: 13, MonoSh: 13,
            ColorSx: 63, ColorSy: 51, ColorSw: 13, ColorSh: 13,
            DestX: 43, DestY: 121, DestW: 13, DestH: 13,
            IsEnabled: false),

        // library
        new(MonoSx: 0, MonoSy: 65, MonoSw: 17, MonoSh: 13,
            ColorSx: 17, ColorSy: 65, ColorSw: 17, ColorSh: 13,
            DestX: 62, DestY: 122, DestW: 17, DestH: 13,
            IsEnabled: false),

        // help
        new(MonoSx: 34, MonoSy: 65, MonoSw: 10, MonoSh: 13,
            ColorSx: 44, ColorSy: 65, ColorSw: 10, ColorSh: 13,
            DestX: 85, DestY: 121, DestW: 10, DestH: 13,
            IsEnabled: false),

        // credits
        new(MonoSx: 54, MonoSy: 65, MonoSw: 14, MonoSh: 11,
            ColorSx: 68, ColorSy: 65, ColorSw: 14, ColorSh: 11,
            DestX: 101, DestY: 119, DestW: 14, DestH: 11,
            IsEnabled: true),

        // settings
        new(MonoSx: 60, MonoSy: 34, MonoSw: 14, MonoSh: 14,
            ColorSx: 74, ColorSy: 34, ColorSw: 14, ColorSh: 14,
            DestX: 121, DestY: 114, DestW: 14, DestH: 14,
            IsEnabled: true),
    ];
}
