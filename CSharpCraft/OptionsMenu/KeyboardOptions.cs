using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu;

public class KeyboardOptions(int startIndex = -1) : IScene, IDisposable
{

    public string SceneName { get => "options"; }
    public double Fps { get => 60.0; }
    private Pico8Functions p8;

    private (int hor, int ver) menuSelected;
    private int menuW;
    private int menuH;
    private bool waitingForInput;
    private bool lockout;

    public void Init(Pico8Functions pico8)
    {
        p8 = pico8;

        menuSelected = (0, startIndex);
        menuW = 2;
        menuH = 7;
        waitingForInput = false;
        lockout = true;
    }

    public void Update()
    {
        if (menuSelected.ver == -1)
        {
            if (p8.Btnp(1)) { p8.LoadCart(new ControllerOptions()); return; }
            if (p8.Btnp(2)) { p8.LoadCart(new ControlsOptions()); return; }
            if (p8.Btnp(3)) { menuSelected.ver += 1; }
            return;
        }

        if (p8.Btnp(5)) { waitingForInput = true; }

        if (waitingForInput)
        {
            Keys[] keys = Keyboard.GetState().GetPressedKeys();
            MouseState mouseState = Mouse.GetState();
            List<string> pressedButtons = [];
            if (mouseState.LeftButton == ButtonState.Pressed) { pressedButtons.Add("LeftButton"); }
            if (mouseState.MiddleButton == ButtonState.Pressed) { pressedButtons.Add("MiddleButton"); }
            if (mouseState.RightButton == ButtonState.Pressed) { pressedButtons.Add("RightButton"); }
            if (mouseState.XButton1 == ButtonState.Pressed) { pressedButtons.Add("XButton1"); }
            if (mouseState.XButton2 == ButtonState.Pressed) { pressedButtons.Add("XButton2"); }

            if (keys.Length + pressedButtons.Count == 0) { lockout = false; }

            if (!lockout && keys.Length + pressedButtons.Count == 1)
            {
                if (pressedButtons.Count == 1 || (keys.Length == 1 && !(keys[0] == Keys.Delete)))
                {
                    PropertyInfo[] properties = typeof(OptionsFile).GetProperties();
                    PropertyInfo currentProperty = properties[menuSelected.ver];
                    PropertyInfo? propertyName = typeof(OptionsFile).GetProperty(currentProperty.Name);
                    Binding binding = (Binding)propertyName.GetValue(p8.OptionsFile);
                    if (menuSelected.hor == 0 && propertyName is not null)
                    {
                        Binding newBinding;
                        if (keys.Length == 1)
                        {
                            newBinding = new Binding(KeysToString.keysToString[keys[0]], binding.Bind2);
                        }
                        else
                        {
                            newBinding = new Binding(pressedButtons[0], binding.Bind2);
                        }
                        propertyName.SetValue(p8.OptionsFile, newBinding);
                        OptionsFile.JsonWrite(p8.OptionsFile);
                    }
                    else if (propertyName is not null)
                    {
                        Binding newBinding;
                        if (keys.Length == 1)
                        {
                            newBinding = new Binding(binding.Bind1, KeysToString.keysToString[keys[0]]);
                        }
                        else
                        {
                            newBinding = new Binding(binding.Bind1, pressedButtons[0]);
                        }
                        propertyName.SetValue(p8.OptionsFile, newBinding);
                        OptionsFile.JsonWrite(p8.OptionsFile);
                    }
                }
                waitingForInput = false;
                lockout = true;
            }
            return;
        }

        if (p8.Btnp(0)) { menuSelected.hor -= 1; }
        if (p8.Btnp(1)) { menuSelected.hor += 1; }
        if (p8.Btnp(2)) { menuSelected.ver -= 1; }
        if (p8.Btnp(3)) { menuSelected.ver += 1; }

        menuSelected.hor = GeneralFunctions.Loop(menuSelected.hor, menuW);
        menuSelected.ver = menuSelected.ver > -1 ? GeneralFunctions.Loop(menuSelected.ver, menuH) : -1;
    }

    public void Draw()
    {
        p8.Cls();

        Vector2 size = new(p8.Cell.Width, p8.Cell.Height);

        p8.Batch.Draw(p8.TextureDictionary["OptionsBackground4"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

        if (waitingForInput)
        {
            p8.Batch.Draw(p8.TextureDictionary["WaitingForInput"], new Vector2(20 * p8.Cell.Width, 51 * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
        }
        else
        {
            p8.Batch.Draw(p8.TextureDictionary["KeybindsMenu"], new Vector2(8 * p8.Cell.Width, 46 * p8.Cell.Height), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            if (menuSelected.ver >= 0)
            {
                Vector2 position5 = new((46 + 36 * menuSelected.hor) * p8.Cell.Width, (menuSelected.ver * 6 + 55) * p8.Cell.Height);
                p8.Batch.Draw(p8.TextureDictionary["Arrow"], position5, null, p8.Colors[6], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
            }
            else if (menuSelected.ver == -1)
            {
                p8.Rectfill(17, 32, 51, 38, 13);
            }

            Vector2 position3 = new(16 * p8.Cell.Width, 31 * p8.Cell.Height);
            Vector2 position4 = new(30 * p8.Cell.Width, 31 * p8.Cell.Height);
            p8.Batch.Draw(p8.TextureDictionary["SelectorHalf"], position3, null, p8.Colors[7], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Batch.Draw(p8.TextureDictionary["SelectorHalf"], position4, null, p8.Colors[7], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);

            p8.Print("keyboard", 19, 33, 7);
            p8.Print("controller", 19 + 54, 33, 7);

            PropertyInfo[] properties = typeof(OptionsFile).GetProperties();
            int j = 0;
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.StartsWith("Kbm_"))
                {
                    p8.Print(property.Name.Substring(4).ToLower(), 8, 55 + j, 7);
                    Binding val = (Binding)property.GetValue(p8.OptionsFile);
                    p8.Print(KeyNames.keyNames[val.Bind1], 51, 55 + j, 6);
                    p8.Print(KeyNames.keyNames[val.Bind2], 87, 55 + j, 6);
                    j += 6;
                }
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
