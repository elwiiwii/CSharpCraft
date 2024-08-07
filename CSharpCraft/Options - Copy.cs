﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft
{
    public class Options2(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, KeyboardOptionsFile keyboardOptionsFile, List<IGameMode> optionsModes) : IGameMode
    {

        public string GameModeName { get => "options"; }

        private int currentMenuOption;
        private int[] menuOptions = new int[3 + typeof(KeyboardOptionsFile).GetProperties().Length];
        private bool waitingForInput;
        private int delay;

        private static string[] keyboardBinds1 = ["left"];
        private static string[] keyboardBinds2 = ["left"];
        private static string[] controllerBinds1 = ["left"];
        private static string[] controllerBinds2 = ["left"];

        private static Dictionary<string, string[]> keyboardControls = new()
        {
            { "bind-1", keyboardBinds1 },
            { "bind-2", keyboardBinds2 }
        };

        private static Dictionary<string, string[]> controllerControls = new()
        {
            { "bind-1", controllerBinds1 },
            { "bind-2", controllerBinds2 }
        };

        private static Dictionary<string, Dictionary<string, string[]>> controlsOptions = new()
        {
            { "keyboard", keyboardControls },
            { "controller", controllerControls }
        };

        private static Dictionary<string, Dictionary<string, Dictionary<string, string[]>>> options = new()
        {
            { "controls", controlsOptions },
            { "graphics", controlsOptions }
        };


        private int Loop(int sel)
        {
            return sel < -1 ? -1 : sel > 0 ? 0 : sel;
        }

        //private int Loop(int sel, int[] l)
        //{
        //    var lp = l.Length;
        //    return ((sel % lp) + lp) % lp;
        //}

        private void Printc(string t, int x, int y, double c)
        {
            p8.Print(t, x - t.Length * 2, y, c);
        }

        public void Init()
        {
            optionsModes[1].Init();
            return;
            //keys = optionsFile;

            //menuSelectedX = false;
            //menuSelectedY = 0;

            waitingForInput = false;
            delay = 0;

            currentMenuOption = 1;
            for (int i = 0; i < menuOptions.Length; i++)
            {
                menuOptions[i] = 0;
            }

        }

        public void Update()
        {
            optionsModes[1].Update();
            return;
            KeyboardState state = Keyboard.GetState();

            if (p8.Btnp(0)) { menuOptions[currentMenuOption] += 1; }
            if (p8.Btnp(1)) { menuOptions[currentMenuOption] -= 1; }
            if (p8.Btnp(2)) { currentMenuOption -= 1; }
            if (p8.Btnp(3)) { currentMenuOption += 1; }

            //Console.WriteLine(Keyboard.GetState());
            
            menuOptions[currentMenuOption] = Loop(menuOptions[currentMenuOption]);

            if (menuOptions[1] == 0)
            {
                if (currentMenuOption > 2 && menuOptions[currentMenuOption] == 0)
                {
                    if (p8.Btnp(5))
                    {
                        waitingForInput = true;
                    }

                    if (waitingForInput)
                    {
                        if (delay > 5)
                        {
                            var key = Keyboard.GetState().GetPressedKeys();
                            if (key.Length == 1)
                            {
                                var properties = typeof(KeyboardOptionsFile).GetProperties();
                                var currentProperty = properties[currentMenuOption - 3];
                                var propertyName = typeof(KeyboardOptionsFile).GetProperty(currentProperty.Name);
                                var binding = (Binding)propertyName.GetValue(keyboardOptionsFile);
                                if (menuOptions[currentMenuOption] == 0 && propertyName != null)
                                {
                                    var newBinding = new Binding(KeysToString.keysToString[key[0]], binding.Bind2);
                                    propertyName.SetValue(keyboardOptionsFile, newBinding);
                                    KeyboardOptionsFile.JsonWrite(keyboardOptionsFile);
                                }
                                else
                                {
                                    var newBinding = new Binding(binding.Bind1, KeysToString.keysToString[key[0]]);
                                    propertyName.SetValue(keyboardOptionsFile, newBinding);
                                    KeyboardOptionsFile.JsonWrite(keyboardOptionsFile);
                                }
                                delay = 0;
                                waitingForInput = false;
                            }
                        }
                        else
                        {
                            delay++;
                        }
                    }
                }
                //else if (currentMenuOption > 2 && menuOptions[currentMenuOption] != 0)
                //{
                //    if (p8.Btnp(5))
                //    {
                //        waitingForInput = true;
                //    }
                //
                //    if (waitingForInput)
                //    {
                //        if (delay > 5)
                //        {
                //            var button = GamePad.GetState(PlayerIndex.One).Buttons;
                //            if (button != null)
                //            {
                //                var properties = typeof(OptionsFile).GetProperties();
                //                var currentProperty = properties[currentMenuOption - 3];
                //                var propertyName = typeof(OptionsFile).GetProperty(currentProperty.Name);
                //                var binding = (Binding)propertyName.GetValue(optionsFile);
                //                if (menuOptions[currentMenuOption] == 0 && propertyName != null)
                //                {
                //                    var newBinding = new Binding(KeysToString.keysToString[button], binding.Bind2);
                //                    propertyName.SetValue(optionsFile, newBinding);
                //                    OptionsFile.JsonWrite(optionsFile);
                //                }
                //                else
                //                {
                //                    var newBinding = new Binding(binding.Bind1, KeysToString.keysToString[button]);
                //                    propertyName.SetValue(optionsFile, newBinding);
                //                    OptionsFile.JsonWrite(optionsFile);
                //                }
                //                delay = 0;
                //                waitingForInput = false;
                //            }
                //        }
                //        else
                //        {
                //            delay++;
                //        }
                //    }
                //}

            }

        }

        public void Draw()
        {
            optionsModes[1].Draw();
            return;
            batch.GraphicsDevice.Clear(Color.Black);

            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);

            batch.Draw(textureDictionary["OptionsMenuBackground"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            batch.Draw(textureDictionary["Arrow"], new Vector2(0 * cellW, 1 * cellH), null, p8.colors[currentMenuOption == 0 ? 7 : 5], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            p8.Print("back", 5, 1, currentMenuOption == 0 ? 7 : 5);

            for (int i = 0; i <= 1; i++)
            {
                var position2 = new Vector2((15 + (58 * i)) * cellW, 16 * cellH);
                var position3 = new Vector2((16 + (58 * i)) * cellW, 17 * cellH);
                batch.Draw(textureDictionary["SelectorBorder"], position2, null, p8.colors[7], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["SelectorCenter"], position3, null, p8.colors[currentMenuOption == 1 ? 2 : 0], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }

            var position = new Vector2((15 + Math.Abs(58 * menuOptions[1])) * cellW, 22 * cellH);
            batch.Draw(textureDictionary["OptionsMenuTab"], position, null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            p8.Rectfill(15 + Math.Abs(58 * menuOptions[1]), 22, 53 + Math.Abs(58 * menuOptions[1]), 27, 7);
            p8.Rectfill(16 + Math.Abs(58 * menuOptions[1]), 22, 52 + Math.Abs(58 * menuOptions[1]), 27, currentMenuOption == 1 ? 2 : 0);

            for (int i = 0; i <= 1; i++)
            {
                p8.Print(options.Keys.ElementAt(i), 19 + 58 * i, 19 - Math.Abs(i + menuOptions[1]), 7);
            }

            if (currentMenuOption < 2)
            {
                p8.Rectfill(6, 27, 6 + 115, 27, 7);
            }



            if (menuOptions[1] == 0)
            {
                if (currentMenuOption == 2)
                {
                    if (menuOptions[2] == 0)
                    {
                        p8.Rectfill(17, 32, 51, 38, 13);
                    }
                    else if (menuOptions[2] == -1)
                    {
                        p8.Rectfill(71, 32, 113, 38, 13);
                    }
                }

                if (waitingForInput)
                {
                    batch.Draw(textureDictionary["WaitingForInput"], new Vector2(20 * cellW, 51 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
                }
                else
                {
                    var position2 = new Vector2((16 + Math.Abs(54 * menuOptions[2])) * cellW, 31 * cellH);
                    var position3 = new Vector2((30 + Math.Abs(62 * menuOptions[2])) * cellW, 31 * cellH);

                    batch.Draw(textureDictionary["SelectorHalf"], position2, null, p8.colors[currentMenuOption == 2 ? 7 : 5], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                    batch.Draw(textureDictionary["SelectorHalf"], position3, null, p8.colors[currentMenuOption == 2 ? 7 : 5], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);

                    batch.Draw(textureDictionary["KeybindsMenu"], new Vector2(8 * cellW, 46 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

                    for (int i = 0; i <= 1; i++)
                    {
                        p8.Print(controlsOptions.Keys.ElementAt(i), 19 + 54 * i, 33, 7);
                    }

                    var properties = typeof(KeyboardOptionsFile).GetProperties();
                    int j = 0;
                    foreach (var property in properties)
                    {
                        p8.Print(property.Name.ToLower(), 8, 55 + j, 7);
                        var val = (Binding)property.GetValue(keyboardOptionsFile);
                        p8.Print(KeyNames.keyNames[val.Bind1], 51, 55 + j, 6);
                        p8.Print(KeyNames.keyNames[val.Bind2], 87, 55 + j, 6);
                        j += 6;
                    }

                    if (currentMenuOption > 2)
                    {
                        var position4 = new Vector2((46 + Math.Abs(36 * menuOptions[currentMenuOption])) * cellW, (((currentMenuOption - 3) * 6) + 55) * cellH);

                        batch.Draw(textureDictionary["Arrow"], position4, null, p8.colors[6], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
                    }
                }





                //p8.Print(nameof(optionsFile.Left).ToLower(), 8, 55, 7);



                //int i = 0;
                //foreach (var key in keys)
                //{
                //    p8.Print($"{key}", 8, 62 + i, 7);
                //    i += 6;
                //}
            }

        }


    }
}
