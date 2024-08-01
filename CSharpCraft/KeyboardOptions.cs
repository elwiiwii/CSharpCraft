using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft
{
    public class KeyboardOptions(Pico8Functions p8, Dictionary<string, Texture2D> textureDictionary, SpriteBatch batch, GraphicsDevice graphicsDevice, KeyboardOptionsFile keyboardOptionsFile, List<IGameMode> optionsModes, MainOptions mainOptions) : IGameMode
    {

        public string GameModeName { get => "4"; }

        private int menuX;
        private int menuY;
        private int menuWidth;
        private int menuLength;
        private bool waitingForInput;
        private int delay;

        private int LoopX(int sel, int size)
        {
            return ((sel % size) + size) % size;
        }

        private int LoopY(int sel, int size)
        {
            return sel > size - 1 ? 0 : sel < -1 ? -1 : sel;
        }

        public void Init()
        {
            menuX = 0;
            menuY = -1;
            menuWidth = 2;
            menuLength = typeof(KeyboardOptionsFile).GetProperties().Length;
            waitingForInput = false;
            delay = 0;
            mainOptions.currentOptionsMode = 4;
        }

        public void Update()
        {
            if (menuY == -1)
            {
                if (p8.Btnp(1)) { optionsModes[5].Init(); return; }
                if (p8.Btnp(2)) { optionsModes[2].Init(); return; }
                if (p8.Btnp(3)) { menuY += 1; }
                return;
            }

            if (p8.Btnp(5)) { waitingForInput = true; }

            if (waitingForInput)
            {
                if (delay > 5)
                {
                    var key = Keyboard.GetState().GetPressedKeys();
                    if (key.Length == 1)
                    {
                        var properties = typeof(KeyboardOptionsFile).GetProperties();
                        var currentProperty = properties[menuY];
                        var propertyName = typeof(KeyboardOptionsFile).GetProperty(currentProperty.Name);
                        var binding = (Binding)propertyName.GetValue(keyboardOptionsFile);
                        if (menuX == 0 && propertyName != null)
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

            if (p8.Btnp(0)) { menuX -= 1; }
            if (p8.Btnp(1)) { menuX += 1; }
            if (p8.Btnp(2)) { menuY -= 1; }
            if (p8.Btnp(3)) { menuY += 1; }
            
            menuX = LoopX(menuX, menuWidth);
            menuY = LoopY(menuY, menuLength);

        }

        public void Draw()
        {
            // Get the size of the viewport
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;

            // Calculate the size of each cell
            int cellW = viewportWidth / 128;
            int cellH = viewportHeight / 128;

            Vector2 size = new(cellW, cellH);

            batch.Draw(textureDictionary["OptionsBackground4"], new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

            if (waitingForInput)
            {
                batch.Draw(textureDictionary["WaitingForInput"], new Vector2(20 * cellW, 51 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }
            else
            {
                batch.Draw(textureDictionary["KeybindsMenu"], new Vector2(8 * cellW, 46 * cellH), null, Color.White, 0, Vector2.Zero, size, SpriteEffects.None, 0);

                if (menuY >= 0)
                {
                    var position5 = new Vector2((46 + (36 * menuX)) * cellW, ((menuY * 6) + 55) * cellH);
                    batch.Draw(textureDictionary["Arrow"], position5, null, p8.colors[6], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);
                }
                else
                {
                    p8.Rectfill(17, 32, 51, 38, 13);
                }
                
                var position3 = new Vector2(16 * cellW, 31 * cellH);
                var position4 = new Vector2(30 * cellW, 31 * cellH);
                batch.Draw(textureDictionary["SelectorHalf"], position3, null, p8.colors[7], 0, Vector2.Zero, size, SpriteEffects.None, 0);
                batch.Draw(textureDictionary["SelectorHalf"], position4, null, p8.colors[7], 0, Vector2.Zero, size, SpriteEffects.FlipHorizontally, 0);

                p8.Print("keyboard", 19, 33, 7);
                p8.Print("controller", 19 + 54, 33, 7);

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

            }
            
        }

    }

}
