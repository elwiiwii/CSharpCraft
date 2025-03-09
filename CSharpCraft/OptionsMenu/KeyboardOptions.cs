using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.OptionsMenu
{
    public class KeyboardOptions(int startIndex = -1) : IScene, IDisposable
    {

        public string SceneName { get => "options"; }
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
                var mouseState = Mouse.GetState();
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
                        PropertyInfo propertyName = typeof(OptionsFile).GetProperty(currentProperty.Name);
                        Binding binding = (Binding)propertyName.GetValue(p8.optionsFile);
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
                            propertyName.SetValue(p8.optionsFile, newBinding);
                            OptionsFile.JsonWrite(p8.optionsFile);
                        }
                        else
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
                            propertyName.SetValue(p8.optionsFile, newBinding);
                            OptionsFile.JsonWrite(p8.optionsFile);
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

            // Get the size of the viewport
            int viewportWidth = p8.graphicsDevice.Viewport.Width;
            int viewportHeight = p8.graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);

            p8.batch.Draw(p8.textureDictionary["OptionsBackground4"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            if (waitingForInput)
            {
                p8.batch.Draw(p8.textureDictionary["WaitingForInput"], new Vector2(20 * cellW, 51 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }
            else
            {
                p8.batch.Draw(p8.textureDictionary["KeybindsMenu"], new Vector2(8 * cellW, 46 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

                if (menuSelected.ver >= 0)
                {
                    Vector2 position5 = new((46 + 36 * menuSelected.hor) * cellW, (menuSelected.ver * 6 + 55) * cellH);
                    p8.batch.Draw(p8.textureDictionary["Arrow"], position5, null, p8.colors[6], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
                }
                else if (menuSelected.ver == -1)
                {
                    p8.Rectfill(17, 32, 51, 38, 13);
                }

                Vector2 position3 = new(16 * cellW, 31 * cellH);
                Vector2 position4 = new(30 * cellW, 31 * cellH);
                p8.batch.Draw(p8.textureDictionary["SelectorHalf"], position3, null, p8.colors[7], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                p8.batch.Draw(p8.textureDictionary["SelectorHalf"], position4, null, p8.colors[7], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);

                p8.Print("keyboard", 19, 33, 7);
                p8.Print("controller", 19 + 54, 33, 7);

                PropertyInfo[] properties = typeof(OptionsFile).GetProperties();
                int j = 0;
                foreach (PropertyInfo property in properties)
                {
                    if (property.Name.StartsWith("Keyb_"))
                    {
                        p8.Print(property.Name.Substring(5).ToLower(), 8, 55 + j, 7);
                        Binding val = (Binding)property.GetValue(p8.optionsFile);
                        p8.Print(KeyNames.keyNames[val.Bind1], 51, 55 + j, 6);
                        p8.Print(KeyNames.keyNames[val.Bind2], 87, 55 + j, 6);
                        j += 6;
                    }
                }

            }

        }

        public string SpriteData => @"";
        public string FlagData => @"";
        public string MapData => @"";
        public Dictionary<string, List<(List<(string name, bool loop)> tracks, int group)>> Music => new();
        public Dictionary<string, Dictionary<int, string>> Sfx => new();
        public void Dispose()
        {

        }

    }
}
