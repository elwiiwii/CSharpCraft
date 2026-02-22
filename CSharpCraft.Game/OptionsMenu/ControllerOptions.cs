using CSharpCraft.Pico8;
using static CSharpCraft.Pico8.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu;

public class ControllerOptions(int startIndex = -1) : IScene, IDisposable
{

    public string SceneName { get => "options"; }
    public double Fps { get => 60.0; }
    public (int w, int h) Resolution { get => (128, 128); }

    private (int hor, int ver) menuSelected;
    private int menuW;
    private int menuH;
    private bool waitingForInput;
    private bool lockout;

    private record BindingDescriptor(
        string Name,
        Func<OptionsFile, InputBinding> Get,
        Action<OptionsFile, InputBinding> Set);

    private static readonly List<BindingDescriptor> Bindings =
    [
        new("left",  f => f.Con_Left,  (f, v) => f.Con_Left = v),
        new("right", f => f.Con_Right, (f, v) => f.Con_Right = v),
        new("up",    f => f.Con_Up,    (f, v) => f.Con_Up = v),
        new("down",  f => f.Con_Down,  (f, v) => f.Con_Down = v),
        new("use",   f => f.Con_Use,   (f, v) => f.Con_Use = v),
        new("menu",  f => f.Con_Menu,  (f, v) => f.Con_Menu = v),
        new("pause", f => f.Con_Pause, (f, v) => f.Con_Pause = v),
    ];

    public void Init()
    {

        menuSelected = (0, startIndex);
        menuW = 2;
        menuH = Bindings.Count;
        waitingForInput = false;
        lockout = true;
    }

    public void Update()
    {
        if (menuSelected.ver == -1)
        {
            if (Btnp(0)) { ScheduleScene(() => new KeyboardOptions()); return; }
            if (Btnp(2)) { ScheduleScene(() => new ControlsOptions()); return; }
            if (Btnp(3)) { menuSelected.ver += 1; }
            return;
        }

        if (Btnp(5)) { waitingForInput = true; }

        if (waitingForInput)
        {
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            List<Buttons> buttons = new();

            foreach (Buttons button in ButtonsToString.buttonsToString.Keys)
            {
                if (state.IsButtonDown(button))
                {
                    buttons.Add(button);
                }
            }
            
            if (buttons.Count == 0) { lockout = false; }

            if (!lockout && buttons.Count == 1)
            {
                var optionsFile = OptionsFile.Current;
                var desc = Bindings[menuSelected.ver];
                var binding = desc.Get(optionsFile);

                string newButton = ButtonsToString.buttonsToString[buttons[0]];

                InputBinding newBinding = menuSelected.hor == 0
                    ? new InputBinding(newButton, binding.Bind2)
                    : new InputBinding(binding.Bind1, newButton);

                desc.Set(optionsFile, newBinding);
                OptionsFile.JsonWrite(optionsFile);

                waitingForInput = false;
                lockout = true;
            }
            return;
        }

        if (Btnp(0)) { menuSelected.hor -= 1; }
        if (Btnp(1)) { menuSelected.hor += 1; }
        if (Btnp(2)) { menuSelected.ver -= 1; }
        if (Btnp(3)) { menuSelected.ver += 1; }

        menuSelected.hor = GeneralFunctions.Loop(menuSelected.hor, menuW);
        menuSelected.ver = menuSelected.ver > - 1 ? GeneralFunctions.Loop(menuSelected.ver, menuH) : - 1;
    }

    public void Draw()
    {
        Cls();

        GameRendering.Current.Draw("OptionsBackground4", new Vector2(0, 0), Color.White, CellWidth, CellHeight);

        if (waitingForInput)
        {
            GameRendering.Current.Draw("WaitingForInput", new Vector2(20 * CellWidth, 51 * CellHeight), Color.White, CellWidth, CellHeight);
        }
        else
        {
            GameRendering.Current.Draw("KeybindsMenu", new Vector2(8 * CellWidth, 46 * CellHeight), Color.White, CellWidth, CellHeight);

            if (menuSelected.ver >= 0)
            {
                Vector2 position5 = new((46 + 36 * menuSelected.hor) * CellWidth, (menuSelected.ver * 6 + 55) * CellHeight);
                GameRendering.Current.Draw("Arrow", position5, Colors[6], CellWidth, CellHeight, flipX: true);
            }
            else if (menuSelected.ver == -1)
            {
                Rectfill(71, 32, 113, 38, 13);
            }

            Vector2 position3 = new(70 * CellWidth, 31 * CellHeight);
            Vector2 position4 = new(92 * CellWidth, 31 * CellHeight);
            GameRendering.Current.Draw("SelectorHalf", position3, Colors[7], CellWidth, CellHeight);
            GameRendering.Current.Draw("SelectorHalf", position4, Colors[7], CellWidth, CellHeight, flipX: true);

            Print("keyboard", 19, 33, 7);
            Print("controller", 19 + 54, 33, 7);

            var optionsFile = OptionsFile.Current;
            int j = 0;
            foreach (var desc in Bindings)
            {
                Print(desc.Name, 8, 55 + j, 7);
                var val = desc.Get(optionsFile);
                Print(ButtonNames.buttonNames[val.Bind1], 51, 55 + j, 6);
                Print(ButtonNames.buttonNames[val.Bind2], 87, 55 + j, 6);
                j += 6;
            }
        }
    }
    public string SpriteImage => "";
    public string SpriteData => @"";
    public string FlagData => @"";
    public (int x, int y) MapDimensions => (0, 0);
    public string MapData => @"";
    public Dictionary<string, List<SongInst>> Music => new();
    public Dictionary<string, Dictionary<int, string>> Sfx => new();
    public void Dispose()
    {

    }

}
